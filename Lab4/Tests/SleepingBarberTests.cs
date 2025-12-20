using System.Threading;
using Xunit;

namespace Lab4.Tests
{
    public class SleepingBarberTests
    {
        [Fact]
        public void BarberShop_ShouldServeCustomers()
        {
            var barberShop = new SleepingBarber.BarberShop(3);
            barberShop.Start();
            Thread.Sleep(100);

            int accepted = 0;
            for (int i = 0; i < 3; i++)
            {
                if (barberShop.TryAddCustomer())
                {
                    accepted++;
                }
                Thread.Sleep(300);
            }

            Thread.Sleep(1000);

            Assert.Equal(3, accepted);
            Assert.Equal(3, barberShop.GetServedCustomers());
            
            barberShop.Stop();
        }

        [Fact]
        public void BarberShop_ShouldRejectCustomersWhenFull()
        {
            var barberShop = new SleepingBarber.BarberShop(1);
            barberShop.Start();
            Thread.Sleep(100);

            int accepted = 0;
            int rejected = 0;
            
            if (barberShop.TryAddCustomer())
                accepted++;
            else
                rejected++;
            
            Thread.Sleep(50);
            
            if (barberShop.TryAddCustomer())
                accepted++;
            else
                rejected++;

            Assert.Equal(1, accepted);
            Assert.Equal(1, rejected);
            
            barberShop.Stop();
        }
    }
}