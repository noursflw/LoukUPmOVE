using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loukupm.Model
{
    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string confirmPassword { get; set; } 
        public string? ImageUser { get; set; }

        public User() { }
        public User(int id, string name, string email, string password)
        {
            Id = id;
            UserName = name;
            Email = email;
            Password = password;
        }
      
    }
}
