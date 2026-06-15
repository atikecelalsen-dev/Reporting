using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reporting.Models
{
    public class StockMovementReportViewModel
    {
        public DateTime BaslangicTarihi { get; set; }
        public DateTime BitisTarihi { get; set; }

        public int ToplamUrunSayisi { get; set; }
        public decimal ToplamGiris { get; set; }
        public decimal ToplamCikis { get; set; }
        public decimal NetStokDegisimi => ToplamGiris - ToplamCikis;

        public List<StockMovementReportRow> EnCokGirisYapilanlar { get; set; } = new();
        public List<StockMovementReportRow> EnCokCikisYapilanlar { get; set; } = new();
    }

    public class StockMovementReportRow
    {
        public int StockRef { get; set; }
        public string StokKodu { get; set; } = "";
        public string StokAdi { get; set; } = "";

        public int HareketSayisi { get; set; }
        public decimal GirisMiktari { get; set; }
        public decimal CikisMiktari { get; set; }
        public decimal NetDegisim => GirisMiktari - CikisMiktari;
    }
}
