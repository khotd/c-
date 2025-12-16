using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

public class PersonSerializer
{
    private readonly JsonSerializerOptions _jsonOptions;
    
    public PersonSerializer()
    {
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };
    }
    
    public string SerializeToJson(Person person)
    {
        return JsonSerializer.Serialize(person, _jsonOptions);
    }
    
    public Person? DeserializeFromJson(string json)
    {
        return JsonSerializer.Deserialize<Person>(json, _jsonOptions);
    }
    
    public void SaveToFile(Person person, string filePath)
    {
        string json = SerializeToJson(person);
        File.WriteAllText(filePath, json);
    }
    
    public Person LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Файл не найден: {filePath}");
        
        string json = File.ReadAllText(filePath);
        var person = DeserializeFromJson(json);
        
        if (person == null)
            throw new InvalidOperationException("Не удалось десериализовать объект из файла");
        
        return person;
    }
    
    public async Task SaveToFileAsync(Person person, string filePath)
    {
        string json = SerializeToJson(person);
        await File.WriteAllTextAsync(filePath, json);
    }
    
    public async Task<Person> LoadFromFileAsync(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Файл не найден: {filePath}");
        
        string json = await File.ReadAllTextAsync(filePath);
        var person = DeserializeFromJson(json);
        
        if (person == null)
            throw new InvalidOperationException("Не удалось десериализовать объект из файла");
        
        return person;
    }
    
    public void SaveListToFile(List<Person> people, string filePath)
    {
        string json = JsonSerializer.Serialize(people, _jsonOptions);
        File.WriteAllText(filePath, json);
    }
    
    public List<Person> LoadListFromFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Файл не найден: {filePath}");
        
        string json = File.ReadAllText(filePath);
        var people = JsonSerializer.Deserialize<List<Person>>(json, _jsonOptions);
        
        return people ?? new List<Person>();
    }
}