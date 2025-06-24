using EventManagerWebRazorPage.Areas.Identity.Data;
using EventManagerWebRazorPage.Data;
using EventManagerWebRazorPage.Models;
using Microsoft.AspNetCore.Identity;

namespace EventManagerWebRazorPage.DAL
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IGenericRepository<User>? _userRepository;
        private IGenericRepository<UserRegistration>? _userRegistrationRepository;
        private IGenericRepository<UserRegistrationConfirmation>? _userRegistrationConfirmationRepository;
        private IGenericRepository<EventDetail>? _eventDetailRepository;
        private IGenericRepository<EventItem>? _eventItemRepository;
        private IGenericRepository<EventItemOption>? _eventItemOptionRepository;
        private IGenericRepository<IdentityRole>? _roleRepository;

        public IGenericRepository<User> UserRepository
        {
            get
            {
                if(_userRepository == null)
                {
                    _userRepository = new GenericRepository<User>(_context);
                }
                return _userRepository;
            }
        }
        public IGenericRepository<UserRegistration> UserRegistrationRepository
        {
            get
            {
                if (_userRegistrationRepository == null)
                {
                    _userRegistrationRepository = new GenericRepository<UserRegistration>(_context);
                }
                return _userRegistrationRepository;
            }
        }
        public IGenericRepository<UserRegistrationConfirmation> UserRegistrationConfirmationRepository
        {
            get
            {
                if (_userRegistrationConfirmationRepository == null)
                {
                    _userRegistrationConfirmationRepository = new GenericRepository<UserRegistrationConfirmation>(_context);
                }
                return _userRegistrationConfirmationRepository;
            }
        }

        public IGenericRepository<EventDetail> EventDetailRepository
        {
            get
            {
                if (_eventDetailRepository == null)
                {
                    _eventDetailRepository = new GenericRepository<EventDetail>(_context);
                }
                return _eventDetailRepository;
            }
        }

        public IGenericRepository<EventItem> EventItemRepository
        {
            get
            {
                if (_eventItemRepository == null)
                {
                    _eventItemRepository = new GenericRepository<EventItem>(_context);
                }
                return _eventItemRepository;
            }
        }

        public IGenericRepository<EventItemOption> EventItemOptionRepository
        {
            get
            {
                if (_eventItemOptionRepository == null)
                {
                    _eventItemOptionRepository = new GenericRepository<EventItemOption>(_context);
                }
                return _eventItemOptionRepository;
            }
        }

        public IGenericRepository<IdentityRole> RoleRepository
        {
            get
            {
                if (_roleRepository == null)
                {
                    _roleRepository = new GenericRepository<IdentityRole>(_context);
                }
                return _roleRepository;
            }
        }

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}
