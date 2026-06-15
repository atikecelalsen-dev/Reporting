using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Reporting.Models;
using Reporting.Sql;
using System.Data;

namespace Reporting.Services
{
    public class CurrentAccountStatementService
    {
        private readonly string _connectionString;
        private readonly string _firmaNo;
        private readonly string _donemNo;

        public CurrentAccountStatementService(string connectionString, string firmaNo, string donemNo)
        {
            _connectionString = connectionString;
            _firmaNo = firmaNo;
            _donemNo = donemNo;
        }

        public CurrentAccountStatementViewModel GetReport(int? cariRef, DateTime baslangicTarihi, DateTime bitisTarihi)
        {
            var model = new CurrentAccountStatementViewModel
            {
                CariRef = cariRef,
                BaslangicTarihi = baslangicTarihi.Date,
                BitisTarihi = bitisTarihi.Date,
                Cariler = GetCariler()
            };

            if (cariRef == null || cariRef <= 0)
                return model;

            string clcardTable = $"LG_{_firmaNo}_CLCARD";
            string clflineTable = $"LG_{_firmaNo}_{_donemNo}_CLFLINE";

            string baslangic = baslangicTarihi.ToString("yyyy-MM-dd");
            string bitis = bitisTarihi.ToString("yyyy-MM-dd");

            string cariQuery = $@"
                SELECT 
                    LOGICALREF,
                    CODE,
                    DEFINITION_
                FROM {clcardTable}
                WHERE LOGICALREF = {cariRef}
            ";

            SqlHelper cariSql = new SqlHelper(_connectionString);
            cariSql.sorgu = cariQuery;

            DataTable cariDt = cariSql.selectDataTable();

            if (cariDt.Rows.Count > 0)
            {
                DataRow cari = cariDt.Rows[0];

                model.CariKodu = cari["CODE"]?.ToString() ?? "";
                model.CariAdi = cari["DEFINITION_"]?.ToString() ?? "";
            }

            string devirQuery = $@"
                SELECT
                    SUM(CASE WHEN SIGN = 0 THEN AMOUNT ELSE 0 END) AS Borc,
                    SUM(CASE WHEN SIGN = 1 THEN AMOUNT ELSE 0 END) AS Alacak
                FROM {clflineTable}
                WHERE CLIENTREF = {cariRef}
                  AND DATE_ < '{baslangic}'
            ";

            SqlHelper devirSql = new SqlHelper(_connectionString);
            devirSql.sorgu = devirQuery;

            DataTable devirDt = devirSql.selectDataTable();

            if (devirDt.Rows.Count > 0)
            {
                decimal oncekiBorc = devirDt.Rows[0]["Borc"] == DBNull.Value ? 0 : Convert.ToDecimal(devirDt.Rows[0]["Borc"]);
                decimal oncekiAlacak = devirDt.Rows[0]["Alacak"] == DBNull.Value ? 0 : Convert.ToDecimal(devirDt.Rows[0]["Alacak"]);

                model.DevirBakiye = oncekiBorc - oncekiAlacak;
            }

            string hareketQuery = $@"
                SELECT
                    DATE_ AS Tarih,
                    TRANNO AS FisNo,
                    LINEEXP AS Aciklama,
                    SIGN,
                    AMOUNT
                FROM {clflineTable}
                WHERE CLIENTREF = {cariRef}
                  AND DATE_ >= '{baslangic}'
                  AND DATE_ <= '{bitis}'
                ORDER BY DATE_, LOGICALREF
            ";

            SqlHelper hareketSql = new SqlHelper(_connectionString);
            hareketSql.sorgu = hareketQuery;

            DataTable hareketDt = hareketSql.selectDataTable();

            decimal bakiye = model.DevirBakiye;

            foreach (DataRow dr in hareketDt.Rows)
            {
                int sign = dr["SIGN"] == DBNull.Value ? 0 : Convert.ToInt32(dr["SIGN"]);
                decimal amount = dr["AMOUNT"] == DBNull.Value ? 0 : Convert.ToDecimal(dr["AMOUNT"]);

                decimal borc = sign == 0 ? amount : 0;
                decimal alacak = sign == 1 ? amount : 0;

                bakiye += borc - alacak;

                model.ToplamBorc += borc;
                model.ToplamAlacak += alacak;

                model.Hareketler.Add(new CurrentAccountStatementRow
                {
                    Tarih = dr["Tarih"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(dr["Tarih"]),
                    FisNo = dr["FisNo"]?.ToString() ?? "",
                    Aciklama = dr["Aciklama"]?.ToString() ?? "",
                    Borc = borc,
                    Alacak = alacak,
                    Bakiye = bakiye
                });
            }

            return model;
        }

        public List<CurrentAccountSelectItem> GetCariler()
        {
            string clcardTable = $"LG_{_firmaNo}_CLCARD";

            string query = $@"
                SELECT 
                    LOGICALREF,
                    CODE,
                    DEFINITION_
                FROM {clcardTable}
                ORDER BY DEFINITION_
            ";

            SqlHelper sql = new SqlHelper(_connectionString);
            sql.sorgu = query;

            DataTable dt = sql.selectDataTable();

            var list = new List<CurrentAccountSelectItem>();

            foreach (DataRow dr in dt.Rows)
            {
                list.Add(new CurrentAccountSelectItem
                {
                    LogicalRef = dr["LOGICALREF"] == DBNull.Value ? 0 : Convert.ToInt32(dr["LOGICALREF"]),
                    Kod = dr["CODE"]?.ToString() ?? "",
                    Ad = dr["DEFINITION_"]?.ToString() ?? ""
                });
            }

            return list;
        }
    }
}
