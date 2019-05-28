using System;
namespace EtbSomalia.Models
{
    public class ContactsExamination
    {
        public string Uuid { get; set; }
        public Contacts Contact { get; set; }

        public bool Cough { get; set; }
        public bool Fever { get; set; }
        public bool WeightLoss { get; set; }
        public bool NightSweat { get; set; }

        public string LTBI { get; set; }
        public string SputumSmear { get; set; }
        public string GeneXpert { get; set; }
        public string XrayExam { get; set; }
        public string PreventiveTherapy { get; set; }

        public DateTime NextScreening { get; set; }
        public DateTime AddedOn { get; set; }

        public ContactsExamination() {
            Uuid = "";
            Contact = new Contacts();

            Cough = false;
            Fever = false;
            WeightLoss = false;
            NightSweat = false;

            LTBI = "";
            SputumSmear = "";
            GeneXpert = "";
            XrayExam = "";

            PreventiveTherapy = "";
            NextScreening = DateTime.Now.AddMonths(6);
            AddedOn = DateTime.Now;
        }
    }
}
