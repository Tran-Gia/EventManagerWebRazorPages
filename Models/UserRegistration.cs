using Microsoft.EntityFrameworkCore;

namespace EventManagerWebRazorPage.Models
{
    [PrimaryKey(nameof(RegistrationId))]
    public class UserRegistration
    {
        public string RegistrationId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string EventId { get; set; } = string.Empty;
        public string ItemId { get; set; } = string.Empty;
        public string OptionsId { get; set; } = string.Empty;
        public bool Approved { get; set; } = false;
        public bool Served { get; set; } = false;
        public DateTime RegistrationTime { get; set; }
        public DateTime CheckInTime { get; set; }
        public EventItemOption? EventItemOption { get; set; }

    }
}
