using System;
using Autofac;
using AzureStorage.Tables;
using Common;
using Common.Log;
using Core;
using Lykke.Logs;
using Lykke.SettingsReader;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Redis;
using Microsoft.Extensions.DependencyInjection;
using Services;

namespace Lykke.Service.OrderBooksCacheProvider.Binders
{
    public class JobModule
    {
        public ContainerBuilder Bind(IReloadingManager<AppSettings> settings)
        {
            var logToTable = CreateLogWithSlack(null, settings);
            var ioc = new ContainerBuilder();
            InitContainer(ioc, settings.CurrentValue.OrderBooksCacheProvider, logToTable);
            return ioc;
        }

        private void InitContainer(ContainerBuilder builder, OrderBooksCacheProviderSettings settings, ILog log)
        {
            log.WriteInfoAsync("MeSocketClients", "App start", null, $"BaseSettings : {settings.ToJson()}").Wait();

            builder.RegisterInstance(log);
            builder.RegisterInstance(settings);

            builder.BindServices(settings);

            var redis = new RedisCache(new RedisCacheOptions
            {
                Configuration = settings.CacheSettings.RedisConfiguration,
                InstanceName = settings.CacheSettings.FinanceDataCacheInstance
            });

            builder.RegisterInstance(redis).As<IDistributedCache>();
        }

        private static ILog CreateLogWithSlack(IServiceCollection services, IReloadingManager<AppSettings> settings)
        {
            var consoleLogger = new LogToConsole();
            var aggregateLogger = new AggregateLogger();

            aggregateLogger.AddLog(consoleLogger);

            var dbLogConnectionStringManager = settings.Nested(x => x.OrderBooksCacheProvider.Db.LogsConnString);
            var dbLogConnectionString = dbLogConnectionStringManager.CurrentValue;

            if (string.IsNullOrEmpty(dbLogConnectionString))
            {
                consoleLogger.WriteWarningAsync(nameof(JobModule), nameof(CreateLogWithSlack), "Table logger is not initialized").Wait();
                return aggregateLogger;
            }

            if (dbLogConnectionString.StartsWith("${") && dbLogConnectionString.EndsWith("}"))
                throw new InvalidOperationException($"LogsConnString {dbLogConnectionString} is not filled in settings");

            var persistenceManager = new LykkeLogToAzureStoragePersistenceManager(
                AzureTableStorage<LogEntity>.Create(dbLogConnectionStringManager, "LogMeSocketClients", consoleLogger),
                consoleLogger);

            //// Creating slack notification service, which logs own azure queue processing messages to aggregate log
            //var slackService = services.UseSlackNotificationsSenderViaAzureQueue(new AzureQueueIntegration.AzureQueueSettings
            //{
            //    ConnectionString = settings.CurrentValue.SlackNotifications.AzureQueue.ConnectionString,
            //    QueueName = settings.CurrentValue.SlackNotifications.AzureQueue.QueueName
            //}, aggregateLogger);

            //var slackNotificationsManager = new LykkeLogToAzureSlackNotificationsManager(slackService, consoleLogger);

            // Creating azure storage logger, which logs own messages to console log
            var azureStorageLogger = new LykkeLogToAzureStorage(
                persistenceManager,
                //slackNotificationsManager,
                null,
                consoleLogger);

            azureStorageLogger.Start();

            aggregateLogger.AddLog(azureStorageLogger);

            return aggregateLogger;
        }
    }
}
