using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reporting.Models
{
    public class IncomeExpenseReportViewModel
    {
        public DateTime BaslangicTarihi { get; set; }
        public DateTime BitisTarihi { get; set; }

        public decimal SalesIncome { get; set; }
        public decimal PurchaseExpense { get; set; }
        public decimal ServiceExpense { get; set; }

        public decimal TotalIncome => SalesIncome;

        public decimal TotalExpense => PurchaseExpense + ServiceExpense;

        public decimal NetResult => TotalIncome - TotalExpense;

        public string Baslik {  get; set; }

        public List<IncomeExpenseDetailRow> Detaylar { get; set; } = new();


    }
}
