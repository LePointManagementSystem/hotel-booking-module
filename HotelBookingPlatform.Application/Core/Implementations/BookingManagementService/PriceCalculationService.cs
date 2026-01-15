using HotelBookingPlatform.Application.Core.Abstracts.IBookingManagementService;
namespace HotelBookingPlatform.Application.Core.Implementations.BookingManagementService;
public class PriceCalculationService : IPriceCalculationService
{
    private readonly IUnitOfWork<Booking> _unitOfWork;
    public PriceCalculationService(IUnitOfWork<Booking> unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<decimal> CalculateTotalPriceAsync(List<int> roomIds, DateTime checkInDate, DateTime checkOutDate)
    {
        decimal totalPrice = 0m;
        var duration = GetBookingDuration(checkInDate, checkOutDate);

        // For multi-day stays, price is calculated per night (Overnight) times number of nights.
        var totalDays = (checkOutDate - checkInDate).TotalDays;
        var nights = totalDays > 0 ? (int)Math.Ceiling(totalDays) : 0;

        foreach (var roomId in roomIds)
        {
            var room = await _unitOfWork.RoomRepository.GetByIdAsync(roomId);
            if (room is not null)
            {
                if (duration == BookingDurationType.Stay)
                {
                    var nightly = RoomPricing.GetPrice(room.RoomClass.RoomType, BookingDurationType.Overnight);
                    totalPrice += nightly * Math.Max(1, nights);
                }
                else
                {
                    totalPrice += RoomPricing.GetPrice(room.RoomClass.RoomType, duration);
                }
            }
        }

        return totalPrice;
    }

    public async Task<decimal> CalculateDiscountedPriceAsync(List<int> roomIds, DateTime checkInDate, DateTime checkOutDate)
    {
        decimal discountedTotalPrice = 0m;
        var duration = GetBookingDuration(checkInDate, checkOutDate);

        var totalDays = (checkOutDate - checkInDate).TotalDays;
        var nights = totalDays > 0 ? (int)Math.Ceiling(totalDays) : 0;

        foreach (var roomId in roomIds)
        {
            var room = await _unitOfWork.RoomRepository.GetByIdAsync(roomId);
            if (room is not null)
            {
                decimal basePrice;

                if (duration == BookingDurationType.Stay)
                {
                    var nightly = RoomPricing.GetPrice(room.RoomClass.RoomType, BookingDurationType.Overnight);
                    basePrice = nightly * Math.Max(1, nights);
                }
                else
                {
                    basePrice = RoomPricing.GetPrice(room.RoomClass.RoomType, duration);
                }
                var discount = await _unitOfWork.DiscountRepository.GetActiveDiscountForRoomAsync(roomId, checkInDate, checkOutDate);
                if (discount != null && discount.IsActive)
                {
                    basePrice *= (1 - (discount.Percentage / 100.0m));
                }
                discountedTotalPrice += basePrice;
            }
        }

        return discountedTotalPrice;
    }

    private BookingDurationType GetBookingDuration(DateTime checkInDate, DateTime checkOutDate)
    {
        var hours = (checkOutDate - checkInDate).TotalHours;
        return hours switch
        {
            <= 2 => BookingDurationType.Hours2,
            <= 4 => BookingDurationType.Hours4,
            // Up to 8 hours is treated as an "Overnight" price bucket (configurable later)
            <= 8 => BookingDurationType.Overnight,
            _ => BookingDurationType.Stay
        };
    }
}

