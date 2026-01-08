using System;
using System.Diagnostics;

namespace CollectionsPerformanceLab;

public class StackTester
{
    private const int Size = 100000;
    private const int Iterations = 5;
    
    public void Test()
    {
        Console.WriteLine("\n Stack<T>");
        
        var stack = new Stack<int>(Size);
        for (int i = 0; i < Size; i++) stack.Push(i);
        
        Measure("Push (добавление)", () => new Stack<int>(stack).Push(999999));
        Measure("Pop (удаление)", () => new Stack<int>(stack).Pop());
        Measure("Поиск элемента", () => new Stack<int>(stack).Contains(Size/2));
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