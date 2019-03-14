using System;

namespace Lykke.Job.OrderBooksCacheProvider.Services.Settings
{
    public class CacheSettings
    {
        public string FinanceDataCacheInstance { get; set; }
        public string RedisConfiguration { get; set; }

        public string OrderBooksCacheKeyPattern { get; set; }

        public TimeSpan ForceUpdateInterval { get; set; }

        public string GetOrderBookKey(string assetPairId, bool isBuy)
        {
            return FinanceDataCacheInstance + string.Format(OrderBooksCacheKeyPattern, assetPairId, isBuy);
        }
    }
}