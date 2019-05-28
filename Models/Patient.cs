using System;
namespace EtbSomalia.Models
{
    public class Patient
    {
        public string Uuid { get; set; }
        public Person Person { get; set; }
        public int Dead { get; set; }
        public DateTime? DiedOn { get; set; }

        public Patient() {
            Uuid = "";
            Person = new Person();
            Dead = 0;
        }
    }
}
