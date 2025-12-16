using Xunit;

public class PersonSerializerTests
{
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