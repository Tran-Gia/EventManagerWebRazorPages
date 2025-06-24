using EventManagerWebRazorPage.Areas.Identity.Data;
using EventManagerWebRazorPage.DAL;
using EventManagerWebRazorPage.Services.Other;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using ILogger = Serilog.ILogger;

namespace EventManagerWebRazorPage.Services
{
    public class UserService :IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;
        private readonly ILogger _logger;
        public UserService(IUnitOfWork unitOfWork, UserManager<User> userManager, ILogger logger)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _logger = logger;
        }
        public IEnumerable<User> GetUserList()
        {
            return _unitOfWork.UserRepository.Get();
        }

        public User? GetUserByEmail(string email)
        {
            var users = _unitOfWork.UserRepository.Get(x => x.Email == email);
            return users.FirstOrDefault();
        }
        public User? GetUserById(string id)
        {
            return _unitOfWork.UserRepository.GetById(id);
        }
        public string GetUserAndRole(ClaimsPrincipal claimsPrincipal, out User? user)
        {
            _logger.Information("Attempt to get User through claimsPrincipal");
            string userRoles = string.Empty;
            user = _userManager.GetUserAsync(claimsPrincipal).Result;
            if (user != null)
            {
                _logger.Information("User found with Id: {UserId}", user.Id);
                var roles = _userManager.GetRolesAsync(user).Result;
                foreach (var role in roles)
                {
                    if(!String.IsNullOrEmpty(role))
                        userRoles += role + ",";
                }
                userRoles = userRoles.TrimEnd(',');
            }
            _logger.Information("User Roles found: {UserRoles}", userRoles);
            return userRoles;
        }

        public ServiceResult UpdateUser(User user)
        {
            _logger.Information("Attempt to update User with id: {UserId}", user.Id);
            try
            {
                _unitOfWork.UserRepository.Update(user);
                _unitOfWork.Save();
            }
            catch(Exception e)
            {
                return new ServiceResult("Cannot update User in database", $"An error has occured: {e}", false).SendResultToLog(_logger,"Update","User");
            }
            return new ServiceResult().SendResultToLog(_logger, "Update", "User");
        }

        public bool UserNotNullAndContainsRole(ClaimsPrincipal claimsPrincipal, string requiredRole, out User? user)
        {
            string roles = GetUserAndRole(claimsPrincipal, out user);
            return roles.Contains(requiredRole, StringComparison.CurrentCultureIgnoreCase) && user != null;
        }
    }
}
