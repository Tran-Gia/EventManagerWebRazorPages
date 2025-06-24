using EventManagerWebRazorPage.Models;
using EventManagerWebRazorPage.Services.Other;
using EventManagerWebRazorPage.ViewModels;

namespace EventManagerWebRazorPage.Services
{
    public interface IEventService
    {
        ServiceResult CreateNewEventDetail(EventDetailViewModel eventDetailModel);
        ServiceResult UpdateEventDetail(EventDetailViewModel eventDetailModel);
        ServiceResult DeleteEventDetail(EventDetail eventDetail);
        ServiceResult DeleteEventDetail(string id);
        EventDetail? GetEventDetailById(string id, bool includeHidden = false);
        EventItem? GetEventItemById(string id);
        IEnumerable<EventDetail> GetEventDetailList(bool includeHidden = false);
        EventWithDetailedEventItemsViewModel GetEventWithDetailedEventItemsViewModel(EventDetail eventDetail);
        ParticipationListViewModel? GetParticipationListByEventId(string id);
        List<UserRegistrationViewModel> CreateUserRegistrationViewModelList(IEnumerable<UserRegistration> userRegistrations);
    }
}
