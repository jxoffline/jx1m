using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FS.VLTK.Entities.Config
{
    /// <summary>
    /// Dữ liệu quái từ File XML
    /// </summary>
    public class MonsterDataXML
    {
        /// <summary>
        /// ID
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// ID Res
        /// </summary>
        public string ResID { get; set; }

        /// <summary>
        /// Tên quái
        /// </summary>
        public string Name { get; set; }
    }
}
