using Reporting.Models;
using Reporting.Sql;
using System.Data;
using System.Data.SqlClient;

namespace Reporting.Services
{
    public class IncomeExpenseReportService
    {
        private readonly string _connectionString;
        private readonly string _firmaNo;
        private readonly string _donemNo;

        public IncomeExpenseReportService(string connectionString, string firmaNo , string donemNo )
        {
            _connectionString = connectionString;
            _firmaNo = firmaNo;
            _donemNo = donemNo;
        }


        public IncomeExpenseReportViewModel GetReport(DateTime baslangicTarihi, DateTime bitisTarihi)
        {
            var model = new IncomeExpenseReportViewModel
            {
                BaslangicTarihi = baslangicTarihi.Date,
                BitisTarihi = bitisTarihi.Date
            };

            string invoiceTable = $"LG_{_firmaNo}_{_donemNo}_INVOICE";
            string clcardTable = $"LG_{_firmaNo}_CLCARD";

            string baslangic = baslangicTarihi.ToString("yyyy-MM-dd");
            string bitis = bitisTarihi.ToString("yyyy-MM-dd");

            string query = $@"
                SELECT
                    I.DATE_ AS Tarih,
                    I.FICHENO AS FisNo,
                    ISNULL(C.DEFINITION_, 'Cari Bulunamadı') AS CariAdi,
                    I.TRCODE,
                    I.NETTOTAL AS Tutar
                FROM {invoiceTable} I
                LEFT JOIN {clcardTable} C
                    ON C.LOGICALREF = I.CLIENTREF
                WHERE I.TRCODE IN (1, 4, 8)
                  AND I.DATE_ >= '{baslangic}'
                  AND I.DATE_ <= '{bitis}'
                ORDER BY I.DATE_ DESC, I.FICHENO DESC
            ";

            SqlHelper sql = new SqlHelper(_connectionString);
            sql.sorgu = query;

            DataTable dt = sql.selectDataTable();

            foreach (DataRow dr in dt.Rows)
            {
                int trcode = dr["TRCODE"] == DBNull.Value ? 0 : Convert.ToInt32(dr["TRCODE"]);
                decimal tutar = dr["Tutar"] == DBNull.Value ? 0 : Convert.ToDecimal(dr["Tutar"]);

                var row = new IncomeExpenseDetailRow
                {
                    Tarih = dr["Tarih"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(dr["Tarih"]),
                    FisNo = dr["FisNo"]?.ToString() ?? "",
                    CariAdi = dr["CariAdi"]?.ToString() ?? "",
                    Tutar = tutar
                };

                if (trcode == 8)
                {
                    row.Tip = "Gelir";
                    row.Kalem = "Satışlar";
                    model.SalesIncome += tutar;
                }
                else if (trcode == 1)
                {
                    row.Tip = "Gider";
                    row.Kalem = "Alışlar";
                    model.PurchaseExpense += tutar;
                }
                else if (trcode == 4)
                {
                    row.Tip = "Gider";
                    row.Kalem = "Hizmet Giderleri";
                    model.ServiceExpense += tutar;
                }

                model.Detaylar.Add(row);
            }

            return model;
        }


        //public IncomeExpenseReportViewModel GetReport(DateTime baslangicTarihi, DateTime bitisTarihi)
        //{
        //    var model = new IncomeExpenseReportViewModel
        //    {
        //        BaslangicTarihi = baslangicTarihi.Date,
        //        BitisTarihi = bitisTarihi.Date
        //    };

        //    string invoiceTable = $"LG_{_firmaNo}_{_donemNo}_INVOICE";
        //    string clcardTable = $"LG_{_firmaNo}_CLCARD";

        //    string query = $@"
        //        SELECT
        //            I.DATE_ AS Tarih,
        //            I.FICHENO AS FisNo,
        //            ISNULL(C.DEFINITION_, 'Cari Bulunamadı') AS CariAdi,
        //            I.TRCODE,
        //            I.NETTOTAL AS Tutar
        //        FROM {invoiceTable} I
        //        LEFT JOIN {clcardTable} C
        //            ON C.LOGICALREF = I.CLIENTREF
        //        WHERE I.TRCODE IN (1, 4, 8)
        //          AND I.DATE_ >= @BaslangicTarihi
        //          AND I.DATE_ <= @BitisTarihi
        //        ORDER BY I.DATE_ DESC, I.FICHENO DESC
        //    ";

        //    using var connection = new SqlConnection(_connectionString);
        //    using var command = new SqlCommand(query, connection);

        //    command.Parameters.Add("@BaslangicTarihi", SqlDbType.Date).Value = baslangicTarihi.Date;
        //    command.Parameters.Add("@BitisTarihi", SqlDbType.Date).Value = bitisTarihi.Date;

        //    connection.Open();

        //    using var reader = command.ExecuteReader();

        //    while (reader.Read())
        //    {
        //        int trcode = reader["TRCODE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["TRCODE"]);
        //        decimal tutar = reader["Tutar"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["Tutar"]);

        //        var row = new IncomeExpenseDetailRow
        //        {
        //            Tarih = reader["Tarih"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["Tarih"]),
        //            FisNo = reader["FisNo"]?.ToString() ?? "",
        //            CariAdi = reader["CariAdi"]?.ToString() ?? "",
        //            Tutar = tutar
        //        };

        //        if (trcode == 8)
        //        {
        //            row.Tip = "Gelir";
        //            row.Kalem = "Satışlar";
        //            model.SalesIncome += tutar;
        //        }
        //        else if (trcode == 1)
        //        {
        //            row.Tip = "Gider";
        //            row.Kalem = "Alışlar";
        //            model.PurchaseExpense += tutar;
        //        }
        //        else if (trcode == 4)
        //        {
        //            row.Tip = "Gider";
        //            row.Kalem = "Hizmet Giderleri";
        //            model.ServiceExpense += tutar;
        //        }

        //        model.Detaylar.Add(row);
        //    }

        //    return model;
        //}


    }
}