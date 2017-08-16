using System.Net;
using Autofac;
using Core;
using Core.Services;
using Lykke.RabbitMqBroker.Subscriber;
using RestSharp;
using StackExchange.Redis;

namespace Services
{
    public static class ServicesBinder
    {
        public static void BindServices(this ContainerBuilder ioc, OrderBooksCacheProviderSettings settings)
        {
            string ipAddress = settings.CacheSettings.RedisInternalHost;
            if (!IPAddress.TryParse(ipAddress, out IPAddress tmp))
            {
                var addresses = Dns.GetHostAddressesAsync(ipAddress).Result;
                ipAddress = addresses[0].ToString();
            }
            var redis = ConnectionMultiplexer.Connect(ipAddress);

            ioc.RegisterInstance(redis).SingleInstance();
            ioc.Register(
                c =>
                    c.Resolve<ConnectionMultiplexer>()
                        .GetServer(ipAddress, settings.CacheSettings.RedisPort));

            ioc.Register(
                c =>
                    c.Resolve<ConnectionMultiplexer>()
                        .GetDatabase());

            var exchangeName = settings.MatchingEngine.RabbitMq.ExchangeOrderbook;
            var rabbitSettings = new RabbitMqSubscriberSettings
            {
                ConnectionString = settings.MatchingEngine.RabbitMq.GetConnectionString(),
                QueueName = $"{exchangeName}.OrderBooksCacheProvider",
                ExchangeName = exchangeName,
            };


            ioc.Register(x => new RestClient()).As<IRestClient>();
            ioc.RegisterType<OrderBookInitializer>().As<IOrderBookInitializer>();

            ioc.RegisterInstance(rabbitSettings);
            ioc.RegisterType<OrderBookReader>().As<IOrderBookReader>().SingleInstance();
            ioc.RegisterType<OrderBooksHandler>().As<IOrderBooksHandler>().SingleInstance();
        }
    }
}
