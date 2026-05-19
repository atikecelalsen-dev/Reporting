using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reporting.Models
{
    public class CustomerProfitReportViewModel
    {
        public string Periyot { get; set; } = "aylik";

        public List<CustomerProfitReportRow> Musteriler { get; set; } = new();

        public ReportDashboardCard Ciro { get; set; } = new();

        public ReportDashboardCard Maliyet { get; set; } = new();

        public ReportDashboardCard Kar { get; set; } = new();

        public ReportDashboardCard KarOrani { get; set; } = new();
    }
}
