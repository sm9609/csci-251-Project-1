// CSCI 251 - Project 1: Race Condition Detective
// Test runner for race condition detection

namespace RaceConditionDetective;

/// <summary>
/// Wrapper for int values to use with BuggyQueue (which requires reference types).
/// </summary>
public record IntItem(int Value);

public class Program
{
    private const int TrialsPerTest = 10;
    private const int ThreadCount = 8;
    private const int OperationsPerThread = 1000;

    public static void Main(string[] args)
    {
        Console.WriteLine("=== Race Condition Detective Test Suite ===\n");

        var results = new Dictionary<string, (int passed, int total, int points)>();

        results["BuggyBank"] = TestBuggyBank();
        results["BuggyCounter"] = TestBuggyCounter();
        results["BuggyCache"] = TestBuggyCache();
        results["BuggyLogger"] = TestBuggyLogger();
        results["BuggyQueue"] = TestBuggyQueue();

        PrintSummary(results);
    }

    static (int passed, int total, int points) TestBuggyBank()
    {
        Console.WriteLine("Testing BuggyBank...");
        int passed = 0;
        const int maxPoints = 2;

        for (int trial = 1; trial <= TrialsPerTest; trial++)
        {
            var bank1 = new BuggyBank(10000m);
            var bank2 = new BuggyBank(10000m);
            decimal expectedTotal = bank1.Balance + bank2.Balance;

            var threads = new Thread[ThreadCount];
            for (int i = 0; i < ThreadCount; i++)
            {
                int threadId = i;
                threads[i] = new Thread(() =>
                {
                    var rng = new Random(threadId);
                    for (int j = 0; j < OperationsPerThread; j++)
                    {
                        decimal amount = rng.Next(1, 100);
                        if (rng.Next(2) == 0)
                            bank1.Transfer(bank2, amount);
                        else
                            bank2.Transfer(bank1, amount);
                    }
                });
            }

            foreach (var t in threads) t.Start();
            foreach (var t in threads) t.Join();

            decimal actualTotal = bank1.Balance + bank2.Balance;
            bool success = actualTotal == expectedTotal;

            Console.WriteLine($"  Trial {trial}/{TrialsPerTest}: {(success ? "PASS" : "FAIL")} " +
                $"(expected {expectedTotal}, got {actualTotal})");

            if (success) passed++;
        }

        int points = passed == TrialsPerTest ? maxPoints : 0;
        Console.WriteLine($"  Result: {passed}/{TrialsPerTest} trials passed - " +
            $"{(passed == TrialsPerTest ? "FIXED!" : "STILL BUGGY")}\n");

        return (passed, TrialsPerTest, points);
    }

    static (int passed, int total, int points) TestBuggyCounter()
    {
        Console.WriteLine("Testing BuggyCounter...");
        int passed = 0;
        const int maxPoints = 2;

        for (int trial = 1; trial <= TrialsPerTest; trial++)
        {
            var counter = new BuggyCounter();
            int expectedCount = ThreadCount * OperationsPerThread;

            var threads = new Thread[ThreadCount];
            for (int i = 0; i < ThreadCount; i++)
            {
                threads[i] = new Thread(() =>
                {
                    for (int j = 0; j < OperationsPerThread; j++)
                    {
                        counter.Increment();
                    }
                });
            }

            foreach (var t in threads) t.Start();
            foreach (var t in threads) t.Join();

            bool success = counter.Count == expectedCount;

            Console.WriteLine($"  Trial {trial}/{TrialsPerTest}: {(success ? "PASS" : "FAIL")} " +
                $"(expected {expectedCount}, got {counter.Count})");

            if (success) passed++;
        }

        int points = passed == TrialsPerTest ? maxPoints : 0;
        Console.WriteLine($"  Result: {passed}/{TrialsPerTest} trials passed - " +
            $"{(passed == TrialsPerTest ? "FIXED!" : "STILL BUGGY")}\n");

        return (passed, TrialsPerTest, points);
    }

    static (int passed, int total, int points) TestBuggyCache()
    {
        Console.WriteLine("Testing BuggyCache...");
        int passed = 0;
        const int maxPoints = 3;

        for (int trial = 1; trial <= TrialsPerTest; trial++)
        {
            var cache = new BuggyCache();
            string testKey = "test-key";

            var threads = new Thread[ThreadCount];
            var results = new int[ThreadCount];

            for (int i = 0; i < ThreadCount; i++)
            {
                int threadId = i;
                threads[i] = new Thread(() =>
                {
                    results[threadId] = cache.GetOrCompute(testKey);
                });
            }

            foreach (var t in threads) t.Start();
            foreach (var t in threads) t.Join();

            // All threads should get the same value
            bool sameValues = results.All(r => r == results[0]);
            // Computation should only happen once
            bool computedOnce = cache.ComputeCount == 1;
            bool success = sameValues && computedOnce;

            Console.WriteLine($"  Trial {trial}/{TrialsPerTest}: {(success ? "PASS" : "FAIL")} " +
                $"(computed {cache.ComputeCount} time(s), values consistent: {sameValues})");

            if (success) passed++;
            cache.Clear();
        }

        int points = passed == TrialsPerTest ? maxPoints : 0;
        Console.WriteLine($"  Result: {passed}/{TrialsPerTest} trials passed - " +
            $"{(passed == TrialsPerTest ? "FIXED!" : "STILL BUGGY")}\n");

        return (passed, TrialsPerTest, points);
    }

    static (int passed, int total, int points) TestBuggyLogger()
    {
        Console.WriteLine("Testing BuggyLogger...");
        int passed = 0;
        const int maxPoints = 4;

        for (int trial = 1; trial <= TrialsPerTest; trial++)
        {
            var logger = new BuggyLogger();
            int messagesPerThread = 100;
            int expectedMessages = ThreadCount * messagesPerThread;
            bool hadException = false;

            var flushThread = logger.StartAutoFlush(10);

            var threads = new Thread[ThreadCount];
            for (int i = 0; i < ThreadCount; i++)
            {
                int threadId = i;
                threads[i] = new Thread(() =>
                {
                    try
                    {
                        for (int j = 0; j < messagesPerThread; j++)
                        {
                            logger.Log($"Thread {threadId} Message {j}");
                        }
                    }
                    catch
                    {
                        hadException = true;
                    }
                });
            }

            foreach (var t in threads) t.Start();
            foreach (var t in threads) t.Join();

            logger.Stop();
            flushThread.Join(1000);

            // Count actual logged messages
            int actualMessages = 0;
            foreach (var flushed in logger.FlushedMessages)
            {
                actualMessages += flushed.Split('\n', StringSplitOptions.RemoveEmptyEntries).Length;
            }

            bool success = !hadException && actualMessages == expectedMessages;

            Console.WriteLine($"  Trial {trial}/{TrialsPerTest}: {(success ? "PASS" : "FAIL")} " +
                $"(expected {expectedMessages} messages, got {actualMessages}, exception: {hadException})");

            if (success) passed++;
            logger.Reset();
        }

        int points = passed == TrialsPerTest ? maxPoints : 0;
        Console.WriteLine($"  Result: {passed}/{TrialsPerTest} trials passed - " +
            $"{(passed == TrialsPerTest ? "FIXED!" : "STILL BUGGY")}\n");

        return (passed, TrialsPerTest, points);
    }

    static (int passed, int total, int points) TestBuggyQueue()
    {
        Console.WriteLine("Testing BuggyQueue...");
        int passed = 0;
        const int maxPoints = 4;

        for (int trial = 1; trial <= TrialsPerTest; trial++)
        {
            var queue = new BuggyQueue<IntItem>();
            int itemsPerProducer = 500;
            int producerCount = ThreadCount / 2;
            int consumerCount = ThreadCount / 2;
            int expectedItems = producerCount * itemsPerProducer;

            var produced = new List<int>();
            var consumed = new List<int>();
            var producedLock = new object();
            var consumedLock = new object();
            bool hadException = false;
            bool hadNull = false;
            bool timedOut = false;

            var producers = new Thread[producerCount];
            var consumers = new Thread[consumerCount];

            for (int i = 0; i < producerCount; i++)
            {
                int producerId = i;
                producers[i] = new Thread(() =>
                {
                    try
                    {
                        for (int j = 0; j < itemsPerProducer; j++)
                        {
                            int value = producerId * 10000 + j;
                            queue.Enqueue(new IntItem(value));
                            lock (producedLock) produced.Add(value);
                        }
                    }
                    catch { hadException = true; }
                });
            }

            for (int i = 0; i < consumerCount; i++)
            {
                consumers[i] = new Thread(() =>
                {
                    try
                    {
                        while (true)
                        {
                            var item = queue.Dequeue();
                            if (item == null) break;
                            lock (consumedLock) consumed.Add(item.Value);
                        }
                    }
                    catch { hadException = true; }
                });
            }

            foreach (var t in producers) t.Start();
            foreach (var t in consumers) t.Start();

            foreach (var t in producers) t.Join();
            queue.Complete();

            // Wait for consumers with timeout
            foreach (var t in consumers)
            {
                if (!t.Join(5000))
                {
                    timedOut = true;
                }
            }

            bool success = !hadException && !hadNull && !timedOut &&
                           produced.Count == expectedItems &&
                           consumed.Count == expectedItems &&
                           new HashSet<int>(produced).SetEquals(consumed);

            Console.WriteLine($"  Trial {trial}/{TrialsPerTest}: {(success ? "PASS" : "FAIL")} " +
                $"(produced {produced.Count}, consumed {consumed.Count}, " +
                $"exception: {hadException}, timeout: {timedOut})");

            if (success) passed++;
            queue.Reset();
        }

        int points = passed == TrialsPerTest ? maxPoints : 0;
        Console.WriteLine($"  Result: {passed}/{TrialsPerTest} trials passed - " +
            $"{(passed == TrialsPerTest ? "FIXED!" : "STILL BUGGY")}\n");

        return (passed, TrialsPerTest, points);
    }

    static void PrintSummary(Dictionary<string, (int passed, int total, int points)> results)
    {
        Console.WriteLine("========================================");
        Console.WriteLine("                SUMMARY");
        Console.WriteLine("========================================\n");

        int totalPoints = 0;
        int maxPoints = 0;

        var pointValues = new Dictionary<string, int>
        {
            ["BuggyBank"] = 2,
            ["BuggyCounter"] = 2,
            ["BuggyCache"] = 3,
            ["BuggyLogger"] = 4,
            ["BuggyQueue"] = 4
        };

        foreach (var (name, (passed, total, points)) in results)
        {
            int max = pointValues[name];
            string status = passed == total ? "FIXED" : "BUGGY";
            Console.WriteLine($"  {name,-15} {status,-8} ({points}/{max} points)");
            totalPoints += points;
            maxPoints += max;
        }

        Console.WriteLine();
        Console.WriteLine($"  Code Score: {totalPoints}/{maxPoints} points");
        Console.WriteLine();
        Console.WriteLine("  Note: Written analysis (ANALYSIS.md) is worth 10 additional points.");
        Console.WriteLine("        Total possible: 25 points");
    }
}
