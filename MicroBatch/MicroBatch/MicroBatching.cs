using System.Runtime.InteropServices;

namespace MicroBatch;

public class MicroBatching : IMicroBatching
{
    public PriorityQueue<Job, JobPriority> JobQueue { get; } = new();
    private readonly IBatchProcessor batchProcessor;
    private readonly int batchSize;
    private readonly TimeSpan batchFrequencyMilliseconds;
    private readonly CancellationTokenSource cancellationTokenSource;
    private bool isStarted;
    private const int MaxBatchSize = 10000;
    private static readonly TimeSpan MaxBatchFrequencyMinutes = TimeSpan.FromMinutes(5);

    public MicroBatching(IBatchProcessor batchProcessor, int batchSize, TimeSpan batchFrequencyMilliseconds)
    {
        if (batchSize is <= 0 or > MaxBatchSize)
        {
            throw new ArgumentOutOfRangeException(nameof(batchSize),$"Batch size must be between 1 and {MaxBatchSize}.");
        }

        if (batchFrequencyMilliseconds <= TimeSpan.Zero || batchFrequencyMilliseconds > MaxBatchFrequencyMinutes)
        {
            throw new ArgumentOutOfRangeException(nameof(this.batchFrequencyMilliseconds),$"Batch frequency milliseconds must be between 1 and {MaxBatchFrequencyMinutes.TotalMilliseconds}.");
        }
        
        this.batchProcessor = batchProcessor ?? throw new ArgumentNullException(nameof(batchProcessor));
        this.batchSize = batchSize;
        this.batchFrequencyMilliseconds = batchFrequencyMilliseconds;
        cancellationTokenSource = new CancellationTokenSource();
    }

    public JobResult SubmitJob(Job job)
    {
        if (cancellationTokenSource.IsCancellationRequested)
        {
            return new JobResult(job.Id, false, "Cannot accept Job as shutdown has been executed.");
        }

        lock (JobQueue)
        {
            JobQueue.Enqueue(job, job.JobPriority);
        }

        if (!isStarted)
        {
            Start();
        }

        return new JobResult(job.Id, true);
    }

    public async Task ShutdownAsync()
    {
        cancellationTokenSource.Cancel();
        isStarted = false;

        await Task.Run(ProcessBatchAsync);
    }
    
    private void Start()
    {
        isStarted = true;
        Task.Run(ProcessBatchAsync, cancellationTokenSource.Token);
    }

    private async Task ProcessBatchAsync()
    {
        while (isStarted || (cancellationTokenSource.IsCancellationRequested && JobQueue.Count > 0))
        {
            var batch = GetBatch();

            if (batch.Any())
            {
                try
                {
                    batchProcessor.ProcessBatch(batch);
                }
                catch (Exception e)
                {
                    throw new ExternalException("ProcessBatch() on BatchProcessor failed: ", e);
                }
            }

            await Task.Delay(batchFrequencyMilliseconds);
        }
    }

    private List<Job> GetBatch()
    {
        lock (JobQueue)
        {
            var batch = new List<Job>(batchSize);

            while (batch.Count < batchSize && JobQueue.Count > 0)
            {
                batch.Add(JobQueue.Dequeue());
            }

            return batch;
        }
    }
}