using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;

namespace EloDoacoes.Models
{
    [Index(nameof(Email), IsUnique = true)]
    public class User
    {
        public int UserID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public DateTime RegistrationDate { get; set; }

        // Foreign Keys
        public Role Role { get; set; }
    }
}
