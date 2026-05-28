# CyberSource Flex Samples (.NET)

This repository provides simple examples demonstrating usage of the CyberSource Flex SDK using either a headless JavaScript call (flex-js-sample) or a fully customizable hosted field/microform which is incorporated into your checkout page (flex-microform-sample). For more details on Secure Acceptance Flex visit our Developer Guide at https://developer.cybersource.com/api/developer-guides/dita-flex/SAFlexibleToken.html

## Usage

1. Clone or download this repository.
2. Open `cybersource-flex-samples-dotnet.sln` in Visual Studio.
3. Update `flex-microform-sample/Config/MerchantConfiguration.cs` with your [CyberSource sandbox credentials](https://ebc2test.cybersource.com).
4. Choose either `flex-js-sample` or `flex-microform-sample` as your startup project.
5. Run the project (F5).

Note: It may be necessary to reinstall the packages. Execute the following command in the NuGet Package Manager Console:
```
PM> Update-Package Microsoft.CodeDom.Providers.DotNetCompilerPlatform -r
```

## Configuring Your Own Merchant Credentials

The sample ships with public CyberSource test credentials (`testrest`). To use your own merchant account, follow these steps.

### 1. Generate a shared secret key

Log in to the CyberSource Business Center and navigate to:
**Payment Configuration → Key Management → Generate Key → REST - Shared Secret**

- Sandbox: https://ebc2test.cybersource.com
- Production: https://ebc2.cybersource.com

This produces three values:

| Field | Description |
|---|---|
| `merchantID` | Your merchant account ID |
| `merchantKeyId` | UUID of the shared secret key |
| `merchantsecretKey` | Base64-encoded shared secret value |

### 2. Update `MerchantConfiguration.cs`

Open `flex-microform-sample/Config/MerchantConfiguration.cs` and replace the placeholder values:

```csharp
{ "merchantID",        "your_merchant_id" },
{ "merchantsecretKey", "your_base64_secret_key" },
{ "merchantKeyId",     "your-key-id-uuid" },
```

Change `runEnvironment` to match your target environment:

```csharp
// Sandbox (default):
{ "runEnvironment", "apitest.cybersource.com" },

// Production:
{ "runEnvironment", "api.cybersource.com" },
```

### 3. Update the target origin

The capture context is scoped to the origin of the page that will embed Microform. Update the `targetOrigin` in `flex-microform-sample/Controllers/HomeController.cs` to match your deployment:

```csharp
var result = await CaptureContext.GenerateAsync(targetOrigin: "https://www.yoursite.com");
```

> **Note:** The target origin must use HTTPS in production.

## Project Structure

```
flex-microform-sample/
├── Config/
│   └── MerchantConfiguration.cs    — Merchant credentials and SDK settings
├── Controllers/
│   └── HomeController.cs           — Orchestrates the three-step payment flow
├── Microform/
│   ├── CaptureContext.cs           — Calls GenerateCaptureContext API; validates and parses the JWT
│   └── CaptureContextValidator.cs  — Verifies the RS256 signature of the capture context JWT
└── Views/Home/
    ├── Checkout.cshtml             — Embeds the Microform hosted card-entry fields
    ├── Token.cshtml                — Debug view: displays the transient token JWT
    └── Receipt.cshtml              — Displays the payment authorisation result
```

## Requirements

* Visual Studio 2019 or later
* .NET Framework 4.8

**_NOTE: We also have samples for Flex available in Java, PHP & Node.js_**

## API Reference

While these examples use the JavaScript libraries which we recommend as the most convenient option, you can try out the APIs behind the JavaScript SDKs by visiting our API Reference at https://developer.cybersource.com/api/reference/api-reference.html

## Background on PCI-DSS

Storing your customer's card data can dramatically increase your repeat-customer conversion rate, but can also add additional risk and [PCI DSS](https://www.pcisecuritystandards.org/pci_security/) overhead. You can mitigate these costs by tokenizing card data. CyberSource will store your customer's card data within secure Visa data centers, replacing it with a token that only you can use.

Secure Acceptance Flexible Token is a secure method for tokenizing card data, that leaves you in total control of the customer experience. Your customer's card number is encrypted on their own device — for example inside a browser or native app — and sent directly to CyberSource. This means card data bypasses your systems altogether. This can help you qualify for [SAQ A](https://www.pcisecuritystandards.org/documents/Understanding_SAQs_PCI_DSS_v3.pdf) based PCI DSS assessments for web-based integrations, and [SAQ A-EP](https://www.pcisecuritystandards.org/documents/Understanding_SAQs_PCI_DSS_v3.pdf) for native app integrations.

You are in total control of the look and feel, with the ability to seamlessly blend the solution in to your existing checkout flow, on web or in-app.

On-device encryption helps to protect your customers from attacks on network middleware such as app accelerators, DLPs, CDNs, and malicious hotspots.

The token can be used in lieu of actual card data in server-side requests for other CyberSource services, for example to make a payment, using our REST APIs: https://developer.cybersource.com/api/reference/api-reference.html

## Samples

### JavaScript (Flex API) Sample

This sample demonstrates how your checkout form can remain exactly as it is today, with the only addition of a JavaScript call to tokenize the customer's credit card information. This happens directly between their browser and CyberSource, replacing the provided data with a secure PCI-compliant token. This can then be sent to your server along with the other non-PCI order data. This can help achieve PCI-DSS SAQ A-EP level compliance for your application.

### Microform Sample

This sample demonstrates how you can replace the sensitive data fields (credit card number) on your checkout form with a field (Flex Microform) hosted entirely on CyberSource servers.

The integration follows three steps:

1. **Server — Generate capture context:** `HomeController` calls `CaptureContext.GenerateAsync()`, which requests a signed JWT from the CyberSource API. The JWT signature is verified against CyberSource's RSA public key (`CaptureContextValidator`) before any values in it are trusted. The JWT contains the URL and integrity hash of the Microform JavaScript bundle.

2. **Client — Tokenize the card:** The checkout page loads the Microform JavaScript (verified by SRI) and renders hosted iframes for card number and CVV. The customer's card data is encrypted and sent directly to CyberSource — it never touches the merchant server. CyberSource returns a short-lived transient token JWT.

3. **Server — Authorize the payment:** The transient token is posted to `HomeController.Receipt()`, which submits it to the CyberSource Payments API in place of a raw card number.

This approach can help achieve PCI-DSS SAQ A level compliance, as even your client-side code contains no mechanism to handle card data.

## Using the Flex Payment Token

The transient token JWT returned by Microform is passed to the CyberSource Payments API in the `tokenInformation` field:

```json
{
  "clientReferenceInformation": {
    "code": "flex-microform-sample"
  },
  "processingInformation": {
    "commerceIndicator": "internet"
  },
  "tokenInformation": {
    "transientTokenJwt": "<transient token JWT from Microform>"
  },
  "orderInformation": {
    "amountDetails": {
      "totalAmount": "102.21",
      "currency": "USD"
    },
    "billTo": {
      "firstName": "John",
      "lastName": "Doe",
      "address1": "1 Market St",
      "locality": "San Francisco",
      "administrativeArea": "CA",
      "postalCode": "94105",
      "country": "US",
      "email": "test@cybs.com"
    }
  }
}
```
