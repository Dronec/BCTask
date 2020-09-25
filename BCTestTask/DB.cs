using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace BCTestTask
{
    public class DB : IDisposable
    {
        private readonly SqlConnection sqlConn;
        public DB(string connStr)
        {
            sqlConn = new SqlConnection(connStr);
            sqlConn.Open();
        }
        public void Dispose()
        {
            sqlConn.Close();
        }

        private object GetScalar(string query, params SqlParameter[] pars)
        {
            return GetTable(query, pars).Rows[0][0];
        }
        private DataTable GetTable(string query, params SqlParameter[] pars)
        {
            DataTable dataTable = new DataTable();
            using SqlCommand comm = new SqlCommand(query, sqlConn)
            {
                CommandTimeout = 600
            };
            if (pars.Count() > 0)
                pars.Select(p => comm.Parameters.Add(p)).Count();
            using (SqlDataReader reader = comm.ExecuteReader())
            {
                dataTable.Load(reader);
            }
            return dataTable;
        }
        private List<string> GetTableFields(string TableName)
        {
            return GetTable("sp_describe_first_result_set @tsql = @tsql", new SqlParameter("@tsql", $"SELECT * FROM {TableName}"))
                .AsEnumerable().Select(r => r.Field<string>("name")).ToList();
        }
        public static string GetCDNInstanceFromTable(string TableName)
        {
            return TableName.Replace(".", "_");
        }
        public bool CheckIfTableExists(string TableName)
        {
            return (bool)GetScalar(@"IF OBJECT_ID(@CDCTableName) IS NULL
	SELECT CAST(0 AS BIT)
ELSE
	SELECT CAST(1 AS BIT)", new SqlParameter("@CDCTableName", TableName));
        }
        public string GetFirstLSN(string TableName)
        {
            string CDCInstanceName = GetCDNInstanceFromTable(TableName);
            return (string)GetScalar(@"SELECT ISNULL(CONVERT(VARCHAR, NULLIF(sys.fn_cdc_get_min_lsn(@CDCInstanceName), 0x00000000000000000000), 1), '') FromLSN",
                new SqlParameter("@CDCInstanceName", CDCInstanceName));
        }
        public string GetLastLSN()
        {
            return (string)GetScalar("SELECT CONVERT(VARCHAR,sys.fn_cdc_get_max_lsn(),1)");
        }
        public DataTable GetCDCFeed(string TableName, string lsnFrom, string lsnTo)
        {
            string CDCInstanceName = GetCDNInstanceFromTable(TableName);
            List<string> fields = GetTableFields(TableName);
            string operationsLookup = @"CASE [__$operation]
			WHEN 1
				THEN 'DELETE'
			WHEN 2
				THEN 'INSERT'
			WHEN 4
				THEN 'UPDATE'
			END Operation";
            string CDCQuery = $"SELECT {operationsLookup},{String.Join(",", fields)} FROM cdc.fn_cdc_get_all_changes_{CDCInstanceName}(CONVERT(binary(10),@from_lsn,1), CONVERT(binary(10),@to_lsn,1), N'all');";
            return GetTable(CDCQuery, new SqlParameter("@from_lsn", lsnFrom), new SqlParameter("@to_lsn", lsnTo));
        }
    }
}
