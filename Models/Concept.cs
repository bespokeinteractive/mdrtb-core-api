using System;
namespace EtbSomalia.Models
{
    public class Concept
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public Concept() {
            Id = 0;
            Name = "";
            Description = "";
        }
    }
}
