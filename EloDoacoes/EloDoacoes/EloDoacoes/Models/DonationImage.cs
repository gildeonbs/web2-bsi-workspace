namespace EloDoacoes.Models
{
    public class DonationImage
    {
        public int DonationImageId { get; set; }
        public int DonationId { get; set; }
        public byte[] ImageData { get; set; } = null!;
        public string ContentType { get; set; } = null!;
        public string FileName { get; set; } = null!;

        // Foreign Key Navigation Property
        public Donation Donation { get; set; } = null!;
    }
}
