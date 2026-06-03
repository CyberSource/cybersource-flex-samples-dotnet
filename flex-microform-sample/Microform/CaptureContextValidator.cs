using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace flex_microform_sample.Microform
{
    // ============================================================
    // CAPTURE CONTEXT JWT VALIDATOR
    // ============================================================
    // The capture context JWT returned by the Sessions API is signed
    // by CyberSource using RS256 (RSA + SHA-256). Validating that
    // signature before extracting claims ensures:
    //
    //   1. The JWT was genuinely issued by CyberSource. An attacker
    //      performing a MITM attack between this server and the
    //      CyberSource API cannot forge a valid RS256 signature
    //      without CyberSource's private key.
    //
    //   2. The clientLibrary URL and clientLibraryIntegrity SRI hash
    //      that flow from the JWT into the checkout page HTML are
    //      authentic. Without validation, a forged JWT could redirect
    //      every payment page visitor to an attacker-controlled script.
    //
    // How CyberSource key discovery works:
    //   - The JWT header contains a "kid" (Key ID) field.
    //   - The matching RSA public key is published at:
    //       GET https://{runEnvironment}/flex/v2/public-keys/{kid}
    //   - The response is a JSON Web Key (JWK, RFC 7517).
    //   - We convert the JWK into an RsaSecurityKey and pass it to
    //     JwtSecurityTokenHandler.ValidateToken() as the IssuerSigningKey.
    // ============================================================

    /// <summary>
    /// Validates the RS256 signature of a CyberSource capture context JWT
    /// before any claims are trusted or used.
    /// </summary>
    public static class CaptureContextValidator
    {
        private static readonly HttpClient              _httpClient = new HttpClient();
        private static readonly JwtSecurityTokenHandler _handler    = new JwtSecurityTokenHandler();

        // ----------------------------------------------------------------
        // Public-key cache, keyed by "kid" (Key ID from the JWT header).
        //
        // CyberSource rotates signing keys periodically. When a new kid
        // appears the cache misses, the new key is fetched and stored.
        //
        // PRODUCTION NOTE:
        //   This ConcurrentDictionary holds keys for the lifetime of the
        //   process with no expiration. That is acceptable for a sample
        //   application but can become stale in a long-running service if
        //   CyberSource retires a key and reuses its kid for a different
        //   key. For production, replace this with IMemoryCache (or
        //   IDistributedCache in a multi-instance deployment) and set a
        //   sliding or absolute expiration — 24 hours is a reasonable
        //   starting point. Example:
        //
        //     _cache.Set(kid, securityKey,
        //         new MemoryCacheEntryOptions()
        //             .SetAbsoluteExpiration(TimeSpan.FromHours(24)));
        //
        //   This ensures a rotated key is automatically refreshed within
        //   one cache TTL without any manual intervention.
        // ----------------------------------------------------------------
        private static readonly ConcurrentDictionary<string, SecurityKey> _keyCache =
            new ConcurrentDictionary<string, SecurityKey>();

        /// <summary>
        /// Validates the RS256 signature of <paramref name="jwt"/> against
        /// the CyberSource public key identified by the token's "kid" header
        /// field, then returns the decoded token for claim extraction.
        /// </summary>
        /// <param name="jwt">
        ///   The raw JWT string returned by the CyberSource Sessions API.
        /// </param>
        /// <param name="runEnvironment">
        ///   The CyberSource API host (e.g. "apitest.cybersource.com").
        ///   Used to build the public-keys endpoint URL.
        /// </param>
        /// <returns>
        ///   The decoded, signature-verified <see cref="JwtSecurityToken"/>.
        /// </returns>
        /// <exception cref="SecurityTokenException">
        ///   The JWT header is missing "kid", or specifies an algorithm
        ///   other than RS256, or the fetched public key has an unexpected
        ///   key type.
        /// </exception>
        /// <exception cref="SecurityTokenValidationException">
        ///   The JWT signature does not match the CyberSource public key,
        ///   or the token has expired.
        /// </exception>
        public static async Task<JwtSecurityToken> ValidateAndReadAsync(
            string jwt, string runEnvironment)
        {
            // ----------------------------------------------------------------
            // Step 1 — Read the JWT header WITHOUT signature validation.
            //
            // The JWT header (the first base64url segment) is not protected
            // by its own signature — only the payload is. Reading it here
            // without validation is safe because we are using the header
            // solely to discover the "kid" and "alg" fields needed to fetch
            // the correct public key. No payload claims are trusted at this
            // stage; they are only used after full validation in Step 3.
            // ----------------------------------------------------------------
            var unvalidatedToken = _handler.ReadJwtToken(jwt);
            var kid              = unvalidatedToken.Header.Kid;
            var alg              = unvalidatedToken.Header.Alg;

            if (string.IsNullOrEmpty(kid))
                throw new SecurityTokenException(
                    "Capture context JWT header is missing the 'kid' (Key ID) field. " +
                    "Cannot look up the CyberSource signing key.");

            Debug.WriteLine($"[CaptureContextValidator] Validating capture context JWT — kid='{kid}', alg='{alg}'");

            // Reject any algorithm other than RS256.
            //
            // This prevents two well-known JWT attacks:
            //   "alg: none"  — a token with no signature at all, which a naive
            //                   validator would accept as valid.
            //   Algorithm    — swapping RS256 for HS256 and signing with the
            //   substitution    public key as the HMAC secret so that the
            //                   validator accepts the forged token.
            if (!string.Equals(alg, SecurityAlgorithms.RsaSha256, StringComparison.Ordinal))
                throw new SecurityTokenException(
                    $"Capture context JWT specifies algorithm '{alg}'. " +
                     "Only 'RS256' is accepted.");

            // ----------------------------------------------------------------
            // Step 2 — Fetch (or retrieve from cache) the RSA public key
            //          that CyberSource used to sign this token.
            // ----------------------------------------------------------------
            var signingKey = await FetchPublicKeyAsync(kid, runEnvironment);

            // ----------------------------------------------------------------
            // Step 3 — Cryptographically validate the JWT.
            //
            // JwtSecurityTokenHandler.ValidateToken() verifies:
            //
            //   Signature  — The token was signed by the holder of the private
            //                key that corresponds to our fetched public key.
            //                Any tampering with the header or payload bytes
            //                will produce a signature mismatch and throw.
            //
            //   Lifetime   — The token is within its "nbf" (not-before) and
            //                "exp" (expiry) window, adjusted by ClockSkew to
            //                tolerate minor clock drift between servers.
            //
            // Issuer (iss) and audience (aud) validation are intentionally
            // disabled. CyberSource's documentation does not specify fixed
            // values for these claims in capture context JWTs. If CyberSource
            // publishes them in the future, set ValidateIssuer = true /
            // ValidIssuer = "<value>" and ValidateAudience = true /
            // ValidAudience = "<value>" for additional defence-in-depth.
            // ----------------------------------------------------------------
            var validationParams = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey         = signingKey,

                ValidateLifetime = true,
                // Allow up to 5 minutes of clock drift between this merchant
                // server and CyberSource's token-issuing infrastructure.
                ClockSkew        = TimeSpan.FromMinutes(5),

                ValidateIssuer   = false,
                ValidateAudience = false,
            };

            // ValidateToken throws SecurityTokenValidationException (or a
            // subclass such as SecurityTokenExpiredException or
            // SecurityTokenSignatureKeyNotFoundException) if any check fails.
            // The caller's try/catch in CaptureContext.GenerateAsync handles it.
            _handler.ValidateToken(jwt, validationParams, out var validatedToken);

            Debug.WriteLine("[CaptureContextValidator] JWT signature and lifetime validation passed");
            return (JwtSecurityToken)validatedToken;
        }

        // ----------------------------------------------------------------
        // Fetches the RSA public key for the given kid from CyberSource's
        // Flex public-keys endpoint, caches it, and returns it.
        //
        // Endpoint:
        //   Test — https://apitest.cybersource.com/flex/v2/public-keys/{kid}
        //   Prod — https://api.cybersource.com/flex/v2/public-keys/{kid}
        //
        // The response is a single JSON Web Key (JWK, RFC 7517). For RS256
        // tokens the relevant fields are:
        //   "kty" : "RSA"
        //   "n"   : base64url-encoded RSA modulus
        //   "e"   : base64url-encoded RSA public exponent
        //   "alg" : "RS256"
        //   "use" : "sig"
        //
        // Only the modulus and exponent are needed to construct an
        // RsaSecurityKey for signature verification. The private key
        // fields (d, p, q, dp, dq, qi) are never present in this response.
        // ----------------------------------------------------------------
        private static async Task<SecurityKey> FetchPublicKeyAsync(
            string kid, string runEnvironment)
        {
            // Return the cached key if we have already fetched it this session.
            if (_keyCache.TryGetValue(kid, out var cached))
            {
                Debug.WriteLine($"[CaptureContextValidator] Public key cache hit for kid='{kid}'");
                return cached;
            }

            // Build the public-keys URL. Uri.EscapeDataString ensures the kid
            // is safely percent-encoded in case it contains special characters.
            var url      = $"https://{runEnvironment}/flex/v2/public-keys/{Uri.EscapeDataString(kid)}";
            Debug.WriteLine($"[CaptureContextValidator] Fetching public key from {url}");
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException(
                    $"Failed to fetch CyberSource public key for kid='{kid}' " +
                    $"from {url}: {(int)response.StatusCode} {response.StatusCode}");

            var jwkJson = await response.Content.ReadAsStringAsync();

            // ----------------------------------------------------------------
            // Parse the JWK response.
            //
            // JsonWebKey (Microsoft.IdentityModel.Tokens) understands the
            // standard JWK JSON format. After construction it exposes the
            // modulus (N) and exponent (E) as base64url strings, which we
            // decode into byte arrays to build an RSAParameters struct.
            // ----------------------------------------------------------------
            var jwk = new JsonWebKey(jwkJson);

            if (!string.Equals(jwk.Kty, JsonWebAlgorithmsKeyTypes.RSA, StringComparison.Ordinal))
                throw new SecurityTokenException(
                    $"CyberSource public key for kid='{kid}' has key type '{jwk.Kty}'. " +
                     "Expected 'RSA'.");

            // Construct the RSA public key from the JWK's modulus and exponent.
            // Base64UrlEncoder.DecodeBytes handles the URL-safe base64 alphabet
            // and omitted padding that the JWK spec requires.
            var rsaParameters = new RSAParameters
            {
                Modulus  = Base64UrlEncoder.DecodeBytes(jwk.N),
                Exponent = Base64UrlEncoder.DecodeBytes(jwk.E),
            };

            // Wrap the RSA parameters in a SecurityKey and tag it with the kid
            // so the JWT validation framework can match the key to the token header.
            var securityKey = new RsaSecurityKey(rsaParameters) { KeyId = kid };

            // Store in the cache. TryAdd is used (rather than indexer assignment)
            // so that a concurrent request that fetched the same key simultaneously
            // does not overwrite a key that is already cached — both threads end up
            // using equivalent keys and no work is lost.
            //
            // See the PRODUCTION NOTE on _keyCache above regarding cache expiration.
            _keyCache.TryAdd(kid, securityKey);

            Debug.WriteLine($"[CaptureContextValidator] Public key fetched and cached for kid='{kid}'");
            return securityKey;
        }
    }
}
