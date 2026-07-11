using System;
using System.Collections.Generic;

namespace EloDoacoes.Models
{
    public class Donation
    {
        public int DonationID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; } 
        public DateTime RegistrationDate { get; set; }

        // Foreign key for User (donor)
        public int? UserID { get; set; }
        public User User { get; set; }

        public Category Category { get; set; }
        public DonationStatus DonationStatus { get; set; }
        public ICollection<Reservation> Reservations { get; set; }
        public ICollection<DonationImage> DonationImages { get; set; }
    }
}