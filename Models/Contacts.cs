using System;
namespace EtbSomalia.Models
{
    public class Contacts
    {
        public string Uuid { get; set; }
        public string Identifier { get; set; }

        public Person Person { get; set; }
        public PatientProgram Index { get; set; }

        public string Status { get; set; }
        public string Location { get; set; }
        public string Relation { get; set; }
        public string Proximity { get; set; }
        public string DiseaseAfter { get; set; }
        public string PrevouslyTreated { get; set; }

        public DateTime NextVisit { get; set; }
        public DateTime ExposedOn { get; set; }
        public DateTime AddedOn { get; set; }

        public string Notes { get; set; }

        public Contacts() {
            Uuid = "";
            Identifier = "";

            Person = new Person();
            Index = new PatientProgram();

            Status = "";
            Location = "";
            Relation = "";
            Proximity = "";
            DiseaseAfter = "";
            PrevouslyTreated = "";

            NextVisit = DateTime.Now;
            ExposedOn = DateTime.Now;
            AddedOn = DateTime.Now;
        }
    }
}
