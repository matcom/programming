namespace MatCom.Tester;

using System;
using System.Diagnostics;
using System.Text.Json;

public enum ResultType { Ok, Wrong, Exception, TimeOut }

public class TestResult
{
    public string Description {get; set;}
    public int Ok { get; set; }
    public int Wrong { get; set; }
    public int Exception { get; set; }
    public int TimeOut { get; set; }
    public int Total { get; set; }

    public TestResult(string description)
    {
        Description = description;
    }

    public void Update(ResultType result)
    {
        switch (result)
        {
            case ResultType.Ok:
                Ok++;
                break;
            case ResultType.Wrong:
                Wrong++;
                break;
            case ResultType.Exception:
                Exception++;
                break;
            case ResultType.TimeOut:
                TimeOut++;
                break;
        }
    }
}

public class CaseCache<TIn, TEOut>
{
    public CaseCache(int seed, long time, TIn input, TEOut output)
    {
        Seed = seed;
        Time = time;
        Input = input;
        Output = output;
    }

    public int Seed { get; set; }
    public long Time { get; set; }
    public TIn Input { get; set; }
    public TEOut Output { get; set; }
}

public abstract class TesterBase<TArg, TIn, TOut, TEOut>
{
    public abstract TIn InputGenerator(int seed, TArg arg);

    public abstract TEOut OutputGenerator(TIn input);

    public abstract bool OutputChecker(TIn input, TOut output, TEOut expectedOutput);

    public List<(TIn, TEOut)> GenerateResponses(int baseSeed, TArg param, int numTests = 100)
    {
        var responses = new List<(TIn, TEOut)>();
        for (var s = 0; s < numTests; ++s)
        {
            var seed = baseSeed + s;
            var input = InputGenerator(seed, param);
            (var output, var time) = RunTask(OutputGenerator, input, default(TEOut));
            if (output is null)
            {
                Console.WriteLine($"Timeout generating output for seed {seed} and input {input}");
                continue;
            }
            // (seed, time, input, output);
            responses.Add((input, output));
        }
        return responses;
    }

    public TestResult? Test(List<(TIn, TEOut)> testCases, Func<TIn, TOut> func, string func_desc, int maxTimeOuts = 30)
    {
        var testResult = new TestResult(func_desc);

        for (int i = 0; i < testCases.Count; ++i)
        {
            var testCase = testCases[i];
            (var result, var time) = RunTask((input) =>
            {
                try
                {
                    return OutputChecker(input, func(input), testCase.Item2) ? ResultType.Ok : ResultType.Wrong;
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine(ex);
                    return ResultType.Exception;
                }
            }, testCase.Item1, ResultType.TimeOut);

            testResult.Update(result);

            Console.ForegroundColor = result == ResultType.Ok ? ConsoleColor.Green : ConsoleColor.Red;
            Console.Write($"Case {i + 1}: {result} ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"{time}s");
            Console.ResetColor();

            if (testResult.TimeOut > maxTimeOuts)
                break;
        }

        testResult.Total = testCases.Count;

        var percent = testResult.Ok * 100f / testResult.Total;
        Console.WriteLine($"Results: {testResult.Ok} de {testResult.Total} {percent}%");
        Console.ForegroundColor = testResult.Ok == testResult.Total ? ConsoleColor.Green : ConsoleColor.Red;
        Console.ResetColor();
        Console.WriteLine(testResult.Ok == testResult.Total ? "Success" : "Failure");
        return testResult;
    }

    private static (T?, long) RunTask<T>(Func<TIn, T> func, TIn input, T? onTimeOut, int seconds = 2)
    {
        var result = onTimeOut;
        var cancelToken = new CancellationTokenSource();
        var task = Task.Run(() => func(input));
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        if (task.Wait(new TimeSpan(seconds: seconds, hours: 0, minutes: 0)))
        {
            result = task.Result;
        }

        try { task.Dispose(); }
        catch(Exception ex) { System.Console.WriteLine(ex);}

        return (result, stopwatch.ElapsedMilliseconds);
    }
}
