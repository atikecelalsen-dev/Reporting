using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reporting.Models
{
    public class StockMovementRow
    {
        public int LogicalRef { get; set; }

        public int StockRef { get; set; }

        public DateTime Tarih { get; set; }

        public string FisNo { get; set; } = "";
        public string CariKodu { get; set; } = "";
        public string CariAdi { get; set; } = "";

        public int TrCode { get; set; }

        public decimal Miktar { get; set; }

        public decimal BirimFiyat { get; set; }

        public decimal Tutar { get; set; }

        public int InvoiceRef { get; set; }

        public string StokKodu { get; set; } = "";

        public string StokAdi { get; set; } = "";
    }
}
