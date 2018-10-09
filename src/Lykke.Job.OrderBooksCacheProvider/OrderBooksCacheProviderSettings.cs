using Lykke.Job.OrderBooksCacheProvider.Services.Settings;
using Lykke.Sdk.Settings;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Job.OrderBooksCacheProvider
{
    public class AppSettings : BaseAppSettings
    {
        public OrderBooksCacheProviderSettings OrderBooksCacheProvider { get; set; }
    }

    public class OrderBooksCacheProviderSettings
    {
        public DbSettings Db { get; set; }
        public MatchingEngineSettings MatchingEngine { get; set; }
        public CacheSettings CacheSettings { get; set; }
    }

    public class DbSettings
    {
        [AzureTableCheck]
        public string LogsConnString { get; set; }
    }
}
