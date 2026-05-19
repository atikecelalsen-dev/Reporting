using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reporting.Models
{
    public class ReportDashboardCard
    {
        public decimal Deger { get; set; }

        public decimal OncekiDeger { get; set; }

        public decimal DegisimOrani { get; set; }

        public bool ArttiMi { get; set; }

        public string Icon => ArttiMi ? "trend-up" : "trend-down";

        public string Renk => ArttiMi ? "text-green-600" : "text-red-600";
    }
}
