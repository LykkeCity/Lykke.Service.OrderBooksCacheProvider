using System;
using System.IO;
using System.Threading.Tasks;
using Autofac;
using Common.Log;
using Core;
using Core.Services;
using Lykke.Service.OrderBooksCacheProvider.Binders;
using Microsoft.Extensions.Configuration;

namespace Lykke.Service.OrderBooksCacheProvider
{
    public class Program
    {
        static void Main(string[] args)
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            var configuration = builder.Build();

            var settings = SettingsReader.SettingsReader
                .ReadGeneralSettings<ServiceSettings>(new Uri(configuration.GetConnectionString("Main")))
                .OrderBooksCacheProvider;

            var container = new AzureBinder().Bind(settings).Build();

            StartAsync(container).Wait();
        }

        static async Task StartAsync(IContainer container)
        {
            var log = container.Resolve<ILog>();

            await log.WriteInfoAsync("MeSocketClients", "Main", "", "Starting...");

            await InitOrderBooks(container, log);

            await StartReadOrderBooks(container, log);
        }

        private static async Task StartReadOrderBooks(IContainer container, ILog log)
        {
            try
            {
                var orderBookReader = container.Resolve<IOrderBookReader>();
                orderBookReader.StartRead();
            }
            catch (Exception ex)
            {
                await log.WriteErrorAsync("MeSocketClients", "Main", "", ex);
                throw;
            }
        }

        private static async Task InitOrderBooks(IContainer container, ILog log)
        {
            await log.WriteInfoAsync("MeSocketClients", "Main", "", "Init order books");

            bool initilized = false;
            while (!initilized)
            {
                try
                {
                    var orderBookInitializer = container.Resolve<IOrderBookInitializer>();
                    await orderBookInitializer.InitOrderBooks();
                    initilized = true;
                }
                catch (Exception ex)
                {
                    await log.WriteErrorAsync("MeSocketClients", "Main", "Error on orderbook init. Retry in 5 seconds", ex);
                }

                await Task.Delay(5000);
            }

            await log.WriteInfoAsync("MeSocketClients", "Main", "", "Init OK.");
        }
    }
}
