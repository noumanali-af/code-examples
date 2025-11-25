<Query Kind="Program">
  <NuGetReference Version="0.13.8">BenchmarkDotNet</NuGetReference>
  <Namespace>BenchmarkDotNet.Attributes</Namespace>
  <Namespace>BenchmarkDotNet.Jobs</Namespace>
  <Namespace>BenchmarkDotNet.Running</Namespace>
</Query>


void Main()
{
	Console.WriteLine("--- Running Benchmark (Ensure Release Mode) ---");

	// This command executes all methods decorated with [Benchmark]
	BenchmarkRunner.Run<CounterBenchmark>();
}

// Define the job configuration
[SimpleJob(RuntimeMoniker.Net80)]
// Use the MemoryDiagnoser to ensure no hidden allocation differences
[MemoryDiagnoser]
public class CounterBenchmark
{
	// --- Configuration ---

	// Shared Resource & Lock
	private int _counter;
	private readonly object _lock = new object();
	// Use a large iteration count to make the overhead measurable
	private const int Iterations = 5;

	// --- Setup and Cleanup ---

	// [IterationCleanup] runs after each measurement iteration to reset the state
	[IterationCleanup]
	public void Cleanup()
	{
		_counter = 0; // Ensures the counter is reset for the next measurement
	}

	// --- Benchmarks ---

	[Benchmark(Description = "0. Unsafe (Race Condition)")]
	public int RunUnsafe()
	{
		// Thread setup is repeated inside the benchmark method
		Thread t1 = new Thread(IncrementCounter_unsafe);
		Thread t2 = new Thread(IncrementCounter_unsafe);

		t1.Start();
		t2.Start();

		t1.Join();
		t2.Join();

		// Return the result for validation (though BDN ignores the value)
		return _counter;
	}

	[Benchmark(Description = "1. Lock Keyword")]
	public int RunLock()
	{
		Thread t1 = new Thread(IncrementCounter_lock);
		Thread t2 = new Thread(IncrementCounter_lock);

		t1.Start();
		t2.Start();

		t1.Join();
		t2.Join();
		return _counter;
	}

	[BenchmarkCategory("ThreadSafe")]
	[Benchmark(Baseline = true, Description = "2. Interlocked.Increment (Baseline)")] // Set this as the reference point for speed comparison
	public int RunInterlocked()
	{
		Thread t1 = new Thread(IncrementCounter_interlock);
		Thread t2 = new Thread(IncrementCounter_interlock);

		t1.Start();
		t2.Start();

		t1.Join();
		t2.Join();
		return _counter;
	}

	// --- Core Increment Methods ---

	private void IncrementCounter_unsafe()
	{
		for (int i = 0; i < Iterations; i++)
		{
			_counter++;
		}
	}

	private void IncrementCounter_lock()
	{
		for (int i = 0; i < Iterations; i++)
		{
			lock (_lock)
			{
				_counter++;
			}
		}
	}

	private void IncrementCounter_interlock()
	{
		for (int i = 0; i < Iterations; i++)
		{
			Interlocked.Increment(ref _counter);
		}
	}
}