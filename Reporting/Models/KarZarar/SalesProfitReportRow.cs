using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reporting.Models
{
    public class SalesProfitReportRow
    {
        public DateTime Tarih { get; set; }

        public string FisNo { get; set; } = "";

        public string CariKodu { get; set; } = "";

        public string CariAdi { get; set; } = "";
        public decimal Miktar { get; set; }
        public decimal Ciro { get; set; }

        public decimal Maliyet { get; set; }

        public decimal Kar { get; set; }

        public decimal KarOrani { get; set; }

        public int StockRef { get; set; }

        public string StokKodu { get; set; } = "";

        public string StokAdi { get; set; } = "";



    }
}