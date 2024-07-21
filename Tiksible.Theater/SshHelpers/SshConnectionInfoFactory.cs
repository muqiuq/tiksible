﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tiksible.Theater.SshHelpers
{
    public static class SshConnectionInfoFactory
    {

        public static ISshConnectionInfo CreateUserNamePasswordConnectionInfo(string hostname, string username,
            string password)
        {
            return new SshConnectionInfoPassword(hostname, username, password);
        }

        public static ISshConnectionInfo CreatePubKeyConnectionInfo(string hostname, string username, string privateKey)
        {
            return new SshConnectionInfoPubKey(hostname, username, privateKey);
        }
    }
}