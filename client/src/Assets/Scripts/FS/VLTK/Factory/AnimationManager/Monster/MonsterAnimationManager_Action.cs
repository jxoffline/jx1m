using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK.Factory.Animation
{
    /// <summary>
    /// Quản lý động tác quái
    /// </summary>
    public partial class MonsterAnimationManager
    {
        /// <summary>
        /// Lấy tên động tác dựa theo loại
        /// </summary>
        /// <param name="actionType"></param>
        /// <param name="autoFlip"></param>
        /// <returns></returns>
        public string GetActionName(MonsterActionType actionType, bool autoFlip)
        {
            string actionName = "";
            switch (actionType)
            {
                case MonsterActionType.FightStand:
                    actionName = autoFlip ? "Stand" : "NormalStand1";
                    break;
                case MonsterActionType.NormalStand:
                    actionName = autoFlip ? "Stand" : "NormalStand1";
                    break;
                case MonsterActionType.Run:
                    actionName = autoFlip ? "Run" : "NormalWalk";
                    break;
                case MonsterActionType.RunAttack:
                    actionName = autoFlip ? "Run" : "NormalWalk";
                    break;
                case MonsterActionType.Wound:
                    actionName = autoFlip ? "Hurt" : "Wound";
                    break;
                case MonsterActionType.NormalAttack:
                    actionName = autoFlip ? "Attack" : "Attack1";
                    break;
                case MonsterActionType.CritAttack:
                    actionName = autoFlip ? "Attack" : "Attack1";
                    break;
                case MonsterActionType.Die:
                    actionName = "Die";
                    break;
            }
            return actionName;
        }
    }
}
