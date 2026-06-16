using ClosedXML.Excel;
using Reporting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reporting.Services.Excel
{
    public class StockAndInventoryExcelExportService
    {
        private readonly string _connStr;

        private readonly string _firmaNo;
        private readonly string _donemNo;

        public StockAndInventoryExcelExportService(string connectionString, string firmaNo, string donemNo)
        {
            _connStr = connectionString;
            _firmaNo = firmaNo;
            _donemNo = donemNo;
        }


        public byte[] CokVeKritikStokExcel()
        {
            StockAndInventoryReportService service = new StockAndInventoryReportService(_connStr, _firmaNo, _donemNo);
            InventoryReportViewModel model = service.EnvanterRaporu();

            var enCokStok = model.Stoklar
                .Where(x => x.MevcutStok > 0)
                .OrderByDescending(x => x.MevcutStok)
                .Take(10)
                .ToList();

            var kritikStoklar = model.Stoklar
                .Where(x => x.MevcutStok <= 10)
                .OrderBy(x => x.MevcutStok)
                .Take(10)
                .ToList();

            using var workbook = new XLWorkbook();

            void SayfaOlustur(string sayfaAdi, List<InventoryReportRow> liste)
            {
                var ws = workbook.Worksheets.Add(sayfaAdi);

                int row = 2;
                int col = 2;

                ws.Cell(row, col).Value = sayfaAdi;
                ws.Range(row, col, row, col + 4).Merge();
                ws.Cell(row, col).Style.Font.Bold = true;
                ws.Cell(row, col).Style.Font.FontSize = 16;
                ws.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                row += 2;

                ws.Cell(row, col).Value = "Stok Kodu";
                ws.Cell(row, col + 1).Value = "Stok Adı";
                ws.Cell(row, col + 2).Value = "Mevcut Stok";
                ws.Cell(row, col + 3).Value = "Stok Değeri";
                ws.Cell(row, col + 4).Value = "Stok Durumu";

                var header = ws.Range(row, col, row, col + 4);
                header.Style.Font.Bold = true;
                header.Style.Fill.BackgroundColor = XLColor.LightGray;
                header.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                header.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                row++;

                foreach (var item in liste)
                {
                    ws.Cell(row, col).Value = item.StokKodu;
                    ws.Cell(row, col + 1).Value = item.StokAdi;
                    ws.Cell(row, col + 2).Value = item.MevcutStok;
                    ws.Cell(row, col + 3).Value = item.StokDegeri;
                    ws.Cell(row, col + 4).Value = item.StokDurumu;

                    row++;
                }

                var usedRange = ws.RangeUsed();
                usedRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                usedRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                ws.Column(1).Width = 4;
                ws.Column(col).Width = 18;
                ws.Column(col + 1).Width = 40;
                ws.Column(col + 2).Width = 16;
                ws.Column(col + 3).Width = 18;
                ws.Column(col + 4).Width = 18;

                ws.Column(col + 3).Style.NumberFormat.Format = "#,##0.00 ₺";

                ws.Rows().AdjustToContents();
            }

            SayfaOlustur("En Çok Stokta Olanlar", enCokStok);
            SayfaOlustur("Kritik Sıfır Stoklar", kritikStoklar);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            return  stream.ToArray() ;
        }

        public byte[] StokHareketRaporuExcel(DateTime? baslangicTarihi, DateTime? bitisTarihi)
        {
            StockAndInventoryReportService service = new StockAndInventoryReportService(_connStr, _firmaNo, _donemNo);
            StockMovementReportViewModel model = service.GetStockMovementReport(baslangicTarihi, bitisTarihi);

            using var workbook = new XLWorkbook();

            void SayfaOlustur(string sayfaAdi, List<StockMovementReportRow> liste)
            {
                var ws = workbook.Worksheets.Add(sayfaAdi);

                int row = 2;
                int col = 2;

                ws.Cell(row, col).Value = sayfaAdi;
                ws.Range(row, col, row, col + 5).Merge();
                ws.Cell(row, col).Style.Font.Bold = true;
                ws.Cell(row, col).Style.Font.FontSize = 16;
                ws.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                row += 2;

                ws.Cell(row, col).Value = "Tarih Aralığı";
                ws.Range(row, col, row, col + 1).Merge();

                ws.Cell(row, col + 2).Value = $"{model.BaslangicTarihi:dd.MM.yyyy} - {model.BitisTarihi:dd.MM.yyyy}";
                ws.Range(row, col + 2, row, col + 5).Merge();

                row += 2;

                ws.Cell(row, col).Value = "Stok Kodu";
                ws.Cell(row, col + 1).Value = "Stok Adı";
                ws.Cell(row, col + 2).Value = "Hareket Sayısı";
                ws.Cell(row, col + 3).Value = "Giriş";
                ws.Cell(row, col + 4).Value = "Çıkış";
                ws.Cell(row, col + 5).Value = "Net Değişim";

                var header = ws.Range(row, col, row, col + 5);
                header.Style.Font.Bold = true;
                header.Style.Fill.BackgroundColor = XLColor.LightGray;
                header.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                header.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                row++;

                foreach (var item in liste)
                {
                    ws.Cell(row, col).Value = item.StokKodu;
                    ws.Cell(row, col + 1).Value = item.StokAdi;
                    ws.Cell(row, col + 2).Value = item.HareketSayisi;
                    ws.Cell(row, col + 3).Value = item.GirisMiktari;
                    ws.Cell(row, col + 4).Value = item.CikisMiktari;
                    ws.Cell(row, col + 5).Value = item.NetDegisim;

                    row++;
                }

                var usedRange = ws.RangeUsed();
                usedRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                usedRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                ws.Column(1).Width = 4;
                ws.Column(col).Width = 18;
                ws.Column(col + 1).Width = 40;
                ws.Column(col + 2).Width = 16;
                ws.Column(col + 3).Width = 16;
                ws.Column(col + 4).Width = 16;
                ws.Column(col + 5).Width = 16;

                ws.Rows().AdjustToContents();
            }

            SayfaOlustur("En Çok Giriş", model.EnCokGirisYapilanlar);
            SayfaOlustur("En Çok Çıkış", model.EnCokCikisYapilanlar);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            return  stream.ToArray() ;
        }

        public byte[] SatilmayanUrunlerExcel()
        {
            StockAndInventoryReportService service = new StockAndInventoryReportService(_connStr, _firmaNo, _donemNo);
            UnsoldProductsReportViewModel model = service.GetUnsoldProductsReport();

            using var workbook = new XLWorkbook();

            void SayfaOlustur(string sayfaAdi, List<UnsoldProductRow> liste, bool sonSatisVarMi)
            {
                var ws = workbook.Worksheets.Add(sayfaAdi);

                int row = 2;
                int col = 2;

                ws.Cell(row, col).Value = sayfaAdi;
                ws.Range(row, col, row, col + 4).Merge();
                ws.Cell(row, col).Style.Font.Bold = true;
                ws.Cell(row, col).Style.Font.FontSize = 16;
                ws.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                row += 2;

                ws.Cell(row, col).Value = "Stok Kodu";
                ws.Cell(row, col + 1).Value = "Stok Adı";
                ws.Cell(row, col + 2).Value = "Mevcut Stok";
                ws.Cell(row, col + 3).Value = "Son Satış";
                ws.Cell(row, col + 4).Value = "Geçen Gün";

                var header = ws.Range(row, col, row, col + 4);
                header.Style.Font.Bold = true;
                header.Style.Fill.BackgroundColor = XLColor.LightGray;
                header.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                header.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                row++;

                foreach (var item in liste)
                {
                    ws.Cell(row, col).Value = item.StokKodu;
                    ws.Cell(row, col + 1).Value = item.StokAdi;
                    ws.Cell(row, col + 2).Value = item.MevcutStok;

                    if (sonSatisVarMi)
                    {
                        ws.Cell(row, col + 3).Value = item.SonSatisTarihi;
                        ws.Cell(row, col + 3).Style.DateFormat.Format = "dd.MM.yyyy";
                        ws.Cell(row, col + 4).Value = item.GecenGun;
                    }
                    else
                    {
                        ws.Cell(row, col + 3).Value = "Yok";
                        ws.Cell(row, col + 4).Value = "-";
                    }

                    row++;
                }

                var usedRange = ws.RangeUsed();
                usedRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                usedRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                ws.Column(1).Width = 4;
                ws.Column(col).Width = 18;
                ws.Column(col + 1).Width = 40;
                ws.Column(col + 2).Width = 16;
                ws.Column(col + 3).Width = 18;
                ws.Column(col + 4).Width = 14;

                ws.Rows().AdjustToContents();
            }

            SayfaOlustur("Uzun Süredir Satılmayan Ürünler", model.UzunSuredirSatilmayanlar, true);
            SayfaOlustur("Hiç Satılmamış Ürünler", model.HicSatilmamisUrunler, false);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            return  stream.ToArray() ;
        }



        public byte[] EnvanterExcel()
        {
            StockAndInventoryReportService service = new StockAndInventoryReportService(_connStr, _firmaNo, _donemNo);
            InventoryReportViewModel model = service.EnvanterRaporu();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Envanter");

            int row = 2;
            int col = 2;

            ws.Cell(row, col).Value = "Stok Ve Envanter Raporu";
            ws.Range(row, col, row, col + 7).Merge();
            ws.Cell(row, col).Style.Font.Bold = true;
            ws.Cell(row, col).Style.Font.FontSize = 16;
            ws.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            row += 2;

            ws.Cell(row, col).Value = "Ürün Kodu";
            ws.Cell(row, col + 1).Value = "Ürün Adı";
            ws.Cell(row, col + 2).Value = "Giren";
            ws.Cell(row, col + 3).Value = "Çıkan";
            ws.Cell(row, col + 4).Value = "Mevcut Stok";
            ws.Cell(row, col + 5).Value = "Ort. Maliyet";
            ws.Cell(row, col + 6).Value = "Envanter Değeri";
            ws.Cell(row, col + 7).Value = "Durum";

            var headerRange = ws.Range(row, col, row, col + 7);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            row++;

            foreach (var item in ModelConcat(model))
            {
                ws.Cell(row, col).Value = item.StokKodu;
                ws.Cell(row, col + 1).Value = item.StokAdi;
                ws.Cell(row, col + 2).Value = item.GirenMiktar;
                ws.Cell(row, col + 3).Value = item.CikanMiktar;
                ws.Cell(row, col + 4).Value = item.MevcutStok;
                ws.Cell(row, col + 5).Value = item.OrtalamaMaliyet;
                ws.Cell(row, col + 6).Value = item.StokDegeri;
                ws.Cell(row, col + 7).Value = item.StokDurumu;

                row++;
            }

            var usedRange = ws.RangeUsed();
            usedRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            usedRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            ws.Column(1).Width = 4;
            ws.Column(col).Width = 18;
            ws.Column(col + 1).Width = 44;
            ws.Column(col + 2).Width = 14;
            ws.Column(col + 3).Width = 14;
            ws.Column(col + 4).Width = 16;
            ws.Column(col + 5).Width = 18;
            ws.Column(col + 6).Width = 18;
            ws.Column(col + 7).Width = 16;

            ws.Column(col + 5).Style.NumberFormat.Format = "#,##0.00 ₺";
            ws.Column(col + 6).Style.NumberFormat.Format = "#,##0.00 ₺";
            ws.Cell(4, col + 1).Style.NumberFormat.Format = "#,##0.00 ₺";

            row += 1;

            ws.Cell(row, col).Value = "Toplam Envanter Değeri";
            ws.Cell(row, col + 1).Value = model.ToplamStokDegeri;
            ws.Cell(row, col + 1).Style.NumberFormat.Format = "#,##0.00 ₺";


            ws.Cell(row, col + 2).Value = "Toplam Ürün";
            ws.Cell(row, col + 3).Value = model.ToplamUrunSayisi;
            ws.Cell(row, col + 3).Style.NumberFormat.Format = "#,##0.00";


            ws.Cell(row, col + 4).Value = "Kritik Stok";
            ws.Cell(row, col + 5).Value = model.KritikStokSayisi;
            ws.Cell(row, col + 5).Style.NumberFormat.Format = "#,##0.00";


            ws.Cell(row, col + 6).Value = "Stok Yok";
            ws.Cell(row, col + 7).Value = model.StokYokSayisi;
            ws.Cell(row, col + 7).Style.NumberFormat.Format = "#,##0.00";


            var summaryRange = ws.Range(row, col, row, col + 7);
            summaryRange.Style.Font.Bold = true;
            summaryRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            summaryRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            summaryRange.Style.Fill.BackgroundColor = XLColor.LightGray;


            ws.Rows().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            return  stream.ToArray() ;
        }

        private List<InventoryReportRow> ModelConcat(InventoryReportViewModel model)
        {
            var liste = new List<InventoryReportRow>();

            if (model.Stoklar != null)
                liste.AddRange(model.Stoklar);

            if (model.KritikStoklar != null)
                liste.AddRange(model.KritikStoklar);

            if (model.StokYoklar != null)
                liste.AddRange(model.StokYoklar);

            return liste;
        }

    }
}
