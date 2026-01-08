using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Lab4.Tests
{
    public class DiningPhilosophersTests
    {
        [Fact]
        public void PhilosopherWithDeadlock_ShouldEventuallyDeadlock()
        {
            var philosophers = new DiningPhilosophers.PhilosopherWithDeadlock[5];
            var tasks = new Task[5];

            for (int i = 0; i < 5; i++)
            {
                philosophers[i] = new DiningPhilosophers.PhilosopherWithDeadlock(i);
                int index = i;
                tasks[i] = Task.Run(() => philosophers[index].Run());
            }

            Thread.Sleep(500);

            bool atLeastOneAte = false;
            for (int i = 0; i < 5; i++)
            {
                if (philosophers[i].GetEatCount() > 0)
                {
                    atLeastOneAte = true;
                    break;
                }
            }

            foreach (var philosopher in philosophers)
            {
                philosopher.Stop();
            }

            Assert.True(atLeastOneAte, "Хотя бы один философ должен был поесть");
            
            int totalEats = 0;
            foreach (var philosopher in philosophers)
            {
                totalEats += philosopher.GetEatCount();
            }
            Assert.True(totalEats < 20, $"Deadlock должен ограничивать количество приемов пищи. Было: {totalEats}");
        }

        [Fact]
        public void PhilosopherFixed_ShouldWorkWithoutDeadlock()
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            var philosophers = new DiningPhilosophers.PhilosopherFixed[5];
            var tasks = new Task[5];

            for (int i = 0; i < 5; i++)
            {
                philosophers[i] = new DiningPhilosophers.PhilosopherFixed(i, cts.Token);
                int index = i;
                tasks[i] = Task.Run(() => philosophers[index].Run());
            }

            Thread.Sleep(2500);

            int totalEats = 0;
            foreach (var philosopher in philosophers)
            {
                int eats = philosopher.GetEatCount();
                totalEats += eats;
                Assert.True(eats > 0, $"Философ {Array.IndexOf(philosophers, philosopher)} должен был поесть");
            }
            
            Assert.True(totalEats >= 10, $"Всего должно быть много приемов пищи. Было: {totalEats}");
        }
    }
}