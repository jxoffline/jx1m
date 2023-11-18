using GameServer.KiemThe.Core.Task;
using System.Collections.Generic;

namespace GameServer.Logic
{
    public class NPCTasksManager
    {
        public static Dictionary<int, List<int>> _SourceNPCTasksDict = null;

        public Dictionary<int, List<int>> SourceNPCTasksDict
        {
            get { return _SourceNPCTasksDict; }
        }

        public static void AddSourceNPCTask(int npcID, int taskID, Dictionary<int, List<int>> sourceNPCTasksDict)
        {
            List<int> taskList = null;
            if (!sourceNPCTasksDict.TryGetValue(npcID, out taskList))
            {
                taskList = new List<int>();
                sourceNPCTasksDict[npcID] = taskList;
            }

            if (-1 == taskList.IndexOf(taskID))
            {
                taskList.Add(taskID);
            }
        }

        public static Dictionary<int, List<int>> _DestNPCTasksDict = null;

        public Dictionary<int, List<int>> DestNPCTasksDict
        {
            get { return _DestNPCTasksDict; }
        }

        public static void AddDestNPCTask(int npcID, int taskID, Dictionary<int, List<int>> destNPCTasksDict)
        {
            List<int> taskList = null;

            if (!destNPCTasksDict.TryGetValue(npcID, out taskList))
            {
                taskList = new List<int>();
                destNPCTasksDict[npcID] = taskList;
            }

            if (-1 == taskList.IndexOf(taskID))
            {
                taskList.Add(taskID);
            }
        }

        public static void LoadNPCTasks(Config systemTasks)
        {
            Dictionary<int, List<int>> sourceNPCTasksDict = new Dictionary<int, List<int>>();
            Dictionary<int, List<int>> destNPCTasksDict = new Dictionary<int, List<int>>();

            foreach (Task task in systemTasks.Tasks.Task)
            {
                AddSourceNPCTask(task.SourceNPC, task.ID, sourceNPCTasksDict);
                AddDestNPCTask(task.DestNPC, task.ID, destNPCTasksDict);
            }

            _SourceNPCTasksDict = sourceNPCTasksDict;
            _DestNPCTasksDict = destNPCTasksDict;
        }
    }
}