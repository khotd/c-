using System;

class Program
{
    static void Main()
    {
        object obj = new object();
        Console.WriteLine("Объект создан");
        // ничего не удаляет
        GC.Collect();
        // удаляем ссылку на объект
        obj = null;
        // удаляет объект
        GC.Collect();
    }
}