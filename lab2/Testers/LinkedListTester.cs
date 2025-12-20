using System;
using System.Diagnostics;

namespace CollectionsPerformanceLab;

public class LinkedListTester
{
    private const int Size = 100000;
    private const int Iterations = 5;
    
    public void Test()
    {
        Console.WriteLine("\n LinkedList<T>");
        
        var list = new LinkedList<int>();
        for (int i = 0; i < Size; i++) list.AddLast(i);
        
        Measure("Добавление в конец", () => new LinkedList<int>(list).AddLast(999999));
        Measure("Добавление в начало", () => new LinkedList<int>(list).AddFirst(999999));
        Measure("Удаление из конца", () => new LinkedList<int>(list).RemoveLast());
        Measure("Удаление из начала", () => new LinkedList<int>(list).RemoveFirst());
        Measure("Поиск элемента", () => new LinkedList<int>(list).Contains(Size/2));
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