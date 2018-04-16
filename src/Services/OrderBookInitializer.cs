using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Common.Log;
using Core;
using Core.Services;
using RestSharp;
using Common;
using Core.Domain;
using StackExchange.Redis;

namespace Services
{
    public class OrderBookInitializer : IOrderBookInitializer
    {
        private readonly IOrderBooksHandler _orderBooksHandler;
        private readonly IRestClient _restClient;
        private readonly OrderBooksCacheProviderSettings _settings;
        private readonly ILog _log;
        private readonly IServer _redisServer;
        private readonly IDatabase _redisDatabase;

        public OrderBookInitializer(IOrderBooksHandler orderBooksHandler, IRestClient restClient,
            OrderBooksCacheProviderSettings settings, ILog log, IServer redisServer, IDatabase redisDatabase)
        {
            _orderBooksHandler = orderBooksHandler;
            _restClient = restClient;
            _settings = settings;
            _log = log;
            _redisServer = redisServer;
            _redisDatabase = redisDatabase;
            _restClient.BaseUrl = settings.MatchingEngine.GetOrderBookInitUri();
        }

        public async Task InitOrderBooks()
        {
            ClearExistingRecords();

            var request = new RestRequest(Method.GET);

            var t = new TaskCompletionSource<IRestResponse>();
            _restClient.ExecuteAsync(request, resp => { t.SetResult(resp); });
            var response = await t.Task;

            if (response.ResponseStatus == ResponseStatus.Completed && response.StatusCode == HttpStatusCode.OK)
            {
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
            var keys = _redisServer.Keys(pattern: _settings.CacheSettings.FinanceDataCacheInstance + "*").ToArray();
            _redisDatabase.KeyDelete(keys);
        }
    }
}
