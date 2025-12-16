using System;
using System.Text.Json.Serialization;

public class Person
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int Age { get; set; }
    
    [JsonIgnore]
    public string Password { get; set; } = string.Empty;
    
    [JsonPropertyName("personId")]
    public string Id { get; set; } = string.Empty;
    
    [JsonInclude]
    private DateTime _birthDate;
    
    public DateTime BirthDate
    {
        get => _birthDate;
        set => _birthDate = value;
    }
    
    private string _email = string.Empty;
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
    public string PhoneNumber { get; set; } = string.Empty;
    
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
        FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
        LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
        Age = age;
        Email = email ?? throw new ArgumentNullException(nameof(email));
        PhoneNumber = phone ?? throw new ArgumentNullException(nameof(phone));
    }
}