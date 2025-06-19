namespace EventManagementApi.DTOs
{
    public class EventCreateDto
    {
        public string Name { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int MaxRegistrations { get; set; }
        public string Location { get; set; } = null!;
    }
}
