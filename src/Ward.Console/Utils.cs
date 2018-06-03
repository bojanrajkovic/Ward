using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;

namespace Ward.Console
{
    class Utils
    {
        public static IEnumerable<IPAddress> GetDnsAddresses() => NetworkInterface.GetAllNetworkInterfaces()
                                                                    .Where(
                                                                        ni => ni.OperationalStatus == OperationalStatus.Up
                                                                        && ni.NetworkInterfaceType != NetworkInterfaceType.Loopback
                                                                    )
                                                                    .Select(ni => ni.GetIPProperties())
                                                                    .SelectMany(ipprops => ipprops.DnsAddresses.ToArray())
                                                                    .Where(ip => !ip.IsIPv6SiteLocal);
    }
}
