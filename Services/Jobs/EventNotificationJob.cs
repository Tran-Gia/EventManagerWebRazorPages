using MimeKit;
using MailKit.Net.Smtp;
using Quartz;
using EventManagerWebRazorPage.DAL;
using Microsoft.Extensions.Logging;
using EventManagerWebRazorPage.Models;
using EventManagerWebRazorPage.Services.Other;

namespace EventManagerWebRazorPage.Services.Jobs
{
    struct UserList
    {
        public string RegistrationId;
        public string ItemId;
        public DateTime CheckinTime;
        public string Email;
        public int CreditScore;
    }
    public class EventNotificationJob : IJob
    {
        private readonly IUserRegistrationService _userRegistrationService;
        private readonly IEventService _eventService;
        private readonly IUserService _userService;
        private readonly ISchedulerFactory _schedulerFactory;
        public EventNotificationJob(IEventService eventService, IUserRegistrationService userRegistrationService, IUserService userService, ISchedulerFactory schedulerFactory)
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
                var eventItems = eventDetail.EventItem;
                var userRegistrations = _userRegistrationService.GetUserRegistrationsByEventId(eventId);
                var users = _userService.GetUserList();
                var userList = from userRegis in userRegistrations
                               join user in users on userRegis.UserId equals user.Id
                               let email = user!.Email
                               select new UserList { RegistrationId = userRegis.RegistrationId, Email = email, ItemId = userRegis.ItemId, CheckinTime = userRegis.CheckInTime, CreditScore = user.CreditScore };
                userList = await ApproveRegistrationList(userList, eventItems);
                string title = $"{eventDetail.Title} - Registration Approved";
                string message = "Congratulations! Your request to participate has been approved!" +
                    $"\nRemember to checkin by the date {eventDetail.EventTime.ToString("dd/MM/yyyy")}!";
                await EmailSender.SendNotificationEmail(userList.Select(x => x.Email).ToList(), title, message);

                title = $"{eventDetail.Title} - Registration Rejected";
                message = "We're sorry to inform you that we've ran out of slots for the event. Better luck next time~";
                var rejectedList = from userRegis in userRegistrations
                                   join user in users on userRegis.UserId equals user.Id
                                   where !userRegis.Approved
                                   let email = user!.Email
                                   select email;
                await EmailSender.SendNotificationEmail(rejectedList.ToList(), title, message);
                AddReminderNotification(eventDetail);
                return;
            }
            catch (Exception e)
            {
                serviceResult.Append(new ServiceResult("", $"Unable to run Task for EventNotificationJob with following details: '{context.JobDetail}'. Error: {e}", false));
            }
            return;
        }

        private Task<IEnumerable<UserList>> ApproveRegistrationList(IEnumerable<UserList> userList, ICollection<EventItem> eventItems)
        {
            var serviceResult = new ServiceResult();
            var approvedUserList = new List<UserList>();
            foreach (var eventItem in eventItems)
            {
                try
                {
                    var userListPerItem = userList.Where(x => x.ItemId == eventItem.ItemId).OrderBy(x => x.CreditScore).ThenBy(x => x.CheckinTime).AsEnumerable();
                    if (userListPerItem.Count() > eventItem.Amount)
                    {
                        userListPerItem = userListPerItem.Take(eventItem.Amount);
                    }
                    approvedUserList.AddRange(userListPerItem);
                    List<string> list = userListPerItem.Select(x => x.RegistrationId).ToList();
                    serviceResult.Append(_userRegistrationService.ApproveRegistration(list));
                }
                catch (Exception e)
                {
                    serviceResult.Append(new ServiceResult("", $"Unable to approve registrations for EventItem with Id: '{eventItem.ItemId}'. Error: {e}", false));
                }
            }
            return Task.FromResult(approvedUserList.AsEnumerable());
        }

        private void AddReminderNotification(EventDetail eventDetail)
        {
            var eventDate = eventDetail.EventTime.Date.AddHours(7).AddMinutes(0).AddSeconds(0);
            eventDate = eventDate < DateTime.Now ? DateTime.Now : eventDate;
            var job = JobBuilder.Create<EventReminderNotificationJob>().WithIdentity(eventDetail.EventId, "Event Reminder Notification").Build();
            var trigger = TriggerBuilder.Create().WithIdentity(eventDetail.EventId, "Event Reminder Notification")
                .WithCronSchedule($"{eventDate.Second + 10} {eventDate.Minute} {eventDate.Hour} {eventDate.Day} {eventDate.Month} ? {eventDate.Year}").Build();
            var scheduler = _schedulerFactory.GetScheduler().Result;
            scheduler.ScheduleJob(job, trigger);
        }
    }
}
