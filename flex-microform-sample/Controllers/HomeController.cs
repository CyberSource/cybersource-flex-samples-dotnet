using CyberSource.Api;
using CyberSource.Model;
using flex_microform_sample.Config;
using flex_microform_sample.Microform;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace flex_microform_sample.Controllers
{
    public class HomeController : Controller
    {
        // ---------------------------------------------------------------------------
        // Step 1 — Generate a capture context and render the checkout page.
        //
        // The capture context is a server-side JWT that authorises the browser to
        // initialise Microform.  It is requested fresh on every page load so that
        // it carries a current expiry and the correct target origin.
        // ---------------------------------------------------------------------------
        public async Task<ActionResult> Checkout()
        {
            var result = await CaptureContext.GenerateAsync(targetOrigin: "http://localhost:65309");

            if (result == null)
            {
                // CaptureContext.Generate() logs the error; surface a simple message to the view.
                ViewBag.Error = "Could not retrieve a capture context from CyberSource. Check the console for details.";
                return View();
            }

            // Pass the three values the Checkout view needs:
            //   Jwt              — the full capture context JWT, given to new Flex(captureContext) on the client
            //   scriptURL        — the Microform JS bundle URL, embedded in the <script> tag
            //   scriptIntegrity  — the SRI hash that verifies the bundle has not been tampered with
            ViewBag.Jwt             = result.Jwt;
            ViewBag.scriptURL       = result.ClientLibraryUrl;
            ViewBag.scriptIntegrity = result.ClientLibraryIntegrity;

            return View();
        }

        // ---------------------------------------------------------------------------
        // Step 2 (optional) — Display the transient token returned by Microform.
        //
        // This intermediate page is useful for learning and debugging.  In a
        // production integration you would skip straight to the Receipt action.
        // ---------------------------------------------------------------------------
        // [HttpPost]             — only accept form submissions; GET requests are rejected
        //                          with 405, blocking the <img src="..."> CSRF attack vector.
        // [ValidateAntiForgeryToken] — verifies the __RequestVerificationToken cookie/field
        //                          pair, preventing cross-origin forged POST requests.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Token()
        {
            dynamic flexResponse = Request.Params["flexResponse"];
            ViewBag.JWT = flexResponse.Replace("\r\n", "").Replace("\"", "");
            return View();
        }

        // ---------------------------------------------------------------------------
        // Step 3 — Authorise the payment using the transient token from Microform.
        //
        // The transient token JWT (flexResponse) is passed from the browser after
        // Microform tokenises the card.  The card number never reaches this server.
        // ---------------------------------------------------------------------------
        // [HttpPost]             — only accept form submissions; GET requests are rejected
        //                          with 405, blocking the <img src="..."> CSRF attack vector.
        // [ValidateAntiForgeryToken] — verifies the __RequestVerificationToken cookie/field
        //                          pair, preventing cross-origin forged POST requests.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Receipt()
        {
            string transientToken = Request.Params["flexResponse"];

            // Build the payment request.
            // In a real integration these order details come from the shopper's cart.
            var request = new CreatePaymentRequest
            {
                ClientReferenceInformation = new Ptsv2paymentsClientReferenceInformation
                {
                    Code = "flex-microform-sample"
                },

                ProcessingInformation = new Ptsv2paymentsProcessingInformation
                {
                    CommerceIndicator = "internet"
                },

                OrderInformation = new Ptsv2paymentsOrderInformation
                {
                    AmountDetails = new Ptsv2paymentsOrderInformationAmountDetails
                    {
                        TotalAmount = "102.21",
                        Currency    = "USD"
                    },
                    BillTo = new Ptsv2paymentsOrderInformationBillTo
                    {
                        FirstName           = "John",
                        LastName            = "Doe",
                        Address1            = "1 Market St",
                        Locality            = "San Francisco",
                        AdministrativeArea  = "CA",
                        PostalCode          = "94105",
                        Country             = "US",
                        Email               = "test@cybs.com"
                    }
                },

                // Pass the transient token JWT issued by Microform — this replaces the raw card number.
                TokenInformation = new Ptsv2paymentsTokenInformation
                {
                    TransientTokenJwt = transientToken
                }
            };

            try
            {
                Console.WriteLine("Calling CyberSource API: CreatePayment");
                var configDictionary = new MerchantConfiguration().GetConfiguration();
                var clientConfig     = new CyberSource.Client.Configuration(merchConfigDictObj: configDictionary);
                var apiInstance      = new PaymentsApi(clientConfig);

                var result = apiInstance.CreatePayment(request);

                ViewBag.paymentJson = JsonConvert.SerializeObject(result, Formatting.Indented);
                ViewBag.status      = result.Status;
            }
            catch (Exception e)
            {
                Console.WriteLine("CreatePayment exception: " + e.Message);
                return null;
            }

            return View();
        }
    }
}
