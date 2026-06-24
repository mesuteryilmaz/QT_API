using System;
using System.Threading.Tasks;

namespace MBO_Market_Data_Analytics
{
    /// <summary>
    /// Seam over the prioritized async work queue. The execution logic posts broker work (cancels,
    /// placements, flattens) through this so a synchronous fake scheduler can run it deterministically
    /// in tests, instead of the real background queue. The work unit matches the queue's Func&lt;Task&gt;
    /// so production posting is a thin pass-through and the bodies (which today complete synchronously
    /// and return Task.CompletedTask) need no change.
    /// </summary>
    public interface IExecScheduler
    {
        /// <summary>Schedule <paramref name="work"/> at the given priority (lower runs first) and category.</summary>
        void Post(Func<Task> work, int priority, TaskCategory category);

        /// <summary>Reject further posts (used when entering Halting / teardown).</summary>
        void StopAccepting();

        /// <summary>Drop queued entry-placement work; returns how many were removed.</summary>
        int CancelPendingEntries();

        /// <summary>Stop accepting, cancel, and bounded-join any in-flight work.</summary>
        void Shutdown();
    }

    /// <summary>Production <see cref="IExecScheduler"/> over <see cref="PrioritizedAsyncTaskQueue"/>.</summary>
    internal sealed class QueueScheduler : IExecScheduler
    {
        private readonly PrioritizedAsyncTaskQueue queue;

        public QueueScheduler(PrioritizedAsyncTaskQueue queue) => this.queue = queue;

        public void Post(Func<Task> work, int priority, TaskCategory category) => queue.Enqueue(work, priority, category);
        public void StopAccepting() => queue.StopAccepting();
        public int CancelPendingEntries() => queue.CancelPendingEntries();
        public void Shutdown() => queue.Shutdown();
    }
}
