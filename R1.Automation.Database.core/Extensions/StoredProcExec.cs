using System;
using System.Collections.Generic;
using System.Data;
using Dapper;
using R1.Automation.Database.core.Base;

namespace R1.Automation.Database.core.Extensions
{
    public class StoredProcExec : DataAccess
    {
        DynamicParameters parameters = new DynamicParameters();
        DataTable dt = new DataTable();
        string parName = "", parValue = "";

        /// <summary>
        // This methods is user for calling of store procedure which return the result by using select statment. 
        /// </summary>
        /// <param name="con"></param>
        /// <param name="storeProcedure"></param>
        /// <param name="columValues"></p aram>
        /// <returns></returns>
        /// 
        public dynamic ExecuteSelectProc(IDbConnection con, string storeProcedure, Dictionary<object, object> columValues)
        {
            dynamic proc;
            if (con != null)
            {
                DictionaryToParameter(columValues);
                proc = con.ExecuteReader(storeProcedure, parameters, commandType: CommandType.StoredProcedure);
            }
            else
                throw new ArgumentNullException("Connection object is null");
            return proc;
        }
        /// <summary>
        /// This method is user for execute DML commands.
        /// </summary>
        /// <param name="con"></param>
        /// <param name="storeProcedure"></param>
        /// <param name="columValues"></param>
        /// <returns></returns>
        public dynamic ExecuteDmlProc(IDbConnection con, string storeProcedure, Dictionary<object, object> columValues)
        {
            dynamic proc;
            if (con != null)
            {
                DictionaryToParameter(columValues);
                proc = con.Execute(storeProcedure, parameters, commandType: CommandType.StoredProcedure);
            }
            else
                throw new ArgumentNullException("Connection object is null");
            return proc;
        }

        /// <summary>
        /// This method is user for execute select commands, with getting expected error message.
        /// </summary>
        /// <param name="ConnectionString"></param>
        /// <param name="storeProcedure"></param>
        /// <param name="columValues"></param>
        /// <param name="errorParameter"></param>
        /// <returns></returns>
        public dynamic ExecuteSelectWithError(IDbConnection con, string storeProcedure, Dictionary<object, object> columValues, out string errorParameter)
        {
            dynamic proc = null;
            errorParameter = "No error";
            if (con != null)
            {
                DictionaryToParameter(columValues);
                object effectRows = con.ExecuteScalar(storeProcedure, parameters, commandType: CommandType.StoredProcedure);
                if (Convert.ToString(effectRows) == "")
                {
                    using (var reader = con.ExecuteReader(storeProcedure, parameters, commandType: CommandType.StoredProcedure))
                    {
                        while (reader.NextResult())
                        {
                            reader.Read();
                            if (reader.GetValue(0) != null)
                            {
                                errorParameter = reader.GetValue(0).ToString().Trim();
                            }
                        }
                    }
                }
                else
                {
                    proc = con.ExecuteReader(storeProcedure, parameters, commandType: CommandType.StoredProcedure);
                }
            }
            else
                throw new ArgumentNullException("Connection object is null");

            return proc;
        }
        /// <summary>
        /// This method is user for execute DML commands, with getting expected error message.
        /// </summary>
        /// <param name="con"></param>
        /// <param name="storeProcedure"></param>
        /// <param name="columValues"></param>
        /// <param name="errorParameter"></param>
        /// <returns></returns>
        public dynamic ExecuteDmlProcError(IDbConnection con, string storeProcedure, Dictionary<object, object> columValues, out string errorParameter)
        {
            dynamic proc;
            errorParameter = "No error";
            if (con != null)
            {
                DictionaryToParameter(columValues);
                proc = con.Execute(storeProcedure, parameters, commandType: CommandType.StoredProcedure);
                if (proc == 0)
                {
                    dt.Load(con.ExecuteReader(storeProcedure, parameters, commandType: CommandType.StoredProcedure));
                    errorParameter = dt.Rows[0][0].ToString().Trim();
                }
            }
            else
                throw new ArgumentNullException("Connection object is null");
            return proc;
        }

        /// <summary>
        /// This method is use in many methods for assign parameter and value in DynamicParameters for using in store procedure. 
        /// </summary>
        /// <param name="columValues"></param>
        /// <param name="parameters"></param>
        private void DictionaryToParameter(Dictionary<object, object> columValues)
        {
            foreach (KeyValuePair<object, object> dic in columValues)
            {
                parName = "@" + dic.Key.ToString().Trim() + "";
                parValue = dic.Value.ToString().Trim();
                parameters.Add(parName, parValue);
            }
        }
    }
}
