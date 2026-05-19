using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using System.Data.SqlClient;

namespace Reporting.Sql
{
    public class SqlHelper
    {
        private readonly string _connectionString;

        public string sorgu = "";

        public SqlHelper(string connectionString)
        {
            _connectionString = connectionString;
        }

        public DataTable selectDataTable()
        {
            DataTable dt = new DataTable();

            using SqlConnection conn = new SqlConnection(_connectionString);
            using SqlDataAdapter da = new SqlDataAdapter(sorgu, conn);

            da.Fill(dt);

            return dt;
        }
    }
}
