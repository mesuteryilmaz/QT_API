using System;
using System.Drawing;
using TradingPlatform.BusinessLayer;
using TradingPlatform.BusinessLayer.Integration;

namespace MBO_Market_Data_Analytics
{
    /// <summary>
    /// Standalone Market-By-Order ingestion + recorder. Subscribes to the live order-level Level2
    /// stream (and trades), reconstructs an order-keyed book, and writes the normalized event stream to
    /// disk as the canonical research corpus — the forward-capture answer to "no historical depth."
    ///
    /// Deliberately independent of the analytics engine / strategy so it can run purely for data
    /// capture with zero impact on the live trading path. Timestamps are receive-time UTC, consistent
    /// with the rest of the engine.
    /// </summary>
    public class MboRecorderIndicator : Indicator
    {
        [InputParameter("Enable Recording", 0)]
        public bool InputEnableRecording = true;

        [InputParameter("Record Trades", 1)]
        public bool InputRecordTrades = true;

        [InputParameter("Output Directory", 2)]
        public string InputOutputDir = "C:\\Quantower\\Settings\\Scripts\\ScriptsData\\mbo";

        [InputParameter("Seed Snapshot Depth Levels", 3, minimum: 0, maximum: 5000, increment: 50, decimalPlaces: 0)]
        public int InputSeedDepth = 1000;

        [InputParameter("Show Panel", 4)]
        public bool InputShowPanel = true;

        private MboOrderBook? book;
        private MboRecorder? recorder;
        private readonly object bookLock = new object();
        private volatile bool subscribed;

        // Diagnostics (read on UI thread)
        private double avgNumberOrders = 1.0;     // EMA of quote.NumberOrders (MBO tell)
        private long eventCount;
        private long lastEventCount;
        private DateTime lastRateStamp = DateTime.UtcNow;
        private double eventsPerSec;

        private readonly Color bg = Color.FromArgb(220, 10, 15, 26);
        private readonly Color border = Color.FromArgb(255, 43, 58, 82);
        private readonly Color text = Color.FromArgb(255, 230, 235, 245);
        private readonly Color accentGreen = Color.FromArgb(255, 34, 197, 94);
        private readonly Color accentRed = Color.FromArgb(255, 239, 68, 68);
        private readonly Color accentOrange = Color.FromArgb(255, 249, 115, 22);

        public MboRecorderIndicator() : base()
        {
            Name = "MBP MBO Recorder";
            Description = "Records the live order-level (MBO) book + trades to disk and reconstructs the order-keyed book.";
            SeparateWindow = false;
        }

        protected override void OnInit()
        {
            base.OnInit();

            book = new MboOrderBook(this.Symbol.TickSize);

            if (InputEnableRecording)
            {
                recorder = new MboRecorder(InputOutputDir, this.Symbol.Name);
                recorder.Start();
            }

            SeedSnapshot();

            this.Symbol.NewLevel2 += OnNewLevel2;
            if (InputRecordTrades)
                this.Symbol.NewLast += OnNewLast;
            subscribed = true;

            Core.Instance.Loggers.Log(
                $"[MboRecorder] Started for {this.Symbol.Name}. Recording: {InputEnableRecording}, dir: {InputOutputDir}.",
                LoggingLevel.System);
        }

        protected override void OnUpdate(UpdateArgs args) { }

        private void SeedSnapshot()
        {
            if (book == null || InputSeedDepth <= 0) return;
            try
            {
                var snap = this.Symbol.DepthOfMarket.GetDepthOfMarketAggregatedCollections(new GetLevel2ItemsParameters
                {
                    AggregateMethod = AggregateMethod.None, // non-aggregated → per-order
                    GetMBOItems = true,
                    LevelsCount = InputSeedDepth
                });

                if (snap == null) return;
                DateTime now = DateTime.UtcNow;

                lock (bookLock)
                {
                    if (snap.Bids != null)
                        foreach (var b in snap.Bids)
                            if (b != null) RecordSeed(book.ApplySnapshot(now, b.Id, true, b.Price, b.Size, b.Priority, b.NumberOrders));

                    if (snap.Asks != null)
                        foreach (var a in snap.Asks)
                            if (a != null) RecordSeed(book.ApplySnapshot(now, a.Id, false, a.Price, a.Size, a.Priority, a.NumberOrders));
                }
            }
            catch (Exception ex)
            {
                Core.Instance.Loggers.Log($"[MboRecorder] Snapshot seed failed (feed may not support MBO snapshot): {ex.Message}", LoggingLevel.Error);
            }
        }

        private void RecordSeed(in MboEvent e) => recorder?.Record(e);

        private void OnNewLevel2(Symbol sym, Level2Quote q, DOMQuote dom)
        {
            if (q == null || book == null) return;

            bool isBid = q.PriceType == QuotePriceType.Bid;
            MboEvent e;
            lock (bookLock)
            {
                e = book.Apply(DateTime.UtcNow, q.Id, isBid, q.Price, q.Size, q.Priority, q.NumberOrders, q.Closed);
                // EMA of NumberOrders: ~1 ⇒ true per-order MBO; >1 ⇒ price-aggregated feed.
                avgNumberOrders += (q.NumberOrders - avgNumberOrders) * 0.001;
            }
            recorder?.Record(e);
            System.Threading.Interlocked.Increment(ref eventCount);
        }

        private void OnNewLast(Symbol sym, Last last)
        {
            if (last == null || book == null) return;
            bool buyAgg = last.AggressorFlag == AggressorFlag.Buy;
            MboEvent e;
            lock (bookLock)
            {
                e = book.ApplyTrade(DateTime.UtcNow, buyAgg, last.Price, last.Size);
            }
            recorder?.Record(e);
            System.Threading.Interlocked.Increment(ref eventCount);
        }

        public override void OnPaintChart(PaintChartEventArgs args)
        {
            base.OnPaintChart(args);
            if (!InputShowPanel) return;

            // Refresh the events/sec rate.
            DateTime now = DateTime.UtcNow;
            double elapsed = (now - lastRateStamp).TotalSeconds;
            if (elapsed >= 1.0)
            {
                long c = System.Threading.Interlocked.Read(ref eventCount);
                eventsPerSec = (c - lastEventCount) / elapsed;
                lastEventCount = c;
                lastRateStamp = now;
            }

            int orders = 0, bidLevels = 0, askLevels = 0;
            double bestBid = 0, bestBidSize = 0, bestAsk = 0, bestAskSize = 0;
            double avgNo;
            if (book != null)
            {
                lock (bookLock)
                {
                    orders = book.OrderCount;
                    bidLevels = book.BidLevels;
                    askLevels = book.AskLevels;
                    book.TryGetBestBid(out bestBid, out bestBidSize);
                    book.TryGetBestAsk(out bestAsk, out bestAskSize);
                    avgNo = avgNumberOrders;
                }
            }
            else avgNo = 1.0;

            bool looksMbo = avgNo <= 1.2;
            long written = recorder?.Written ?? 0;
            long dropped = recorder?.Dropped ?? 0;
            string file = recorder?.CurrentFile ?? "(not recording)";
            if (file.Length > 42) file = "..." + file.Substring(file.Length - 39);

            var g = args.Graphics;
            int x = 20, y = 50, w = 360, rows = 9;
            int h = 16 + rows * 18;
            using (var b = new SolidBrush(bg)) g.FillRectangle(b, x, y, w, h);
            using (var p = new Pen(border, 1.5f)) g.DrawRectangle(p, x, y, w, h);

            using (var fTitle = new Font("Segoe UI", 9, FontStyle.Bold))
            using (var f = new Font("Segoe UI", 8, FontStyle.Regular))
            {
                int ty = y + 8;
                void Line(string s, Color c) { using var br = new SolidBrush(c); g.DrawString(s, f, br, x + 10, ty); ty += 18; }

                using (var br = new SolidBrush(text)) g.DrawString("MBO RECORDER", fTitle, br, x + 10, ty); ty += 18;

                Color recColor = InputEnableRecording ? accentGreen : accentOrange;
                Line($"{this.Symbol.Name}   REC: {(InputEnableRecording ? "ON" : "OFF")}", recColor);
                Line($"File: {file}", text);
                Line($"Written: {written:N0}   Dropped: {dropped:N0}", dropped > 0 ? accentRed : text);
                Line($"Events/sec: {eventsPerSec:N0}", text);
                Line($"Orders: {orders:N0}   Bid lvls: {bidLevels}   Ask lvls: {askLevels}", text);
                Line($"Best bid: {bestBid:F2} ({bestBidSize:N0})", text);
                Line($"Best ask: {bestAsk:F2} ({bestAskSize:N0})", text);
                Line($"Granularity: {(looksMbo ? "MBO (per-order)" : "AGGREGATED")}  avg/level {avgNo:F2}", looksMbo ? accentGreen : accentOrange);
            }
        }

        protected override void OnClear()
        {
            if (subscribed && this.Symbol != null)
            {
                this.Symbol.NewLevel2 -= OnNewLevel2;
                this.Symbol.NewLast -= OnNewLast;
                subscribed = false;
            }

            recorder?.Stop();
            recorder = null;
            book = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();

            base.OnClear();
        }
    }
}
