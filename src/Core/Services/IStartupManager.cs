using System.Threading.Tasks;

namespace Lykke.Job.OrderBooksCacheProvider.Core.Services
{
    public interface IStartupManager
    {
        Task StartAsync();
    }
}