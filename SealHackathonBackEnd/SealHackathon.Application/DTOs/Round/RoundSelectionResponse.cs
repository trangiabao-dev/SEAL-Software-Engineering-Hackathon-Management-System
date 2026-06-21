namespace SealHackathon.Application.DTOs.Round
{
    public class RoundSelectionResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int TrackId { get; set; }
        public string TrackName { get; set; } = null!;
    }
}
