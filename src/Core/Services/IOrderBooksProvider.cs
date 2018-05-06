using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Job.OrderBooksCacheProvider.Core.Domain;

namespace Lykke.Job.OrderBooksCacheProvider.Core.Services
{
    public interface IOrderBooksProvider
    {
        Task<IEnumerable<OrderBook>> GetCurrentOrderBooksAsync(string assetPair);
    }
}