using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Job.OrderBooksCacheProvider.Core.Domain;
using Lykke.Job.OrderBooksCacheProvider.Core.Services;
using Lykke.Job.OrderBooksCacheProvider.Services.Settings;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace Lykke.Job.OrderBooksCacheProvider.Services
{
    public class OrderBooksHandler : IOrderBooksHandler
    {
        private readonly ConnectionMultiplexer _redis;
        private readonly CacheSettings _settings;
        private readonly ILog _log;

        public OrderBooksHandler(CacheSettings settings, ILogFactory logFactory, ConnectionMultiplexer redis)
        {
            _settings = settings;
            _redis = redis;
            _log = logFactory.CreateLog(this);
        }

        public async Task HandleOrderBook(OrderBook orderBook)
        {
            try
            {
                await _redis.GetDatabase().HashSetAsync(_settings.GetOrderBookKey(orderBook.AssetPair, orderBook.IsBuy), "data", orderBook.ToJson());
            }
            catch (Exception ex)
            {
                _log.Warning("Redis problem", ex);
                throw;
            }
        }
    }
}
