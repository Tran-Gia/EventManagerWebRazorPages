using EventManagerWebRazorPage.Areas.Identity.Data;
using EventManagerWebRazorPage.Models;
using Microsoft.AspNetCore.Identity;

namespace EventManagerWebRazorPage.DAL
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<User> UserRepository { get; }
        IGenericRepository<UserRegistration> UserRegistrationRepository { get; }
        IGenericRepository<UserRegistrationConfirmation> UserRegistrationConfirmationRepository { get; }
        IGenericRepository<EventDetail> EventDetailRepository { get; }
        IGenericRepository<EventItem> EventItemRepository { get; }
        IGenericRepository<EventItemOption> EventItemOptionRepository { get; }
        IGenericRepository<IdentityRole> RoleRepository { get; }
        void Save();
        new void Dispose();
    }
}
