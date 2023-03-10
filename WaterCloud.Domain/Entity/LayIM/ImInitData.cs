using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaterCloud.Domain.Entity.LayIM
{
    public class ImInitData
    {
        public int code { get; set; }
        public string msg { get; set; }
        public object data { get; set; }

    }

    public class ImUserInfo
    {
        public string username { get; set; }
        public string id { get; set; }
        public string status { get; set; }
        public string avatar { get; set; }
        public string sign { get; set; }

    }

    public class FriendInfo
    {
        public string groupname { get; set; }
        public string id { get; set; }
        public List<ImUserInfo> list { get; set; }

    }

    public class GroupInfo
    {
        public string groupname { get; set; }
        public string id { get; set; }
        public string avatar { get; set; }

    }
}
