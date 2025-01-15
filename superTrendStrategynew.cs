#region Using declarations
using System;
using NinjaTrader.Cbi;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.Indicators;
using System.ComponentModel.DataAnnotations;
#endregion

namespace NinjaTrader.NinjaScript.Strategies
{
    public class SuperTrendStrategyNew : Strategy
    {
        // User-modifiable inputs
        [NinjaScriptProperty]
        [Display(Name = "ATR Period", GroupName = "SuperTrend Parameters", Order = 1)]
        public int ATRPeriod { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Multiplier", GroupName = "SuperTrend Parameters", Order = 2)]
        public double Multiplier { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Profit Target (Points)", GroupName = "Trade Parameters", Order = 1)]
        public int ProfitTarget { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Stop Loss (Points)", GroupName = "Trade Parameters", Order = 2)]
        public int StopLoss { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Enable Time Filter", GroupName = "Filters", Order = 1)]
        public bool EnableTimeFilter { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Start Time (HHmm)", GroupName = "Filters", Order = 2)]
        public int StartTime { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "End Time (HHmm)", GroupName = "Filters", Order = 3)]
        public int EndTime { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Enable Volatility Filter", GroupName = "Filters", Order = 4)]
        public bool EnableVolatilityFilter { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Volatility Threshold (ATR %)", GroupName = "Filters", Order = 5)]
        public double VolatilityThreshold { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Enable Trailing Stop", GroupName = "Trade Parameters", Order = 3)]
        public bool EnableTrailingStop { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Trailing Stop Distance (ATR Multiplier)", GroupName = "Trade Parameters", Order = 4)]
        public double TrailingStopDistance { get; set; }

        private NinjaTrader.NinjaScript.Indicators.SuperTrend superTrendIndicator;
        private ATR atr;
        private bool isPositionClosed;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = "SuperTrend strategy with additional dynamic features and debugging.";
                Name = "SuperTrendStrategyNew";
                Calculate = Calculate.OnBarClose;
                EntriesPerDirection = 1;
                EntryHandling = EntryHandling.AllEntries;
                IsExitOnSessionCloseStrategy = true;
                ExitOnSessionCloseSeconds = 30;

                ATRPeriod = 14;
                Multiplier = 2.0;
                ProfitTarget = 100;
                StopLoss = 50;

                EnableTimeFilter = true;
                StartTime = 93000; // Default: 9:30 AM
                EndTime = 160000; // Default: 4:00 PM

                EnableVolatilityFilter = true;
                VolatilityThreshold = 0.001; // 0.1% of the current price

                EnableTrailingStop = true;
                TrailingStopDistance = 2.0; // Default multiplier for trailing stop
                isPositionClosed = true; // Initialize position state
            }
            else if (State == State.Configure)
            {
                if (!EnableTrailingStop)
                {
                    SetProfitTarget(CalculationMode.Ticks, ProfitTarget * 4); // Static Profit Target
                    SetStopLoss(CalculationMode.Ticks, StopLoss * 4); // Static Stop Loss
                }
            }
            else if (State == State.DataLoaded)
            {
                superTrendIndicator = SuperTrend(ATRPeriod, Multiplier);
                AddChartIndicator(superTrendIndicator);

                atr = ATR(ATRPeriod); // Add ATR indicator for dynamic filtering and trade management
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < ATRPeriod)
                return;

            // Debugging: Print Bar Information
            Print($"Bar {CurrentBar} - Time: {Time[0]} - Close: {Close[0]}");

            // Time Filter
            if (EnableTimeFilter)
            {
                int currentTime = ToTime(Time[0]);
                if (currentTime < StartTime || currentTime > EndTime)
                {
                    Print($"Skipping bar due to time filter: {currentTime}");
                    return;
                }
            }

            // Volatility Filter
            if (EnableVolatilityFilter)
            {
                double threshold = Close[0] * VolatilityThreshold;
                if (atr[0] < threshold)
                {
                    Print($"Skipping bar due to volatility filter: ATR={atr[0]}, Threshold={threshold}");
                    return;
                }
            }

            // Use the trend signal from the SuperTrend indicator
            int trendSignal = superTrendIndicator.TrendSignal[0];
            Print($"TrendSignal: {trendSignal} at {Time[0]}");

            // Check Market Position
            Print($"Market Position: {Position.MarketPosition}, IsPositionClosed: {isPositionClosed}");

            // Buy signal
            if (trendSignal == 1 && isPositionClosed)
            {
                EnterLong("Buy");
                isPositionClosed = false;
                Print($"Entered Long at {Close[0]}");

                if (EnableTrailingStop)
                {
                    double trailingDistance = atr[0] * TrailingStopDistance;
                    SetTrailStop(CalculationMode.Price, trailingDistance);
                    Print($"Setting trailing stop: {trailingDistance}");
                }
                else
                {
                    double profitTarget = Close[0] + (atr[0] * Multiplier);
                    double stopLoss = Close[0] - (atr[0] * Multiplier);
                    SetProfitTarget(CalculationMode.Price, profitTarget);
                    SetStopLoss(CalculationMode.Price, stopLoss);
                    Print($"Setting profit target: {profitTarget}, stop loss: {stopLoss}");
                }
            }

            // Sell signal
            if (trendSignal == -1 && !isPositionClosed)
            {
                ExitLong("Sell", "Buy");
                isPositionClosed = true;
                Print($"Exited Long at {Close[0]}");
            }
        }
    }
}
