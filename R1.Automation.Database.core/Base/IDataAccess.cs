using System;
using System.Data;

namespace R1.Automation.Database.core.Base
{
    interface IDataAccess
    {
        public string ExecuteQuerySingleValue(IDbConnection con, string query);
        public int ExecuteEffectRowQuery(IDbConnection con, string query);
        public DataTable SelectExecuteQuery(IDbConnection con, string query);
        public IDbConnection ConnectToDB(String connectionString);
        public IDbConnection GetDBConnection();
        public void CloseDBConnection();
    }
}
