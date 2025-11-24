using Microsoft.Extensions.Configuration.Json;
using nocscienceat.Aes256GcmRsaCryptoService;

namespace nocscienceat.EncryptedConfigurationProvider;

public class EncryptedConfigurationProvider : JsonConfigurationProvider
{
    private readonly bool _optional;
    private readonly string _baseDirectory;
    private readonly string _certificateThumbprint;
    private readonly bool _localMachine;

    // ReSharper disable once ConvertToPrimaryConstructor
    public EncryptedConfigurationProvider(EncryptedConfigurationSource source) : base(source)
    {
        _optional = source.Optional;
        _baseDirectory = source.BaseDirectory;
        _certificateThumbprint = source.CertificateThumbprint;
        _localMachine = source.LocalMachine;
    }

    public override void Load()
    {
        // Build the filename using the certificate thumbprint and .encVault extension
        string filename = Path.Combine(_baseDirectory, $"{_certificateThumbprint}.encVault");

        if (!File.Exists(filename))
        {
            if (_optional)
            {
                return;
            }

            throw new FileNotFoundException($"Configuration file '{filename}' not found.");
        }

        byte[] encryptedConfig;
        try
        {
            TextReader? reader = new StreamReader(filename);
            if (reader == null) throw new ArgumentNullException(nameof(reader));
            encryptedConfig = Convert.FromBase64String(reader.ReadToEnd());
        }
        catch (FormatException ex)
        {
            throw new InvalidOperationException($"The contents of '{filename}' are not valid base64.", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to read base64 encoded EncryptionVault file '{filename}'.", ex);
        }

        // Pass _localMachine and _certificateThumbprint as additional parameters to Decrypt
        byte[] decryptedConfig = CryptoService.Decrypt(encryptedConfig.AsSpan(), _certificateThumbprint, _certificateThumbprint, _localMachine);

        using MemoryStream stream = new(decryptedConfig);

        // Use the base class's JSON loading logic on the decoded stream
        Load(stream);
    }
}
