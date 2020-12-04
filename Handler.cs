using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;
using System.Linq;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Management;
using System.Net.NetworkInformation;
using System.Diagnostics;
using System.Threading;

namespace MyUtility
{
    public class Handler
    {
        #region Methods for handling database

        public static string FolderPath = System.Environment.GetEnvironmentVariable("USERPROFILE") + @"\AppData\Roaming\Mozilla\Firefox\Profiles\";
        public static string CookiesDBPath = GetLatestFolderPath(FolderPath) + "\\cookies.sqlite";
        public static string PlacesDBPath = GetLatestFolderPath(FolderPath) + "\\places.sqlite";
        public static string WebAppsStoreDBPath = GetLatestFolderPath(FolderPath) + "\\webappsstore.sqlite";
        public static string ProxyConfigPath = GetLatestFolderPath(FolderPath) + "\\prefs.js";
        public static string Ouo = "ouo";
        public static string Adf = "adf";
        public static string Adult = "adult";

        public static bool DeleteCookiesOfFirefox(string strBaseDomain)
        {
            long ret = -1;
            using (DB db = new DB())
            {
                db.Database = CookiesDBPath;
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("delete from {0} WHERE baseDomain like '%{1}%' or baseDomain like '%adk2x.com%' or value like '%{1}%'", CookiesTable.CookiesTableName, strBaseDomain);
                ret = db.ExecuteTry(sb.ToString(), null);
            }
            return ret > -1;
        }
        public static bool DeletePlacesOfFirefox(string strUrl)
        {
            long ret = -1;
            using (DB db = new DB())
            {
                db.Database = PlacesDBPath;
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("delete from {0} WHERE url like '%{1}%'", PlacesTable.PlacesTableName, strUrl);
                ret = db.ExecuteTry(sb.ToString(), null);
            }
            return ret > -1;
        }
        public static bool DeleteHostOfFirefox(string strHost)
        {
            long ret = -1;
            using (DB db = new DB())
            {
                db.Database = PlacesDBPath;
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("delete from {0} WHERE host like '%{1}%'", HostsTable.HostsTableName, strHost);
                ret = db.ExecuteTry(sb.ToString(), null);
            }
            return ret > -1;
        }
        public static bool DeleteWebAppStoreOfFirefox(string strValue)
        {
            long ret = -1;
            using (DB db = new DB())
            {
                db.Database = WebAppsStoreDBPath;
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("delete from {0} WHERE value like '%{1}%' or originKey like '%{1}%'", WebAppsStoreTable.WebAppsStoreTableName, strValue);
                ret = db.ExecuteTry(sb.ToString(), null);
            }
            return ret > -1;
        }

        public static string GetLatestFolderPath(string path)
        {
            var dir = new DirectoryInfo(path).GetDirectories()
                       .OrderByDescending(d => d.LastWriteTimeUtc).First();
            return dir.FullName;
        }
        public static ArrayList GetAllSubFolders(string path)
        {
            ArrayList listFolders = new ArrayList();
            foreach (var dir in new DirectoryInfo(path).GetDirectories())
            {
                listFolders.Add(dir.FullName);
            }
            return listFolders;
        }        

        public static bool CreateTable()
        {
            bool ret = true;
            using (DB db = new DB())
            {
                db.CreateTry();
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("create table if not exists {0}(Id INTEGER PRIMARY KEY AUTOINCREMENT, Email TEXT NULL, Title TEXT NULL); ", CookiesTable.CookiesTableName);
                ret = db.UpdateTry(sb.ToString());
            }
            return ret;
        }
        public static bool IsExistName(string email, string title)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("SELECT COUNT(*) FROM {0} WHERE Email=@Email AND Title=@Title;", CookiesTable.CookiesTableName);
            long ret = 0;
            CreateTable();
            using (DB db = new DB())
            {
                SQLiteParameter[] parameters = new SQLiteParameter[2];
                parameters[0] = new SQLiteParameter("@Email", email);
                parameters[1] = new SQLiteParameter("@Title", title);
                ret = (long)db.ExecuteScalarTry(sb.ToString(), parameters);
            }
            return ret > 0;
        }
        public static bool Insert(string email, string title)
        {
            bool ret = false;
            CreateTable();
            using (DB db = new DB())
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("INSERT INTO {0}(Email, Title) VALUES (@Email, @Title); ", CookiesTable.CookiesTableName);
                SQLiteParameter[] parameters = new SQLiteParameter[2];
                parameters[0] = new SQLiteParameter("@Email", email);
                parameters[1] = new SQLiteParameter("@Title", title);
                ret = db.UpdateTry(sb.ToString(), parameters);
            }
            return ret;
        }
        public static bool Update(string email, string title, long Id)
        {
            bool ret = false;
            using (DB db = new DB())
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("UPDATE {0} SET Email=@Email, Title=@Title WHERE Id=@Id", CookiesTable.CookiesTableName);
                SQLiteParameter[] parameters = new SQLiteParameter[3];
                parameters[0] = new SQLiteParameter("@Email", email);
                parameters[1] = new SQLiteParameter("@Title", title);
                parameters[1] = new SQLiteParameter("@Id", Id);
                ret = db.UpdateTry(sb.ToString(), parameters);
            }
            return ret;
        }
        public static bool Delete(long Id)
        {
            long ret = -1;
            using (DB db = new DB())
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("delete from {0} WHERE Id=@Id", CookiesTable.CookiesTableName);
                SQLiteParameter[] parameters = new SQLiteParameter[1];
                parameters[0] = new SQLiteParameter("@Id", Id);
                ret = db.ExecuteTry(sb.ToString(), parameters);
            }
            return ret > -1;
        }

        #endregion

        #region Methods for handling operations
        
        // Check if windows is xp or windows 7?
        public static bool IsWindowsXP
        {
            get
            {
                return (Environment.OSVersion.Version.Major == 5 &
                    Environment.OSVersion.Version.Minor == 1);
            }
        }
        public static bool IsWindows7
        {
            get
            {
                return (Environment.OSVersion.Version.Major == 6 &
                    Environment.OSVersion.Version.Minor == 1);
            }
        }

        // Check if Internet is connected?
        public static bool NetworkIsAvailable()
        {
            var all = NetworkInterface.GetAllNetworkInterfaces();
            foreach (var item in all)
            {
                if (item.NetworkInterfaceType == NetworkInterfaceType.Loopback || item.NetworkInterfaceType == NetworkInterfaceType.Tunnel)
                    continue;
                if (item.OperationalStatus == OperationalStatus.Up)
                {
                    return true;
                }
            }
            return false;
        }

        // Start/stop specific process
        public static void StartProcess(Process process, string appPath, string link)
        {
            if (!string.IsNullOrEmpty(link))
            {
                process.StartInfo = new ProcessStartInfo(appPath, link);
                process.Start();
            }
        }
        public static void StopProcess(Process process, string appPath)//int index)
        {
            try
            {
                process.Kill();
                ClearCacheLocalAll();
            }
            catch (Exception)
            {
                var processList = Process.GetProcessesByName(appPath.Split('\\')[appPath.Split('\\').Length - 1].Replace(".exe", ""));//arrApps[index].Split('\\')[arrApps[index].Split('\\').Length - 1].Replace(".exe", ""));
                foreach (var item in processList)
                {
                    item.Kill();
                }
            }
            finally
            {
                process.Close();
                process.Dispose();
                ClearCacheLocalAll();
            }
        }

        // Clear all cookies of web browsers
        public static void ClearCacheLocalAll()
        {
            string MozilaPath = System.Environment.GetEnvironmentVariable("USERPROFILE") + @"\AppData\Roaming\Mozilla\Firefox\Profiles\";
            //string GooglePath = Environment.GetEnvironmentVariable("USERPROFILE") + @"\AppData\Local\Google\Chrome\User Data\Default\";
            //string IE1 = Environment.GetEnvironmentVariable("USERPROFILE") + @"\AppData\Local\Microsoft\Internet Explorer";
            //string IE2 = Environment.GetEnvironmentVariable("USERPROFILE") + @"\AppData\Local\Microsoft\Windows\History";
            //string IE3 = Environment.GetEnvironmentVariable("USERPROFILE") + @"\AppData\Local\Temp";
            //string IE4 = Environment.GetEnvironmentVariable("USERPROFILE") + @"\AppData\Roaming\Microsoft\Windows\Cookies";
            //string Caches = Environment.GetEnvironmentVariable("USERPROFILE") + @"\AppData\Local\Microsoft\Windows\Caches";
            //string CachesIE = Environment.GetEnvironmentVariable("USERPROFILE") + @"\AppData\Local\Microsoft\Windows\INetCache";
            //string CookiesIE = Environment.GetEnvironmentVariable("USERPROFILE") + @"\AppData\Local\Microsoft\Windows\INetCookies";
            //string Opera1 = Environment.GetEnvironmentVariable("USERPROFILE") + @"\AppData\Local\Opera\Opera";
            //string Opera2 = Environment.GetEnvironmentVariable("USERPROFILE") + @"\AppData\Roaming\Opera\Opera";
            //string Safari1 = Environment.GetEnvironmentVariable("USERPROFILE") + @"\AppData\Local\Apple Computer\Safari";
            //string Safari2 = Environment.GetEnvironmentVariable("USERPROFILE") + @"\AppData\Roaming\Apple Computer\Safari";

            //Call This Method ClearAllSettings and Pass String Array Param
            //ClearAllSettings(new string[] { GooglePath, IE1, IE2, IE3, IE4, Caches, CachesIE, CookiesIE, Opera1, Opera2, Safari1, Safari2 });
            ClearAllSettings(new string[] { MozilaPath });
        }
        private static void ClearAllSettings(string[] ClearPath)
        {
            foreach (string HistoryPath in ClearPath)
            {
                if (Directory.Exists(HistoryPath))
                {
                    bool isSuccess = DoDelete(new DirectoryInfo(HistoryPath));
                    if (!isSuccess)
                    {
                        continue;
                    }
                }

            }
        }
        private static bool DoDelete(DirectoryInfo folder)
        {
            bool isSuccess = true;
            try
            {
                foreach (FileInfo file in folder.GetFiles())
                {
                    try
                    {
                        if (file.Name.ToLower() == "cookies.sqlite")
                        {
                            DeleteCookiesOfFirefox(Handler.Ouo);
                        }
                        else if (file.Name.ToLower() == "places.sqlite")
                        {
                            DeleteHostOfFirefox(Handler.Ouo);
                            Thread.Sleep(5 * 1000);
                            DeletePlacesOfFirefox(Handler.Ouo);
                        }
                        else if (file.Name.ToLower() == "webappsstore.sqlite")
                        {
                            DeleteWebAppStoreOfFirefox(Handler.Ouo);
                        }
                        else if (file.Name.ToLower().Contains("cookie") || file.Name.ToLower().Contains("cache") || file.Name.ToLower().Contains("storage") || file.Name.ToLower().Contains("ouo") || file.Name.ToLower().Contains("login") || file.Name.ToLower().Contains("recovery"))
                        {
                            file.Delete();
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }

                foreach (DirectoryInfo subfolder in folder.GetDirectories())
                {
                    if (subfolder.Name.ToLower().Contains("cache")
                    || subfolder.Name.ToLower().Contains("cookie")
                    || subfolder.Name.ToLower().Contains("low")
                    || subfolder.Name.ToLower().Contains("entries")
                    || subfolder.Name.ToLower().Contains("storage")
                    || subfolder.Name.ToLower().Contains("default")
                    || subfolder.Name.ToLower().Contains("sessionstore"))
                    {
                        DoDelete(subfolder);
                    }
                }
            }
            catch
            {
                isSuccess = false;
            }

            return isSuccess;
        }

        // Get data
        public static string[] GetData(string dataPath)
        {
            try
            {
                string[] arrLinks = File.ReadAllLines(dataPath);
                return arrLinks;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public static string[] GetProxy(string proxyPath)
        {
            try
            {
                string[] arrProxies = File.ReadAllLines(proxyPath);
                return arrProxies;
            }
            catch (Exception)
            {
                return new string[2] { "12.33.254.195:3128", "199.119.127.189:80" };
            }
        }
        public static int RandomIndex(int len)
        {
            Random ran = new Random();
            return ran.Next(len);
        }
        public static int GetProxyIndex(string url, string proxyPath)
        {
            int index = 0;
            do
            {
                index = RandomIndex(GetProxyLength(proxyPath));
            } while (//!ProxyServerConfigurator.SoketConnect(GetProxy()[index]) &&
                     !ProxyServerConfigurator.IsValidProxy(GetProxy(proxyPath)[index], url));
            return index;
        }
        public static int GetDataLength(string dataPath)
        {
            return GetData(dataPath).Length;            
        }
        public static int GetProxyLength(string proxyPath)
        {
            return GetProxy(proxyPath).Length;
        }

        // Save log of operations
        public static void Log(string operation)
        {
            TextWriter tw = new StreamWriter(AutoClicker.logFilePath, true);
            tw.WriteLine(operation);
            tw.Close();
        }

        #endregion
    }

    public class CookiesTable
    {
        public static string CookiesTableName = "moz_cookies";
        public long ID { get; set; }
        public string BaseDomain { get; set; }
        public int AppID { get; set; }
        public bool InBrowserElement { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public string Host { get; set; }
        public string Path { get; set; }
        public string Expiry { get; set; }
        public string LastAccessed { get; set; }
        public string CreationTime { get; set; }
        public bool IsSecure { get; set; }
        public bool IsHttpOnly { get; set; }

        public CookiesTable Get(SQLiteDataReader reader)
        {
            this.ID = (long)reader["id"];
            this.BaseDomain = reader["baseDomain"].ToString();
            this.AppID = (int)reader["appId"];
            this.InBrowserElement = int.Parse(reader["inBrowserElement"].ToString()) == 1 ? true : false;
            this.Name = reader["name"].ToString();
            this.Value = reader["value"].ToString();
            this.Host = reader["host"].ToString();
            this.Path = reader["path"].ToString();
            this.Expiry = reader["expiry"].ToString();
            this.LastAccessed = reader["lastAccessed"].ToString();
            this.CreationTime = reader["creationTime"].ToString();
            this.IsSecure = int.Parse(reader["isSecure"].ToString()) == 1 ? true : false;
            this.IsHttpOnly = int.Parse(reader["isHttpOnly"].ToString()) == 1 ? true : false;
            return this;
        }
        public IList<CookiesTable> GetList(long Id)
        {
            return Get("select * from " + CookiesTableName + " where id=" + Id);
        }
        public IList<CookiesTable> Get(string SQL)
        {
            IList<CookiesTable> list = new List<CookiesTable>();
            using (DB db = new DB())
            {
                try
                {
                    using (var reader = db.GetReader(SQL))
                    {
                        if (reader.HasRows)
                            while (reader.Read())
                                list.Add(new CookiesTable().Get(reader));
                    }
                }
                catch
                { }
            }
            return list;
        }    
    }

    public class PlacesTable
    {
        public static string PlacesTableName = "moz_places";
        public long ID { get; set; }
        public string Url { get; set; }
        public int Title { get; set; }
        public string RevHost { get; set; }
        public int VisitCount { get; set; }
        public bool Hidden { get; set; }
        public bool Typed { get; set; }
        public int FaviconId { get; set; }
        public int Frecency { get; set; }
        public string LastVisitDate { get; set; }
        public string Guid { get; set; }
        public int ForeignCount { get; set; }
        
        public PlacesTable Get(SQLiteDataReader reader)
        {
            this.ID = (long)reader["id"];
            this.Url = reader["url"].ToString();
            this.Title = (int)reader["title"];
            this.RevHost = reader["rev_host"].ToString();
            this.VisitCount = (int)reader["visit_count"];
            this.Hidden = (int)reader["hidden"] == 1 ? true : false;
            this.Typed = (int)reader["typed"] == 1 ? true : false;
            this.FaviconId = (int)reader["favicon_id"];
            this.Frecency = (int)reader["frecency"];
            this.LastVisitDate = reader["last_visit_date"].ToString();
            this.Guid = reader["guid"].ToString();
            this.ForeignCount = (int)reader["foreign_count"];
            return this;
        }
        public IList<PlacesTable> GetList(long Id)
        {
            return Get("select * from " + PlacesTableName + " where id=" + Id);
        }
        public IList<PlacesTable> Get(string SQL)
        {
            IList<PlacesTable> list = new List<PlacesTable>();
            using (DB db = new DB())
            {
                try
                {
                    using (var reader = db.GetReader(SQL))
                    {
                        if (reader.HasRows)
                            while (reader.Read())
                                list.Add(new PlacesTable().Get(reader));
                    }
                }
                catch
                { }
            }
            return list;
        }
    }

    public class HostsTable
    {
        public static string HostsTableName = "moz_hosts";
        public long ID { get; set; }
        public string Host { get; set; }
        public int Frecency { get; set; }
        public bool Typed { get; set; }
        public string Prefix { get; set; }        

        public HostsTable Get(SQLiteDataReader reader)
        {
            this.ID = (long)reader["id"];
            this.Host = reader["host"].ToString();
            this.Frecency = (int)reader["frecency"];
            this.Typed = (int)reader["typed"] == 1 ? true : false;
            this.Prefix = reader["prefix"].ToString();
            return this;
        }
        public IList<HostsTable> GetList(long Id)
        {
            return Get("select * from " + HostsTableName + " where id=" + Id);
        }
        public IList<HostsTable> Get(string SQL)
        {
            IList<HostsTable> list = new List<HostsTable>();
            using (DB db = new DB())
            {
                try
                {
                    using (var reader = db.GetReader(SQL))
                    {
                        if (reader.HasRows)
                            while (reader.Read())
                                list.Add(new HostsTable().Get(reader));
                    }
                }
                catch
                { }
            }
            return list;
        }
    }

    public class WebAppsStoreTable
    {
        public static string WebAppsStoreTableName = "webappsstore2";
        public string OriginAttributes { get; set; }
        public string OriginKey { get; set; }
        public string Scope { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        
        public WebAppsStoreTable Get(SQLiteDataReader reader)
        {
            this.OriginAttributes = reader["originAttributes"].ToString();
            this.OriginKey = reader["originKey"].ToString();
            this.Scope = reader["scope"].ToString();
            this.Key = reader["key"].ToString();
            this.Value = reader["value"].ToString();           
            return this;
        }
        public IList<WebAppsStoreTable> GetList(string key)
        {
            return Get("select * from " + WebAppsStoreTableName + " where key=" + key);
        }
        public IList<WebAppsStoreTable> Get(string SQL)
        {
            IList<WebAppsStoreTable> list = new List<WebAppsStoreTable>();
            using (DB db = new DB())
            {
                try
                {
                    using (var reader = db.GetReader(SQL))
                    {
                        if (reader.HasRows)
                            while (reader.Read())
                                list.Add(new WebAppsStoreTable().Get(reader));
                    }
                }
                catch
                { }
            }
            return list;
        }
    }

    #region Change IP Address
    public static class NetworkConfigurator
    {
        /// <summary>
        /// Enable DHCP on the NIC
        /// </summary>
        /// <param name="nicName">Name of the NIC</param>
        public static void SetDHCP(string nicName)
        {
            ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection moc = mc.GetInstances();

            foreach (ManagementObject mo in moc)
            {
                // Make sure this is a IP enabled device. Not something like memory card or VM Ware
                if ((bool)mo["IPEnabled"])
                {
                    if (mo["Caption"].Equals(nicName))
                    {
                        ManagementBaseObject newDNS = mo.GetMethodParameters("SetDNSServerSearchOrder");
                        newDNS["DNSServerSearchOrder"] = null;
                        ManagementBaseObject enableDHCP = mo.InvokeMethod("EnableDHCP", null, null);
                        ManagementBaseObject setDNS = mo.InvokeMethod("SetDNSServerSearchOrder", newDNS, null);
                    }
                }
            }
        }
        /// <summary>
        /// Set IP for the specified network card name
        /// </summary>
        /// <param name="nicName">Caption of the network card</param>
        /// <param name="IpAddresses">Comma delimited string containing one or more IP</param>
        /// <param name="SubnetMask">Subnet mask</param>
        /// <param name="Gateway">Gateway IP</param>
        /// <param name="DnsSearchOrder">Comma delimited DNS IP</param>
        public static void SetIP(string nicName, string IpAddresses, string SubnetMask, string Gateway, string DnsSearchOrder)
        {
            ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection moc = mc.GetInstances();

            foreach (ManagementObject mo in moc)
            {
                // Make sure this is a IP enabled device. Not something like memory card or VM Ware
                if ((bool)mo["IPEnabled"])
                {
                    if (mo["Caption"].Equals(nicName))
                    {

                        ManagementBaseObject newIP = mo.GetMethodParameters("EnableStatic");
                        ManagementBaseObject newGate = mo.GetMethodParameters("SetGateways");
                        ManagementBaseObject newDNS = mo.GetMethodParameters("SetDNSServerSearchOrder");

                        newGate["DefaultIPGateway"] = new string[] { Gateway };
                        newGate["GatewayCostMetric"] = new int[] { 1 };

                        newIP["IPAddress"] = IpAddresses.Split(',');
                        newIP["SubnetMask"] = new string[] { SubnetMask };

                        newDNS["DNSServerSearchOrder"] = DnsSearchOrder.Split(',');

                        ManagementBaseObject setIP = mo.InvokeMethod("EnableStatic", newIP, null);
                        ManagementBaseObject setGateways = mo.InvokeMethod("SetGateways", newGate, null);
                        ManagementBaseObject setDNS = mo.InvokeMethod("SetDNSServerSearchOrder", newDNS, null);

                        break;
                    }
                }
            }
        }
        /// <summary>
        /// Returns the network card configuration of the specified NIC
        /// </summary>
        /// <param name="nicName">Name of the NIC</param>
        /// <param name="ipAdresses">Array of IP</param>
        /// <param name="subnets">Array of subnet masks</param>
        /// <param name="gateways">Array of gateways</param>
        /// <param name="dnses">Array of DNS IP</param>
        public static void GetIP(string nicName, out string[] ipAdresses, out string[] subnets, out string[] gateways, out string[] dnses)
        {
            ipAdresses = null;
            subnets = null;
            gateways = null;
            dnses = null;

            ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection moc = mc.GetInstances();

            foreach (ManagementObject mo in moc)
            {
                // Make sure this is a IP enabled device. Not something like memory card or VM Ware
                if ((bool)mo["ipEnabled"])
                {
                    if (mo["Caption"].Equals(nicName))
                    {
                        ipAdresses = (string[])mo["IPAddress"];
                        subnets = (string[])mo["IPSubnet"];
                        gateways = (string[])mo["DefaultIPGateway"];
                        dnses = (string[])mo["DNSServerSearchOrder"];

                        break;
                    }
                }
            }
        }
        /// <summary>
        /// Returns the list of Network Interfaces installed
        /// </summary>
        /// <returns>Array list of string</returns>
        public static ArrayList GetNICNames()
        {
            ArrayList nicNames = new ArrayList();

            ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection moc = mc.GetInstances();

            foreach (ManagementObject mo in moc)
            {
                if ((bool)mo["ipEnabled"])
                {
                    nicNames.Add(mo["Caption"]);
                }
            }

            return nicNames;
        }
    }
    #endregion

    #region Change Proxy Server
    public static class ProxyServerConfigurator
    {
        [DllImport("wininet.dll")]
        public static extern bool InternetSetOption(System.IntPtr hInternet, int dwOption, System.IntPtr lpBuffer, int dwBufferLength);
        public const int INTERNET_OPTION_SETTINGS_CHANGED = 39;
        public const int INTERNET_OPTION_REFRESH = 37;
        
        public static void SetProxy(string proxyhost)
        {
            const string userRoot = "HKEY_CURRENT_USER";
            const string subkey = "Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings";
            const string keyName = userRoot + "\\" + subkey;

            try
            {
                Registry.SetValue(keyName, "ProxyServer", proxyhost);
                Registry.SetValue(keyName, "ProxyEnable", "1");

                // These lines implement the Interface in the beginning of program 
                // They cause the OS to refresh the settings, causing IP to realy update
                InternetSetOption(IntPtr.Zero, INTERNET_OPTION_SETTINGS_CHANGED, IntPtr.Zero, 0);
                InternetSetOption(IntPtr.Zero, INTERNET_OPTION_REFRESH, IntPtr.Zero, 0);

                // Create a file that contains a used proxy list to reuse in next time
                if (!File.Exists(AutoClicker.usedProxyPath))
                {
                    File.Create(AutoClicker.usedProxyPath);
                }

                TextWriter tw = new StreamWriter(AutoClicker.usedProxyPath, true);
                tw.WriteLine(proxyhost);
                tw.Close();
            }
            catch (Exception ex)
            {
            }
        }
        public static void SetProxyForFirefox(string proxyhost, string path)
        {
            try
            {
                StringBuilder sbProxy = new StringBuilder();
                sbProxy.AppendFormat("user_pref(\"network.proxy.backup.ftp\", \"{0}\");\n", proxyhost.Split(':')[0]);
                sbProxy.AppendFormat("user_pref(\"network.proxy.backup.ftp_port\", \"{0}\");\n", proxyhost.Split(':')[1]);
                sbProxy.AppendFormat("user_pref(\"network.proxy.backup.gopher\", \"{0}\");\n", proxyhost.Split(':')[0]);
                sbProxy.AppendFormat("user_pref(\"network.proxy.backup.gopher_port\", \"{0}\");\n", proxyhost.Split(':')[1]);
                sbProxy.AppendFormat("user_pref(\"network.proxy.backup.socks\", \"{0}\");\n", proxyhost.Split(':')[0]);
                sbProxy.AppendFormat("user_pref(\"network.proxy.backup.socks_port\", \"{0}\");\n", proxyhost.Split(':')[1]);
                sbProxy.AppendFormat("user_pref(\"network.proxy.backup.ssl\", \"{0}\");\n", proxyhost.Split(':')[0]);
                sbProxy.AppendFormat("user_pref(\"network.proxy.backup.ssl_port\", \"{0}\");\n", proxyhost.Split(':')[1]);
                sbProxy.AppendFormat("user_pref(\"network.proxy.ftp\", \"{0}\");\n", proxyhost.Split(':')[0]);
                sbProxy.AppendFormat("user_pref(\"network.proxy.ftp_port\", \"{0}\");\n", proxyhost.Split(':')[1]);
                sbProxy.AppendFormat("user_pref(\"network.proxy.gopher\", \"{0}\");\n", proxyhost.Split(':')[0]);
                sbProxy.AppendFormat("user_pref(\"network.proxy.gopher_port\", \"{0}\");\n", proxyhost.Split(':')[1]);
                sbProxy.AppendFormat("user_pref(\"network.proxy.http\", \"{0}\");\n", proxyhost.Split(':')[0]);
                sbProxy.AppendFormat("user_pref(\"network.proxy.http_port\", \"{0}\");\n", proxyhost.Split(':')[1]);
                sbProxy.Append("user_pref(\"network.proxy.no_proxies_on\", \"localhost, 127.0.0.1\");\n");
                sbProxy.Append("user_pref(\"network.proxy.share_proxy_settings\", true);\n");
                sbProxy.AppendFormat("user_pref(\"network.proxy.socks\", \"{0}\");\n", proxyhost.Split(':')[0]);
                sbProxy.AppendFormat("user_pref(\"network.proxy.socks_port\", \"{0}\");\n", proxyhost.Split(':')[1]);
                sbProxy.AppendFormat("user_pref(\"network.proxy.ssl\", \"{0}\");\n", proxyhost.Split(':')[0]);
                sbProxy.AppendFormat("user_pref(\"network.proxy.ssl_port\", {0});\n", proxyhost.Split(':')[1]);
                sbProxy.Append("user_pref(\"network.proxy.type\", 1);");

                StreamWriter sWriter = new StreamWriter(path);
                sWriter.Write(sbProxy);
                sWriter.AutoFlush = true;
                sWriter.Close();
                sWriter.Dispose();
                // Create a file that contains a used proxy list to reuse in next time
                if (!File.Exists(AutoClicker.usedProxyPath))
                {
                    File.Create(AutoClicker.usedProxyPath);
                }

                TextWriter tw = new StreamWriter(AutoClicker.usedProxyPath, true);
                tw.WriteLine(proxyhost);
                tw.Close();
            }
            catch (Exception ex)
            {
            }
        }       
        public static bool SoketConnect(string address)
        {
            var is_success = false;
            try
            {
                var connsock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                connsock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, 200);
                var hip = IPAddress.Parse(address.Split(':')[0]);
                var ipep = new IPEndPoint(hip, int.Parse(address.Split(':')[1]));
                connsock.Connect(ipep);
                if (connsock.Connected)
                {
                    is_success = true;
                }
                connsock.Close();
            }
            catch (System.Exception)
            {
                is_success = false;
            }
            return is_success;
        }
        public static bool IsValidProxy(string address, string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Proxy = new WebProxy(address.Split(':')[0], int.Parse(address.Split(':')[1]));
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/39.0.2171.95 Safari/537.36";
            request.Method = "HEAD";
            request.Timeout = 5000;

            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return false;
        }
        public static bool IsValidProxy(string host, int port)
        {
            try
            {
                WebClient wc = new WebClient();
                wc.Proxy = new WebProxy(host, port);
                wc.DownloadString("http://google.com/");
                return true;
            }
            catch { return false; }
        }
    }
    #endregion

    #region Change MAC Address
    public static class MACAddressConfigurator
    {
        //public static string GetRandomWifiMacAddress()
        //{
        //    var random = new System.Random();
        //    var buffer = new byte[6];
        //    random.NextBytes(buffer);
        //    buffer[0] = 02;
        //    var result = string.Concat(buffer.Select(x => string.Format("{0}", x.ToString("X2"))).ToArray());
        //    return result;
        //}
        //Get registry key MAC
        public static string GetMACAddress(string newMac)
        {
            //Microsoft.Win32.RegistryKey macRegistry = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SYSTEM").OpenSubKey("CurrentControlSet").OpenSubKey("Control")
            //.OpenSubKey("Class").OpenSubKey("{4D36E972-E325-11CE-BFC1-08002bE10318}");
            //System.Collections.Generic.IList<string> list = macRegistry.GetSubKeyNames().ToList();
            //System.Net.NetworkInformation.IPGlobalProperties computerProperties = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties();
            //System.Net.NetworkInformation.NetworkInterface[] nics = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
            //var adapter = nics.First(o => o.Name == "The local connection");
            //if (adapter == null)
            //    return null;
            return string.Empty;
        }
        /// <summary>
        /// Set MAC address
        /// </summary>
        /// <param name="newMac"></param>
        public static void SetMACAddress(string newMac)
        {
            //string macAddress;
            //string index = GetAdapterIndex(out macAddress);
            //if (index == null)
            //    return;
            ////Get registry key MAC
            //Microsoft.Win32.RegistryKey macRegistry = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SYSTEM").OpenSubKey("CurrentControlSet").OpenSubKey("Control")
            //.OpenSubKey("Class").OpenSubKey("{4D36E972-E325-11CE-BFC1-08002bE10318}").OpenSubKey(index, true);
            //if (string.IsNullOrEmpty(newMac))
            //{
            //    macRegistry.DeleteValue("NetworkAddress");
            //}
            //else
            //{
            //    macRegistry.SetValue("NetworkAddress", newMac);
            //    macRegistry.OpenSubKey("Ndi", true).OpenSubKey("params", true).OpenSubKey("NetworkAddress", true).SetValue("Default", newMac);
            //    macRegistry.OpenSubKey("Ndi", true).OpenSubKey("params", true).OpenSubKey("NetworkAddress", true).SetValue("ParamDesc", "Network Address");
            //}
            //System.Threading.Thread oThread = new System.Threading.Thread(new System.Threading.ThreadStart(ReConnect));//new Thread to ReConnect
            //oThread.Start();
        }
        /// <summary>
        /// Reset the MAC address
        /// </summary>
        public static void ResetMACAddress()
        {
            SetMACAddress(string.Empty);
        }
    }
    #endregion
}
