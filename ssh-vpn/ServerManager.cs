using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Microsoft.Win32;

namespace ssh_vpn
{
    public class ServerInfo
    {
        public string Name { get; set; }
        public string IPAddress { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public static class ServerManager
    {
        private static readonly string ServersRegistryPath = "ssh_vpn_servers";
        private static List<ServerInfo> servers = new List<ServerInfo>();

        public static List<ServerInfo> Servers
        {
            get { return servers; }
            set { servers = value; }
        }

        public static void LoadServers()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(ServersRegistryPath))
                {
                    if (key != null)
                    {
                        string serversXml = key.GetValue("list") as string;
                        if (!string.IsNullOrEmpty(serversXml))
                        {
                            XmlSerializer serializer = new XmlSerializer(typeof(List<ServerInfo>));
                            using (StringReader reader = new StringReader(serversXml))
                            {
                                servers = (List<ServerInfo>)serializer.Deserialize(reader);
                            }
                        }
                    }
                }
            }
            catch
            {
                servers = new List<ServerInfo>();
            }
        }

        public static void SaveServers()
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<ServerInfo>));
                using (StringWriter writer = new StringWriter())
                {
                    serializer.Serialize(writer, servers);
                    string serversXml = writer.ToString();

                    using (RegistryKey key = Registry.CurrentUser.CreateSubKey(ServersRegistryPath))
                    {
                        key.SetValue("list", serversXml);
                    }
                }
            }
            catch
            {
                // Handle error silently
            }
        }

        public static void AddServer(ServerInfo server)
        {
            servers.Add(server);
            SaveServers();
        }

        public static void RemoveServer(int index)
        {
            if (index >= 0 && index < servers.Count)
            {
                servers.RemoveAt(index);
                SaveServers();
            }
        }

        public static void UpdateServer(int index, ServerInfo server)
        {
            if (index >= 0 && index < servers.Count)
            {
                servers[index] = server;
                SaveServers();
            }
        }

        public static ServerInfo GetServer(int index)
        {
            if (index >= 0 && index < servers.Count)
                return servers[index];
            return null;
        }
    }
}