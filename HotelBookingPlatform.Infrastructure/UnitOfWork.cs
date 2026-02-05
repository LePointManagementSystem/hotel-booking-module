using System;
using HotelBookingPlatform.Domain;
using HotelBookingPlatform.Domain.Abstracts;
using HotelBookingPlatform.Domain.Entities;
using HotelBookingPlatform.Infrastructure.Data;
using HotelBookingPlatform.Infrastructure.Implementation;
using Microsoft.AspNetCore.Identity;

namespace HotelBookingPlatform.Infrastructure
{
    public class UnitOfWork<T> : IUnitOfWork<T> where T : class
    {
        private readonly AppDbContext _context;
        private readonly UserManager<LocalUser> _userManager;
        public ICashSessionRepository CashSessionRepository { get; set; }

        public UnitOfWork(AppDbContext context, UserManager<LocalUser> userManager)
        {
            _context = context;
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));

            HotelRepository = new HotelRepository(_context);
            BookingRepository = new BookingRepository(_context);
            RoomClasseRepository = new RoomClassRepository(_context);
            RoomRepository = new RoomRepository(_context);
            CityRepository = new CityRepository(_context);
            OwnerRepository = new OwnerRepository(_context);
            DiscountRepository = new DiscountRepository(_context);
            ReviewRepository = new ReviewRepository(_context);
            InvoiceRecordRepository = new InvoiceRecordRepository(_context);
            AmenityRepository = new AmenityRepository(_context);
            UserRepository = new UserRepository(_userManager, _context);
            ImageRepository = new ImageRepository(_context);
            StaffRepository = new StaffRepository(_context);
            GuestRepository = new GuestRepository(_context);

            NotificationRepository = new NotificationRepository(_context); // ✅ ok
            CashTransactionRepository = new CashTransactionRepository(_context);
            CashSessionRepository = new CashSessionRepository(_context);

        }

        public IHotelRepository HotelRepository { get; }
        public IBookingRepository BookingRepository { get; }
        public IRoomClasseRepository RoomClasseRepository { get; }
        public IRoomRepository RoomRepository { get; }
        public ICityRepository CityRepository { get; }
        public IOwnerRepository OwnerRepository { get; }
        public IDiscountRepository DiscountRepository { get; }
        public IReviewRepository ReviewRepository { get; }
        public IInvoiceRecordRepository InvoiceRecordRepository { get; }
        public IAmenityRepository AmenityRepository { get; }
        public IUserRepository UserRepository { get; }
        public IImageRepository ImageRepository { get; }
        public IStaffRepository StaffRepository { get; }
        public IGuestRepository GuestRepository { get; }

        public INotificationRepository NotificationRepository { get; }
        public ICashTransactionRepository CashTransactionRepository { get; }

        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}
