using Renci.SshNet;
using System;
using System.Collections.Generic;
using Tiksible.Theater.SshHelpers;

namespace Tiksible.Theater
{
    public class SshConnectionPool : IDisposable
    {
        private readonly record struct PoolKey(string Host, int Port);

        private readonly struct PoolEntry
        {
            public SshClient Ssh { get; init; }
            public SftpClient Sftp { get; init; }
        }

        private readonly Dictionary<PoolKey, PoolEntry> entries = new();
        private bool disposed;

        public (SshClient Ssh, SftpClient Sftp) GetOrOpen(ISshConnectionInfo info)
        {
            var key = new PoolKey(info.HostName, info.SshPort);

            if (entries.TryGetValue(key, out var existing))
            {
                if (!existing.Ssh.IsConnected)
                    existing.Ssh.Connect();
                if (!info.SshOnly && !existing.Sftp.IsConnected)
                    existing.Sftp.Connect();
                return (existing.Ssh, existing.Sftp);
            }

            var connInfo = info.GetConnectionInfo();
            var ssh  = new SshClient(connInfo);
            var sftp = new SftpClient(connInfo);

            ssh.Connect();
            if (!info.SshOnly)
                sftp.Connect();

            var entry = new PoolEntry { Ssh = ssh, Sftp = sftp };
            entries[key] = entry;
            return (ssh, sftp);
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            foreach (var entry in entries.Values)
            {
                if (entry.Ssh.IsConnected)  entry.Ssh.Disconnect();
                if (entry.Sftp.IsConnected) entry.Sftp.Disconnect();
                entry.Ssh.Dispose();
                entry.Sftp.Dispose();
            }
            entries.Clear();
        }
    }
}
