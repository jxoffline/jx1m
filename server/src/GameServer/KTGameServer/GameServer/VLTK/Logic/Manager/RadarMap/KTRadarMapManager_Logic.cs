using GameServer.Interface;
using GameServer.KiemThe;
using GameServer.KiemThe.Core;
using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Logic;
using Server.Tools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using static GameServer.Logic.KTMapManager;

namespace GameServer.Logic
{
    /// <summary>
    /// Quản lý tầm nhìn của người chơi
    /// </summary>
    public static partial class KTRadarMapManager
    {
        /// <summary>
        /// Trả ra danh sách đối tượng cần được thêm hoặc xóa xung quanh người chơi
        /// </summary>
        /// <param name="client"></param>
        /// <param name="toRemoveObjsList"></param>
        /// <param name="toAddObjsList"></param>
        private static void GetObjectsAroundToAddOrRemove(KPlayer client, out List<IObject> toRemoveObjsList, out List<IObject> toAddObjsList)
        {
            toRemoveObjsList = null;
            toAddObjsList = null;

            try
            {
                /// Bản đồ hiện tại
                GameMap gameMap = KTMapManager.Find(client.MapCode);
                /// Tọa độ lưới
                int centerCridX = client.PosX / gameMap.Grid.MapGridWidth;
                int centerCridY = client.PosY / gameMap.Grid.MapGridHeight;

                /// Danh sách đối tượng xung quanh
                List<IObject> keysList = client.VisibleGrid9Objects.Keys.ToList();
                /// Duyệt danh sách
                foreach (IObject key in keysList)
                {
                    /// Nếu không còn tồn tại nữa
                    if (!client.VisibleGrid9Objects.TryGetValue(key, out byte data))
                    {
                        /// Đánh dấu cần xóa
                        client.VisibleGrid9Objects[key] = 0;
                    }
                    /// Nếu vẫn còn tồn tại
                    else
                    {
                        /// Xử lý
                        client.VisibleGrid9Objects[key] = client.VisibleGrid9Objects[key] == 3 ? (byte) 3 : (byte) 0;
                    }
                }

                /// Danh sách cần thêm vào xóa nhưng không xóa khỏi danh sách hiển thị của đối tượng
                List<IObject> toRemoveAdditionObjsList = new List<IObject>();

                /// Duyệt toàn bộ các ô trong lưới Radar quét
                for (int l = -KTRadarMapManager.RadarHalfWidth; l < KTRadarMapManager.RadarHalfWidth; l++)
                {
                    for (int j = -KTRadarMapManager.RadarHalfHeight; j < KTRadarMapManager.RadarHalfHeight; j++)
                    {
                        /// Vị trí X
                        int x = l + centerCridX;
                        /// Vị trí Y
                        int y = j + centerCridY;
                        /// Nếu hợp lệ
                        if (x >= 0 && y >= 0 && x < gameMap.Grid.MapGridXNum && y < gameMap.Grid.MapGridYNum)
                        {
                            /// Danh sách các đối tượng tại vị trí tương ứng
                            List<IObject> objsList = gameMap.Grid.FindObjects(x, y);
                            /// Không tìm thấy
                            if (objsList == null || objsList.Count <= 0)
                            {
                                /// Bot qua
                                continue;
                            }

                            /// Duyệt toàn bộ danh sách các đối tượng
                            for (int i = 0; i < objsList.Count; i++)
                            {
                                /// Đối tượng tương ứng
                                IObject iObj = objsList[i];

                                /// Nếu không cùng phụ bản
                                if (client.CurrentCopyMapID != iObj.CurrentCopyMapID)
                                {
                                    continue;
                                }

                                /// Nếu đối tượng có trong danh sách xung quanh đã lưu lại
                                if (client.VisibleGrid9Objects.ContainsKey(objsList[i]))
                                {
                                    if (iObj is GameObject)
                                    {
                                        GameObject go = iObj as GameObject;
                                        /// Nếu đối tượng đang trong trạng thái tàng hình, và không hiện thân với người chơi
                                        if (go.IsInvisible() && !go.VisibleTo(client))
                                        {
                                            /// Nếu đối tượng chưa tồn tại trong danh sách tạm giữ nhưng chưa làm gì
                                            if (client.VisibleGrid9Objects[objsList[i]] != 3)
                                            {
                                                /// Xóa đối tượng khỏi danh tạm giữ nhưng chưa làm gì
                                                client.VisibleGrid9Objects[objsList[i]] = 3;
                                                /// Thêm vào danh sách thêm cần xóa
                                                toRemoveAdditionObjsList.Add(objsList[i]);
                                            }
                                            continue;
                                        }
                                    }

                                    /// Nếu đối tượng có trong danh sách tạm giữ nhưng chưa làm gì
                                    if (client.VisibleGrid9Objects[objsList[i]] == 3)
                                    {
                                        /// Thêm vào danh sách cần thêm
                                        client.VisibleGrid9Objects[objsList[i]] = 2;
                                    }
                                    else
                                    {
                                        /// Nếu là người chơi
                                        if (iObj is KPlayer otherPlayer)
                                        {
                                            /// Nếu thằng này vừa chuyển vị trí
                                            if (KTGlobal.GetCurrentTimeMilis() - otherPlayer.LastChangePositionTicks < 200)
                                            {
                                                /// Bỏ qua
                                                continue;
                                            }
                                        }

                                        /// Giữ đối tượng
                                        client.VisibleGrid9Objects[objsList[i]] = 1;
                                    }
                                }
                                /// Nếu đối tượng không có trong danh sách xung quanh đã lưu lại
                                else
                                {
                                    if (iObj is GameObject)
                                    {
                                        GameObject go = iObj as GameObject;
                                        /// Nếu đối tượng đang trong trạng thái tàng hình, và không hiện thân với người chơi
                                        if (go.IsInvisible() && !go.VisibleTo(client))
                                        {
                                            /// Xóa đối tượng khỏi danh sách tạm giữ nhưng chưa làm gì
                                            client.VisibleGrid9Objects[objsList[i]] = 3;
                                            /// Thêm vào danh sách thêm cần xóa
                                            toRemoveAdditionObjsList.Add(objsList[i]);
                                            continue;
                                        }
                                    }

                                    /// Thêm vào danh sách cần thêm
                                    client.VisibleGrid9Objects[objsList[i]] = 2;
                                }
                            }
                        }
                    }
                }

                /// Danh sách cần xóa
                toRemoveObjsList = new List<IObject>();
                /// Danh sách cần thêm
                toAddObjsList = new List<IObject>();
                List<IObject> toRemoveObjsList2 = new List<IObject>();

                /// Duyệt toàn bộ các đối tượng xung quanh bản thân
                List<IObject> keys = client.VisibleGrid9Objects.Keys.ToList();
                foreach (var key in keys)
                {
                    /// Toác
                    if (!client.VisibleGrid9Objects.TryGetValue(key, out byte value))
                    {
                        continue;
                    }

                    /// Nếu cần xóa
                    if (value == 0)
                    {
                        if (key is Monster)
                        {
                            Monster monster = key as Monster;
                            monster.VisibleClientsNum--;
                        }
                        else if (key is KDynamicArea)
                        {
                            KDynamicArea dynArea = key as KDynamicArea;
                            dynArea.VisibleClientsNum--;
                        }
                        else if (key is KDecoBot)
                        {
                            KDecoBot bot = key as KDecoBot;
                            bot.VisibleClientsNum--;
                        }

                        toRemoveObjsList.Add(key);
                    }
                    /// Nếu cần thêm
                    else if (2 == value)
                    {
                        /// Nếu nó Offline rồi thì thôi
                        if (!client.IsOnline())
                        {
                            continue;
                        }

                        if (key is Monster)
                        {
                            Monster monster = key as Monster;
                            if (monster.CurrentCopyMapID == client.CopyMapID)
                            {
                                monster.VisibleClientsNum++;
                                /// Nếu không phải loại quái đặc biệt hoặc NPC di động
                                if (monster.MonsterType != MonsterAIType.DynamicNPC && monster.MonsterType != MonsterAIType.Special_Boss && monster.MonsterType != MonsterAIType.Special_Normal)
                                {
                                    /// Thực thi Timer của quái
                                    KTMonsterTimerManager.Instance.Add(monster);
                                }
                            }
                        }
                        else if (key is KDynamicArea)
                        {
                            KDynamicArea dynArea = key as KDynamicArea;
                            if (dynArea.CurrentCopyMapID == client.CopyMapID)
                            {
                                dynArea.VisibleClientsNum++;
                                /// Thêm khu vực động vào Timer quản lý
                                KTDynamicAreaTimerManager.Instance.Add(dynArea);
                            }
                        }
                        else if (key is KDecoBot)
                        {
                            KDecoBot bot = key as KDecoBot;
                            if (bot.CurrentCopyMapID == client.CopyMapID)
                            {
                                bot.VisibleClientsNum++;
                                /// Thêm BOT vào Timer quản lý
                                KTDecoBotTimerManager.Instance.Add(bot);
                            }
                        }

                        toAddObjsList.Add(key);
                    }
                }

                for (int i = 0; i < toRemoveObjsList.Count; i++)
                {
                    client.VisibleGrid9Objects.TryRemove(toRemoveObjsList[i], out _);
                }

                /// Thêm danh sách cần xóa vào
                if (toRemoveObjsList == null && toRemoveAdditionObjsList.Count > 0)
                {
                    toRemoveObjsList = toRemoveAdditionObjsList;
                }
                else if (toRemoveObjsList != null && toRemoveAdditionObjsList.Count > 0)
                {
                    toRemoveObjsList.AddRange(toRemoveAdditionObjsList);
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.RolePosition, "Radar map scan object failed. Exception: " + ex.ToString());
            }
        }
    }
}
