using Microsoft.Extensions.Configuration;
using nocscienceat.EncryptedConfigurationProvider.Models;

namespace nocscienceat.EncryptedConfigurationProvider;

public static class EncryptedConfigurationProviderExtensions
{
    public static IConfigurationBuilder AddEncryptedConfigurationProvider(
        this IConfigurationBuilder builder,
        bool optional = false)
    {
        IConfigurationRoot tempConfig = builder.Build();

        // Get the "EncryptedConfiguration" section
        ProviderSettings? providerSettings = tempConfig.GetSection("nocscienceat.EncryptedConfigurationProvider").Get<ProviderSettings>();
        if (providerSettings is null)
        {
            throw new InvalidOperationException("EncryptedConfigurationProvider settings are missing in the configuration.");
        }

        EncryptedConfigurationSource source = new()
        {
            BaseDirectory = providerSettings.BaseDirectory,
            CertificateThumbprint = providerSettings.CertificateThumbprint,
            Optional = optional,
            LocalMachine = providerSettings.LocalMachine
        };
        builder.Add(source);
        return builder;
    }
}