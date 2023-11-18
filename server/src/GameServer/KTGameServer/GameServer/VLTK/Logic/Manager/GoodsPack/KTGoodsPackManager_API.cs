using GameServer.KiemThe.Core.Item;
using GameServer.Logic;
using Server.Data;
using System.Collections.Generic;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý vật phẩm rơi
    /// </summary>
    public static partial class KTGoodsPackManager
    {
        #region Tạo vật phẩm rơi ở Map
        #region Private
        /// <summary>
        /// Tạo vật phẩm từ template tương ứng
        /// </summary>
        /// <param name="itemData"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static GoodsData CreateGoodDataFromItemData(ItemData itemData, int count = 1)
        {
            /// Toác
            if (itemData == null)
            {
                return null;
            }

            /// Tạo mới
            GoodsData itemGD = new GoodsData()
            {
                Id = -1,
                GoodsID = itemData.ItemID,
                GCount = count,
                Props = ItemManager.GenerateProbs(itemData),
                Series = ItemManager.GetItemSeries(itemData.Series),
                Site = 0,
                Strong = 100,
                Using = -1,
                BagIndex = -1,
                Binding = 0,
                Forge_level = 0,
            };
            
            /// Trả về kết quả
            return itemGD;
        }
        #endregion

        /// <summary>
        /// Tạo vật phẩm rơi từ vật phẩm tương ứng (mặc định nhóm thằng người chơi sẽ được ưu tiên nhặt)
        /// </summary>
        /// <param name="client"></param>
        /// <param name="itemGD"></param>
        public static void CreateDropMapFromSingleGoods(KPlayer client, GoodsData itemGD)
        {
            /// Nếu thằng người chơi không tồn tại
            if (client == null)
            {
                return;
            }
            /// Nếu vật phẩm không tồn tại
            else if (itemGD == null)
            {
                return;
            }

            /// Danh sách thành viên nhóm
            KPlayer[] teammates = null;
            /// Nếu thằng này không có nhóm
            if (client.TeamID == -1 || !KTTeamManager.IsTeamExist(client.TeamID))
            {
                /// Thành viên chỉ có mình nó
                teammates = new KPlayer[1] { client };
            }
            /// Nếu thằng này có nhóm
            else
            {
                /// Lấy thành viên nhóm của nó
                teammates = client.Teammates.ToArray();
            }

            /// Vị trí rơi
            UnityEngine.Vector2 fallPos = KTGlobal.FindGoodsPackFallPosition(client.CurrentMapCode, client.CurrentCopyMapID, client.PosX, client.PosY);
            /// Tạo vật phẩm rơi
            KGoodsPack goodsPack = KTGoodsPackManager.Create(client.CurrentMapCode, client.CurrentCopyMapID, itemGD, (int) fallPos.x, (int) fallPos.y, teammates);
            /// Toác
            if (goodsPack == null)
            {
                //LogManager.WriteLog(LogTypes.Error, "Create drop from player failed => Player: " + client.RoleName + " (ID: " + client.RoleID + ") - MapCode: " + client.CurrentMapCode + " - CopySceneID: " + client.CurrentCopyMapID);
                return;
            }

            // Nếu thằng này vứt huyền tinh 3 ra đất
            if(ItemManager.IsCrytalDropArlet(itemGD.GoodsID,3))
            {
                string Notify = "[S" + GameManager.ServerLineID + "][" + client.RoleID + "][" + client.RoleName + "][" + client.CurrentMapCode + "][" + client.PosX + "][" + client.PosY + "] Drop Items: " + goodsPack.GoodsData.ItemName + "| ItemID :" + goodsPack.GoodsData.GoodsID + "| IDDB :" + goodsPack.GoodsData.Id;

                Utils.SendTelegram(Notify);
            }    
            /// Ghi Log
            goodsPack.EnableWriteLogOnPickUp = true;
            /// Nguồn rơi
            goodsPack.Source = client;
        }

        /// <summary>
        /// Tạo vật phẩm rơi khi quái vật chết (mặc định nhóm thằng giết sẽ được ưu tiên nhặt)
        /// </summary>
        /// <param name="monster"></param>
        /// <param name="itemGD"></param>
        /// <param name="client"></param>
        public static void CreateDropToMapWhenMonsterDie(Monster monster, GoodsData itemGD, KPlayer client = null)
        {
            /// Nếu quái không tồn tại
            if (monster == null)
            {
                return;
            }
            /// Nếu vật phẩm không tồn tại
            else if (itemGD == null)
            {
                return;
            }

            /// Danh sách thành viên nhóm
            KPlayer[] teammates = null;
            /// Nếu thằng người chơi tồn tại
            if (client != null)
            {
                /// Nếu thằng này không có nhóm
                if (client.TeamID == -1 || !KTTeamManager.IsTeamExist(client.TeamID))
                {
                    /// Thành viên chỉ có mình nó
                    teammates = new KPlayer[1] { client };
                }
                /// Nếu thằng này có nhóm
                else
                {
                    /// Lấy thành viên nhóm của nó
                    teammates = client.Teammates.ToArray();
                }
            }

            /// Vị trí rơi
            UnityEngine.Vector2 fallPos = KTGlobal.FindGoodsPackFallPosition(monster.CurrentMapCode, monster.CurrentCopyMapID, (int) monster.CurrentPos.X, (int) monster.CurrentPos.Y);
            /// Tạo vật phẩm rơi
            KGoodsPack goodsPack = KTGoodsPackManager.Create(monster.CurrentMapCode, monster.CurrentCopyMapID, itemGD, (int) fallPos.x, (int) fallPos.y, teammates);
            /// Toác
            if (goodsPack == null)
            {
                //LogManager.WriteLog(LogTypes.Error, "Create drop from monster failed => Monster: " + monster.RoleName + " - MapCode: " + monster.CurrentMapCode + " - CopySceneID: " + monster.CurrentCopyMapID);
                return;
            }
            /// Ghi Log
            goodsPack.EnableWriteLogOnPickUp = monster.MonsterType == Entities.MonsterAIType.Pirate || monster.MonsterType == Entities.MonsterAIType.Boss || monster.MonsterType == Entities.MonsterAIType.Special_Boss || monster.MonsterType == Entities.MonsterAIType.Elite || monster.MonsterType == Entities.MonsterAIType.Leader;
            /// Nguồn rơi
            goodsPack.Source = monster;
        }

        /// <summary>
        /// Tạo Drop ở MAP
        /// </summary>
        /// <param name="client"></param>
        /// <param name="items"></param>
        /// <param name="monster"></param>
        public static void CreateDropToMap(KPlayer client, List<ItemData> items, Monster monster)
        {
            /// Nếu quái không tồn tại
            if (monster == null)
            {
                return;
            }
            /// Nếu vật phẩm không tồn tại
            else if (items == null)
            {
                return;
            }

            /// Danh sách thành viên nhóm
            KPlayer[] teammates = null;
            /// Nếu thằng người chơi tồn tại
            if (client != null)
            {
                /// Nếu thằng này không có nhóm
                if (client.TeamID == -1 || !KTTeamManager.IsTeamExist(client.TeamID))
                {
                    /// Thành viên chỉ có mình nó
                    teammates = new KPlayer[1] { client };
                }
                /// Nếu thằng này có nhóm
                else
                {
                    /// Lấy thành viên nhóm của nó
                    teammates = client.Teammates.ToArray();
                }
            }

            /// Duyệt danh sách vật phẩm
            foreach (ItemData itemData in items)
            {
                /// Vật phẩm tương ứng
                GoodsData itemGD = KTGoodsPackManager.CreateGoodDataFromItemData(itemData);
                /// Toác
                if (itemGD == null)
                {
                    /// Bỏ qua
                    continue;
                }

                /// Vị trí rơi
                UnityEngine.Vector2 fallPos = KTGlobal.FindGoodsPackFallPosition(monster.CurrentMapCode, monster.CurrentCopyMapID, (int) monster.CurrentPos.X, (int) monster.CurrentPos.Y);
                /// Tạo vật phẩm rơi
                KGoodsPack goodsPack = KTGoodsPackManager.Create(monster.CurrentMapCode, monster.CurrentCopyMapID, itemGD, (int) fallPos.x, (int) fallPos.y, teammates);
                /// Toác
                if (goodsPack == null)
                {
                    return;
                }
                /// Ghi Log
                goodsPack.EnableWriteLogOnPickUp = monster.MonsterType == Entities.MonsterAIType.Pirate || monster.MonsterType == Entities.MonsterAIType.Boss || monster.MonsterType == Entities.MonsterAIType.Special_Boss || monster.MonsterType == Entities.MonsterAIType.Elite || monster.MonsterType == Entities.MonsterAIType.Leader;
                /// Nguồn rơi
                goodsPack.Source = monster;
            }
        }

        /// <summary>
        /// Tạo Drop ở MAP
        /// </summary>
        /// <param name="ListItemDataDrop"></param>
        /// <param name="Monster"></param>
        public static void CreateDropToMap(List<ItemData> items, Monster monster)
        {
            /// Nếu vật phẩm không tồn tại
            if (items == null)
            {
                return;
            }

            /// Duyệt danh sách vật phẩm
            foreach (ItemData itemData in items)
            {
                /// Vật phẩm tương ứng
                GoodsData itemGD = KTGoodsPackManager.CreateGoodDataFromItemData(itemData);
                /// Toác
                if (itemGD == null)
                {
                    /// Bỏ qua
                    continue;
                }

                /// Vị trí rơi
                UnityEngine.Vector2 fallPos = KTGlobal.FindGoodsPackFallPosition(monster.CurrentMapCode, monster.CurrentCopyMapID, (int) monster.CurrentPos.X, (int) monster.CurrentPos.Y);
                /// Tạo vật phẩm rơi
                KGoodsPack goodsPack = KTGoodsPackManager.Create(monster.CurrentMapCode, monster.CurrentCopyMapID, itemGD, (int) fallPos.x, (int) fallPos.y);
                /// Toác
                if (goodsPack == null)
                {
                    return;
                }
                /// Ghi Log
                goodsPack.EnableWriteLogOnPickUp = monster.MonsterType == Entities.MonsterAIType.Pirate || monster.MonsterType == Entities.MonsterAIType.Boss || monster.MonsterType == Entities.MonsterAIType.Special_Boss || monster.MonsterType == Entities.MonsterAIType.Elite || monster.MonsterType == Entities.MonsterAIType.Leader;
                /// Nguồn rơi
                goodsPack.Source = monster;
            }
        }

        /// <summary>
        /// Tạo Drop ở MAP
        /// </summary>
        /// <param name="client"></param>
        /// <param name="items"></param>
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        /// <param name="go"></param>
        public static void CreateDropToMap(KPlayer client, List<KeyValuePair<ItemData, int>> items, int posX, int posY, GameObject go)
        {
            /// Nếu thằng người chơi không tồn tại
            if (client == null)
            {
                return;
            }
            /// Nếu vật phẩm không tồn tại
            else if (items == null)
            {
                return;
            }

            /// Danh sách thành viên nhóm
            KPlayer[] teammates = null;
            /// Nếu thằng người chơi tồn tại
            if (client != null)
            {
                /// Nếu thằng này không có nhóm
                if (client.TeamID == -1 || !KTTeamManager.IsTeamExist(client.TeamID))
                {
                    /// Thành viên chỉ có mình nó
                    teammates = new KPlayer[1] { client };
                }
                /// Nếu thằng này có nhóm
                else
                {
                    /// Lấy thành viên nhóm của nó
                    teammates = client.Teammates.ToArray();
                }
            }

            /// Duyệt danh sách vật phẩm số lượng tương ứng
            foreach (KeyValuePair<ItemData, int> pair in items)
            {
                /// Vật phẩm tương ứng
                GoodsData itemGD = KTGoodsPackManager.CreateGoodDataFromItemData(pair.Key, pair.Value);
                /// Toác
                if (itemGD == null)
                {
                    /// Bỏ qua
                    continue;
                }

                /// Vị trí rơi
                UnityEngine.Vector2 fallPos = KTGlobal.FindGoodsPackFallPosition(client.CurrentMapCode, client.CurrentCopyMapID, posX, posY);
                /// Tạo vật phẩm rơi
                KGoodsPack goodsPack = KTGoodsPackManager.Create(client.CurrentMapCode, client.CurrentCopyMapID, itemGD, (int) fallPos.x, (int) fallPos.y, teammates);
                /// Toác
                if (goodsPack == null)
                {
                    return;
                }
                /// Nếu tồn tại nguồn rơi
                if (go != null)
                {
                    /// Nếu là quái
                    if (go is Monster monster)
                    {
                        /// Ghi Log
                        goodsPack.EnableWriteLogOnPickUp = monster.MonsterType == Entities.MonsterAIType.Pirate || monster.MonsterType == Entities.MonsterAIType.Boss || monster.MonsterType == Entities.MonsterAIType.Special_Boss || monster.MonsterType == Entities.MonsterAIType.Elite || monster.MonsterType == Entities.MonsterAIType.Leader;
                        /// Nguồn rơi
                        goodsPack.Source = monster;
                    }
                    /// Nếu là xe tiêu
                    else if (go is TraderCarriage carriage)
                    {
                        /// Ghi Log
                        goodsPack.EnableWriteLogOnPickUp = true;
                        /// Nguồn rơi
                        goodsPack.Source = carriage;
                    }
                }
            }
        }

        /// <summary>
        /// Tạo Drop ở MAP
        /// </summary>
        /// <param name="mapCode"></param>
        /// <param name="copyMapCode"></param>
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        /// <param name="items"></param>
        public static void CreateDropToMap(int mapCode, int copyMapCode, int posX, int posY, List<ItemData> items)
        {
            /// Nếu vật phẩm không tồn tại
            if (items == null)
            {
                return;
            }

            /// Duyệt danh sách vật phẩm
            foreach (ItemData itemData in items)
            {
                /// Vật phẩm tương ứng
                GoodsData itemGD = KTGoodsPackManager.CreateGoodDataFromItemData(itemData);
                /// Toác
                if (itemGD == null)
                {
                    /// Bỏ qua
                    continue;
                }

                /// Vị trí rơi
                UnityEngine.Vector2 fallPos = KTGlobal.FindGoodsPackFallPosition(mapCode, copyMapCode, posX, posY);
                /// Tạo vật phẩm rơi
                KTGoodsPackManager.Create(mapCode, copyMapCode, itemGD, (int) fallPos.x, (int) fallPos.y);
            }
        }

        /// <summary>
        /// Tạo Drop ở MAP
        /// </summary>
        /// <param name="mapCode"></param>
        /// <param name="copyMapCode"></param>
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        /// <param name="items"></param>
        /// <param name="go"></param>
        public static void CreateDropToMap(int mapCode, int copyMapCode, int posX, int posY, List<KeyValuePair<ItemData, int>> items, GameObject go = null)
        {
            /// Nếu vật phẩm không tồn tại
            if (items == null)
            {
                return;
            }

            /// Duyệt danh sách vật phẩm số lượng tương ứng
            foreach (KeyValuePair<ItemData, int> pair in items)
            {
                /// Vật phẩm tương ứng
                GoodsData itemGD = KTGoodsPackManager.CreateGoodDataFromItemData(pair.Key, pair.Value);
                /// Toác
                if (itemGD == null)
                {
                    /// Bỏ qua
                    continue;
                }

                /// Vị trí rơi
                UnityEngine.Vector2 fallPos = KTGlobal.FindGoodsPackFallPosition(mapCode, copyMapCode, posX, posY);
                /// Tạo vật phẩm rơi
                KGoodsPack goodsPack = KTGoodsPackManager.Create(mapCode, copyMapCode, itemGD, (int) fallPos.x, (int) fallPos.y);
                /// Toác
                if (goodsPack == null)
                {
                    return;
                }
                /// Nếu tồn tại nguồn rơi
                if (go != null)
                {
                    /// Nếu là quái
                    if (go is Monster monster)
                    {
                        /// Ghi Log
                        goodsPack.EnableWriteLogOnPickUp = monster.MonsterType == Entities.MonsterAIType.Pirate || monster.MonsterType == Entities.MonsterAIType.Boss || monster.MonsterType == Entities.MonsterAIType.Special_Boss || monster.MonsterType == Entities.MonsterAIType.Elite || monster.MonsterType == Entities.MonsterAIType.Leader;
                        /// Nguồn rơi
                        goodsPack.Source = monster;
                    }
                    /// Nếu là xe tiêu
                    else if (go is TraderCarriage carriage)
                    {
                        /// Ghi Log
                        goodsPack.EnableWriteLogOnPickUp = true;
                        /// Nguồn rơi
                        goodsPack.Source = carriage;
                    }
                }
            }
        }
        #endregion
    }
}
