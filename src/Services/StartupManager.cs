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
    // Do not forget to remove As<IStartable>() and AutoActivate() from DI registartions of services, 
    // which you want to startup explicitly.

    public class StartupManager : IStartupManager
    {
        private readonly ILog _log;
        private readonly IOrderBookInitializer _orderBookInitializer;
        private readonly IOrderBookReader _orderBookReader;

        public StartupManager(ILog log, IOrderBookInitializer orderBookInitializer, IOrderBookReader orderBookReader)
        {
            _log = log;
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
            await _log.WriteInfoAsync("OrderBooksCacheProvider", "InitOrderBooks", "", "Init order books");

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
                    await _log.WriteErrorAsync("OrderBooksCacheProvider", "InitOrderBooks", "Error on orderbook init. Retry in 5 seconds", ex);
                }

                await Task.Delay(5000);
            }

            await _log.WriteInfoAsync("OrderBooksCacheProvider", "InitOrderBooks", "", "Init OK.");
        }

        private async Task StartReadOrderBooks()
        {
            try
            {
                _orderBookReader.StartRead();
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync("OrderBooksCacheProvider", "Main", "", ex);
                throw;
            }
        }

    }
}