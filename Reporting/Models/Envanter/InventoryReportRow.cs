using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reporting.Models
{
    public class InventoryReportRow
    {
        public int StockRef { get; set; }

        public string StokKodu { get; set; } = "";

        public string StokAdi { get; set; } = "";

        public decimal GirenMiktar { get; set; }

        public decimal CikanMiktar { get; set; }

        public decimal MevcutStok { get; set; }

        public decimal OrtalamaMaliyet { get; set; }

        public decimal StokDegeri { get; set; }

        public string StokDurumu { get; set; } = "";
    }
}
