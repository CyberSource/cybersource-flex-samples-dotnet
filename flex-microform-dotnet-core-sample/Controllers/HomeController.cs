using flex_microform_dotnet_core_sample.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CyberSource.Api;
using CyberSource.Model;
using Newtonsoft.Json;

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

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index() => View();

        public IActionResult Checkout()
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
                var result = apiInstance.GeneratePublicKey("JWT", requestObj);

                _logger.LogInformation(JsonConvert.SerializeObject(result));
                _logger.LogInformation(result.KeyId);

                // result.KeyId here is the temporary token
                ViewBag.Jwk = result.KeyId; // eyJraWQiOiJ6dSIsImFsZyI6IlJTMjU2In0.eyJmbHgiOnsicGF0aCI6Ii9mbGV4L3YyL3Rva2VucyIsImRhdGEiOiJNTG0vNk1zdW83bm1uZ2toaWZJVjB4QUFFT21qZmROVlI2aVNWeXFlaUg1ZHpLQ080cWUvL0VKYTJ6WU5pU3haeUgxMlFHNHhaSWRyR1RtNzN1NmVLTjZuMFdaVWtKU005WW5jaFduYjBSM3ErempJYmU0Yk0wOWZhOG5nUTlQOXNIeTAiLCJvcmlnaW4iOiJodHRwczovL3Rlc3RmbGV4LmN5YmVyc291cmNlLmNvbSIsImp3ayI6eyJrdHkiOiJSU0EiLCJlIjoiQVFBQiIsInVzZSI6ImVuYyIsIm4iOiJpTXJleE5lVWlmOVFvUWRKMWFrbWVvYTRBaWhjRDVqdS1ZcS1iVlNVcDNCZGdUSm9EUndGV3FFUkZic2tqSUp2M0RDRU9XMzZHbWkxZlZJS3dVTDdrQlFmM3dFc0N5VEdadDYtdGFfdjdkZS14SUduMlJjZGFsZXJHUDFyZFE1VlJQYVBNcmVTcnFKSl85bjlyVE44WkZvN1ZteXlPM2tkd3ZzNnVMSGVVZEdPVFdaU3hqaWJkTEdjMVJmTVdpQXRKQktkMHNtNzhGdUc3N042RTQxM1J4Ml9BNF81SV8xSFhkRDRhbDNfWlZabFJ4bEtBMUJjbUcyQXhMNlVrTDFsMFp0RjdwUzB6YmY4Z1ljeTdOQVAyeFJBOW5lalR3ZE8zaWRYZm9rbkFiQmMxU2VYdFNxaXJETGRISkcxbk1zTm9MOWVVNTRFb25MemZweVRmeGdUWVEiLCJraWQiOiIwOEpYY20xUDhSWlBUQVhYN2s0Y2txSFpGVkVsMzFydCJ9fSwiY3R4IjpbeyJkYXRhIjp7InRhcmdldE9yaWdpbnMiOlsiaHR0cDovL2xvY2FsaG9zdDo2NTMwOSJdLCJtZk9yaWdpbiI6Imh0dHBzOi8vdGVzdGZsZXguY3liZXJzb3VyY2UuY29tIn0sInR5cGUiOiJtZi0wLjExLjAifV0sImlzcyI6IkZsZXggQVBJIiwiZXhwIjoxNjI5ODgyNTc4LCJpYXQiOjE2Mjk4ODE2NzgsImp0aSI6IldjTFFubnA4QUxNZm9LblIifQ.dW4KSTaaMo1F_AAnUYjRRUIAtvhHE8QYCClOd_hC49BGuqyXRMVYIWzZWCe-qFtQMqCgU4rGNlwADiBf0MABjw_r8rW81Lh0NtLs1uUpzfwRMkMEXAI4mz_hWcwahDLACQLpr8nxfLs64UkNy5l_u_ANSPnUsyE2jlDxInPJ8IXUZRw14fYsc4uUQiHK-TVARvP6nbSc8TGTSrv3SC2qaeAM-PMOGz5gpUfPfoTX0oy7bbnluWcGYICCzqOi2mwS70bO3s6ukZgaF6VNEbdrul1_n632cNqb2J25ElEqoQVU9Mt3HylaShAzcs2kJyvXYgTXrgL2kGdHt91lLVLCOA
            }
            catch (Exception e)
            {
                _logger.LogError("Exception on calling the API: " + e.Message);
            }

            return View();
        }
        
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() 
            => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
