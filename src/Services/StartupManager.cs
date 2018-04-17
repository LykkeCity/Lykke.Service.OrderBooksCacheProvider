using System;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Job.OrderBooksCacheProvider.Core.Services;

namespace Lykke.Job.OrderBooksCacheProvider.Services
{
    // NOTE: Sometimes, startup process which is expressed explicitly is not just better, 
    // but the only way. If this is your case, use this class to manage startup.
    // For example, sometimes some state should be restored before any periodical handler will be started, 
    // or any incoming message will be processed and so on.
    // Do not forget to remove As<IStartable>() and AutoActivate() from DI registrations of services, 
    // which you want to startup explicitly.

    public class StartupManager : IStartupManager
    {
        private readonly ILog _log;
        private readonly IOrderBookInitializer _orderBookInitializer;
        private readonly IOrderBookReader _orderBookReader;

        public StartupManager(ILog log, IOrderBookInitializer orderBookInitializer, IOrderBookReader orderBookReader)
        {
            _log = log.CreateComponentScope(nameof(StartupManager));
            _orderBookInitializer = orderBookInitializer;
            _orderBookReader = orderBookReader;
        }

        public async Task StartAsync()
        {
            await InitOrderBooks();
            await StartReadOrderBooks();
        }

        private async Task InitOrderBooks()
        {
            _log.WriteInfo("InitOrderBooks", null, "Initialization order books");

            bool initilized = false;
            while (!initilized)
            {
                try
                {
                    await _orderBookInitializer.InitOrderBooks();
                    initilized = true;
                }
                catch (Exception ex)
                {
                    _log.WriteError("InitOrderBooks", "Error on orderbook init. Retry in 5 seconds", ex);
                }

                await Task.Delay(5000);
            }

            _log.WriteInfo("InitOrderBooks", null, "Init OK.");
        }

        private async Task StartReadOrderBooks()
        {
            try
            {
                _orderBookReader.StartRead();
            }
            catch (Exception ex)
            {
                _log.WriteError("StartRead", null, ex);
                throw;
            }
        }

    }
}