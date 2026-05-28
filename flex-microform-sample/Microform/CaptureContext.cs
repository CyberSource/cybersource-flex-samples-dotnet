using CyberSource.Api;
using CyberSource.Model;
using flex_microform_sample.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace flex_microform_sample.Microform
{
    /// <summary>
    /// Holds the data returned by a successful GenerateCaptureContext call.
    /// </summary>
    public class CaptureContextResult
    {
        /// <summary>The full capture context JWT passed to Microform on the client.</summary>
        public string Jwt { get; set; }

        /// <summary>URL of the Microform JavaScript bundle (extracted from the JWT).</summary>
        public string ClientLibraryUrl { get; set; }

        /// <summary>SHA-256 SRI hash used to verify the JavaScript bundle (extracted from the JWT).</summary>
        public string ClientLibraryIntegrity { get; set; }
    }

    /// <summary>
    /// Calls the CyberSource Microform v2 API to generate a capture context.
    ///
    /// The capture context is a signed JWT that:
    ///   1. Authorises the browser to create hosted card-entry fields via Microform.
    ///   2. Embeds the URL and integrity hash of the Microform JavaScript bundle.
    ///   3. Scopes the session to the allowed card networks and the originating page.
    ///
    /// Reference: https://developer.cybersource.com/docs/cybs/en-us/digital-accept-flex/developer/all/rest/digital-accept-flex/microform-integ-v2/api-field-ref-v2/api-request-response-v2/microform-rest-api-v2-capture-cntxt-req.html
    /// </summary>
    public static class CaptureContext
    {
        /// <summary>
        /// Generates a Microform v2 capture context for the given origin URL.
        /// </summary>
        /// <param name="targetOrigin">
        /// The exact origin (scheme + host + port) of the page that will embed Microform,
        /// e.g. "http://localhost:65309". Must match the browser's window.location.origin.
        /// </param>
        /// <returns>
        /// A <see cref="CaptureContextResult"/> containing the JWT, JavaScript URL, and SRI hash,
        /// or null if the API call or JWT validation fails.
        /// </returns>
        public static async Task<CaptureContextResult> GenerateAsync(string targetOrigin)
        {
            // --- 1. Build the request ---

            // targetOrigins restricts which pages are allowed to initialise Microform with this context.
            var targetOrigins = new List<string> { targetOrigin };

            // Specify which card networks you want to accept.
            var allowedCardNetworks = new List<string>
            {
                "VISA", "MASTERCARD", "AMEX", "CARNET", "CARTESBANCAIRES",
                "CUP", "DINERSCLUB", "DISCOVER", "EFTPOS", "ELO",
                "JCB", "JCREW", "MADA", "MAESTRO", "MEEZA"
            };

            // Specify the payment types — CARD for credit/debit card acceptance.
            var allowedPaymentTypes = new List<string> { "CARD" };

            var request = new GenerateCaptureContextRequest(
                ClientVersion: "v2",
                TargetOrigins: targetOrigins,
                AllowedCardNetworks: allowedCardNetworks,
                AllowedPaymentTypes: allowedPaymentTypes
            );

            // --- 2. Call the API ---

            try
            {
                var configDictionary = new MerchantConfiguration().GetConfiguration();
                var clientConfig     = new CyberSource.Client.Configuration(merchConfigDictObj: configDictionary);
                var apiInstance      = new MicroformIntegrationApi(clientConfig);

                Console.WriteLine("Calling CyberSource API: GenerateCaptureContext");
                string captureContextJwt = apiInstance.GenerateCaptureContext(request);

                // --- 3. Validate the JWT signature before trusting any claims ---
                //
                // CaptureContextValidator fetches the CyberSource public key by kid,
                // verifies the RS256 signature, and checks the token lifetime.
                // This ensures the clientLibrary URL and SRI hash in step 4 are authentic.
                string runEnvironment = configDictionary["runEnvironment"];
                var token = await CaptureContextValidator.ValidateAndReadAsync(
                    captureContextJwt, runEnvironment);

                // --- 4. Extract the JavaScript URL and SRI hash from the validated JWT ---
                //
                // The "ctx" claim contains clientLibrary (JS bundle URL) and
                // clientLibraryIntegrity (SRI hash). These are read dynamically because
                // they change across Microform versions.
                var ctxClaim = token.Claims.FirstOrDefault(c => c.Type == "ctx");
                if (ctxClaim == null)
                {
                    Console.WriteLine("GenerateCaptureContext: JWT is missing the 'ctx' claim.");
                    return null;
                }

                // The ctx claim is a JSON array — wrap it so JsonDocument can parse it.
                var ctxArray = JsonDocument.Parse($"[{ctxClaim.Value}]").RootElement;
                var ctxData  = ctxArray[0].GetProperty("data");

                return new CaptureContextResult
                {
                    Jwt                    = captureContextJwt,
                    ClientLibraryUrl       = ctxData.GetProperty("clientLibrary").GetString(),
                    ClientLibraryIntegrity = ctxData.GetProperty("clientLibraryIntegrity").GetString()
                };
            }
            catch (Exception e)
            {
                Console.WriteLine("GenerateCaptureContext exception: " + e.Message);
                return null;
            }
        }
    }
}
