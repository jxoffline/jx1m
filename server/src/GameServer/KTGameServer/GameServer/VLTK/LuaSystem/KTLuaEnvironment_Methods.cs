using GameServer.Interface;
using GameServer.KiemThe.Core;
using GameServer.KiemThe.Core.Item;
using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Logic;
using GameServer.KiemThe.LuaSystem.Entities;
using GameServer.KiemThe.LuaSystem.Entities.Math;
using GameServer.Logic;
using MoonSharp.Interpreter;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.LuaSystem
{
    /// <summary>
    /// Quản lý tương tác với hệ thống
    /// </summary>
    public static partial class KTLuaEnvironment
    {
        /// <summary>
        /// Đổi dấy khóa của Dict
        /// </summary>
        /// <param name="dict"></param>
        /// <returns></returns>
        private static Dictionary<int, string> ReverseKey(Dictionary<int, string> dict)
        {
            if (dict == null)
            {
                return null;
            }
            return dict.ToDictionary(k => -k.Key, v => v.Value);
        }

        #region NPC Dialog
        /// <summary>
        /// Thực thi Script bắt đầu tương tác với NPC của người chơi
        /// </summary>
        /// <param name="sceneID"></param>
        /// <param name="npcID"></param>
        /// <param name="playerID"></param>
        /// <param name="scriptID"></param>
        /// <param name="otherParams"></param>
        public static void ExecuteNPCScript_Open(GameMap scene, NPC npc, KPlayer player, int scriptID, Dictionary<int, string> otherParams)
        {
            Lua_Scene luaScene = new Lua_Scene()
            {
                RefObject = scene,
            };
            Lua_NPC luaNPC = new Lua_NPC()
            {
                RefObject = npc,
                CurrentScene = luaScene,
            };
            Lua_Player luaPlayer = new Lua_Player()
            {
                RefObject = player,
                CurrentScene = luaScene,
            };

            KTLuaScript.Instance.ExecuteFunctionAsync(KTLuaEnvironment.LuaEnv, scriptID, "OnOpen", new object[] {
                luaScene, luaNPC, luaPlayer, KTLuaEnvironment.ReverseKey(otherParams),
            }, null);
        }

        /// <summary>
        /// Thực thi Script khi có sự lựa chọn từ người chơi của NPC
        /// </summary>
        /// <param name="sceneID"></param>
        /// <param name="npcID"></param>
        /// <param name="playerID"></param>
        /// <param name="scriptID"></param>
        /// <param name="selectionID"></param>
        /// <param name="otherParams"></param>
        public static void ExecuteNPCScript_Selection(GameMap scene, NPC npc, KPlayer player, int scriptID, int selectionID, Dictionary<int, string> otherParams)
        {
            Lua_Scene luaScene = new Lua_Scene()
            {
                RefObject = scene,
            };
            Lua_NPC luaNPC = new Lua_NPC()
            {
                RefObject = npc,
                CurrentScene = luaScene,
            };
            Lua_Player luaPlayer = new Lua_Player()
            {
                RefObject = player,
                CurrentScene = luaScene,
            };

            KTLuaScript.Instance.ExecuteFunctionAsync(KTLuaEnvironment.LuaEnv, scriptID, "OnSelection", new object[] {
                luaScene, luaNPC, luaPlayer, -selectionID, KTLuaEnvironment.ReverseKey(otherParams),
            }, null);
        }

        /// <summary>
        /// Thực thi Script khi có sự lựa chọn vật phẩm từ người chơi của NPC
        /// </summary>
        /// <param name="sceneID"></param>
        /// <param name="npcID"></param>
        /// <param name="playerID"></param>
        /// <param name="scriptID"></param>
        /// <param name="itemInfo"></param>
        /// <param name="otherParams"></param>
        public static void ExecuteNPCScript_ItemSelected(GameMap scene, NPC npc, KPlayer player, int scriptID, DialogItemSelectionInfo itemInfo, Dictionary<int, string> otherParams)
        {
            Lua_Scene luaScene = new Lua_Scene()
            {
                RefObject = scene,
            };
            Lua_NPC luaNPC = new Lua_NPC()
            {
                RefObject = npc,
                CurrentScene = luaScene,
            };
            Lua_Player luaPlayer = new Lua_Player()
            {
                RefObject = player,
                CurrentScene = luaScene,
            };
            Lua_ItemSelectionInfo luaSelectItem = new Lua_ItemSelectionInfo()
            {
                RefObject = itemInfo,
            };

            KTLuaScript.Instance.ExecuteFunctionAsync(KTLuaEnvironment.LuaEnv, scriptID, "OnItemSelected", new object[] {
                luaScene, luaNPC, luaPlayer, luaSelectItem, KTLuaEnvironment.ReverseKey(otherParams),
            }, null);
        }
        #endregion

        #region Monster
        /// <summary>
        /// Thực hiện hàm AITick của script AI quái
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="monster"></param>
        /// <param name="scriptID"></param>
        public static void ExecuteMonsterAIScript_AITick(GameMap scene, Monster monster, GameObject target, int scriptID, Action<UnityEngine.Vector2?> callback)
        {
            Lua_Scene luaScene = new Lua_Scene()
            {
                RefObject = scene,
            };
            Lua_Monster luaMonster = new Lua_Monster()
            {
                RefObject = monster,
                CurrentScene = luaScene,
            };
            Lua_Object luaObject = new Lua_Object()
            {
                RefObject = target,
                CurrentScene = luaScene,
            };

            KTLuaScript.Instance.ExecuteFunctionAsync(KTLuaEnvironment.LuaEnv, scriptID, "AITick", new object[] {
                luaScene, luaMonster, luaObject
            }, (dynValue) => {
                if (dynValue.IsNilOrNan())
                {
                    callback?.Invoke(null);
                }
                else
                {
                    Lua_Vector2 destPos = dynValue.ToObject<Lua_Vector2>();
                    if (destPos== null)
                    {
                        callback?.Invoke(null);
                    }
                    else
                    {
                        callback?.Invoke(destPos.UnityVector2);
                    }
                }
            });
        }

        /// <summary>
        /// Thực hiện hàm Start của quái
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="monster"></param>
        /// <param name="scriptID"></param>
        public static void ExecuteMonsterScript_Start(GameMap scene, Monster monster, int scriptID)
        {
            Lua_Scene luaScene = new Lua_Scene()
            {
                RefObject = scene,
            };
            Lua_Monster luaMonster = new Lua_Monster()
            {
                RefObject = monster,
                CurrentScene = luaScene,
            };

            KTLuaScript.Instance.ExecuteFunctionAsync(KTLuaEnvironment.LuaEnv, scriptID, "Start", new object[] {
                luaScene, luaMonster
            }, null);
        }

        /// <summary>
        /// Thực hiện hàm Tick của quái
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="monster"></param>
        /// <param name="scriptID"></param>
        public static void ExecuteMonsterScript_Tick(GameMap scene, Monster monster, int scriptID)
        {
            Lua_Scene luaScene = new Lua_Scene()
            {
                RefObject = scene,
            };
            Lua_Monster luaMonster = new Lua_Monster()
            {
                RefObject = monster,
                CurrentScene = luaScene,
            };

            KTLuaScript.Instance.ExecuteFunctionAsync(KTLuaEnvironment.LuaEnv, scriptID, "Tick", new object[] {
                luaScene, luaMonster
            }, null);
        }

        /// <summary>
        /// Thực hiện hàm OnDie của quái
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="monster"></param>
        /// <param name="killer"></param>
        /// <param name="scriptID"></param>
        public static void ExecuteMonsterScript_OnDie(GameMap scene, Monster monster, KPlayer killer, int scriptID)
        {
            if (killer == null)
            {
                return;
            }

            Lua_Scene luaScene = new Lua_Scene()
            {
                RefObject = scene,
            };
            Lua_Monster luaMonster = new Lua_Monster()
            {
                RefObject = monster,
                CurrentScene = luaScene,
            };
            Lua_Player luaPlayer = new Lua_Player()
            {
                RefObject = killer,
                CurrentScene = luaScene,
            };

            KTLuaScript.Instance.ExecuteFunctionAsync(KTLuaEnvironment.LuaEnv, scriptID, "OnDie", new object[] {
                luaScene, luaMonster, luaPlayer
            }, null);
        }

        /// <summary>
        /// Thực hiện hàm OnKillObject của quái
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="monster"></param>
        /// <param name="killer"></param>
        /// <param name="scriptID"></param>
        public static void ExecuteMonsterScript_OnKillObject(GameMap scene, Monster monster, KPlayer deadPlayer, int scriptID)
        {
            if (deadPlayer == null)
            {
                return;
            }

            Lua_Scene luaScene = new Lua_Scene()
            {
                RefObject = scene,
            };
            Lua_Monster luaMonster = new Lua_Monster()
            {
                RefObject = monster,
                CurrentScene = luaScene,
            };
            Lua_Player luaPlayer = new Lua_Player()
            {
                RefObject = deadPlayer,
                CurrentScene = luaScene,
            };

            KTLuaScript.Instance.ExecuteFunctionAsync(KTLuaEnvironment.LuaEnv, scriptID, "OnDie", new object[] {
                luaScene, luaMonster, luaPlayer
            }, null);
        }

        /// <summary>
        /// Thực hiện hàm OnHitObject của người quái
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="monster"></param>
        /// <param name="player"></param>
        /// <param name="nDamage"></param>
        /// <param name="scriptID"></param>
        public static void ExecuteMonsterScript_OnHitObject(GameMap scene, Monster monster, KPlayer player, int nDamage, int scriptID)
        {
            if (player == null)
            {
                return;
            }

            Lua_Scene luaScene = new Lua_Scene()
            {
                RefObject = scene,
            };
            Lua_Monster luaMonster = new Lua_Monster()
            {
                RefObject = monster,
                CurrentScene = luaScene,
            };
            Lua_Player luaPlayer = new Lua_Player()
            {
                RefObject = player,
                CurrentScene = luaScene,
            };

            KTLuaScript.Instance.ExecuteFunctionAsync(KTLuaEnvironment.LuaEnv, scriptID, "OnHitObject", new object[] {
                luaScene, luaMonster, luaPlayer, nDamage
            }, null);
        }

        /// <summary>
        /// Thực hiện hàm OnBeHit của người quái
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="monster"></param>
        /// <param name="player"></param>
        /// <param name="nDamage"></param>
        /// <param name="scriptID"></param>
        public static void ExecuteMonsterScript_OnBeHit(GameMap scene, Monster monster, KPlayer player, int nDamage, int scriptID)
        {
            if (player == null)
            {
                return;
            }

            Lua_Scene luaScene = new Lua_Scene()
            {
                RefObject = scene,
            };
            Lua_Player luaPlayer = new Lua_Player()
            {
                RefObject = player,
                CurrentScene = luaScene,
            };
            Lua_Monster luaMonster = new Lua_Monster()
            {
                RefObject = monster,
                CurrentScene = luaScene,
            };

            KTLuaScript.Instance.ExecuteFunctionAsync(KTLuaEnvironment.LuaEnv, scriptID, "OnBeHit", new object[] {
                luaScene, luaMonster, luaPlayer, nDamage
            }, null);
        }
        #endregion

        #region Item
        /// <summary>
        /// Thực thi Script khi có sự lựa chọn từ người chơi của vật phẩm
        /// </summary>
        /// <param name="sceneID"></param>
        /// <param name="itemGD"></param>
        /// <param name="playerID"></param>
        /// <param name="scriptID"></param>
        /// <param name="selectionID"></param>
        /// <param name="otherParams"></param>
        public static void ExecuteItemScript_Selection(GameMap scene, GoodsData itemGD, KPlayer player, int scriptID, int selectionID, Dictionary<int, string> otherParams)
        {
            Lua_Scene luaScene = new Lua_Scene()
            {
                RefObject = scene,
            };
            Lua_Item luaItem = new Lua_Item()
            {
                RefObject = itemGD,
                CurrentScene = luaScene,
            };
            Lua_Player luaPlayer = new Lua_Player()
            {
                RefObject = player,
                CurrentScene = luaScene,
            };

            KTLuaScript.Instance.ExecuteFunctionAsync(KTLuaEnvironment.LuaEnv, scriptID, "OnSelection", new object[] {
                luaScene, luaItem, luaPlayer, -selectionID, KTLuaEnvironment.ReverseKey(otherParams),
            }, null, () => {
                /// Nếu không có vật phẩm
                if (itemGD == null)
                {
                    return false;
                }

                /// Thông tin vật phẩm tương ứng
                GoodsData goodsData = player.GoodsData.Find(itemGD.Id, 0);
                /// Trả về kết quả
                return goodsData != null && goodsData.GCount > 0;
            });
        }

        /// <summary>
        /// Thực thi Script khi có sự lựa chọn vật phẩm từ người chơi của vật phẩm
        /// </summary>
        /// <param name="sceneID"></param>
        /// <param name="itemGD"></param>
        /// <param name="playerID"></param>
        /// <param name="scriptID"></param>
        /// <param name="itemInfo"></param>
        /// <param name="selectionID"></param>
        /// <param name="otherParams"></param>
        public static void ExecuteItemScript_ItemSelected(GameMap scene, GoodsData itemGD, KPlayer player, int scriptID, DialogItemSelectionInfo itemInfo, Dictionary<int, string> otherParams)
        {
            Lua_Scene luaScene = new Lua_Scene()
            {
                RefObject = scene,
            };
            Lua_Item luaItem = new Lua_Item()
            {
                RefObject = itemGD,
                CurrentScene = luaScene,
            };
            Lua_Player luaPlayer = new Lua_Player()
            {
                RefObject = player,
                CurrentScene = luaScene,
            };
            Lua_ItemSelectionInfo luaSelectItem = new Lua_ItemSelectionInfo()
            {
                RefObject = itemInfo,
            };

            KTLuaScript.Instance.ExecuteFunctionAsync(KTLuaEnvironment.LuaEnv, scriptID, "OnItemSelected", new object[] {
                luaScene, luaItem, luaPlayer, luaSelectItem, KTLuaEnvironment.ReverseKey(otherParams),
            }, null, () => {
                /// Nếu không có vật phẩm
                if (itemGD == null)
                {
                    return false;
                }

                /// Thông tin vật phẩm tương ứng
                GoodsData goodsData = player.GoodsData.Find(itemGD.Id, 0);
                /// Trả về kết quả
                return goodsData != null && goodsData.GCount > 0;
            });
        }

        /// <summary>
        /// Thực hiện hàm OnPreCheckCondition của vật phẩm
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="itemGD"></param>
        /// <param name="player"></param>
        /// <param name="scriptID"></param>
        /// <param name="otherParams"></param>
        /// <param name="callback"></param>
        public static void ExecuteItemScript_OnPreCheckCondition(GameMap scene, GoodsData itemGD, KPlayer player, int scriptID, Dictionary<int, string> otherParams, Action<bool> callback)
        {
            Lua_Scene luaScene = new Lua_Scene()
            {
                RefObject = scene,
            };
            Lua_Item luaItem = new Lua_Item()
            {
                RefObject = itemGD,
                CurrentScene = luaScene,
            };
            Lua_Player luaPlayer = new Lua_Player()
            {
                RefObject = player,
                CurrentScene = luaScene,
            };

            KTLuaScript.Instance.ExecuteFunctionAsync(KTLuaEnvironment.LuaEnv, scriptID, "OnPreCheckCondition", new object[] {
                luaScene, luaItem, luaPlayer, KTLuaEnvironment.ReverseKey(otherParams),
            }, (dynValue) => {
                try
                {
                    if (dynValue.Type == MoonSharp.Interpreter.DataType.Boolean)
                    {
                        bool res = dynValue.Boolean;
                        callback?.Invoke(res);
                    }
                }
                catch (Exception) { }
            }, () => {
                /// Nếu không có vật phẩm
                if (itemGD == null)
                {
                    return false;
                }

                /// Thông tin vật phẩm tương ứng
                GoodsData goodsData = player.GoodsData.Find(itemGD.Id, 0);
                /// Trả về kết quả
                return goodsData != null && goodsData.GCount > 0;
            });
        }

        /// <summary>
        /// Thực hiện hàm OnUse của vật phẩm
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="item"></param>
        /// <param name="player"></param>
        /// <param name="scriptID"></param>
        /// <param name="otherParams"></param>
        public static void ExecuteItemScript_OnUse(GameMap scene, GoodsData itemGD, KPlayer player, int scriptID, Dictionary<int, string> otherParams)
        {
            Lua_Scene luaScene = new Lua_Scene()
            {
                RefObject = scene,
            };
            Lua_Item luaItem = new Lua_Item()
            {
                RefObject = itemGD,
                CurrentScene = luaScene,
            };
            Lua_Player luaPlayer = new Lua_Player()
            {
                RefObject = player,
                CurrentScene = luaScene,
            };

            KTLuaScript.Instance.ExecuteFunctionAsync(KTLuaEnvironment.LuaEnv, scriptID, "OnUse", new object[] {
                luaScene, luaItem, luaPlayer, KTLuaEnvironment.ReverseKey(otherParams),
            }, null, () => {
                /// Nếu không có vật phẩm
                if (itemGD == null)
				{
                    return false;
				}

                /// Thông tin vật phẩm tương ứng
                GoodsData goodsData = player.GoodsData.Find(itemGD.Id, 0);
                /// Trả về kết quả
                return goodsData != null && goodsData.GCount > 0;
            });
        }
        #endregion

        #region Grow Point
        /// <summary>
        /// Thực thi Script kiểm tra điều kiện mở điểm thu thập
        /// </summary>
        /// <param name="sceneID"></param>
        /// <param name="npcID"></param>
        /// <param name="playerID"></param>
        /// <param name="scriptID"></param>
        /// <param name="otherParams"></param>
        public static void ExecuteGrowPointScript_OnPreCheckCondition(GameMap scene, GrowPoint growPoint, KPlayer player, int scriptID, Action<bool> callback)
        {
            Lua_Scene luaScene = new Lua_Scene()
            {
                RefObject = scene,
            };
            Lua_GrowPoint luaGrowPoint = new Lua_GrowPoint()
            {
                RefObject = growPoint,
                CurrentScene = luaScene,
            };
            Lua_Player luaPlayer = new Lua_Player()
            {
                RefObject = player,
                CurrentScene = luaScene,
            };

            KTLuaScript.Instance.ExecuteFunctionAsync(KTLuaEnvironment.LuaEnv, scriptID, "OnPreCheckCondition", new object[] {
                luaScene, luaGrowPoint, luaPlayer,
            }, (dynValue) => {
                try
                {
                    if (dynValue.Type == MoonSharp.Interpreter.DataType.Boolean)
                    {
                        bool res = dynValue.Boolean;
                        callback?.Invoke(res);
                    }
                }
                catch (Exception) { }
            });
        }

        /// <summary>
        /// Thực thi Script thi triển liên tục suốt thao tác mở với điểm thu thập
        /// </summary>
        /// <param name="sceneID"></param>
        /// <param name="npcID"></param>
        /// <param name="playerID"></param>
        /// <param name="scriptID"></param>
        public static void ExecuteGrowPointScript_OnActivateEachTick(GameMap scene, GrowPoint growPoint, KPlayer player, int scriptID)
        {
            Lua_Scene luaScene = new Lua_Scene()
            {
                RefObject = scene,
            };
            Lua_GrowPoint luaGrowPoint = new Lua_GrowPoint()
            {
                RefObject = growPoint,
                CurrentScene = luaScene,
            };
            Lua_Player luaPlayer = new Lua_Player()
            {
                RefObject = player,
                CurrentScene = luaScene,
            };

            KTLuaScript.Instance.ExecuteFunctionAsync(KTLuaEnvironment.LuaEnv, scriptID, "OnActivateEachTick", new object[] {
                luaScene, luaGrowPoint, luaPlayer,
            }, null);
        }

        /// <summary>
        /// Thực thi Script hoàn tất mở điểm thu thập
        /// </summary>
        /// <param name="sceneID"></param>
        /// <param name="npcID"></param>
        /// <param name="playerID"></param>
        /// <param name="scriptID"></param>
        public static void ExecuteGrowPointScript_OnComplete(GameMap scene, GrowPoint growPoint, KPlayer player, int scriptID)
        {
            Lua_Scene luaScene = new Lua_Scene()
            {
                RefObject = scene,
            };
            Lua_GrowPoint luaGrowPoint = new Lua_GrowPoint()
            {
                RefObject = growPoint,
                CurrentScene = luaScene,
            };
            Lua_Player luaPlayer = new Lua_Player()
            {
                RefObject = player,
                CurrentScene = luaScene,
            };

            KTLuaScript.Instance.ExecuteFunctionAsync(KTLuaEnvironment.LuaEnv, scriptID, "OnComplete", new object[] {
                luaScene, luaGrowPoint, luaPlayer,
            }, null);
        }

        /// <summary>
        /// Thực thi Script hủy bỏ mở điểm thu thập
        /// </summary>
        /// <param name="sceneID"></param>
        /// <param name="npcID"></param>
        /// <param name="playerID"></param>
        /// <param name="scriptID"></param>
        public static void ExecuteGrowPointScript_OnCancel(GameMap scene, GrowPoint growPoint, KPlayer player, int scriptID)
        {
            Lua_Scene luaScene = new Lua_Scene()
            {
                RefObject = scene,
            };
            Lua_GrowPoint luaGrowPoint = new Lua_GrowPoint()
            {
                RefObject = growPoint,
                CurrentScene = luaScene,
            };
            Lua_Player luaPlayer = new Lua_Player()
            {
                RefObject = player,
                CurrentScene = luaScene,
            };

            KTLuaScript.Instance.ExecuteFunctionAsync(KTLuaEnvironment.LuaEnv, scriptID, "OnCancel", new object[] {
                luaScene, luaGrowPoint, luaPlayer,
            }, null);
        }

        /// <summary>
        /// Thực thi Script thao tác mở điểm thu thập thất bại
        /// </summary>
        /// <param name="sceneID"></param>
        /// <param name="npcID"></param>
        /// <param name="playerID"></param>
        /// <param name="scriptID"></param>
        public static void ExecuteGrowPointScript_OnFaild(GameMap scene, GrowPoint growPoint, KPlayer player, int scriptID)
        {
            Lua_Scene luaScene = new Lua_Scene()
            {
                RefObject = scene,
            };
            Lua_GrowPoint luaGrowPoint = new Lua_GrowPoint()
            {
                RefObject = growPoint,
                CurrentScene = luaScene,
            };
            Lua_Player luaPlayer = new Lua_Player()
            {
                RefObject = player,
                CurrentScene = luaScene,
            };

            KTLuaScript.Instance.ExecuteFunctionAsync(KTLuaEnvironment.LuaEnv, scriptID, "OnFaild", new object[] {
                luaScene, luaGrowPoint, luaPlayer,
            }, null);
        }
        #endregion

        #region Khu vực động
        /// <summary>
        /// Thực thi sự kiện OnEnter
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="go"></param>
        /// <param name="dynArea"></param>
        /// <param name="scriptID"></param>
        public static void ExecuteDynamicAreaScript_OnEnter(GameMap scene, GameObject go, KDynamicArea dynArea, int scriptID)
        {
            Lua_Scene luaScene = new Lua_Scene()
            {
                RefObject = scene,
            };
            Lua_DynamicArea luaDynArea = new Lua_DynamicArea()
            {
                CurrentScene = luaScene,
                RefObject = dynArea,
            };
            if (go is KPlayer)
            {
                Lua_Player luaPlayer = new Lua_Player()
                {
                    RefObject = go as KPlayer,
                    CurrentScene = luaScene,
                };
                KTLuaScript.Instance.ExecuteFunctionAsync(KTLuaEnvironment.LuaEnv, scriptID, "OnEnter", new object[] {
                    luaScene, luaDynArea, luaPlayer,
                }, null);
            }
            else if (go is Monster)
            {
                Lua_Monster luaMonster = new Lua_Monster()
                {
                    RefObject = go as Monster,
                    CurrentScene = luaScene,
                };
                KTLuaScript.Instance.ExecuteFunctionAsync(KTLuaEnvironment.LuaEnv, scriptID, "OnEnter", new object[] {
                    luaScene, luaDynArea, luaMonster,
                }, null);
            }
        }

        /// <summary>
        /// Thực thi sự kiện OnStayTick
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="go"></param>
        /// <param name="dynArea"></param>
        /// <param name="scriptID"></param>
        public static void ExecuteDynamicAreaScript_OnStayTick(GameMap scene, GameObject go, KDynamicArea dynArea, int scriptID)
        {
            Lua_Scene luaScene = new Lua_Scene()
            {
                RefObject = scene,
            };
            Lua_DynamicArea luaDynArea = new Lua_DynamicArea()
            {
                CurrentScene = luaScene,
                RefObject = dynArea,
            };
            if (go is KPlayer)
            {
                Lua_Player luaPlayer = new Lua_Player()
                {
                    RefObject = go as KPlayer,
                    CurrentScene = luaScene,
                };
                KTLuaScript.Instance.ExecuteFunctionAsync(KTLuaEnvironment.LuaEnv, scriptID, "OnStayTick", new object[] {
                    luaScene, luaDynArea, luaPlayer,
                }, null);
            }
            else if (go is Monster)
            {
                Lua_Monster luaMonster = new Lua_Monster()
                {
                    RefObject = go as Monster,
                    CurrentScene = luaScene,
                };
                KTLuaScript.Instance.ExecuteFunctionAsync(KTLuaEnvironment.LuaEnv, scriptID, "OnStayTick", new object[] {
                    luaScene, luaDynArea, luaMonster,
                }, null);
            }
        }

        /// <summary>
        /// Thực thi sự kiện OnLeave
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="go"></param>
        /// <param name="dynArea"></param>
        /// <param name="scriptID"></param>
        public static void ExecuteDynamicAreaScript_OnLeave(GameMap scene, GameObject go, KDynamicArea dynArea, int scriptID)
        {
            Lua_Scene luaScene = new Lua_Scene()
            {
                RefObject = scene,
            };
            Lua_DynamicArea luaDynArea = new Lua_DynamicArea()
            {
                CurrentScene = luaScene,
                RefObject = dynArea,
            };
            if (go is KPlayer)
            {
                Lua_Player luaPlayer = new Lua_Player()
                {
                    RefObject = go as KPlayer,
                    CurrentScene = luaScene,
                };
                KTLuaScript.Instance.ExecuteFunctionAsync(KTLuaEnvironment.LuaEnv, scriptID, "OnLeave", new object[] {
                    luaScene, luaDynArea, luaPlayer,
                }, null);
            }
            else if (go is Monster)
            {
                Lua_Monster luaMonster = new Lua_Monster()
                {
                    RefObject = go as Monster,
                    CurrentScene = luaScene,
                };
                KTLuaScript.Instance.ExecuteFunctionAsync(KTLuaEnvironment.LuaEnv, scriptID, "OnLeave", new object[] {
                    luaScene, luaDynArea, luaMonster,
                }, null);
            }
        }
        #endregion
    }
}
