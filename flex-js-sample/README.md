# Flex API Sample

A simple client-side tokenization example integration using Flex JavaScript SDK to access the Flex API. For more details on this see our Developer Guide at: https://developer.cybersource.com/api/developer-guides/dita-flex/SAFlexibleToken/FlexAPI.html 

## Prerequisites

-- Visual Studio 2017

## Setup Instructions

1. Select the flex-js-sample project in the cybersource-flex-samples-dotnet solution

2. Modify Configuration class in `Controllers\HomeController.cs` with the CyberSource REST credentials created through [EBC Portal](https://ebc2test.cybersource.com/).

  ```csharp
            _configurationDictionary.Add("merchantID", "YOUR_MERCHANT_ID");
            _configurationDictionary.Add("merchantsecretKey", "YOUR_SECRET_KEY");
            _configurationDictionary.Add("merchantKeyId", "YOUR_KEY_ID");
  ```

3. Set flex-js-sample as the startup project and run the application using Visual Studio (F5)
  
4. This will open a browser at the sample checkout page

## Tips

- NOTE: You will probably have to update the targetOrigin property at line 22 of `Controllers\HomeController.cs` to match your development IIS Server.

- If you are having issues, checkout the full [FLEX API documentation](https://developer.cybersource.com/api/developer-guides/dita-flex/SAFlexibleToken/FlexAPI.html).

- If the application throws `java.security.InvalidKeyException: Illegal key size` you have probably not installed the [JCE unlimited policy files](http://www.oracle.com/technetwork/java/javase/downloads/jce8-download-2133166.html).

- Safari version 10 and below does not support `RsaOaep256` encryption schema, for those browser please specify encryption type `RsaOaep` when making a call to the `/keys` endpoint.  For a detailed example please see [FlexKeyProvider.java](./src/main/java/com.cybersource/example/FlexKeyProvider.java), line 47.
