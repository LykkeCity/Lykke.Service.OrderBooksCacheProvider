using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Job.OrderBooksCacheProvider.Core.Services;

namespace Lykke.Job.OrderBooksCacheProvider.PeriodicalHandlers
{
    internal class ForcedOrderbookUpdated : TimerPeriod
    {
        private static bool _isFirstRun = true;

        private readonly IOrderBookInitializer _orderBookInitializer;

        public ForcedOrderbookUpdated(
            [NotNull] IOrderBookInitializer orderBookInitializer,
            TimeSpan checkInterval,
            ILog log = null)
            : base(nameof(ForcedOrderbookUpdated), (int)checkInterval.TotalMilliseconds, log)
        {
            _orderBookInitializer = orderBookInitializer ?? throw new ArgumentNullException(nameof(orderBookInitializer));
        }

        public override async Task Execute()
        {
            if (_isFirstRun)
            {
                // do not update orderbook right after the service starts
                _isFirstRun = false;
                return;
            }

            await _orderBookInitializer.InitOrderBooks(false);
        }
    }
}
