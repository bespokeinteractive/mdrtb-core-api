using System;
using System.Data.SqlClient;
using EtbSomalia.Helpers;
using Microsoft.Extensions.Options;

namespace EtbSomalia.Extensions
{
    public class SqlServerConnection {
        private readonly string sConn; //= "Data Source=41.76.170.72;Initial Catalog=EtbSomalia;User ID=ct;Password=ct-2011;Max Pool Size=200;";
        private readonly SqlConnection conn; //= new SqlConnection(sConn);
        private SqlCommand comm = new SqlCommand();

        public SqlServerConnection(AppSettings settings) {
            sConn = settings.ConnString;
            conn = new SqlConnection(sConn);
        }

        public SqlDataReader SqlServerConnect(string SqlString) {
            try {
                conn.Open();
                comm = new SqlCommand(SqlString, conn);

                return comm.ExecuteReader();
            }
            catch (Exception) {
                return null;
            }
        }

        public long SqlServerUpdate(string SqlString) {
            try {
                SqlCommand command = new SqlCommand(SqlString, conn);
                command.Connection.Open();

                if (SqlString.ToLower().Contains("output")) {
                    return Convert.ToInt64(command.ExecuteScalar());
                }
                else {
                    command.ExecuteNonQuery();
                    return 0;
                }
            }
            catch (Exception ex) {
                Console.WriteLine("Post Error: " + ex.Message);
                return 0;
            }
            finally {
                if (conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
            }
        }

        public string GetQueryString(string filter, string command, string sAdditionalString = "", bool AndJoin = true) {
            string query = "";
            string JOIN = " AND ";

            if (!AndJoin)
                JOIN = " OR ";

            char[] Seps = new[] { ' ', '*', '-', '&', '%', '/', '$', '#' };
            string[] MyInfo = filter.Split(Seps, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i <= (MyInfo.Length - 1); i++) {
                if (JOIN.Trim() == "OR" & !(MyInfo[i].Length > 1))
                    continue;

                if (query.Trim() == "")
                    query = " WHERE (" + command + " LIKE '%" + GetValidSqlString(MyInfo[i]) + "%'";
                else
                    query += JOIN + command + " LIKE '%" + GetValidSqlString(MyInfo[i]) + "%'";
            }

            if (query != "")
                query += ")";

            if (sAdditionalString != "") {
                if (query == "")
                    query = " WHERE " + sAdditionalString;
                else
                    query += " AND " + sAdditionalString;
            }

            return query;
        }

        private string GetValidSqlString(String query) {
            return query.Replace("'", "''");
        }

    }
}
