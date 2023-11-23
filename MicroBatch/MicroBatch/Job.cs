namespace MicroBatch;

public class Job
{
    public string Id { get; }
    public object Data { get; }
    public JobPriority JobPriority { get; set; }

    public Job(string id, object data, JobPriority jobPriority = JobPriority.Low)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException(null, nameof(id));
        }

        Id = id;
        Data = data ?? throw new ArgumentNullException(nameof(data));
        JobPriority = jobPriority;
    }
}