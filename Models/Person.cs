using System;
namespace EtbSomalia.Models
{
    public class Person
    {
        public string Uuid { get; set; }
        public string Name { get; set; }
        public string Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public PersonAddress Address { get; set; }

        public Person() {
            Uuid = "";
            Name = "";
            Gender = "";
            DateOfBirth = DateTime.Now;
        }
    }
}
