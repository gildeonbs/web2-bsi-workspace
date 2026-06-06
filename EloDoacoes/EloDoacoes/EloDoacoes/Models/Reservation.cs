using Microsoft.AspNetCore.Identity;
using Microsoft.VisualBasic;
using System;

namespace EloDoacoes.Models
{
    public class Reservation
    {
        public int ReservationID { get; set; }
        public DateTime ReservationDate { get; set; }
        public Donation Donation { get; set; }
        public User User { get; set; }
        public ReservationStatus ReservationStatus { get; set; }

    }
}
