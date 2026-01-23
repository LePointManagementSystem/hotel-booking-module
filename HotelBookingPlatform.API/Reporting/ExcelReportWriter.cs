using ClosedXML.Excel;

namespace HotelBookingPlatform.API.Reporting;

public static class ExcelReportWriter
{
    public static byte[] Build(string sheetName, Action<IXLWorksheet> buildSheet)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add(sheetName);

        buildSheet(ws);

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    public static void WriteHeader(IXLWorksheet ws, int row, params string[] headers)
    {
        for (int c = 0; c < headers.Length; c++)
        {
            var cell = ws.Cell(row, c + 1);
            cell.Value = headers[c];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        }

        ws.Row(row).Height = 20;
    }

    public static void WriteRow(IXLWorksheet ws, int row, params object?[] values)
    {
        for (int c = 0; c < values.Length; c++)
        {
            var cell = ws.Cell(row, c + 1);
            var v = values[c];

            if (v is null)
            {
                cell.Value = "";
                continue;
            }

            // ✅ Dates: écriture propre + format Excel
            if (v is DateTime dt)
            {
                cell.Value = dt;
                cell.Style.DateFormat.Format = "yyyy-mm-dd hh:mm:ss";
                continue;
            }

            if (v is DateTimeOffset dto)
            {
                cell.Value = dto.UtcDateTime;
                cell.Style.DateFormat.Format = "yyyy-mm-dd hh:mm:ss";
                continue;
            }

            cell.Value = v.ToString();
        }
    }

    // ✅ Utilitaire si tu veux forcer une date sur une colonne précise
    public static void WriteDate(IXLWorksheet ws, int row, int col, DateTime? dt)
    {
        var cell = ws.Cell(row, col);
        if (!dt.HasValue)
        {
            cell.Value = "";
            return;
        }

        cell.Value = dt.Value;
        cell.Style.DateFormat.Format = "yyyy-mm-dd hh:mm:ss";
    }
}
