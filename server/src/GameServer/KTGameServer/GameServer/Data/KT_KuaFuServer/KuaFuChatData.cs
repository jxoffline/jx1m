using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Data
{
    public class KuaFuChatData
    {
        public int roleID { get; set; }

        public string roleName { get; set; }

        public int status { get; set; }

        public string toRoleName { get; set; }

        public int index { get; set; }

        public string textMsg { get; set; }

        public int chatType { get; set; }

        public int extTag1 { get; set; }


        public int serverLineID { get; set; }


    }
}
