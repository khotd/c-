using Xunit;

public class PersonTests
{
    [Fact]
    public void Person_Create_Works()
    {
        var person = new Person("Иван", "Иванов", 25, "aaaa@test.com", "123");
        
        Assert.Equal("Иван", person.FirstName);
        Assert.Equal("Иванов", person.LastName);
        Assert.Equal(25, person.Age);
    }
    
    [Fact]
    public void Person_FullName_Works()
    {
        var person = new Person("Анна", "Петрова", 30, "bbbb@test.com", "456");
        
        Assert.Equal("Анна Петрова", person.FullName);
    }
    
    [Fact]
    public void Person_IsAdult_Works()
    {
        var adult = new Person("Иван", "Иванов", 18, "test@test.com", "111");
        var child = new Person("Петя", "Петров", 15, "test@test.com", "222");
        
        Assert.True(adult.IsAdult);
        Assert.False(child.IsAdult);
    }
}