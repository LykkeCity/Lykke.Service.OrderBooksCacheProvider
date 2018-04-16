using System.Threading.Tasks;
using Core.Domain;

namespace Core.Services
{
    public interface IOrderBookInitializer
    {
        Task InitOrderBooks();
    }

    public interface IOrderBooksHandler
    {
        Task HandleOrderBook(OrderBook orderBook);
    }
    
    public interface IOrderBookReader
    {
        void StartRead();
    }
}
