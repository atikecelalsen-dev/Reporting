using Reporting.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClosedXML.Excel;


namespace Reporting.Services.Excel
{
    public class SalesExcelExportService
    {

        private readonly string _connStr;

        private readonly string _firmaNo;
        private readonly string _donemNo;

        public SalesExcelExportService(string connectionString, string firmaNo, string donemNo)
        {
            _connStr = connectionString;
            _firmaNo = firmaNo;
            _donemNo = donemNo;
        } 

        public byte[] SatisRaporuExcel(ReportFilter filter)
        {
            if (string.IsNullOrWhiteSpace(filter.Periyot))
                filter.Periyot = "gunluk";

            SalesReportService service = new SalesReportService(_connStr, _firmaNo, _donemNo);

            SalesReportDashboardModel model =
                service.SatisKarZararDashboard(filter, filter.Periyot);


            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Satış Raporu");

            int row = 2;
            int col = 2;

            ws.Cell(row, col).Value = "Ciro Kar/Zarar Raporu";
            ws.Range(row, col, row, col + 4).Merge();

            ws.Cell(row, col).Style.Font.Bold = true;
            ws.Cell(row, col).Style.Font.FontSize = 16;
            ws.Cell(row, col).Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

            row += 2;

            string periyot = model.Periyot;
            ws.Cell(row, col).Value = "Periyot";
            ws.Range(row, col, row, col + 1).Merge();
            string baslik = periyot == "haftalik" ? "Haftalık" :
                periyot == "aylik" ? "Aylık" :
                periyot == "yillik" ? "Yıllık" :
                periyot == "gunluk" ? "Günlük" :
                "Özel tarihler için";
            ws.Cell(row, col + 2).Value = baslik;
            ws.Range(row, col + 2, row, col + 4).Merge();
            row++;

            ws.Cell(row, col).Value = "Tarih Aralığı";
            ws.Range(row, col, row, col + 1).Merge();
            DateTime baslangicTarih =
                model.RaporSatirlari.Any()
                    ? model.RaporSatirlari.Min(x => x.Tarih)
                    : DateTime.Today;

            DateTime bitisTarih =
                model.RaporSatirlari.Any()
                    ? model.RaporSatirlari.Max(x => x.Tarih)
                    : DateTime.Today;

            ws.Cell(row, col + 2).Value =
                $"{baslangicTarih:dd.MM.yyyy} - {bitisTarih:dd.MM.yyyy}";

            ws.Range(row, col + 2, row, col + 4).Merge();

            row += 2;

            ws.Cell(row, col).Value = "Dönem";
            ws.Cell(row, col + 1).Value = "Ciro";
            ws.Cell(row, col + 2).Value = "Maliyet";
            ws.Cell(row, col + 3).Value = "Kar";
            ws.Cell(row, col + 4).Value = "Kar Oranı";

            var headerRange = ws.Range(row, col, row, col + 4);

            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            row++;

            foreach (var item in model.RaporSatirlari.OrderBy(x => x.Tarih))
            {
                ws.Cell(row, col).Value = item.Baslik;
                ws.Cell(row, col + 1).Value = item.Ciro;
                ws.Cell(row, col + 2).Value = item.Maliyet;
                ws.Cell(row, col + 3).Value = item.Kar;
                ws.Cell(row, col + 4).Value = item.KarOrani;

                row++;
            }

            var dataRange = ws.Range(1, 1, row - 1, col + 4);

            dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            ws.Column(col + 1).Style.NumberFormat.Format = "#,##0.00 ₺";
            ws.Column(col + 2).Style.NumberFormat.Format = "#,##0.00 ₺";
            ws.Column(col + 3).Style.NumberFormat.Format = "#,##0.00 ₺";
            ws.Column(col + 4).Style.NumberFormat.Format = "0.00";



            ws.Column(1).Width = 4;
            ws.Column(col).Width = 20;
            ws.Column(col + 1).Width = 18;
            ws.Column(col + 2).Width = 18;
            ws.Column(col + 3).Width = 18;
            ws.Column(col + 4).Width = 14;

            ws.Cell(row, col).Value = "Toplam Satış";
            ws.Cell(row, col + 1).Value = model.Ciro.Deger;
            ws.Cell(row, col + 2).Value = model.Maliyet.Deger;
            ws.Cell(row, col + 3).Value = model.Kar.Deger;
            ws.Cell(row, col + 4).Value = model.KarOrani.Deger;

            var toplamRange = ws.Range(row, col, row, col + 4);
            toplamRange.Style.NumberFormat.Format = "#,##0.00 ₺";
            toplamRange.Style.Font.Bold = true;
            toplamRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            toplamRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            toplamRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range(row, col + 4, row, col + 4).Style.NumberFormat.Format = "0.00";


            ws.Rows().AdjustToContents();

            using var stream = new MemoryStream();

            workbook.SaveAs(stream);

            return stream.ToArray();
        }

        public byte[] UrunKarlilikExcel(ReportFilter filter)
        {
            if (string.IsNullOrWhiteSpace(filter.Periyot))
                filter.Periyot = "gunluk";

            SalesReportService service = new SalesReportService(_connStr, _firmaNo, _donemNo);

            ProductProfitReportViewModel model = service.UrunKarlilikRaporu(filter);

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Ürün Karlılık");

            int row = 2;
            int col = 2;

            ws.Cell(row, col).Value = "Ürün Karlılık Raporu";
            ws.Range(row, col, row, col + 6).Merge();

            ws.Cell(row, col).Style.Font.Bold = true;
            ws.Cell(row, col).Style.Font.FontSize = 16;
            ws.Cell(row, col).Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

            row += 2;

            string periyot = model.Periyot;
            ws.Cell(row, col).Value = "Periyot";
            ws.Range(row, col, row, col + 1).Merge();

            string baslik = periyot == "haftalik" ? "Haftalık" :
                periyot == "aylik" ? "Aylık" :
                periyot == "yillik" ? "Yıllık" :
                periyot == "gunluk" ? "Günlük" :
                "Özel tarihler için";

            ws.Cell(row, col + 2).Value = baslik;
            ws.Range(row, col + 2, row, col + 6).Merge();
            row++;

            ws.Cell(row, col).Value = "Tarih Aralığı";
            ws.Range(row, col, row, col + 1).Merge();

            string tarihAraligi;

            if (filter.Periyot == "ozel"
                && filter.BaslangicTarihi.HasValue
                && filter.BitisTarihi.HasValue)
            {
                tarihAraligi =
                    $"{filter.BaslangicTarihi:dd.MM.yyyy} - {filter.BitisTarihi:dd.MM.yyyy}";
            }
            else
            {
                DateTime bugun = DateTime.Today;

                DateTime baslangic;
                DateTime bitis;

                switch (filter.Periyot)
                {
                    case "gunluk":
                        baslangic = bugun;
                        bitis = bugun;
                        break;

                    case "haftalik":
                        baslangic = bugun.AddDays(
                            bugun.DayOfWeek == DayOfWeek.Sunday
                                ? -6
                                : DayOfWeek.Monday - bugun.DayOfWeek);

                        bitis = bugun;
                        break;

                    case "aylik":
                        baslangic = new DateTime(bugun.Year, bugun.Month, 1);
                        bitis = bugun;
                        break;

                    case "yillik":
                        baslangic = new DateTime(bugun.Year, 1, 1);
                        bitis = bugun;
                        break;

                    default:
                        baslangic = bugun;
                        bitis = bugun;
                        break;
                }

                tarihAraligi =
                    $"{baslangic:dd.MM.yyyy} - {bitis:dd.MM.yyyy}";
            }


            ws.Cell(row, col + 2).Value = tarihAraligi;
            ws.Range(row, col + 2, row, col + 6).Merge();

            row += 2;

            row += 2;

            ws.Cell(row, col).Value = "Ürün Kodu";
            ws.Cell(row, col + 1).Value = "Ürün Adı";
            ws.Cell(row, col + 2).Value = "Miktar";
            ws.Cell(row, col + 3).Value = "Ciro";
            ws.Cell(row, col + 4).Value = "Maliyet";
            ws.Cell(row, col + 5).Value = "Kar";
            ws.Cell(row, col + 6).Value = "Kar Oranı";

            var headerRange = ws.Range(row, col, row, col + 6);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            row++;

            foreach (var item in model.Urunler.OrderByDescending(x => x.Kar))
            {
                ws.Cell(row, col).Value = item.StokKodu;
                ws.Cell(row, col + 1).Value = item.StokAdi;
                ws.Cell(row, col + 2).Value = item.Miktar;
                ws.Cell(row, col + 3).Value = item.Ciro;
                ws.Cell(row, col + 4).Value = item.Maliyet;
                ws.Cell(row, col + 5).Value = item.Kar;
                ws.Cell(row, col + 6).Value = item.KarOrani;

                row++;
            }

            ws.Cell(row, col).Value = "Toplam";
            ws.Range(row, col, row, col + 1).Merge();
            ws.Cell(row, col + 2).Value = model.Urunler.Sum(x => x.Miktar);
            ws.Cell(row, col + 3).Value = model.Urunler.Sum(x => x.Ciro);
            ws.Cell(row, col + 4).Value = model.Urunler.Sum(x => x.Maliyet);
            ws.Cell(row, col + 5).Value = model.Urunler.Sum(x => x.Kar);
            ws.Cell(row, col + 6).Value =
                model.Urunler.Sum(x => x.Ciro) == 0
                    ? 0
                    : (model.Urunler.Sum(x => x.Kar) / model.Urunler.Sum(x => x.Ciro)) * 100;

            var toplamRange = ws.Range(row, col, row, col + 6);
            toplamRange.Style.Font.Bold = true;
            toplamRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            toplamRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            toplamRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            var usedRange = ws.Range(1, 1, row, col + 6);
            usedRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            usedRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            ws.Column(1).Width = 4;
            ws.Column(col).Width = 18;
            ws.Column(col + 1).Width = 35;
            ws.Column(col + 2).Width = 14;
            ws.Column(col + 3).Width = 18;
            ws.Column(col + 4).Width = 18;
            ws.Column(col + 5).Width = 18;
            ws.Column(col + 6).Width = 14;

            ws.Column(col + 2).Style.NumberFormat.Format = "#,##0.00";
            ws.Column(col + 3).Style.NumberFormat.Format = "#,##0.00 ₺";
            ws.Column(col + 4).Style.NumberFormat.Format = "#,##0.00 ₺";
            ws.Column(col + 5).Style.NumberFormat.Format = "#,##0.00 ₺";
            ws.Column(col + 6).Style.NumberFormat.Format = "0.00";

            ws.Rows().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            return  stream.ToArray();
        }

        public byte[] MusteriKarlilikExcel(ReportFilter filter)
        {
            if (string.IsNullOrWhiteSpace(filter.Periyot))
                filter.Periyot = "gunluk";

            SalesReportService service = new SalesReportService(_connStr, _firmaNo, _donemNo);

            CustomerProfitReportViewModel model =
                service.MusteriKarlilikRaporu(filter);

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Müşteri Karlılık");

            int row = 2;
            int col = 2;

            ws.Cell(row, col).Value = "Müşteri Karlılık Raporu";
            ws.Range(row, col, row, col + 6).Merge();

            ws.Cell(row, col).Style.Font.Bold = true;
            ws.Cell(row, col).Style.Font.FontSize = 16;
            ws.Cell(row, col).Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

            row += 2;

            string periyot = model.Periyot;

            string baslik =
                periyot == "haftalik" ? "Haftalık" :
                periyot == "aylik" ? "Aylık" :
                periyot == "yillik" ? "Yıllık" :
                periyot == "gunluk" ? "Günlük" :
                "Özel tarihler için";

            ws.Cell(row, col).Value = "Periyot";
            ws.Range(row, col, row, col + 1).Merge();

            ws.Cell(row, col + 2).Value = baslik;
            ws.Range(row, col + 2, row, col + 6).Merge();
            row++;

            ws.Cell(row, col).Value = "Tarih Aralığı";
            ws.Range(row, col, row, col + 1).Merge();

            string tarihAraligi;

            if (filter.Periyot == "ozel"
                && filter.BaslangicTarihi.HasValue
                && filter.BitisTarihi.HasValue)
            {
                tarihAraligi =
                    $"{filter.BaslangicTarihi:dd.MM.yyyy} - {filter.BitisTarihi:dd.MM.yyyy}";
            }
            else
            {
                DateTime bugun = DateTime.Today;

                DateTime baslangic;
                DateTime bitis;

                switch (filter.Periyot)
                {
                    case "gunluk":
                        baslangic = bugun;
                        bitis = bugun;
                        break;

                    case "haftalik":
                        baslangic = bugun.AddDays(
                            bugun.DayOfWeek == DayOfWeek.Sunday
                                ? -6
                                : DayOfWeek.Monday - bugun.DayOfWeek);

                        bitis = bugun;
                        break;

                    case "aylik":
                        baslangic = new DateTime(bugun.Year, bugun.Month, 1);
                        bitis = bugun;
                        break;

                    case "yillik":
                        baslangic = new DateTime(bugun.Year, 1, 1);
                        bitis = bugun;
                        break;

                    default:
                        baslangic = bugun;
                        bitis = bugun;
                        break;
                }

                tarihAraligi =
                    $"{baslangic:dd.MM.yyyy} - {bitis:dd.MM.yyyy}";
            }


            ws.Cell(row, col + 2).Value = tarihAraligi;
            ws.Range(row, col + 2, row, col + 6).Merge();


            row += 2;

            ws.Cell(row, col).Value = "Cari Kodu";
            ws.Cell(row, col + 1).Value = "Cari Adı";
            ws.Cell(row, col + 2).Value = "Fatura";
            ws.Cell(row, col + 3).Value = "Ciro";
            ws.Cell(row, col + 4).Value = "Maliyet";
            ws.Cell(row, col + 5).Value = "Kar";
            ws.Cell(row, col + 6).Value = "Kar %";

            var headerRange = ws.Range(row, col, row, col + 6);

            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            row++;

            foreach (var item in model.Musteriler.OrderByDescending(x => x.Kar))
            {
                ws.Cell(row, col).Value = item.CariKodu;
                ws.Cell(row, col + 1).Value = item.CariAdi;
                ws.Cell(row, col + 2).Value = item.FaturaSayisi;
                ws.Cell(row, col + 3).Value = item.Ciro;
                ws.Cell(row, col + 4).Value = item.Maliyet;
                ws.Cell(row, col + 5).Value = item.Kar;
                ws.Cell(row, col + 6).Value = item.KarOrani;

                row++;
            }

            ws.Cell(row, col).Value = "Toplam";
            ws.Range(row, col, row, col + 1).Merge();

            ws.Cell(row, col + 2).Value =
                model.Musteriler.Sum(x => x.FaturaSayisi);

            ws.Cell(row, col + 3).Value =
                model.Musteriler.Sum(x => x.Ciro);

            ws.Cell(row, col + 4).Value =
                model.Musteriler.Sum(x => x.Maliyet);

            ws.Cell(row, col + 5).Value =
                model.Musteriler.Sum(x => x.Kar);

            ws.Cell(row, col + 6).Value =
                model.Musteriler.Sum(x => x.Ciro) == 0
                    ? 0
                    : (model.Musteriler.Sum(x => x.Kar)
                       / model.Musteriler.Sum(x => x.Ciro)) * 100;

            var toplamRange = ws.Range(row, col, row, col + 6);

            toplamRange.Style.Font.Bold = true;
            toplamRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            toplamRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            toplamRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            var usedRange = ws.Range(1, 1, row, col + 6);

            usedRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            usedRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            ws.Column(1).Width = 4;
            ws.Column(col).Width = 18;
            ws.Column(col + 1).Width = 40;
            ws.Column(col + 2).Width = 12;
            ws.Column(col + 3).Width = 18;
            ws.Column(col + 4).Width = 18;
            ws.Column(col + 5).Width = 18;
            ws.Column(col + 6).Width = 14;

            ws.Column(col + 3).Style.NumberFormat.Format = "#,##0.00 ₺";
            ws.Column(col + 4).Style.NumberFormat.Format = "#,##0.00 ₺";
            ws.Column(col + 5).Style.NumberFormat.Format = "#,##0.00 ₺";
            ws.Column(col + 6).Style.NumberFormat.Format = "0.00";

            ws.Rows().AdjustToContents();

            using var stream = new MemoryStream();

            workbook.SaveAs(stream);

            return stream.ToArray();
        }






    }
}
