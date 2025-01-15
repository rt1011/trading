#region Using declarations
using System;
using System.ComponentModel;
using System.Xml.Serialization;
using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

namespace NinjaTrader.NinjaScript.Indicators
{
    public class SuperTrend : Indicator
    {
        private Series<double> superTrend;
        private Series<bool> upTrend;
        private Series<int> trendSignal; // Buy/Sell signals
        private ATR atr;

        // Parameters
        [NinjaScriptProperty]
        [Description("Number of periods for the ATR calculation.")]
        public int ATRPeriod { get; set; } = 14;

        [NinjaScriptProperty]
        [Description("Multiplier for the ATR value.")]
        public double Multiplier { get; set; } = 2.0;

        [XmlIgnore]
        [Browsable(false)]
        public System.Windows.Media.Brush UpTrendColor { get; set; } = System.Windows.Media.Brushes.Green;

        [XmlIgnore]
        [Browsable(false)]
        public System.Windows.Media.Brush DownTrendColor { get; set; } = System.Windows.Media.Brushes.Red;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = "SuperTrend indicator with buy/sell signals.";
                Name = "SuperTrend";
                IsOverlay = true;
                AddPlot(System.Windows.Media.Brushes.DodgerBlue, "SuperTrend");
            }
            else if (State == State.DataLoaded)
            {
                atr = ATR(ATRPeriod);
                superTrend = new Series<double>(this);
                upTrend = new Series<bool>(this);
                trendSignal = new Series<int>(this);
            }
        }

        protected override void OnBarUpdate()
        {
            // Optimize for historical data
            if (State == State.Historical)
            {
                if (CurrentBar < ATRPeriod)
                {
                    InitializeTrend();
                    return;
                }
            }

            double atrValue = atr[0] * Multiplier;
            double midPoint = (High[0] + Low[0]) / 2;

            if (Close[0] > superTrend[1])
            {
                superTrend[0] = Math.Max(midPoint - atrValue, superTrend[1]);
                upTrend[0] = true;
                trendSignal[0] = !upTrend[1] ? 1 : 0; // Buy signal if trend changes to up
            }
            else if (Close[0] < superTrend[1])
            {
                superTrend[0] = Math.Min(midPoint + atrValue, superTrend[1]);
                upTrend[0] = false;
                trendSignal[0] = upTrend[1] ? -1 : 0; // Sell signal if trend changes to down
            }
            else
            {
                superTrend[0] = superTrend[1];
                upTrend[0] = upTrend[1];
                trendSignal[0] = 0;
            }

            // Set plot color dynamically
            PlotBrushes[0][0] = upTrend[0] ? UpTrendColor : DownTrendColor;
            Values[0][0] = superTrend[0];

            // Optimize drawing for real-time data only
            if (State == State.Realtime)
            {
                if (trendSignal[0] == 1) // Buy signal
                {
                    Draw.ArrowUp(this, $"BuySignal_{CurrentBar}", false, 0, Low[0] - TickSize, UpTrendColor);
                    Draw.Text(this, $"BuyPrice_{CurrentBar}", string.Format("{0:F2}", Close[0]), 0, Low[0] - 32 * TickSize, UpTrendColor); // Text above arrow
				    PlaySound("Buy.wav"); // Play sound for buy signal
                }
                else if (trendSignal[0] == -1) // Sell signal
                {
                    Draw.ArrowDown(this, $"SellSignal_{CurrentBar}", false, 0, High[0] + TickSize, DownTrendColor);				
                    Draw.Text(this, $"SellPrice_{CurrentBar}", string.Format("{0:F2}", Close[0]), 0, High[0] + 32 * TickSize, DownTrendColor); // Text below arrow
					PlaySound("Sell.wav"); // Play sound for sell signal
                }
            }
        }

        private void InitializeTrend()
        {
            superTrend[0] = Close[0];
            upTrend[0] = true;
            trendSignal[0] = 0;
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> SuperTrendLine => superTrend;

        [Browsable(false)]
        [XmlIgnore]
        public Series<bool> UpTrend => upTrend;

        [Browsable(false)]
        [XmlIgnore]
        public Series<int> TrendSignal => trendSignal; // Expose buy/sell signals
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private SuperTrend[] cacheSuperTrend;
		public SuperTrend SuperTrend(int aTRPeriod, double multiplier)
		{
			return SuperTrend(Input, aTRPeriod, multiplier);
		}

		public SuperTrend SuperTrend(ISeries<double> input, int aTRPeriod, double multiplier)
		{
			if (cacheSuperTrend != null)
				for (int idx = 0; idx < cacheSuperTrend.Length; idx++)
					if (cacheSuperTrend[idx] != null && cacheSuperTrend[idx].ATRPeriod == aTRPeriod && cacheSuperTrend[idx].Multiplier == multiplier && cacheSuperTrend[idx].EqualsInput(input))
						return cacheSuperTrend[idx];
			return CacheIndicator<SuperTrend>(new SuperTrend(){ ATRPeriod = aTRPeriod, Multiplier = multiplier }, input, ref cacheSuperTrend);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.SuperTrend SuperTrend(int aTRPeriod, double multiplier)
		{
			return indicator.SuperTrend(Input, aTRPeriod, multiplier);
		}

		public Indicators.SuperTrend SuperTrend(ISeries<double> input , int aTRPeriod, double multiplier)
		{
			return indicator.SuperTrend(input, aTRPeriod, multiplier);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.SuperTrend SuperTrend(int aTRPeriod, double multiplier)
		{
			return indicator.SuperTrend(Input, aTRPeriod, multiplier);
		}

		public Indicators.SuperTrend SuperTrend(ISeries<double> input , int aTRPeriod, double multiplier)
		{
			return indicator.SuperTrend(input, aTRPeriod, multiplier);
		}
	}
}

#endregion
