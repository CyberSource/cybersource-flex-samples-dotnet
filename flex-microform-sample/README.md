# Flex Microform Sample

Flex Microform is a CyberSource-hosted HTML/JavaScript component that replaces the card number input field on your checkout page and calls the Flex API on your behalf. This simple example integration demonstrates using the Flex Microform SDK to embed this PCI SAQ A level component in your form. For more details on this see our Developer Guide at:  https://developer.cybersource.com/api/developer-guides/dita-flex/SAFlexibleToken/FlexMicroform.html

## Prerequisites

-- Visual Studio 2017

## Setup Instructions

1. Select the flex-microform-sample project in the cybersource-flex-samples-dotnet solution

2. Modify Configuration class in `Controllers\HomeController.cs` with the CyberSource REST credentials created through [EBC Portal](https://ebc2test.cybersource.com/).

  ```csharp
            _configurationDictionary.Add("merchantID", "YOUR_MERCHANT_ID");
            _configurationDictionary.Add("merchantsecretKey", "YOUR_SECRET_KEY");
            _configurationDictionary.Add("merchantKeyId", "YOUR_KEY_ID");
  ```

3. Set flex-microform-sample as the startup project and run the application using Visual Studio (F5)
  
4. This will open a browser at the sample checkout page

## Tips

- NOTE: You will probably have to update the targetOrigin property at line 22 of `Controllers\HomeController.cs` to match your development IIS Server.

- Safari version 10 and below does not support `RsaOaep256` encryption schema, for those browser please specify encryption type `RsaOaep` when making a call to the `/keys` endpoint.  For a detailed example please see [FlexKeyProvider.java](./src/main/java/com.cybersource/example/FlexKeyProvider.java), line 47.
