using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NoorSardPlatform.Data;
using NoorSardPlatform.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Microsoft.AspNetCore.SignalR;
using NoorSardPlatform.Hubs;
using Microsoft.AspNetCore.Authorization;

namespace NoorSardPlatform.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IHubContext<DashboardHub> _dashboardHub;
        private IActionResult CreateParticipantsExcel(
    List<Participant> participants,
    string worksheetName,
    string fileName
)



{
    using var workbook = new XLWorkbook();

    var worksheet = workbook.Worksheets.Add(worksheetName);

    worksheet.RightToLeft = true;

    worksheet.Cell(1, 1).Value = "م";
    worksheet.Cell(1, 2).Value = "اسم الحافظة";
    worksheet.Cell(1, 3).Value = "الهدف";
    worksheet.Cell(1, 4).Value = "المنجز";
    worksheet.Cell(1, 5).Value = "نسبة الإنجاز";
    worksheet.Cell(1, 6).Value = "الوسام البرونزي";
    worksheet.Cell(1, 7).Value = "الوسام الفضي";
    worksheet.Cell(1, 8).Value = "الوسام الذهبي";
    worksheet.Cell(1, 9).Value = "آخر تحديث";

    var headerRange = worksheet.Range(1, 1, 1, 9);

    headerRange.Style.Font.Bold = true;
    headerRange.Style.Font.FontColor =
        XLColor.White;

    headerRange.Style.Fill.BackgroundColor =
        XLColor.FromHtml("#005B70");

    headerRange.Style.Alignment.Horizontal =
        XLAlignmentHorizontalValues.Center;

    headerRange.Style.Alignment.Vertical =
        XLAlignmentVerticalValues.Center;

    for (int index = 0; index < participants.Count; index++)
    {
        var participant = participants[index];
        int rowNumber = index + 2;

        worksheet.Cell(rowNumber, 1).Value = index + 1;
        worksheet.Cell(rowNumber, 2).Value = participant.FullName;
        worksheet.Cell(rowNumber, 3).Value = participant.TargetParts;
        worksheet.Cell(rowNumber, 4).Value = participant.CompletedParts;

        worksheet.Cell(rowNumber, 5).Value =
            participant.CompletionPercentage / 100;

        worksheet.Cell(rowNumber, 5)
            .Style.NumberFormat.Format = "0%";

        worksheet.Cell(rowNumber, 6).Value =
            participant.BronzeMedal ? "نعم" : "لا";

        worksheet.Cell(rowNumber, 7).Value =
            participant.SilverMedal ? "نعم" : "لا";

        worksheet.Cell(rowNumber, 8).Value =
            participant.GoldMedal ? "نعم" : "لا";

        worksheet.Cell(rowNumber, 9).Value =
            participant.LastUpdatedAt
                ?.ToString("yyyy/MM/dd hh:mm tt")
            ?? "-";
    }

    if (participants.Count == 0)
    {
        worksheet.Cell(2, 1).Value =
            "لا توجد بيانات في هذه القائمة.";

        worksheet.Range(2, 1, 2, 9).Merge();

        worksheet.Cell(2, 1).Style.Alignment.Horizontal =
            XLAlignmentHorizontalValues.Center;
    }

    var usedRange = worksheet.RangeUsed();

    if (usedRange != null)
    {
        usedRange.Style.Border.TopBorder =
            XLBorderStyleValues.Thin;

        usedRange.Style.Border.BottomBorder =
            XLBorderStyleValues.Thin;

        usedRange.Style.Border.LeftBorder =
            XLBorderStyleValues.Thin;

        usedRange.Style.Border.RightBorder =
            XLBorderStyleValues.Thin;

        usedRange.Style.Border.TopBorderColor =
            XLColor.FromHtml("#D8C28E");

        usedRange.Style.Border.BottomBorderColor =
            XLColor.FromHtml("#D8C28E");

        usedRange.Style.Border.LeftBorderColor =
            XLColor.FromHtml("#D8C28E");

        usedRange.Style.Border.RightBorderColor =
            XLColor.FromHtml("#D8C28E");

        usedRange.Style.Alignment.Vertical =
            XLAlignmentVerticalValues.Center;
    }

    worksheet.Column(1).Width = 8;
    worksheet.Column(2).Width = 38;
    worksheet.Columns(3, 8).Width = 17;
    worksheet.Column(9).Width = 24;

    worksheet.Row(1).Height = 26;
    worksheet.SheetView.FreezeRows(1);

    using var stream = new MemoryStream();

    workbook.SaveAs(stream);

    return File(
        stream.ToArray(),
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        fileName
    );
}
        private readonly ApplicationDbContext _context;

        public AdminController(
    ApplicationDbContext context,
    IHubContext<DashboardHub> dashboardHub
)
{
    _context = context;
    _dashboardHub = dashboardHub;
}

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> ResetResults(
    string confirmationText
)
{
    const string requiredConfirmation = "إعادة ضبط النتائج";

    if (!string.Equals(
        confirmationText?.Trim(),
        requiredConfirmation,
        StringComparison.Ordinal
    ))
    {
        TempData["ErrorMessage"] =
            $"لم تتم إعادة الضبط. اكتبي العبارة التالية كما هي: {requiredConfirmation}";

        return RedirectToAction(nameof(Index));
    }

    var participants = await _context.Participants.ToListAsync();

    foreach (var participant in participants)
    {
        participant.TargetParts = 0;
        participant.CompletedParts = 0;

        participant.BronzeMedal = false;
        participant.SilverMedal = false;
        participant.GoldMedal = false;

        participant.LastUpdatedAt = null;
    }

    await _context.SaveChangesAsync();

    await _dashboardHub.Clients.All.SendAsync(
        "DashboardUpdated"
    );

    TempData["SuccessMessage"] =
        $"تمت إعادة ضبط نتائج {participants.Count} حافظة مع الاحتفاظ بجميع الأسماء.";

    return RedirectToAction(nameof(Index));
}

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportParticipants(IFormFile excelFile)
        {
            if (excelFile == null || excelFile.Length == 0)
            {
                TempData["ErrorMessage"] = "يرجى اختيار ملف Excel.";
                return RedirectToAction(nameof(Index));
            }

            string extension = Path.GetExtension(excelFile.FileName)
                .ToLowerInvariant();

            if (extension != ".xlsx")
            {
                TempData["ErrorMessage"] =
                    "الملف يجب أن يكون بصيغة xlsx.";
                return RedirectToAction(nameof(Index));
            }

            var importedNames = new List<string>();

            try
            {
                using var stream = excelFile.OpenReadStream();
                using var workbook = new XLWorkbook(stream);

                var worksheet = workbook.Worksheets.First();
                var usedRows = worksheet.RowsUsed();

                foreach (var row in usedRows)
                {
                    string fullName = row.Cell(1)
                        .GetString()
                        .Trim();

                    if (string.IsNullOrWhiteSpace(fullName))
                    {
                        continue;
                    }

                    importedNames.Add(fullName);
                }

                importedNames = importedNames
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (importedNames.Count == 0)
                {
                    TempData["ErrorMessage"] =
                        "لم يتم العثور على أسماء داخل الملف.";

                    return RedirectToAction(nameof(Index));
                }

                var existingNames = await _context.Participants
                    .Select(participant => participant.FullName)
                    .ToListAsync();

                var existingNamesSet = existingNames
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var newParticipants = importedNames
                    .Where(name => !existingNamesSet.Contains(name))
                    .Select(name => new Participant
                    {
                        FullName = name,
                        TargetParts = 0,
                        CompletedParts = 0,
                        BronzeMedal = false,
                        SilverMedal = false,
                        GoldMedal = false
                    })
                    .ToList();

                if (newParticipants.Count > 0)
                {
                    await _context.Participants.AddRangeAsync(
                        newParticipants
                    );

                    await _context.SaveChangesAsync();
                }

                int skippedCount =
                    importedNames.Count - newParticipants.Count;

                TempData["SuccessMessage"] =
                    $"تم استيراد {newParticipants.Count} اسمًا بنجاح.";

                if (skippedCount > 0)
                {
                    TempData["InfoMessage"] =
                        $"تم تجاهل {skippedCount} اسمًا موجودًا مسبقًا.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] =
                    $"حدث خطأ: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> ExportAllResults()
        {
            var participants = await _context.Participants
                .AsNoTracking()
                .OrderBy(participant => participant.FullName)
                .ToListAsync();

            return CreateParticipantsExcel(
                participants,
                "نتائج يوم السرد",
                "نتائج-يوم-السرد.xlsx"
            );
        }

        [HttpGet]
        public async Task<IActionResult> ExportBronzeMedal()
        {
            var participants = await _context.Participants
                .AsNoTracking()
                .Where(participant => participant.BronzeMedal)
                .OrderBy(participant => participant.FullName)
                .ToListAsync();

            return CreateParticipantsExcel(
                participants,
                "الوسام البرونزي",
                "الحاصلات-على-الوسام-البرونزي.xlsx"
            );
        }

        [HttpGet]
        public async Task<IActionResult> ExportSilverMedal()
        {
            var participants = await _context.Participants
                .AsNoTracking()
                .Where(participant => participant.SilverMedal)
                .OrderBy(participant => participant.FullName)
                .ToListAsync();

            return CreateParticipantsExcel(
                participants,
                "الوسام الفضي",
                "الحاصلات-على-الوسام-الفضي.xlsx"
            );
        }

        [HttpGet]
        public async Task<IActionResult> ExportGoldMedal()
        {
            var participants = await _context.Participants
                .AsNoTracking()
                .Where(participant => participant.GoldMedal)
                .OrderBy(participant => participant.FullName)
                .ToListAsync();

            return CreateParticipantsExcel(
                participants,
                "الوسام الذهبي",
                "الحاصلات-على-الوسام-الذهبي.xlsx"
            );
        }

        private static void CreatePdfStatisticBox(
    IContainer container,
    string title,
    string value
)
{
    container
        .Border(1)
        .BorderColor("#D8C28E")
        .Background("#F4F8F9")
        .PaddingVertical(8)
        .PaddingHorizontal(6)
        .AlignCenter()
        .Column(column =>
        {
            column.Spacing(3);

            column.Item()
                .AlignCenter()
                .Text(title)
                .FontSize(8)
                .FontColor("#6D8389");

            column.Item()
                .AlignCenter()
                .Text(value)
                .FontSize(15)
                .Bold()
                .FontColor("#005B70");
        });
}

private static IContainer PdfHeaderCellStyle(
    IContainer container
)
{
    return container
        .Background("#005B70")
        .Border(0.5f)
        .BorderColor("#D8C28E")
        .PaddingVertical(7)
        .PaddingHorizontal(4)
        .AlignMiddle()
        .AlignCenter();
}

private static IContainer PdfTableCellStyle(
    IContainer container
)
{
    return container
        .Border(0.5f)
        .BorderColor("#D8C28E")
        .PaddingVertical(5)
        .PaddingHorizontal(4)
        .AlignMiddle();
}

        [HttpGet]
public async Task<IActionResult> ExportAllResultsPdf()
{
    var participants = await _context.Participants
        .AsNoTracking()
        .OrderBy(participant => participant.FullName)
        .ToListAsync();

    int totalTargetParts = participants.Sum(
        participant => participant.TargetParts
    );

    int totalCompletedParts = participants.Sum(
        participant => participant.CompletedParts
    );

    double overallPercentage = totalTargetParts > 0
        ? (double)totalCompletedParts /
          totalTargetParts * 100
        : 0;

    overallPercentage = Math.Min(
        overallPercentage,
        100
    );

    byte[] pdfFile = Document.Create(document =>
    {
        document.Page(page =>
        {
            page.Size(PageSizes.A4.Landscape());

            page.Margin(25);

            page.ContentFromRightToLeft();

            page.DefaultTextStyle(style =>
                style
                    .FontFamily("Arial")
                    .FontSize(9)
                    .FontColor("#17343C")
            );

            page.Header().Column(header =>
            {
                header.Spacing(7);

                header.Item()
                    .AlignCenter()
                    .Text("منصة السرد – برنامج النور")
                    .FontSize(20)
                    .Bold()
                    .FontColor("#005B70");

                header.Item()
                    .AlignCenter()
                    .Text("نتائج يوم السرد القرآني ١٤٤٧هـ")
                    .FontSize(14)
                    .Bold()
                    .FontColor("#C49A50");

                header.Item()
                    .PaddingTop(5)
                    .Row(row =>
                    {
                        row.Spacing(8);

                        row.RelativeItem()
                            .Element(container =>
                                CreatePdfStatisticBox(
                                    container,
                                    "عدد المشاركات",
                                    participants.Count.ToString()
                                )
                            );

                        row.RelativeItem()
                            .Element(container =>
                                CreatePdfStatisticBox(
                                    container,
                                    "مجموع المنجز",
                                    totalCompletedParts.ToString()
                                )
                            );

                        row.RelativeItem()
                            .Element(container =>
                                CreatePdfStatisticBox(
                                    container,
                                    "الهدف الكلي",
                                    totalTargetParts.ToString()
                                )
                            );

                        row.RelativeItem()
                            .Element(container =>
                                CreatePdfStatisticBox(
                                    container,
                                    "نسبة الإنجاز",
                                    $"{overallPercentage:0}%"
                                )
                            );

                        row.RelativeItem()
                            .Element(container =>
                                CreatePdfStatisticBox(
                                    container,
                                    "متمات محفوظ السنة",
                                    participants.Count(
                                        participant =>
                                            participant.BronzeMedal
                                    ).ToString()
                                )
                            );
                    });
            });

            page.Content()
                .PaddingVertical(15)
                .Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(35);
                        columns.RelativeColumn(3.4f);
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn(1.2f);
                        columns.RelativeColumn(1.2f);
                        columns.RelativeColumn(1.2f);
                    });

                    table.Header(header =>
{
    header.Cell()
        .Element(PdfHeaderCellStyle)
        .Text("م")
        .Bold()
        .FontColor(Colors.White);

    header.Cell()
        .Element(PdfHeaderCellStyle)
        .Text("اسم الحافظة")
        .Bold()
        .FontColor(Colors.White);

    header.Cell()
        .Element(PdfHeaderCellStyle)
        .Text("الهدف")
        .Bold()
        .FontColor(Colors.White);

    header.Cell()
        .Element(PdfHeaderCellStyle)
        .Text("المنجز")
        .Bold()
        .FontColor(Colors.White);

    header.Cell()
        .Element(PdfHeaderCellStyle)
        .Text("نسبة الإنجاز")
        .Bold()
        .FontColor(Colors.White);

    header.Cell()
        .Element(PdfHeaderCellStyle)
        .Text("برونزي")
        .Bold()
        .FontColor(Colors.White);

    header.Cell()
        .Element(PdfHeaderCellStyle)
        .Text("فضي")
        .Bold()
        .FontColor(Colors.White);

    header.Cell()
        .Element(PdfHeaderCellStyle)
        .Text("ذهبي")
        .Bold()
        .FontColor(Colors.White);
});

                    if (participants.Count == 0)
                    {
                        table.Cell()
                            .ColumnSpan(8)
                            .Element(PdfTableCellStyle)
                            .AlignCenter()
                            .Text("لا توجد نتائج مسجلة.");
                    }
                    else
                    {
                        for (
                            int index = 0;
                            index < participants.Count;
                            index++
                        )
                        {
                            var participant =
                                participants[index];

                            table.Cell()
    .Element(PdfTableCellStyle)
    .AlignCenter()
    .Text((index + 1).ToString());

table.Cell()
    .Element(PdfTableCellStyle)
    .AlignRight()
    .Text(participant.FullName);

table.Cell()
    .Element(PdfTableCellStyle)
    .AlignCenter()
    .Text(participant.TargetParts.ToString());

table.Cell()
    .Element(PdfTableCellStyle)
    .AlignCenter()
    .Text(participant.CompletedParts.ToString());

table.Cell()
    .Element(PdfTableCellStyle)
    .AlignCenter()
    .Text($"{participant.CompletionPercentage:0}%");

table.Cell()
    .Element(PdfTableCellStyle)
    .AlignCenter()
    .Text(participant.BronzeMedal ? "نعم" : "-");

table.Cell()
    .Element(PdfTableCellStyle)
    .AlignCenter()
    .Text(participant.SilverMedal ? "نعم" : "-");

table.Cell()
    .Element(PdfTableCellStyle)
    .AlignCenter()
    .Text(participant.GoldMedal ? "نعم" : "-");
                        }
                    }
                });

            page.Footer()
                .BorderTop(1)
                .BorderColor("#C49A50")
                .PaddingTop(7)
                .Row(row =>
                {
                    row.RelativeItem()
                        .AlignRight()
                        .Text(text =>
                        {
                            text.Span(
                                $"تاريخ التقرير: {DateTime.UtcNow:yyyy/MM/dd}"
                            );
                        });

                    row.RelativeItem()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span("صفحة ");
                            text.CurrentPageNumber();
                            text.Span(" من ");
                            text.TotalPages();
                        });

                    row.RelativeItem()
                        .AlignLeft()
                        .Text("برنامج النور");
                });
        });
    }).GeneratePdf();

    return File(
        pdfFile,
        "application/pdf",
        "نتائج-يوم-السرد.pdf"
    );
}
    }
}