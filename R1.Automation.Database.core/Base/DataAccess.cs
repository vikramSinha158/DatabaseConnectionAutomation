using System;
using System.Data;
using System.Data.SqlClient;
using Dapper;

namespace R1.Automation.Database.core.Base
{
    public class DataAccess : IDataAccess
    {
        private IDbConnection con;
        public string ServerName { get; set; }
        public string DatabaseName { get; set; }

        /// <summary>This method is used when single value is expected to be returned as result of passed query</summary>
        /// <param name="connectionstring"></param>
        /// <param name="query"></param>
        /// <returns>Result as string</returns>
        public string ExecuteQuerySingleValue(IDbConnection con, string query)
        {
            string queryResult;
            queryResult = Convert.ToString(con.ExecuteScalar(query, commandType: CommandType.Text));
            return queryResult;
        }

        /// <summary>This method is used for executing DML Query</summary>
        /// <param name="connectionstring"></param>
        /// <param name="query"></param>
        /// <returns>This method is return number of effected rows </returns>
        public int ExecuteEffectRowQuery(IDbConnection con, string query)
        {
            if (con != null)
            {
                int effectRow = con.Execute(query, commandType: CommandType.Text);
                return effectRow;
            }
            else
                throw new ArgumentNullException("Connection object is null");
        }
        /// <summary>This method is return query result in datatable by passing connection and query.</summary>
        /// <param name="con"></param>
        /// <param name="query"></param>
        /// <returns>return query result is datatable</returns>
        public DataTable SelectExecuteQuery(IDbConnection con, string query)
        {
            if (con != null)
            {
                DataTable dataTable = new DataTable();
                dataTable.Load(con.ExecuteReader(query, commandType: CommandType.Text));
                return dataTable;
            }
            else
                throw new ArgumentNullException("Connection object is null");
        }
        /// <summary>/// This method is return  connection with SQLin open mode./// </summary>
        /// <param name="connectionString"></param>
        /// <returns>This method is return  connection </returns>
        public IDbConnection ConnectToDB(String connectionString)
        {
            try
            {
                con = new SqlConnection(connectionString);
                if (con.State == ConnectionState.Closed)
                    con.Open();
            }
            catch (Exception)
            {
                return null;
            }
            return con;
        }

        /// <summary>Gets the database connection object for already initialized connection</summary>
        /// <returns>IDB Connection</returns>
        /// <exception cref="NullReferenceException">DB is not initialized. Please call desirable DB connection methods first</exception>
        public IDbConnection GetDBConnection()
        {
            if (con != null)
                return con;
            else
                throw new NullReferenceException("DB is not initialized. Please call desirable DB connection methods first");
        }

        /// <summary>
        /// This method is use for close open connection.
        /// </summary>
        public void CloseDBConnection()
        {
            if (con != null)
            {
                con.Close();
                con.Dispose();
            }
        }
    }
}