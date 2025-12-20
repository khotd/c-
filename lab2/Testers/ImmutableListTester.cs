using System;
using System.Diagnostics;
using System.Collections.Immutable;

namespace CollectionsPerformanceLab;

public class ImmutableListTester
{
    private const int Size = 100000;
    private const int Iterations = 5;
    
    public void Test()
    {
        Console.WriteLine("\n ImmutableList<T>");
        
        var builder = ImmutableList.Create<int>().ToBuilder();
        for (int i = 0; i < Size; i++) builder.Add(i);
        var list = builder.ToImmutable();
        
        Measure("Добавление в конец", () => list.Add(999999));
        Measure("Добавление в начало", () => list.Insert(0, 999999));
        Measure("Поиск элемента", () => list.Contains(Size/2));
        Measure("Получение по индексу", () => { var x = list[Size/2]; });
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