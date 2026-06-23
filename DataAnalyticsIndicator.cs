using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using TradingPlatform.BusinessLayer;

namespace MBO_Market_Data_Analytics
{
    public enum CalibrationMode
    {
        AutoMTR,
        Manual
    }

    public enum MarketEventKind : byte
    {
        Trade,
        BookLevel,
        BookSnapshotBegin,
        BookSnapshotEnd,
        BookReset,
        SessionReset,
        ConnectionState
    }

    public enum MarketSide : byte
    {
        Unknown,
        Bid,
        Ask
    }

    public enum TradeSide : byte
    {
        Unknown,
        BuyAggressor,
        SellAggressor
    }

    public readonly struct MarketEvent
    {
        public readonly MarketEventKind Kind;
        public readonly double Price;
        public readonly double Size;
        public readonly string? Id;
        public readonly bool IsBid;
        public readonly AggressorFlag Aggressor;
        public readonly DateTime Time;
        public readonly bool Closed;
        public readonly long Priority;      // Level2Quote.Priority (MBO queue rank; 0 if unavailable)
        public readonly int NumberOrders;   // Level2Quote.NumberOrders (>1 means aggregated feed)

        public MarketEvent(DateTime time, double price, double size, AggressorFlag aggressor)
        {
            Kind = MarketEventKind.Trade;
            Time = time;
            Price = price;
            Size = size;
            Aggressor = aggressor;
            Id = null;
            IsBid = false;
            Closed = false;
            Priority = 0;
            NumberOrders = 0;
        }

        public MarketEvent(DateTime time, string? id, double price, double size, bool isBid, bool closed,
                           long priority = 0, int numberOrders = 0)
        {
            Kind = MarketEventKind.BookLevel;
            Time = time;
            Id = id;
            Price = price;
            Size = size;
            IsBid = isBid;
            Closed = closed;
            Aggressor = AggressorFlag.None;
            Priority = priority;
            NumberOrders = numberOrders;
        }
    }

    public sealed class AnalyticsSnapshot
    {
        public long Version { get; init; }
        public long ReceiveTicks { get; init; }
        public bool BookValid { get; init; }
        public bool IsCalibrated { get; init; }
        public bool IsMboActive { get; init; }
        public int OrderCountWindowShort { get; init; }
        public int OrderCountWindowLong { get; init; }
        public double TradeVolumeWindowShort { get; init; }
        public double TradeVolumeWindowLong { get; init; }

        // Category 1: Buyer/Seller
        public MetricValue BuyerTradeCountShort { get; init; }
        public MetricValue SellerTradeCountShort { get; init; }
        public MetricValue BuyerSellerTradeCountRatioShort { get; init; }
        public MetricValue BuyerSellerQtyRatioShort { get; init; }
        public MetricValue CumulativeBuyerSellerTradeCountRatio { get; init; }

        // 60s window
        public MetricValue BuyTradeCount60 { get; init; }
        public MetricValue SellTradeCount60 { get; init; }
        public MetricValue BuyVolume60 { get; init; }
        public MetricValue SellVolume60 { get; init; }
        public MetricValue BuySellCountRatio60 { get; init; }
        public MetricValue BuySellVolumeRatio60 { get; init; }

        // Cumulative
        public MetricValue CumBuyVolume { get; init; }
        public MetricValue CumSellVolume { get; init; }
        public MetricValue CumDelta { get; init; }
        public MetricValue CumTradeCount { get; init; }

        // Category 2: VWAP
        public MetricValue TradesVwapLong { get; init; }
        public MetricValue BuyerTradesVwapLong { get; init; }
        public MetricValue SellerTradesVwapLong { get; init; }
        public MetricValue CumulativeTradesVwap { get; init; }

        public MetricValue RollingVwap1m { get; init; }
        public MetricValue RollingVwap5m { get; init; }
        public MetricValue RollingVwap15m { get; init; }
        public MetricValue SessionVwap { get; init; }
        public MetricValue VwapDistance { get; init; }
        public MetricValue VwapDeviation { get; init; }
        public MetricValue BuyVwap { get; init; }
        public MetricValue SellVwap { get; init; }

        // Category 3: Arrivals
        public MetricValue BidL2AddEventCountShort { get; init; }
        public MetricValue BidL2AddedVisibleQtyShort { get; init; }
        public MetricValue AskL2AddEventCountShort { get; init; }
        public MetricValue AskL2AddedVisibleQtyShort { get; init; }
        public MetricValue CumulativeL2AddEventCount { get; init; }

        public MetricValue NewOrderCount { get; init; }
        public MetricValue NewBidCount { get; init; }
        public MetricValue NewAskCount { get; init; }
        public MetricValue NewBidVolume { get; init; }
        public MetricValue NewAskVolume { get; init; }

        // Category 4: Order Flow
        public MetricValue MeanBidAddedVisibleQtyLong { get; init; }
        public MetricValue MeanAskAddedVisibleQtyLong { get; init; }
        public MetricValue StdDevBidAddedVisibleQtyLong { get; init; }
        public MetricValue StdDevAskAddedVisibleQtyLong { get; init; }

        // Category 5: Cancellations
        public MetricValue L2RemoveEventCountShort { get; init; }
        public MetricValue EstimatedCancelQtyShort { get; init; }
        public MetricValue RemovedToAddedVisibleRatioCountShort { get; init; }
        public MetricValue RemovedToAddedVisibleRatioQtyShort { get; init; }
        public MetricValue CumulativeEstimatedCancelVwap { get; init; }

        public MetricValue CancelCount { get; init; }
        public MetricValue CancelBidCount { get; init; }
        public MetricValue CancelAskCount { get; init; }
        public MetricValue CancelBidVolume { get; init; }
        public MetricValue CancelAskVolume { get; init; }
        public MetricValue CancelRatio { get; init; }
        public MetricValue CancelVolumeRatio { get; init; }

        // Category 6: DOM Imbalances
        public MetricValue DOMImbalance3 { get; init; }
        public MetricValue DOMImbalance5 { get; init; }
        public MetricValue DOMImbalance10 { get; init; }
        public MetricValue QueueImbalance { get; init; }
        public MetricValue BookPressure { get; init; }

        // Category 7: Microstructure Signals
        public MetricValue AbsorptionBuy { get; init; }
        public MetricValue AbsorptionSell { get; init; }
        public MetricValue IcebergScoreBid { get; init; }
        public MetricValue IcebergScoreAsk { get; init; }
        public MetricValue ReplenishmentBid { get; init; }
        public MetricValue ReplenishmentAsk { get; init; }
        public MetricValue SpoofScoreBid { get; init; }
        public MetricValue SpoofScoreAsk { get; init; }

        // Deltas
        public MetricValue Delta1s { get; init; }
        public MetricValue Delta5s { get; init; }
        public MetricValue Delta30s { get; init; }
        public MetricValue Delta60s { get; init; }
        public MetricValue DeltaVelocity { get; init; }
    }

    /// <summary>
    /// Displays a real-time, premium dashboard of the advanced metrics specified in DataAnalytics.md
    /// by utilizing the shared <see cref="AnalyticsEngineHost"/> and custom GDI+ chart drawing.
    /// All market data is processed on a background thread; rendering reads a double-buffered snapshot.
    /// </summary>
    public class DataAnalyticsIndicator : Indicator
    {
        // -----------------------------------------------------
        // Input Parameters (Exposed to Quantower UI Settings)
        // -----------------------------------------------------
        [InputParameter("Calibration Mode", 0, variants: new object[] {
            "Auto (MTR Calibration)", CalibrationMode.AutoMTR,
            "Manual", CalibrationMode.Manual
        })]
        public CalibrationMode InputCalibrationMode = CalibrationMode.AutoMTR;

        [InputParameter("Short Trade Volume Window", 1, minimum: 10, maximum: 1000000, increment: 10, decimalPlaces: 0)]
        public double InputTradeVolumeShort = 1000;

        [InputParameter("Long Trade Volume Window", 2, minimum: 50, maximum: 5000000, increment: 50, decimalPlaces: 0)]
        public double InputTradeVolumeLong = 5000;

        [InputParameter("Manual Short L2 Window", 3, minimum: 100, maximum: 500000, increment: 100, decimalPlaces: 0)]
        public int InputOrderCountShort = 2000;

        [InputParameter("Manual Long L2 Window", 4, minimum: 500, maximum: 2000000, increment: 500, decimalPlaces: 0)]
        public int InputOrderCountLong = 10000;

        [InputParameter("MTR Calibration Trades Count", 5, minimum: 100, maximum: 10000, increment: 100, decimalPlaces: 0)]
        public int InputCalibrationTrades = 1000;

        [InputParameter("Initial Snapshot Depth Levels", 6, minimum: 5, maximum: 2000, increment: 5, decimalPlaces: 0)]
        public int InputSnapshotDepth = 360;

        [InputParameter("Analytics Update Frequency (ms)", 7, minimum: 50, maximum: 5000, increment: 50, decimalPlaces: 0)]
        public int InputUpdateFrequencyMs = 250;

        [InputParameter("Absorption Volume Threshold (5s contracts)", 8, minimum: 1, maximum: 100000, increment: 10, decimalPlaces: 0)]
        public double InputAbsorptionThreshold = 0; // 0 = auto-calibrate from MTR; >0 = fixed override

        [InputParameter("Enable Feature Store Export", 9)]
        public bool InputEnableFeatureStore = false;

        [InputParameter("Feature Store Path", 10)]
        public string InputFeatureStorePath = "C:\\Quantower\\Settings\\Scripts\\ScriptsData\\microstructure_features.csv";

        private AnalyticsEngineHost? engine;

        // Custom UI Theme Config (HSL / Modern Dark Palette)
        private readonly Color bgHeader = Color.FromArgb(230, 15, 23, 36);
        private readonly Color bgBody = Color.FromArgb(200, 10, 15, 26);
        private readonly Color borderClr = Color.FromArgb(255, 43, 58, 82);
        private readonly Color textPrimary = Color.FromArgb(255, 230, 235, 245);
        private readonly Color textSecondary = Color.FromArgb(255, 140, 153, 174);
        private readonly Color accentGreen = Color.FromArgb(255, 34, 197, 94);
        private readonly Color accentRed = Color.FromArgb(255, 239, 68, 68);
        private readonly Color accentBlue = Color.FromArgb(255, 59, 130, 246);
        private readonly Color accentOrange = Color.FromArgb(255, 249, 115, 22);

        public DataAnalyticsIndicator() : base()
        {
            Name = "MBP Market Data Analytics";
            Description = "Advanced microstructure analytics for CME Futures trading based on dxFeed MBP data.";
            SeparateWindow = false; // Overlays directly on the main chart
        }

        protected override void OnInit()
        {
            base.OnInit();

            var config = new AnalyticsEngineConfig
            {
                CalibrationMode = InputCalibrationMode,
                TradeVolumeShort = InputTradeVolumeShort,
                TradeVolumeLong = InputTradeVolumeLong,
                OrderCountShort = InputOrderCountShort,
                OrderCountLong = InputOrderCountLong,
                CalibrationTrades = InputCalibrationTrades,
                SnapshotDepth = InputSnapshotDepth,
                UpdateFrequencyMs = InputUpdateFrequencyMs,
                AbsorptionThreshold = InputAbsorptionThreshold,
                EnableFeatureStore = InputEnableFeatureStore,
                FeatureStorePath = InputFeatureStorePath
            };

            engine = new AnalyticsEngineHost(this.Symbol, config, (msg, lvl) => Core.Instance.Loggers.Log(msg, lvl));
            engine.Start();
        }

        protected override void OnUpdate(UpdateArgs args)
        { }

        public override void OnPaintChart(PaintChartEventArgs args)
        {
            base.OnPaintChart(args);

            // Read the double-buffered snapshot reference atomically
            var snapshot = engine?.CurrentSnapshot;
            if (snapshot == null || engine == null || !engine.IsInitialized)
                return;

            Graphics g = args.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Position and Size of Dashboard
            int x = 20;
            int y = 50;
            int width = 740;
            int height = 540;
            int rowHeight = 19;

            // Draw Background panel
            using (GraphicsPath path = GetRoundedRectPath(new Rectangle(x, y, width, height), 8))
            {
                using (SolidBrush brush = new SolidBrush(bgBody))
                    g.FillPath(brush, path);
                using (Pen pen = new Pen(borderClr, 1.5f))
                    g.DrawPath(pen, path);
            }

            // Draw Header
            using (GraphicsPath path = GetRoundedRectPath(new Rectangle(x, y, width, 32), 8, true))
            {
                using (SolidBrush brush = new SolidBrush(bgHeader))
                    g.FillPath(brush, path);
                using (Pen pen = new Pen(borderClr, 1.5f))
                    g.DrawLine(pen, x, y + 32, x + width, y + 32);
            }

            // Typography
            using (Font fontHeader = new Font("Segoe UI", 10, FontStyle.Bold))
            using (Font fontLabel = new Font("Segoe UI", 8, FontStyle.Regular))
            using (Font fontValue = new Font("Segoe UI", 8, FontStyle.Bold))
            using (Font fontCategory = new Font("Segoe UI", 8, FontStyle.Bold))
            using (Font fontMini = new Font("Segoe UI", 7, FontStyle.Regular))
            {
                // Header Text
                using (SolidBrush brush = new SolidBrush(textPrimary))
                    g.DrawString("CME MICROSTRUCTURE ANALYTICS ENGINE", fontHeader, brush, x + 12, y + 8);

                // Instrument and Account info in Header Right
                string infoStr = $"{this.Symbol.Name} | {Core.Instance.CurrentVersion}";
                using (SolidBrush brush = new SolidBrush(textSecondary))
                {
                    SizeF infoSize = g.MeasureString(infoStr, fontLabel);
                    g.DrawString(infoStr, fontLabel, brush, x + width - 12 - infoSize.Width, y + 8);
                }

                // Column X Coordinates (Width: 345 each, 20px gap)
                int col1X = x + 15;
                int col2X = x + 380;
                int colWidth = 345;

                // ==========================================
                // COLUMN 1: TRADES, VWAP & DELTA ANALYTICS
                // ==========================================
                int drawY1 = y + 42;

                // Category 1: Buyer/Seller Analytics
                DrawCategoryHeader(g, "1. BUYER SELLER (60s & Session)", fontCategory, col1X, drawY1, colWidth);
                drawY1 += 17;

                DrawMetricCombinedValueRow(g, "Trade Count Buy / Sell (60s)", snapshot.BuyTradeCount60, "F0", snapshot.SellTradeCount60, "F0", fontLabel, fontValue, textSecondary, textPrimary, col1X, drawY1, colWidth);
                drawY1 += rowHeight;

                DrawMetricCombinedValueRow(g, "Trade Volume Buy / Sell (60s)", snapshot.BuyVolume60, "F0", snapshot.SellVolume60, "F0", fontLabel, fontValue, textSecondary, textPrimary, col1X, drawY1, colWidth, isQty: true);
                drawY1 += rowHeight;

                DrawMetricCombinedValueRow(g, "Buy / Sell Ratio (Count / Vol)", snapshot.BuySellCountRatio60, "F2", snapshot.BuySellVolumeRatio60, "F2", fontLabel, fontValue, textSecondary, GetRatioColor(snapshot.BuySellVolumeRatio60.Value), col1X, drawY1, colWidth);
                drawY1 += rowHeight;

                DrawMetricCombinedValueRow(g, "Cum. Volume Buy / Sell (Sess)", snapshot.CumBuyVolume, "F0", snapshot.CumSellVolume, "F0", fontLabel, fontValue, textSecondary, textPrimary, col1X, drawY1, colWidth, isQty: true);
                drawY1 += rowHeight;

                DrawMetricValueRow(g, "Cumulative Delta (Session)", snapshot.CumDelta, "F0", fontLabel, fontValue, textSecondary, textPrimary, col1X, drawY1, colWidth);
                drawY1 += rowHeight;

                DrawMetricValueRow(g, "Cumulative Trades Count (Session)", snapshot.CumTradeCount, "F0", fontLabel, fontValue, textSecondary, textPrimary, col1X, drawY1, colWidth);
                drawY1 += rowHeight;

                drawY1 += 5; // Separator

                // Category 2: VWAP Analytics
                DrawCategoryHeader(g, "2. VWAP METRICS", fontCategory, col1X, drawY1, colWidth);
                drawY1 += 17;

                DrawMetricCombinedValueRow(g, "Rolling VWAP (1m / 5m)", snapshot.RollingVwap1m, "F2", snapshot.RollingVwap5m, "F2", fontLabel, fontValue, textSecondary, accentBlue, col1X, drawY1, colWidth, isPrice: true);
                drawY1 += rowHeight;

                DrawMetricValueRow(g, "Rolling VWAP (15m)", snapshot.RollingVwap15m, "F2", fontLabel, fontValue, textSecondary, accentBlue, col1X, drawY1, colWidth, isPrice: true);
                drawY1 += rowHeight;

                DrawMetricCombinedValueRow(g, "Session VWAP / Deviation", snapshot.SessionVwap, "F2", snapshot.VwapDeviation, "F2", fontLabel, fontValue, textSecondary, textPrimary, col1X, drawY1, colWidth, isPrice: true);
                drawY1 += rowHeight;

                DrawMetricValueRow(g, "VWAP Distance (Ticks)", snapshot.VwapDistance, "F2", fontLabel, fontValue, textSecondary, textPrimary, col1X, drawY1, colWidth);
                drawY1 += rowHeight;

                DrawMetricCombinedValueRow(g, "Aggressor VWAP Buy / Sell", snapshot.BuyVwap, "F2", snapshot.SellVwap, "F2", fontLabel, fontValue, textSecondary, textPrimary, col1X, drawY1, colWidth, isPrice: true);
                drawY1 += rowHeight;

                drawY1 += 5; // Separator

                // Category 3: Delta Analytics
                DrawCategoryHeader(g, "3. DELTA & VELOCITY", fontCategory, col1X, drawY1, colWidth);
                drawY1 += 17;

                DrawMetricCombinedValueRow(g, "Delta (1s / 5s)", snapshot.Delta1s, "F0", snapshot.Delta5s, "F0", fontLabel, fontValue, textSecondary, textPrimary, col1X, drawY1, colWidth);
                drawY1 += rowHeight;

                DrawMetricCombinedValueRow(g, "Delta (30s / 60s)", snapshot.Delta30s, "F0", snapshot.Delta60s, "F0", fontLabel, fontValue, textSecondary, textPrimary, col1X, drawY1, colWidth);
                drawY1 += rowHeight;

                DrawMetricValueRow(g, "Delta Velocity (1s Diff)", snapshot.DeltaVelocity, "F0", fontLabel, fontValue, textSecondary, textPrimary, col1X, drawY1, colWidth);

                // ==========================================
                // COLUMN 2: DOM, ARRIVALS & MICROSTRUCTURE
                // ==========================================
                int drawY2 = y + 42;

                // Category 4: DOM Imbalances
                DrawCategoryHeader(g, "4. DOM IMBALANCES & PRESSURE", fontCategory, col2X, drawY2, colWidth);
                drawY2 += 17;

                DrawMetricValueRow(g, "Queue Imbalance (Best Bid/Ask)", snapshot.QueueImbalance, "P1", fontLabel, fontValue, textSecondary, GetRatioColor(snapshot.QueueImbalance.Value + 1.0), col2X, drawY2, colWidth);
                drawY2 += rowHeight;

                DrawMetricValueRow(g, "3-Level DOM Imbalance", snapshot.DOMImbalance3, "P1", fontLabel, fontValue, textSecondary, textPrimary, col2X, drawY2, colWidth);
                drawY2 += rowHeight;

                DrawMetricValueRow(g, "5-Level DOM Imbalance", snapshot.DOMImbalance5, "P1", fontLabel, fontValue, textSecondary, textPrimary, col2X, drawY2, colWidth);
                drawY2 += rowHeight;

                DrawMetricValueRow(g, "10-Level DOM Imbalance", snapshot.DOMImbalance10, "P1", fontLabel, fontValue, textSecondary, textPrimary, col2X, drawY2, colWidth);
                drawY2 += rowHeight;

                DrawMetricValueRow(g, "Order Book Pressure", snapshot.BookPressure, "F1", fontLabel, fontValue, textSecondary, textPrimary, col2X, drawY2, colWidth);
                drawY2 += rowHeight;

                drawY2 += 7; // Separator

                // Category 5: Order Arrivals & Cancels
                DrawCategoryHeader(g, "5. ORDER ARRIVALS & CANCELS (60s)", fontCategory, col2X, drawY2, colWidth);
                drawY2 += 17;

                DrawMetricCombinedValueRow(g, "New Orders Bid / Ask (Count)", snapshot.NewBidCount, "F0", snapshot.NewAskCount, "F0", fontLabel, fontValue, textSecondary, textPrimary, col2X, drawY2, colWidth);
                drawY2 += rowHeight;

                DrawMetricCombinedValueRow(g, "New Volume Bid / Ask (Qty)", snapshot.NewBidVolume, "F0", snapshot.NewAskVolume, "F0", fontLabel, fontValue, textSecondary, textPrimary, col2X, drawY2, colWidth, isQty: true);
                drawY2 += rowHeight;

                DrawMetricCombinedValueRow(g, "Cancel Orders Bid / Ask (Count)", snapshot.CancelBidCount, "F0", snapshot.CancelAskCount, "F0", fontLabel, fontValue, textSecondary, textPrimary, col2X, drawY2, colWidth);
                drawY2 += rowHeight;

                DrawMetricCombinedValueRow(g, "Cancel Volume Bid / Ask (Qty)", snapshot.CancelBidVolume, "F0", snapshot.CancelAskVolume, "F0", fontLabel, fontValue, textSecondary, textPrimary, col2X, drawY2, colWidth, isQty: true);
                drawY2 += rowHeight;

                DrawMetricCombinedValueRow(g, "Cancel Ratio Count / Vol (Qty)", snapshot.CancelRatio, "P1", snapshot.CancelVolumeRatio, "P1", fontLabel, fontValue, textSecondary, accentOrange, col2X, drawY2, colWidth);
                drawY2 += rowHeight;

                drawY2 += 7; // Separator

                // Category 6: Microstructure Signals
                DrawCategoryHeader(g, "6. MICROSTRUCTURE EVENTS (60s)", fontCategory, col2X, drawY2, colWidth);
                drawY2 += 17;

                DrawMetricCombinedValueRow(g, "Absorption Buy / Sell (5s)", snapshot.AbsorptionBuy, "F0", snapshot.AbsorptionSell, "F0", fontLabel, fontValue, textSecondary, textPrimary, col2X, drawY2, colWidth, isQty: true);
                drawY2 += rowHeight;

                DrawMetricCombinedValueRow(g, "Iceberg Score Bid / Ask (60s)", snapshot.IcebergScoreBid, "F0", snapshot.IcebergScoreAsk, "F0", fontLabel, fontValue, textSecondary, textPrimary, col2X, drawY2, colWidth, isQty: true);
                drawY2 += rowHeight;

                DrawMetricCombinedValueRow(g, "Replenishment Bid / Ask (60s)", snapshot.ReplenishmentBid, "F0", snapshot.ReplenishmentAsk, "F0", fontLabel, fontValue, textSecondary, textPrimary, col2X, drawY2, colWidth, isQty: true);
                drawY2 += rowHeight;

                DrawMetricCombinedValueRow(g, "Spoofing Score Bid / Ask (60s)", snapshot.SpoofScoreBid, "F0", snapshot.SpoofScoreAsk, "F0", fontLabel, fontValue, textSecondary, textPrimary, col2X, drawY2, colWidth, isQty: true);

                // ==========================================
                // BOTTOM DIAGNOSTIC & MONITORING PANEL
                // ==========================================
                // Draw horizontal line separator
                using (Pen pen = new Pen(borderClr, 1.5f))
                    g.DrawLine(pen, x, y + 440, x + width, y + 440);

                // Status indicators
                string statusText;
                Color statusColor;
                if (!snapshot.BookValid)
                {
                    statusText = "BOOK INVALID (OVERFLOW)";
                    statusColor = accentRed;
                }
                else if (InputCalibrationMode == CalibrationMode.Manual)
                {
                    statusText = "MANUAL MODE (OK)";
                    statusColor = textSecondary;
                }
                else if (snapshot.IsCalibrated)
                {
                    double mtr = (double)snapshot.OrderCountWindowShort / snapshot.TradeVolumeWindowShort;
                    statusText = $"CALIBRATED (MTR: {mtr:F2})";
                    statusColor = accentGreen;
                }
                else
                {
                    statusText = "CALIBRATING STATE...";
                    statusColor = accentOrange;
                }

                long overflows = engine.QueueOverflowCount;
                if (overflows > 0)
                {
                    statusText += $" [OVR: {overflows}]";
                    statusColor = accentRed;
                }

                // Data mode label shown on the right side row 0
                string dataMode = snapshot.IsMboActive ? "MBO (per-order)" : "MBP (price-level)";
                Color dataModeColor = snapshot.IsMboActive ? accentGreen : accentOrange;

                int bottomY = y + 448;
                using (SolidBrush textBrush = new SolidBrush(textSecondary))
                {
                    g.DrawString("Engine Status:", fontMini, textBrush, x + 16, bottomY);
                    g.DrawString("L2 Window (Short/Long):", fontMini, textBrush, x + 16, bottomY + 16);
                    g.DrawString("Trade Window (Short/Long):", fontMini, textBrush, x + 16, bottomY + 32);
                    g.DrawString("Feature Store Path:", fontMini, textBrush, x + 16, bottomY + 48);

                    g.DrawString("Data Mode:", fontMini, textBrush, x + 380, bottomY);
                    g.DrawString("L2 Depth / Update (ms):", fontMini, textBrush, x + 380, bottomY + 16);
                    g.DrawString("Export Logs Status:", fontMini, textBrush, x + 380, bottomY + 32);
                    g.DrawString("Last Updated Utc:", fontMini, textBrush, x + 380, bottomY + 48);
                }

                // Draw status values
                using (SolidBrush statusBrush = new SolidBrush(statusColor))
                    g.DrawString(statusText, fontMini, statusBrush, x + 160, bottomY);

                using (SolidBrush dataModeBrush = new SolidBrush(dataModeColor))
                    g.DrawString(dataMode, fontMini, dataModeBrush, x + 530, bottomY);

                using (SolidBrush valBrush = new SolidBrush(textPrimary))
                {
                    g.DrawString($"{snapshot.OrderCountWindowShort:N0} / {snapshot.OrderCountWindowLong:N0} updates", fontMini, valBrush, x + 160, bottomY + 16);
                    g.DrawString($"{snapshot.TradeVolumeWindowShort:N0} / {snapshot.TradeVolumeWindowLong:N0} contracts", fontMini, valBrush, x + 160, bottomY + 32);

                    string shortPath = InputFeatureStorePath;
                    if (shortPath.Length > 36) shortPath = "..." + shortPath.Substring(shortPath.Length - 33);
                    g.DrawString(InputEnableFeatureStore ? shortPath : "DISABLED", fontMini, valBrush, x + 160, bottomY + 48);

                    g.DrawString($"{InputSnapshotDepth} levels / {InputUpdateFrequencyMs} ms", fontMini, valBrush, x + 530, bottomY + 16);
                    g.DrawString(InputEnableFeatureStore ? "ACTIVE" : "OFF", fontMini, valBrush, x + 530, bottomY + 32);

                    string lastTimeStr = new DateTime(snapshot.ReceiveTicks, DateTimeKind.Utc).ToString("yyyy-MM-dd HH:mm:ss.fff");
                    g.DrawString(lastTimeStr, fontMini, valBrush, x + 530, bottomY + 48);
                }
            }
        }

        private void DrawCategoryHeader(Graphics g, string title, Font font, int x, int y, int width)
        {
            // Accent colored bar
            using (SolidBrush brush = new SolidBrush(Color.FromArgb(40, accentBlue.R, accentBlue.G, accentBlue.B)))
                g.FillRectangle(brush, x + 8, y, width - 16, 16);

            using (SolidBrush textBrush = new SolidBrush(accentBlue))
                g.DrawString(title, font, textBrush, x + 12, y + 2);
        }

        private void DrawMetricRow(Graphics g, string label, string value, Font fontLabel, Font fontValue, Color colorLabel, Color colorValue, int x, int y, int width)
        {
            using (SolidBrush brushLabel = new SolidBrush(colorLabel))
                g.DrawString(label, fontLabel, brushLabel, x + 16, y);

            using (SolidBrush brushValue = new SolidBrush(colorValue))
            {
                SizeF valSize = g.MeasureString(value, fontValue);
                g.DrawString(value, fontValue, brushValue, x + width - 16 - valSize.Width, y);
            }
        }

        private void DrawMetricValueRow(Graphics g, string label, MetricValue metric, string format, Font fontLabel, Font fontValue, Color colorLabel, Color colorValue, int x, int y, int width, bool isPrice = false, bool isQty = false)
        {
            using (SolidBrush brushLabel = new SolidBrush(colorLabel))
                g.DrawString(label, fontLabel, brushLabel, x + 16, y);

            string valueStr = FormatMetricValue(metric, format, isPrice, isQty);
            Color valueColor = colorValue;

            using (SolidBrush brushValue = new SolidBrush(valueColor))
            {
                SizeF valSize = g.MeasureString(valueStr, fontValue);
                g.DrawString(valueStr, fontValue, brushValue, x + 250 - valSize.Width, y);
            }

            string qualityStr = metric.Quality.ToString().ToUpperInvariant();
            Color qualityColor = GetQualityColor(metric.Quality);

            using (Font fontQuality = new Font("Segoe UI", 7, FontStyle.Bold))
            using (SolidBrush brushQuality = new SolidBrush(qualityColor))
            {
                SizeF qSize = g.MeasureString(qualityStr, fontQuality);
                g.DrawString(qualityStr, fontQuality, brushQuality, x + width - 16 - qSize.Width, y + 1);
            }
        }

        private void DrawMetricCombinedValueRow(Graphics g, string label, MetricValue metric1, string format1, MetricValue metric2, string format2, Font fontLabel, Font fontValue, Color colorLabel, Color colorValue, int x, int y, int width, bool isPrice = false, bool isQty = false)
        {
            using (SolidBrush brushLabel = new SolidBrush(colorLabel))
                g.DrawString(label, fontLabel, brushLabel, x + 16, y);

            string valStr1 = FormatMetricValue(metric1, format1, isPrice, isQty);
            string valStr2 = FormatMetricValue(metric2, format2, isPrice, isQty);
            string valueStr = $"{valStr1} / {valStr2}";

            using (SolidBrush brushValue = new SolidBrush(colorValue))
            {
                SizeF valSize = g.MeasureString(valueStr, fontValue);
                g.DrawString(valueStr, fontValue, brushValue, x + 250 - valSize.Width, y);
            }

            MetricQuality minQuality = GetMinQuality(metric1.Quality, metric2.Quality);
            string qualityStr = minQuality.ToString().ToUpperInvariant();
            Color qualityColor = GetQualityColor(minQuality);

            using (Font fontQuality = new Font("Segoe UI", 7, FontStyle.Bold))
            using (SolidBrush brushQuality = new SolidBrush(qualityColor))
            {
                SizeF qSize = g.MeasureString(qualityStr, fontQuality);
                g.DrawString(qualityStr, fontQuality, brushQuality, x + width - 16 - qSize.Width, y + 1);
            }
        }

        private string FormatMetricValue(MetricValue metric, string format, bool isPrice = false, bool isQty = false)
        {
            if (metric.Quality == MetricQuality.Unavailable)
                return "-";

            if (isPrice)
                return this.Symbol.FormatPrice(metric.Value);

            if (isQty)
                return this.Symbol.FormatQuantity(metric.Value);

            return metric.Value.ToString(format);
        }

        private Color GetQualityColor(MetricQuality quality)
        {
            switch (quality)
            {
                case MetricQuality.Exact:
                    return accentGreen;
                case MetricQuality.Derived:
                    return accentBlue;
                case MetricQuality.Heuristic:
                    return accentOrange;
                case MetricQuality.Unavailable:
                default:
                    return textSecondary;
            }
        }

        private MetricQuality GetMinQuality(MetricQuality q1, MetricQuality q2)
        {
            return (q1 < q2) ? q1 : q2;
        }

        private Color GetRatioColor(double ratio)
        {
            if (ratio > 1.15) return accentGreen;
            if (ratio < 0.85) return accentRed;
            return textPrimary;
        }

        private GraphicsPath GetRoundedRectPath(Rectangle bounds, int radius, bool topOnly = false)
        {
            GraphicsPath path = new GraphicsPath();
            int d = radius * 2;

            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.X + bounds.Width - d, bounds.Y, d, d, 270, 90);

            if (topOnly)
            {
                path.AddLine(bounds.X + bounds.Width, bounds.Y + bounds.Height, bounds.X, bounds.Y + bounds.Height);
            }
            else
            {
                path.AddArc(bounds.X + bounds.Width - d, bounds.Y + bounds.Height - d, d, d, 0, 90);
                path.AddArc(bounds.X, bounds.Y + bounds.Height - d, d, d, 90, 90);
            }

            path.CloseFigure();
            return path;
        }

        protected override void OnClear()
        {
            engine?.Stop();
            engine = null;

            // Force garbage collection to flush and release any open Serilog files
            GC.Collect();
            GC.WaitForPendingFinalizers();

            base.OnClear();
        }
    }
}
