using System;
using System.Collections.Generic;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Тестирование PersonSerializer");
        
        var serializer = new PersonSerializer();
        var person = new Person("Диана", "Хотеева", 18, "mail@example.com", "+7-999-999-99-99");
        
        // Сериализация в строку
        Console.WriteLine("1. SerializeToJson:");
        string json = serializer.SerializeToJson(person);
        Console.WriteLine(json);
        
        // Десериализация из строки
        Console.WriteLine("\n2. DeserializeFromJson:");
        var person2 = serializer.DeserializeFromJson(json);
        Console.WriteLine($"Десериализован: {person2.FullName}");
        
        // Сохранение в файл (синхронно)
        Console.WriteLine("\n3. SaveToFile:");
        serializer.SaveToFile(person, "test1.json");
        Console.WriteLine("Файл сохранен");
        
        // Загрузка из файла (синхронно)
        Console.WriteLine("\n4. LoadFromFile:");
        var person3 = serializer.LoadFromFile("test1.json");
        Console.WriteLine($"Загружен: {person3.FullName}");
        
        // Сохранение в файл (асинхронно)
        Console.WriteLine("\n5. SaveToFileAsync:");
        await serializer.SaveToFileAsync(person, "test2.json");
        Console.WriteLine("Файл сохранен асинхронно");
        
        // Загрузка из файла (асинхронно)
        Console.WriteLine("\n6. LoadFromFileAsync:");
        var person4 = await serializer.LoadFromFileAsync("test2.json");
        Console.WriteLine($"Загружен асинхронно: {person4.FullName}");
        
        // Экспорт нескольких объектов
        Console.WriteLine("\n7. SaveListToFile:");
        var people = new List<Person> { person, new Person("Анна", "Петрова", 30, "anna@test.com", "123") };
        serializer.SaveListToFile(people, "test3.json");
        Console.WriteLine("Список сохранен");
        
        // Импорт из файла
        Console.WriteLine("\n8. LoadListFromFile:");
        var people2 = serializer.LoadListFromFile("test3.json");
        Console.WriteLine($"Загружено {people2.Count} объектов");
        
        Console.WriteLine("\nВсе тесты пройдены");
    }
}