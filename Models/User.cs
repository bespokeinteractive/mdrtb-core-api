using System;
namespace EtbSomalia.Models
{
    public class User {
        public string Uuid { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }

        public bool Enabled { get; set; }
        public bool ToChange { get; set; }

        public string Username { get; set; }
        public string Password { get; set; }
        public string Token { get; set; }
    }
}
