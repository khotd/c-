using System;
using System.Diagnostics;

namespace CollectionsPerformanceLab;

public class ListTester
{
    private const int Size = 100000;
    private const int Iterations = 5;
    
    public void Test()
    {
        Console.WriteLine("\n List<T>");
        
        var list = new List<int>(Size);
        for (int i = 0; i < Size; i++) list.Add(i);
        
        Measure("Добавление в конец", () => new List<int>(list).Add(999999));
        Measure("Добавление в начало", () => new List<int>(list).Insert(0, 999999));
        Measure("Добавление в середину", () => new List<int>(list).Insert(Size/2, 999999));
        Measure("Удаление из конца", () => new List<int>(list).RemoveAt(list.Count-1));
        Measure("Удаление из начала", () => new List<int>(list).RemoveAt(0));
        Measure("Удаление из середины", () => new List<int>(list).RemoveAt(Size/2));
        Measure("Поиск элемента", () => new List<int>(list).Contains(Size/2));
        Measure("Получение по индексу", () => { var x = new List<int>(list)[Size/2]; });
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