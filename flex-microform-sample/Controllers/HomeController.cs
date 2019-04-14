using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using CyberSource.Api;
using CyberSource.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace flex_microform_sample.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Checkout()
        {

            // Call the .NET CyberSource SDK to generate the Flex Key

            ViewBag.Jwk = "{\"kid\":\"HKJHKJ\"}";

            var requestObj = new GeneratePublicKeyRequest("RsaOaep256", "http://localhost:65309");

            try
            {
                var configDictionary = new Configuration().GetConfiguration();
                var clientConfig = new CyberSource.Client.Configuration(merchConfigDictObj: configDictionary);
                var apiInstance = new KeyGenerationApi(clientConfig);

                var result = apiInstance.GeneratePublicKey(requestObj);
                Console.WriteLine(result);
                Console.WriteLine(result.Jwk.ToString());
                Console.WriteLine(result.Jwk.ToJson().ToString());


                ViewBag.Jwk = result.Jwk.ToJson().Replace("\r\n", "").Replace(@"\", "");

            }
            catch (Exception e)
            {
                Console.WriteLine("Exception on calling the API: " + e.Message);

            }

            return View();
        }

        public ActionResult Receipt()
        {
            dynamic flexObj = JValue.Parse(Request.Params["flexresponse"]);

            ViewBag.Token = flexObj.token;
            ViewBag.MaskedPan = flexObj.maskedPan;
            ViewBag.Signature = flexObj.signature;

            return View();
        }

    }
    public class Configuration
    {
        // initialize dictionary object
        private readonly Dictionary<string, string> _configurationDictionary = new Dictionary<string, string>();

        public Dictionary<string, string> GetConfiguration()
        {
            _configurationDictionary.Add("authenticationType", "HTTP_SIGNATURE");
            _configurationDictionary.Add("merchantID", "testrest");
            _configurationDictionary.Add("merchantsecretKey", "yBJxy6LjM2TmcPGu+GaJrHtkke25fPpUX+UY6/L/1tE=");
            _configurationDictionary.Add("merchantKeyId", "08c94330-f618-42a3-b09d-e1e43be5efda");
            _configurationDictionary.Add("keysDirectory", "Resource");
            _configurationDictionary.Add("keyFilename", "testrest");
            _configurationDictionary.Add("runEnvironment", "cybersource.environment.sandbox");
            _configurationDictionary.Add("keyAlias", "testrest");
            _configurationDictionary.Add("keyPass", "testrest");
            _configurationDictionary.Add("enableLog", "FALSE");
            _configurationDictionary.Add("logDirectory", string.Empty);
            _configurationDictionary.Add("logFileName", string.Empty);
            _configurationDictionary.Add("logFileMaxSize", "5242880");
            _configurationDictionary.Add("timeout", "1000");
            _configurationDictionary.Add("proxyAddress", string.Empty);
            _configurationDictionary.Add("proxyPort", string.Empty);

            return _configurationDictionary;
        }
    }
}