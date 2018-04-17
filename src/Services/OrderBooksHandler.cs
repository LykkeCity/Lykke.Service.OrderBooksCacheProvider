﻿using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Job.OrderBooksCacheProvider.Core;
using Lykke.Job.OrderBooksCacheProvider.Core.Domain;
using Lykke.Job.OrderBooksCacheProvider.Core.Services;
using Microsoft.Extensions.Caching.Distributed;

namespace Lykke.Job.OrderBooksCacheProvider.Services
{
    public class OrderBooksHandler : IOrderBooksHandler
    {
        private readonly IDistributedCache _cache;
        private readonly OrderBooksCacheProviderSettings _settings;
        private readonly ILog _log;

        public OrderBooksHandler(IDistributedCache cache, OrderBooksCacheProviderSettings settings, ILog log)
        {
            _cache = cache;
            _settings = settings;
            _log = log.CreateComponentScope(nameof(OrderBooksHandler));
        }

        public async Task HandleOrderBook(OrderBook orderBook)
        {
            try
            {
                await _cache.SetStringAsync(_settings.CacheSettings.GetOrderBookKey(orderBook.AssetPair, orderBook.IsBuy), orderBook.ToJson());
            }
            catch (Exception ex)
            {
                _log.WriteError(nameof(HandleOrderBook), orderBook, ex);
            }
        }
    }
}
