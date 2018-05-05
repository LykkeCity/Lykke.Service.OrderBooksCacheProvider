using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Job.OrderBooksCacheProvider.Core;
using Lykke.Job.OrderBooksCacheProvider.Core.Domain;
using Lykke.Job.OrderBooksCacheProvider.Core.Services;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace Lykke.Job.OrderBooksCacheProvider.Services
{
    public class OrderBooksProvider : IOrderBooksProvider
    {
        private readonly IDistributedCache _cache;
        private readonly CacheSettings _settings;
        private readonly ILog _log;

        public OrderBooksProvider(IDistributedCache cache, CacheSettings settings, ILog log)
        {
            _cache = cache;
            _settings = settings;
            _log = log;
        }

        public async Task<IEnumerable<OrderBook>> GetCurrentOrderBooksAsync(string assetPair)
        {
            try
            {
                var buyTask = _cache.GetStringAsync(_settings.GetOrderBookKey(assetPair, true));
                var sellTask = _cache.GetStringAsync(_settings.GetOrderBookKey(assetPair, false));
                await Task.WhenAll(buyTask, sellTask);

                if (string.IsNullOrEmpty(buyTask.Result) || string.IsNullOrEmpty(sellTask.Result))
                {
                    return null;
                }
                var result = new List<OrderBook>();
                result.Add(JsonConvert.DeserializeObject<OrderBook>(buyTask.Result));
                result.Add(JsonConvert.DeserializeObject<OrderBook>(buyTask.Result));

                return result;
            }
            catch (Exception ex)
            {
                _log.WriteWarning(nameof(GetCurrentOrderBooksAsync), assetPair, "", ex);
                throw;
            }
        }
    }
}