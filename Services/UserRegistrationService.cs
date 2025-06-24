using EventManagerWebRazorPage.Areas.Identity.Data;
using EventManagerWebRazorPage.DAL;
using EventManagerWebRazorPage.Hubs;
using EventManagerWebRazorPage.Models;
using EventManagerWebRazorPage.Services.Other;
using EventManagerWebRazorPage.ViewModels;
using Microsoft.AspNetCore.SignalR;
using ILogger = Serilog.ILogger;

namespace EventManagerWebRazorPage.Services
{
    public class UserRegistrationService : IUserRegistrationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEventService _eventService;
        private readonly IUserService _userService;
        private readonly IPathProvider _pathProvider;
        private readonly IHubContext<ConfirmStatusHub> _hubContext;
        private readonly ILogger _logger;
        public UserRegistrationService(IUnitOfWork unitOfWork, IEventService eventService, IUserService userService,
            IPathProvider pathProvider, IHubContext<ConfirmStatusHub> hubContext, ILogger logger)
        {
            _unitOfWork = unitOfWork;
            _eventService = eventService;
            _userService = userService;
            _pathProvider = pathProvider;
            _hubContext = hubContext;
            _logger = logger;
        }
        public IEnumerable<UserRegistration> GetUserRegistrations()
        {
            return _unitOfWork.UserRegistrationRepository.Get();
        }

        public IEnumerable<UserRegistration> GetUserRegistrationsByEventId(string eventId)
        {
            return GetUserRegistrations().Where(x => x.EventId == eventId);
        }

        public List<UserRegistrationViewModel> GetPartialParticipationList(string eventId, string internalId = "")
        {
            var userRegistrations = GetUserRegistrationsByEventId(eventId);
            if (!String.IsNullOrEmpty(internalId))
            {
                var users = _unitOfWork.UserRepository.Get().Where(x => x.UserInternalId.StartsWith(internalId) || x.UserInternalId.EndsWith(internalId));
                userRegistrations = from user in users
                                    join userRegistration in userRegistrations on user.Id equals userRegistration.UserId
                                    select userRegistration;
            }
            return _eventService.CreateUserRegistrationViewModelList(userRegistrations);
        }

        public UserRegistration? GetUserRegistration(string eventId, string userId)
        {
            return _unitOfWork.UserRegistrationRepository.Get().FirstOrDefault(x => x.EventId == eventId && x.UserId == userId);
        }

        public UserRegistrationConfirmation? GetUserRegistrationConfirmation(string id, bool useConfirmId = true)
        {
            return useConfirmId ? _unitOfWork.UserRegistrationConfirmationRepository.Get().FirstOrDefault(x => x.ConfirmationId == id)
                    : _unitOfWork.UserRegistrationConfirmationRepository.Get().FirstOrDefault(x => x.UserRegistrationId == id);
        }

        public ServiceResult ApproveRegistration(string regisId)
        {
            _logger.Information("Attempt to approve Registration with Id: {Id}", regisId);
            try
            {
                var userRegistration = _unitOfWork.UserRegistrationRepository.Get().FirstOrDefault(x => x.RegistrationId == regisId);
                if (userRegistration != null)
                    userRegistration.Approved = true;
                else
                    throw new NullReferenceException();
                _unitOfWork.UserRegistrationRepository.Update(userRegistration);
                _unitOfWork.Save();
            }
            catch (Exception e)
            {
                return new ServiceResult(internalErrorMessage: $"Unable to approve UserRegistration with RegisId: '{regisId}', error: {e}", result: false).SendResultToLog(_logger, "Update", "Registration");
            }
            return new ServiceResult().SendResultToLog(_logger, "Update", "Registration");
        }

        public ServiceResult ApproveRegistration(List<string> regisIdList)
        {
            var serviceResult = new ServiceResult();
            foreach (string regisId in regisIdList)
            {
                serviceResult.Append(ApproveRegistration(regisId));
            }
            return serviceResult;
        }

        public ServiceResult CreateNewRegistration(string eventId, string userId, string itemId, out UserRegistration? userRegistration)
        {
            _logger.Information("Attempt to create a new UserRegistration with EventId: {EventId}, UserId: {UserId}, ItemId: {ItemId}", eventId, userId, itemId);
            userRegistration = null;
            if (_eventService.GetEventDetailById(eventId) == null || _userService.GetUserById(userId) == null)
            {
                return new ServiceResult("Input Event or User doesn't exist", "", false).SendResultToLog(_logger, "Create", "Registration");
            }
            try
            {
                userRegistration = new UserRegistration
                {
                    RegistrationId = Guid.NewGuid().ToString(),
                    EventId = eventId,
                    UserId = userId,
                    ItemId = itemId,
                    RegistrationTime = DateTime.Now,
                    //CheckInTime = null,
                    Approved = false,
                    Served = false
                };
                _unitOfWork.UserRegistrationRepository.Insert(userRegistration);
                _unitOfWork.Save();
            }
            catch (Exception e)
            {
                return new ServiceResult("An internal error occured", $"Cannot create a UserRegistration, error: {e}", false).SendResultToLog(_logger, "Create", "Registration");
            }
            return new ServiceResult().SendResultToLog(_logger, "Create", "Registration");
        }
        public ServiceResult SetServedStatus(string regisId, out DateTime? checkInTime, string confirmationPath, bool served = true)
        {
            _logger.Information("Attempt to update ServedStatus for RegisId: {RegisId}", regisId);
            checkInTime = null;
            var userRegistration = _unitOfWork.UserRegistrationRepository.GetById(regisId);
            if (userRegistration != null)
            {
                if (CheckUserRegistrationConfirmStatus(regisId))
                {
                    return new ServiceResult("Confirm Status is already confirmed", "", false).SendResultToLog(_logger, "Update", "Registration");
                }
                userRegistration.Served = served;
                if (served)
                {
                    userRegistration.CheckInTime = DateTime.Now;
                    checkInTime = userRegistration.CheckInTime;
                }
                _unitOfWork.UserRegistrationRepository.Update(userRegistration);
                _unitOfWork.Save();
                return SendConfirmationEmailForServedItem(userRegistration, served, confirmationPath).Result.SendResultToLog(_logger, "Update", "Registration");
            }
            return new ServiceResult("User Registration not found", "", false).SendResultToLog(_logger, "Update", "Registration");
        }
        public ServiceResult UpdateConfirmStatus(string id, ConfirmStatus confirmStatus, bool useConfirmId = true)
        {
            _logger.Information("Attempt to update ConfirmStatus for id: '{Id}' , IsConfirmId: {UseConfirmId}", id, useConfirmId);
            try
            {
                var confirmInfo = GetUserRegistrationConfirmation(id, useConfirmId);
                if (id == "expired" || confirmInfo == null)
                {
                    throw new Exception("Link expired");
                }
                confirmInfo!.ConfirmStatus = confirmStatus;
                confirmInfo.ConfirmationId = "expired";
                _unitOfWork.UserRegistrationConfirmationRepository.Update(confirmInfo);
                _unitOfWork.Save();
                _hubContext.Clients.All.SendAsync("ReceiveUserUpdateConfirmStatus", confirmInfo.UserRegistrationId, confirmStatus);
            }
            catch (Exception e)
            {
                return new ServiceResult("The link doesn't exist or has expired. Please contact us directly for further details.", $"Cannot update Participation Status, Id: {id}, Using ConfirmId: {useConfirmId}, error: {e}", false).SendResultToLog(_logger, "Update", "ConfirmInfo");
            }
            return new ServiceResult().SendResultToLog(_logger, "Update", "ConfirmInfo");
        }

        public UserRegistrationHistoryListViewModel? GetUserRegistrationHistoryListViewModel(string userId, bool showDetailedInformation = false)
        {
            var user = _userService.GetUserById(userId);
            if (user != null)
            {
                var userRegistrationHistoryViewModels = new List<UserRegistrationHistoryViewModel>();
                var userRegistrations = GetUserRegistrations().Where(x => x.UserId == userId);
                foreach (UserRegistration userRegistration in userRegistrations)
                {
                    AddUserRegistrationHistoryToList(userRegistration, ref userRegistrationHistoryViewModels, showDetailedInformation);
                }

                return new UserRegistrationHistoryListViewModel
                {
                    UserName = user.UserName,
                    CreditScore = (showDetailedInformation) ? user.CreditScore : 0,
                    ConsecutiveCheckIn = (showDetailedInformation) ? user.ConsecutiveCheckIn : 0,
                    UserRegistrationHistoryViewModels = userRegistrationHistoryViewModels
                };
            }
            return null;
        }


        #region Private Methods

        private async Task<ServiceResult> SendConfirmationEmailForServedItem(UserRegistration userRegistration, bool served, string confirmationPath)
        {
            var email = new List<string>();
            try
            {
                var user = _userService.GetUserById(userRegistration.UserId);
                var eventDetail = _eventService.GetEventDetailById(userRegistration.EventId);
                var eventItem = eventDetail!.EventItem.FirstOrDefault(x => x.ItemId == userRegistration.ItemId);
                if (user == null || user.Email == null || eventItem == null)
                {
                    return new ServiceResult("Event Item, User or User Email not found", "", false);
                }

                email.Add(user!.Email!);
                string title = eventDetail.Title + " - Confirmation Email";
                string message = SetMessageForServedItemEmail(user, eventItem, userRegistration, confirmationPath, served);
                string? image = GetEventItemImagePath(eventItem);
                await EmailSender.SendNotificationEmail(email, title, message, image);
            }
            catch (Exception e)
            {
                return new ServiceResult("Unable to send confirmation email", $"Unable to send confirmation email: {e}", false);
            }
            return new ServiceResult();
        }

        private string SetMessageForServedItemEmail(User user, EventItem eventItem, UserRegistration userRegistration, string confirmationPath, bool served)
        {
            string message = $"Dear {user.UserName} (Internal Id: {user.UserInternalId}),<br/><br/>";
            if (served)
            {
                message += "You have claimed the following item in the event:<br/>";
            }
            else
            {
                message += "We have revoked the confirmation email for the following item in the event: <br/>";
            }
            message += $"Item: {eventItem.ItemName}<br/>Checkin Time: {userRegistration.CheckInTime}<br/><br/>";
            if (served)
            {
                string id = AddUserRegistrationConfirmationInfo(userRegistration);
                message += $"Please confirm by clicking the following link: {AddUserRegistrationConfirmationLink(id, confirmationPath, ConfirmStatus.Confirmed)}<br/>" +
                    $"If you didn't make this claim, please report to us using the following link: {AddUserRegistrationConfirmationLink(id, confirmationPath, ConfirmStatus.Denied)}<br/><br/>" +
                    "Please note you have until the end of the day to report. Otherwise, the status will automatically be changed to Confirmed by then.";
            }
            else
            {
                message += "We apologize for any inconvenience this has caused. Please come and claim your item as soon as possible.";
            }
            return message;
        }
        private string AddUserRegistrationConfirmationInfo(UserRegistration userRegistration)
        {
            _logger.Information("Attempt to add/update ConfirmInfo for regisId: {RegisId}", userRegistration.RegistrationId);
            var confirmInfo = _unitOfWork.UserRegistrationConfirmationRepository.Get().FirstOrDefault(x => x.UserRegistrationId == userRegistration.RegistrationId);
            try
            {
                if (confirmInfo != null)
                {
                    confirmInfo.ConfirmationId = Guid.NewGuid().ToString();
                    confirmInfo.ConfirmStatus = ConfirmStatus.Pending;
                    _unitOfWork.UserRegistrationConfirmationRepository.Update(confirmInfo);
                }
                else
                {
                    confirmInfo = new UserRegistrationConfirmation
                    {
                        UserRegistrationId = userRegistration.RegistrationId,
                        ConfirmStatus = ConfirmStatus.Pending,
                        ConfirmationId = Guid.NewGuid().ToString()
                    };
                    _unitOfWork.UserRegistrationConfirmationRepository.Insert(confirmInfo);
                }
                _unitOfWork.Save();
                _logger.Information("Successfully created/updated ConfirmInfo with confirmId: {ConfirmId}", confirmInfo.ConfirmationId);
                return confirmInfo.ConfirmationId;
            }
            catch (Exception e)
            {
                _logger.Error("Unable to create/update ConfirmInfo for RegisId {RegisId}, error: {Error}", userRegistration.RegistrationId, e);
            }
            return string.Empty;
        }
        private string AddUserRegistrationConfirmationLink(string confirmationId, string confirmationPath, ConfirmStatus confirmStatus)
        {
            var link = confirmationPath + $"?id={confirmationId}&&status={confirmStatus}";
            _logger.Information("Created link for ConfirmStatus {ConfirmStatus}: {Link}", confirmStatus, link);
            return link;
        }
        private string? GetEventItemImagePath(EventItem eventItem)
        {
            return !String.IsNullOrEmpty(eventItem.Image) ? Path.Combine(_pathProvider.MapPath("Images"), eventItem.Image) : null;
        }

        private bool CheckUserRegistrationConfirmStatus(string regisId)
        {
            var confirmInfo = GetUserRegistrationConfirmation(regisId, false);
            if (confirmInfo != null && confirmInfo.ConfirmStatus == ConfirmStatus.Confirmed)
            {
                return true;
            }
            return false;
        }

        private void AddUserRegistrationHistoryToList(UserRegistration userRegistration, ref List<UserRegistrationHistoryViewModel> list, bool showDetailedInformation)
        {
            var eventDetail = _eventService.GetEventDetailById(userRegistration.EventId);
            var eventItem = _eventService.GetEventItemById(userRegistration.ItemId);

            if (eventDetail != null && eventItem != null)
            {
                list.Add(new UserRegistrationHistoryViewModel
                {
                    UserRegistrationId = userRegistration.RegistrationId,
                    EventId = (showDetailedInformation) ? userRegistration.EventId : null,
                    EventName = eventDetail.Title,
                    ItemName = eventItem.ItemName,
                    RegistrationTime = userRegistration.RegistrationTime,
                    CheckInTime = ValidateDateTime.CompareDate(userRegistration.CheckInTime, eventDetail.EventTime, ">=") ? userRegistration.CheckInTime : null,
                    RegistrationStatus = SetRegistrationStatus(userRegistration, eventDetail),
                });
            }
        }

        private static DetailedRegistrationStatus SetRegistrationStatus(UserRegistration userRegistration, EventDetail eventDetail)
        {
            if (userRegistration.Served || userRegistration.CheckInTime.ValidDateTime(eventDetail, false))
            {
                return DetailedRegistrationStatus.Claimed;
            }
            if (userRegistration.Approved)
            {
                //Approved but isn't served and CheckinTime is not within EventTime
                //=> EventTime hasn't started, or the user didn't claim the item
                if (ValidateDateTime.CompareDate(DateTime.Now, eventDetail.EventTime, "<"))
                {
                    return DetailedRegistrationStatus.Approved;
                }
                return DetailedRegistrationStatus.Missed;
            }
            //Not Approved
            //=> Registration Time hasn't ended, or the registration was rejected
            if (userRegistration.RegistrationTime.ValidDateTime(eventDetail))
            {
                return DetailedRegistrationStatus.Pending;
            }
            return DetailedRegistrationStatus.Rejected;
        }

        #endregion
    }
}
