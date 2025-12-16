using Xunit;

public class SimpleTests
{
    // ТЕСТЫ ДЛЯ PERSON
    
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
    
    // ТЕСТЫ ДЛЯ SERIALIZER
    
    [Fact]
    public void Serializer_Serialize_Works()
    {
        var person = new Person("Иван", "Иванов", 25, "aaaa@test.com", "123");
        var serializer = new PersonSerializer();
        
        string json = serializer.SerializeToJson(person);
        
        Assert.Contains("Иван", json);
        Assert.Contains("Иванов", json);
    }
    
    [Fact]
    public void Serializer_Deserialize_Works()
    {
        string json = @"{""FirstName"":""Мария"",""LastName"":""Смирнова"",""Age"":28}";
        var serializer = new PersonSerializer();
        
        var person = serializer.DeserializeFromJson(json);
        
        Assert.Equal("Мария", person.FirstName);
        Assert.Equal("Смирнова", person.LastName);
        Assert.Equal(28, person.Age);
    }
    
    [Fact]
    public void Serializer_SaveLoad_Works()
    {
        var person = new Person("Тест", "Тестов", 30, "helpme@test.com", "555");
        var serializer = new PersonSerializer();
        string filename = "test123.json";
        
        // Сохраняем
        serializer.SaveToFile(person, filename);
        
        // Загружаем
        var loaded = serializer.LoadFromFile(filename);
        
        // Проверяем
        Assert.Equal("Тест", loaded.FirstName);
        Assert.Equal("Тестов", loaded.LastName);
        
        // Убираем файл
        System.IO.File.Delete(filename);
    }
}