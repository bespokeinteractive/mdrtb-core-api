using System;
namespace EtbSomalia.Models
{
    public class PatientProgram
    {
        public string TbmuNumber { get; set; }
        public DateTime DateEnrolled { get; set; }
        public Patient Patient { get; set; }

        public PatientProgram() {
            TbmuNumber = "";
            Patient = new Patient();
        }
    }
}
