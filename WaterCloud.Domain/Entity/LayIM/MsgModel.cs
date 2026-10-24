using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaterCloud.Domain.LayIM
{
    public class MsgModel
    {
        public string username { get; set; }
        public string avatar { get; set; }

        public string id { get; set; }

        public string type { get; set; }

        public string content { get; set; }

        public int? cid { get; set; }

        public bool mine { get; set; }

        public string fromid { get; set; }

        public long? timestamp { get; set; }

        public string toid { get; set; }

    }


}
