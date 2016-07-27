using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Security
{
    public class BanList
    {
        public class BannedIP
        {
            public string IPAddress = string.Empty;
            public string Reason = string.Empty;
        }

        public class BannedHost
        {
            public string Hostmask = string.Empty;
            public string Reason = string.Empty;
        }

        public List<BannedHost> BannedHosts = new List<BannedHost>();
        public List<BannedIP> BannedIPs = new List<BannedIP>();

        public BannedIP GetIPBan(string ip)
        {
            if (BannedIPs.Count == 0)
                return null;

            foreach(var ban in BannedIPs)
            {
                string subIP = ip.Substring(ban.IPAddress.Length);
                if (subIP == ban.IPAddress) // TODO, this is not good enough need masking and stuff
                    return ban;
            }

            return null;
        }

        public BannedHost GetHostBan(string host)
        {
            if (BannedIPs.Count == 0)
                return null;

            foreach (var ban in BannedHosts)
            {
                string subIP = host.Substring(ban.Hostmask.Length);
                if (subIP == ban.Hostmask) // TODO, this is not good enough need masking and stuff
                    return ban;
            }

            return null;
        }
    }
}
