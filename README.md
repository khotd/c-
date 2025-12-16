# Лабораторная работа 1: Сериализация JSON и работа с файлами в C#
Выполнила: Хотеева Диана К0709-23/3

Классы:
1. Person - с атрибутами JSON
2. PersonSerializer - сериализация и файлы
3. FileResourceManager - работа с ресурсами
4. Logger - логирование ошибок

Основные технологии:
1. .NET 10.0 - платформа разработки
2. C# 11 - язык программирования
3. System.Text.Json - встроенная библиотека для работы с JSON

Используемые библиотеки .NET:
1. System
2. System.IO - работа с файлами и потоками
3. System.Text.Json - сериализация/десериализация JSON
4. System.Threading.Tasks - асинхронные операции
5. System.Text.Json.Serialization
6. JsonIgnoreAttribute - игнорирование свойств при сериализации
7. JsonPropertyNameAttribute - переименование свойств в JSON
8. JsonIncludeAttribute - включение приватных полей

Тестирование:
1. xUnit 2.6.3 - фреймворк для unit-тестирования
2. Microsoft.NET.Test.Sdk 17.8.0 - SDK для тестирования

Запуск тестов:
dotnet test
