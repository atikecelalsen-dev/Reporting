using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reporting.Models
{
    public class UnsoldProductsReportViewModel
    {
        public int ToplamUrunSayisi { get; set; }
        public int UzunSuredirSatilmayanSayisi { get; set; }
        public int HicSatilmamisSayisi { get; set; }

        public List<UnsoldProductRow> UzunSuredirSatilmayanlar { get; set; } = new();
        public List<UnsoldProductRow> HicSatilmamisUrunler { get; set; } = new();
    }

    public class UnsoldProductRow
    {
        public int StockRef { get; set; }

        public string StokKodu { get; set; } = "";
        public string StokAdi { get; set; } = "";

        public decimal MevcutStok { get; set; }

        public DateTime? SonSatisTarihi { get; set; }

        public int? GecenGun { get; set; }
    }
}