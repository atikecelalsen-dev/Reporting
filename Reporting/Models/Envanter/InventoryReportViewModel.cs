using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reporting.Models
{
    public class InventoryReportViewModel
    {
        public List<InventoryReportRow> Stoklar { get; set; } = new();

        public decimal ToplamStokDegeri { get; set; }

        public int ToplamUrunSayisi { get; set; }

        public int KritikStokSayisi { get; set; }

        public int StokYokSayisi { get; set; }
    }
}
