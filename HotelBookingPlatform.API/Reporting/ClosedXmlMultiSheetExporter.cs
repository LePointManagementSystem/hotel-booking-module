using ClosedXML.Excel;
using HotelBookingPlatform.Application.Core.Abstracts.IHotelManagementService;
using HotelBookingPlatform.Application.Core.Abstracts.RoomClassManagementService;
using HotelBookingPlatform.Domain.DTOs.Hotel;

namespace HotelBookingPlatform.API.Reporting;

public static class ClosedXmlMultiSheetExporter
{
    public static async Task<byte[]> BuildHotelStructureAsync(
        IEnumerable<HotelResponseDto> hotels,
        IRoomClassService roomClassService,
        IHotelRoomService hotelRoomService
    )
    {
        using var wb = new XLWorkbook();

        // ---------------- Hotels
        var wsHotels = wb.Worksheets.Add("Hotels");
        ExcelReportWriter.WriteHeader(wsHotels, 1,
            "HotelId",
            "Name",
            "OwnerName",
            "CityName",
            "PhoneNumber",
            "StarRating",
            "ReviewsRating",
            "CreatedAtUtc",
            "Description"
        );

        int hr = 2;
        foreach (var h in hotels)
        {
            ExcelReportWriter.WriteRow(wsHotels, hr++,
                h.HotelId,
                h.Name,
                h.OwnerName,
                h.CityName,
                h.PhoneNumber,
                h.StarRating,
                h.ReviewsRating,
                h.CreatedAtUtc,
                h.Description ?? ""
            );
        }

        // ---------------- RoomClasses
        var wsRoomClasses = wb.Worksheets.Add("RoomClasses");
        ExcelReportWriter.WriteHeader(wsRoomClasses, 1,
            "RoomClassID",
            "HotelId",
            "HotelName",
            "RoomType",
            "Name",
            "Description"
        );

        // ---------------- Rooms
        var wsRooms = wb.Worksheets.Add("Rooms");
        ExcelReportWriter.WriteHeader(wsRooms, 1,
            "RoomId",
            "HotelId",
            "RoomClassId",
            "RoomClassName",
            "Number",
            "AdultsCapacity",
            "ChildrenCapacity",
            "PricePerNight",
            "CreatedAtUtc"
        );

        int rcRow = 2;
        int roomRow = 2;

        foreach (var h in hotels)
        {
            // ✅ Ton interface: GetRoomClassesByHotelId(int hotelId)
            var roomClasses = await roomClassService.GetRoomClassesByHotelId(h.HotelId);

            foreach (var rc in roomClasses)
            {
                // ✅ RoomClassResponseDto: RoomClassID, RoomType, Name, Description, HotelName, HotelId
                ExcelReportWriter.WriteRow(wsRoomClasses, rcRow++,
                    rc.RoomClassID,
                    rc.HotelId,
                    rc.HotelName ?? "",
                    rc.RoomType ?? "",
                    rc.Name ?? "",
                    rc.Description ?? ""
                );
            }

            // ✅ Ton interface: GetRoomsByHotelIdAsync(int hotelId)
            var rooms = await hotelRoomService.GetRoomsByHotelIdAsync(h.HotelId);

            foreach (var r in rooms)
            {
                // ⚠️ Ici j’assume que ton Room DTO contient bien ces champs.
                // Si certains n'existent pas dans TON Rooms DTO, dis-moi le modèle exact et j'ajuste.
                ExcelReportWriter.WriteRow(wsRooms, roomRow,
                    r.RoomId,
                    r.HotelId,
                    r.RoomClassId,
                    r.RoomClassName ?? "",
                    r.Number ?? "",
                    r.AdultsCapacity,
                    r.ChildrenCapacity,
                    r.PricePerNight,
                    "" // date écrite via WriteDate
                );

                ExcelReportWriter.WriteDate(wsRooms, roomRow, 9, r.CreatedAtUtc);
                roomRow++;
            }
        }

        wsHotels.Columns().AdjustToContents();
        wsRoomClasses.Columns().AdjustToContents();
        wsRooms.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }
}
