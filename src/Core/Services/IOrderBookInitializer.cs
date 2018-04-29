using System.Threading.Tasks;

namespace Lykke.Job.OrderBooksCacheProvider.Core.Services
{
    public interface IOrderBookInitializer
    {
        Task InitOrderBooks(bool clearExistingRecords = true);
    }
}