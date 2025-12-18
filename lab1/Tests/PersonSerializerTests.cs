using System.IO;
using Xunit;
using Lab1.Models;
using Lab1.Serializers;

namespace Lab1.Tests
{
    public class PersonSerializerTests
    {
        [Fact]
        public void Serializer_Serialize_Works()
        {
            var person = new Person("Диана", "Хотеева", 25, "t@test.com", "123");
            var serializer = new PersonSerializer();
            string json = serializer.SerializeToJson(person);
            Assert.Contains("Диана", json);
            Assert.Contains("Хотеева", json);
        }
        
        [Fact]
        public void Serializer_Deserialize_Works()
        {
            string json = @"{""FirstName"":""Мария"",""LastName"":""Смирнова"",""Age"":28}";
            var serializer = new PersonSerializer();
            var person = serializer.DeserializeFromJson(json);
            Assert.Equal("Мария", person.FirstName);
            Assert.Equal("Смирнова", person.LastName);
        }
        
        [Fact]
        public void Serializer_SaveLoad_Works()
        {
            var person = new Person("Диана", "ААААА", 30, "aaaaa@aaa.com", "555");
            var serializer = new PersonSerializer();
            string filename = "test123.json";
            
            serializer.SaveToFile(person, filename);
            var loaded = serializer.LoadFromFile(filename);
            
            Assert.Equal("Диана", loaded.FirstName);
            Assert.Equal("AAAA", loaded.LastName);
            
            File.Delete(filename);
        }
    }
}