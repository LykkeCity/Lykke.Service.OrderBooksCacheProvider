using System;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Job.OrderBooksCacheProvider.Core
{
    public class DbSettings
    {
        [AzureTableCheck]
        public string LogsConnString { get; set; }
    }

    public class CacheSettings
    {
        public string FinanceDataCacheInstance { get; set; }
        public string RedisConfiguration { get; set; }

        public string OrderBooksCacheKeyPattern { get; set; }

        public TimeSpan ForceUpdateInterval { get; set; }
    }

    public static class CacheSettingsExt
    {
        public static string GetOrderBookKey(this CacheSettings settings, string assetPairId, bool isBuy)
        {
            return string.Format(settings.OrderBooksCacheKeyPattern, assetPairId, isBuy);
        }
    }

    public class MatchingEngineSettings
    {
        public IpEndpointSettings IpEndpoint { get; set; }
        public RabbitMqSettings RabbitMq { get; set; }
        public string HttpOrderBookPort { get; set; }
    }

    public class RabbitMqSettings
    {
        [AmqpCheck]
        public string ConnectionString { get; set; }
        public string ExchangeOrderbook { get; set; }
    }

    public static class Ext
    {
        public static Uri GetOrderBookInitUri(this MatchingEngineSettings settings)
        {
            return new Uri($"http://{settings.IpEndpoint.InternalHost}:{settings.HttpOrderBookPort}/orderBooks");
        }
    }

    public class IpEndpointSettings
    {
        public string InternalHost { get; set; }
    }

    public class OrderBooksCacheProviderSettings
    {
        public DbSettings Db { get; set; }
        public MatchingEngineSettings MatchingEngine { get; set; }
        public CacheSettings CacheSettings { get; set; }
    }

    public class AppSettings
    {
        public OrderBooksCacheProviderSettings OrderBooksCacheProvider { get; set; }
        public SlackNotificationsSettings SlackNotifications { get; set; }
    }

    public class SlackNotificationsSettings
    {
        public AzureQueueSettings AzureQueue { get; set; }
    }

    public class AzureQueueSettings
    {
        public string ConnectionString { get; set; }

        public string QueueName { get; set; }
    }

}
