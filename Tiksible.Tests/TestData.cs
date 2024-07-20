using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tiksible.Tests
{
    public class TestData
    {

        public static string MissingLineTest1 =
            @"/ip firewall filter add action=accept chain=input comment=""defconf: accept ICMP"" protocol=icmp";

        public static string MissingLineTest2 =
            "/ip address add address=192.168.1.1/24 comment=defconf interface=bridge network=192.168.1.0";

        public static string RscFile1Variant1 = @"/ip firewall filter
add action=accept chain=input comment=\
    ""defconf: accept established,related,untracked"" connection-state=\
    established,related,untracked
add action=accept chain=input comment=""Anti-Lockout Rule"" src-address=\
    192.168.0.0/16
add action=drop chain=input comment=""defconf: drop invalid"" connection-state=\
    invalid
add action=accept chain=input comment=\
    ""defconf: accept to local loopback (for CAPsMAN)"" dst-address=127.0.0.1
add action=drop chain=input comment=""defconf: drop all not coming from LAN"" \
    in-interface-list=!LAN
add action=accept chain=forward comment=""defconf: accept in ipsec policy"" \
    ipsec-policy=in,ipsec
add action=accept chain=forward comment=""defconf: accept out ipsec policy"" \
    ipsec-policy=out,ipsec
add action=fasttrack-connection chain=forward comment=""defconf: fasttrack"" \
    connection-state=established,related hw-offload=yes
add action=accept chain=forward comment=\
    ""defconf: accept established,related, untracked"" connection-state=\
    established,related,untracked
add action=drop chain=forward comment=""defconf: drop invalid"" \
    connection-state=invalid
add action=drop chain=forward comment=\
    ""defconf: drop all from WAN not DSTNATed"" connection-nat-state=!dstnat \
    connection-state=new in-interface-list=WAN
/ip firewall nat
add action=masquerade chain=srcnat comment=""defconf: masquerade"" \
    ipsec-policy=out,none out-interface-list=WAN";


        public static string RscFile1Variant2 = @"/ip firewall filter
add action=accept chain=input comment=\
    ""defconf: accept established,related,untracked"" connection-state=\
    established,related,untracked
add action=accept chain=input comment=""Anti-Lockout Rule"" src-address=\
    192.168.0.0/16
add action=drop chain=input comment=""defconf: drop invalid"" connection-state=\
    invalid
add action=accept chain=input comment=""defconf: accept ICMP"" protocol=icmp
add action=accept chain=input comment=\
    ""defconf: accept to local loopback (for CAPsMAN)"" dst-address=127.0.0.1
add action=drop chain=input comment=""defconf: drop all not coming from LAN"" \
    in-interface-list=!LAN
add action=accept chain=forward comment=""defconf: accept in ipsec policy"" \
    ipsec-policy=in,ipsec
add action=accept chain=forward comment=""defconf: accept out ipsec policy"" \
    ipsec-policy=out,ipsec
add action=fasttrack-connection chain=forward comment=""defconf: fasttrack"" \
    connection-state=established,related hw-offload=yes
add action=accept chain=forward comment=\
    ""defconf: accept established,related, untracked"" connection-state=\
    established,related,untracked
add action=drop chain=forward comment=""defconf: drop invalid"" \
    connection-state=invalid
add action=drop chain=forward comment=\
    ""defconf: drop all from WAN not DSTNATed"" connection-nat-state=!dstnat \
    connection-state=new in-interface-list=WAN
/ip firewall nat
add action=masquerade chain=srcnat comment=""defconf: masquerade"" \
    ipsec-policy=out,none out-interface-list=WAN";

        public static string RscFile2Variant1 = @"/ip address
add address=192.168.88.1/24 comment=defconf interface=bridge network=\
    192.168.88.0
add address=192.168.10.1/24 comment=defconf interface=bridge network=192.168.10.0    ";

        public static string RscFile2Variant2 =
            @"/ip address add address=192.168.1.1/24 comment=defconf interface=bridge network=192.168.1.0
/ip address add address=192.168.10.1/24 comment=defconf interface=bridge network=192.168.10.0
/ip address add address=192.168.88.1/24 comment=defconf interface=bridge network=192.168.88.0";

    public static string ExpectedParseResultOutput =
        @"/ip firewall filter add action=accept chain=input comment=""defconf: accept established,related,untracked"" connection-state=established,related,untracked
/ip firewall filter add action=accept chain=input comment=""Anti-Lockout Rule"" src-address=192.168.0.0/16
/ip firewall filter add action=drop chain=input comment=""defconf: drop invalid"" connection-state=invalid
/ip firewall filter add action=accept chain=input comment=""defconf: accept ICMP"" protocol=icmp
/ip firewall filter add action=accept chain=input comment=""defconf: accept to local loopback (for CAPsMAN)"" dst-address=127.0.0.1
/ip firewall filter add action=drop chain=input comment=""defconf: drop all not coming from LAN"" in-interface-list=!LAN
/ip firewall filter add action=accept chain=forward comment=""defconf: accept in ipsec policy"" ipsec-policy=in,ipsec
/ip firewall filter add action=accept chain=forward comment=""defconf: accept out ipsec policy"" ipsec-policy=out,ipsec
/ip firewall filter add action=fasttrack-connection chain=forward comment=""defconf: fasttrack"" connection-state=established,related hw-offload=yes
/ip firewall filter add action=accept chain=forward comment=""defconf: accept established,related, untracked"" connection-state=established,related,untracked
/ip firewall filter add action=drop chain=forward comment=""defconf: drop invalid"" connection-state=invalid
/ip firewall filter add action=drop chain=forward comment=""defconf: drop all from WAN not DSTNATed"" connection-nat-state=!dstnat connection-state=new in-interface-list=WAN
/ip firewall nat add action=masquerade chain=srcnat comment=""defconf: masquerade"" ipsec-policy=out,none out-interface-list=WAN";
    }
}
