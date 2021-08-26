using CyberSource.Api;
using CyberSource.Model;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace flex_microform_sample.Controllers
{
    public class HomeController : Controller
    {
        public static class Configuration
        {
            public static Dictionary<string, string> GetDictionary()
                => new Dictionary<string, string>
                {
                    { "authenticationType", "HTTP_SIGNATURE" },
                    { "merchantID", "testrest" },
                    { "merchantsecretKey", "yBJxy6LjM2TmcPGu+GaJrHtkke25fPpUX+UY6/L/1tE=" },
                    { "merchantKeyId", "08c94330-f618-42a3-b09d-e1e43be5efda" },
                    { "keysDirectory", "Resource" },
                    { "keyFilename", "testrest" },
                    { "runEnvironment", "cybersource.environment.sandbox" },
                    { "keyAlias", "testrest" },
                    { "keyPass", "testrest" },
                    { "enableLog", "FALSE" },
                    { "logDirectory", string.Empty },
                    { "logFileName", string.Empty },
                    { "logFileMaxSize", "5242880" },
                    { "timeout", "1000" },
                    { "proxyAddress", string.Empty },
                    { "proxyPort", string.Empty }
                };
        }

        public ActionResult Checkout()
        {
            try
            {
                var apiInstance = new KeyGenerationApi(
                    new CyberSource.Client.Configuration(
                        merchConfigDictObj: Configuration.GetDictionary()));

                // Generating Capture Context Request Payload
                // Defining Encryption Type = RsaOaep
                // Defining TargetOrigin = http://localhost:65309
                var requestObj = new GeneratePublicKeyRequest("RsaOaep256", "http://localhost:65309");

                // Initiating public Key request 
                // query parameter set to format=JWT for Flex 11
                var result = apiInstance.GeneratePublicKey(requestObj, "JWT");

                Console.WriteLine(result);
                Console.WriteLine(result.KeyId);

                // result.KeyId here is the temporary token
                ViewBag.Jwk = result.KeyId;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception on calling the API: " + e.Message);
            }

            return View();
        }

        /// <summary>
        /// This is not important, just to show the token
        /// </summary>
        public ActionResult Token()
        {
            dynamic flexObj = Request.Params["flexResponse"];

            ViewBag.JWT = flexObj.Replace("\r\n", "").Replace("\"", "");

            return View();
        }

        public ActionResult Receipt()
        {
            // This is the Transient token combining temporary token and the credit card info
            var transientToken = Request.Params["flexResponse"];

            try
            {
                // Same as above
                var apiInstance = new PaymentsApi(
                    new CyberSource.Client.Configuration(
                        merchConfigDictObj: Configuration.GetDictionary()));

                var requestObj = new CreatePaymentRequest
                {
                    // Processing Authorization Request
                    // Code developed from CyberSource Rest Samples csharp
                    // https://github.com/CyberSource/cybersource-rest-samples-csharp
                    ProcessingInformation = new Ptsv2paymentsProcessingInformation { CommerceIndicator = "internet" },
                    ClientReferenceInformation = new Ptsv2paymentsClientReferenceInformation { Code = "test_payment" },
                    OrderInformation = new Ptsv2paymentsOrderInformation
                    {
                        BillTo = new Ptsv2paymentsOrderInformationBillTo
                        {
                            Country = "US",
                            FirstName = "John",
                            LastName = "Doe",
                            Address1 = "1 Market St",
                            PostalCode = "94105",
                            Locality = "San Francisco",
                            AdministrativeArea = "CA",
                            Email = "test@cybs.com"
                        },
                        AmountDetails = new Ptsv2paymentsOrderInformationAmountDetails
                        {
                            TotalAmount = "102.21",
                            Currency = "USD"
                        }
                    },
                    // Passing Transient token
                    TokenInformation = new Ptsv2paymentsTokenInformation { TransientTokenJwt = transientToken }
                };

                var result = apiInstance.CreatePayment(requestObj);

                Console.WriteLine(result);

                //Making response pretty & passing to page
                ViewBag.paymentResponse = result;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception on calling the API: " + e.Message);
                return null;
            }

            return View();
        }
    }
}