using Microsoft.AspNetCore.Identity;
using Microsoft.VisualBasic;
using System;

namespace EloDoacoes.Models
{
    
    public enum ReservationStatusEnum
    {
        Pending,
        Confirmed,
        Cancelled
    }

    public class Reservation
    {
        public int ReservationID { get; set; }
        public DateTime ReservationDate { get; set; }
        public ReservationStatusEnum ReservationStatus { get; set; } = ReservationStatusEnum.Pending;


        // Foreign Keys
        public Donation Donation { get; set; }
        public User User { get; set; }


    }
}
