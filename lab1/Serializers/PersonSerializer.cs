using System.Text.Json;
using System.IO;
using System.Collections.Generic;
using Lab1.Models;

namespace Lab1.Serializers
{
    public class PersonSerializer
    {
        private readonly JsonSerializerOptions _options = new()
        {
            WriteIndented = true
        };
        
        public string SerializeToJson(Person person) 
            => JsonSerializer.Serialize(person, _options);
        
        public Person DeserializeFromJson(string json) 
            => JsonSerializer.Deserialize<Person>(json, _options);
        
        public void SaveToFile(Person person, string filePath) 
            => File.WriteAllText(filePath, SerializeToJson(person));
        
        public Person LoadFromFile(string filePath) 
            => DeserializeFromJson(File.ReadAllText(filePath));
    }
}