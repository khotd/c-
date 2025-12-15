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
    
    // сериализация в строку
    public string SerializeToJson(Person person)
    {
        try
        {
            return JsonSerializer.Serialize(person, _jsonOptions);
        }
        catch (Exception ex)
        {
            Logger.LogError($"Ошибка сериализации: {ex.Message}");
            throw;
        }
    }
    
    // десериализация из строки
    public Person DeserializeFromJson(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<Person>(json, _jsonOptions);
        }
        catch (Exception ex)
        {
            Logger.LogError($"Ошибка десериализации: {ex.Message}");
            throw;
        }
    }
    
    // сохранение в файл (синхронно)
    public void SaveToFile(Person person, string filePath)
    {
        try
        {
            string json = SerializeToJson(person);
            File.WriteAllText(filePath, json);
        }
        catch (Exception ex)
        {
            Logger.LogError($"Ошибка сохранения в файл: {ex.Message}");
            throw;
        }
    }
    
    // загрузка из файла (синхронно)
    public Person LoadFromFile(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Файл не найден: {filePath}");
            
            string json = File.ReadAllText(filePath);
            return DeserializeFromJson(json);
        }
        catch (Exception ex)
        {
            Logger.LogError($"Ошибка загрузки из файла: {ex.Message}");
            throw;
        }
    }
    
    // сохранение в файл
    public async Task SaveToFileAsync(Person person, string filePath)
    {
        try
        {
            string json = SerializeToJson(person);
            await File.WriteAllTextAsync(filePath, json);
        }
        catch (Exception ex)
        {
            Logger.LogError($"Ошибка асинхронного сохранения: {ex.Message}");
            throw;
        }
    }
    
    // загрузка из файла
    public async Task<Person> LoadFromFileAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Файл не найден: {filePath}");
            
            string json = await File.ReadAllTextAsync(filePath);
            return DeserializeFromJson(json);
        }
        catch (Exception ex)
        {
            Logger.LogError($"Ошибка асинхронной загрузки: {ex.Message}");
            throw;
        }
    }
    
    // экспорт нескольких объектов в файл
    public void SaveListToFile(List<Person> people, string filePath)
    {
        try
        {
            string json = JsonSerializer.Serialize(people, _jsonOptions);
            File.WriteAllText(filePath, json);
        }
        catch (Exception ex)
        {
            Logger.LogError($"Ошибка сохранения списка: {ex.Message}");
            throw;
        }
    }
    
    // импорт из файла
    public List<Person> LoadListFromFile(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Файл не найден: {filePath}");
            
            string json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<List<Person>>(json, _jsonOptions);
        }
        catch (Exception ex)
        {
            Logger.LogError($"Ошибка загрузки списка: {ex.Message}");
            throw;
        }
    }
}