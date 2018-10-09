using Common.Log;
using Lykke.Common.Log;
using Lykke.Job.OrderBooksCacheProvider.Core.Domain;
using Lykke.Job.OrderBooksCacheProvider.Core.Services;
using Lykke.Job.OrderBooksCacheProvider.Services.Settings;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Job.OrderBooksCacheProvider.Services
{
    public class OrderBooksProvider : IOrderBooksProvider
    {
        private readonly ConnectionMultiplexer _redis;
        private readonly CacheSettings _settings;
        private readonly ILog _log;

        public OrderBooksProvider(CacheSettings settings, ILogFactory logFactory, ConnectionMultiplexer redis)
        {
            _settings = settings;
            _redis = redis;
            _log = logFactory.CreateLog(this);
        }

        public async Task<IEnumerable<OrderBook>> GetCurrentOrderBooksAsync(string assetPair)
        {
            try
            {

                var buyTask = _redis.GetDatabase().HashGetAsync(_settings.GetOrderBookKey(assetPair, true), "data");
                var sellTask = _redis.GetDatabase().HashGetAsync(_settings.GetOrderBookKey(assetPair, false), "data");
                await Task.WhenAll(buyTask, sellTask);

                if (string.IsNullOrEmpty(buyTask.Result) || string.IsNullOrEmpty(sellTask.Result))
                {
                    return null;
                }
                var result = new List<OrderBook>();
                result.Add(JsonConvert.DeserializeObject<OrderBook>(buyTask.Result));
                result.Add(JsonConvert.DeserializeObject<OrderBook>(sellTask.Result));

                return result;
            }
            catch (Exception ex)
            {
                _log.Warning(nameof(GetCurrentOrderBooksAsync), ex, assetPair);
                throw;
            }
        }
    }
}