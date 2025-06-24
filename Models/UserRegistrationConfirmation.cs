using Microsoft.EntityFrameworkCore;

namespace EventManagerWebRazorPage.Models
{
    [PrimaryKey(nameof(UserRegistrationId))]
    public class UserRegistrationConfirmation
    {
        public string UserRegistrationId { get; set; } = string.Empty;
        public string ConfirmationId { get; set; } = string.Empty;
        public ConfirmStatus ConfirmStatus { get; set; }
    }
    public enum ConfirmStatus { Confirmed = 1, Pending = 0, Denied = -1}
}
