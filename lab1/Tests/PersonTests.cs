using Xunit;
using Lab1.Models;

namespace Lab1.Tests
{
    public class PersonTests
    {
        [Fact]
        public void Person_Create_Works()
        {
            var person = new Person("Диана", "Хотеева", 25, "aa@test.com", "123");
            Assert.Equal("Диана", person.FirstName);
            Assert.Equal("Хотеева", person.LastName);
        }
        
        [Fact]
        public void Person_FullName_Works()
        {
            var person = new Person("Анна", "Петрова", 30, "t@test.com", "456");
            Assert.Equal("Анна Петрова", person.FullName);
        }
        
        [Fact]
        public void Person_IsAdult_Works()
        {
            var adult = new Person("Арина", "Яматина", 18, "aaa@test.com", "111");
            var child = new Person("Соня", "Мурина", 15, "sss@test.com", "222");
            Assert.True(adult.IsAdult);
            Assert.False(child.IsAdult);
        }
    }
}