using System.Collections.Generic;
using System.Threading;

namespace Lab4.SleepingBarber
{
    public class BarberShop
    {
        private readonly SemaphoreSlim customersWaiting = new SemaphoreSlim(0);
        private readonly SemaphoreSlim barberReady = new SemaphoreSlim(0, 1);
        private readonly object lockObj = new object();
        private readonly Queue<int> waiting = new Queue<int>();
        private readonly int maxSeats;
        private int customersServed = 0;
        private bool isRunning = true;

        public BarberShop(int maxSeats = 3)
        {
            this.maxSeats = maxSeats;
        }

        public void Start()
        {
            Thread barber = new Thread(BarberWork);
            barber.IsBackground = true;
            barber.Start();
        }

        public void Stop()
        {
            isRunning = false;
            customersWaiting.Release();
        }

        public bool TryAddCustomer()
        {
            lock (lockObj)
            {
                if (waiting.Count >= maxSeats)
                    return false;
                    
                int customerId = waiting.Count + 1;
                waiting.Enqueue(customerId);
                customersWaiting.Release();
                return true;
            }
        }

        private void BarberWork()
        {
            while (isRunning)
            {
                customersWaiting.Wait();
                
                if (!isRunning) break;
                
                int customer;
                lock (lockObj)
                {
                    if (waiting.Count == 0) continue;
                    customer = waiting.Dequeue();
                }
                
                Thread.Sleep(150);
                
                // Отмечаем как обслуженного
                lock (lockObj)
                {
                    customersServed++;
                }
                
                // Готов к следующему клиенту
                try
                {
                    barberReady.Release();
                }
                catch (SemaphoreFullException)
                {
                    // Игнорируем, если уже готов
                }
            }
        }

        public int GetServedCustomers() => customersServed;
        
        public int GetWaitingCount()
        {
            lock (lockObj)
            {
                return waiting.Count;
            }
        }
    }
}