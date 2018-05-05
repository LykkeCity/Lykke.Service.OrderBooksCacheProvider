using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Lykke.Job.OrderBooksCacheProvider.Client
{
    public class OrderBookProviderClient : IOrderBookProviderClient, IDisposable
    {
        private readonly string _serviceUrl;
        private readonly HttpClient _client;

        public OrderBookProviderClient(string serviceUrl)
        {
            _serviceUrl = serviceUrl;
            if (!_serviceUrl.EndsWith("/")) _serviceUrl += "/";
            _serviceUrl += "api/OrderBookProvider?assetPair={0}";
            _client = new HttpClient();
        }

        public async Task<List<OrderBookRaw>> GetOrderBookRawAsync(string assetPair)
        {
            var url = string.Format(_serviceUrl, assetPair);
            var responce = await _client.GetAsync(url);
            var content = await responce.Content.ReadAsStringAsync();
            if (responce.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Error on call to {url}. StatusCode = {responce.StatusCode}. Content = {content}");
            }

            return JsonConvert.DeserializeObject<List<OrderBookRaw>>(content);
        }

        public async Task<OrderBook> GetOrderBookAsync(string assetPair)
        {
            var raw = await GetOrderBookRawAsync(assetPair);

            var result = new OrderBook()
            {
                AssetPair = assetPair,
                Timestamp = DateTime.UtcNow,
                Prices = new List<VolumePrice>()
            };

            if (raw.Count == 0)
            {
                result.Timestamp = DateTime.UtcNow;
            }

            if (raw.Count >= 1)
            {
                result.Timestamp = raw[0].Timestamp;
                result.Prices.AddRange(raw[0].Prices);
            }
            
            if (raw.Count >= 2)
            {
                if (result.Timestamp < raw[1].Timestamp)
                {
                    result.Timestamp = raw[1].Timestamp;
                }
                
                result.Prices.AddRange(raw[1].Prices);
            }

            return result;
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}