using System;

namespace Lykke.Job.OrderBooksCacheProvider.Core.Services
{
    public interface IOrderBookReader
    {
        void StartRead();
        DateTime LastReceivedTimeStamp { get; }
    }
}
