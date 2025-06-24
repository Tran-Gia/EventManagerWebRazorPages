using EventManagerWebRazorPage.Areas.Identity.Data;
using EventManagerWebRazorPage.Models;
using EventManagerWebRazorPage.Services;
using EventManagerWebRazorPage.Services.Other;
using EventManagerWebRazorPage.ViewModels;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace EventManagerWebRazorPage.Controllers
{
    [Authorize]
    public class EventController : Controller
    {
        readonly string imagePath = "/Images/";
        private readonly IEventService _eventService;
        private readonly IUserRegistrationService _userRegistrationService;
        private readonly IUserService _userService;
        private readonly IValidator<EventDetailViewModel> _eventDetailViewModelValidator;
        private readonly SignInManager<User> _signInManager;

        public EventController(IEventService eventService, IUserRegistrationService userRegistrationService, IUserService userService,
             IValidator<EventDetailViewModel> eventDetailViewModelValidator, SignInManager<User> signInManager)
        {
            _eventService = eventService;
            _userRegistrationService = userRegistrationService;
            _userService = userService;
            _eventDetailViewModelValidator = eventDetailViewModelValidator;
            _signInManager = signInManager;
        }

        // GET: Event
        public IActionResult Index()
        {
            var signedIn = _signInManager.IsSignedIn(User);
            if (!signedIn)
            {
                return Redirect("Identity/Account/Login");
            }
            ViewBag.ImagePath = imagePath;
            return View(_eventService.GetEventDetailList());
        }

        // GET: Event/Details/5
        public IActionResult Details(string id)
        {
            ViewBag.ImagePath = imagePath;
            if (id == null)
            {
                return NotFound();
            }

            var eventDetail = _eventService.GetEventDetailById(id);
            if (eventDetail == null)
            {
                return NotFound();
            }

            if(_userService.UserNotNullAndContainsRole(User, "Participant", out var user))
            {
                GetUserRegistrationResult(eventDetail, user!.Id);
            }
            return View(_eventService.GetEventWithDetailedEventItemsViewModel(eventDetail));
        }

        [Authorize(Roles = "Participant")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RegisterToEvent(IFormCollection formCollections)
        {
            string? itemId = formCollections.FirstOrDefault(x => x.Key == "ItemButton").Value;
            var eventId = formCollections.FirstOrDefault(x => x.Key == "EventDetail.EventId").Value;
            var eventDetail = !String.IsNullOrEmpty(eventId) ? _eventService.GetEventDetailById(eventId!) : null;
            if(eventDetail != null)
            {
                if (_userService.UserNotNullAndContainsRole(User, "Participant", out var user) && !String.IsNullOrEmpty(itemId) && DateTime.Now.ValidDateTime(eventDetail))
                {
                    TempData["RegisterToEventMessage"] = RegisterToEventResult(eventDetail.EventId, user!.Id, itemId);
                }
                else
                {
                    TempData["RegisterToEventMessage"] = "Invalid attempt to register to the event";
                }
                return RedirectToAction(nameof(Details), new { id = eventDetail.EventId });
            }
            return NotFound();
        }

        // GET: Event/Create
        [Authorize(Roles = "Organizer")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Event/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Organizer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(EventDetailViewModel eventDetailViewModel)
        {
            _userService.GetUserAndRole(User, out var user);
            if(user != null)
                eventDetailViewModel.EventDetail.UserId = user.Id;
            ModelState.ValidateEventDetailViewModel(eventDetailViewModel, _eventDetailViewModelValidator);
            if (ModelState.IsValid)
            {
                var service = _eventService.CreateNewEventDetail(eventDetailViewModel);
                if (service.Result)
                    return RedirectToAction(nameof(Details), new { id = eventDetailViewModel.EventDetail.EventId });
                else
                {
                    ModelState.AddModelError("", service.Message);
                }
            }
            return View(eventDetailViewModel);
        }

        // GET: Event/Edit/5
        [Authorize(Roles = "Organizer")]
        public IActionResult Edit(string id)
        {
            ViewBag.ImagePath = imagePath;
            if (id == null)
            {
                return NotFound();
            }

            var eventDetail = _eventService.GetEventDetailById(id);
            if (eventDetail == null)
            {
                return NotFound();
            }
            var eventDetailViewModel = new EventDetailViewModel { Image = null, EventDetail = eventDetail, EventItemImagesViewModels = CreateEventItemImagesViewModels(eventDetail.EventItem.Count) };
            return EditEventDetailTimerCheck(eventDetail,eventDetailViewModel);
        }

        // POST: Event/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Organizer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(string id, EventDetailViewModel eventDetailViewModel, List<EventItem> eventItems)
        {
            ViewBag.ImagePath = imagePath;
            var eventDetail = _eventService.GetEventDetailById(id);
            if (eventDetail == null || id != eventDetail.EventId)
            {
                return NotFound();
            }
            var restrictedMode = false;
            if (DateTime.Now > eventDetail.RegistrationStartDate)
                restrictedMode = true;

            eventDetailViewModel.EventDetail.EventItem = eventItems;
            ModelState.ValidateEventDetailViewModel(eventDetailViewModel, _eventDetailViewModelValidator, restrictedMode);

            if (ModelState.IsValid)
            {
                var service = _eventService.UpdateEventDetail(eventDetailViewModel);
                if (service.Result)
                    return RedirectToAction(nameof(Details),new {id});
                else
                {
                    ModelState.AddModelError("", service.Message);
                }
            }
            return EditEventDetailTimerCheck(eventDetail, eventDetailViewModel);
        }

        // GET: Event/Delete/5
        [Authorize(Roles = "Organizer")]
        public IActionResult Delete(string id)
        {
            ViewBag.ImagePath = imagePath;
            if (id == null)
            {
                return NotFound();
            }

            var eventDetail = _eventService.GetEventDetailById(id);
            if (eventDetail == null)
            {
                return NotFound();
            }

            return View(eventDetail);
        }

        // POST: Event/Delete/5
        [Authorize(Roles = "Organizer")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(string id)
        {
            var eventDetail = _eventService.GetEventDetailById(id);
            if (eventDetail == null)
            {
                return NotFound();
            }
            var service = _eventService.DeleteEventDetail(id);
            if (service.Result)
            {
                return RedirectToAction(nameof(Index));
            }
            else
            {
                ModelState.AddModelError("", service.Message);
            }
            return View(eventDetail);
        }

        [Authorize(Roles = "Organizer")]
        public IActionResult ParticipationList(string id)
        {
            var participantListViewModel = _eventService.GetParticipationListByEventId(id);
            return View(participantListViewModel);
        }

        [Authorize]
        public IActionResult ConfirmParticipation(string id, ConfirmStatus status)
        {
            //TODO: requires logging in
            var serviceResult = _userRegistrationService.UpdateConfirmStatus(id, status);
            if (serviceResult.Result)
                TempData["UpdateParticipationStatusResult"] = "We have received your response! Thank you.";
            else
                TempData["UpdateParticipationStatusResult"] = serviceResult.Message;
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Organizer")]
        [HttpPost]
        public JsonResult MarkRegistrationAsServed(string regisId, bool served)
        {
            var url = $"{this.Request.Scheme}://{this.Request.Host}/Event/ConfirmParticipation";
            var serviceResult = _userRegistrationService.SetServedStatus(regisId, out var checkinTime, url, served);
            if (serviceResult.Result)
            {
                if(checkinTime != null)
                {
                    return new JsonResult(Ok(new { checkinTime = checkinTime!.Value.ToString("hh:mm:ss") }));
                }
                return new JsonResult(Ok());
            }
            return new JsonResult(BadRequest(error: serviceResult.Message));
        }

        [Authorize(Roles = "Organizer")]
        [HttpGet]
        public JsonResult SearchUserRegistration(string internalId, string eventId)
        {
            var userRegistrations = _userRegistrationService.GetPartialParticipationList(eventId, internalId).ToList();
            return new JsonResult(Ok(new { userRegistrations }));
        }

        private string RegisterToEventResult(string eventId, string userId, string itemId)
        {
            var userRegis = _userRegistrationService.GetUserRegistration(eventId, userId);
            if (userRegis is null)
            {
                var serviceResult = _userRegistrationService.CreateNewRegistration(eventId, userId, itemId, out _);
                if (serviceResult.Result)
                    return "Your registration has been recorded successfully";
                else
                    return serviceResult.Message;
            }
            return "Worry not! You have already registered to this event!";
        }

        private void GetUserRegistrationResult (EventDetail eventDetail, string userId)
        {
            var userRegis = _userRegistrationService.GetUserRegistration(eventDetail.EventId, userId);
            if (userRegis is not null)
            {
                ViewData["UserRegistrationItemId"] = userRegis.ItemId;
            }
            if (DateTime.Now.ValidDateTime(eventDetail))
            {
                ViewData["registrationAvailable"] = 1;
            }
        }

        private static List<EventItemImagesViewModel> CreateEventItemImagesViewModels(int count)
        {
            var eventItemImagesViewModels = new List<EventItemImagesViewModel>();
            for (int i = 0; i < count; i++)
            {
                var eventItemImagesViewModel = new EventItemImagesViewModel { EventItemImage = null, EventItemIndex = i };
                eventItemImagesViewModels.Add(eventItemImagesViewModel);
            }
            return eventItemImagesViewModels;
        }

        private IActionResult EditEventDetailTimerCheck(EventDetail eventDetail, EventDetailViewModel eventDetailViewModel)
        {

            if (DateTime.Now > eventDetail.RegistrationEndDate)
            {
                TempData["EditTimeExpired"] = "You can no longer update this Event as the timer for Registration has ended";
                return RedirectToAction(nameof(Details), new { id= eventDetail.EventId });
            }

            if (DateTime.Now > eventDetail.RegistrationStartDate)
            {
                ViewData["EditRestrictedMode"] = "Please note that as the time for Registration has started, only a select of limited fields can be updated";
            }
            return View(eventDetailViewModel);
        }
    }
}
