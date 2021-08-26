using CyberSource.Api;
using CyberSource.Model;
using flex_microform_dotnet_core_sample.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace flex_microform_dotnet_core_sample.Controllers
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
                { "runEnvironment", "apitest.cybersource.com" },
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

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger) => _logger = logger;

        public IActionResult Index() => View();

        public async Task<IActionResult> CheckoutAsync()
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
                var result = await apiInstance.GeneratePublicKeyAsync("JWT", requestObj);

                _logger.LogInformation(JsonConvert.SerializeObject(result));
                _logger.LogInformation(result.KeyId);

                // result.KeyId here is the temporary token
                ViewBag.Jwk = result.KeyId;
            }
            catch (Exception e)
            {
                _logger.LogError("Exception on calling the API: " + e.Message);
            }

            return View();
        }

        /// <summary>
        /// This is not important, just to show the token
        /// </summary>
        public IActionResult Token()
        {
            var flexObj = Request.Form["flexResponse"];
            var cardHolderName = Request.Form["cardholderName"];

            ViewBag.CardHolderName = cardHolderName[0];
            ViewBag.JWT = flexObj[0].Replace("\r\n", "").Replace("\"", "");

            return View();
        }

        public async Task<IActionResult> Receipt()
        {
            // This is the Transient token combining temporary token and the credit card info
            var transientToken = Request.Form["flexResponse"][0];

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

                var result = await apiInstance.CreatePaymentAsync(requestObj);

                _logger.LogInformation(JsonConvert.SerializeObject(result));

                //Making response pretty & passing to page
                ViewBag.paymentResponse = result;
            }
            catch (Exception e)
            {
                _logger.LogError("Exception on calling the API: " + e.Message);
                return null;
            }

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
            => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
