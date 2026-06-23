using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using TradingPlatform.BusinessLayer;

namespace MBO_Market_Data_Analytics
{
    /// <summary>A single historical trade event fed to the replay engine.</summary>
    public readonly struct BacktestTrade
    {
        public readonly DateTime Time;
        public readonly double Price;
        public readonly double Size;
        public readonly AggressorFlag Aggressor;

        public BacktestTrade(DateTime time, double price, double size, AggressorFlag aggressor)
        {
            Time = time;
            Price = price;
            Size = size;
            Aggressor = aggressor;
        }
    }

    public enum BacktestExitReason { TakeProfit, StopLoss, EndOfData }

    public sealed class BacktestConfig
    {
        public double TickSize = 0.25;
        public double PointValue = 20.0;     // currency per 1.0 price move, per contract
        public double OrderQty = 1.0;

        public StrategyParameterMode Mode = StrategyParameterMode.Adaptive;

        // Static-mode parameters
        public double BuyThreshold = 1.5;
        public double SellThreshold = 0.67;
        public double BuyReset = 1.1;
        public double SellReset = 0.9;
        public int TakeProfitTicks = 10;
        public int StopLossTicks = 10;

        // Adaptive-mode parameters
        public AdaptiveParameterConfig Adaptive = new AdaptiveParameterConfig();

        public int CooldownSeconds = 5;

        // Cost model
        public double CommissionPerContract = 0.0; // charged per side (entry + exit)
        public double SlippageTicks = 0.0;         // adverse ticks on entry and on stop-loss exits

        public bool InferAggressorIfUnknown = true;

        public double TradeVolumeWindowShort = 1000;
        public double TradeVolumeWindowLong = 5000;
    }

    public sealed class BacktestTradeRecord
    {
        public DateTime EntryTime { get; init; }
        public DateTime ExitTime { get; init; }
        public Side Side { get; init; }
        public double EntryPrice { get; init; }
        public double ExitPrice { get; init; }
        public double Qty { get; init; }
        public double NetPnL { get; init; }
        public BacktestExitReason Reason { get; init; }
    }

    public sealed class BacktestResult
    {
        public bool Ran;
        public string? Message;
        public int InputTrades;

        public DateTime FirstEvent;
        public DateTime LastEvent;
        public double AggressorCoverage;

        public int TotalTrades;
        public int Wins;
        public int Losses;
        public double WinRate;

        public double GrossPnL;
        public double Commission;
        public double NetPnL;
        public double ProfitFactor;
        public double AvgWin;
        public double AvgLoss;
        public double Expectancy;
        public double MaxDrawdown;
        public int MaxConsecutiveLosses;

        public readonly List<BacktestTradeRecord> Trades = new List<BacktestTradeRecord>();
        public readonly List<(DateTime time, double equity)> Equity = new List<(DateTime, double)>();

        public string Summary()
        {
            if (!Ran)
                return $"Backtest did not run: {Message}";

            var sb = new StringBuilder();
            sb.AppendLine($"Replay backtest: {InputTrades:N0} ticks, {FirstEvent:yyyy-MM-dd HH:mm} → {LastEvent:yyyy-MM-dd HH:mm} UTC");
            sb.AppendLine($"Aggressor coverage: {AggressorCoverage:P1}");
            sb.AppendLine($"Trades: {TotalTrades}  Win%: {WinRate:P1}  ({Wins}W / {Losses}L)");
            sb.AppendLine($"Net PnL: {NetPnL:C2}  Gross: {GrossPnL:C2}  Commission: {Commission:C2}");
            sb.AppendLine($"Profit factor: {(double.IsInfinity(ProfitFactor) ? "∞" : ProfitFactor.ToString("F2"))}  Expectancy/trade: {Expectancy:C2}");
            sb.AppendLine($"Avg win: {AvgWin:C2}  Avg loss: {AvgLoss:C2}");
            sb.AppendLine($"Max drawdown: {MaxDrawdown:C2}  Max consec losses: {MaxConsecutiveLosses}");
            return sb.ToString();
        }
    }

    /// <summary>
    /// Deterministic, trade-driven replay backtester. Reuses the live
    /// <see cref="DataAnalyticsCalculator"/> and <see cref="AdaptiveParameterController"/> so the
    /// simulated signal and adaptive parameters match the running strategy.
    ///
    /// Scope / honest limitations:
    /// - Trade-only (historical Level 2 is not generally available), so order-book metrics are not
    ///   exercised; the signal is the trade-flow buyer/seller volume ratio, which is what the
    ///   strategy trades on.
    /// - Models a single bracketed position at a time (no pyramiding). When both TP and SL fall inside
    ///   the same trade, the stop is assumed to fill first (conservative).
    /// - Entry fills at the signal trade price (+ slippage); stop exits fill at the stop price
    ///   (+ slippage); take-profit fills at the limit price.
    /// </summary>
    public sealed class ReplayBacktester
    {
        private readonly Symbol symbol;
        private readonly BacktestConfig cfg;

        public ReplayBacktester(Symbol symbol, BacktestConfig cfg)
        {
            this.symbol = symbol ?? throw new ArgumentNullException(nameof(symbol));
            this.cfg = cfg ?? throw new ArgumentNullException(nameof(cfg));
        }

        public BacktestResult Run(IReadOnlyList<BacktestTrade> trades)
        {
            var result = new BacktestResult { InputTrades = trades?.Count ?? 0 };
            if (trades == null || trades.Count == 0)
            {
                result.Message = "No historical trades available for the requested range.";
                return result;
            }

            var calc = new DataAnalyticsCalculator(symbol, FeedCapabilities.Trades | FeedCapabilities.TradeAggressor)
            {
                TradeVolumeWindowShort = cfg.TradeVolumeWindowShort,
                TradeVolumeWindowLong = cfg.TradeVolumeWindowLong,
                IsManualMode = true,
                IsCalibrated = true // no L2 for MTR calibration in replay; bypass the gate
            };

            double tickSize = cfg.TickSize > 0 ? cfg.TickSize : 1.0;
            double slip = cfg.SlippageTicks * tickSize;

            AdaptiveParameterController? ctrl = cfg.Mode == StrategyParameterMode.Adaptive
                ? new AdaptiveParameterController(cfg.Adaptive, tickSize)
                : null;

            // Simulation state
            int pos = 0;            // 0 flat, +1 long, -1 short
            double entryPrice = 0, tpPrice = 0, slPrice = 0;
            DateTime entryTime = default;
            double qty = cfg.OrderQty;
            bool buyActive = false, sellActive = false;
            DateTime lastExitTime = DateTime.MinValue;

            double cum = 0, peak = 0, maxDD = 0;
            int consecLoss = 0, maxConsecLoss = 0;
            double grossProfitNet = 0, grossLossNet = 0;
            long withAgg = 0;

            result.FirstEvent = trades[0].Time;
            result.LastEvent = trades[trades.Count - 1].Time;

            double prevPrice = double.NaN;

            void Close(double exitRaw, DateTime time, BacktestExitReason reason)
            {
                double exitFill = exitRaw;
                if (reason == BacktestExitReason.StopLoss)
                    exitFill += pos > 0 ? -slip : slip; // adverse slippage on stops

                double gross = (exitFill - entryPrice) * pos * cfg.PointValue * qty;
                double comm = cfg.CommissionPerContract * qty * 2.0; // entry + exit
                double net = gross - comm;

                cum += net;
                peak = Math.Max(peak, cum);
                maxDD = Math.Max(maxDD, peak - cum);

                if (net >= 0) { grossProfitNet += net; consecLoss = 0; }
                else { grossLossNet += -net; consecLoss++; maxConsecLoss = Math.Max(maxConsecLoss, consecLoss); }

                result.Commission += comm;
                result.Trades.Add(new BacktestTradeRecord
                {
                    EntryTime = entryTime,
                    ExitTime = time,
                    Side = pos > 0 ? Side.Buy : Side.Sell,
                    EntryPrice = entryPrice,
                    ExitPrice = exitFill,
                    Qty = qty,
                    NetPnL = net,
                    Reason = reason
                });
                result.Equity.Add((time, cum));

                pos = 0;
                lastExitTime = time;
            }

            void Enter(int direction, BacktestTrade t, int tpTicks, int slTicks)
            {
                pos = direction;
                entryTime = t.Time;
                entryPrice = t.Price + (direction > 0 ? slip : -slip); // adverse entry slippage

                if (direction > 0)
                {
                    tpPrice = tpTicks > 0 ? entryPrice + tpTicks * tickSize : double.PositiveInfinity;
                    slPrice = slTicks > 0 ? entryPrice - slTicks * tickSize : double.NegativeInfinity;
                }
                else
                {
                    tpPrice = tpTicks > 0 ? entryPrice - tpTicks * tickSize : double.NegativeInfinity;
                    slPrice = slTicks > 0 ? entryPrice + slTicks * tickSize : double.PositiveInfinity;
                }
            }

            foreach (var raw in trades)
            {
                // Aggressor resolution (use feed flag, else tick rule)
                AggressorFlag agg = raw.Aggressor;
                if (agg != AggressorFlag.Buy && agg != AggressorFlag.Sell && cfg.InferAggressorIfUnknown && !double.IsNaN(prevPrice))
                {
                    if (raw.Price > prevPrice) agg = AggressorFlag.Buy;
                    else if (raw.Price < prevPrice) agg = AggressorFlag.Sell;
                }
                if (agg == AggressorFlag.Buy || agg == AggressorFlag.Sell) withAgg++;
                prevPrice = raw.Price;

                calc.ProcessTrade(raw.Time, raw.Price, raw.Size, agg);

                // 1. Exits (stop assumed first if both touched this trade)
                if (pos > 0)
                {
                    if (raw.Price <= slPrice) Close(slPrice, raw.Time, BacktestExitReason.StopLoss);
                    else if (raw.Price >= tpPrice) Close(tpPrice, raw.Time, BacktestExitReason.TakeProfit);
                }
                else if (pos < 0)
                {
                    if (raw.Price >= slPrice) Close(slPrice, raw.Time, BacktestExitReason.StopLoss);
                    else if (raw.Price <= tpPrice) Close(tpPrice, raw.Time, BacktestExitReason.TakeProfit);
                }

                // 2. Signal evaluation
                var ratioVal = calc.GetBuyerSellerQtyRatioShort();
                bool ratioUsable = ratioVal.IsWarm && ratioVal.Quality != MetricQuality.Unavailable;

                double buyTh = cfg.BuyThreshold, sellTh = cfg.SellThreshold, buyReset = cfg.BuyReset, sellReset = cfg.SellReset;
                int effTp = cfg.TakeProfitTicks, effSl = cfg.StopLossTicks;
                bool ready = true;

                if (ctrl != null)
                {
                    ctrl.Observe(raw.Price, true, ratioVal.Value, ratioUsable, raw.Time);
                    var ap = ctrl.Current;
                    if (ap == null || !ap.IsReady) ready = false;
                    else
                    {
                        buyTh = ap.BuyThreshold; sellTh = ap.SellThreshold;
                        buyReset = ap.BuyResetThreshold; sellReset = ap.SellResetThreshold;
                        effTp = ap.TakeProfitTicks; effSl = ap.StopLossTicks;
                    }
                }

                if (!ratioUsable || !ready)
                    continue;

                double ratio = ratioVal.Value;
                bool cool = (raw.Time - lastExitTime).TotalSeconds >= cfg.CooldownSeconds;

                if (ratio >= buyTh)
                {
                    if (!buyActive)
                    {
                        buyActive = true;
                        if (pos == 0 && cool && (effTp > 0 || effSl > 0)) Enter(+1, raw, effTp, effSl);
                    }
                }
                else if (ratio <= buyReset) buyActive = false;

                if (ratio <= sellTh)
                {
                    if (!sellActive)
                    {
                        sellActive = true;
                        if (pos == 0 && cool && (effTp > 0 || effSl > 0)) Enter(-1, raw, effTp, effSl);
                    }
                }
                else if (ratio >= sellReset) sellActive = false;
            }

            // Close any residual open position at the last price
            if (pos != 0)
            {
                var last = trades[trades.Count - 1];
                Close(last.Price, last.Time, BacktestExitReason.EndOfData);
            }

            // Aggregate
            result.Ran = true;
            result.AggressorCoverage = result.InputTrades > 0 ? (double)withAgg / result.InputTrades : 0;
            result.TotalTrades = result.Trades.Count;
            foreach (var tr in result.Trades)
            {
                if (tr.NetPnL > 0) result.Wins++;
                else if (tr.NetPnL < 0) result.Losses++;
            }
            result.WinRate = result.TotalTrades > 0 ? (double)result.Wins / result.TotalTrades : 0;
            result.NetPnL = cum;
            result.GrossPnL = cum + result.Commission; // Commission accumulated during Close()
            result.ProfitFactor = grossLossNet > 0 ? grossProfitNet / grossLossNet : (grossProfitNet > 0 ? double.PositiveInfinity : 0);
            result.AvgWin = result.Wins > 0 ? grossProfitNet / result.Wins : 0;
            result.AvgLoss = result.Losses > 0 ? grossLossNet / result.Losses : 0;
            result.Expectancy = result.TotalTrades > 0 ? cum / result.TotalTrades : 0;
            result.MaxDrawdown = maxDD;
            result.MaxConsecutiveLosses = maxConsecLoss;

            return result;
        }

        /// <summary>Writes the per-trade records to a CSV file (overwrites).</summary>
        public static void WriteTradesCsv(string path, BacktestResult result)
        {
            using var w = new System.IO.StreamWriter(path, false);
            w.WriteLine("EntryTimeUtc,ExitTimeUtc,Side,EntryPrice,ExitPrice,Qty,NetPnL,ExitReason,CumEquity");
            double cum = 0;
            var ci = CultureInfo.InvariantCulture;
            foreach (var t in result.Trades)
            {
                cum += t.NetPnL;
                w.WriteLine(string.Join(",",
                    t.EntryTime.ToString("o", ci),
                    t.ExitTime.ToString("o", ci),
                    t.Side,
                    t.EntryPrice.ToString("F4", ci),
                    t.ExitPrice.ToString("F4", ci),
                    t.Qty.ToString("F2", ci),
                    t.NetPnL.ToString("F2", ci),
                    t.Reason,
                    cum.ToString("F2", ci)));
            }
        }
    }
}
