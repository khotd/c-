using System;
class Program
{
    static void Main()
    {
        object test = "Hello";
        Type t = test.GetType();
        Console.WriteLine($"Тип: {t}");
        
        Console.WriteLine("Свойства:");
        foreach (var pr in typeof(test).GetProperty(test))
        {
            Console.WriteLine($"Свойство: {pr.Name}, тип: {pr.Type}");
        }

        foreach (var m in typeof(test).GetMethod(test))
        {
            Console.WriteLine($"Метод {m.Name}");
        }
    }
}

