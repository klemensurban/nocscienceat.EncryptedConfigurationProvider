using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace nocscienceat.EncryptedConfigurationProvider;

public class EncryptedConfigurationSource : JsonConfigurationSource
{
    private string _certificateThumbprint = string.Empty;
    public string BaseDirectory { get; set; } = string.Empty;
    public bool LocalMachine { get; set; } = true;

    public string CertificateThumbprint
    {
        get => _certificateThumbprint;
        set
        {
            // Certificate thumbprints are typically 40 hexadecimal characters (SHA-1)
            // or 64 hexadecimal characters (SHA-256)
            string? thumbprint = value?.ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(thumbprint) ||
                thumbprint.Length is not (40 or 64) ||
                !thumbprint.All(Uri.IsHexDigit))
            {
                throw new ArgumentException("Certificate thumbprint must be a 40 or 64 character hexadecimal string.", nameof(value));
            }
            _certificateThumbprint = thumbprint;
        }
    }
    

    public override IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        EnsureDefaults(builder);

        return new EncryptedConfigurationProvider(this);
    }
}
