using Common;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Job.OrderBooksCacheProvider.Core.Services;
using System;
using System.Threading.Tasks;

namespace Lykke.Job.OrderBooksCacheProvider.PeriodicalHandlers
{
    internal class ForcedOrderbookUpdater : TimerPeriod
    {
        private static bool _isFirstRun = true;

        private readonly IOrderBookInitializer _orderBookInitializer;

        public ForcedOrderbookUpdater(
            [NotNull] IOrderBookInitializer orderBookInitializer,
            TimeSpan checkInterval,
            ILogFactory logFactory)
            : base(checkInterval, logFactory, nameof(ForcedOrderbookUpdater))
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
