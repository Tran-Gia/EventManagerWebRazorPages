
using EventManagerWebRazorPage.Areas.Identity.Data;
using EventManagerWebRazorPage.Models;
using System.ComponentModel;

namespace EventManagerWebRazorPage.ViewModels
{
    public class ParticipationListViewModel
    {
        public EventDetail EventDetail { get; set; } = new EventDetail();
        public List<UserRegistrationViewModel> UserRegistrationViewModels { get; set; } = [];
        public List<EventItemViewModel> EventItemViewModels { get; set; } = [];
        public int ParticipantsServed => UserRegistrationViewModels.Count(x => x.Served);
        public int TotalParticipants 
        {  
            get
            {
                return UserRegistrationViewModels.Count;
            }
        }
    }
    public class UserRegistrationViewModel
    {
        public UserRegistrationViewModel(User user, string? userRegistrationId, string? eventItemName, bool served, ConfirmStatus confirmStatus, DateTime checkinTime)
        {
            UserId = user.Id;
            UserRegistrationId = userRegistrationId;
            UserName = user.UserName;
            InternalId = user.UserInternalId;
            EventItemName = eventItemName;
            Served = served;
            CheckinTime = checkinTime;
            ConfirmStatus = confirmStatus;
        }
        public UserRegistrationViewModel() { }

        public string? UserRegistrationId { get; set; }
        public string? UserId { get; set; }
        [DisplayName("User Name")]
        public string? UserName { get; set; }
        [DisplayName("Internal ID")]
        public string? InternalId { get; set; }
        [DisplayName("Item Choice")]
        public string? EventItemName { get; set; }
        [DisplayName("Check In")]
        public bool Served { get; set; } = false;
        [DisplayName("Confirmed ?")]
        public ConfirmStatus ConfirmStatus { get; set; }
        public DateTime CheckinTime { get; set; }
    }
    public class EventItemViewModel()
    {
        [DisplayName("Item's Name")]
        public EventItem EventItem { get; set; } = new EventItem();
        [DisplayName("Registered")]
        public int ItemsRegistered { get;set; }
        [DisplayName("Delivered")]
        public int ItemsServed { get; set; }
    }
}
