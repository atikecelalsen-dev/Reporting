using Reporting.Models;
using Reporting.Sql;
using System.Data;
using static System.Net.WebRequestMethods;

namespace Reporting.Services
{
    public class SalesReportService
    {
       
        private readonly string _connectionString;

        private readonly string _firmaNo;
        private readonly string _donemNo;

        public SalesReportService(string connectionString, string firmaNo, string donemNo)
        {
            _connectionString = connectionString;
            _firmaNo = firmaNo;
            _donemNo = donemNo;
        }

        public List<SalesProfitReportRow> SatisKarZarar(ReportFilter filter)
        {
            List<StockMovementRow> hareketler = StokHareketleriniGetir();

            List<SalesProfitReportRow> satislar = FifoKarZararHesapla(hareketler);

            return satislar
                .OrderByDescending(x => x.Tarih)
                .ToList();
        }

        private List<StockMovementRow> StokHareketleriniGetir()
        {
            List<StockMovementRow> liste = new List<StockMovementRow>();

            string query = @"
                SELECT
                    L.LOGICALREF AS LogicalRef,
                    L.STOCKREF AS StockRef,
                    L.DATE_ AS Tarih,

                    ISNULL(I.FICHENO, '') AS FisNo,

                    ISNULL(C.CODE, '') AS CariKodu,
                    ISNULL(C.DEFINITION_, '') AS CariAdi,

                    L.TRCODE AS TrCode,
                    L.AMOUNT AS Miktar,
                    L.PRICE AS BirimFiyat,
                    L.LINENET AS Tutar,
                    ISNULL(L.INVOICEREF, 0) AS InvoiceRef,

                    ISNULL(IT.CODE, '') AS StokKodu,
                    ISNULL(IT.NAME, '') AS StokAdi

                FROM LG_001_01_STLINE L

                LEFT JOIN LG_001_01_INVOICE I
                    ON I.LOGICALREF = L.INVOICEREF

                LEFT JOIN LG_001_CLCARD C
                    ON C.LOGICALREF = L.CLIENTREF

                LEFT JOIN LG_001_ITEMS IT
                    ON IT.LOGICALREF = L.STOCKREF

                WHERE L.LINETYPE = 0
                  AND L.STOCKREF > 0
                  AND L.TRCODE IN (1,7,8)

                ORDER BY 
                    L.DATE_,
                    L.STOCKREF, 
                    L.LOGICALREF
            ";

            SqlHelper sql = new SqlHelper(_connectionString);
            sql.sorgu = query;
            DataTable dt = sql.selectDataTable();


            foreach (DataRow dr in dt.Rows)
            {
                liste.Add(new StockMovementRow
                {
                    LogicalRef = Convert.ToInt32(dr["LogicalRef"]),
                    StockRef = Convert.ToInt32(dr["StockRef"]),
                    Tarih = Convert.ToDateTime(dr["Tarih"]),
                    FisNo = dr["FisNo"].ToString() ?? "",
                    CariKodu = dr["CariKodu"].ToString() ?? "",
                    CariAdi = dr["CariAdi"].ToString() ?? "",
                    TrCode = Convert.ToInt32(dr["TrCode"]),
                    Miktar = Convert.ToDecimal(dr["Miktar"]),
                    BirimFiyat = Convert.ToDecimal(dr["BirimFiyat"]),
                    Tutar = Convert.ToDecimal(dr["Tutar"]),
                    InvoiceRef = Convert.ToInt32(dr["InvoiceRef"]),
                    StokKodu = dr["StokKodu"].ToString() ?? "",
                    StokAdi = dr["StokAdi"].ToString() ?? ""
                });
            }

            return liste;
        }

        private List<SalesProfitReportRow> FifoKarZararHesapla(List<StockMovementRow> hareketler)
        {
            List<SalesProfitReportRow> rapor = new List<SalesProfitReportRow>();

            Dictionary<int, Queue<FifoPurchaseLot>> stokFifo = new Dictionary<int, Queue<FifoPurchaseLot>>();

            foreach (StockMovementRow hareket in hareketler)
            {
                if (!stokFifo.ContainsKey(hareket.StockRef))
                {
                    stokFifo[hareket.StockRef] = new Queue<FifoPurchaseLot>();
                }

                Queue<FifoPurchaseLot> fifo = stokFifo[hareket.StockRef];

                if (hareket.TrCode == 1)
                {
                    fifo.Enqueue(new FifoPurchaseLot
                    {
                        StockRef = hareket.StockRef,
                        KalanMiktar = hareket.Miktar,
                        BirimMaliyet = hareket.BirimFiyat
                    });

                    continue;
                }

                if (hareket.TrCode == 7 || hareket.TrCode == 8)
                {
                    decimal satilacakMiktar = hareket.Miktar;
                    decimal maliyet = 0;

                    while (satilacakMiktar > 0 && fifo.Count > 0)
                    {
                        FifoPurchaseLot lot = fifo.Peek();

                        decimal kullanilanMiktar = Math.Min(satilacakMiktar, lot.KalanMiktar);

                        maliyet += kullanilanMiktar * lot.BirimMaliyet;

                        lot.KalanMiktar -= kullanilanMiktar;
                        satilacakMiktar -= kullanilanMiktar;

                        if (lot.KalanMiktar <= 0)
                        {
                            fifo.Dequeue();
                        }
                    }

                    decimal ciro = hareket.Tutar;
                    decimal kar = ciro - maliyet;
                    decimal karOrani = OranHesapla(kar, ciro);

                    rapor.Add(new SalesProfitReportRow
                    {
                        Tarih = hareket.Tarih,
                        FisNo = hareket.FisNo,
                        CariKodu = hareket.CariKodu,
                        CariAdi = hareket.CariAdi,
                        Miktar = hareket.Miktar,
                        Ciro = ciro,
                        Maliyet = maliyet,
                        Kar = kar,
                        KarOrani = karOrani,
                        StockRef = hareket.StockRef,
                        StokKodu = hareket.StokKodu,
                        StokAdi = hareket.StokAdi
                    });
                }
            }

            return rapor;
        }

        public SalesReportDashboardModel SatisKarZararDashboard(ReportFilter filter, string periyot)
        {
            List<SalesProfitReportRow> liste = SatisKarZarar(filter);

            periyot = string.IsNullOrWhiteSpace(periyot) ? "gunluk" : periyot;

            //List<SalesProfitReportRow> seciliListe = PeriyodaGoreFiltrele(liste, periyot);
            //List<SalesProfitReportRow> oncekiListe = OncekiPeriyodaGoreFiltrele(liste, periyot);

            List<SalesProfitReportRow> seciliListe = PeriyodaGoreFiltrele(liste, filter);
            List<SalesProfitReportRow> oncekiListe = OncekiPeriyodaGoreFiltrele(liste, filter);

            return new SalesReportDashboardModel
            {
                Periyot = periyot,

                Ciro = KartOlustur(seciliListe.Sum(x => x.Ciro), oncekiListe.Sum(x => x.Ciro)),
                Maliyet = KartOlustur(seciliListe.Sum(x => x.Maliyet), oncekiListe.Sum(x => x.Maliyet)),
                Kar = KartOlustur(seciliListe.Sum(x => x.Kar), oncekiListe.Sum(x => x.Kar)),

                KarOrani = KartOlustur(
                    OranHesapla(seciliListe.Sum(x => x.Kar), seciliListe.Sum(x => x.Ciro)),
                    OranHesapla(oncekiListe.Sum(x => x.Kar), oncekiListe.Sum(x => x.Ciro))
                ),

                //RaporSatirlari = OzetRaporOlustur(liste, filter.Periyot)
                RaporSatirlari = OzetRaporOlustur(
                    filter.Periyot == "ozel" ? seciliListe : liste,
                    filter.Periyot
                )
            };

        }

      

        public ProductProfitReportViewModel UrunKarlilikRaporu(ReportFilter filter)
        {
            string periyot = string.IsNullOrWhiteSpace(filter.Periyot)
                ? "gunluk"
                : filter.Periyot;

            List<SalesProfitReportRow> satislar = SatisKarZarar(filter);

            //satislar = PeriyodaGoreFiltrele(satislar, periyot);
            satislar = PeriyodaGoreFiltrele(satislar, filter);

            List<ProductProfitReportRow> urunler = satislar
                .GroupBy(x => new
                {
                    x.StockRef,
                    x.StokKodu,
                    x.StokAdi
                })
                .Select(g => new ProductProfitReportRow
                {
                    StockRef = g.Key.StockRef,
                    StokKodu = g.Key.StokKodu,
                    StokAdi = g.Key.StokAdi,

                    Miktar = g.Sum(x => x.Miktar),
                    Ciro = g.Sum(x => x.Ciro),
                    Maliyet = g.Sum(x => x.Maliyet),
                    Kar = g.Sum(x => x.Kar),

                    KarOrani = OranHesapla(
                        g.Sum(x => x.Kar),
                        g.Sum(x => x.Ciro)
                    )
                })
                .OrderByDescending(x => x.Kar)
                .ToList();

            return new ProductProfitReportViewModel
            {
                Periyot = periyot,
                Urunler = urunler
            };
        }

        public CustomerProfitReportViewModel MusteriKarlilikRaporu(ReportFilter filter)
        {
            string periyot = string.IsNullOrWhiteSpace(filter.Periyot)
                ? "aylik"
                : filter.Periyot;

            List<SalesProfitReportRow> satislar = SatisKarZarar(filter);

            //satislar = PeriyodaGoreFiltrele(satislar, periyot);
            satislar = PeriyodaGoreFiltrele(satislar, filter);

            List<CustomerProfitReportRow> musteriler = satislar
                .GroupBy(x => new
                {
                    x.CariKodu,
                    x.CariAdi
                })
                .Select(g => new CustomerProfitReportRow
                {
                    CariKodu = g.Key.CariKodu,
                    CariAdi = g.Key.CariAdi,

                    Ciro = g.Sum(x => x.Ciro),
                    Maliyet = g.Sum(x => x.Maliyet),
                    Kar = g.Sum(x => x.Kar),

                    KarOrani = OranHesapla(
                        g.Sum(x => x.Kar),
                        g.Sum(x => x.Ciro)
                    ),

                    FaturaSayisi = g.Select(x => x.FisNo).Distinct().Count()
                })
                .OrderByDescending(x => x.Kar)
                .ToList();

            return new CustomerProfitReportViewModel
            {
                Periyot = periyot,
                Musteriler = musteriler
            };
        }


        private ReportDashboardCard KartOlustur(decimal simdiki, decimal onceki)
        {
            decimal degisimOrani;

            if (onceki == 0)
            {
                degisimOrani = simdiki == 0 ? 0 : 100;
            }
            else
            {
                degisimOrani = ((simdiki - onceki) / onceki) * 100;
            }

            return new ReportDashboardCard
            {
                Deger = simdiki,
                OncekiDeger = onceki,
                DegisimOrani = degisimOrani,
                ArttiMi = degisimOrani >= 0
            };
        }

        private decimal OranHesapla(decimal kar, decimal ciro)
        {
            return ciro == 0 ? 0 : (kar / ciro) * 100;
        }

       

        private List<SalesProfitReportRow> PeriyodaGoreFiltrele(
                    List<SalesProfitReportRow> liste,
                    ReportFilter filter)
        {
            string periyot = string.IsNullOrWhiteSpace(filter.Periyot)
                ? "gunluk"
                : filter.Periyot;

            DateTime referansTarih = filter.BaslangicTarihi?.Date ?? DateTime.Today;

            DateTime baslangic;
            DateTime bitis;

            if (periyot == "gunluk")
            {
                baslangic = referansTarih;
                bitis = referansTarih;
            }
            else if (periyot == "haftalik")
            {
                baslangic = referansTarih.AddDays(
                    referansTarih.DayOfWeek == DayOfWeek.Sunday
                        ? -6
                        : DayOfWeek.Monday - referansTarih.DayOfWeek
                );

                bitis = baslangic.AddDays(6);
            }
            else if (periyot == "aylik")
            {
                baslangic = new DateTime(referansTarih.Year, referansTarih.Month, 1);
                bitis = baslangic.AddMonths(1).AddDays(-1);
            }
            else if (periyot == "yillik")
            {
                baslangic = new DateTime(referansTarih.Year, 1, 1);
                bitis = new DateTime(referansTarih.Year, 12, 31);
            }
            else if (periyot == "ozel")
            {
                baslangic = filter.BaslangicTarihi?.Date ?? referansTarih;
                bitis = filter.BitisTarihi?.Date ?? baslangic;
            }
            else
            {
                baslangic = DateTime.MinValue;
                bitis = DateTime.MaxValue;
            }

            return liste
                .Where(x => x.Tarih.Date >= baslangic &&
                            x.Tarih.Date <= bitis)
                .ToList();
        }


        private List<SalesProfitReportRow> OncekiPeriyodaGoreFiltrele(
            List<SalesProfitReportRow> liste,
            ReportFilter filter)
        {
            string periyot = string.IsNullOrWhiteSpace(filter.Periyot)
        ? "aylik"
        : filter.Periyot;

            DateTime bugun = DateTime.Today;

            DateTime haftaBaslangic = bugun.AddDays(
                bugun.DayOfWeek == DayOfWeek.Sunday
                    ? -6
                    : DayOfWeek.Monday - bugun.DayOfWeek
            );

            if (periyot == "gunluk")
            {
                DateTime dun = bugun.AddDays(-1);

                return liste
                    .Where(x => x.Tarih.Date == dun)
                    .ToList();
            }

            if (periyot == "haftalik")
            {
                DateTime oncekiHaftaBaslangic = haftaBaslangic.AddDays(-7);
                DateTime oncekiHaftaBitis = haftaBaslangic.AddDays(-1);

                return liste
                    .Where(x => x.Tarih.Date >= oncekiHaftaBaslangic &&
                                x.Tarih.Date <= oncekiHaftaBitis)
                    .ToList();
            }

            if (periyot == "aylik")
            {
                DateTime oncekiAy = bugun.AddMonths(-1);

                return liste
                    .Where(x => x.Tarih.Year == oncekiAy.Year &&
                                x.Tarih.Month == oncekiAy.Month)
                    .ToList();
            }

            if (periyot == "yillik")
            {
                DateTime oncekiYil = bugun.AddYears(-1);

                return liste
                    .Where(x => x.Tarih.Year == oncekiYil.Year)
                    .ToList();
            }
            if (periyot == "ozel" &&
                filter.BaslangicTarihi.HasValue &&
                filter.BitisTarihi.HasValue)
            {
                DateTime baslangic = filter.BaslangicTarihi.Value.Date;
                DateTime bitis = filter.BitisTarihi.Value.Date;

                int gunSayisi = (bitis - baslangic).Days + 1;

                DateTime oncekiBitis = baslangic.AddDays(-1);
                DateTime oncekiBaslangic = oncekiBitis.AddDays(-(gunSayisi - 1));

                return liste
                    .Where(x => x.Tarih.Date >= oncekiBaslangic &&
                                x.Tarih.Date <= oncekiBitis)
                    .ToList();
            }

            return new List<SalesProfitReportRow>();
        }
       
        private List<SalesReportSummaryRow> OzetRaporOlustur(
            List<SalesProfitReportRow> liste,
            string periyot)
        {
            if (periyot == "ozel")
            {
                return liste
                    .GroupBy(x => x.Tarih.Date)
                    .Select(g => new SalesReportSummaryRow
                    {
                        Tarih = g.Key,
                        Baslik = g.Key.ToString("dd.MM.yyyy"),
                        Ciro = g.Sum(x => x.Ciro),
                        Maliyet = g.Sum(x => x.Maliyet),
                        Kar = g.Sum(x => x.Kar),
                        KarOrani = OranHesapla(g.Sum(x => x.Kar), g.Sum(x => x.Ciro))
                    })
                    .OrderBy(x => x.Tarih)
                    .ToList();
            }

            if (periyot == "haftalik")
            {
                return liste
                    .GroupBy(x => new
                    {
                        Yil = x.Tarih.Year,
                        Hafta = System.Globalization.ISOWeek.GetWeekOfYear(x.Tarih)
                    })
                    .Select(g => new SalesReportSummaryRow
                    {
                        Tarih = g.Max(x => x.Tarih),
                        Baslik = $"{g.Key.Yil} / {g.Key.Hafta}. Hafta",
                        Ciro = g.Sum(x => x.Ciro),
                        Maliyet = g.Sum(x => x.Maliyet),
                        Kar = g.Sum(x => x.Kar),
                        KarOrani = OranHesapla(g.Sum(x => x.Kar), g.Sum(x => x.Ciro))
                    })
                    .OrderByDescending(x => x.Tarih)
                    .ToList();
            }

            if (periyot == "aylik")
            {
                return liste
                    .GroupBy(x => new { x.Tarih.Year, x.Tarih.Month })
                    .Select(g => new SalesReportSummaryRow
                    {
                        Tarih = new DateTime(g.Key.Year, g.Key.Month, 1),
                        Baslik = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                        Ciro = g.Sum(x => x.Ciro),
                        Maliyet = g.Sum(x => x.Maliyet),
                        Kar = g.Sum(x => x.Kar),
                        KarOrani = OranHesapla(g.Sum(x => x.Kar), g.Sum(x => x.Ciro))
                    })
                    .OrderByDescending(x => x.Tarih)
                    .ToList();
            }

            if (periyot == "yillik")
            {
                return liste
                    .GroupBy(x => x.Tarih.Year)
                    .Select(g => new SalesReportSummaryRow
                    {
                        Tarih = new DateTime(g.Key, 1, 1),
                        Baslik = g.Key.ToString(),
                        Ciro = g.Sum(x => x.Ciro),
                        Maliyet = g.Sum(x => x.Maliyet),
                        Kar = g.Sum(x => x.Kar),
                        KarOrani = OranHesapla(g.Sum(x => x.Kar), g.Sum(x => x.Ciro))
                    })
                    .OrderByDescending(x => x.Tarih)
                    .ToList();
            }

            return liste
                .GroupBy(x => x.Tarih.Date)
                .Select(g => new SalesReportSummaryRow
                {
                    Tarih = g.Key,
                    Baslik = g.Key.ToString("dd.MM.yyyy"),
                    Ciro = g.Sum(x => x.Ciro),
                    Maliyet = g.Sum(x => x.Maliyet),
                    Kar = g.Sum(x => x.Kar),
                    KarOrani = OranHesapla(g.Sum(x => x.Kar), g.Sum(x => x.Ciro))
                })
                .OrderByDescending(x => x.Tarih)
                .ToList();
        }
    }
}