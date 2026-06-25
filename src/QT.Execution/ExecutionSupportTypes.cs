using TradingPlatform.BusinessLayer;

namespace MBO_Market_Data_Analytics;

internal enum OrderRole : byte
{
    Entry,
    Bracket,
    Reconciled
}

internal enum StrategyLifecycleState : byte
{
    Initializing,
    Running,
    Halting,
    Flattening,
    FlatVerified,
    Stopped
}

internal sealed record TrackedOrderState
{
    public string LocalId { get; init; } = null!;
    public string? OrderId { get; init; }
    public Side Side { get; init; }
    public double Price { get; init; }
    public double Quantity { get; init; }
    public double FilledQuantity { get; init; }
    public double CumAverageFillPrice { get; init; }
    public OrderStatus Status { get; init; }
    public Order? OrderInstance { get; init; }
    public OrderRole Role { get; init; }
    public bool? IsStopHint { get; init; }
}

public enum TaskCategory : byte
{
    General,
    EntryPlacement,
    Protection,
    Flatten
}

internal sealed class PrioritizedAsyncTaskQueue
{
    private readonly PriorityQueue<(Func<Task> action, TaskCategory category), int> queue = new();
    private readonly object lockObj = new();
    private readonly SemaphoreSlim semaphore = new(0);
    private readonly CancellationTokenSource cts = new();
    private readonly Task consumer;
    private volatile bool accepting = true;

    public PrioritizedAsyncTaskQueue()
    {
        consumer = Task.Run(ProcessQueueLoopAsync);
    }

    public bool IsAccepting => accepting;

    public bool Enqueue(Func<Task> taskAction, int priority, TaskCategory category = TaskCategory.General)
    {
        if (taskAction == null) return false;
        lock (lockObj)
        {
            if (!accepting) return false;
            queue.Enqueue((taskAction, category), priority);
        }
        semaphore.Release();
        return true;
    }

    public void StopAccepting() => accepting = false;

    public int CancelPendingEntries()
    {
        lock (lockObj)
        {
            if (queue.Count == 0) return 0;
            var kept = new List<((Func<Task> action, TaskCategory category) item, int priority)>(queue.Count);
            int removed = 0;
            while (queue.TryDequeue(out var item, out int prio))
            {
                if (item.category == TaskCategory.EntryPlacement)
                {
                    removed++;
                    continue;
                }
                kept.Add((item, prio));
            }
            foreach (var k in kept)
                queue.Enqueue(k.item, k.priority);
            return removed;
        }
    }

    public void Shutdown(int waitMs = 2000)
    {
        accepting = false;
        cts.Cancel();
        semaphore.Release();
        try { consumer.Wait(waitMs); }
        catch (AggregateException) { }
    }

    private async Task ProcessQueueLoopAsync()
    {
        while (!cts.Token.IsCancellationRequested)
        {
            try
            {
                await semaphore.WaitAsync(cts.Token);
                (Func<Task> action, TaskCategory category)? work = null;
                lock (lockObj)
                {
                    if (queue.Count > 0)
                        work = queue.Dequeue();
                }

                if (work != null)
                    await work.Value.action();
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch
            {
                // Dormant infrastructure: execution tests assert state behavior; host logging is not active in V2.
            }
        }
    }
}
