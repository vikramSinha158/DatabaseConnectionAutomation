using Dapper;
using Microsoft.Extensions.Configuration;
using R1.Automation.Database.core.Base;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace R1.Automation.Database.core.Extensions
{
    class Accretive : DataAccess
    {
        string facilityCode, coreServer, coreDataBase, auth, userId, passWord, url;
        readonly string queryValue = "SELECT Replace(Replace(serverpath, '[', ''), ']', '')  AS servername, databasename from locations where code = @facilitycode";
        private IDbConnection con;

        public IConfigurationRoot GetAppsetting
        {
            get
            {
                var config = new ConfigurationBuilder()
                 .AddJsonFile("appsettings.json")
                 .Build();
                return config;
            }
        }

        /// <summary>
        /// This method is use return servername and database name according to facility wise by inputing coreserver, coredatabase 
        /// </summary>
        private List<DataAccess> GetTranServerDB(string facilityColumn)
        {
            string connectionString = auth.Equals("Windows") ? "Data Source = " + coreServer + "; Initial Catalog = " + coreDataBase + "; Integrated Security = True" : "Data Source = " + coreServer + "; Initial Catalog = " + coreDataBase + "; User ID = " + userId + " ; Password =" + passWord + "";
            con = ConnectToDB(connectionString);
            DynamicParameters parameters = new DynamicParameters();
            string parameterName = "@" + facilityColumn + "";
            parameters.Add(parameterName, facilityCode);
            List<DataAccess> TranDBName = con.Query<DataAccess>(queryValue, parameters, commandType: CommandType.Text).ToList();
            return TranDBName;
        }

        private void LoadAppSettingsJson()
        {
            if (GetAppsetting != null)
            {
                try
                {
                    url = GetAppsetting["Connection:URL"].Trim();
                    coreServer = GetAppsetting["Connection:UATCARESERVER"].Trim();
                    coreDataBase = GetAppsetting["Connection:COREDB"].Trim();
                    auth = GetAppsetting["Connection:Auth"].Trim();
                    facilityCode = GetAppsetting["Connection:Facility"].Trim();
                    if (auth.Equals("User"))
                    {
                        userId = GetAppsetting["Connection:UserID"].Trim();
                        passWord = GetAppsetting["Connection:PWD"].Trim();
                    }

                }
                catch (Exception e)
                {
                    throw new NullReferenceException("Mandatory Values missing from AppSettings.json" + e.Message);
                }

            }
        }
        /// <summary>
        /// This method returns the connection string using fetchTranServerDB method according to facility passing by @facility parameter</summary>
        public string GetTranConnectionString(string facilityColumn)
        {
            string connectionString = null;
            LoadAppSettingsJson();
            try
            {
                List<DataAccess> listValue = GetTranServerDB(facilityColumn);
                if (listValue != null)
                {
                    ServerName = listValue[0].ServerName;
                    DatabaseName = listValue[0].DatabaseName;
                    ServerName = url.Contains("rcohub") ? ServerName + ".EXTAPP.LOCAL" : ServerName;
                    connectionString = auth.Equals("Windows") ? "Data Source = " + ServerName + "; Initial Catalog = " + DatabaseName + "; Integrated Security = True" : "Data Source = " + ServerName + "; Initial Catalog = " + DatabaseName + "; User ID = " + userId + " ; Password =" + passWord + "";
                }
            }
            catch (Exception)
            {
                return null;
            }
            return connectionString;
        }
    }
}