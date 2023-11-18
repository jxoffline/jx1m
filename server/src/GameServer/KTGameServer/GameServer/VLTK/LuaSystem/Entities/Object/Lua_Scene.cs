using GameServer.KiemThe.Core;
using GameServer.KiemThe.Logic;
using GameServer.KiemThe.Logic.Manager;
using GameServer.Logic;
using MoonSharp.Interpreter;
using System.Collections.Generic;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.LuaSystem.Entities
{
    /// <summary>
    /// Đối tượng bản đồ
    /// </summary>
    [MoonSharpUserData]
    public class Lua_Scene
    {
        #region Base for all objects
        /// <summary>
        /// Đối tượng tham chiếu trong hệ thống
        /// </summary>
        public GameMap RefObject { get; set; }

        /// <summary>
        /// Trả về ID bản đồ
        /// </summary>
        /// <returns></returns>
        public int GetID()
        {
            return this.RefObject.MapCode;
        }

        /// <summary>
        /// Trả về tên bản đồ
        /// </summary>
        /// <returns></returns>
        public string GetName()
        {
            return this.RefObject.MapName;
        }
        #endregion

        /// <summary>
        /// Trả về tổng số người chơi trong bản đồ
        /// </summary>
        /// <returns></returns>
        public int GetTotalPlayers()
        {
            return KTPlayerManager.FindAll(this.RefObject.MapCode).Count;
        }

        /// <summary>
        /// Trả về danh sách người chơi trong bản đồ
        /// </summary>
        /// <returns></returns>
        public Lua_Player[] GetPlayers()
        {
            List<Lua_Player> results = new List<Lua_Player>();

            /// Danh sách đối tượng trong bản đồ
            List<KPlayer> objs = KTPlayerManager.FindAll(this.RefObject.MapCode);
            /// Duyệt danh sách
            foreach (KPlayer obj in objs)
            {
                results.Add(new Lua_Player()
                {
                    CurrentScene = this,
                    RefObject = obj,
                });
            }

            /// Trả về kết quả
            return results.ToArray();
        }

        /// <summary>
        /// Trả về tổng số quái trong bản đồ
        /// </summary>
        /// <returns></returns>
        public int GetTotalMonsters()
        {
            return KTMonsterManager.GetTotalMonstersAtMap(this.RefObject.MapCode);
        }

        /// <summary>
        /// Trả về danh sách quái trong bản đồ
        /// </summary>
        /// <returns></returns>
        public Lua_Monster[] GetMonsters()
        {
            List<Lua_Monster> results = new List<Lua_Monster>();

            /// Danh sách đối tượng trong bản đồ
            List<Monster> objs = KTMonsterManager.GetMonstersAtMap(this.RefObject.MapCode);
            /// Duyệt danh sách
            foreach (Monster obj in objs)
            {
                results.Add(new Lua_Monster()
                {
                    CurrentScene = this,
                    RefObject = obj,
                });
            }

            /// Trả về kết quả
            return results.ToArray();
        }

        /// <summary>
        /// Trả về quái có ID tương ứng trong bản đồ
        /// </summary>
        /// <param name="monsterID"></param>
        /// <returns></returns>
        public Lua_Monster GetMonster(int monsterID)
        {
            /// Đối tượng tương ứng
            Monster obj = KTMonsterManager.Find(monsterID);
            /// Nếu tìm thấy
            if (obj != null)
            {
                return new Lua_Monster()
                {
                    CurrentScene = this,
                    RefObject = obj,
                };
            }

            /// Không tìm thấy
            return null;
        }

        /// <summary>
        /// Trả về tổng số NPC trong bản đồ
        /// </summary>
        /// <returns></returns>
        public int GetTotalNPCs()
        {
            return KTNPCManager.GetMapNPCs(this.RefObject.MapCode).Count;
        }

        /// <summary>
        /// Trả về danh sách NPC trong bản đồ
        /// </summary>
        /// <returns></returns>
        public Lua_NPC[] GetNPCs()
        {
            List<Lua_NPC> results = new List<Lua_NPC>();

            /// Danh sách NPC trong bản đồ
            List<NPC> npcs = KTNPCManager.GetMapNPCs(this.RefObject.MapCode);
            /// Duyệt danh sách
            foreach (NPC npc in npcs)
            {
                results.Add(new Lua_NPC()
                {
                    CurrentScene = this,
                    RefObject = npc,
                });
            }

            /// Trả về kết quả
            return results.ToArray();
        }

        /// <summary>
        /// Trả về NPC có ID tương ứng trong bản đồ
        /// </summary>
        /// <param name="npcID"></param>
        /// <returns></returns>
        public Lua_NPC GetNPC(int npcID)
        {
            /// Danh sách NPC trong bản đồ
            List<NPC> npcs = KTNPCManager.GetMapNPCs(this.RefObject.MapCode);
            /// Duyệt danh sách
            foreach (NPC npc in npcs)
            {
                if (npc.NPCID == npcID)
                {
                    return new Lua_NPC()
                    {
                        CurrentScene = this,
                        RefObject = npc,
                    };
                }
            }

            /// Không tìm thấy
            return null;
        }

        /// <summary>
        /// Trả về tổng số điểm thu thập trong bản đồ
        /// </summary>
        /// <returns></returns>
        public int GetTotalGrowPoints()
        {
            return KTGrowPointManager.GetMapGrowPoints(this.RefObject.MapCode).Count;
        }

        /// <summary>
        /// Trả về danh sách điểm thu thập trong bản đồ
        /// </summary>
        /// <returns></returns>
        public Lua_GrowPoint[] GetGrowPoints()
        {
            List<Lua_GrowPoint> results = new List<Lua_GrowPoint>();

            /// Danh sách điểm thu thập trong bản đồ
            List<GrowPoint> growPoints = KTGrowPointManager.GetMapGrowPoints(this.RefObject.MapCode);
            /// Duyệt danh sách
            foreach (GrowPoint growPoint in growPoints)
            {
                results.Add(new Lua_GrowPoint()
                {
                    CurrentScene = this,
                    RefObject = growPoint,
                });
            }

            /// Trả về kết quả
            return results.ToArray();
        }

        /// <summary>
        /// Trả về NPC có ID tương ứng trong bản đồ
        /// </summary>
        /// <param name="growPointID"></param>
        /// <returns></returns>
        public Lua_GrowPoint GetGrowPoint(int growPointID)
        {
            /// Danh sách điểm thu thập trong bản đồ
            List<GrowPoint> growPoints = KTGrowPointManager.GetMapGrowPoints(this.RefObject.MapCode);
            /// Duyệt danh sách
            foreach (GrowPoint growPoint in growPoints)
            {
                if (growPoint.ID == growPointID)
                {
                    return new Lua_GrowPoint()
                    {
                        CurrentScene = this,
                        RefObject = growPoint,
                    };
                }
            }

            /// Không tìm thấy
            return null;
        }

        /// <summary>
        /// Trả về tổng số khu vực động trong bản đồ
        /// </summary>
        /// <returns></returns>
        public int GetTotalDynamicAreas()
        {
            return KTDynamicAreaManager.GetMapDynamicAreas(this.RefObject.MapCode).Count;
        }

        /// <summary>
        /// Trả về danh sách khu vực động trong bản đồ
        /// </summary>
        /// <returns></returns>
        public Lua_DynamicArea[] GetDynamicAreas()
        {
            List<Lua_DynamicArea> results = new List<Lua_DynamicArea>();

            /// Danh sách khu vực động trong bản đồ
            List<KDynamicArea> dynAreas = KTDynamicAreaManager.GetMapDynamicAreas(this.RefObject.MapCode);
            /// Duyệt danh sách
            foreach (KDynamicArea dynArea in dynAreas)
            {
                results.Add(new Lua_DynamicArea()
                {
                    CurrentScene = this,
                    RefObject = dynArea,
                });
            }

            /// Trả về kết quả
            return results.ToArray();
        }

        /// <summary>
        /// Trả về khu vực động có ID tương ứng trong bản đồ
        /// </summary>
        /// <param name="dynAreaID"></param>
        /// <returns></returns>
        public Lua_DynamicArea GetDynamicArea(int dynAreaID)
        {
            /// Danh sách khu vực động trong bản đồ
            List<KDynamicArea> dynAreas = KTDynamicAreaManager.GetMapDynamicAreas(this.RefObject.MapCode);
            /// Duyệt danh sách
            foreach (KDynamicArea dynArea in dynAreas)
            {
                if (dynArea.ID == dynAreaID)
                {
                    return new Lua_DynamicArea()
                    {
                        CurrentScene = this,
                        RefObject = dynArea,
                    };
                }
            }

            /// Không tìm thấy
            return null;
        }
    }
}
