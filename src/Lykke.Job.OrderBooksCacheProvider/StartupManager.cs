using System;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Job.OrderBooksCacheProvider.Core.Services;
using Lykke.Sdk;

namespace Lykke.Job.OrderBooksCacheProvider
{
    public class StartupManager : IStartupManager
    {
        private readonly ILog _log;
        private readonly IOrderBookInitializer _orderBookInitializer;
        private readonly IOrderBookReader _orderBookReader;

        public StartupManager(ILogFactory logFactory, IOrderBookInitializer orderBookInitializer, IOrderBookReader orderBookReader)
        {
            _orderBookInitializer = orderBookInitializer;
            _orderBookReader = orderBookReader;
            _log = logFactory.CreateLog(this);
        }

        public async Task StartAsync()
        {
            await InitOrderBooks();
            StartReadOrderBooks();
        }

        private async Task InitOrderBooks()
        {
            _log.Info("Initialization order books");

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
                    _log.Error(ex, "Error on orderbook init. Retry in 5 seconds");
                }

                await Task.Delay(5000);
            }

            _log.Info("Init OK.");
        }

        private void StartReadOrderBooks()
        {
            try
            {
                _orderBookReader.StartRead();
            }
            catch (Exception ex)
            {
                _log.Error(ex, "StartRead");
                throw;
            }
        }

    }
}