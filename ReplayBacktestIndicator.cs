using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using TradingPlatform.BusinessLayer;

namespace MBO_Market_Data_Analytics
{
    /// <summary>
    /// On-chart launcher for the trade-replay backtester. On init it pulls historical trade ticks for
    /// the chart symbol, replays them through the same calculator + adaptive controller the live
    /// strategy uses, logs a performance report, optionally exports a trades CSV, and draws a summary
    /// panel. It places no orders.
    ///
    /// Because it reuses <see cref="ReplayBacktester"/> (which reuses the live engine), tuning the
    /// adaptive percentile / ATR-multiplier inputs here reflects how the strategy would have behaved.
    /// </summary>
    public class ReplayBacktestIndicator : Indicator
    {
        [InputParameter("Lookback (Days)", 0, minimum: 1, maximum: 10, increment: 1, decimalPlaces: 0)]
        public int InputLookbackDays = 2;

        [InputParameter("Parameter Mode", 1, variants: new object[] {
            "Adaptive (self-tuning)", StrategyParameterMode.Adaptive,
            "Static (fixed inputs)", StrategyParameterMode.Static
        })]
        public StrategyParameterMode InputParameterMode = StrategyParameterMode.Adaptive;

        [InputParameter("Order Quantity", 2, minimum: 1, maximum: 100, increment: 1)]
        public double InputOrderQty = 1;

        [InputParameter("Cooldown (Seconds)", 3, minimum: 0, maximum: 300, increment: 1, decimalPlaces: 0)]
        public int InputCooldownSeconds = 5;

        // Static-mode parameters
        [InputParameter("Static Buy Threshold", 4, minimum: 1.0, maximum: 10.0, increment: 0.1)]
        public double InputBuyThreshold = 1.5;

        [InputParameter("Static Sell Threshold", 5, minimum: 0.1, maximum: 1.0, increment: 0.1)]
        public double InputSellThreshold = 0.67;

        [InputParameter("Static Take Profit (Ticks)", 6, minimum: 0, maximum: 1000, increment: 1, decimalPlaces: 0)]
        public int InputTakeProfitTicks = 10;

        [InputParameter("Static Stop Loss (Ticks)", 7, minimum: 0, maximum: 1000, increment: 1, decimalPlaces: 0)]
        public int InputStopLossTicks = 10;

        // Adaptive-mode parameters
        [InputParameter("Adaptive Entry Percentile", 8, minimum: 0.55, maximum: 0.99, increment: 0.01, decimalPlaces: 2)]
        public double InputEntryPercentile = 0.85;

        [InputParameter("Adaptive ATR Period (Minutes)", 9, minimum: 2, maximum: 100, increment: 1, decimalPlaces: 0)]
        public int InputAtrPeriod = 14;

        [InputParameter("Adaptive TP ATR Multiplier", 10, minimum: 0.1, maximum: 10.0, increment: 0.1, decimalPlaces: 1)]
        public double InputTpAtrMultiplier = 1.0;

        [InputParameter("Adaptive SL ATR Multiplier", 11, minimum: 0.1, maximum: 10.0, increment: 0.1, decimalPlaces: 1)]
        public double InputSlAtrMultiplier = 1.0;

        // Cost model
        [InputParameter("Commission per Contract ($, per side)", 12, minimum: 0, maximum: 100, increment: 0.05, decimalPlaces: 2)]
        public double InputCommissionPerContract = 0.0;

        [InputParameter("Slippage (Ticks)", 13, minimum: 0, maximum: 50, increment: 1, decimalPlaces: 0)]
        public int InputSlippageTicks = 0;

        [InputParameter("Infer Aggressor (Tick Rule) if Unknown", 14)]
        public bool InputInferAggressor = true;

        [InputParameter("Export Trades CSV", 15)]
        public bool InputExportCsv = false;

        [InputParameter("Trades CSV Path", 16)]
        public string InputCsvPath = "C:\\Quantower\\Settings\\Scripts\\ScriptsData\\backtest_trades.csv";

        private volatile BacktestResult? result;
        private volatile string status = "Initializing…";

        private readonly Color bg = Color.FromArgb(220, 10, 15, 26);
        private readonly Color border = Color.FromArgb(255, 43, 58, 82);
        private readonly Color textPrimary = Color.FromArgb(255, 230, 235, 245);
        private readonly Color accentGreen = Color.FromArgb(255, 34, 197, 94);
        private readonly Color accentRed = Color.FromArgb(255, 239, 68, 68);

        public ReplayBacktestIndicator() : base()
        {
            Name = "MBP Replay Backtester";
            Description = "Replays historical trades through the analytics engine and adaptive strategy logic to produce a performance report.";
            SeparateWindow = false;
        }

        protected override void OnInit()
        {
            base.OnInit();
            status = "Loading history…";

            List<BacktestTrade> trades;
            try
            {
                trades = LoadHistory();
            }
            catch (Exception ex)
            {
                status = "History load failed: " + ex.Message;
                Core.Instance.Loggers.Log($"[Backtest] History load failed: {ex.Message}", LoggingLevel.Error);
                return;
            }

            if (trades.Count == 0)
            {
                status = "No historical trade data for the requested range (tick history may be unavailable for this feed).";
                Core.Instance.Loggers.Log("[Backtest] No historical trade data returned.", LoggingLevel.System);
                return;
            }

            var cfg = BuildConfig();
            status = $"Replaying {trades.Count:N0} ticks…";

            // Run the (potentially heavy) replay off the platform thread.
            Task.Run(() =>
            {
                try
                {
                    var bt = new ReplayBacktester(this.Symbol, cfg);
                    var r = bt.Run(trades);
                    result = r;
                    status = r.Ran ? "Done." : (r.Message ?? "No result.");

                    Core.Instance.Loggers.Log("[Backtest] " + r.Summary().Replace(Environment.NewLine, " | "), LoggingLevel.System);

                    if (r.Ran && InputExportCsv && !string.IsNullOrEmpty(InputCsvPath))
                    {
                        try
                        {
                            ReplayBacktester.WriteTradesCsv(InputCsvPath, r);
                            Core.Instance.Loggers.Log($"[Backtest] Trades CSV written: {InputCsvPath}", LoggingLevel.System);
                        }
                        catch (Exception ex)
                        {
                            Core.Instance.Loggers.Log($"[Backtest] CSV export failed: {ex.Message}", LoggingLevel.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    status = "Replay failed: " + ex.Message;
                    Core.Instance.Loggers.Log($"[Backtest] Replay failed: {ex.Message}", LoggingLevel.Error);
                }
            });
        }

        protected override void OnUpdate(UpdateArgs args) { }

        private List<BacktestTrade> LoadHistory()
        {
            var list = new List<BacktestTrade>();
            DateTime to = Core.Instance.TimeUtils.DateTimeUtcNow;
            DateTime from = to.AddDays(-InputLookbackDays);

            var hist = this.Symbol.GetHistory(new HistoryRequestParameters
            {
                Symbol = this.Symbol,
                Aggregation = new HistoryAggregationTick(HistoryType.Last),
                FromTime = from,
                ToTime = to
            });

            if (hist == null)
                return list;

            for (int i = 0; i < hist.Count; i++)
            {
                if (hist[i] is HistoryItemLast last && last.Price > 0 && last.Volume > 0)
                    list.Add(new BacktestTrade(last.TimeLeft, last.Price, last.Volume, last.AggressorFlag));
            }
            return list;
        }

        private BacktestConfig BuildConfig()
        {
            double price = this.Symbol.Last;
            if (price <= 0) price = this.Symbol.Bid > 0 ? this.Symbol.Bid : 1.0;
            double tickSize = this.Symbol.TickSize > 0 ? this.Symbol.TickSize : 1.0;
            double tickCost = this.Symbol.GetTickCost(price);
            double pointValue = tickCost > 0 ? tickCost / tickSize : 1.0;

            double upper = Math.Clamp(InputEntryPercentile, 0.55, 0.99);

            return new BacktestConfig
            {
                TickSize = tickSize,
                PointValue = pointValue,
                OrderQty = InputOrderQty,
                Mode = InputParameterMode,
                BuyThreshold = InputBuyThreshold,
                SellThreshold = InputSellThreshold,
                BuyReset = 1.1,
                SellReset = 0.9,
                TakeProfitTicks = InputTakeProfitTicks,
                StopLossTicks = InputStopLossTicks,
                Adaptive = new AdaptiveParameterConfig
                {
                    EntryUpperPercentile = upper,
                    EntryLowerPercentile = 1.0 - upper,
                    AtrPeriod = InputAtrPeriod,
                    TpAtrMultiplier = InputTpAtrMultiplier,
                    SlAtrMultiplier = InputSlAtrMultiplier
                },
                CooldownSeconds = InputCooldownSeconds,
                CommissionPerContract = InputCommissionPerContract,
                SlippageTicks = InputSlippageTicks,
                InferAggressorIfUnknown = InputInferAggressor
            };
        }

        public override void OnPaintChart(PaintChartEventArgs args)
        {
            base.OnPaintChart(args);
            Graphics g = args.Graphics;

            int x = 20, y = 50, w = 360;
            var r = result;

            var lines = new List<(string text, Color color)>();
            lines.Add(("MBP REPLAY BACKTESTER", textPrimary));
            if (r == null || !r.Ran)
            {
                lines.Add((status, textPrimary));
            }
            else
            {
                Color pnlColor = r.NetPnL >= 0 ? accentGreen : accentRed;
                lines.Add(($"{this.Symbol.Name}  |  {InputParameterMode}  |  {InputLookbackDays}d", textPrimary));
                lines.Add(($"Ticks: {r.InputTrades:N0}   Aggressor: {r.AggressorCoverage:P0}", textPrimary));
                lines.Add(($"Trades: {r.TotalTrades}   Win%: {r.WinRate:P1} ({r.Wins}W/{r.Losses}L)", textPrimary));
                lines.Add(($"Net PnL: {r.NetPnL:C2}", pnlColor));
                lines.Add(($"Profit factor: {(double.IsInfinity(r.ProfitFactor) ? "∞" : r.ProfitFactor.ToString("F2"))}   Exp/trade: {r.Expectancy:C2}", textPrimary));
                lines.Add(($"Avg win: {r.AvgWin:C2}   Avg loss: {r.AvgLoss:C2}", textPrimary));
                lines.Add(($"Max DD: {r.MaxDrawdown:C2}   Max consec L: {r.MaxConsecutiveLosses}", textPrimary));
                lines.Add(($"Commission: {r.Commission:C2}", textPrimary));
            }

            int h = 14 + lines.Count * 18 + 10;
            using (var b = new SolidBrush(bg)) g.FillRectangle(b, x, y, w, h);
            using (var p = new Pen(border, 1.5f)) g.DrawRectangle(p, x, y, w, h);

            using (var fontTitle = new Font("Segoe UI", 9, FontStyle.Bold))
            using (var font = new Font("Segoe UI", 8, FontStyle.Regular))
            {
                int ty = y + 8;
                for (int i = 0; i < lines.Count; i++)
                {
                    using var brush = new SolidBrush(lines[i].color);
                    g.DrawString(lines[i].text, i == 0 ? fontTitle : font, brush, x + 10, ty);
                    ty += 18;
                }
            }
        }

        protected override void OnClear()
        {
            result = null;
            base.OnClear();
        }
    }
}
