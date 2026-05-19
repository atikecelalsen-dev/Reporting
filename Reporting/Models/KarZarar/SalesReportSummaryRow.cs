using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reporting.Models
{
    public class SalesReportSummaryRow
    {
        public DateTime Tarih { get; set; }

        public string Baslik { get; set; } = "";

        public decimal Ciro { get; set; }

        public decimal Maliyet { get; set; }

        public decimal Kar { get; set; }

        public decimal KarOrani { get; set; }
    }
}
