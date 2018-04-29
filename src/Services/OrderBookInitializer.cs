using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Job.OrderBooksCacheProvider.Core;
using Lykke.Job.OrderBooksCacheProvider.Core.Domain;
using Lykke.Job.OrderBooksCacheProvider.Core.Services;
using RestSharp;
using StackExchange.Redis;

namespace Lykke.Job.OrderBooksCacheProvider.Services
{
    public class OrderBookInitializer : IOrderBookInitializer
    {
        private readonly IOrderBooksHandler _orderBooksHandler;
        private readonly IRestClient _restClient;
        private readonly CacheSettings _cacheSettings;
        private readonly ILog _log;
        private readonly ConnectionMultiplexer _redis;

        public OrderBookInitializer(
            IOrderBooksHandler orderBooksHandler, 
            IRestClient restClient,
            CacheSettings cacheSettings,
            MatchingEngineSettings settings,
            ILog log,
            ConnectionMultiplexer redis)
        {
            _orderBooksHandler = orderBooksHandler;
            _restClient = restClient;
            _cacheSettings = cacheSettings;
            _log = log;
            _redis = redis;
            _restClient.BaseUrl = settings.GetOrderBookInitUri();
        }

        public async Task InitOrderBooks(bool clearExistingRecords = true)
        {
            var request = new RestRequest(Method.GET);

            var t = new TaskCompletionSource<IRestResponse>();
            _restClient.ExecuteAsync(request, resp => { t.SetResult(resp); });
            var response = await t.Task;

            if (response.ResponseStatus == ResponseStatus.Completed && response.StatusCode == HttpStatusCode.OK)
            {
                if (clearExistingRecords)
                {
                    ClearExistingRecords();
                }

                var orderBooks = response.Content.DeserializeJson<OrderBook[]>();
                if (orderBooks != null && orderBooks.Any())
                {
                    foreach (var orderBook in orderBooks)
                    {
                        await _orderBooksHandler.HandleOrderBook(orderBook);
                    }
                }
                else
                {
                    await _log.WriteWarningAsync("OrderBookInitializer", "InitOrderBooks", "", "No orderbooks on init");
                }

                return;
            }

            var exception = response.ErrorException ?? new Exception(response.Content);
            await _log.WriteErrorAsync("OrderBookInitializer", "InitOrderBooks", "", exception);

            throw exception;
        }

        private void ClearExistingRecords()
        {
            var keys = _redis.GetServer(_redis.GetEndPoints()[0])
                .Keys(pattern: _cacheSettings.FinanceDataCacheInstance + "*").ToArray();
            _redis.GetDatabase().KeyDelete(keys);
        }
    }
}
