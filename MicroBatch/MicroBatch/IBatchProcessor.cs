namespace MicroBatch;

public interface IBatchProcessor
{
    void ProcessBatch(List<Job> batch);
}