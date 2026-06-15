using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reporting.Models
{
    public class CurrentAccountStatementViewModel
    {
        public int? CariRef { get; set; }
        public string CariKodu { get; set; } = "";
        public string CariAdi { get; set; } = "";

        public DateTime BaslangicTarihi { get; set; }
        public DateTime BitisTarihi { get; set; }

        public decimal DevirBakiye { get; set; }
        public decimal ToplamBorc { get; set; }
        public decimal ToplamAlacak { get; set; }
        public decimal SonBakiye => DevirBakiye + ToplamBorc - ToplamAlacak;

        public List<CurrentAccountStatementRow> Hareketler { get; set; } = new();
        public List<CurrentAccountSelectItem> Cariler { get; set; } = new();
    }

    public class CurrentAccountStatementRow
    {
        public DateTime Tarih { get; set; }
        public string FisNo { get; set; } = "";
        public string Aciklama { get; set; } = "";
        public decimal Borc { get; set; }
        public decimal Alacak { get; set; }
        public decimal Bakiye { get; set; }
    }

    public class CurrentAccountSelectItem
    {
        public int LogicalRef { get; set; }
        public string Kod { get; set; } = "";
        public string Ad { get; set; } = "";
    }
}
