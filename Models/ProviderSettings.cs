namespace nocscienceat.EncryptedConfigurationProvider.Models;

internal class ProviderSettings
{
    public string BaseDirectory { get; set; } = string.Empty;
    public string CertificateThumbprint { get; set; } = string.Empty;
    public bool LocalMachine { get; set; } = true;
}