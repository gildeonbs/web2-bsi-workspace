using System;

namespace EloDoacoes.ViewModels
{
    public class DonationCardViewModel
    {
        public int DonationId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ShortDescription { get; set; } = string.Empty;
        public string ImageBase64 { get; set; }
        public string DonationStatus { get; set; } = string.Empty;
        public bool IsOwner { get; set; }
        public int ReservationsCount { get; set; }
        public string ReservationStatusBadge { get; set; } = string.Empty;
    }
}
