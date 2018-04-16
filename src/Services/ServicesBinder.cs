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
        public static void BindServices(this ContainerBuilder builder, OrderBooksCacheProviderSettings settings)
        {
            var redis = ConnectionMultiplexer.Connect(settings.CacheSettings.RedisConfiguration);

            builder.RegisterInstance(redis).SingleInstance();
            builder.Register(
                c =>
                    c.Resolve<ConnectionMultiplexer>()
                        .GetServer(redis.GetEndPoints()[0]));

            builder.Register(
                c =>
                    c.Resolve<ConnectionMultiplexer>()
                        .GetDatabase());

            var exchangeName = settings.MatchingEngine.RabbitMq.ExchangeOrderbook;
            var rabbitSettings = new RabbitMqSubscriptionSettings
            {
                ConnectionString = settings.MatchingEngine.RabbitMq.GetConnectionString(),
                QueueName = $"{exchangeName}.OrderBooksCacheProvider",
                ExchangeName = exchangeName
            };


            builder.Register(x => new RestClient()).As<IRestClient>();
            builder.RegisterType<OrderBookInitializer>().As<IOrderBookInitializer>();

            builder.RegisterInstance(rabbitSettings);
            builder.RegisterType<OrderBookReader>().As<IOrderBookReader>().SingleInstance();
            builder.RegisterType<OrderBooksHandler>().As<IOrderBooksHandler>().SingleInstance();
        }
    }
}
