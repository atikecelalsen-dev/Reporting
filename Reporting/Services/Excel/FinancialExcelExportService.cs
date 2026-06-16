using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reporting.Services.Excel
{
    public class FinancialExcelExportService
    {

        private readonly string _connStr;

        private readonly string _firmaNo;
        private readonly string _donemNo;

        public FinancialExcelExportService(string connectionString, string firmaNo, string donemNo)
        {
            _connStr = connectionString;
            _firmaNo = firmaNo;
            _donemNo = donemNo;
        }



        public byte[] CariHesapEkstresiExcel(int cariRef, DateTime? baslangicTarihi, DateTime? bitisTarihi)
        {
            DateTime baslangic = baslangicTarihi ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            DateTime bitis = bitisTarihi ?? DateTime.Today;

            var service = new CurrentAccountStatementService(_connStr, _firmaNo, _donemNo);
            var model = service.GetReport(cariRef, baslangic, bitis);

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Cari Ekstre");

            int row = 2;
            int col = 2; // B sütunu

            ws.Cell(row, col).Value = "Cari Hesap Ekstresi";
            ws.Range(row, col, row, col + 5).Merge();
            ws.Cell(row, col).Style.Font.Bold = true;
            ws.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, col).Style.Font.FontSize = 16;
            row += 2;

            // Cari satırı
            ws.Cell(row, col).Value = "Cari : ";
            ws.Range(row, col, row, col + 1).Merge();

            ws.Cell(row, col + 2).Value = $"{model.CariKodu} - {model.CariAdi}";
            ws.Range(row, col + 2, row, col + 5).Merge();

            var rangeCenter = ws.Range(row, col, row + 1, col + 1);
            rangeCenter.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            row++;

            // Tarih aralığı satırı
            ws.Cell(row, col).Value = "Tarih Aralığı : ";
            ws.Range(row, col, row, col + 1).Merge();

            ws.Cell(row, col + 2).Value = $"{model.BaslangicTarihi:dd.MM.yyyy} - {model.BitisTarihi:dd.MM.yyyy}";
            ws.Range(row, col + 2, row, col + 5).Merge();
            row += 2;

            // Özet başlıkları
            ws.Cell(row, col).Value = "Devir Bakiye";
            ws.Range(row, col, row, col + 1).Merge();

            ws.Cell(row, col + 2).Value = "Toplam Borç";

            ws.Cell(row, col + 3).Value = "Toplam Alacak";

            ws.Cell(row, col + 4).Value = "Son Bakiye";
            ws.Range(row, col + 4, row, col + 5).Merge();

            row++;

            // Özet değerleri
            ws.Cell(row, col).Value = model.DevirBakiye;
            ws.Range(row, col, row, col + 1).Merge();

            ws.Cell(row, col + 2).Value = model.ToplamBorc;

            ws.Cell(row, col + 3).Value = model.ToplamAlacak;

            ws.Cell(row, col + 4).Value = model.SonBakiye;
            ws.Range(row, col + 4, row, col + 5).Merge();

            var summaryRange = ws.Range(row - 1, col, row - 1, col + 5);
            summaryRange.Style.Font.Bold = true;
            summaryRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            summaryRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            summaryRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            row += 2;

            ws.Cell(row, col).Value = "Tarih";
            ws.Cell(row, col + 1).Value = "Fiş No";
            ws.Cell(row, col + 2).Value = "Açıklama";
            ws.Cell(row, col + 3).Value = "Borç";
            ws.Cell(row, col + 4).Value = "Alacak";
            ws.Cell(row, col + 5).Value = "Bakiye";

            var headerRange = ws.Range(row, col, row, col + 5);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            row++;

            ws.Cell(row, col).Value = "Devir Bakiye";
            ws.Range(row, col, row, col + 4).Merge();
            ws.Cell(row, col + 5).Value = model.DevirBakiye;
            row++;

            foreach (var item in model.Hareketler)
            {
                ws.Cell(row, col).Value = item.Tarih;
                ws.Cell(row, col).Style.DateFormat.Format = "dd.MM.yyyy";

                ws.Cell(row, col + 1).Value = item.FisNo;
                ws.Cell(row, col + 2).Value = item.Aciklama;
                ws.Cell(row, col + 3).Value = item.Borc;
                ws.Cell(row, col + 4).Value = item.Alacak;
                ws.Cell(row, col + 5).Value = item.Bakiye;

                row++;
            }

            ws.Cell(row, col).Value = "Toplam";
            ws.Range(row, col, row, col + 2).Merge();
            ws.Cell(row, col + 3).Value = model.ToplamBorc;
            ws.Cell(row, col + 4).Value = model.ToplamAlacak;
            ws.Cell(row, col + 5).Value = model.SonBakiye;

            ws.Range(row, col, row, col + 5).Style.Font.Bold = true;

            var usedRange = ws.RangeUsed();
            usedRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            usedRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            // Para formatları
            ws.Range(7, col, 8, col + 5).Style.NumberFormat.Format = "#,##0.00 ₺";
            ws.Column(col + 3).Style.NumberFormat.Format = "#,##0.00 ₺";
            ws.Column(col + 4).Style.NumberFormat.Format = "#,##0.00 ₺";
            ws.Column(col + 5).Style.NumberFormat.Format = "#,##0.00 ₺";

            // Genişlikler
            ws.Column(1).Width = 4;
            ws.Column(col).Width = 16;
            ws.Column(col + 1).Width = 18;
            ws.Column(col + 2).Width = 35;
            ws.Column(col + 3).Width = 18;
            ws.Column(col + 4).Width = 18;
            ws.Column(col + 5).Width = 18;

            ws.Rows().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            return  stream.ToArray() ;
        }


        public byte[] GelirGiderOzetiExcel(DateTime? baslangicTarihi, DateTime? bitisTarihi)
        {
            DateTime baslangic = baslangicTarihi ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            DateTime bitis = bitisTarihi ?? DateTime.Today;

            var service = new IncomeExpenseReportService(_connStr, _firmaNo, _donemNo);
            var model = service.GetReport(baslangic, bitis);

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Gelir Gider");

            int row = 2;
            int col = 2; // B sütunu

            ws.Cell(row, col).Value = "Gelir-Gider Özeti";
            ws.Range(row, col, row, col + 4).Merge();
            ws.Cell(row, col).Style.Font.Bold = true;
            ws.Cell(row, col).Style.Font.FontSize = 16;
            ws.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            row += 2;

            ws.Cell(row, col).Value = "Tarih Aralığı :";
            ws.Range(row, col, row, col + 1).Merge();

            ws.Cell(row, col + 2).Value = $"{model.BaslangicTarihi:dd.MM.yyyy} - {model.BitisTarihi:dd.MM.yyyy}";
            ws.Range(row, col + 2, row, col + 4).Merge();

            row += 2;

            // Özet başlıkları
            ws.Cell(row, col).Value = "Toplam Gelir";
            ws.Range(row, col, row, col + 1).Merge();
            ws.Cell(row, col + 2).Value = "Toplam Gider";
            ws.Cell(row, col + 3).Value = "Net Sonuç";
            ws.Range(row, col + 3, row, col + 4).Merge();


            var summaryHeader = ws.Range(row, col, row, col + 4);
            summaryHeader.Style.Font.Bold = true;
            summaryHeader.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            summaryHeader.Style.Fill.BackgroundColor = XLColor.LightGray;
            summaryHeader.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            summaryHeader.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            row++;

            ws.Cell(row, col).Value = model.TotalIncome;
            ws.Range(row, col, row, col + 1).Merge();
            ws.Cell(row, col + 2).Value = model.TotalExpense;
            ws.Cell(row, col + 3).Value = model.NetResult;
            ws.Range(row, col + 3, row, col + 4).Merge();

            var summaryValue = ws.Range(row, col, row, col + 4);
            summaryValue.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            summaryValue.Style.Font.Bold = true;
            summaryValue.Style.NumberFormat.Format = "#,##0.00 ₺";
            summaryValue.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            summaryValue.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            row += 2;

            // Özet tablo
            ws.Cell(row, col).Value = "Tip";
            ws.Range(row, col, row, col + 1).Merge();
            ws.Cell(row, col + 2).Value = "Kalem";
            ws.Cell(row, col + 3).Value = "Tutar";
            ws.Range(row, col + 3, row, col + 4).Merge();


            var headerRange = ws.Range(row, col, row, col + 4);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            row++;

            ws.Cell(row, col).Value = "Gelir";
            ws.Range(row, col, row, col + 1).Merge();
            ws.Cell(row, col + 2).Value = "Satışlar";
            ws.Cell(row, col + 3).Value = model.SalesIncome;
            ws.Range(row, col + 3, row, col + 4).Merge();
            row++;

            ws.Cell(row, col).Value = "Gider";
            ws.Range(row, col, row, col + 1).Merge();
            ws.Cell(row, col + 2).Value = "Alışlar";
            ws.Cell(row, col + 3).Value = model.PurchaseExpense;
            ws.Range(row, col + 3, row, col + 4).Merge();
            row++;

            ws.Cell(row, col).Value = "Gider";
            ws.Range(row, col, row, col + 1).Merge();
            ws.Cell(row, col + 2).Value = "Hizmet Giderleri";
            ws.Cell(row, col + 3).Value = model.ServiceExpense;
            ws.Range(row, col + 3, row, col + 4).Merge();
            row++;

            ws.Cell(row, col).Value = "Net";
            ws.Range(row, col, row, col + 1).Merge();
            ws.Cell(row, col + 2).Value = "Net Sonuç";
            ws.Cell(row, col + 3).Value = model.NetResult;
            ws.Range(row, col + 3, row, col + 4).Merge();
            ws.Range(row, col, row, col + 2).Style.Font.Bold = true;

            ws.Range(row - 3, col + 3, row, col + 4).Style.NumberFormat.Format = "#,##0.00 ₺";


            row += 2;

            // Detay başlığı
            ws.Cell(row, col).Value = "Detaylar";
            ws.Range(row, col, row, col + 4).Merge();
            ws.Cell(row, col).Style.Font.Bold = true;
            ws.Cell(row, col).Style.Font.FontSize = 13;

            row++;

            ws.Cell(row, col).Value = "Tarih";
            ws.Cell(row, col + 1).Value = "Fiş No";
            ws.Cell(row, col + 2).Value = "Cari";
            ws.Cell(row, col + 3).Value = "Kalem";
            ws.Cell(row, col + 4).Value = "Tutar";

            var detailHeader = ws.Range(row, col, row, col + 4);
            detailHeader.Style.Font.Bold = true;
            detailHeader.Style.Fill.BackgroundColor = XLColor.LightGray;
            detailHeader.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            detailHeader.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            row++;

            foreach (var item in model.Detaylar)
            {
                ws.Cell(row, col).Value = item.Tarih;
                ws.Cell(row, col).Style.DateFormat.Format = "dd.MM.yyyy";

                ws.Cell(row, col + 1).Value = item.FisNo;
                ws.Cell(row, col + 2).Value = item.CariAdi;
                ws.Cell(row, col + 3).Value = item.Kalem;
                ws.Cell(row, col + 4).Value = item.Tutar;

                row++;
            }

            var detailToplam = ws.Cell(row, col + 4);
            detailToplam.Value = model.NetResult;
            detailToplam.Style.Font.Bold = true;
            detailToplam.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            detailToplam.Style.Border.InsideBorder = XLBorderStyleValues.Thin;




            var usedRange = ws.RangeUsed();
            usedRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            usedRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            ws.Column(1).Width = 4;
            ws.Column(col).Width = 16;
            ws.Column(col + 1).Width = 18;
            ws.Column(col + 2).Width = 40;
            ws.Column(col + 3).Width = 20;
            ws.Column(col + 4).Width = 18;

            ws.Column(col + 2).Style.NumberFormat.Format = "#,##0.00 ₺";
            ws.Column(col + 4).Style.NumberFormat.Format = "#,##0.00 ₺";

            ws.Rows().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            return  stream.ToArray() ;
        }


    }
}
