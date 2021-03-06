﻿using System.Threading.Tasks;
using Lykke.Job.OrderBooksCacheProvider.Core.Domain;

namespace Lykke.Job.OrderBooksCacheProvider.Core.Services
{
    public interface IOrderBooksHandler
    {
        Task HandleOrderBook(OrderBook orderBook);
    }
}