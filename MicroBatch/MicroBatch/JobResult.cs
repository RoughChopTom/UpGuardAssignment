namespace MicroBatch;

public class JobResult
{
    public string Id { get; private set; }
    public bool IsSuccess { get; private set; }
    public string ErrorMessage { get; private set; }

    public JobResult(string id, bool isSuccess, string errorMessage = "")
    {
        Id = id;
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }
}