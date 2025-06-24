using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EventManagerWebRazorPage.ViewModels
{
    public class UserRegistrationHistoryListViewModel
    {
        public string? UserName { get; set; } = string.Empty;
        public string? EventId { get; set; } = string.Empty;
        public int CreditScore { get; set; }
        public int ConsecutiveCheckIn { get; set; }
        public List<UserRegistrationHistoryViewModel> UserRegistrationHistoryViewModels { get; set; } = [];
    }
    public class UserRegistrationHistoryViewModel
    {
        public string UserRegistrationId { get; set; } = string.Empty;
        public string? EventId { get; set; } = string.Empty;
        [DisplayName("Event")]
        public string? EventName { get; set; } = string.Empty;
        [DisplayName("Item")]
        public string? ItemName { get;set; } = string.Empty;
        [DisplayName("Registered Time")]
        [DisplayFormat(DataFormatString = "{0:hh:mm:ss dd/MM/yyyy}")]
        public DateTime RegistrationTime { get; set; }
        [DisplayName("Checked In Time")]
        [DisplayFormat(DataFormatString = "{0:hh:mm:ss dd/MM/yyyy}")]
        public DateTime? CheckInTime { get; set; } = null;
        [DisplayName("Status")]
        public DetailedRegistrationStatus RegistrationStatus { get; set; } = DetailedRegistrationStatus.Pending;
    }
    public enum DetailedRegistrationStatus
    {
        Rejected = -1,
        Pending = 0,
        Approved = 1,
        Missed =2,
        Claimed = 3
    }
}
