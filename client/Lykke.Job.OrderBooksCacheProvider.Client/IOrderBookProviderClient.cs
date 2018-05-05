using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Job.OrderBooksCacheProvider.Client
{
    public interface IOrderBookProviderClient
    {
        Task<List<OrderBookRaw>> GetOrderBookRawAsync(string assetPair);
        Task<OrderBook> GetOrderBookAsync(string assetPair);
    }
}
