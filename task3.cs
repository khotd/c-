class Program
{
    static void Main()
    {
        int n = 10;
        int sum = 0;
        Thread newt = new Thread(n, sum)
        {
            for (int i = 1; i < n; i++)
            {
                sum += i;
            }
            Console.WriteLine($"Сумма {sum}");
        }
    }
}