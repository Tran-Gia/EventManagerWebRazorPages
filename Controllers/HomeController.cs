using EventManagerWebRazorPage.Areas.Identity.Data;
using EventManagerWebRazorPage.Models;
using EventManagerWebRazorPage.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace EventManagerWebRazorPage.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUserService _userService;
        private readonly IUserRegistrationService _userRegistrationService;
        private readonly SignInManager<User> _signInManager;

        public HomeController(IUserService userService, IUserRegistrationService userRegistrationService, SignInManager<User> signInManager)
        {
            _userService = userService;
            _userRegistrationService = userRegistrationService;
            _signInManager = signInManager;
        }

        public IActionResult Index()
        {
            var signedIn = _signInManager.IsSignedIn(User);
            if (!signedIn)
            {
                return Redirect("Identity/Account/Login");
            }
            return Redirect("Event");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public JsonResult GetUserInternalId(string internalId)
        {
            var exist = false;
            var user = _userService.GetUserList().FirstOrDefault(x => x.UserInternalId == internalId);
            exist = user != null;
            return new JsonResult(Ok(new { exist }));
        }

        public JsonResult GetUserEmail(string email)
        {
            var exist = false;
            var user = _userService.GetUserList().FirstOrDefault(x => x.Email != null && x.Email.Equals(email, StringComparison.CurrentCultureIgnoreCase));
            exist = user != null;
            return new JsonResult(Ok(new { exist }));
        }

        [Route("Identity/Account/Manage/RegistrationHistory")]
        [Route("Identity/Account/Manage/RegistrationHistory/{id}")]
        [Route("Identity/Account/RegistrationHistory")]
        [Route("Identity/Account/RegistrationHistory/{id}")]
        public IActionResult RegistrationHistory(string id = "", string eventId = "")
        {
            bool showDetailedInformation = false;
            var role = _userService.GetUserAndRole(User, out var user);
            if (role.Contains("Organizer"))
                showDetailedInformation = true;
            if (user != null)
            {
                if (!String.IsNullOrEmpty(id) && id != user.Id && !role.Contains("Organizer"))
                {
                    return NotFound();
                }
                if (String.IsNullOrEmpty(id))
                {
                    id = user.Id;
                }
            }
            var model = _userRegistrationService.GetUserRegistrationHistoryListViewModel(id, showDetailedInformation);
            if (model == null)
                return NotFound();
            if (!String.IsNullOrEmpty(eventId))
            {
                model.EventId = eventId;
            }
            return View(model);
        }
    }
}
