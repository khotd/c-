using System.Threading;

namespace Lab4.DiningPhilosophers
{
    public class PhilosopherFixed
    {
        private static readonly SemaphoreSlim Table = new SemaphoreSlim(4, 4);
        private static readonly object[] Forks = new object[5];
        private readonly int id;
        private readonly CancellationToken cancellationToken;
        private int eatCount = 0;

        public PhilosopherFixed(int id, CancellationToken cancellationToken)
        {
            this.id = id;
            this.cancellationToken = cancellationToken;
            if (id == 0)
            {
                for (int i = 0; i < 5; i++) Forks[i] = new object();
            }
        }

        public void Run()
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Think();
                PickForks();
                Eat();
                PutForks();
            }
        }

        private void Think() => Thread.Sleep(50);
        
        private void Eat()
        {
            Thread.Sleep(50);
            Interlocked.Increment(ref eatCount);
        }

        private void PickForks()
        {
            Table.Wait();

            object first = Forks[id];
            object second = Forks[(id + 1) % 5];

            if (id % 2 == 0)
            {
                lock (first) lock (second) { }
            }
            else
            {
                lock (second) lock (first) { }
            }
        }

        private void PutForks()
        {
            Table.Release();
        }

        public int GetEatCount() => eatCount;
    }
}