using CsvHelper;
using EngagementService.Application.Abstraction;
using EngagementService.Application.Dtos.LoyaltyAccount;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;
using System.Text;

namespace EngagementService.Application
{
    public class ReportService : IReportService
    {
        public byte[] ExportToCsv(LoyaltyReportDto data)
        {
            using var ms = new MemoryStream();
            // Sử dụng Encoding.UTF8 để tránh lỗi font tiếng Việt khi mở bằng Excel
            using var writer = new StreamWriter(ms, Encoding.UTF8);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            // 1. Tiêu đề chính
            csv.WriteField("BÁO CÁO CHƯƠNG TRÌNH KHÁCH HÀNG THÂN THIẾT");
            csv.NextRecord();
            csv.NextRecord(); // Dòng trống

            // 2. Phần Tóm tắt (Summary)
            csv.WriteField("CHỈ SỐ TỔNG QUAN");
            csv.NextRecord();
            // Ghi tiêu đề cột cho phần Summary
            csv.WriteField("Tổng thành viên");
            csv.WriteField("Điểm đã cấp");
            csv.WriteField("Điểm đã dùng");
            csv.NextRecord();
            // Ghi dữ liệu
            csv.WriteField(data.TotalMembers);
            csv.WriteField(data.TotalPointsEarned);
            csv.WriteField(data.TotalPointsRedeemed);
            csv.NextRecord();
            csv.NextRecord(); // Dòng trống

            // 3. Phần Phân bổ theo Hạng (Tier Distribution)
            csv.WriteField("PHÂN BỔ THEO HẠNG THÀNH VIÊN");
            csv.NextRecord();
            // Ghi tiêu đề cột thủ công để đảm bảo đẹp
            csv.WriteField("Tên Hạng");
            csv.WriteField("Số lượng thành viên");
            csv.NextRecord();

            // Ghi danh sách Tier (không dùng WriteRecords trực tiếp để tránh lặp lại Header mặc định của DTO)
            foreach (var tier in data.TierDistribution)
            {
                csv.WriteField(tier.TierName);
                csv.WriteField(tier.MemberCount);
                csv.NextRecord();
            }

            writer.Flush();
            return ms.ToArray();
        }

        public byte[] ExportToPdf(LoyaltyReportDto data)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    // 1. Định dạng trang A4 đứng chuẩn
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(11));

                    // Header
                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("HỆ THỐNG CÀ HONG BẠN").FontSize(16).SemiBold().FontColor(Colors.Red.Medium);
                            col.Item().Text("BÁO CÁO TỔNG QUAN CHƯƠNG TRÌNH THÂN THIẾT").FontSize(14).Bold();
                            col.Item().Text($"Thời gian xuất: {DateTime.Now:dd/MM/yyyy HH:mm}").Italic().FontSize(10).FontColor(Colors.Grey.Medium);
                        });
                    });

                    page.Content().PaddingVertical(10).Column(col =>
                    {
                        // Summary Cards
                        col.Item().PaddingBottom(15).Row(row =>
                        {
                            row.RelativeItem().Component(new SummaryCard("Tổng thành viên", data.TotalMembers.ToString("N0"), Colors.Blue.Medium));
                            row.ConstantItem(15);
                            row.RelativeItem().Component(new SummaryCard("Điểm đã cấp", data.TotalPointsEarned.ToString("N0"), Colors.Green.Medium));
                            row.ConstantItem(15);
                            row.RelativeItem().Component(new SummaryCard("Điểm đã dùng", data.TotalPointsRedeemed.ToString("N0"), Colors.Red.Medium));
                        });

                        // Tier Table
                        col.Item().PaddingBottom(5).Text("Phân bổ theo hạng thành viên").FontSize(12).SemiBold();
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(3);
                                c.RelativeColumn(2);
                            });

                            table.Header(h =>
                            {
                                h.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Tên Hạng").SemiBold();
                                h.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Số lượng TV").SemiBold().AlignCenter();
                            });

                            foreach (var item in data.TierDistribution)
                            {
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten4).Padding(5).Text(item.TierName);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten4).Padding(5).Text(item.MemberCount.ToString("N0")).AlignCenter();
                            }
                        });
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Trang ").FontSize(10);
                        x.CurrentPageNumber().FontSize(10);
                    });
                });
            }).GeneratePdf();
        }

        // Component phụ trợ để vẽ các ô chỉ số cho đẹp
        public class SummaryCard : IComponent
        {
            private string Title { get; }
            private string Value { get; }
            private string Color { get; }

            public SummaryCard(string title, string value, string color)
            {
                Title = title;
                Value = value;
                Color = color;
            }

            public void Compose(IContainer container)
            {
                container.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(col =>
                {
                    col.Item().Text(Title).FontSize(9).SemiBold();
                    col.Item().Text(Value).FontSize(14).Bold().FontColor(Color);
                });
            }
        }
    }
}
