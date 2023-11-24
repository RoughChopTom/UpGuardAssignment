using MicroBatch;

namespace MicroBatchTests;

[TestFixture]
public class JobTests
{
    [Test]
    public void Should_ThrowException_If_JobId_IsNullOrWhiteSpace()
    {
        try
        {
            _ = new Job(null!, default!);
        }
        catch (ArgumentException e)
        {
            Assert.AreEqual("Value does not fall within the expected range. (Parameter 'id')", e.Message);
        }
        catch (Exception e)
        {
            Assert.Fail();
        }
    }
    
    [Test]
    public void Should_ThrowException_If_Data_IsNull()
    {
        try
        {
            _ = new Job("1", null!);
        }
        catch (ArgumentNullException e)
        {
            Assert.AreEqual("Value cannot be null. (Parameter 'data')", e.Message);
        }
        catch (Exception e)
        {
            Assert.Fail();
        }
    }
    
    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    public void Should_SetJobPriorityCorrectly(int jobPriority)
    {
        var job = new Job("1", new object(), (JobPriority)jobPriority);
        Assert.AreEqual((JobPriority)jobPriority, job.JobPriority);
    }
    
    [Test]
    public void Should_DefaultJobPriorityToLow()
    {
        var job = new Job("1", new object());
        Assert.AreEqual(JobPriority.Low, job.JobPriority);
    }
}