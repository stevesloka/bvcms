/* Author: David Carroll
 * Copyright (c) 2008, 2009 Bellevue Baptist Church
 * Licensed under the GNU General Public License (GPL v2)
 * you may not use this code except in compliance with the License.
 * You may obtain a copy of the License at http://bvcms.codeplex.com/license
 */
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using UtilityExtensions;
using System.Web.Caching;
using System.Data.SqlClient;

namespace CmsData
{
    public static partial class DbUtil
    {
        public static bool DatabaseExists(string name)
        {
            return DatabaseExists(Util.GetConnectionString2("master", 3), name);
        }
        public static bool DatabaseExists(string mastercs, string name)
        {
            using (var cn = new SqlConnection(mastercs))
            {
                cn.Open();
                return DatabaseExists(cn, name);
            }
        }

        public static bool DatabaseExists(SqlConnection cn, string name)
        {
            var cmd = new SqlCommand(
                    "SELECT CAST(CASE WHEN EXISTS(SELECT NULL FROM sys.databases WHERE name = '"
                    + name + "') THEN 1 ELSE 0 END AS BIT)", cn);
            return (bool)cmd.ExecuteScalar();
        }

        public enum CheckDatabaseResult
        {
            DatabaseExists,
            DatabaseDoesNotExist,
            ServerNotFound
        }
        public static bool CmsDatabaseExists()
        {
            var exists = (bool?)HttpRuntime.Cache[Util.Host + "-DatabaseExists"];
            if (exists.HasValue)
                return exists.Value;

            var r = CheckDatabaseExists(Util.CmsHost);
            var b = CheckDatabaseResult.DatabaseExists == r;
            HttpRuntime.Cache.Insert(Util.Host + "-DatabaseExists", b, null,
                DateTime.Now.AddSeconds(60), Cache.NoSlidingExpiration);
            return b;
        }

        public static CheckDatabaseResult CheckDatabaseExists(string name, bool nocache = false)
        {
            if (nocache == false)
            {
                var r1 = HttpRuntime.Cache[Util.Host + "-CheckDatabaseResult"];
                if (r1 != null)
                    return (CheckDatabaseResult)r1;
            }

            using (var cn = new SqlConnection(Util.GetConnectionString2("master", 3)))
            {
                CheckDatabaseResult ret;
                try
                {
                    cn.Open();
                    var b = DatabaseExists(cn, name);
                    ret = b ? CheckDatabaseResult.DatabaseExists : CheckDatabaseResult.DatabaseDoesNotExist;
                }
                catch (Exception ex)
                {
                    if (ex.Message.StartsWith("A network-related"))
                        ret = CheckDatabaseResult.ServerNotFound;
                    else
                        throw;
                }
                if (nocache == false)
                {
                    HttpRuntime.Cache.Insert(Util.Host + "-CheckDatabaseResult", ret, null,
                        ret == CheckDatabaseResult.DatabaseExists
                            ? DateTime.Now.AddSeconds(60)
                            : DateTime.Now.AddSeconds(5), Cache.NoSlidingExpiration);
                }
                return ret;
            }
        }
        public static string CreateDatabase()
        {
            var server = HttpContext.Current.Server;
            var path = server.MapPath("/");
            var sqlScriptsPath = path + @"..\SqlScripts\";
            var cs = Util.GetConnectionString2("master");

            var retVal = CreateDatabase(Util.Host, sqlScriptsPath, cs, Util.ConnectionStringImage,
                Util.GetConnectionString2("Elmah"), Util.ConnectionString);

            HttpRuntime.Cache.Remove(Util.Host + "-DatabaseExists");
            HttpRuntime.Cache.Remove(Util.Host + "-CheckDatabaseResult");

            return retVal;
        }

        public static string CreateDatabase(string hostName, string sqlScriptsPath, string masterConnectionString, string imageConnectionString, string elmahConnectionString, string standardConnectionString)
        {
            var currentFile = string.Empty;
            try
            {
                RunScripts(masterConnectionString, "create database CMS_" + hostName);

                using (var cn = new SqlConnection(masterConnectionString))
                {
                    cn.Open();
                    if (!DatabaseExists(cn, "CMSi_" + hostName))
                    {
                        RunScripts(masterConnectionString, "create database CMSi_" + hostName);
                        currentFile = "BuildImageDatabase.sql";
                        RunScripts(imageConnectionString,
                            File.ReadAllText(sqlScriptsPath + currentFile));
                    }

                    if (!DatabaseExists(cn, "Elmah"))
                    {
                        RunScripts(masterConnectionString, "create database Elmah");
                        currentFile = "BuildElmahDb.sql";
                        RunScripts(elmahConnectionString,
                            File.ReadAllText(sqlScriptsPath + currentFile));
                    }
                }

                using (var cn = new SqlConnection(standardConnectionString))
                {
                    cn.Open();
                    var list = File.ReadAllLines(sqlScriptsPath + "allscripts.txt");
                    foreach (var f in list)
                    {
                        currentFile = f;
                        var script = File.ReadAllText(sqlScriptsPath + @"BuildDb\" + currentFile);
                        RunScripts(cn, script);
                    }
                    currentFile = hostName == "testdb"
                        ? "datascriptTest.sql"
                        : "datascriptStarter.sql";
                    var datascript = File.ReadAllText(sqlScriptsPath + currentFile);
                    RunScripts(cn, datascript);
                }
            }
            catch (Exception ex)
            {
                return $"Error in {currentFile}:\n{ex.Message}";
            }

            return null;
        }

        private static void RunScripts(string cs, string script)
        {
            using (var cn = new SqlConnection(cs))
            {
                cn.Open();
                RunScripts(cn, script);
            }
        }

        private static void RunScripts(SqlConnection cn, string script)
        {
            using (var cmd = new SqlCommand {Connection = cn})
            {
                var scripts = Regex.Split(script, "^GO.*$", RegexOptions.Multiline);
                foreach (var s in scripts)
                {
                    if (s.HasValue())
                    {
                        cmd.CommandText = s;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}
