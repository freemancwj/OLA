using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaterCloud.Service.Hubs
{
    public static class ImHelper
    {
        public static List<SignaRConn> SignaRConns { get; set; } = new List<SignaRConn>();

        public static void AddConnInfo(SignaRConn conn)
        {
            SignaRConns.Add(conn);
        }

        public static void DelConnInfo(string connid)
        {
            var signaRConn = SignaRConns.FirstOrDefault(x => x.ConnectionId == connid);
            if (signaRConn != null)
            {
                SignaRConns.Remove(signaRConn);
            }
        }

        public static void UpdateConnInfo(string userid, string newconnid)
        {
            var signaRConn = SignaRConns.FirstOrDefault(x => x.UserId == userid);
            signaRConn.ConnectionId = newconnid;
        }

        public static bool IsConn(string userid)
        {
            var signaRConn = SignaRConns.FirstOrDefault(x => x.UserId.ToLower() == userid.ToLower());
            if (signaRConn == null)
            {
                return false;
            }
            return true;

        }
    }
}
