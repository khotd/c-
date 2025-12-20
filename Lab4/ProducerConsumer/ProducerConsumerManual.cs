using System.Collections.Generic;
using System.Threading;

namespace Lab4.ProducerConsumer
{
    public class ProducerConsumerManual
    {
        private readonly Queue<int> buffer = new Queue<int>();
        private readonly int maxSize;
        private readonly SemaphoreSlim empty;
        private readonly SemaphoreSlim full = new SemaphoreSlim(0, 100);
        private readonly object lockObj = new object();
        private int producedCount = 0;
        private int consumedCount = 0;

        public ProducerConsumerManual(int maxSize)
        {
            this.maxSize = maxSize;
            empty = new SemaphoreSlim(maxSize, maxSize);
        }

        public void Start(int producerCount = 1, int consumerCount = 1)
        {
            for (int i = 0; i < producerCount; i++)
            {
                int producerId = i;
                ThreadPool.QueueUserWorkItem(_ => Producer(producerId));
            }

            for (int i = 0; i < consumerCount; i++)
            {
                int consumerId = i;
                ThreadPool.QueueUserWorkItem(_ => Consumer(consumerId));
            }
        }

        private void Producer(int id)
        {
            int item = 0;
            while (true)
            {
                empty.Wait();
                lock (lockObj)
                {
                    buffer.Enqueue(item);
                    Interlocked.Increment(ref producedCount);
                }
                full.Release();
                item++;
                Thread.Sleep(100);
            }
        }

        private void Consumer(int id)
        {
            while (true)
            {
                full.Wait();
                lock (lockObj)
                {
                    buffer.Dequeue();
                    Interlocked.Increment(ref consumedCount);
                }
                empty.Release();
                Thread.Sleep(150);
            }
        }

        public int GetProducedCount() => producedCount;
        public int GetConsumedCount() => consumedCount;
        public int GetBufferSize()
        {
            lock (lockObj)
            {
                return buffer.Count;
            }
        }
    }
}