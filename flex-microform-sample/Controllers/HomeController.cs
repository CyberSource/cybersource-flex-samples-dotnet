using CyberSource.Api;
using CyberSource.Model;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Diagnostics;
using System.IO;
using Jose;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using System.Linq;
using System.Security.Claims;

namespace flex_microform_sample.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Checkout()
        {
            Debug.WriteLine("Initiatiating Request to get Microform capture Context");
            // Call the .NET CyberSource SDK to generate the Flex Key

            ViewBag.Jwt = "eyJraWQiOiJ6dSIsImFsZyI6IlJTMjU2In0.eyJmbHgiOnsicGF0aCI6Ii9mbGV4L3YyL3Rva2VucyIsImRhdGEiOiIwenhkdExvWVRMMXEwamNzMUphM3J4QUFFR3lNdE0xY3lLRElXb3huTEpabXh0ZHBERGFMNDFyNnBDSk05VHFNTjh5NWpRR0VpZ3FHaFNXM2g2MHc5elMyQVkxM3pObUE1NlRlY2pxR3JTMk5Wd3NjQ2NhNlc1ZnZUMjh1M0lQV3ZPQUUiLCJvcmlnaW4iOiJodHRwczovL3Rlc3RmbGV4LmN5YmVyc291cmNlLmNvbSIsImp3ayI6eyJrdHkiOiJSU0EiLCJlIjoiQVFBQiIsInVzZSI6ImVuYyIsIm4iOiIwalR1bndwWTVTbUpuYldYTzBVQ205OHBzd3dmeGtKZ1hobURtRU9pSWZJWVdraF8zUmRFZTdmeGpUeXVfQ2pEVEVNWlF2ZXl4Vm81WDZnSkJvelFPQnM2Sm50TE1UOGgzb2h4NlVJV0UzVkFWckd2dlFVUXAtNm5tODNQc0FYS1h0czNiVGlzNEpURVpfMGtlNVVjSlNvWHJBaVdSUWNGVkphM1ptclZFWFdZTWd6Q1JLUk1zdG5sLTRKTElNWDMxX29vMG5qOVo1Z3lmVVFtVDVzSE5JZlk5ZmtuVTRSdzhfbTAyTmc3RVNaRGZtTmVXbjN4eVFva1dmSml3bTYxSEFUTFg0S2dUVkgxTnh6ckVWUEtPWXU0NkVoYVRDSjlVNWZwV0dNdG50RXIyQ001Wkt5VW1lSzAtNUZmaTNFb2M5WEVCVXRnTlZ1XzRmdUF1cEV3dVEiLCJraWQiOiIwN0ZBVWFaSFZTcFhvbDhTaVVETElZZ2pKUFpPSlFQaSJ9fSwiY3R4IjpbeyJkYXRhIjp7ImNsaWVudExpYnJhcnlJbnRlZ3JpdHkiOiJzaGEyNTYtM0ZxOVJxQlVDaW1DanRNNGNpZDlia0EyVEJWRUZpWkwvbzZjRzIzVnJHb1x1MDAzZCIsImNsaWVudExpYnJhcnkiOiJodHRwczovL3Rlc3RmbGV4LmN5YmVyc291cmNlLmNvbS9taWNyb2Zvcm0vYnVuZGxlL3YyLjUuMi9mbGV4LW1pY3JvZm9ybS5taW4uanMiLCJhbGxvd2VkQ2FyZE5ldHdvcmtzIjpbIlZJU0EiLCJNQVNURVJDQVJEIiwiQU1FWCIsIkNBUk5FVCIsIkNBUlRFU0JBTkNBSVJFUyIsIkNVUCIsIkRJTkVSU0NMVUIiLCJESVNDT1ZFUiIsIkVGVFBPUyIsIkVMTyIsIkpDQiIsIkpDUkVXIiwiTUFEQSIsIk1BRVNUUk8iLCJNRUVaQSJdLCJ0YXJnZXRPcmlnaW5zIjpbImh0dHBzOi8vd3d3LnRlc3QuY29tIl0sIm1mT3JpZ2luIjoiaHR0cHM6Ly90ZXN0ZmxleC5jeWJlcnNvdXJjZS5jb20iLCJhbGxvd2VkUGF5bWVudFR5cGVzIjpbIkNBUkQiXX0sInR5cGUiOiJtZi0yLjEuMCJ9XSwiaXNzIjoiRmxleCBBUEkiLCJleHAiOjE3NDIzMzc2NjAsImlhdCI6MTc0MjMzNjc2MCwianRpIjoiaHoxQUdIQllGdjR1aDFhdyJ9.XsBEmMVLMENfkibHPPHs5zOScBuhI0xuEHPm1gSd-0fuMAXGeWM_6sSri2TUHbVx7BR6_OpL2zNPk37qJ7WZ5VD4tkZNRoW2XXplg5Kq-qDJWGOyq3Qr0PSkK2cmLRLRR3UdJ8efg52ztHL3W_JQpnoXYypx4ow9tDmmLxsFCVgpfeM6Yq2MngMJyxaMzoPD1HoAsI_QGbtR-EdPXlZfLPUzmEbcz994jsVATM_2-1j7eGS--VzDYx863VcnJPWhTRV-n91y2daDx9JLSSJfLutjmFd7KDY7FVTXAl0PpkO0pmvjIUTHzdZDNIDA9B4WHctW1ymi0UonNioOTYJhKg";
            ViewBag.scriptURL = "https://flex.cybersource.com/cybersource/assets/microform/2/flex-microform.min.js";
            ViewBag.scriptIntegrity = "sha256-H/fByApbp4aRmpfXYT6VyWJsYb7T/f7MJwEyU46ie8k=";


            /**
             *The following code compiles the capture context request for microform. 
             *It passess the list of card netwoks you wish to accept & payment types (card, check) the origination URL of the page you are embeding microform in
             * This demonstration showcases leveraging microform for card acceptance
             */

            string clientVersion = "v2";

            List<string> targetOrigins = new List<string>();
            targetOrigins.Add("http://localhost:65309");

            List<string> allowedCardNetworks = new List<string>();
            allowedCardNetworks.Add("VISA");
            allowedCardNetworks.Add("MASTERCARD");
            allowedCardNetworks.Add("AMEX");
            allowedCardNetworks.Add("CARNET");
            allowedCardNetworks.Add("CARTESBANCAIRES");
            allowedCardNetworks.Add("CUP");
            allowedCardNetworks.Add("DINERSCLUB");
            allowedCardNetworks.Add("DISCOVER");
            allowedCardNetworks.Add("EFTPOS");
            allowedCardNetworks.Add("ELO");
            allowedCardNetworks.Add("JCB");
            allowedCardNetworks.Add("JCREW");
            allowedCardNetworks.Add("MADA");
            allowedCardNetworks.Add("MAESTRO");
            allowedCardNetworks.Add("MEEZA");
            List<string> allowedPaymentTypes = new List<string>();
            allowedPaymentTypes.Add("CARD");

            Console.WriteLine("Compiling Capture context request");
            var requestObj = new GenerateCaptureContextRequest(
                ClientVersion: clientVersion,
                TargetOrigins: targetOrigins,
                AllowedCardNetworks: allowedCardNetworks,
                AllowedPaymentTypes: allowedPaymentTypes

            );



            try
            {
                Console.WriteLine("Creating Configuration");
                var configDictionary = new Configuration().GetConfiguration();

                Console.WriteLine("Creating client config");
                var clientConfig = new CyberSource.Client.Configuration(merchConfigDictObj: configDictionary);


                /**
                 * Initiating Request for capture context 
                 * 
                 */
                Console.WriteLine("Compiling api instance");
                var apiInstance = new MicroformIntegrationApi(clientConfig);
                Console.WriteLine("Calling Sessions");
                String result = apiInstance.GenerateCaptureContext(requestObj);
                Console.WriteLine(result);

                /**
                 * With the JWT Capture context response we will want to extrat the Javascript URL and the SRI value
                 * These values are passed dynamically into the application
                 */

                // Create an instance of JwtSecurityTokenHandler
                var handler = new JwtSecurityTokenHandler();

                // Read the JWT token
                var jsonToken = handler.ReadToken(result) as JwtSecurityToken;

                if (jsonToken != null)
                {
                    // Extract specific claims
                    var flxClaim = jsonToken.Claims.FirstOrDefault(claim => claim.Type == "flx");
                    var ctxClaim = jsonToken.Claims.FirstOrDefault(claim => claim.Type == "ctx");

                    if (flxClaim != null)
                    {
                        // Parse the "flx" claim as JSON
                        var flxData = JsonDocument.Parse(flxClaim.Value).RootElement;

                        // Extract specific properties from the "flx" claim
                        string path = flxData.GetProperty("path").GetString();
                        string data = flxData.GetProperty("data").GetString();
                        string origin = flxData.GetProperty("origin").GetString();

                        Console.WriteLine($"Path: {path}");
                        Console.WriteLine($"Data: {data}");
                        Console.WriteLine($"Origin: {origin}");
                    }

                    if (ctxClaim != null)
                    {

                        // Parse the "ctx" claim as JSON array
                        var ctxData = JsonDocument.Parse($"[{ctxClaim.Value}]").RootElement;

                        // Check if ctxData is an array
                        if (ctxData.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var item in ctxData.EnumerateArray())
                            {
                                var dataElement = item.GetProperty("data");
                                string clientLibraryIntegrity = dataElement.GetProperty("clientLibraryIntegrity").GetString();
                                string clientLibrary = dataElement.GetProperty("clientLibrary").GetString();

                                ViewBag.scriptIntegrity = clientLibraryIntegrity;

                                //setting client library URL
                                ViewBag.scriptURL = clientLibrary;

                               Console.WriteLine($"Client Library Integrity: {clientLibraryIntegrity}");
                                Console.WriteLine($"Client Library: {clientLibrary}");
                            }
                        }
                        else
                        {
                            Console.WriteLine("ctx claim is not in expected format");
                        }




                    }
                }
                else
                {
                    Console.WriteLine("Invalid JWT token.");
                }


                /**
                 * Note: The capture context ins a JWT data object and whose signature can be crytographically validated
                 * In order to ensure that the capture context is comming from the correct source, validation is recommended
                 */




                /**
                 * Here we are passing the appropriate paramiters to our page for rendering
                 * 1: the full Capture context resposne
                 * 2: the Javascript URL
                 * 3: The SRI Value
                 * 
                 */

                ViewBag.Jwt = result;

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
                ViewBag.status =result.Status;
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
            _configurationDictionary.Add("runEnvironment", "apitest.cybersource.com");
            _configurationDictionary.Add("keyAlias", "testrest");
            _configurationDictionary.Add("keyPass", "testrest");
            _configurationDictionary.Add("timeout", "300000");

            return _configurationDictionary;
        }
    }
}