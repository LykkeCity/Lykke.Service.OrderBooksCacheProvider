using Autofac;
using Lykke.Job.OrderBooksCacheProvider.Core.Services;
using Lykke.Job.OrderBooksCacheProvider.PeriodicalHandlers;
using Lykke.Job.OrderBooksCacheProvider.Services;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Sdk;
using Lykke.SettingsReader;
using RestSharp;
using StackExchange.Redis;
using System;

namespace Lykke.Job.OrderBooksCacheProvider.Modules
{
    public class JobModule : Module
    {
        private readonly OrderBooksCacheProviderSettings _settings;

        public JobModule(IReloadingManager<AppSettings> settings)
        {
            _settings = settings.CurrentValue.OrderBooksCacheProvider;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<StartupManager>()
                .As<IStartupManager>();


            var exchangeName = _settings.MatchingEngine.RabbitMq.ExchangeOrderbook;
            var rabbitSettings = new RabbitMqSubscriptionSettings
            {
                ConnectionString = _settings.MatchingEngine.RabbitMq.ConnectionString,
                QueueName = $"{exchangeName}.{_settings.MatchingEngine.RabbitMq.QueueName}",
                ExchangeName = exchangeName
            };

            builder.Register(x => new RestClient()).As<IRestClient>();
            builder.RegisterType<OrderBookInitializer>()
                .WithParameter(TypedParameter.From(_settings.MatchingEngine))
                .WithParameter(TypedParameter.From(_settings.CacheSettings))
                .As<IOrderBookInitializer>();

            builder.RegisterInstance(rabbitSettings);
            builder.RegisterType<OrderBookReader>()
                .As<IOrderBookReader>()
                .SingleInstance();

            builder.RegisterType<OrderBooksHandler>()
                .WithParameter(TypedParameter.From(_settings.CacheSettings))
                .As<IOrderBooksHandler>()
                .SingleInstance();

            builder.RegisterType<OrderBooksProvider>()
                .WithParameter(TypedParameter.From(_settings.CacheSettings))
                .As<IOrderBooksProvider>()
                .SingleInstance();

            RegisterPeriodicalHandlers(builder);
            RegisterRedis(builder);
        }

        private void RegisterPeriodicalHandlers(ContainerBuilder builder)
        {
            builder.RegisterType<OrderbookFlowChecker>()
                .WithParameter(TypedParameter.From(TimeSpan.FromMinutes(1)))
                .As<IStartable>()
                .AutoActivate()
                .SingleInstance();

            builder.RegisterType<ForcedOrderbookUpdater>()
                .WithParameter(TypedParameter.From(_settings.CacheSettings.ForceUpdateInterval))
                .As<IStartable>()
                .AutoActivate()
                .SingleInstance();
        }

        private void RegisterRedis(ContainerBuilder builder)
        {
            System.Threading.ThreadPool.SetMinThreads(100, 100);
            var options = ConfigurationOptions.Parse(_settings.CacheSettings.RedisConfiguration);
            options.ReconnectRetryPolicy = new ExponentialRetry(3000, 15000);
            options.ClientName = "Lykke.Job.OrderBooksCacheProvider";

            var redis = ConnectionMultiplexer.Connect(options);

            builder.RegisterInstance(redis).SingleInstance();
        }
    }
}
