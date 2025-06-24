using EventManagerWebRazorPage.DAL;
using EventManagerWebRazorPage.Models;
using EventManagerWebRazorPage.Services.Jobs;
using EventManagerWebRazorPage.Services.Other;
using EventManagerWebRazorPage.ViewModels;
using Quartz;
using ILogger = Serilog.ILogger;

namespace EventManagerWebRazorPage.Services
{
    public class EventService : IEventService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPathProvider _pathProvider;
        private readonly ISchedulerFactory _schedulerFactory;
        private readonly string filePath;
        private readonly ILogger _logger;

        public EventService(IUnitOfWork unitOfWork, IPathProvider pathProvider, ISchedulerFactory schedulerFactory, ILogger logger)
        {
            _unitOfWork = unitOfWork;
            _pathProvider = pathProvider;
            _schedulerFactory = schedulerFactory;
            filePath = _pathProvider.MapPath("Images");
            _logger = logger;
        }

        public IEnumerable<EventDetail> GetEventDetailList(bool includeHidden = false)
        {
            if (!includeHidden)
                return _unitOfWork.EventDetailRepository.Get().Where(x => !x.IsHidden);
            else
                return _unitOfWork.EventDetailRepository.Get();
        }
        public ServiceResult CreateNewEventDetail(EventDetailViewModel eventDetailModel)
        {
            var eventDetail = eventDetailModel.EventDetail;
            var eventDetailImage = eventDetailModel.Image;
            var serviceResult = new ServiceResult();
            var eventItemImages = eventDetailModel.EventItemImagesViewModels;

            eventDetail.EventId = String.IsNullOrEmpty(eventDetail.EventId) ? Guid.NewGuid().ToString() : eventDetail.EventId;
            _logger.Information("Attempt to create a new Event: {@EventDetail}", eventDetailModel);
            serviceResult.Append(new ServiceResult[]
            {
                SetImageForModel(ref eventDetail, eventDetailImage, true),
                CreateNewEventItems(ref eventDetail, eventItemImages!)
            });

            if (serviceResult.Result)
            {
                try
                {
                    var eventDate = eventDetail.RegistrationEndDate.Date.AddDays(1);
                    ScheduleJobForEvent<EventNotificationJob>(eventDate, eventDetail.EventId, "Event Notification");

                    _unitOfWork.EventDetailRepository.Insert(eventDetail);
                    _unitOfWork.Save();
                }
                catch (Exception ex)
                {
                    serviceResult.Append(new ServiceResult("An internal error has occured.",
                        $"Cannot create Event Detail {eventDetail.Title}, Id: '{eventDetail.EventId}': {ex.Message}",
                        false));
                }
            }
            return serviceResult.SendResultToLog(_logger, "Create", "An Event");
        }

        public ServiceResult UpdateEventDetail(EventDetailViewModel eventDetailModel)
        {
            var updatedEventDetail = eventDetailModel.EventDetail;
            var eventDetail = _unitOfWork.EventDetailRepository.Get(x => x.EventId == updatedEventDetail.EventId, includeProperties: "EventItem").FirstOrDefault();

            var eventItems = updatedEventDetail.EventItem.ToList();
            _logger.Information("Attempt to update Event with following model: {@EventDetail}", eventDetailModel);
            if (eventDetail != null && DateTime.Now < eventDetail.EventTime)
            {
                var restrictedMode = DateTime.Now.ValidDateTime(eventDetail);
                _logger.Information("Edit Restricted Mode On: {RestrictedMode}", restrictedMode);
                var serviceResult = new ServiceResult();
                serviceResult.Append(new ServiceResult[]
                {
                    UpdateEventDetailModel(ref eventDetail, eventDetailModel, restrictedMode),
                    UpdateEventItems(ref eventDetail, eventItems, eventDetailModel.EventItemImagesViewModels, restrictedMode)
                });

                if (serviceResult.Result)
                {
                    try
                    {
                        var eventDate = eventDetail.RegistrationEndDate.Date.AddDays(1);
                        ScheduleJobForEvent<EventNotificationJob>(eventDate, eventDetail.EventId, "Event Notification", true);

                        _unitOfWork.EventDetailRepository.Update(eventDetail);
                        _unitOfWork.Save();
                    }
                    catch (Exception e)
                    {
                        serviceResult.Append(new ServiceResult($"Failed to update Event Detail {eventDetail.Title} due to an internal error.",
                                        $"Failed to update Event Detail {eventDetail.Title}, Id: '{eventDetail.EventId}', error: {e}",
                                        false));
                    }
                }
                return serviceResult.SendResultToLog(_logger, "Update", "Event");
            }
            return new ServiceResult("Event Detail is not found or can no longer be updated.", false).SendResultToLog(_logger, "Update", "Event");
        }
        public ServiceResult DeleteEventDetail(EventDetail eventDetail)
        {
            var serviceResult = new ServiceResult();
            eventDetail.IsHidden = true;
            try
            {
                _unitOfWork.EventDetailRepository.Update(eventDetail);
                _unitOfWork.Save();
                RemoveJobForEvent<EventNotificationJob>(eventDetail.EventId, "Event Notification");
            }
            catch (Exception e)
            {
                serviceResult.Append(new ServiceResult($"Cannot delete this Event {eventDetail.Title} due to an internal error.",
                                        $"Cannot delete Event {eventDetail.Title} with Id: '{eventDetail.EventId}'. Error: {e}",
                                        false));
            }
            return serviceResult.SendResultToLog(_logger, "Delete", "Event");
        }

        public ServiceResult DeleteEventDetail(string id)
        {
            var eventDetail = GetEventDetailById(id);
            if (eventDetail != null)
            {
                return (DeleteEventDetail(eventDetail));
            }
            return new ServiceResult("Cannot delete as no Event is associated with this Id", false).SendResultToLog(_logger, "Delete", "Event");
        }

        public EventDetail? GetEventDetailById(string id, bool includeHidden = false)
        {
            var eventDetails = _unitOfWork.EventDetailRepository.Get(includeProperties: "EventItem").Where(x => x.EventId == id);
            if (!includeHidden)
                eventDetails = eventDetails.Where(x => x.IsHidden == false);
            return eventDetails.FirstOrDefault();
        }
        public EventItem? GetEventItemById(string id)
        {
            return _unitOfWork.EventItemRepository.GetById(id);
        }
        public EventWithDetailedEventItemsViewModel GetEventWithDetailedEventItemsViewModel(EventDetail eventDetail)
        {
            _logger.Information("Attempt to populate Event Detail: {@EventDetail}", eventDetail);
            var userName = "Anonymous";
            if (!String.IsNullOrEmpty(eventDetail.UserId))
            {
                var user = _unitOfWork.UserRepository.GetById(eventDetail.UserId);
                if (user != null)
                {
                    userName = user.UserName;
                }
            }
            var eventItemViewModel = new List<EventItemViewModel>();
            foreach (var eventItem in eventDetail.EventItem)
            {
                var count = _unitOfWork.UserRegistrationRepository.Get().Count(x => x.EventId == eventItem.EventId && x.ItemId == eventItem.ItemId);
                eventItemViewModel.Add(new EventItemViewModel { EventItem = eventItem, ItemsRegistered = count, ItemsServed = 0 });
            }
            return new EventWithDetailedEventItemsViewModel { EventDetail = eventDetail, CreatedBy = $"Created By {userName}", EventItemViewModels = eventItemViewModel };
        }

        public ParticipationListViewModel? GetParticipationListByEventId(string id)
        {
            var eventDetail = GetEventDetailById(id);
            if (eventDetail == null)
                return null;
            var userRegistrations = _unitOfWork.UserRegistrationRepository.Get().Where(x => x.EventId == id && x.Approved == true);

            return CreateParticipationListViewModel(userRegistrations, eventDetail);
        }

        #region Private Methods

        private ServiceResult SetImageForModel(ref EventItem eventItem, IFormFile? imageFile, bool required = false)
        {
            try
            {
                eventItem.Image = AddOrUpdateImage(eventItem.Image, imageFile);
            }
            catch (Exception e)
            {
                return new ServiceResult($"Cannot set this image for the Event Item {eventItem.ItemName}",
                                        $"Cannot set image for Event Item {eventItem.ItemName}, Id: '{eventItem.ItemId}': {e.Message}",
                                        false);
            }
            if (required && String.IsNullOrEmpty(eventItem.Image))
            {
                return new ServiceResult("Please upload an image for this Event Item", false);
            }
            return new ServiceResult();
        }
        private ServiceResult SetImageForModel(ref EventDetail eventDetail, IFormFile? imageFile, bool required = false)
        {
            try
            {
                eventDetail.MainImage = AddOrUpdateImage(eventDetail.MainImage, imageFile);
            }
            catch (Exception e)
            {
                return new ServiceResult($"Cannot set image for the Event {eventDetail.Title}",
                                        $"Cannot set image for Event Detail {eventDetail.Title}, Id: '{eventDetail.EventId}': {e.Message}",
                                        false);
            }
            if (required && String.IsNullOrEmpty(eventDetail.MainImage))
            {
                return new ServiceResult("Please upload a valid image for this Event", false);
            }
            return new ServiceResult();
        }
        /// <summary>
        /// Add a new Image, or update an existing Image to the host, then returns the path to its location. Returns null if input image is null
        /// </summary>
        private string? AddOrUpdateImage(string? oldImageName, IFormFile? inputImage)
        {
            string? newImageName = oldImageName;
            if (inputImage != null && inputImage.Length > 0)
            {
                _logger.Information("Attempt to add or update Image");
                string newImageExtension = Path.GetExtension(inputImage.FileName);
                newImageName = Guid.NewGuid().ToString() + newImageExtension;
                if (!string.IsNullOrEmpty(oldImageName))
                {
                    var existingImageName = Path.GetFileNameWithoutExtension(oldImageName);
                    var existingImagePath = Path.Combine(filePath, existingImageName + Path.GetExtension(oldImageName));
                    //delete old image
                    _logger.Information("Remove exiting image with path {Path}", existingImagePath);
                    if (File.Exists(existingImagePath))
                    {
                        File.Delete(existingImagePath);
                    }
                }
                _logger.Information("Updating Event Item image with file {InputImageName}", inputImage.FileName);
                using (var stream = new FileStream(Path.Combine(filePath, newImageName), FileMode.Create))
                {
                    inputImage.CopyTo(stream);
                }
            }
            return newImageName;
        }

        private ServiceResult CreateNewEventItems(ref EventDetail eventDetail, List<EventItemImagesViewModel> eventItemImages)
        {
            var serviceResult = new ServiceResult();
            if (eventDetail.EventItem != null && eventDetail.EventItem.Count > 0 && eventItemImages.Count > 0)
            {
                for (int i = 0; i < eventDetail.EventItem.Count; i++)
                {
                    serviceResult.Append(CreateNewEventItem(ref eventDetail, i, eventItemImages[i]));
                }

                RemoveUnusedEventItem(ref eventDetail);

                if (eventDetail.EventItem.Count == 0)
                {
                    serviceResult.Append(new ServiceResult("No valid Event Item found. Did you remove all Event Items from the form?", false));
                }
                return serviceResult;
            }
            return serviceResult.UpdateSingle("No valid Event Item found.", "", false);
        }

        private ServiceResult CreateNewEventItem(ref EventDetail eventDetail, int index, EventItemImagesViewModel eventItemImage)
        {
            try
            {
                var eventItem = eventDetail.EventItem.ToList()[index];
                if (eventItem.EventId == "1")
                {
                    eventItem.EventId = eventDetail.EventId;
                    eventItem.ItemId = String.IsNullOrEmpty(eventItem.ItemId) ? Guid.NewGuid().ToString() : eventItem.ItemId;
                    return SetImageForModel(ref eventItem, eventItemImage.EventItemImage);
                }
            }
            catch (Exception e)
            {
                return new ServiceResult($"Unable to add Event Item {index + 1}",
                    $"Unable to add Event Item {index + 1} for Event {eventDetail.Title}, error: {e.Message}",
                    false);
            }
            return new ServiceResult();
        }
        private static void RemoveUnusedEventItem(ref EventDetail eventDetail)
        {
            var unusedEventItem = eventDetail.EventItem.Where(x => x.EventId == "0").ToList();
            foreach (var eventItem in unusedEventItem)
            {
                eventDetail.EventItem.Remove(eventItem);
            }
        }
        private void RemoveUnusedEventItem(ref EventDetail eventDetail, ref List<EventItem> eventItems, ref List<EventItemImagesViewModel>? eventItemImages)
        {
            var unusedEventItems = eventItems.Where(x => x.EventId == "0").ToList();
            foreach (var unusedEventItem in unusedEventItems)
            {
                _logger.Information("Remove following Item from Event: {@EventItem}", unusedEventItem);
                var index = eventItems.IndexOf(unusedEventItem);
                if(eventItemImages != null && eventItemImages.Count > index)
                {
                    _logger.Information("The following image was removed as well: {@EventItemImage}", eventItemImages[index]);
                    eventItemImages.RemoveAt(index);
                }
                eventItems.Remove(unusedEventItem);
                var eventItem = eventDetail.EventItem.FirstOrDefault(x => x.ItemId == unusedEventItem.ItemId);
                if (eventItem != null)
                {
                    DeleteEventItem(ref eventDetail, eventItem);
                }
            }
        }
        private ServiceResult UpdateEventDetailModel(ref EventDetail eventDetail, EventDetailViewModel eventDetailViewModel, bool restrictedMode)
        {
            try
            {
                var updatedEventDetail = eventDetailViewModel.EventDetail;
                if (!EditRestrictedModeValidCheck(eventDetail, updatedEventDetail, out string[]? errorDates) && errorDates != null && restrictedMode)
                    return new ServiceResult("At least one date was changed", $"Edit Restricted Mode Error: at least one date was changed: {String.Join(",", errorDates)}", false);
                eventDetail.Title = updatedEventDetail.Title;
                eventDetail.RegistrationEndDate = updatedEventDetail.RegistrationEndDate;
                eventDetail.RegistrationStartDate = updatedEventDetail.RegistrationStartDate;
                eventDetail.EventTime = updatedEventDetail.EventTime;
                eventDetail.Message = updatedEventDetail.Message;

                //replace existing image. Do nothing if the input IFormFile is null
                return SetImageForModel(ref eventDetail, eventDetailViewModel.Image);
            }
            catch (Exception e)
            {
                return new ServiceResult("Failed to update Event due to an internal error.",
                    $"Failed to update Event {eventDetail.Title} with Id '{eventDetail.EventId}', error: {e}",
                    false);
            }
        }
        private ServiceResult UpdateEventItems(ref EventDetail eventDetail, List<EventItem> eventItems, List<EventItemImagesViewModel>? eventItemImages, bool restrictedMode)
        {
            if (eventItems != null && eventItems.Count > 0)
            {
                if (restrictedMode && !EditRestrictedModeValidCheck(eventDetail, eventItems, out string[]? eventItemNames) && eventItemNames != null)
                {
                    return new ServiceResult("At least one existing Event Item was removed", $"Edit Restricted Mode Error: At least one existing Event Item was removed: {String.Join(",", eventItemNames)} ", false);
                }

                RemoveUnusedEventItem(ref eventDetail, ref eventItems, ref eventItemImages);

                if (eventDetail.EventItem.Count > 0)
                {
                    return UpdateEventItemsWithNullCheck(ref eventDetail, eventItems, eventItemImages);
                }
            }
            return new ServiceResult("No valid Event Item exists", "", false);
        }
        private ServiceResult UpdateEventItemsWithNullCheck(ref EventDetail eventDetail, List<EventItem> eventItems, List<EventItemImagesViewModel>? eventItemImages)
        {
            var serviceResult = new ServiceResult();
            _logger.Information("Attempt to update the following Event Items: {@EventItems}", eventItems);
            for (int i = 0; i < eventItems.Count; i++)
            {
                if (eventItemImages != null && eventItemImages.Count >= i + 1)
                    serviceResult.Append(UpdateEventItem(ref eventDetail, eventItems[i], eventItemImages[i]));
                else
                    serviceResult.Append(UpdateEventItem(ref eventDetail, eventItems[i], null));
            }
            return serviceResult;
        }
        private ServiceResult UpdateEventItem(ref EventDetail eventDetail, EventItem inputEventItem, EventItemImagesViewModel? eventItemImage)
        {
            _logger.Information("Attemp to add or update Event Item using following model: {@EventItem}", inputEventItem);
            if (!string.IsNullOrEmpty(inputEventItem.EventId))
            {
                if (inputEventItem.EventId == eventDetail.EventId)
                {
                    var eventItem = FindOrCreateNewEventItem(ref eventDetail, inputEventItem.ItemId);
                    if (eventItem != null)
                    {
                        var serviceResult = new ServiceResult();

                        eventItem.Description = inputEventItem.Description;
                        eventItem.ItemName = inputEventItem.ItemName;
                        eventItem.Amount = inputEventItem.Amount;
                        if (eventItemImage != null)
                        {
                            serviceResult.Append(SetImageForModel(ref eventItem, eventItemImage.EventItemImage));
                        }
                        return serviceResult;
                    }
                    return new ServiceResult($"Cannot add or update {inputEventItem.ItemName}",
                            $"Cannot add or update Event Item {inputEventItem.ItemName} with '{inputEventItem.EventId}'",
                            false); ;
                }
                if (inputEventItem.EventId == "0")
                    return new ServiceResult(); //this item will be removed later
            }
            return new ServiceResult("Unexpected behavior occurred",
                    $"Cannot update Event Item with ItemName '{inputEventItem.ItemName}' for the current Event Detail with Event Id '{eventDetail.EventId}'",
                    false);
        }
        private EventItem? FindOrCreateNewEventItem(ref EventDetail eventDetail, string eventItemId)
        {
            EventItem? eventItem;
            if (string.IsNullOrEmpty(eventItemId) || eventItemId == "0")
            {
                eventItem = new EventItem
                {
                    EventDetail = eventDetail,
                    EventId = eventDetail.EventId,
                    ItemId = Guid.NewGuid().ToString()
                };
                eventDetail.EventItem.Add(eventItem);
            }
            else
            {
                eventItem = eventDetail.EventItem.FirstOrDefault(x => x.ItemId == eventItemId);
                _logger.Information("Event Item Found from database: {@EventItem}", eventItem);
            }
            return eventItem;
        }
        private ServiceResult DeleteEventItem(ref EventDetail eventDetail, EventItem inputEventItem)
        {
            if (!string.IsNullOrEmpty(inputEventItem.ItemId))
            {
                var eventItem = eventDetail.EventItem.FirstOrDefault(x => x.ItemId == inputEventItem.ItemId);
                if (eventItem != null)
                {
                    try
                    {
                        eventDetail.EventItem.Remove(eventItem!);
                        _unitOfWork.EventItemRepository.Delete(eventItem!);
                        return new ServiceResult();
                    }
                    catch (Exception e)
                    {
                        return new ServiceResult($"Failed to delete Event Item {inputEventItem.ItemName} due to an internal error.",
                            $"Failed to delete Event Item Title: '{inputEventItem.ItemName}' with id '{inputEventItem.ItemId}', error: {e}",
                            false);
                    }
                }
                return new ServiceResult($"This Event Item doesn't exist in the database",
                        $"This Event Item '{inputEventItem.ItemName}' with id '{inputEventItem.ItemId}' doesn't exist in the database",
                        false);
            }
            return new ServiceResult("Item Not Found", "ItemId is null", false);
        }

        private void ScheduleJobForEvent<T>(DateTime eventDate, string eventId, string group, bool reschedule = false) where T : IJob
        {
            var scheduler = _schedulerFactory.GetScheduler().Result;
            var trigger = TriggerBuilder.Create().WithIdentity(eventId, group)
                .WithCronSchedule($"{eventDate.Second + 10} {eventDate.Minute} {eventDate.Hour} {eventDate.Day} {eventDate.Month} ? {eventDate.Year}").Build();
            if (!reschedule)
            {
                var job = JobBuilder.Create<T>().WithIdentity(eventId, group).Build();
                scheduler.ScheduleJob(job, trigger);
            }
            else
                scheduler.RescheduleJob(new TriggerKey(eventId, group), trigger);
        }

        private void RemoveJobForEvent<T>(string eventId, string group) where T : IJob
        {

            var scheduler = _schedulerFactory.GetScheduler().Result;
            scheduler.UnscheduleJob(new TriggerKey(eventId, group));
        }

        private ParticipationListViewModel CreateParticipationListViewModel(IEnumerable<UserRegistration> userRegistrations, EventDetail eventDetail)
        {

            return new ParticipationListViewModel
            {
                EventDetail = eventDetail,
                UserRegistrationViewModels = CreateUserRegistrationViewModelList(userRegistrations),
                EventItemViewModels = CreateEventItemViewModelList(eventDetail.EventItem)
            };
        }
        private List<EventItemViewModel> CreateEventItemViewModelList(IEnumerable<EventItem> eventItems)
        {
            var list = new List<EventItemViewModel>();
            foreach (EventItem eventItem in eventItems)
            {
                list.Add(CreateEventItemViewModel(eventItem));
            }
            return list;
        }
        private EventItemViewModel CreateEventItemViewModel(EventItem eventItem)
        {
            var approvedRegistrations = _unitOfWork.UserRegistrationRepository.Get(x => x.ItemId == eventItem.ItemId && x.Approved == true);
            var servedRegistrations = approvedRegistrations.Where(x => x.Served == true);
            return new EventItemViewModel
            {
                EventItem = eventItem,
                ItemsRegistered = approvedRegistrations.Count(),
                ItemsServed = servedRegistrations.Count()
            };
        }

        public List<UserRegistrationViewModel> CreateUserRegistrationViewModelList(IEnumerable<UserRegistration> userRegistrations)
        {
            var userRegistrationsViewModels = new List<UserRegistrationViewModel>();

            foreach (var userRegistration in userRegistrations)
            {
                userRegistrationsViewModels.Add(CreateUserRegistrationViewModel(userRegistration));
            }
            return userRegistrationsViewModels;
        }

        private UserRegistrationViewModel CreateUserRegistrationViewModel(UserRegistration userRegistration)
        {
            var user = _unitOfWork.UserRepository.GetById(userRegistration.UserId)!;
            var eventItem = GetEventItemById(userRegistration.ItemId)!;
            var userRegistrationConfirmation = _unitOfWork.UserRegistrationConfirmationRepository.Get().FirstOrDefault(x => x.UserRegistrationId == userRegistration.RegistrationId);
            var confirmStatus = ConfirmStatus.Pending;
            if (userRegistrationConfirmation != null)
            {
                confirmStatus = userRegistrationConfirmation.ConfirmStatus;
            }
            return new UserRegistrationViewModel
            (
                user,
                userRegistration.RegistrationId,
                eventItem.ItemName,
                userRegistration.Served,
                confirmStatus,
                userRegistration.CheckInTime
            );
        }

        private static bool EditRestrictedModeValidCheck(EventDetail eventDetail, EventDetail updatedEventDetail, out string[]? errorDate)
        {
            var errorDateList = new List<string>();
            var valid = true;
            if (!ValidateDateTime.CompareDate(eventDetail.RegistrationStartDate, updatedEventDetail.RegistrationStartDate))
            {
                errorDateList.Add("Registration StartDate,");
                valid = false;
            }
            if (!ValidateDateTime.CompareDate(eventDetail.RegistrationEndDate, updatedEventDetail.RegistrationEndDate))
            {
                errorDateList.Add("Registration EndDate");
                valid = false;
            }
            if (!ValidateDateTime.CompareDate(eventDetail.EventTime, updatedEventDetail.EventTime))
            {
                errorDateList.Add("Event Time");
                valid = false;
            }
            errorDate = errorDateList.ToArray();
            return valid;
        }
        private static bool EditRestrictedModeValidCheck(EventDetail eventDetail, List<EventItem> updatedEventItems, out string[]? eventItemNames)
        {
            var eventItemNameList = new List<string>();
            var valid = true;
            foreach (var eventItem in eventDetail.EventItem)
            {
                var matchingEventItem = updatedEventItems.Find(x => x.ItemId == eventItem.ItemId);
                //EventId = 0 means it is removed
                if (matchingEventItem != null && matchingEventItem.EventId == "0")
                {
                    eventItemNameList.Add(matchingEventItem.ItemName!);
                    valid = false;
                }
            }
            eventItemNames = eventItemNameList.ToArray();
            return valid;
        }

        #endregion
    }
}
