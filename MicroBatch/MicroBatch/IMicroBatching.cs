namespace MicroBatch;

public interface IMicroBatching
{
    PriorityQueue<Job, JobPriority> JobQueue { get; }
    JobResult SubmitJob(Job job);
    Task ShutdownAsync();
}