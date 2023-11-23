Micro-Batching
-------------------

Micro-batching is a technique used in processing pipelines where individual tasks are grouped together into small batches. This can improve throughput by reducing the number of requests made to a downstream system. Here we have implemented a micro-batching library, with the following requirements:

● it should allow the caller to submit a single Job, and it should return a JobResult

● it should process accepted Jobs in batches using a BatchProcessor

    ●  BatchProcessor is not implemented. This is a dependency of this library.

● it should provide a way to configure the batching behaviour i.e. size and frequency

● it should expose a shutdown method which returns after all previously accepted Jobs are processed

### Target framework: net7.0
### Language Version: C# 11.0

<br>

### Assumptions
● A maximum batch size of 10000.

● A maximum batch frequency of 5 minutes (300000 milliseconds).

● If a given batch is smaller than `batchSize` it will still be processed. This ensures individual jobs don't sit idly in the queue longer than they need to.



## MicroBatching Class

The `MicroBatching` class provides a mechanism for submitting jobs to a batch processor and processing them in batches.
Batches are processed on a given frequency `batchFrequencyMilliseconds`.

### Constructor

```
public MicroBatching(IBatchProcessor batchProcessor, int batchSize, TimeSpan batchFrequencyMilliseconds)
```

Creates a new instance of the `MicroBatching` class.

Parameters:

-   `batchProcessor`: The batch processor to use for processing jobs.
-   `batchSize`: The maximum number of jobs to process in a single batch.
-   `batchFrequencyMilliseconds`: The frequency with which to process batches, in milliseconds.

### Methods

#### SubmitJob

```
public JobResult SubmitJob(Job job)
```

Submits a job to the batch processor.

Parameters:

-   `job`: The job to submit.

Returns:

A `JobResult` object that indicates whether the job was accepted or rejected.

#### ShutdownAsync

```
public async Task ShutdownAsync()
```

Jobs will no longer be accepted. All previously accepted jobs are processed in batches before returning.

#### Start

```
private void Start()
```

Starts the batch processor and begins processing jobs.

#### ProcessBatchAsync

```
private async Task ProcessBatchAsync()
```

Processes a batch of jobs.

#### GetBatch

```
private List<Job> GetBatch()
```

Gets a batch of jobs from the job queue. Batches of jobs are fetched in order of a given priority (`low`, `medium`, `high`) associated to a job. Job's default to a `low` priority.

### Properties

#### JobQueue

```
public PriorityQueue<Job, JobPriority> JobQueue { get; }
```

The queue of jobs waiting to be processed. This is a priority queue which is an array-backed quaternary min-heap. Each element is enqueued with an associated priority that determines the dequeue order.

### Exceptions

The `MicroBatching` class throws the following exceptions:

-   `ArgumentOutOfRangeException`: If the `batchSize` or `batchFrequencyMilliseconds` is not within the valid range.
-   `ArgumentNullException`: If the `batchProcessor` is null.
-   `ExternalException`: If the `batchProcessor` dependency throws any errors when processing batches.