using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reporting.Models
{
    public class CustomerProfitReportRow
    {
        public string CariKodu { get; set; } = "";

        public string CariAdi { get; set; } = "";

        public decimal Ciro { get; set; }

        public decimal Maliyet { get; set; }

        public decimal Kar { get; set; }

        public decimal KarOrani { get; set; }

        public int FaturaSayisi { get; set; }
    }
}
