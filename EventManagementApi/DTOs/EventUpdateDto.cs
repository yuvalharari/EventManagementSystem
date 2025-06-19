namespace EventManagementApi.DTOs
{
    public class EventUpdateDto
    {
        public string Name { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int MaxRegistrations { get; set; }
        public string Location { get; set; } = null!;
    }
}
