using GameServer.KiemThe.LuaSystem.Entities.Math;
using GameServer.Logic;
using MoonSharp.Interpreter;
using System.Collections.Generic;

namespace GameServer.KiemThe.LuaSystem.Entities
{
    /// <summary>
    /// Đối tượng quái
    /// </summary>
    [MoonSharpUserData]
    public class Lua_Monster
    {
        #region Base for all objects
        /// <summary>
        /// Đối tượng tham chiếu trong hệ thống
        /// </summary>
        [MoonSharpHidden]
        public Monster RefObject { get; set; }

        /// <summary>
        /// Trả về ID đối tượng
        /// </summary>
        /// <returns></returns>
        public int GetID()
        {
            return this.RefObject.RoleID;
        }

        /// <summary>
        /// Trả về tên đối tượng
        /// </summary>
        /// <returns></returns>
        public string GetName()
        {
            return this.RefObject.RoleName;
        }

        /// <summary>
        /// Trả về cấp độ đối tượng
        /// </summary>
        /// <returns></returns>
        public int GetLevel()
        {
            return this.RefObject.m_Level;
        }
        #endregion

        #region Base for all scene-objects
        /// <summary>
        /// Bản đồ hiện tại
        /// </summary>
        [MoonSharpHidden]
        public Lua_Scene CurrentScene { get; set; }

        /// <summary>
        /// Trả về vị trí của đối tượng
        /// </summary>
        /// <returns></returns>
        public Lua_Vector2 GetPos()
        {
            return new Lua_Vector2((int)this.RefObject.CurrentPos.X, (int)this.RefObject.CurrentPos.Y);
        }

        /// <summary>
        /// Trả về bản đồ hiện tại
        /// </summary>
        /// <returns></returns>
        public Lua_Scene GetScene()
        {
            return this.CurrentScene;
        }

        /// <summary>
        /// Trả về danh hiệu hiện tại
        /// </summary>
        /// <returns></returns>
        public string GetTitle()
        {
            return this.RefObject.Title;
        }

        /// <summary>
        /// Di chuyển đến vị trí chỉ định
        /// </summary>
        /// <param name="toPos"></param>
        public void MoveTo(Lua_Vector2 toPos)
        {
            this.RefObject.MoveTo(new System.Windows.Point(toPos.X, toPos.Y));
        }

        /// <summary>
        /// Trả về loại đối tượng
        /// </summary>
        /// <returns></returns>
        public int GetObjectType()
        {
            return (int) ObjectTypes.OT_MONSTER;
        }
        #endregion

        /// <summary>
        /// Có thể dùng kỹ năng lúc này không
        /// </summary>
        /// <returns></returns>
        public bool CanUseSkillNow()
        {
            return KTGlobal.FinishedUseSkillAction(this.RefObject, this.RefObject.GetCurrentAttackSpeed());
        }

        /// <summary>
        /// Trả về Tag của đối tượng
        /// </summary>
        /// <returns></returns>
        public string GetTag()
        {
            return this.RefObject.Tag;
        }

        /// <summary>
        /// Thiết lập Tag của đối tượng
        /// </summary>
        /// <param name="tag"></param>
        public void SetTag(string tag)
        {
            this.RefObject.Tag = tag;
        }

        /// <summary>
        /// Trả về ngũ hành của đối tượng
        /// </summary>
        /// <returns></returns>
        public int GetElemental()
        {
            return (int)this.RefObject.m_Series;
        }

        /// <summary>
        /// Trả về giá trị biến cục bộ tại vị trí tương ứng
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public long GetLocalVariable(int id)
        {
            return this.RefObject.GetLocalParam(id);
        }

        /// <summary>
        /// Thiết lập giá trị biến cục bộ tại vị trí tương ứng
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        public void SetLocalVariable(int id, long value)
        {
            this.RefObject.SetLocalParam(id, value);
        }



        /// <summary>
        /// Sử dụng kỹ năng với mục tiêu tương ứng
        /// </summary>
        /// <param name="skillID"></param>
        /// <param name="level"></param>
        /// <param name="target"></param>
        public bool UseSkill(int skillID, int level, Lua_Object target)
        {
            return this.RefObject.UseSkill(skillID, level, target.RefObject);
        }

        /// <summary>
        /// Sử dụng kỹ năng tương ứng
        /// </summary>
        /// <param name="skillID"></param>
        /// <param name="level"></param>
        public bool UseSkill(int skillID, int level)
        {
            return this.RefObject.UseSkill(skillID, level, null);
        }

        /// <summary>
        /// Đối tượng còn sống không
        /// </summary>
        /// <returns></returns>
        public bool IsAlive()
        {
            return !this.RefObject.IsDead();
        }
    
        /// <summary>
        /// Thiết lập vị trí cho đối tượng
        /// </summary>
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        /// <returns></returns>
        public bool SetPos(int posX, int posY)
        {
            /// TODO viết thêm vào đây
            return true;
            //GameManager.ClientMgr.ChangePosition2(Global._TCPManager.MySocketListener, Global._TCPManager.TcpOutPacketPool, null, roleID, mapCode, client.CopyMapID, toX, toY, toDirection, null);
        }
    }
}
