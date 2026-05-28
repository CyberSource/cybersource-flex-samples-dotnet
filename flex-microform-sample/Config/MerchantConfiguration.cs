using System.Collections.Generic;

namespace flex_microform_sample.Config
{
    /// <summary>
    /// Merchant credentials and CyberSource SDK settings.
    /// Update these values with your sandbox or production credentials from
    /// the Business Center: https://businesscenter.cybersource.com
    /// </summary>
    public class MerchantConfiguration
    {
        public Dictionary<string, string> GetConfiguration()
        {
            return new Dictionary<string, string>
            {
                { "authenticationType", "HTTP_SIGNATURE" },
                { "merchantID",         "testrest" },
                { "merchantsecretKey",  "yBJxy6LjM2TmcPGu+GaJrHtkke25fPpUX+UY6/L/1tE=" },
                { "merchantKeyId",      "08c94330-f618-42a3-b09d-e1e43be5efda" },
                { "keysDirectory",      "Resource" },
                { "keyFilename",        "testrest" },
                { "keyAlias",           "testrest" },
                { "keyPass",            "testrest" },

                // runEnvironment: use "apitest.cybersource.com" for sandbox, "api.cybersource.com" for production
                { "runEnvironment",     "apitest.cybersource.com" },

                { "timeout",            "300000" }
            };
        }
    }
}
