using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reporting.Models
{
    public class ReportFilter
    {
        public DateTime? BaslangicTarihi { get; set; }

        public DateTime? BitisTarihi { get; set; }

        public string Periyot { get; set; } = "gunluk";
        public int? CariRef { get; set; }

        public int? StokRef { get; set; }
    }
}