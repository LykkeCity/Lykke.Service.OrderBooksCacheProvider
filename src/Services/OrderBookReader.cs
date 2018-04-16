﻿using System;
using System.Threading.Tasks;
using Common.Log;
using Core.Domain;
using Core.Services;
using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Subscriber;

namespace Services
{
    public class OrderBookReader : IOrderBookReader
    {
        private readonly IOrderBooksHandler _orderBooksHandler;
        private readonly RabbitMqSubscriber<OrderBook> _subscriber;

        public OrderBookReader(RabbitMqSubscriptionSettings settings,
            IOrderBooksHandler orderBooksHandler,
            ILog log)
        {
            _orderBooksHandler = orderBooksHandler;

            _subscriber =
                new RabbitMqSubscriber<OrderBook>(settings,
                        new ResilientErrorHandlingStrategy(log, settings, TimeSpan.FromSeconds(10),
                            next: new DefaultErrorHandlingStrategy(log, settings)))
                    .SetMessageDeserializer(new JsonMessageDeserializer<OrderBook>())
                    .SetMessageReadStrategy(new MessageReadWithTemporaryQueueStrategy())
                    .Subscribe(HandleData)
                    .SetLogger(log);
        }

        public void StartRead()
        {
            _subscriber.Start();
        }

        private async Task HandleData(OrderBook orderBook)
        {
            await _orderBooksHandler.HandleOrderBook(orderBook);
        }
    }
}
