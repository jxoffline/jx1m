using GameServer.Core.Executor;
using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Logic;
using GameServer.Logic;
using GameServer.Server;
using Server.Data;
using Server.Protocol;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.KiemThe.Core.Item
{
    /// <summary>
    /// Class xử lý việc tháo đồ mặc đồ của Player
    /// </summary>
    public class KPlayerEquipBody
    {
        /// <summary>
        /// Lượng máu trước khi Detach
        /// </summary>
        private double hpBeforeDetach = 0;

        /// <summary>
        /// Lượng khí trước khi Detach
        /// </summary>
        private double mpBeforeDetach = 0;

        /// <summary>
        /// Lượng thể lực trước khi Detach
        /// </summary>
        private double staminaBeforeDetach = 0;

        /// <summary>
        /// Thực thể plyer
        /// </summary>
        public KPlayer _Player { get; set; }

        /// <summary>
        /// Dict lưu trữ lại
        /// </summary>
        public List<KMagicAttrib> SuitUpActive = new List<KMagicAttrib>();

        /// <summary>
        /// Set người đang mặc
        /// </summary>
        public Dictionary<KE_EQUIP_POSITION, KItem> EquipBody = new Dictionary<KE_EQUIP_POSITION, KItem>();

        /// <summary>
        /// Set trang bị của PET
        /// </summary>
        public Dictionary<KE_EQUIP_PET_POSTION, KItem> PetEquipBody = new Dictionary<KE_EQUIP_PET_POSTION, KItem>();

        /// <summary>
        /// Set dự phòng
        /// </summary>
        public Dictionary<KE_EQUIP_POSITION_SUB, KItem> PreventiveEquipBody = new Dictionary<KE_EQUIP_POSITION_SUB, KItem>();

        public bool IsHaveEquipBody()
        {
            if (EquipBody.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="player"></param>
        public KPlayerEquipBody(KPlayer player)
        {
            this._Player = player;
        }

        /// <summary>
        ///  Làm giảm độ bền của 1 đồ bất kỳ trên người
        /// </summary>
        /// <param name="nPercent"></param>
        public void AbradeInDeath(int nPercent)
        {
            int MaxBugItem = 4;

            if (nPercent <= 0)
                return;

            if (nPercent >= 100)
                nPercent = 100;
            else
                nPercent = nPercent % 100;

            if (this._Player.m_nCurLucky > 20)
            {
                MaxBugItem = 1;
            }

            for (int i = 0; i < (int)KE_EQUIP_POSITION.emEQUIPPOS_NUM; ++i)
            {
                EquipBody.TryGetValue((KE_EQUIP_POSITION)i, out KItem ItemOut);

                if (ItemOut == null)
                {
                    continue;
                }

                if ((KE_EQUIP_POSITION)i == KE_EQUIP_POSITION.emEQUIPPOS_HORSE || (KE_EQUIP_POSITION)i == KE_EQUIP_POSITION.emEQUIPPOS_MANTLE || (KE_EQUIP_POSITION)i == KE_EQUIP_POSITION.emEQUIPPOS_ORNAMENT || (KE_EQUIP_POSITION)i == KE_EQUIP_POSITION.emEQUIPPOS_SIGNET || (KE_EQUIP_POSITION)i == KE_EQUIP_POSITION.emEQUIPPOS_BOOK || (KE_EQUIP_POSITION)i == KE_EQUIP_POSITION.emEQUIPPOS_CHOP)
                {
                    continue;
                }

                if (ItemOut._GoodDatas.Strong < 50)
                {
                    KTPlayerManager.ShowNotification(this._Player, "Độ bền trang bị [" + ItemOut._ItemData.Name + "] đang quá thấp vui lòng sửa chữa trước khi hỏng trang bị");
                }

                MaxBugItem--;

                if (MaxBugItem > 0)
                {
                    ItemOut.AbradeInDeath(nPercent, this._Player);
                }
            }
        }

        /// <summary>
        /// Hàm swap lại vị trí đồ
        /// </summary>
        public void SwapSet()
        {
            Dictionary<KE_EQUIP_POSITION, KItem> EquipBodyTmp = new Dictionary<KE_EQUIP_POSITION, KItem>();

            Dictionary<KE_EQUIP_POSITION_SUB, KItem> EquipBodySubTmp = new Dictionary<KE_EQUIP_POSITION_SUB, KItem>();

            foreach (KeyValuePair<KE_EQUIP_POSITION_SUB, KItem> entry in PreventiveEquipBody)
            {
                int FinalKey = (int)entry.Key - 100;

                EquipBodyTmp.Add((KE_EQUIP_POSITION)FinalKey, entry.Value);

                UpdatePosition(entry.Value._GoodDatas, FinalKey);
            }

            foreach (KeyValuePair<KE_EQUIP_POSITION, KItem> entry in EquipBody)
            {
                int FinalKey = (int)entry.Key + 100;

                EquipBodySubTmp.Add((KE_EQUIP_POSITION_SUB)FinalKey, entry.Value);

                UpdatePosition(entry.Value._GoodDatas, FinalKey);
            }

            // TODO : UPDATE VÀO DATABASE NẾU THÍCH

            // Xóa bỏ toàn bộ trang bị hiện tại
            ClearAllEffecEquipBody();

            // SET LẠI CHÍNH THÀNH PHỤ VÀ PHỤ THÀNH CHÍNH
            EquipBody = EquipBodyTmp;
            PreventiveEquipBody = EquipBodySubTmp;

            //ATTACK LẠI CHỈ SỐ TRANG BỊ
            AttackAllEquipBody();
        }

        /// <summary>
        /// Hàm update lại vị trí nếu thích dùng
        /// </summary>
        /// <param name="goodsData"></param>
        /// <param name="Postion"></param>
        public void UpdatePosition(GoodsData goodsData, int Postion)
        {
            string[] dbFields = null;

            TCPOutPacketPool pool = Global._TCPManager.TcpOutPacketPool;
            TCPClientPool tcpClientPool = Global._TCPManager.tcpClientPool;

            Dictionary<UPDATEITEM, object> TotalUpdate = new Dictionary<UPDATEITEM, object>();

            TotalUpdate.Add(UPDATEITEM.ROLEID, this._Player.RoleID);
            TotalUpdate.Add(UPDATEITEM.ITEMDBID, goodsData.Id);
            // Update lại vị trí cho vật phẩm hiện tại
            TotalUpdate.Add(UPDATEITEM.BAGINDEX, -1);
            //Set lại giá tri cho việc không sử dụng nữa
            TotalUpdate.Add(UPDATEITEM.USING, Postion);

            string ScriptUpdateBuild = ItemManager.ItemUpdateScriptBuild(TotalUpdate);

            TCPProcessCmdResults dbRequestResult = Global.RequestToDBServer(tcpClientPool, pool, (int)TCPGameServerCmds.CMD_DB_UPDATEGOODS_CMD, ScriptUpdateBuild, out dbFields, this._Player.ServerId);

            if (dbRequestResult == TCPProcessCmdResults.RESULT_FAILED)
            {
            }
            else if (dbRequestResult == TCPProcessCmdResults.RESULT_DATA)
            {
                //KTGlobal.ModifyGoods(this._Player, goodsData.Id, TotalUpdate);

                this._Player.GoodsData.LocalModify(goodsData.Id, TotalUpdate);
                // Gửi packet thứ nhất - Tháo đồ trước đó ra vị trí của đồ chuẩn bị mặc lên

                SCModGoods scData = new SCModGoods()
                {
                    BagIndex = -1,
                    Count = goodsData.GCount,
                    IsUsing = Postion,
                    ModType = (int)ModGoodsTypes.EquipLoad,
                    ID = goodsData.Id,
                    NewHint = 0,
                    Site = 0,
                    State = 0,
                };
                // Gửi thông báo về cho việc mặc đồ thành công
                this._Player.SendPacket((int)TCPGameServerCmds.CMD_SPR_MOD_GOODS, scData);
            }
        }

        /// <summary>
        /// Hàm tháo đồ
        /// </summary>
        public bool EquipUnloadMain(GoodsData goodsData)
        {
            KItem ItemFind = new KItem(goodsData);

            // Nếu là mặc đồ cho PET
            if (ItemFind._ItemData.Genre == 7)
            {
                // Lấy ra vị trí hiện tại của đồ
                int Postion = ItemManager.g_anEquipPetPos[ItemFind.GetBaseItem.DetailType];

                string[] dbFields = null;

                TCPOutPacketPool pool = Global._TCPManager.TcpOutPacketPool;
                TCPClientPool tcpClientPool = Global._TCPManager.tcpClientPool;

                // Check xem có đủ chỗ trống trong túi đồ không
                int BagIndex = this._Player.GoodsData.GetBagFirstEmptyPosition(0);

                if (BagIndex < 0)
                {
                    KTPlayerManager.ShowNotification(this._Player, "Túi đồ không đủ chỗ trống,không thể tháo trang bị");
                    return false;
                }

                // Lấy ra vị trí của con pet đang mặc
                if (PetEquipBody.ContainsKey((KE_EQUIP_PET_POSTION)Postion))
                {
                    // Verify Item xem có đúng vật phẩm không
                    KItem _ItemFromDic = PetEquipBody[(KE_EQUIP_PET_POSTION)Postion];

                    if (ItemFind.ItemDBID != _ItemFromDic.ItemDBID)
                    {
                        KTPlayerManager.ShowNotification(this._Player, "Trang bị muốn tháo bị sai CSDL");

                        LogManager.WriteLog(LogTypes.Item, "[EquipUnload][" + ItemFind.ItemDBID + "][" + ItemFind.GetBaseItem.ItemID + "] : Vật phẩm gửi lên không chính xác! ");

                        return false;
                    }
                    else
                    {
                        Dictionary<UPDATEITEM, object> TotalUpdate = new Dictionary<UPDATEITEM, object>();

                        TotalUpdate.Add(UPDATEITEM.ROLEID, this._Player.RoleID);

                        TotalUpdate.Add(UPDATEITEM.ITEMDBID, _ItemFromDic._GoodDatas.Id);
                        // Update lại vị trí cho vật phẩm hiện tại
                        TotalUpdate.Add(UPDATEITEM.BAGINDEX, BagIndex);
                        //Set lại giá tri cho việc không sử dụng nữa
                        TotalUpdate.Add(UPDATEITEM.USING, -1);

                        string ScriptUpdateBuild = ItemManager.ItemUpdateScriptBuild(TotalUpdate);

                        TCPProcessCmdResults dbRequestResult = Global.RequestToDBServer(tcpClientPool, pool, (int)TCPGameServerCmds.CMD_DB_UPDATEGOODS_CMD, ScriptUpdateBuild, out dbFields, this._Player.ServerId);

                        if (dbRequestResult == TCPProcessCmdResults.RESULT_FAILED)
                        {
                            KTPlayerManager.ShowNotification(this._Player, "Máy chủ bận vui lòng thử lại sau!");

                            LogManager.WriteLog(LogTypes.EquipBody, "Mặc đồ lỗi do không thể tháo đồ cũ ra");

                            return false;
                        }
                        else if (dbRequestResult == TCPProcessCmdResults.RESULT_DATA)
                        {
                            // Gửi packet thứ nhất - Tháo đồ trước đó ra vị trí của đồ chuẩn bị mặc lên

                            // KTGlobal.ModifyGoods(this._Player, _ItemFromDic._GoodDatas.Id, TotalUpdate);

                            this._Player.GoodsData.LocalModify(_ItemFromDic._GoodDatas.Id, TotalUpdate);

                            SCModGoods scData = new SCModGoods()
                            {
                                BagIndex = BagIndex,
                                Count = _ItemFromDic._GoodDatas.GCount,
                                IsUsing = -1,
                                ModType = (int)ModGoodsTypes.EquipUnload,
                                ID = _ItemFromDic._GoodDatas.Id,
                                NewHint = 0,
                                Site = 0,
                                State = 0,
                            };
                            // Gửi thông báo về là tháo đồ ra thành công
                            this._Player.SendPacket((int)TCPGameServerCmds.CMD_SPR_MOD_GOODS, scData);
                        }

                        // Xóa toàn bộ thuộc tính trên người PET
                        this.ClearAllEffecPetEquipBody();

                        /// Gọi thêm clear all effect của vũ khí
                        // THỰC HIỆN REMOVE ITEM KHỎI ARAY
                        lock (PetEquipBody)
                        {
                            PetEquipBody.Remove((KE_EQUIP_PET_POSTION)Postion);
                        }

                        this.AttackAllPetEquipBody();
                    }
                }
                else
                {
                    KTPlayerManager.ShowNotification(this._Player, "Trang bị hiện không tồn trại trên người");
                    return false;
                }
            }
            else
            {
                int Postion = ItemManager.g_anEquipPos[ItemFind.GetBaseItem.DetailType];

                string[] dbFields = null;

                TCPOutPacketPool pool = Global._TCPManager.TcpOutPacketPool;
                TCPClientPool tcpClientPool = Global._TCPManager.tcpClientPool;

                // Check xem có đủ chỗ trống trong túi đồ không
                int BagIndex = this._Player.GoodsData.GetBagFirstEmptyPosition(0);

                if (BagIndex < 0)
                {
                    KTPlayerManager.ShowNotification(this._Player, "Túi đồ không đủ chỗ trống,không thể tháo trang bị");
                    return false;
                }

                // Nếu đây là nhẫn thì thực hiện tìm mở rộng để xác định vị trí nhẫn muốn tháo là vị trí nào
                if (Postion == (int)KE_EQUIP_POSITION.emEQUIPPOS_RING)
                {
                    var FindValue = EquipBody.Where(x => x.Value.ItemDBID == goodsData.Id).FirstOrDefault().Value;

                    if (FindValue != null)
                    {
                        if (FindValue.IsSubRing)
                        {
                            Postion = (int)KE_EQUIP_POSITION.emEQUIPPOS_RING_2;
                        }
                    }
                }
                // Kiểm tra xem có vị trí đấy không
                if (EquipBody.ContainsKey((KE_EQUIP_POSITION)Postion))
                {
                    // Verify Item xem có đúng vật phẩm không
                    KItem _ItemFromDic = EquipBody[(KE_EQUIP_POSITION)Postion];

                    if (ItemFind.ItemDBID != _ItemFromDic.ItemDBID)
                    {
                        KTPlayerManager.ShowNotification(this._Player, "Trang bị muốn tháo bị sai CSDL");

                        LogManager.WriteLog(LogTypes.Item, "[EquipUnload][" + ItemFind.ItemDBID + "][" + ItemFind.GetBaseItem.ItemID + "] : Vật phẩm gửi lên không chính xác! ");

                        return false;
                    }
                    else
                    {
                        Dictionary<UPDATEITEM, object> TotalUpdate = new Dictionary<UPDATEITEM, object>();

                        TotalUpdate.Add(UPDATEITEM.ROLEID, this._Player.RoleID);

                        TotalUpdate.Add(UPDATEITEM.ITEMDBID, _ItemFromDic._GoodDatas.Id);
                        // Update lại vị trí cho vật phẩm hiện tại
                        TotalUpdate.Add(UPDATEITEM.BAGINDEX, BagIndex);
                        //Set lại giá tri cho việc không sử dụng nữa
                        TotalUpdate.Add(UPDATEITEM.USING, -1);

                        string ScriptUpdateBuild = ItemManager.ItemUpdateScriptBuild(TotalUpdate);

                        TCPProcessCmdResults dbRequestResult = Global.RequestToDBServer(tcpClientPool, pool, (int)TCPGameServerCmds.CMD_DB_UPDATEGOODS_CMD, ScriptUpdateBuild, out dbFields, this._Player.ServerId);

                        if (dbRequestResult == TCPProcessCmdResults.RESULT_FAILED)
                        {
                            KTPlayerManager.ShowNotification(this._Player, "Máy chủ bận vui lòng thử lại sau!");

                            LogManager.WriteLog(LogTypes.EquipBody, "Mặc đồ lỗi do không thể tháo đồ cũ ra");

                            return false;
                        }
                        else if (dbRequestResult == TCPProcessCmdResults.RESULT_DATA)
                        {
                            // Gửi packet thứ nhất - Tháo đồ trước đó ra vị trí của đồ chuẩn bị mặc lên

                            // KTGlobal.ModifyGoods(this._Player, _ItemFromDic._GoodDatas.Id, TotalUpdate);

                            this._Player.GoodsData.LocalModify(_ItemFromDic._GoodDatas.Id, TotalUpdate);

                            SCModGoods scData = new SCModGoods()
                            {
                                BagIndex = BagIndex,
                                Count = _ItemFromDic._GoodDatas.GCount,
                                IsUsing = -1,
                                ModType = (int)ModGoodsTypes.EquipUnload,
                                ID = _ItemFromDic._GoodDatas.Id,
                                NewHint = 0,
                                Site = 0,
                                State = 0,
                            };
                            // Gửi thông báo về là tháo đồ ra thành công
                            this._Player.SendPacket((int)TCPGameServerCmds.CMD_SPR_MOD_GOODS, scData);
                        }

                        this.ClearAllEffecEquipBody();

                        /// Gọi thêm clear all effect của vũ khí
                        // THỰC HIỆN REMOVE ITEM KHỎI ARAY
                        lock (EquipBody)
                        {
                            EquipBody.Remove((KE_EQUIP_POSITION)Postion);
                        }

                        /// Nếu là ngựa
                        if (KE_EQUIP_POSITION.emEQUIPPOS_HORSE == (KE_EQUIP_POSITION)Postion)
                        {
                            if (this._Player.IsRiding)
                            {
                                /// Chuyển trạng thái sang xuống ngựa
                                this._Player.IsRiding = false;

                                /// Cập nhật thời gian thay trạng thái cưỡi
                                this._Player.LastTickToggleHorseState = KTGlobal.GetCurrentTimeMilis();

                                /// Lưu vào DB
                                Global.SaveRoleParamsInt32ValueToDB(this._Player, RoleParamName.HorseToggleOn, this._Player.IsRiding ? 1 : 0, true);

                                /// Gửi thông báo tới tất cả người chơi xung quanh
                                KT_TCPHandler.SendHorseStateChanged(this._Player);
                            }
                        }

                        this.AttackAllEquipBody();
                        // Reload lại passible skill
                        //this._Player.ReactivateAuraPassiveAndEnchantSkills();
                    }
                }
                else
                {
                    KTPlayerManager.ShowNotification(this._Player, "Trang bị hiện không tồn trại trên người");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Tháo đồ khỏi slot phụ
        /// </summary>
        /// <param name="goodsData"></param>
        /// <returns></returns>
        public bool EquipUnloadSub(GoodsData goodsData)
        {
            KItem ItemFind = new KItem(goodsData);

            // POSTION = GỐC + 100
            int Postion = ItemManager.g_anEquipSubPos[ItemFind.GetBaseItem.DetailType];

            string[] dbFields = null;

            TCPOutPacketPool pool = Global._TCPManager.TcpOutPacketPool;
            TCPClientPool tcpClientPool = Global._TCPManager.tcpClientPool;

            // Check xem có đủ chỗ trống trong túi đồ không
            int BagIndex = this._Player.GoodsData.GetBagFirstEmptyPosition(0);

            if (BagIndex < 0)
            {
                KTPlayerManager.ShowNotification(this._Player, "Túi đồ không đủ chỗ trống,không thể tháo trang bị");
                return false;
            }

            if (Postion == (int)KE_EQUIP_POSITION_SUB.emEQUIPPOS_RING)
            {
                var FindValue = EquipBody.Where(x => x.Value.ItemDBID == goodsData.Id).FirstOrDefault().Value;

                if (FindValue != null)
                {
                    if (FindValue.IsSubRing)
                    {
                        Postion = (int)KE_EQUIP_POSITION_SUB.emEQUIPPOS_RING_2;
                    }
                }
            }

            // Kiểm tra xem có vị trí đấy không
            if (PreventiveEquipBody.ContainsKey((KE_EQUIP_POSITION_SUB)Postion))
            {
                // Verify Item xem có đúng vật phẩm không
                KItem _ItemFromDic = PreventiveEquipBody[(KE_EQUIP_POSITION_SUB)Postion];

                if (ItemFind.ItemDBID != _ItemFromDic.ItemDBID)
                {
                    KTPlayerManager.ShowNotification(this._Player, "Trang bị muốn tháo bị sai CSDL");

                    LogManager.WriteLog(LogTypes.Item, "[EquipUnload][" + ItemFind.ItemDBID + "][" + ItemFind.GetBaseItem.ItemID + "] : Vật phẩm gửi lên không chính xác! ");

                    return false;
                }
                else
                {
                    Dictionary<UPDATEITEM, object> TotalUpdate = new Dictionary<UPDATEITEM, object>();

                    TotalUpdate.Add(UPDATEITEM.ROLEID, this._Player.RoleID);

                    TotalUpdate.Add(UPDATEITEM.ITEMDBID, _ItemFromDic._GoodDatas.Id);
                    // Update lại vị trí cho vật phẩm hiện tại
                    TotalUpdate.Add(UPDATEITEM.BAGINDEX, BagIndex);
                    //Set lại giá tri cho việc không sử dụng nữa
                    TotalUpdate.Add(UPDATEITEM.USING, -1);

                    string ScriptUpdateBuild = ItemManager.ItemUpdateScriptBuild(TotalUpdate);

                    TCPProcessCmdResults dbRequestResult = Global.RequestToDBServer(tcpClientPool, pool, (int)TCPGameServerCmds.CMD_DB_UPDATEGOODS_CMD, ScriptUpdateBuild, out dbFields, this._Player.ServerId);

                    if (dbRequestResult == TCPProcessCmdResults.RESULT_FAILED)
                    {
                        KTPlayerManager.ShowNotification(this._Player, "Máy chủ bận vui lòng thử lại sau!");

                        LogManager.WriteLog(LogTypes.EquipBody, "Mặc đồ lỗi do không thể tháo đồ cũ ra");

                        return false;
                    }
                    else if (dbRequestResult == TCPProcessCmdResults.RESULT_DATA)
                    {
                        // Gửi packet thứ nhất - Tháo đồ trước đó ra vị trí của đồ chuẩn bị mặc lên

                        //KTGlobal.ModifyGoods(this._Player, _ItemFromDic._GoodDatas.Id, TotalUpdate);

                        this._Player.GoodsData.LocalModify(_ItemFromDic._GoodDatas.Id, TotalUpdate);

                        SCModGoods scData = new SCModGoods()
                        {
                            BagIndex = BagIndex,
                            Count = _ItemFromDic._GoodDatas.GCount,
                            IsUsing = -1,
                            ModType = (int)ModGoodsTypes.EquipUnload,
                            ID = _ItemFromDic._GoodDatas.Id,
                            NewHint = 0,
                            Site = 0,
                            State = 0,
                        };
                        // Gửi thông báo về là tháo đồ ra thành công
                        this._Player.SendPacket((int)TCPGameServerCmds.CMD_SPR_MOD_GOODS, scData);
                    }

                    // Vì là set phụ nên sẽ ko active gì
                    // this.ClearAllEffecEquipBody();
                    // THỰC HIỆN REMOVE ITEM KHỎI ARAY
                    lock (PreventiveEquipBody)
                    {
                        PreventiveEquipBody.Remove((KE_EQUIP_POSITION_SUB)Postion);
                    }

                    /// Nếu là ngựa
                    //if (KE_EQUIP_POSITION.emEQUIPPOS_HORSE == (KE_EQUIP_POSITION)Postion)
                    //{
                    //    if (this._Player.IsRiding)
                    //    {
                    //        /// Chuyển trạng thái sang xuống ngựa
                    //        this._Player.IsRiding = false;

                    //        /// Cập nhật thời gian thay trạng thái cưỡi
                    //        this._Player.LastTickToggleHorseState = KTGlobal.GetCurrentTimeMilis();

                    //        /// Lưu vào DB
                    //        Global.SaveRoleParamsInt32ValueToDB(this._Player, RoleParamName.HorseToggleOn, this._Player.IsRiding ? 1 : 0, true);

                    //        /// Gửi thông báo tới tất cả người chơi xung quanh
                    //        KT_TCPHandler.SendHorseStateChanged(this._Player);
                    //    }

                    //}

                    // VÌ LÀ SET PHỤ LÊN KO KÍCH HOẠT GÌ CHỈ KICSK LẠI HÀM RELOAD LẠI TÀI PHÚ
                    this.ReloadTotalValue();
                }

                // THỰC HIỆN DETACK TOÀN BỘ THUỘC TÍNH CỦA ITEM HIỆN TẠI

                // THỰC HIỆN CALL LẠI RECACLU ALL ITEM VÀ ACTIVE HIIDEN PROB()
            }
            else
            {
                KTPlayerManager.ShowNotification(this._Player, "Trang bị hiện không tồn trại trên người");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Hàm xử lý mặc đồ vào người
        /// </summary>
        /// <param name="goodsData"></param>
        /// <returns></returns>
        public bool EquipLoadMain(GoodsData goodsData)
        {
            /// Khóa luôn trang bị vào
            goodsData.Binding = 1;

            KItem ItemFind = new KItem(goodsData);

            if (ItemFind._ItemData.Genre == 7)
            {
                // Lấy ra vị trí trang bị
                int Postion = ItemManager.g_anEquipPetPos[ItemFind.GetBaseItem.DetailType];

                string[] dbFields = null;

                TCPOutPacketPool pool = Global._TCPManager.TcpOutPacketPool;
                TCPClientPool tcpClientPool = Global._TCPManager.tcpClientPool;

                // Trường hợp này chắc chắn sẽ còn chỗ

                // Nếu thằng này đang muốn mặc nhẫn

                /// Nếu có thì thực hiện DETACK món đồ cũ trước
                if (PetEquipBody.ContainsKey((KE_EQUIP_PET_POSTION)Postion))
                {
                    // Verify Item xem có đúng vật phẩm không
                    KItem _ItemFromDic = PetEquipBody[(KE_EQUIP_PET_POSTION)Postion];

                    // Thực hiện Detack toàn bộ hiệu ứng đang mặc của bộ hiện tại

                    int GetBagIndexFormNewItem = goodsData.BagIndex;

                    Dictionary<UPDATEITEM, object> TotalUpdate = new Dictionary<UPDATEITEM, object>();

                    TotalUpdate.Add(UPDATEITEM.ROLEID, this._Player.RoleID);

                    TotalUpdate.Add(UPDATEITEM.ITEMDBID, _ItemFromDic._GoodDatas.Id);
                    // Update lại vị trí cho vật phẩm hiện tại
                    TotalUpdate.Add(UPDATEITEM.BAGINDEX, GetBagIndexFormNewItem);
                    //Set lại giá tri cho việc không sử dụng nữa
                    TotalUpdate.Add(UPDATEITEM.USING, -1);

                    string ScriptUpdateBuild = ItemManager.ItemUpdateScriptBuild(TotalUpdate);

                    TCPProcessCmdResults dbRequestResult = Global.RequestToDBServer(tcpClientPool, pool, (int)TCPGameServerCmds.CMD_DB_UPDATEGOODS_CMD, ScriptUpdateBuild, out dbFields, this._Player.ServerId);

                    if (dbRequestResult == TCPProcessCmdResults.RESULT_FAILED)
                    {
                        LogManager.WriteLog(LogTypes.EquipBody, "Mặc đồ lỗi do không thể tháo đồ cũ ra");

                        return false;
                    }
                    else if (dbRequestResult == TCPProcessCmdResults.RESULT_DATA)
                    {
                        // Gửi packet thứ nhất - Tháo đồ trước đó ra vị trí của đồ chuẩn bị mặc lên

                        // KTGlobal.ModifyGoods(this._Player, _ItemFromDic._GoodDatas.Id, TotalUpdate);
                        this._Player.GoodsData.LocalModify(_ItemFromDic._GoodDatas.Id, TotalUpdate);
                        SCModGoods scData = new SCModGoods()
                        {
                            BagIndex = GetBagIndexFormNewItem,
                            Count = _ItemFromDic._GoodDatas.GCount,
                            IsUsing = -1,
                            ModType = (int)ModGoodsTypes.EquipUnload,
                            ID = _ItemFromDic._GoodDatas.Id,
                            NewHint = 0,
                            Site = 0,
                            State = 0,
                        };
                        // Gửi thông báo về là tháo đồ ra thành công
                        this._Player.SendPacket((int)TCPGameServerCmds.CMD_SPR_MOD_GOODS, scData);
                    }

                    // NẾu thực hiện tháo đồ ra thành công

                    this.ClearAllEffecPetEquipBody();
                    // Thực hiện tháo đồ cũ ra khỏi người
                    lock (PetEquipBody)
                    {
                        PetEquipBody.Remove((KE_EQUIP_PET_POSTION)Postion);
                    }

                    // Thực hiện mặc đồ mới vòa người

                    TotalUpdate = new Dictionary<UPDATEITEM, object>();

                    TotalUpdate.Add(UPDATEITEM.ROLEID, this._Player.RoleID);
                    TotalUpdate.Add(UPDATEITEM.ITEMDBID, goodsData.Id);
                    // Update lại vị trí cho vật phẩm hiện tại
                    TotalUpdate.Add(UPDATEITEM.BAGINDEX, -1);
                    /// Khóa luôn
                    TotalUpdate.Add(UPDATEITEM.BINDING, 1);
                    //Set lại giá tri cho việc không sử dụng nữa
                    TotalUpdate.Add(UPDATEITEM.USING, Postion);

                    ScriptUpdateBuild = ItemManager.ItemUpdateScriptBuild(TotalUpdate);

                    dbRequestResult = Global.RequestToDBServer(tcpClientPool, pool, (int)TCPGameServerCmds.CMD_DB_UPDATEGOODS_CMD, ScriptUpdateBuild, out dbFields, this._Player.ServerId);

                    if (dbRequestResult == TCPProcessCmdResults.RESULT_FAILED)
                    {
                        LogManager.WriteLog(LogTypes.EquipBody, "Mặc đồ lỗi do không thể tháo đồ cũ ra");

                        return false;
                    }
                    else if (dbRequestResult == TCPProcessCmdResults.RESULT_DATA)
                    {
                        // KTGlobal.ModifyGoods(this._Player, goodsData.Id, TotalUpdate);

                        this._Player.GoodsData.LocalModify(goodsData.Id, TotalUpdate);
                        // Gửi packet thứ nhất - Tháo đồ trước đó ra vị trí của đồ chuẩn bị mặc lên

                        SCModGoods scData = new SCModGoods()
                        {
                            BagIndex = -1,
                            Count = goodsData.GCount,
                            IsUsing = Postion,
                            ModType = (int)ModGoodsTypes.EquipLoad,
                            ID = goodsData.Id,
                            NewHint = 0,
                            Site = 0,
                            State = 0,
                        };
                        // Gửi thông báo về cho việc mặc đồ thành công
                        this._Player.SendPacket((int)TCPGameServerCmds.CMD_SPR_MOD_GOODS, scData);
                    }

                    lock (PetEquipBody)
                    {
                        PetEquipBody.Add((KE_EQUIP_PET_POSTION)Postion, ItemFind);
                    }

                    this.AttackAllPetEquipBody();
                }
                /// Nếu vị trí trước đó chưa từng có đồ nào
                else
                {
                    //Xóa toàn bộ thuộc tính của pet
                    this.ClearAllEffecPetEquipBody();

                    Dictionary<UPDATEITEM, object> TotalUpdate = new Dictionary<UPDATEITEM, object>();

                    TotalUpdate.Add(UPDATEITEM.ROLEID, this._Player.RoleID);
                    TotalUpdate.Add(UPDATEITEM.ITEMDBID, goodsData.Id);
                    // Update lại vị trí cho vật phẩm hiện tại
                    TotalUpdate.Add(UPDATEITEM.BAGINDEX, -1);
                    /// Khóa luôn
                    TotalUpdate.Add(UPDATEITEM.BINDING, 1);
                    //Set lại giá tri cho việc không sử dụng nữa
                    TotalUpdate.Add(UPDATEITEM.USING, Postion);

                    string ScriptUpdateBuild = ItemManager.ItemUpdateScriptBuild(TotalUpdate);

                    TCPProcessCmdResults dbRequestResult = Global.RequestToDBServer(tcpClientPool, pool, (int)TCPGameServerCmds.CMD_DB_UPDATEGOODS_CMD, ScriptUpdateBuild, out dbFields, this._Player.ServerId);

                    if (dbRequestResult == TCPProcessCmdResults.RESULT_FAILED)
                    {
                        LogManager.WriteLog(LogTypes.EquipBody, "Mặc đồ lỗi do không thể tháo đồ cũ ra");

                        return false;
                    }
                    else if (dbRequestResult == TCPProcessCmdResults.RESULT_DATA)
                    {
                        // KTGlobal.ModifyGoods(this._Player, goodsData.Id, TotalUpdate);
                        // Gửi packet mặc đồ thành công về
                        this._Player.GoodsData.LocalModify(goodsData.Id, TotalUpdate);
                        SCModGoods scData = new SCModGoods()
                        {
                            BagIndex = -1,
                            Count = goodsData.GCount,
                            IsUsing = Postion,
                            ModType = (int)ModGoodsTypes.EquipLoad,
                            ID = goodsData.Id,
                            NewHint = 0,
                            Site = 0,
                            State = 0,
                        };
                        // Gửi thông báo về cho việc mặc đồ thành công
                        this._Player.SendPacket((int)TCPGameServerCmds.CMD_SPR_MOD_GOODS, scData);
                    }

                    lock (PetEquipBody)
                    {
                        PetEquipBody.Add((KE_EQUIP_PET_POSTION)Postion, ItemFind);
                    }

                    this.AttackAllPetEquipBody();
                }
            }
            else //Nếu là người mặc
            {
                // lấy ra vị trí của vật phẩm
                int Postion = ItemManager.g_anEquipPos[ItemFind.GetBaseItem.DetailType];

                string[] dbFields = null;

                TCPOutPacketPool pool = Global._TCPManager.TcpOutPacketPool;
                TCPClientPool tcpClientPool = Global._TCPManager.tcpClientPool;

                // Trường hợp này chắc chắn sẽ còn chỗ

                // Nếu thằng này đang muốn mặc nhẫn
                if (Postion == (int)KE_EQUIP_POSITION.emEQUIPPOS_RING)
                {
                    // Nếu mà vị trí thứ 1 nó chưa có cái nhẫn nào thì mặc định là mặc vào vị trí 1
                    if (!EquipBody.ContainsKey(KE_EQUIP_POSITION.emEQUIPPOS_RING))
                    {
                        // Suy ra nó đang muốn đeo cái ring 1
                        Postion = (int)KE_EQUIP_POSITION.emEQUIPPOS_RING;
                    }

                    // Nếu đã mặc cái nhẫn 1 rồi mà vị trí của nhẫn 2 đang trống
                    if (EquipBody.ContainsKey(KE_EQUIP_POSITION.emEQUIPPOS_RING) && !EquipBody.ContainsKey(KE_EQUIP_POSITION.emEQUIPPOS_RING_2))
                    {
                        // Suy ra nó đang muốn đeo cái ring 2
                        Postion = (int)KE_EQUIP_POSITION.emEQUIPPOS_RING_2;
                        ItemFind.IsSubRing = true;
                    }
                }

                /// Nếu có thì thực hiện DETACK món đồ cũ trước
                if (EquipBody.ContainsKey((KE_EQUIP_POSITION)Postion))
                {
                    // Verify Item xem có đúng vật phẩm không
                    KItem _ItemFromDic = EquipBody[(KE_EQUIP_POSITION)Postion];

                    // Thực hiện Detack toàn bộ hiệu ứng đang mặc của bộ hiện tại

                    int GetBagIndexFormNewItem = goodsData.BagIndex;

                    Dictionary<UPDATEITEM, object> TotalUpdate = new Dictionary<UPDATEITEM, object>();

                    TotalUpdate.Add(UPDATEITEM.ROLEID, this._Player.RoleID);

                    TotalUpdate.Add(UPDATEITEM.ITEMDBID, _ItemFromDic._GoodDatas.Id);
                    // Update lại vị trí cho vật phẩm hiện tại
                    TotalUpdate.Add(UPDATEITEM.BAGINDEX, GetBagIndexFormNewItem);
                    //Set lại giá tri cho việc không sử dụng nữa
                    TotalUpdate.Add(UPDATEITEM.USING, -1);

                    string ScriptUpdateBuild = ItemManager.ItemUpdateScriptBuild(TotalUpdate);

                    TCPProcessCmdResults dbRequestResult = Global.RequestToDBServer(tcpClientPool, pool, (int)TCPGameServerCmds.CMD_DB_UPDATEGOODS_CMD, ScriptUpdateBuild, out dbFields, this._Player.ServerId);

                    if (dbRequestResult == TCPProcessCmdResults.RESULT_FAILED)
                    {
                        LogManager.WriteLog(LogTypes.EquipBody, "Mặc đồ lỗi do không thể tháo đồ cũ ra");

                        return false;
                    }
                    else if (dbRequestResult == TCPProcessCmdResults.RESULT_DATA)
                    {
                        // Gửi packet thứ nhất - Tháo đồ trước đó ra vị trí của đồ chuẩn bị mặc lên

                        //KTGlobal.ModifyGoods(this._Player, _ItemFromDic._GoodDatas.Id, TotalUpdate);

                        this._Player.GoodsData.LocalModify(_ItemFromDic._GoodDatas.Id, TotalUpdate);

                        SCModGoods scData = new SCModGoods()
                        {
                            BagIndex = GetBagIndexFormNewItem,
                            Count = _ItemFromDic._GoodDatas.GCount,
                            IsUsing = -1,
                            ModType = (int)ModGoodsTypes.EquipUnload,
                            ID = _ItemFromDic._GoodDatas.Id,
                            NewHint = 0,
                            Site = 0,
                            State = 0,
                        };
                        // Gửi thông báo về là tháo đồ ra thành công
                        this._Player.SendPacket((int)TCPGameServerCmds.CMD_SPR_MOD_GOODS, scData);
                    }

                    // NẾu thực hiện tháo đồ ra thành công

                    this.ClearAllEffecEquipBody();
                    // Thực hiện tháo đồ cũ ra khỏi người
                    lock (EquipBody)
                    {
                        EquipBody.Remove((KE_EQUIP_POSITION)Postion);
                    }

                    // Thực hiện mặc đồ mới vòa người

                    TotalUpdate = new Dictionary<UPDATEITEM, object>();

                    TotalUpdate.Add(UPDATEITEM.ROLEID, this._Player.RoleID);
                    TotalUpdate.Add(UPDATEITEM.ITEMDBID, goodsData.Id);
                    // Update lại vị trí cho vật phẩm hiện tại
                    TotalUpdate.Add(UPDATEITEM.BAGINDEX, -1);
                    /// Khóa luôn
                    TotalUpdate.Add(UPDATEITEM.BINDING, 1);
                    //Set lại giá tri cho việc không sử dụng nữa
                    TotalUpdate.Add(UPDATEITEM.USING, Postion);

                    ScriptUpdateBuild = ItemManager.ItemUpdateScriptBuild(TotalUpdate);

                    dbRequestResult = Global.RequestToDBServer(tcpClientPool, pool, (int)TCPGameServerCmds.CMD_DB_UPDATEGOODS_CMD, ScriptUpdateBuild, out dbFields, this._Player.ServerId);

                    if (dbRequestResult == TCPProcessCmdResults.RESULT_FAILED)
                    {
                        LogManager.WriteLog(LogTypes.EquipBody, "Mặc đồ lỗi do không thể tháo đồ cũ ra");

                        return false;
                    }
                    else if (dbRequestResult == TCPProcessCmdResults.RESULT_DATA)
                    {
                        // KTGlobal.ModifyGoods(this._Player, goodsData.Id, TotalUpdate);

                        this._Player.GoodsData.LocalModify(goodsData.Id, TotalUpdate);
                        // Gửi packet thứ nhất - Tháo đồ trước đó ra vị trí của đồ chuẩn bị mặc lên

                        SCModGoods scData = new SCModGoods()
                        {
                            BagIndex = -1,
                            Count = goodsData.GCount,
                            IsUsing = Postion,
                            ModType = (int)ModGoodsTypes.EquipLoad,
                            ID = goodsData.Id,
                            NewHint = 0,
                            Site = 0,
                            State = 0,
                        };
                        // Gửi thông báo về cho việc mặc đồ thành công
                        this._Player.SendPacket((int)TCPGameServerCmds.CMD_SPR_MOD_GOODS, scData);
                    }

                    lock (EquipBody)
                    {
                        EquipBody.Add((KE_EQUIP_POSITION)Postion, ItemFind);
                    }

                    this.AttackAllEquipBody();
                }
                /// Nếu vị trí trước đó chưa từng có đồ nào
                else
                {
                    this.ClearAllEffecEquipBody();

                    Dictionary<UPDATEITEM, object> TotalUpdate = new Dictionary<UPDATEITEM, object>();

                    TotalUpdate.Add(UPDATEITEM.ROLEID, this._Player.RoleID);
                    TotalUpdate.Add(UPDATEITEM.ITEMDBID, goodsData.Id);
                    // Update lại vị trí cho vật phẩm hiện tại
                    TotalUpdate.Add(UPDATEITEM.BAGINDEX, -1);
                    /// Khóa luôn
                    TotalUpdate.Add(UPDATEITEM.BINDING, 1);
                    //Set lại giá tri cho việc không sử dụng nữa
                    TotalUpdate.Add(UPDATEITEM.USING, Postion);

                    string ScriptUpdateBuild = ItemManager.ItemUpdateScriptBuild(TotalUpdate);

                    TCPProcessCmdResults dbRequestResult = Global.RequestToDBServer(tcpClientPool, pool, (int)TCPGameServerCmds.CMD_DB_UPDATEGOODS_CMD, ScriptUpdateBuild, out dbFields, this._Player.ServerId);

                    if (dbRequestResult == TCPProcessCmdResults.RESULT_FAILED)
                    {
                        LogManager.WriteLog(LogTypes.EquipBody, "Mặc đồ lỗi do không thể tháo đồ cũ ra");

                        return false;
                    }
                    else if (dbRequestResult == TCPProcessCmdResults.RESULT_DATA)
                    {
                        this._Player.GoodsData.LocalModify(goodsData.Id, TotalUpdate);
                        //KTGlobal.ModifyGoods(this._Player, goodsData.Id, TotalUpdate);
                        // Gửi packet mặc đồ thành công về

                        SCModGoods scData = new SCModGoods()
                        {
                            BagIndex = -1,
                            Count = goodsData.GCount,
                            IsUsing = Postion,
                            ModType = (int)ModGoodsTypes.EquipLoad,
                            ID = goodsData.Id,
                            NewHint = 0,
                            Site = 0,
                            State = 0,
                        };
                        // Gửi thông báo về cho việc mặc đồ thành công
                        this._Player.SendPacket((int)TCPGameServerCmds.CMD_SPR_MOD_GOODS, scData);
                    }

                    lock (EquipBody)
                    {
                        EquipBody.Add((KE_EQUIP_POSITION)Postion, ItemFind);
                    }

                    this.AttackAllEquipBody();
                }
            }

            return true;
        }

        /// <summary>
        /// Hàm mặc đồ phụ
        /// </summary>
        /// <param name="goodsData"></param>
        /// <returns></returns>
        public bool EquipLoadSub(GoodsData goodsData)
        {
            KTPlayerManager.ShowNotification(this._Player, "Hệ thống dự phòng hiện đang tạm khóa!");
            return false;
            /// Khóa luôn trang bị vào
            //goodsData.Binding = 1;

            //KItem ItemFind = new KItem(goodsData);

            //// lấy ra vị trí của vật phẩm
            //int Postion = ItemManager.g_anEquipSubPos[ItemFind.GetBaseItem.DetailType];

            //string[] dbFields = null;

            //TCPOutPacketPool pool = Global._TCPManager.TcpOutPacketPool;
            //TCPClientPool tcpClientPool = Global._TCPManager.tcpClientPool;

            //// Trường hợp này chắc chắn sẽ còn chỗ

            //if (Postion == (int)KE_EQUIP_POSITION_SUB.emEQUIPPOS_RING)
            //{
            //    // Nếu mà vị trí thứ 1 nó chưa có cái nhẫn nào thì mặc định là mặc vào vị trí 1
            //    if (!PreventiveEquipBody.ContainsKey(KE_EQUIP_POSITION_SUB.emEQUIPPOS_RING))
            //    {
            //        // Suy ra nó đang muốn đeo cái ring 1
            //        Postion = (int)KE_EQUIP_POSITION_SUB.emEQUIPPOS_RING;
            //    }

            //    // Nếu đã mặc cái nhẫn 1 rồi mà vị trí của nhẫn 2 đang trống
            //    if (PreventiveEquipBody.ContainsKey(KE_EQUIP_POSITION_SUB.emEQUIPPOS_RING) && !PreventiveEquipBody.ContainsKey(KE_EQUIP_POSITION_SUB.emEQUIPPOS_RING_2))
            //    {
            //        // Suy ra nó đang muốn đeo cái ring 2
            //        Postion = (int)KE_EQUIP_POSITION_SUB.emEQUIPPOS_RING_2;
            //        ItemFind.IsSubRing = true;
            //    }
            //}

            ///// Kiểm tra xem có đang mặc 1 món đồ khác trên người mà cùng vị trí không
            ///// Nếu có thì thực hiện DETACK món đồ cũ trước
            //if (PreventiveEquipBody.ContainsKey((KE_EQUIP_POSITION_SUB)Postion))
            //{
            //    // Verify Item xem có đúng vật phẩm không
            //    KItem _ItemFromDic = PreventiveEquipBody[(KE_EQUIP_POSITION_SUB)Postion];

            //    // Thực hiện Detack toàn bộ hiệu ứng đang mặc của bộ hiện tại

            //    int GetBagIndexFormNewItem = goodsData.BagIndex;

            //    Dictionary<UPDATEITEM, object> TotalUpdate = new Dictionary<UPDATEITEM, object>();

            //    TotalUpdate.Add(UPDATEITEM.ROLEID, this._Player.RoleID);

            //    TotalUpdate.Add(UPDATEITEM.ITEMDBID, _ItemFromDic._GoodDatas.Id);
            //    // Update lại vị trí cho vật phẩm hiện tại
            //    TotalUpdate.Add(UPDATEITEM.BAGINDEX, GetBagIndexFormNewItem);
            //    //Set lại giá tri cho việc không sử dụng nữa
            //    TotalUpdate.Add(UPDATEITEM.USING, -1);

            //    string ScriptUpdateBuild = ItemManager.ItemUpdateScriptBuild(TotalUpdate);

            //    TCPProcessCmdResults dbRequestResult = Global.RequestToDBServer(tcpClientPool, pool, (int)TCPGameServerCmds.CMD_DB_UPDATEGOODS_CMD, ScriptUpdateBuild, out dbFields, this._Player.ServerId);

            //    if (dbRequestResult == TCPProcessCmdResults.RESULT_FAILED)
            //    {
            //        LogManager.WriteLog(LogTypes.EquipBody, "Mặc đồ lỗi do không thể tháo đồ cũ ra");

            //        return false;
            //    }
            //    else if (dbRequestResult == TCPProcessCmdResults.RESULT_DATA)
            //    {
            //        // Gửi packet thứ nhất - Tháo đồ trước đó ra vị trí của đồ chuẩn bị mặc lên

            //       // KTGlobal.ModifyGoods(this._Player, _ItemFromDic._GoodDatas.Id, TotalUpdate);

            //        this._Player.GoodsData.LocalModify(_ItemFromDic._GoodDatas.Id, TotalUpdate);

            //        SCModGoods scData = new SCModGoods()
            //        {
            //            BagIndex = GetBagIndexFormNewItem,
            //            Count = _ItemFromDic._GoodDatas.GCount,
            //            IsUsing = -1,
            //            ModType = (int)ModGoodsTypes.EquipUnload,
            //            ID = _ItemFromDic._GoodDatas.Id,
            //            NewHint = 0,
            //            Site = 0,
            //            State = 0,
            //        };
            //        // Gửi thông báo về là tháo đồ ra thành công
            //        this._Player.SendPacket((int)TCPGameServerCmds.CMD_SPR_MOD_GOODS, scData);
            //    }

            //    // NẾu thực hiện tháo đồ ra thành công
            //    // Vì là hàm phụ lên ko detack gì cả
            //    //this.ClearAllEffecEquipBody();
            //    // Thực hiện tháo đồ cũ ra khỏi người
            //    lock (PreventiveEquipBody)
            //    {
            //        PreventiveEquipBody.Remove((KE_EQUIP_POSITION_SUB)Postion);
            //    }

            //    // Thực hiện mặc đồ mới vòa người

            //    TotalUpdate = new Dictionary<UPDATEITEM, object>();

            //    TotalUpdate.Add(UPDATEITEM.ROLEID, this._Player.RoleID);
            //    TotalUpdate.Add(UPDATEITEM.ITEMDBID, goodsData.Id);
            //    // Update lại vị trí cho vật phẩm hiện tại
            //    TotalUpdate.Add(UPDATEITEM.BAGINDEX, -1);
            //    /// Khóa luôn
            //    TotalUpdate.Add(UPDATEITEM.BINDING, 1);
            //    //Set lại giá tri cho việc không sử dụng nữa
            //    TotalUpdate.Add(UPDATEITEM.USING, Postion);

            //    ScriptUpdateBuild = ItemManager.ItemUpdateScriptBuild(TotalUpdate);

            //    dbRequestResult = Global.RequestToDBServer(tcpClientPool, pool, (int)TCPGameServerCmds.CMD_DB_UPDATEGOODS_CMD, ScriptUpdateBuild, out dbFields, this._Player.ServerId);

            //    if (dbRequestResult == TCPProcessCmdResults.RESULT_FAILED)
            //    {
            //        LogManager.WriteLog(LogTypes.EquipBody, "Mặc đồ lỗi do không thể tháo đồ cũ ra");

            //        return false;
            //    }
            //    else if (dbRequestResult == TCPProcessCmdResults.RESULT_DATA)
            //    {
            //        //KTGlobal.ModifyGoods(this._Player, goodsData.Id, TotalUpdate);

            //        this._Player.GoodsData.LocalModify(goodsData.Id, TotalUpdate);

            //        // Gửi packet thứ nhất - Tháo đồ trước đó ra vị trí của đồ chuẩn bị mặc lên

            //        SCModGoods scData = new SCModGoods()
            //        {
            //            BagIndex = -1,
            //            Count = goodsData.GCount,
            //            IsUsing = Postion,
            //            ModType = (int)ModGoodsTypes.EquipLoad,
            //            ID = goodsData.Id,
            //            NewHint = 0,
            //            Site = 0,
            //            State = 0,
            //        };
            //        // Gửi thông báo về cho việc mặc đồ thành công
            //        this._Player.SendPacket((int)TCPGameServerCmds.CMD_SPR_MOD_GOODS, scData);
            //    }

            //    // ADD ĐỒ VÀO SLOT DỰ PHÒNG
            //    lock (PreventiveEquipBody)
            //    {
            //        PreventiveEquipBody.Add((KE_EQUIP_POSITION_SUB)Postion, ItemFind);
            //    }

            //    // CHỈ TÍNH LẠI TÀI PHÚ
            //    this.ReloadTotalValue();
            //    //this.AttackAllEquipBody();
            //}
            ///// Nếu vị trí trước đó chưa từng có đồ nào
            //else
            //{
            //    Dictionary<UPDATEITEM, object> TotalUpdate = new Dictionary<UPDATEITEM, object>();

            //    TotalUpdate.Add(UPDATEITEM.ROLEID, this._Player.RoleID);
            //    TotalUpdate.Add(UPDATEITEM.ITEMDBID, goodsData.Id);
            //    // Update lại vị trí cho vật phẩm hiện tại
            //    TotalUpdate.Add(UPDATEITEM.BAGINDEX, -1);
            //    /// Khóa luôn
            //    TotalUpdate.Add(UPDATEITEM.BINDING, 1);
            //    //Set lại giá tri cho việc không sử dụng nữa
            //    TotalUpdate.Add(UPDATEITEM.USING, Postion);

            //    string ScriptUpdateBuild = ItemManager.ItemUpdateScriptBuild(TotalUpdate);

            //    TCPProcessCmdResults dbRequestResult = Global.RequestToDBServer(tcpClientPool, pool, (int)TCPGameServerCmds.CMD_DB_UPDATEGOODS_CMD, ScriptUpdateBuild, out dbFields, this._Player.ServerId);

            //    if (dbRequestResult == TCPProcessCmdResults.RESULT_FAILED)
            //    {
            //        LogManager.WriteLog(LogTypes.EquipBody, "Mặc đồ lỗi do không thể tháo đồ cũ ra");

            //        return false;
            //    }
            //    else if (dbRequestResult == TCPProcessCmdResults.RESULT_DATA)
            //    {
            //        //KTGlobal.ModifyGoods(this._Player, goodsData.Id, TotalUpdate);
            //        // Gửi packet mặc đồ thành công về
            //        this._Player.GoodsData.LocalModify(goodsData.Id, TotalUpdate);

            //        SCModGoods scData = new SCModGoods()
            //        {
            //            BagIndex = -1,
            //            Count = goodsData.GCount,
            //            IsUsing = Postion,
            //            ModType = (int)ModGoodsTypes.EquipLoad,
            //            ID = goodsData.Id,
            //            NewHint = 0,
            //            Site = 0,
            //            State = 0,
            //        };
            //        // Gửi thông báo về cho việc mặc đồ thành công
            //        this._Player.SendPacket((int)TCPGameServerCmds.CMD_SPR_MOD_GOODS, scData);
            //    }

            //    lock (PreventiveEquipBody)
            //    {
            //        PreventiveEquipBody.Add((KE_EQUIP_POSITION_SUB)Postion, ItemFind);
            //    }

            //    // KO ATTACK GÌ HẾT MÀ CHỈ RECACLULATION LẠI TÀI PHÚ
            //    // this.AttackAllEquipBody();

            //    this.ReloadTotalValue();
            //}

            //return true;
        }

        /// <summary>
        /// Trả về trang bị ở vị trí tương ứng
        /// </summary>
        /// <param name="Position"></param>
        /// <returns></returns>
        public KItem GetItemByPostion(int Position)
        {
            if (EquipBody.ContainsKey((KE_EQUIP_POSITION)Position))
            {
                return EquipBody[(KE_EQUIP_POSITION)Position];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gọi khi nhân vật vào Game, Attach chỉ số của tất cả trang bị
        /// </summary>
        public void InitEquipBody()
        {
            /// Danh sách trang bị hiện có
            List<GoodsData> equips = this._Player.GoodsData.FindAll(x => x.Using >= 0 && x.Site == 0);

            /// Nếu tồn tại
            if (equips.Count > 0)
            {
                /// Duyệt danh sách trang bị
                foreach (GoodsData itemGD in equips)
                {
                    /// Trang bị thường
                    if (itemGD.Using < 100)
                    {
                        KE_EQUIP_POSITION _Positon = (KE_EQUIP_POSITION)itemGD.Using;
                        lock (EquipBody)
                        {
                            if(!EquipBody.ContainsKey(_Positon))
                            EquipBody.Add(_Positon, new KItem(itemGD));
                        }
                    }
                    /// Trang bị pet
                    else if (itemGD.Using >= 200)
                    {
                        KE_EQUIP_PET_POSTION _Positon = (KE_EQUIP_PET_POSTION)itemGD.Using;

                        lock (PetEquipBody)
                        {
                            if (!PetEquipBody.ContainsKey(_Positon))
                                PetEquipBody.Add(_Positon, new KItem(itemGD));
                        }
                    }
                    /// Trang bị dự phòng
                    else if (itemGD.Using >= 100)
                    {
                        KE_EQUIP_POSITION_SUB _Positon = (KE_EQUIP_POSITION_SUB)itemGD.Using;

                        lock (PreventiveEquipBody)
                        {
                            if (!PreventiveEquipBody.ContainsKey(_Positon))
                                PreventiveEquipBody.Add(_Positon, new KItem(itemGD));
                        }
                    }
                }

                /// Kích hoạt chỉ số
                this.AttackAllEquipBody(true);
            }
        }

        /// <summary>
        /// Thay đổi ngũ hành các trang bị đặc biệt tương ứng với ngũ hành hiện tại của nhân vật
        /// </summary>
        public void ChangeEquipsCorrespondingToSeries()
        {
            /// Danh sách trang bị đặc biệt
            List<GoodsData> specialEquips = this._Player.GoodsData.FindAll((itemGD) =>
            {
                /// Nếu là phi phong
                if (itemGD.Using == (int)KE_EQUIP_POSITION.emEQUIPPOS_MANTLE || itemGD.Using == (int)KE_EQUIP_POSITION_SUB.emEQUIPPOS_MANTLE)
                {
                    return true;
                }
                /// Nếu là ngũ hành ấn
                else if (itemGD.Using == (int)KE_EQUIP_POSITION.emEQUIPPOS_SIGNET || itemGD.Using == (int)KE_EQUIP_POSITION_SUB.emEQUIPPOS_SIGNET)
                {
                    return true;
                }
                /// Nếu là quan ấn
                else if (itemGD.Using == (int)KE_EQUIP_POSITION.emEQUIPPOS_CHOP || itemGD.Using == (int)KE_EQUIP_POSITION_SUB.emEQUIPPOS_CHOP)
                {
                    return true;
                }

                /// Không thỏa mãn
                return false;
            });

            /// Môn phái nhân vật
            int factionID = this._Player.m_cPlayerFaction.GetFactionId();
            /// Giới tính nhân vật
            int sex = this._Player.RoleSex;
            /// Ngũ hành nhân vật
            KE_SERIES_TYPE series = this._Player.m_Series;
            /// Thông tin TCP
            TCPOutPacketPool pool = Global._TCPManager.TcpOutPacketPool;
            TCPClientPool tcpClientPool = Global._TCPManager.tcpClientPool;

            /// Duyệt danh sách trang bị đặc biệt
            foreach (GoodsData itemGD in specialEquips)
            {
                /// Thông tin vật phẩm
                ItemData goodsItemData = ItemManager.GetItemTemplate(itemGD.GoodsID);
                /// Toác
                if (goodsItemData == null)
                {
                    continue;
                }

                /// Danh sách cập nhật chỉ số trang bị
                Dictionary<UPDATEITEM, object> updateProperties = new Dictionary<UPDATEITEM, object>();
                updateProperties.Add(UPDATEITEM.ROLEID, this._Player.RoleID);
                updateProperties.Add(UPDATEITEM.ITEMDBID, itemGD.Id);

                /// ID vật phẩm
                int goodsID = itemGD.GoodsID;
                /// Ngũ hành
                int goodsSeries = itemGD.Series;
                /// Nếu là ngũ hành ấn
                if (itemGD.Using == (int)KE_EQUIP_POSITION.emEQUIPPOS_SIGNET || itemGD.Using == (int)KE_EQUIP_POSITION_SUB.emEQUIPPOS_SIGNET)
                {
                    /// Tìm ngũ hành ấn tương ứng phái
                    ItemData itemData = ItemManager.Signets.Where((goods) =>
                    {
                        /// Nếu không cùng cấp độ
                        if (goods.Level != goodsItemData.Level)
                        {
                            return false;
                        }
                        /// Nếu phái không khớp
                        else if (!goods.ListReqProp.Any(x => x.ReqPropType == (int)KE_ITEM_REQUIREMENT.emEQUIP_REQ_FACTION && x.ReqPropValue == factionID))
                        {
                            return false;
                        }

                        /// Thỏa mãn
                        return true;
                    }).FirstOrDefault();
                    /// Nếu tìm thấy
                    if (itemData != null)
                    {
                        /// Thay đổi ID vật phẩm
                        goodsID = itemData.ItemID;
                        updateProperties.Add(UPDATEITEM.GOODSID, itemData.ItemID);
                        /// Thay đổi ngũ hành
                        goodsSeries = itemData.Series;
                        updateProperties.Add(UPDATEITEM.SERIES, itemData.Series);
                    }
                }
                /// Nếu là phi phong
                else if (itemGD.Using == (int)KE_EQUIP_POSITION.emEQUIPPOS_MANTLE || itemGD.Using == (int)KE_EQUIP_POSITION_SUB.emEQUIPPOS_MANTLE)
                {
                    /// Tìm phi phong tương ứng hệ
                    ItemData itemData = ItemManager.Mantles.Where((goods) =>
                    {
                        /// Nếu không cùng cấp độ
                        if (goods.Level != goodsItemData.Level)
                        {
                            return false;
                        }
                        /// Nếu yêu cầu giới tính không thỏa mãn
                        else if (!goods.ListReqProp.Any(x => x.ReqPropType == (int)KE_ITEM_REQUIREMENT.emEQUIP_REQ_SEX && x.ReqPropValue == sex))
                        {
                            return false;
                        }
                        /// Nếu yêu cầu ngũ hành không thỏa mãn
                        else if (!goods.ListReqProp.Any(x => x.ReqPropType == (int)KE_ITEM_REQUIREMENT.emEQUIP_REQ_SERIES && x.ReqPropValue == (int)series))
                        {
                            return false;
                        }

                        /// Thỏa mãn
                        return true;
                    }).FirstOrDefault();
                    /// Nếu tìm thấy
                    if (itemData != null)
                    {
                        /// Thay đổi ID vật phẩm
                        goodsID = itemData.ItemID;
                        updateProperties.Add(UPDATEITEM.GOODSID, itemData.ItemID);
                        /// Thay đổi ngũ hành
                        goodsSeries = itemData.Series;
                        updateProperties.Add(UPDATEITEM.SERIES, itemData.Series);
                    }
                }
                /// Nếu là quan ấn
                else if (itemGD.Using == (int)KE_EQUIP_POSITION.emEQUIPPOS_CHOP || itemGD.Using == (int)KE_EQUIP_POSITION_SUB.emEQUIPPOS_CHOP)
                {
                    /// Tìm quan ấn tương ứng hệ
                    ItemData itemData = ItemManager.Chops.Where((goods) =>
                    {
                        /// Nếu không cùng cấp độ
                        if (goods.Level != goodsItemData.Level)
                        {
                            return false;
                        }
                        /// Nếu yêu cầu ngũ hành không thỏa mãn
                        else if (!goods.ListReqProp.Any(x => x.ReqPropType == (int)KE_ITEM_REQUIREMENT.emEQUIP_REQ_SERIES && x.ReqPropValue == (int)series))
                        {
                            return false;
                        }

                        /// Thỏa mãn
                        return true;
                    }).FirstOrDefault();
                    /// Nếu tìm thấy
                    if (itemData != null)
                    {
                        /// Thay đổi ID vật phẩm
                        goodsID = itemData.ItemID;
                        updateProperties.Add(UPDATEITEM.GOODSID, itemData.ItemID);
                        /// Thay đổi ngũ hành
                        goodsSeries = itemData.Series;
                        updateProperties.Add(UPDATEITEM.SERIES, itemData.Series);
                    }
                }

                /// Nếu không có gì thay đổi thì thôi
                if (goodsID == itemGD.GoodsID)
                {
                    continue;
                }

                string cmdData = ItemManager.ItemUpdateScriptBuild(updateProperties);
                TCPProcessCmdResults dbRequestResult = Global.RequestToDBServer(tcpClientPool, pool, (int)TCPGameServerCmds.CMD_DB_UPDATEGOODS_CMD, cmdData, out string[] dbFields, this._Player.ServerId);
                /// Nếu thay đổi thành công
                if (dbRequestResult == TCPProcessCmdResults.RESULT_DATA)
                {
                    /// Cập nhật thay đổi ở GS
                    itemGD.GoodsID = goodsID;
                    itemGD.Series = goodsSeries;
                    /// Cập nhật thay đổi về Client
                    this._Player.SendPacket<GoodsData>((int)TCPGameServerCmds.CMD_SPR_NOTIFYGOODSINFO, itemGD);
                }
            }
        }

        /// <summary>
        /// Xóa hết toàn bộ Effect item đang mặc
        /// </summary>
        public void ClearAllEffecEquipBody()
        {
            this.hpBeforeDetach = (double)this._Player.m_CurrentLife / (double)this._Player.m_CurrentLifeMax * 100;
            this.mpBeforeDetach = (double)this._Player.m_CurrentMana / (double)this._Player.m_CurrentManaMax * 100;
            this.staminaBeforeDetach = (double)this._Player.m_CurrentStamina / (double)this._Player.m_CurrentStaminaMax * 100;

            lock (EquipBody)
            {
                // For thực hiện DETRACK toàn bộ thuộc tính của người đi
                foreach (KeyValuePair<KE_EQUIP_POSITION, KItem> entry in EquipBody)
                {
                    KItem _Item = entry.Value;

                    // Nếu vật phẩm này có attack chỉ số trước đó thì ta mới thực hiện detack tránh lỗi âm máu
                    if (_Item.IsAttack)
                    {    // CODE ATTACK TOÀN BỘ THUỘC TÍNH CỦA ITEM VÀO NGƯỜI CHƠI
                        _Item.AttackEffect(this._Player, true);
                    }

                    //  Nếu đây là ngựa
                    if (entry.Key == KE_EQUIP_POSITION.emEQUIPPOS_HORSE)
                    {
                        // Nếu đang cưỡi ngựa thì detack toàn bộ thuộc tính của ngựa
                        if (this._Player.IsRiding)
                        {
                            _Item.AttachRiderEffect(this._Player, true);
                        }
                    }
                }

                /// Reset lại quan hàm
                this._Player.m_ChopLevel = 0;

                // Detack toàn bộ thuộc tính bộ khỏi người
                foreach (KMagicAttrib _KMagic in this.SuitUpActive)
                {
                    KTAttributesModifier.AttachProperty(_KMagic, _Player, true);
                }
            }

            // Xóa toàn bộ skill aura và skill hỗ trợ
            this._Player.DeactivateAuraPassiveAndEnchantSkills();
        }

        /// <summary>
        /// Xóa hết effect của pet đang mặc
        /// </summary>
        public void ClearAllEffecPetEquipBody()
        {
            // Lấy ra con pet hiện tại đang đeo
            Pet PET = this._Player.CurrentPet;

            if (PET != null)
            {
                lock (PetEquipBody)
                {
                    // For thực hiện DETRACK toàn bộ thuộc tính của người đi
                    foreach (KeyValuePair<KE_EQUIP_PET_POSTION, KItem> entry in PetEquipBody)
                    {
                        KItem _Item = entry.Value;
                        // CODE ATTACK TOÀN BỘ THUỘC TÍNH CỦA ITEM VÀO NGƯỜI CHƠI
                        _Item.AttackEffect(PET, true);
                    }
                }
                //NOtify lại chỉ số cho client bú lại khung
                PET.NotifyPetAttributes();
            }
        }

        /// <summary>
        /// Attack toàn bộ thuộc tính cho con PET
        /// </summary>
        public void AttackAllPetEquipBody()
        {
            Pet PET = this._Player.CurrentPet;

            if (PET != null)
            {
                lock (PetEquipBody)
                {
                    // Vòng for đầu tiên để kích hoạt các dòng ẩn của các trang bị đang mặc trước
                    //foreach (KeyValuePair<KE_EQUIP_PET_POSTION, KItem> entry in PetEquipBody)
                    //{
                    //    KItem _Item = entry.Value;

                    //    _Item.Update(this._Player);
                    //}
                    // Vòng for thứ 2 bắt đầu ATTACK các thuộc tính của dòng vào người
                    foreach (KeyValuePair<KE_EQUIP_PET_POSTION, KItem> entry in PetEquipBody)
                    {
                        KItem _Item = entry.Value;

                        // check xem pet có đủ khả năng mặc trang bị này không
                        if (this.CanUsingPetEquip(_Item._GoodDatas))
                        {
                            _Item.AttackEffect(PET, false);
                        }
                    }
                }

                //NOTIFY CHO PET KHI NÀO MẶC ĐỒ XONG
                PET.NotifyPetAttributes();
            }

            this.ReloadTotalValue();
        }

        /// <summary>
        /// Attack all effect của item đang mặc
        /// </summary>
        public void AttackAllEquipBody(bool RecovryWhenLogin = false)
        {
            lock (EquipBody)
            {
                bool active_all_ornament = false;
                bool active_suit = false;

                /// Create New list After Clear Effect
                this.SuitUpActive = new List<KMagicAttrib>();

                // Kích hoạt thuộc tính kích hoạt theo bộ nếu đang mặc 1 set
                List<SuiteActiveProp> TotalProbs = ItemManager.TotalSuiteActiveProp.ToList();

                foreach (SuiteActiveProp _Active in TotalProbs)
                {
                    int Active = 0;

                    int SuiteID = _Active.SuiteID;

                    foreach (KeyValuePair<KE_EQUIP_POSITION, KItem> entry in EquipBody)
                    {
                        KItem _Item = entry.Value;

                        if (_Item.SuiteID == SuiteID)
                        {
                            Active++;
                        }
                    }

                    // Nếu active lớn hơn 1

                    if (Active >= 1)
                    {
                        foreach (SuiteActive _SuteProbs in _Active.ListActive)
                        {
                            int NUMBER_REQUEST = _SuteProbs.RequestNum;

                            if (Active >= NUMBER_REQUEST)
                            {
                                KMagicAttrib Atribute = new KMagicAttrib();

                                Atribute.Init(_SuteProbs.SuiteName, _SuteProbs.SuiteMAPA1, _SuteProbs.SuiteMAPA2, _SuteProbs.SuiteMAPA3);

                                this.SuitUpActive.Add(Atribute);

                                if (_SuteProbs.SuiteName == "active_all_ornament")
                                {
                                    active_all_ornament = true;
                                }

                                if (_SuteProbs.SuiteName == "active_suit")
                                {
                                    active_suit = true;
                                }
                            }
                        }
                    }
                }

                // Vòng for đầu tiên để kích hoạt các dòng ẩn của các trang bị đang mặc trước
                foreach (KeyValuePair<KE_EQUIP_POSITION, KItem> entry in EquipBody)
                {
                    // Nếu đồ này đã hỏng thì bỏ qua đéo làm j cả
                    if (entry.Value._GoodDatas.Strong <= 0)
                    {
                        continue;
                    }

                    KItem _Item = entry.Value;

                    _Item.Update(this._Player, active_all_ornament, active_suit);
                }
                // Vòng for thứ 2 bắt đầu ATTACK các thuộc tính của dòng vào người
                foreach (KeyValuePair<KE_EQUIP_POSITION, KItem> entry in EquipBody.OrderByDescending(x => x.Value.InitLevel))
                {
                    // Nếu đồ này đã hỏng thì bỏ qua đéo làm j cả
                    if (entry.Value._GoodDatas.Strong <= 0)
                    {
                        continue;
                    }

                    KItem _Item = entry.Value;

                    // Nếu như có thể mặc được đồ này thì kích hoạt thuộc tính
                    if (this.CanUsingEquip(_Item._GoodDatas))
                    {
                        _Item.AttackEffect(this._Player, false);
                    }
                    else
                    {
                        //Set = False luôn là đéo attac được thuộc tính
                        _Item.IsAttack = false;
                    }

                    if (entry.Key == KE_EQUIP_POSITION.emEQUIPPOS_HORSE)
                    {
                        // Nếu đang cưỡi ngựa thì detack toàn bộ thuộc tính của ngựa
                        if (this._Player.IsRiding)
                        {
                            _Item.AttachRiderEffect(this._Player, false);
                        }
                    }
                }

                // attack toàn bộ thuộc tính của thuộc tính bộ vào
                foreach (KMagicAttrib _KMagic in this.SuitUpActive)
                {
                    KTAttributesModifier.AttachProperty(_KMagic, _Player, false);
                }

                /// Reset lại quan hàm
                this._Player.m_ChopLevel = 0;
                /// Quan hàm
                if (this.EquipBody.TryGetValue(KE_EQUIP_POSITION.emEQUIPPOS_CHOP, out KItem itemMgr))
                {
                    /// Thông tin vật phẩm tương ứng
                    GoodsData itemGD = itemMgr._GoodDatas;
                    /// Nếu tồn tại
                    if (itemGD != null)
                    {
                        /// Thông tin vật phẩm
                        ItemData itemData = ItemManager.GetItemTemplate(itemGD.GoodsID);
                        /// Nếu tồn tại
                        if (itemData != null)
                        {
                            /// Đánh dấu cấp độ quan hàm tương ứng
                            this._Player.m_ChopLevel = itemData.Level;
                        }
                    }
                }

                this.ReloadTotalValue();

                // Kích hoạt lại toàn bộ skill aura và skill hỗ tợ
                this._Player.ReactivateAuraPassiveAndEnchantSkills();

                if (RecovryWhenLogin)
                {
                    this.hpBeforeDetach = Global.GetRoleParamsInt32FromDB(this._Player, RoleParamName.CurHP);
                    //  SET LẠI CHỈ SỐ MÁU HIỆN TẠI CHO NHÂN VẬT

                    this.mpBeforeDetach = Global.GetRoleParamsInt32FromDB(this._Player, RoleParamName.CurMP);

                    //Stamina
                    this.staminaBeforeDetach = Global.GetRoleParamsInt32FromDB(this._Player, RoleParamName.CurStamina);
                }
                if (this.hpBeforeDetach > 0)
                {
                    this._Player.m_CurrentLife = (int)(((double)this._Player.m_CurrentLifeMax / 100) * this.hpBeforeDetach);
                }
                if (this.mpBeforeDetach > 0)
                {
                    this._Player.m_CurrentMana = (int)(((double)this._Player.m_CurrentManaMax / 100) * this.mpBeforeDetach);
                }
                if (this.staminaBeforeDetach > 0)
                {
                    this._Player.m_CurrentStamina = (int)(((double)this._Player.m_CurrentStaminaMax / 100) * this.staminaBeforeDetach);
                }
            }
        }

        /// <summary>
        /// Tính toán lại tổng tài phú từ trang bị
        /// </summary>
        public void ReloadTotalValue()
        {
            long TotalValue = 0;

            lock (EquipBody)
            {
                // Vòng for đầu tiên để kích hoạt các dòng ẩn của các trang bị đang mặc trước
                foreach (KeyValuePair<KE_EQUIP_POSITION, KItem> entry in EquipBody)
                {
                    KItem _Item = entry.Value;

                    long TaiPhu = 0;

                    StarLevelStruct _ItemData = ItemManager.ItemValueCalculation(_Item._GoodDatas, out TaiPhu, out int LinesCount);

                    TotalValue += TaiPhu;
                }

                // Tính tài phú cho vật phẩm của PET
                foreach (KeyValuePair<KE_EQUIP_PET_POSTION, KItem> entry in PetEquipBody)
                {
                    KItem _Item = entry.Value;

                    long TaiPhu = 0;

                    StarLevelStruct _ItemData = ItemManager.ItemValueCalculation(_Item._GoodDatas, out TaiPhu, out int LinesCount);

                    TotalValue += TaiPhu;
                }

                foreach (KeyValuePair<KE_EQUIP_POSITION_SUB, KItem> entry in PreventiveEquipBody)
                {
                    KItem _Item = entry.Value;

                    long TaiPhu = 0;

                    StarLevelStruct _ItemData = ItemManager.ItemValueCalculation(_Item._GoodDatas, out TaiPhu, out int LinesCount);

                    TotalValue += TaiPhu;
                }
                // Vòng for thứ 2 bắt đầu ATTACK các thuộc tính của dòng vào người

                this._Player.SetTotalValue(TotalValue);

                /// Notify về Client
                KT_TCPHandler.NotifySelfRoleValueChanged(this._Player, TotalValue);
            }
        }

        /// <summary>
        /// Kích hoạt hoặc hủy kích hoạt trạng thái cưỡi
        /// </summary>
        public bool IsCanUsing(int LEVEL, int FACTION, int SERIES, int SEX, List<KREQUIRE_ATTR> TotalRequest, int STR, int DEX, int VIT, int ENG)
        {
            if (TotalRequest == null)
            {
                return true;
            }

            foreach (KREQUIRE_ATTR Request in TotalRequest)
            {
                if (Request.eRequire == KE_ITEM_REQUIREMENT.emEQUIP_REQ_FACTION)
                {
                    if (FACTION != Request.nValue)
                    {
                        KTPlayerManager.ShowNotification(this._Player, "Trang bị chỉ có thể sử dụng cho phái :" + KFaction.GetName(Request.nValue));
                        return false;
                    }
                }
                if (Request.eRequire == KE_ITEM_REQUIREMENT.emEQUIP_REQ_LEVEL)
                {
                    if (LEVEL < Request.nValue)
                    {
                        KTPlayerManager.ShowNotification(this._Player, "Đại hiệp phải đạt cấp độ " + Request.nValue + " mới có thể sử dụng vật phẩm này");
                        return false;
                    }
                }

                if (Request.eRequire == KE_ITEM_REQUIREMENT.emEQUIP_REQ_STR)
                {
                    if (STR < Request.nValue)
                    {
                        KTPlayerManager.ShowNotification(this._Player, "Đại hiệp phải cộng đủ  " + Request.nValue + " sức mạnh mới có thể sử dụng");
                        return false;
                    }
                }

                if (Request.eRequire == KE_ITEM_REQUIREMENT.emEQUIP_REQ_DEX)
                {
                    if (DEX < Request.nValue)
                    {
                        KTPlayerManager.ShowNotification(this._Player, "Đại hiệp phải cộng đủ  " + Request.nValue + " thân pháp mới có thể sử dụng");
                        return false;
                    }
                }

                if (Request.eRequire == KE_ITEM_REQUIREMENT.emEQUIP_REQ_VIT)
                {
                    if (VIT != Request.nValue)
                    {
                        KTPlayerManager.ShowNotification(this._Player, "Đại hiệp phải cộng đủ  " + Request.nValue + " ngoại công mới có thể sử dụng");
                        return false;
                    }
                }

                if (Request.eRequire == KE_ITEM_REQUIREMENT.emEQUIP_REQ_ENG)
                {
                    if (ENG != Request.nValue)
                    {
                        KTPlayerManager.ShowNotification(this._Player, "Đại hiệp phải cộng đủ  " + Request.nValue + " nội công mới có thể sử dụng");
                        return false;
                    }
                }

                if (Request.eRequire == KE_ITEM_REQUIREMENT.emEQUIP_REQ_SERIES)
                {
                    if (SERIES != Request.nValue)
                    {
                        KTPlayerManager.ShowNotification(this._Player, "Trang bị yêu cầu ngũ hành: " + KTGlobal.GetSeriesText(Request.nValue) + " mới có thể sử dụng");
                        return false;
                    }
                }

                if (Request.eRequire == KE_ITEM_REQUIREMENT.emEQUIP_REQ_SEX)
                {
                    if (SEX != Request.nValue)
                    {
                        KTPlayerManager.ShowNotification(this._Player, "Trang bị yêu cầu giới tính : " + KTGlobal.SexToString(Request.nValue) + " mới có thể sử dụng");
                        return false;
                    }
                }
            }

            return true;
        }

        public long m_LastTicks = 0;

        /// <summary>
        /// Hỏng đồ
        /// </summary>
        public void InjuredSomebody()
        {
            long startTicks = TimeUtil.NOW();

            // Cứ 20s hỏng phát
            if (startTicks - m_LastTicks < 500000)
            {
                return;
            }

            m_LastTicks = startTicks;

            if (EquipBody.Count <= 0)
            {
                return;
            }

            // Nếu nhân vật có hơn 20 điểm may mắn
            if (this._Player.m_nCurLucky > 0)
            {
                int Random = new Random().Next(0, 100);

                // Nếu may thì không hỏng
                if (this._Player.m_nCurLucky * 2 > Random)
                {
                    return;
                }
            }

            KItem goodsData = null;

           
            int nRandom = KTGlobal.GetRandomNumber(0, (int)KE_EQUIP_POSITION.emEQUIPPOS_NUM - 1);

            // Nếu random vào mấy cái kia thì ko hỏng
            if ((KE_EQUIP_POSITION)nRandom == KE_EQUIP_POSITION.emEQUIPPOS_HORSE || (KE_EQUIP_POSITION)nRandom == KE_EQUIP_POSITION.emEQUIPPOS_MANTLE || (KE_EQUIP_POSITION)nRandom == KE_EQUIP_POSITION.emEQUIPPOS_ORNAMENT || (KE_EQUIP_POSITION)nRandom == KE_EQUIP_POSITION.emEQUIPPOS_SIGNET || (KE_EQUIP_POSITION)nRandom == KE_EQUIP_POSITION.emEQUIPPOS_BOOK || (KE_EQUIP_POSITION)nRandom == KE_EQUIP_POSITION.emEQUIPPOS_CHOP)
            {
                return;
            }
            lock (EquipBody)
            {
                this.EquipBody.TryGetValue((KE_EQUIP_POSITION)nRandom, out goodsData);
            }

            if (goodsData != null)
            {
                goodsData.Abrade(2, this._Player);
            }
            //GameManager.ClientMgr.AddEquipStrong(this._Player, goodsData._GoodDatas, 1);
        }

        /// <summary>
        /// Có thể dùng trang bị không
        /// </summary>
        /// <param name="goodsData"></param>
        /// <returns></returns>
        public bool CanUsingEquip(GoodsData goodsData)
        {
            if (goodsData == null)
            {
                return false;
            }

            KItem ItemFind = new KItem(goodsData);

            if (IsCanUsing(this._Player.m_Level, this._Player.m_cPlayerFaction.GetFactionId(), (int)this._Player.m_Series, this._Player.RoleSex, ItemFind.TotalRequest, this._Player.GetCurStrength(), this._Player.GetCurDexterity(), this._Player.GetCurVitality(), this._Player.GetCurEnergy()))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // TODO CHECK CHỈ SỐ
        public bool CanUsingPetEquip(GoodsData goodsData)
        {
            if (this._Player.CurrentPet == null)
            {
                return false;
            }

            if (goodsData == null)
            {
                return false;
            }

            KItem ItemFind = new KItem(goodsData);

            if (IsCanUsing(this._Player.CurrentPet.m_Level, -1, -1, -1, ItemFind.TotalRequest, this._Player.CurrentPet.Str, this._Player.CurrentPet.Dex, this._Player.CurrentPet.Sta, this._Player.CurrentPet.Int))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Lấy ra toàn bộ dánh sách itemID với res
        /// </summary>
        /// <returns></returns>
        public List<int> GetItemResID()
        {
            List<int> ItemList = new List<int>();

            ItemList.Add(this._Player.RoleSex);

            int ArmorID = -1;

            if (EquipBody.ContainsKey(KE_EQUIP_POSITION.emEQUIPPOS_BODY))
            {
                ArmorID = EquipBody[KE_EQUIP_POSITION.emEQUIPPOS_BODY]._ItemData.ItemID;
            }
            ItemList.Add(ArmorID);

            int HemlID = -1;

            if (EquipBody.ContainsKey(KE_EQUIP_POSITION.emEQUIPPOS_HEAD))
            {
                HemlID = EquipBody[KE_EQUIP_POSITION.emEQUIPPOS_HEAD]._ItemData.ItemID;
            }
            ItemList.Add(HemlID);

            int Mantal = -1;

            if (EquipBody.ContainsKey(KE_EQUIP_POSITION.emEQUIPPOS_MANTLE))
            {
                Mantal = EquipBody[KE_EQUIP_POSITION.emEQUIPPOS_MANTLE]._ItemData.ItemID;
            }
            ItemList.Add(Mantal);

            return ItemList;
        }

        public void RefreshEquip()
        {
            this.ClearAllEffecEquipBody();

            this.AttackAllEquipBody();
        }
    }
}