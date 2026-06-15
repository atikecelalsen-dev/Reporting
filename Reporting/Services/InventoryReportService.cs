using Reporting.Models;
using Reporting.Sql;
using System.Data;

namespace Reporting.Services
{
    public class InventoryReportService
    { 
        private readonly string _connectionString;

        private readonly string _firmaNo;
        private readonly string _donemNo;

        public InventoryReportService(string connectionString, string firmaNo, string donemNo)
        {
            _connectionString = connectionString;
            _firmaNo = firmaNo;
            _donemNo = donemNo;
        }

       

        public InventoryReportViewModel EnvanterRaporu()
        {
            decimal kritikSeviye = 10;

            List<InventoryReportRow> stoklar = new List<InventoryReportRow>();

            string query = @"
                SELECT
                    IT.LOGICALREF AS StockRef,
                    IT.CODE AS StokKodu,
                    IT.NAME AS StokAdi,

                    SUM(CASE WHEN L.TRCODE = 1 THEN ISNULL(L.AMOUNT, 0) ELSE 0 END) AS GirenMiktar,

                    SUM(CASE WHEN L.TRCODE IN (7,8) THEN ISNULL(L.AMOUNT, 0) ELSE 0 END) AS CikanMiktar,

                    SUM(CASE WHEN L.TRCODE = 1 THEN ISNULL(L.AMOUNT, 0) ELSE 0 END)
                    -
                    SUM(CASE WHEN L.TRCODE IN (7,8) THEN ISNULL(L.AMOUNT, 0) ELSE 0 END) AS MevcutStok,

                    CASE 
                        WHEN SUM(CASE WHEN L.TRCODE = 1 THEN ISNULL(L.AMOUNT, 0) ELSE 0 END) = 0 
                        THEN 0
                        ELSE
                            SUM(CASE WHEN L.TRCODE = 1 THEN ISNULL(L.AMOUNT, 0) * ISNULL(L.PRICE, 0) ELSE 0 END)
                            /
                            SUM(CASE WHEN L.TRCODE = 1 THEN ISNULL(L.AMOUNT, 0) ELSE 0 END)
                    END AS OrtalamaMaliyet

                FROM LG_001_ITEMS IT

                LEFT JOIN LG_001_01_STLINE L
                    ON L.STOCKREF = IT.LOGICALREF
                   AND L.LINETYPE = 0
                   AND L.TRCODE IN (1,7,8)

                WHERE
                    ISNULL(IT.NAME, '') <> ''
                    AND ISNULL(IT.CODE, '') <> ''
                    AND ISNULL(IT.CARDTYPE, 0) <> 22

                GROUP BY
                    IT.LOGICALREF,
                    IT.CODE,
                    IT.NAME

                ORDER BY MevcutStok DESC
            ";

            SqlHelper sql = new SqlHelper(_connectionString);
            sql.sorgu = query;
            DataTable dt = sql.selectDataTable();

            foreach (DataRow dr in dt.Rows)
            {
                decimal mevcut = Convert.ToDecimal(dr["MevcutStok"]);
                decimal ortalamaMaliyet = Convert.ToDecimal(dr["OrtalamaMaliyet"]);
                decimal stokDegeri = mevcut * ortalamaMaliyet;

                string stokDurumu =
                    mevcut <= 0 ? "Stok Yok" :
                    mevcut <= kritikSeviye ? "Kritik" :
                    "Yeterli";

                stoklar.Add(new InventoryReportRow
                {
                    StockRef = Convert.ToInt32(dr["StockRef"]),
                    StokKodu = dr["StokKodu"].ToString() ?? "",
                    StokAdi = dr["StokAdi"].ToString() ?? "",

                    GirenMiktar = Convert.ToDecimal(dr["GirenMiktar"]),
                    CikanMiktar = Convert.ToDecimal(dr["CikanMiktar"]),
                    MevcutStok = mevcut,

                    OrtalamaMaliyet = ortalamaMaliyet,
                    StokDegeri = stokDegeri,
                    StokDurumu = stokDurumu
                });
            }

            return new InventoryReportViewModel
            {
                Stoklar = stoklar,
                ToplamStokDegeri = stoklar.Sum(x => x.StokDegeri),
                ToplamUrunSayisi = stoklar.Count,
                KritikStokSayisi = stoklar.Count(x => x.StokDurumu == "Kritik"),
                StokYokSayisi = stoklar.Count(x => x.StokDurumu == "Stok Yok")
            };
        }

        public StockMovementReportViewModel GetStockMovementReport(DateTime? baslangicTarihi, DateTime? bitisTarihi)
        {
            DateTime baslangic = baslangicTarihi ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            DateTime bitis = bitisTarihi ?? DateTime.Today;

            var model = new StockMovementReportViewModel
            {
                BaslangicTarihi = baslangic.Date,
                BitisTarihi = bitis.Date
            };

            string stlineTable = $"LG_{_firmaNo}_{_donemNo}_STLINE";
            string itemTable = $"LG_{_firmaNo}_ITEMS";

            string baslangicSql = baslangic.ToString("yyyy-MM-dd");
            string bitisSql = bitis.ToString("yyyy-MM-dd");

            string query = $@"
                SELECT
                    S.STOCKREF AS StockRef,
                    ISNULL(I.CODE, '') AS StokKodu,
                    ISNULL(I.NAME, '') AS StokAdi,

                    COUNT(*) AS HareketSayisi,

                    SUM(CASE 
                            WHEN S.TRCODE IN (1, 2, 3, 13, 14, 25, 50) THEN ABS(S.AMOUNT)
                            ELSE 0 
                        END) AS GirisMiktari,

                    SUM(CASE 
                             WHEN S.TRCODE IN (7, 8, 9, 10, 11, 12, 51) THEN ABS(S.AMOUNT)
                            ELSE 0 
                        END) AS CikisMiktari

                FROM {stlineTable} S
                LEFT JOIN {itemTable} I
                    ON I.LOGICALREF = S.STOCKREF

                WHERE S.LINETYPE = 0
                  AND S.STOCKREF > 0
                  AND S.DATE_ >= '{baslangicSql}'
                  AND S.DATE_ <= '{bitisSql}'

                GROUP BY
                    S.STOCKREF,
                    I.CODE,
                    I.NAME
            ";

            SqlHelper sql = new SqlHelper(_connectionString);
            sql.sorgu = query;

            DataTable dt = sql.selectDataTable();

            List<StockMovementReportRow> rows = new();

            foreach (DataRow dr in dt.Rows)
            {
                decimal giris = dr["GirisMiktari"] == DBNull.Value ? 0 : Convert.ToDecimal(dr["GirisMiktari"]);
                decimal cikis = dr["CikisMiktari"] == DBNull.Value ? 0 : Convert.ToDecimal(dr["CikisMiktari"]);

                if (giris == 0 && cikis == 0)
                    continue;

                rows.Add(new StockMovementReportRow
                {
                    StockRef = dr["StockRef"] == DBNull.Value ? 0 : Convert.ToInt32(dr["StockRef"]),
                    StokKodu = dr["StokKodu"]?.ToString() ?? "",
                    StokAdi = dr["StokAdi"]?.ToString() ?? "",
                    HareketSayisi = dr["HareketSayisi"] == DBNull.Value ? 0 : Convert.ToInt32(dr["HareketSayisi"]),
                    GirisMiktari = giris,
                    CikisMiktari = cikis
                });
            }

            model.ToplamUrunSayisi = rows.Count;
            model.ToplamGiris = rows.Sum(x => x.GirisMiktari);
            model.ToplamCikis = rows.Sum(x => x.CikisMiktari);

            model.EnCokGirisYapilanlar = rows
                .Where(x => x.GirisMiktari > 0)
                .OrderByDescending(x => x.GirisMiktari)
                .Take(10)
                .ToList();

            model.EnCokCikisYapilanlar = rows
                .Where(x => x.CikisMiktari > 0)
                .OrderByDescending(x => x.CikisMiktari)
                .Take(10)
                .ToList();

            return model;
        }


        public UnsoldProductsReportViewModel GetUnsoldProductsReport()
        {
            var model = new UnsoldProductsReportViewModel();

            string itemTable = $"LG_{_firmaNo}_ITEMS";
            string stlineTable = $"LG_{_firmaNo}_{_donemNo}_STLINE";

            string query = $@"
                SELECT
                    I.LOGICALREF AS StockRef,
                    I.CODE AS StokKodu,
                    I.NAME AS StokAdi,

                    ISNULL(SUM(
                        CASE 
                            WHEN S.IOCODE IN (1, 3) THEN S.AMOUNT
                            WHEN S.IOCODE IN (2, 4) THEN -S.AMOUNT
                            ELSE 0
                        END
                    ), 0) AS MevcutStok,

                    MAX(CASE 
                            WHEN S.TRCODE IN (7, 8) THEN S.DATE_
                            ELSE NULL
                        END) AS SonSatisTarihi

                FROM {itemTable} I

                LEFT JOIN {stlineTable} S
                    ON S.STOCKREF = I.LOGICALREF
                   AND S.LINETYPE = 0

                WHERE I.ACTIVE = 0

                GROUP BY
                    I.LOGICALREF,
                    I.CODE,
                    I.NAME
            ";

            SqlHelper sql = new SqlHelper(_connectionString);
            sql.sorgu = query;

            DataTable dt = sql.selectDataTable();

            List<UnsoldProductRow> tumUrunler = new();

            DateTime bugun = DateTime.Today;

            foreach (DataRow dr in dt.Rows)
            {
                DateTime? sonSatisTarihi = null;
                int? gecenGun = null;

                if (dr["SonSatisTarihi"] != DBNull.Value)
                {
                    sonSatisTarihi = Convert.ToDateTime(dr["SonSatisTarihi"]).Date;
                    gecenGun = (bugun - sonSatisTarihi.Value).Days;
                }

                tumUrunler.Add(new UnsoldProductRow
                {
                    StockRef = dr["StockRef"] == DBNull.Value ? 0 : Convert.ToInt32(dr["StockRef"]),
                    StokKodu = dr["StokKodu"]?.ToString() ?? "",
                    StokAdi = dr["StokAdi"]?.ToString() ?? "",
                    MevcutStok = dr["MevcutStok"] == DBNull.Value ? 0 : Convert.ToDecimal(dr["MevcutStok"]),
                    SonSatisTarihi = sonSatisTarihi,
                    GecenGun = gecenGun
                });
            }

            model.ToplamUrunSayisi = tumUrunler.Count;

            model.UzunSuredirSatilmayanlar = tumUrunler
                .Where(x => x.SonSatisTarihi != null)
                .OrderByDescending(x => x.GecenGun)
                .ToList();

            model.HicSatilmamisUrunler = tumUrunler
                .Where(x => x.SonSatisTarihi == null)
                .OrderByDescending(x => x.MevcutStok)
                .ThenBy(x => x.StokAdi)
                .ToList();

            model.UzunSuredirSatilmayanSayisi = model.UzunSuredirSatilmayanlar.Count;
            model.HicSatilmamisSayisi = model.HicSatilmamisUrunler.Count;

            return model;
        }

    }
}