using GameServer.Logic;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.KiemThe.Core.MonsterAIScript
{
    /// <summary>
    /// Interface Monster Script
    /// </summary>
    public interface IMonsterAIScript
    {
        /// <summary>
        /// Thực thi AI
        /// </summary>
        /// <param name="monster"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        UnityEngine.Vector2 Process(Monster monster, GameObject target);

        /// <summary>
        /// Vị trí mặc định
        /// </summary>
        UnityEngine.Vector2 EmptyPos { get; }
    }

    /// <summary>
    /// Quản lý các Script AI
    /// </summary>
    public static class KTMonsterAIScriptManager
    {
        /// <summary>
        /// Danh sách Script AI thực thi
        /// </summary>
        private static Dictionary<int, IMonsterAIScript> MonsterAIScripts = new Dictionary<int, IMonsterAIScript>();

        /// <summary>
        /// Khởi tạo
        /// </summary>
        public static void Init()
        {
            /// Script mặc định
            KTMonsterAIScriptManager.MonsterAIScripts[100000] = new KTMonsterAI_ScriptDefault();
            /// Script dùng kỹ năng tùy chọn
            KTMonsterAIScriptManager.MonsterAIScripts[100001] = new KTMonsterAI_ScriptCustomSkills();
        }

        /// <summary>
        /// Kiểm tra trong danh sách hệ thống có AIScript ID tương ứng không
        /// </summary>
        /// <param name="aiScriptID"></param>
        /// <returns></returns>
        public static bool HasAIScript(int aiScriptID)
        {
            return KTMonsterAIScriptManager.MonsterAIScripts.TryGetValue(aiScriptID, out _);
        }

        /// <summary>
        /// Trả về AIScript quái có ID tương ứng
        /// </summary>
        /// <param name="aiScriptID"></param>
        /// <returns></returns>
        public static IMonsterAIScript GetAIScript(int aiScriptID)
        {
            if (KTMonsterAIScriptManager.MonsterAIScripts.TryGetValue(aiScriptID, out IMonsterAIScript aiScript))
            {
                return aiScript;
            }
            else
            {
                return null;
            }
        }
    }
}
