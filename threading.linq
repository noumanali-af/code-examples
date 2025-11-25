<Query Kind="Program" />

void Main()

{
    var process = new UnsafeCounter();

    Console.WriteLine("\nTesting with 'lock' synchronization...");
    process.Execute(process.GiveMeLockCounter());

    Console.WriteLine("\nTesting with 'Interlocked.Increment' synchronization...");
    process.Execute(process.GiveMeInterLockCounter());

    Console.WriteLine("\nTesting with NO synchronization (Unsafe)...");
    process.Execute(process.GiveMeUnsafeCounter());

}


public class UnsafeCounter
{
	public delegate void IncrementActionDelegate();

	// 1. Shared Resource
	private int _counter = 0;
	private const int Iterations = 900000;
	private readonly object _lock = new object();



	public void Execute(IncrementActionDelegate counterLogic)
	{
		ResetCounter();

		// 2. Multiple Threads
		Thread t1 = new Thread(() => counterLogic());
		Thread t2 = new Thread(() => counterLogic());

		t1.Start();
		t2.Start();



		t1.Join(); // Wait for t1 to finish
		t2.Join(); // Wait for t2 to finish

		// FIX 2: Added a check for clarity
		int expectedResult = Iterations * 2;
		string status = (_counter == expectedResult) ? "✅ SUCCESS (Thread-Safe)" : "❌ FAILURE (Race Condition)";



		Console.WriteLine($"Expected result: {expectedResult}");
		Console.WriteLine($"Actual result: {_counter}");
		Console.WriteLine($"Status: {status}");
	}



	public void ResetCounter()
	{
		_counter = 0;
	}



	public IncrementActionDelegate GiveMeLockCounter()
	{
		return IncrementCounter_lock;
	}



	public IncrementActionDelegate GiveMeInterLockCounter()
	{
		return IncrementCounter_interlock;
	}

	public IncrementActionDelegate GiveMeUnsafeCounter()
	{
		return IncrementCounter_unsafe;
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

	private void IncrementCounter_unsafe()
	{
		for (int i = 0; i < Iterations; i++)
		{
			_counter++;
		}
	}

}