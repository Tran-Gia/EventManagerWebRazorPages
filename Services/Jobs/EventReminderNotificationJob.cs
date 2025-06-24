using EventManagerWebRazorPage.Models;
using EventManagerWebRazorPage.Services.Other;
using Quartz;

namespace EventManagerWebRazorPage.Services.Jobs
{
    public class EventReminderNotificationJob : IJob
    {
        private readonly IUserRegistrationService _userRegistrationService;
        private readonly IEventService _eventService;
        private readonly IUserService _userService;
        private readonly ISchedulerFactory _schedulerFactory;
        public EventReminderNotificationJob(IEventService eventService, IUserRegistrationService userRegistrationService, IUserService userService, ISchedulerFactory schedulerFactory)
        {
            _userRegistrationService = userRegistrationService;
            _eventService = eventService;
            _userService = userService;
            _schedulerFactory = schedulerFactory;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            var serviceResult = new ServiceResult();
            try
            {
                string eventId = context.JobDetail.Key.Name;
                var eventDetail = _eventService.GetEventDetailById(eventId);
                if (eventDetail == null)
                    throw new NullReferenceException(nameof(eventDetail) + "is null");
                var userRegistrations = _userRegistrationService.GetUserRegistrationsByEventId(eventId).Where(x => x.Approved == true);
                var users = _userService.GetUserList();
                string title = $"Reminder: {eventDetail.Title} - Event Manager";
                string message = "The event occurs today! Please come and claim your items before the event ends!";
                var emails = from userRegis in userRegistrations
                             join user in users on userRegis.UserId equals user.Id
                             let email = user!.Email
                             select email;
                await EmailSender.SendNotificationEmail(emails.ToList(), title, message);
                AddEndOfEventScoreCalculation(eventDetail);
                return;
            }
            catch (Exception e)
            {
                serviceResult.Append(new ServiceResult("", $"Unable to run Task for EventReminderNotificationJob with following details: '{context.JobDetail}'. Error: {e}", false));
            }
            return;
        }

        private void AddEndOfEventScoreCalculation(EventDetail eventDetail)
        {
            var eventDate = eventDetail.EventTime.Date.AddDays(1);
            var job = JobBuilder.Create<EndOfEventJob>().WithIdentity(eventDetail.EventId, "End Of Event Score Calculation").Build();
            var trigger = TriggerBuilder.Create().WithIdentity(eventDetail.EventId, "End Of Event Score Calculation")
                .WithCronSchedule($"{eventDate.Second} {eventDate.Minute} {eventDate.Hour} {eventDate.Day} {eventDate.Month} ? {eventDate.Year}").Build();
            var scheduler = _schedulerFactory.GetScheduler().Result;
            scheduler.ScheduleJob(job, trigger);
        }
    }
}
