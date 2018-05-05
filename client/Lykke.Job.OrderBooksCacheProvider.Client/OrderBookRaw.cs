using System;
using System.Collections.Generic;

namespace Lykke.Job.OrderBooksCacheProvider.Client
{
    public class OrderBookRaw
    {
        public string AssetPair { get; set; }
        public bool IsBuy { get; set; }
        public DateTime Timestamp { get; set; }
        public List<VolumePrice> Prices { get; set; } = new List<VolumePrice>();
    }
}