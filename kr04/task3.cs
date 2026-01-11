using System;
using System.Threading;

class Program
{
    static void Main()
    {
        int n = 10;
        Thread thread = new Thread(() => {
            int sum = 0;
            for (int i = 1; i <= n; i++)
                sum += i;
            Console.WriteLine($"Сумма: {sum}");
        });
        thread.Start();
        thread.Join();
    }
}