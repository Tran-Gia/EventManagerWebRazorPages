using Microsoft.EntityFrameworkCore;

namespace EventManagerWebRazorPage.Models
{
    [PrimaryKey(nameof(OptionId))]
    public class EventItemOption
    {
        public string OptionId { get; set; } = string.Empty;
        public string? OptionDetail { get; set; }
        public string EventItemId { get; set; } = string.Empty;
        public EventItem EventItem { get; set; } = new EventItem();
    }
}
