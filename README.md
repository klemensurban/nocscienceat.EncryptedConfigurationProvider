# nocscienceat.EncryptedConfigurationProvider

A .NET configuration provider that loads encrypted configuration data from files using AES-256-GCM encryption with RSA key wrapping via X.509 certificates.

## Overview

This library extends `Microsoft.Extensions.Configuration` to support encrypted configuration files (`.encVault`). 
Configuration values are encrypted using AES-256-GCM and the encryption key is protected using RSA with an X.509 certificate from the Windows Certificate Store (LocalMachine or CurrentUser.)
For this purpose, it utilizes the `nocscienceat.Aes256GcmRsaCryptoService` library for encryption and decryption operations, but with the same Certificate for Encryption and Signing (AES Key, nonce and tag are signed during Encryption and verified during Decryption)

## Features

- **Secure Configuration Storage**: Encrypts sensitive configuration data at rest
- **Certificate-Based Encryption**: Uses X.509 certificates for RSA key wrapping
- **Seamless Integration**: Works with standard .NET configuration system
- **Certificate Location**: Supports both CurrentUser and LocalMachine certificate stores, Certificate is referenced by its thumbprint


## Installation

Add the package to your project: `nocscienceat.EncryptedConfigurationProvider`


## Usage

1. **Create an encrypted configuration file**  
   Use the `nocscienceat.Aes256GcmRsaCryptoService` NuGet package to encrypt your JSON configuration - see library description for usage details - and encode it with base64.  
   The encrypted file must be named `<certificateThumbprint>.encVault` and placed in a base directory defined in e.g. appsettings.json or other methods of configuration. The ServiceAccount running the application must have access to the certificate and its private key in the specified certificate store.
```
using nocscienceat.Aes256GcmRsaCryptoService;
....
....

byte[] plainText = Encoding.UTF8.GetBytes(jsonString);
ReadOnlySpan<byte> plainTextSpan = plainText.AsSpan();

if (string.IsNullOrWhiteSpace(certificateThumbprint) ||
    certificateThumbprint.Length is not (40 or 64) ||
    !certificateThumbprint.All(Uri.IsHexDigit))
{
    throw new ArgumentException("Certificate thumbprint must be a 40 or 64 character hexadecimal string.");
}
byte[] cipherText = CryptoService.Encrypt(plainTextSpan, certificateThumbprint, certificateThumbprint, true); // true .. LocalMachine Certificate Store

string base64CipherText = Convert.ToBase64String(cipherText, Base64FormattingOptions.InsertLineBreaks);

// Output the Base64-encoded ciphertext to a file with name <CertificateThumbprint>.encVault in the current directory
string outputFileName = $"{certificateThumbprint}.encVault";
File.WriteAllText(outputFileName, base64CipherText);
```
2. **Configure your host to use the provider**  

   - Add the NuGet package `nocscienceat.EncryptedConfigurationProvider` to your project

   - Add in the using block of program.cs:
   `using nocscienceat.EncryptedConfigurationProvider;`

   - Add the provider in your application's configuration setup ..  `IConfigurationBuilder.AddEncryptedConfigurationProvider(optional: false);` e.g. in  a .NET 6+ WebApplication:
```
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEncryptedConfigurationProvider(optional: false);

ConfigurationManager configuration = builder.Configuration;   
```
   - Add the following section to your `appsettings.json`:
```
  "nocscienceat.EncryptedConfigurationProvider": {
    "BaseDirectory": "Directory where to find <certificateThumbprint>.encVault",
    "CertificateThumbprint": "<certificateThumbprint>",
    "LocalMachine": true/false
  }
```

 4. Access the configuration values as usual e.g 
 
```
  public class Worker 
    {
        private readonly IConfiguration _configuration;
       
        public Worker(IConfiguration configuration, ILogger<Worker> logger)
        {
            _configuration = configuration;

        }

        public async returntype SomeWork(parameters)
        {
            AccountInfo? accountinfo = _configuration.GetSection("LdapAccountInfo").Get<AccountInfo>(); // Access decrypted configuration values as encrypted in <certificateThumbprint>.encVault
            // do some work
        }
    }
```
## Notes

- The certificate must be available in the specified certificate store. The ServiceAccount running the application must have access to the certificate and its private key in the specified certificate store.
- The provider will throw if the encrypted file is missing and `optional` is set to `false`.

