using System.Threading;
using Xunit;

namespace Lab4.Tests
{
    public class ProducerConsumerTests
    {
        [Fact]
        public void BlockingCollection_ShouldNotOverflowBuffer()
        {
            var pc = new ProducerConsumer.ProducerConsumerBlockingCollection(3);
            
            pc.Start(1, 1);
            Thread.Sleep(1000);
            
            int bufferSize = pc.GetBufferSize();
            Assert.True(bufferSize <= 3, $"Буфер не должен превышать 3. Текущий размер: {bufferSize}");
            
            Assert.True(pc.GetProducedCount() > 0, "Должны быть произведенные элементы");
            Assert.True(pc.GetConsumedCount() > 0, "Должны быть потребленные элементы");
        }

        [Fact]
        public void ManualImplementation_ShouldMaintainBalance()
        {
            var pc = new ProducerConsumer.ProducerConsumerManual(5);
            
            pc.Start(1, 1);
            Thread.Sleep(800);
            
            int produced = pc.GetProducedCount();
            int consumed = pc.GetConsumedCount();
            
            Assert.True(consumed <= produced, 
                $"Потреблено ({consumed}) не должно быть больше произведенного ({produced})");
            
            int diff = produced - consumed;
            Assert.True(diff <= 5, 
                $"Разница между произведенным и потребленным ({diff}) должна быть ≤ размеру буфера (5)");
        }

        [Fact]
        public void BothImplementations_ShouldWork()
        {
            var pc1 = new ProducerConsumer.ProducerConsumerBlockingCollection(3);
            var pc2 = new ProducerConsumer.ProducerConsumerManual(3);
            
            pc1.Start(1, 1);
            pc2.Start(1, 1);
            Thread.Sleep(1000);
            
            Assert.True(pc1.GetProducedCount() > 0, 
                $"BlockingCollection должен производить. Произведено: {pc1.GetProducedCount()}");
            Assert.True(pc2.GetProducedCount() > 0, 
                $"Manual должен производить. Произведено: {pc2.GetProducedCount()}");
        }
    }
}