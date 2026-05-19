using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reporting.Models
{
    public class TumRaporlarViewModel
    {
        public SalesReportDashboardModel SatisRaporu { get; set; } = new();

        public ProductProfitReportViewModel UrunKarlilik { get; set; } = new();

        public CustomerProfitReportViewModel MusteriKarlilik { get; set; } = new();

    }
}
