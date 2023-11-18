using FS.VLTK.Entities.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace FS.VLTK.Loader
{
    /// <summary>
    /// Đối tượng chứa danh sách các cấu hình trong game
    /// </summary>
    public static partial class Loader
    {
        #region Nhiệm vụ
        /// <summary>
        /// Danh sách nhiệm vụ trong hệ thống
        /// </summary>
        public static Dictionary<int, TaskDataXML> Tasks { get; private set; } = new Dictionary<int, TaskDataXML>();

        /// <summary>
        /// Tải xuống danh sách nhiệm vụ
        /// </summary>
        /// <param name="xmlNode"></param>
        public static void LoadTasks(XElement xmlNode)
        {
            Loader.Tasks.Clear();

            foreach (XElement node in xmlNode.Element("Tasks").Elements("Task"))
            {
                TaskDataXML taskDataXML = TaskDataXML.Parse(node);
                Loader.Tasks[taskDataXML.ID] = taskDataXML;
            }
        }
        #endregion
    }
}
