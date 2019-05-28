using System;
namespace EtbSomalia.Models
{
    public class PersonAddress
    {
        public long Id { get; set; }
        public Boolean Default { get; set; }
        public string Telephone { get; set; }
        public string Address { get; set; }

        public PersonAddress() {
            Id = 0;
            Default = false;
            Telephone = "";
            Address = "";
        }
    }
}
