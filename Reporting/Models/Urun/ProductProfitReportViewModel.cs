using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reporting.Models
{
    public class ProductProfitReportViewModel
    {
        public string Periyot { get; set; } = "gunluk";

        public ReportDashboardCard Ciro { get; set; } = new();
        public ReportDashboardCard Maliyet { get; set; } = new();
        public ReportDashboardCard Kar { get; set; } = new();
        public ReportDashboardCard KarOrani { get; set; } = new();

        public List<ProductProfitReportRow> Urunler { get; set; } = new();
    }
}
