using System.Collections.Concurrent;
using System.Threading;

namespace Lab4.ProducerConsumer
{
    public class ProducerConsumerBlockingCollection
    {
        private readonly BlockingCollection<int> buffer;
        private int producedCount = 0;
        private int consumedCount = 0;

        public ProducerConsumerBlockingCollection(int bufferSize)
        {
            buffer = new BlockingCollection<int>(bufferSize);
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
                buffer.Add(item);
                Interlocked.Increment(ref producedCount);
                item++;
                Thread.Sleep(100);
            }
        }

        private void Consumer(int id)
        {
            while (true)
            {
                buffer.Take();
                Interlocked.Increment(ref consumedCount);
                Thread.Sleep(150);
            }
        }

        public int GetProducedCount() => producedCount;
        public int GetConsumedCount() => consumedCount;
        public int GetBufferSize() => buffer.Count;
    }
}