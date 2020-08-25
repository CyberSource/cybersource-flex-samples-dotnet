using CyberSource.Api;
using CyberSource.Model;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace flex_microform_sample.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Checkout()
        {

            // Call the .NET CyberSource SDK to generate the Flex Key

            ViewBag.Jwk = "{\"kid\":\"HKJHKJ\"}";



            /**
             * Generating Capture Context Request Payload
             * Defining Encryption Type = RsaOaep
             * Defining TargetOrigin = http://localhost:65309 or http://localhost:8080
             * 
             */
            var requestObj = new GeneratePublicKeyRequest("RsaOaep256", "http://localhost:65309 http://localhost:8080");

            try
            {
                var configDictionary = new Configuration().GetConfiguration();
                var clientConfig = new CyberSource.Client.Configuration(merchConfigDictObj: configDictionary);
                var apiInstance = new KeyGenerationApi(clientConfig);

                /**
                 * Initiating public Key request 
                 * query paramiter set to format=JWT for Flex 11
                 */
                var result = apiInstance.GeneratePublicKey(requestObj, "JWT");
                Console.WriteLine(result);
                Console.WriteLine(result.KeyId);
                ViewBag.Jwk = result.KeyId;

            }
            catch (Exception e)
            {
                Console.WriteLine("Exception on calling the API: " + e.Message);

            }

            return View();
        }

        public ActionResult Receipt()
        {
            dynamic flexObj = Request.Params["flexResponse"];


            /**
             * Processing Authorization Request
             * Code developed from CyberSource Rest Samples csharp
             * https://github.com/CyberSource/cybersource-rest-samples-csharp
             */

            var processingInformationObj = new Ptsv2paymentsProcessingInformation() { CommerceIndicator = "internet" };

            var clientReferenceInformationObj = new Ptsv2paymentsClientReferenceInformation { Code = "test_payment" };


            var orderInformationObj = new Ptsv2paymentsOrderInformation();

            var billToObj = new Ptsv2paymentsOrderInformationBillTo
            {
                Country = "US",
                FirstName = "John",
                LastName = "Doe",
                Address1 = "1 Market St",
                PostalCode = "94105",
                Locality = "San Francisco",
                AdministrativeArea = "CA",
                Email = "test@cybs.com"
            };

            orderInformationObj.BillTo = billToObj;

            var amountDetailsObj = new Ptsv2paymentsOrderInformationAmountDetails
            {
                TotalAmount = "102.21",
                Currency = "USD"
            };

            orderInformationObj.AmountDetails = amountDetailsObj;


            // Passing Transient token

            var transientTokenObj = new Ptsv2paymentsTokenInformation { TransientTokenJwt = flexObj };


            var requestObj = new CreatePaymentRequest
            {
                ProcessingInformation = processingInformationObj,
                ClientReferenceInformation = clientReferenceInformationObj,
                OrderInformation = orderInformationObj,
                TokenInformation = transientTokenObj
            };


            try
            {
                var configDictionary = new Configuration().GetConfiguration();
                var clientConfig = new CyberSource.Client.Configuration(merchConfigDictObj: configDictionary);
                var apiInstance = new PaymentsApi(clientConfig);

                var result = apiInstance.CreatePayment(requestObj);
                
                Console.WriteLine(result);

                //Making response pretty & passing to page
                ViewBag.paymentResponse =result;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception on calling the API: " + e.Message);
                return null;
            }




            return View();
        }

        public ActionResult Token()
        {
            //dynamic flexObj = JValue.Parse(Request.Params["flexResponse"]);
            dynamic flexObj = Request.Params["flexResponse"];

            ViewBag.JWT = flexObj.Replace("\r\n", "").Replace("\"", "");


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