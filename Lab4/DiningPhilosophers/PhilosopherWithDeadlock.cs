using System.Threading;

namespace Lab4.DiningPhilosophers
{
    public class PhilosopherWithDeadlock
    {
        private static readonly object[] Forks = new object[5];
        private readonly int id;
        private int eatCount = 0;
        private bool isRunning = true;

        static PhilosopherWithDeadlock()
        {
            for (int i = 0; i < 5; i++) Forks[i] = new object();
        }

        public PhilosopherWithDeadlock(int id)
        {
            this.id = id;
        }

        public void Run()
        {
            while (isRunning)
            {
                Think();
                PickForks();
                Eat();
                PutForks();
            }
        }

        public void Stop()
        {
            isRunning = false;
        }

        private void Think() => Thread.Sleep(50);
        
        private void Eat()
        {
            Thread.Sleep(50);
            eatCount++;
        }

        private void PickForks()
        {
            int left = id;
            int right = (id + 1) % 5;

            lock (Forks[left])
            {
                Thread.Sleep(20);
                lock (Forks[right])
                {
                    // кушат
                }
            }
        }

        private void PutForks() { }

        public int GetEatCount() => eatCount;
    }
}