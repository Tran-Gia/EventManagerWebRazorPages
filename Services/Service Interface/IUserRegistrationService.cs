using EventManagerWebRazorPage.Models;
using EventManagerWebRazorPage.Services.Other;
using EventManagerWebRazorPage.ViewModels;
using System.Security.Claims;

namespace EventManagerWebRazorPage.Services
{
    public interface IUserRegistrationService
    {
        ServiceResult ApproveRegistration(string regisId);
        ServiceResult ApproveRegistration(List<string> regisIdList);
        ServiceResult CreateNewRegistration(string eventId, string userId, string itemId, out UserRegistration? userRegistration);
        IEnumerable<UserRegistration> GetUserRegistrations();
        IEnumerable<UserRegistration> GetUserRegistrationsByEventId(string eventId);
        List<UserRegistrationViewModel> GetPartialParticipationList(string eventId, string internalId = "");
        UserRegistration? GetUserRegistration(string eventId, string userId);
        UserRegistrationConfirmation? GetUserRegistrationConfirmation(string id, bool useConfirmId = true);
        ServiceResult SetServedStatus(string regisId, out DateTime? checkInTime, string confirmationPath, bool served = true);
        ServiceResult UpdateConfirmStatus(string id, ConfirmStatus confirmStatus, bool useConfirmId = true);
        UserRegistrationHistoryListViewModel? GetUserRegistrationHistoryListViewModel(string userId, bool showDetailedInformation = false);
    }
}
