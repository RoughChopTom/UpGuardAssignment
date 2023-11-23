using System.Runtime.InteropServices;
using MicroBatch;
using Rhino.Mocks;

namespace MicroBatchTests;

public class MicroBatchingTests
{
    [Test]
    public void SubmitJob_Should_ProcessJobAndReturnJobResult()
    {
        var batchProcessorStub = MockRepository.GenerateStub<IBatchProcessor>();
        batchProcessorStub.Stub(x => x.ProcessBatch(Arg<List<Job>>.Is.Anything));
        var testee = new MicroBatching(batchProcessorStub, 5, TimeSpan.FromMilliseconds(1000));
        var job = new Job("1", "data");
        var jobResult = testee.SubmitJob(job);
        
        Assert.AreEqual(job.Id, jobResult.Id);
        Assert.AreEqual(true, jobResult.IsSuccess);
        Assert.AreEqual(0, testee.JobQueue.Count);
    }
    
    [Test]
    public async Task ProcessBatch_Should_ThrowExceptionWhenExternalErrorOccurs()
    {
        var batchProcessorStub = MockRepository.GenerateStub<IBatchProcessor>();
        batchProcessorStub.Stub(x => x.ProcessBatch(Arg<List<Job>>.Is.Anything)).IgnoreArguments().Throw(new Exception("BOOM"));

        try
        {
            var testee = new MicroBatching(batchProcessorStub, 5, TimeSpan.FromMilliseconds(1000));
        
            for (var i = 0; i <= 5; i++)
            {
                var job = new Job(i.ToString(), "data");
                testee.SubmitJob(job);
            }
        
            await testee.ShutdownAsync();
        }
        catch (ExternalException e)
        {
            Assert.AreEqual("ProcessBatch() on BatchProcessor failed: ", e.Message);
        }
        catch (Exception)
        {
            Assert.Fail();
        }
    }
    
    [Test]
    public async Task ShutdownAsync_Should_ProcessAllRemainingJobs()
    {
        var batchProcessorStub = MockRepository.GenerateStub<IBatchProcessor>();
        batchProcessorStub.Stub(x => x.ProcessBatch(Arg<List<Job>>.Is.Anything));
        var testee = new MicroBatching(batchProcessorStub, 5, TimeSpan.FromMilliseconds(1000));
    
        for (var i = 0; i <= 20; i++)
        {
            var job = new Job(i.ToString(), "data");
            testee.SubmitJob(job);
        }
        
        await testee.ShutdownAsync();
    
        Assert.That(testee.JobQueue, Is.Empty);
    }
    
    [Test]
    public async Task SubmitJob_Should_Return_UnsuccessfulJobResult_When_ShutdownOccurs()
    {
        var batchProcessorStub = MockRepository.GenerateStub<IBatchProcessor>();
        batchProcessorStub.Stub(x => x.ProcessBatch(Arg<List<Job>>.Is.Anything));
        var testee = new MicroBatching(batchProcessorStub, 5, TimeSpan.FromMilliseconds(1000));
        var job = new Job("1", "data");
    
        await testee.ShutdownAsync();
        var jobResult = testee.SubmitJob(job);
    
        Assert.AreEqual(job.Id, jobResult.Id);
        Assert.AreEqual(false, jobResult.IsSuccess);
        Assert.AreEqual("Cannot accept Job as shutdown has been executed.", jobResult.ErrorMessage);
    }
    
    [Test]
    public void Should_ThrowException_If_BatchProcessor_IsNull()
    {
        var batchProcessorStub = MockRepository.GenerateStub<IBatchProcessor>();
        batchProcessorStub.Stub(x => x.ProcessBatch(Arg<List<Job>>.Is.Anything));

        try
        {
           _ = new MicroBatching(null!, 5, TimeSpan.FromMilliseconds(100));
        }
        catch (ArgumentNullException e)
        {
            Assert.AreEqual("Value cannot be null. (Parameter 'batchProcessor')", e.Message);
        }
        catch (Exception)
        {
            Assert.Fail();
        }
    }

    [TestCase(0)]
    [TestCase(10001)]
    public void Should_ThrowException_If_BatchSize_IsOutOfRange(int batchSize)
    {
        var batchProcessorStub = MockRepository.GenerateStub<IBatchProcessor>();
        batchProcessorStub.Stub(x => x.ProcessBatch(Arg<List<Job>>.Is.Anything));

        try
        {
            _ = new MicroBatching(batchProcessorStub, batchSize, TimeSpan.FromMilliseconds(100));
        }
        catch (ArgumentOutOfRangeException e)
        {
            Assert.AreEqual("Batch size must be between 1 and 10000. (Parameter 'batchSize')", e.Message);
        }
        catch (Exception)
        {
            Assert.Fail();
        }
    }
    
    [TestCase(0)]
    [TestCase(300001)]
    public void Should_ThrowException_If_BatchFrequency_IsOutOfRange(int batchFrequencyMilliseconds)
    {
        var batchProcessorStub = MockRepository.GenerateStub<IBatchProcessor>();
        batchProcessorStub.Stub(x => x.ProcessBatch(Arg<List<Job>>.Is.Anything));

        try
        {
            _ = new MicroBatching(batchProcessorStub, 5, TimeSpan.FromMilliseconds(0));
        }
        catch (ArgumentOutOfRangeException e)
        {
            Assert.AreEqual("Batch frequency milliseconds must be between 1 and 300000. (Parameter 'batchFrequencyMilliseconds')", e.Message);
        }
        catch (Exception)
        {
            Assert.Fail();
        }
    }
}