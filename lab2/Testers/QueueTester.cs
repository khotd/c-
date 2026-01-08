using System;
using System.Diagnostics;

namespace CollectionsPerformanceLab;

public class QueueTester
{
    private const int Size = 100000;
    private const int Iterations = 5;
    
    public void Test()
    {
        Console.WriteLine("\n Queue<T>");
        
        var queue = new Queue<int>(Size);
        for (int i = 0; i < Size; i++) queue.Enqueue(i);
        
        Measure("Enqueue (добавление)", () => new Queue<int>(queue).Enqueue(999999));
        Measure("Dequeue (удаление)", () => new Queue<int>(queue).Dequeue());
        Measure("Поиск элемента", () => new Queue<int>(queue).Contains(Size/2));
    }
    
    private void Measure(string name, Action test)
    {
        long total = 0;
        for (int i = 0; i < Iterations; i++)
        {
            var sw = Stopwatch.StartNew();
            test();
            sw.Stop();
            total += sw.ElapsedTicks;
        }
        Console.WriteLine($"{name}: {total/Iterations} тиков");
    }
}