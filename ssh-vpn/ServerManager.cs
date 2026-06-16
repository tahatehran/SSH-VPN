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
        private static readonly object SyncRoot = new object();
        private static readonly string ServersRegistryPath = "ssh_vpn_servers";
        private static List<ServerInfo> servers = new List<ServerInfo>();

        public static List<ServerInfo> Servers
        {
            get
            {
                lock (SyncRoot)
                {
                    return new List<ServerInfo>(servers);
                }
            }
            set
            {
                lock (SyncRoot)
                {
                    servers = value == null ? new List<ServerInfo>() : new List<ServerInfo>(value);
                }
            }
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
                                object deserialized = serializer.Deserialize(reader);
                                List<ServerInfo> loadedServers = deserialized as List<ServerInfo>;
                                if (loadedServers != null)
                                {
                                    lock (SyncRoot)
                                    {
                                        servers = loadedServers;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                lock (SyncRoot)
                {
                    servers = new List<ServerInfo>();
                }
            }
        }

        public static void SaveServers()
        {
            try
            {
                List<ServerInfo> snapshot;
                lock (SyncRoot)
                {
                    snapshot = new List<ServerInfo>(servers);
                }

                XmlSerializer serializer = new XmlSerializer(typeof(List<ServerInfo>));
                using (StringWriter writer = new StringWriter())
                {
                    serializer.Serialize(writer, snapshot);
                    string serversXml = writer.ToString();

                    using (RegistryKey key = Registry.CurrentUser.CreateSubKey(ServersRegistryPath))
                    {
                        if (key != null)
                            key.SetValue("list", serversXml);
                    }
                }
            }
            catch
            {
                // Save failures should not break the UI.
            }
        }

        public static void AddServer(ServerInfo server)
        {
            if (server == null)
                return;

            lock (SyncRoot)
            {
                servers.Add(server);
            }

            SaveServers();
        }

        public static void RemoveServer(int index)
        {
            lock (SyncRoot)
            {
                if (index >= 0 && index < servers.Count)
                {
                    servers.RemoveAt(index);
                }
                else
                {
                    return;
                }
            }

            SaveServers();
        }

        public static void UpdateServer(int index, ServerInfo server)
        {
            if (server == null)
                return;

            lock (SyncRoot)
            {
                if (index >= 0 && index < servers.Count)
                {
                    servers[index] = server;
                }
                else
                {
                    return;
                }
            }

            SaveServers();
        }

        public static ServerInfo GetServer(int index)
        {
            lock (SyncRoot)
            {
                if (index >= 0 && index < servers.Count)
                    return servers[index];
                return null;
            }
        }

        public static bool IsValidPort(int port)
        {
            return port >= 1 && port <= 65535;
        }
    }
}
