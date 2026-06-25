using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Threading;

namespace MBO_Market_Data_Analytics.LegacyV1
{
    /// <summary>
    /// Asynchronous, batched, daily-rotated recorder for the normalized MBO event stream. Runs off the
    /// market-data thread: producers only enqueue; a dedicated writer thread serializes to CSV. The
    /// queue is bounded and overflows are dropped-and-counted so recording can never stall the feed.
    ///
    /// CSV columns: seq,timeUtc,action,side,price,size,priority,numOrders,orderId
    /// One file per UTC day: &lt;prefix&gt;_&lt;symbol&gt;_&lt;yyyyMMdd&gt;.csv
    /// </summary>
    public sealed class MboRecorder
    {
        private readonly string directory;
        private readonly string symbolTag;
        private readonly BlockingCollection<MboEvent> queue;
        private Thread? writer;
        private CancellationTokenSource? cts;

        private long written;
        private long dropped;
        private volatile string currentFile = "";

        private const int FlushEveryLines = 2000;
        private static readonly CultureInfo CI = CultureInfo.InvariantCulture;

        public MboRecorder(string directory, string symbolTag, int queueCapacity = 200_000)
        {
            this.directory = string.IsNullOrWhiteSpace(directory) ? "." : directory;
            this.symbolTag = Sanitize(symbolTag);
            this.queue = new BlockingCollection<MboEvent>(Math.Max(1000, queueCapacity));
        }

        public long Written => Interlocked.Read(ref written);
        public long Dropped => Interlocked.Read(ref dropped);
        public string CurrentFile => currentFile;
        public int QueueDepth => queue.Count;

        public void Start()
        {
            Directory.CreateDirectory(directory);
            cts = new CancellationTokenSource();
            writer = new Thread(WriteLoop)
            {
                IsBackground = true,
                Name = "MboRecorderWriter"
            };
            writer.Start();
        }

        public void Record(in MboEvent evt)
        {
            if (queue.IsAddingCompleted) return;
            if (!queue.TryAdd(evt))
                Interlocked.Increment(ref dropped);
        }

        public void Stop()
        {
            // H-23: CompleteAdding() signals the writer to drain then exit via InvalidOperationException.
            // Cancel only after Join so queued events are not lost; if Join times out the cancel forces exit.
            try { queue.CompleteAdding(); } catch { }
            if (writer != null && writer.IsAlive)
                writer.Join(5000);
            cts?.Cancel();
        }

        private void WriteLoop()
        {
            if (cts == null) return;
            var token = cts.Token;

            StreamWriter? sw = null;
            string openDay = "";
            int sinceFlush = 0;

            try
            {
                while (!token.IsCancellationRequested || queue.Count > 0)
                {
                    MboEvent evt;
                    try
                    {
                        if (!queue.TryTake(out evt, 200, token))
                        {
                            sw?.Flush();
                            sinceFlush = 0;
                            continue;
                        }
                    }
                    catch (OperationCanceledException) { break; }
                    catch (InvalidOperationException) { break; } // completed + empty

                    string day = evt.Time.ToString("yyyyMMdd", CI);
                    if (sw == null || day != openDay)
                    {
                        sw?.Flush();
                        sw?.Dispose();
                        sw = OpenFile(day);
                        openDay = day;
                    }

                    sw.WriteLine(Format(evt));
                    Interlocked.Increment(ref written);

                    if (++sinceFlush >= FlushEveryLines)
                    {
                        sw.Flush();
                        sinceFlush = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                TradingPlatform.BusinessLayer.Core.Instance.Loggers.Log(
                    $"[MboRecorder] Writer error: {ex.Message}", TradingPlatform.BusinessLayer.LoggingLevel.Error);
            }
            finally
            {
                try { sw?.Flush(); sw?.Dispose(); } catch { }
            }
        }

        private StreamWriter OpenFile(string day)
        {
            string path = Path.Combine(directory, $"mbo_{symbolTag}_{day}.csv");
            bool isNew = !File.Exists(path);
            var s = new StreamWriter(path, append: true);
            currentFile = path;
            if (isNew)
                s.WriteLine("seq,timeUtc,action,side,price,size,priority,numOrders,orderId");
            return s;
        }

        private static string Format(in MboEvent e)
        {
            // orderId is feed-controlled; strip commas defensively.
            string id = e.OrderId?.Replace(",", "") ?? "";
            string side = (e.Action == MboAction.Trade && !e.IsAggressorKnown) ? "U" : (e.IsBid ? "B" : "A");
            return string.Concat(
                e.Seq.ToString(CI), ",",
                e.Time.ToString("o", CI), ",",
                e.Action.ToString(), ",",
                side, ",",
                e.Price.ToString("R", CI), ",",
                e.Size.ToString("R", CI), ",",
                e.Priority.ToString(CI), ",",
                e.NumberOrders.ToString(CI), ",",
                id);
        }

        private static string Sanitize(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "SYM";
            foreach (char c in Path.GetInvalidFileNameChars())
                s = s.Replace(c, '_');
            return s;
        }
    }
}
