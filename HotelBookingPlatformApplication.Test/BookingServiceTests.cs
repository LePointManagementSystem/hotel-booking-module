using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using FluentAssertions;
using HotelBookingPlatform.Application.Core.Implementations.BookingManagementService;
using HotelBookingPlatform.Domain.Abstracts;
using HotelBookingPlatform.Domain;
using HotelBookingPlatform.Domain.Entities;
using HotelBookingPlatform.Domain.Enums;
using HotelBookingPlatform.Application.Core.Abstracts.IBookingManagementService;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using HotelBookingPlatform.Application.Core.Abstracts.NotificationManagementService;


namespace HotelBookingPlatformApplication.Test
{
    public class BookingServiceTests
    {
        private BookingService CreateService(
            Mock<IUnitOfWork<Booking>> mockUow,
            Mock<IMapper>? mapper = null)
        {
            var confirmationMock = new Mock<IConfirmationNumberGeneratorService>();
            confirmationMock.Setup(x => x.GenerateConfirmationNumber()).Returns("CONF123");

            var priceCalcMock = new Mock<IPriceCalculationService>();
            priceCalcMock
                .Setup(x => x.CalculateTotalPriceAsync(It.IsAny<List<int>>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(0m);

            priceCalcMock
                .Setup(x => x.CalculateDiscountedPriceAsync(It.IsAny<List<int>>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(0m);

            var userManager = CreateUserManagerMock();

            var notificationServiceMock = new Mock<INotificationService>();

return new BookingService(
    mockUow.Object,
    mapper?.Object ?? new Mock<IMapper>().Object,
    confirmationMock.Object,
    priceCalcMock.Object,
    userManager,
    notificationServiceMock.Object
);
        }

        private static UserManager<LocalUser> CreateUserManagerMock()
        {
            var store = new Mock<IUserStore<LocalUser>>();

            var options = new Mock<IOptions<IdentityOptions>>();
            options.Setup(o => o.Value).Returns(new IdentityOptions());

            var passwordHasher = new Mock<IPasswordHasher<LocalUser>>();
            var userValidators = new List<IUserValidator<LocalUser>> { new Mock<IUserValidator<LocalUser>>().Object };
            var pwdValidators = new List<IPasswordValidator<LocalUser>> { new Mock<IPasswordValidator<LocalUser>>().Object };

            var lookupNormalizer = new Mock<ILookupNormalizer>();
            var errorDescriber = new IdentityErrorDescriber();
            var services = new Mock<IServiceProvider>();
            var logger = new Mock<ILogger<UserManager<LocalUser>>>();

            return new UserManager<LocalUser>(
                store.Object,
                options.Object,
                passwordHasher.Object,
                userValidators,
                pwdValidators,
                lookupNormalizer.Object,
                errorDescriber,
                services.Object,
                logger.Object
            );
        }

        [Fact]
        public async Task ReleaseExpiredBookingsAsync_NoExpiredBookings_ReturnsEmpty()
        {
            var mockUow = new Mock<IUnitOfWork<Booking>>();
            var mockBookingRepo = new Mock<IBookingRepository>();
            var mockRoomRepo = new Mock<IRoomRepository>();

            mockBookingRepo
                .Setup(x => x.GetExpiredBookingsWithRoomsAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(new List<Booking>());

            mockUow.SetupGet(u => u.BookingRepository).Returns(mockBookingRepo.Object);
            mockUow.SetupGet(u => u.RoomRepository).Returns(mockRoomRepo.Object);

            var service = CreateService(mockUow);

            var result = await service.ReleaseExpiredBookingsAsync();

            result.Should().BeEmpty();
            mockBookingRepo.Verify(b => b.UpdateAsync(It.IsAny<int>(), It.IsAny<Booking>()), Times.Never);
            mockRoomRepo.Verify(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<Room>()), Times.Never);
            mockUow.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task ReleaseExpiredBookingsAsync_ExpiredBooking_ProcessesAndReturnsData()
        {
            var mockUow = new Mock<IUnitOfWork<Booking>>();
            var mockBookingRepo = new Mock<IBookingRepository>();
            var mockRoomRepo = new Mock<IRoomRepository>();

            var room = new Room { RoomID = 10, Number = "101", IsAvailable = false };
            var booking = new Booking
            {
                BookingID = 1,
                Status = BookingStatus.Confirmed,
                CheckOutDateUtc = DateTime.UtcNow.AddMinutes(-5),
                Rooms = new List<Room> { room }
            };

            mockBookingRepo
                .Setup(x => x.GetExpiredBookingsWithRoomsAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(new List<Booking> { booking });

            mockBookingRepo
                .Setup(x => x.UpdateAsync(booking.BookingID, booking))
                .ReturnsAsync(booking);

            mockRoomRepo
                .Setup(x => x.UpdateAsync(room.RoomID, room))
                .ReturnsAsync(room);

            mockUow.SetupGet(u => u.BookingRepository).Returns(mockBookingRepo.Object);
            mockUow.SetupGet(u => u.RoomRepository).Returns(mockRoomRepo.Object);
            mockUow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var service = CreateService(mockUow);

            var result = await service.ReleaseExpiredBookingsAsync();

            result.Should().HaveCount(1);
            booking.Status.Should().Be(BookingStatus.Completed);
            room.IsAvailable.Should().BeTrue();

            mockBookingRepo.Verify(b => b.UpdateAsync(booking.BookingID, booking), Times.Once);
            mockRoomRepo.Verify(r => r.UpdateAsync(room.RoomID, room), Times.Once);
            mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task ReleaseExpiredBookingsAsync_AlreadyCompletedBooking_Ignored()
        {
            var mockUow = new Mock<IUnitOfWork<Booking>>();
            var mockBookingRepo = new Mock<IBookingRepository>();
            var mockRoomRepo = new Mock<IRoomRepository>();

            var room = new Room { RoomID = 20, Number = "202", IsAvailable = false };
            var booking = new Booking
            {
                BookingID = 2,
                Status = BookingStatus.Completed,
                CheckOutDateUtc = DateTime.UtcNow.AddMinutes(-10),
                Rooms = new List<Room> { room }
            };

            mockBookingRepo
                .Setup(x => x.GetExpiredBookingsWithRoomsAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(new List<Booking> { booking });

            mockUow.SetupGet(u => u.BookingRepository).Returns(mockBookingRepo.Object);
            mockUow.SetupGet(u => u.RoomRepository).Returns(mockRoomRepo.Object);

            var service = CreateService(mockUow);

            var result = await service.ReleaseExpiredBookingsAsync();

            result.Should().BeEmpty();
            mockBookingRepo.Verify(b => b.UpdateAsync(It.IsAny<int>(), It.IsAny<Booking>()), Times.Never);
            mockRoomRepo.Verify(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<Room>()), Times.Never);
            mockUow.Verify(u => u.SaveChangesAsync(), Times.Never);
        }
    }
}
