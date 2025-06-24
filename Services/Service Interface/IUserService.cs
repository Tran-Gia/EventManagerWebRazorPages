using EventManagerWebRazorPage.Areas.Identity.Data;
using EventManagerWebRazorPage.Services.Other;
using System.Security.Claims;

namespace EventManagerWebRazorPage.Services
{
    public interface IUserService
    {
        IEnumerable<User> GetUserList();
        User? GetUserByEmail(string email);
        string GetUserAndRole(ClaimsPrincipal claimsPrincipal, out User? user);
        User? GetUserById(string id);
        ServiceResult UpdateUser(User user);
        bool UserNotNullAndContainsRole(ClaimsPrincipal claimsPrincipal, string requiredRole, out User? user);
    }
}
