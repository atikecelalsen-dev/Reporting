using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reporting.Models
{
    public class IncomeExpenseDetailRow
    {
        public DateTime Tarih { get; set; }

        public string FisNo { get; set; }

        public string CariAdi { get; set; }

        public string Tip { get; set; }
        public string Kalem { get; set; } = "";

        public decimal Tutar { get; set; }
    }
}
