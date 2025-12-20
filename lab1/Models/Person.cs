using System;
using System.Text.Json.Serialization;

namespace Lab1.Models
{
    public class Person
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public int Age { get; set; }
        
        [JsonIgnore]
        public string Password { get; set; } = "";
        
        [JsonPropertyName("personId")]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        public string Email { get; set; } = "";
        
        [JsonPropertyName("phone")]
        public string PhoneNumber { get; set; } = "";
        
        public string FullName => $"{FirstName} {LastName}";
        public bool IsAdult => Age >= 18;
        
        public Person() { }
        
        public Person(string firstName, string lastName, int age, string email, string phone)
        {
            FirstName = firstName;
            LastName = lastName;
            Age = age;
            Email = email;
            PhoneNumber = phone;
        }
    }
}