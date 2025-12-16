using System;
using System.Text.Json.Serialization;

public class Person
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Age { get; set; }
    
    [JsonIgnore]
    public string Password { get; set; }
    
    [JsonPropertyName("personId")]
    public string Id { get; set; }
    
    [JsonInclude]
    private DateTime _birthDate;
    
    public DateTime BirthDate
    {
        get => _birthDate;
        set => _birthDate = value;
    }
    private string _email;
    public string Email
    {
        get => _email;
        set
        {
            if (!IsValidEmail(value))
                throw new ArgumentException("Некорректный email адрес");
            _email = value;
        }
    }
    
    [JsonPropertyName("phone")]
    public string PhoneNumber { get; set; }
    
    public string FullName => $"{FirstName} {LastName}";
    
    public bool IsAdult => Age >= 18;
    
    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;
        
        return email.Contains("@");
    }
    public Person()
    {
        Id = Guid.NewGuid().ToString();
    }
    
    public Person(string firstName, string lastName, int age, string email, string phone)
        : this()
    {
        FirstName = firstName;
        LastName = lastName;
        Age = age;
        Email = email;
        PhoneNumber = phone;
    }
}