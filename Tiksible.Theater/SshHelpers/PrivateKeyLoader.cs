using System;
using System.IO;
using System.Text;
using Renci.SshNet;

namespace Tiksible.Theater.SshHelpers
{
    internal static class PrivateKeyLoader
    {
        /// <summary>
        /// Loads a private key either directly from its string representation
        /// or from a file path (supporting '~/' for the user's home directory).
        /// </summary>
        public static PrivateKeyFile Load(string privateKey)
        {
            if (privateKey.StartsWith("-----BEGIN OPENSSH PRIVATE KEY-----"))
            {
                return new PrivateKeyFile(new MemoryStream(Encoding.UTF8.GetBytes(privateKey)));
            }

            var privateKeyPath = privateKey.Trim();
            if (privateKeyPath.StartsWith("~/"))
            {
                privateKeyPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    privateKeyPath.Substring(2));
            }

            return new PrivateKeyFile(File.OpenRead(Path.GetFullPath(privateKeyPath)));
        }
    }
}
