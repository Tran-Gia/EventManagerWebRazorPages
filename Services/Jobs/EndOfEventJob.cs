using EventManagerWebRazorPage.Areas.Identity.Data;
using EventManagerWebRazorPage.Models;
using EventManagerWebRazorPage.Services.Other;
using Quartz;

namespace EventManagerWebRazorPage.Services.Jobs
{
    public class EndOfEventJob(IEventService eventService, IUserRegistrationService userRegistrationService, IUserService userService) : IJob
    {
        private readonly IUserRegistrationService _userRegistrationService = userRegistrationService;
        private readonly IEventService _eventService = eventService;
        private readonly IUserService _userService = userService;

        public async Task Execute(IJobExecutionContext context)
        {
            var serviceResult = new ServiceResult();
            try
            {
                string eventId = context.JobDetail.Key.Name;
                var eventDetail = _eventService.GetEventDetailById(eventId);
                if (eventDetail is null)
                    throw new NullReferenceException(nameof(eventDetail) + "is null");
                var userRegistrations = _userRegistrationService.GetUserRegistrationsByEventId(eventId).Where(x => x.Approved == true);
                await UpdateCreditScoreForAll(userRegistrations.Where(x => x.Served), true);
                await UpdateCreditScoreForAll(userRegistrations.Where(x => !x.Served), false);
            }
            catch (Exception e)
            {
                serviceResult.Append(new ServiceResult("", $"Unable to run Task for EndOfEventJob with following details: '{context.JobDetail}'. Error: {e}", false));
            }
        }

        private Task<ServiceResult> UpdateCreditScoreForAll(IEnumerable<UserRegistration> userRegistrations, bool increase)
        {
            var serviceResult = new ServiceResult();
            foreach (var userRegistration in userRegistrations)
            {
                serviceResult.Append(UpdateCreditScore(userRegistration, increase));
            }
            return Task.FromResult(serviceResult);
        }
        private ServiceResult UpdateCreditScore(UserRegistration userRegistration, bool increase)
        {
            var serviceResult = new ServiceResult();
            try
            {
                User user = _userService.GetUserById(userRegistration.UserId)!;
                if(increase)
                {
                    IncreaseCreditScore(ref user);
                    SetConfirmStatusToConfirmed(userRegistration.RegistrationId);
                }
                else
                {
                    DecreaseCreditScore(ref user);
                }
                serviceResult.Append(_userService.UpdateUser(user));
            }
            catch(Exception e)
            {
                serviceResult.Append(new ServiceResult("", $"Cannot update Credit Score for {userRegistration.UserId} : {e}", false));
            }
            return serviceResult;
        }
        private static void IncreaseCreditScore(ref User user)
        {
            if (user.ConsecutiveCheckIn >= GlobalConfig.RequiredConsecutiveCheckIn)
            {
                user.CreditScore += GlobalConfig.CreditScoreAddition;
                if (user.CreditScore > 100) { user.CreditScore = 100; }
            }
            user.ConsecutiveCheckIn += 1;
        }
        private static void DecreaseCreditScore(ref User user)
        {
            user.CreditScore -= GlobalConfig.CreditScoreReduction;
            if (user.CreditScore < 0) { user.CreditScore = 0; }
            user.ConsecutiveCheckIn = 0;
        }
        private ServiceResult SetConfirmStatusToConfirmed(string userRegisId)
        {
            return _userRegistrationService.UpdateConfirmStatus(userRegisId, ConfirmStatus.Confirmed, false);
        }
    }
}
