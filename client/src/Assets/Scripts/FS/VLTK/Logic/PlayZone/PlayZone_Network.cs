using System;
using System.Text;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using FS.Drawing;
using FS.GameEngine.Logic;
using FS.GameEngine.Network;
using FS.GameFramework.Logic;
using Server.Tools;
using Server.Data;
using FS.GameEngine.Sprite;
using Tmsk.Contract;
using FS.VLTK.Network.Skill;
using FS.VLTK.Network;
using FS.VLTK.Logic;
using FS.VLTK;
using static FS.VLTK.Entities.Enum;
using FS.VLTK.Loader;
using FS.VLTK.Entities.Config;
using FS.VLTK.Factory.UIManager;
using FS.VLTK.Factory;
using FS.VLTK.Utilities.Threading;
using FS.VLTK.Entities;
using HSGameEngine.GameEngine.Network;
using HSGameEngine.GameEngine.Network.Tools;
using FS.VLTK.Logic.Settings;

/// <summary>
/// Quản lý màn chơi - Tầng Mạng
/// </summary>
public partial class PlayZone
{
    #region Sự kiện mạng
    /// <summary>
    /// Sự kiện Socket kết nối thất bại
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void GameSocketFailed(object sender, SocketConnectEventArgs e)
    {
        MainGame.Instance.QueueOnMainThread(() =>
        {
            Super.HideNetWaiting();

            if (e.ReturnStartPage)
            {
                this.ReLoadGame(0);
            }
            else
            {
                /// Ngừng Auto
                AutoPathManager.Instance.StopAutoPath();
                AutoQuest.Instance.StopAutoQuest();
                /// Dừng StoryBoard
                Global.Data.Leader.StopMove();

                this.ShowUIDisconnected();
            }
        });
    }

    /// <summary>
    /// Sự kiện Socket kết nối thành công
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void GameSocketSuccess(object sender, SocketConnectEventArgs e)
    {
        MainGame.Instance.QueueOnMainThread(() =>
        {
            Super.HideNetWaiting();
            GameInstance.Game.InitPlayGame();
        });
    }

    /// <summary>
    /// Sự kiện bắt gói tin gửi về từ Server
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void GameSocketCommand(object sender, SocketConnectEventArgs e)
    {
        if (((TCPGameServerCmds) e.CmdID).ToString() == "CMD_SPR_CHECK" ||
                ((TCPGameServerCmds) e.CmdID).ToString() == "CMD_SPR_POSITION" ||
                ((TCPGameServerCmds) e.CmdID).ToString() == "CMD_SPR_USEGOODS" ||
                ((TCPGameServerCmds) e.CmdID).ToString() == "CMD_SPR_MOVE" ||
                ((TCPGameServerCmds) e.CmdID).ToString() == "CMD_SPR_UPDATE_ROLEDATA" ||
                ((TCPGameServerCmds) e.CmdID).ToString() == "CMD_SYNC_TIME_BY_CLIENT"
                )
        { }
        else
        {
            //KTDebug.Log("Receive packet from GS: " + (TCPGameServerCmds) e.CmdID);
        }

        if (e.CmdID == (int) (TCPGameServerCmds.CMD_SPR_CHECK))
        {
            FS.VLTK.UI.Main.MainUI.RolePart.UIRolePart_PingInfo.LastReceivePingTick = KTGlobal.GetCurrentTimeMilis();
        }

        /// Nếu là Packet thêm hoặc xóa nhóm đối tượng
        else if (e.CmdID == (int) (TCPGameServerCmds.CMD_NEW_OBJECTS))
        {
            /// Dữ liệu
            AddNewObjects data = DataHelper.BytesToObject<AddNewObjects>(e.bytesData, 0, e.bytesData.Length);

            MainGame.Instance.QueueOnMainThread(() =>
            {
                PlayZone.Instance.StartCoroutine(KT_TCPHandler.ReceiveNewObjects(data));
            });
        }
        else if (e.CmdID == (int) (TCPGameServerCmds.CMD_REMOVE_OBJECTS))
        {
            RemoveObjects data = DataHelper.BytesToObject<RemoveObjects>(e.bytesData, 0, e.bytesData.Length);

            MainGame.Instance.QueueOnMainThread(() =>
            {
                PlayZone.Instance.StartCoroutine(KT_TCPHandler.ReceiveRemoveObjects(data));
            });
        }
        else if (e.CmdID == (int) (TCPGameServerCmds.CMD_SPR_UPDATE_ROLEDATA))
        {
            SpriteLifeChangeData data = DataHelper.BytesToObject<SpriteLifeChangeData>(e.bytesData, 0, e.bytesData.Length);

            MainGame.Instance.QueueOnMainThread(() =>
            {
                this.scene.ServerUpdateRoleData(data);
            });
        }


        

        MainGame.Instance.QueueOnMainThread(() =>
        {
            long tick = KTGlobal.GetCurrentTimeMilis();
            this.ProcessNetCommandByUI(e);
            long processTick = KTGlobal.GetCurrentTimeMilis() - tick;

            //KTDebug.Log("Process " + (TCPGameServerCmds) e.CmdID + " - duration = " + processTick);
        });
    }

    #endregion

    #region Quản lý sự kiện
    /// <summary>
    /// Thực hiện lệnh bởi hệ thống
    /// </summary>
    /// <param name="e"></param>
    protected void ProcessNetCommandByUI(SocketConnectEventArgs e)
    {
        try
        {
            this.ProcessNetCommand(e);
        }
        catch (Exception ex)
        {
            KTDebug.LogException(ex);
        }
    }

    /// <summary>
    /// Gọi đến khi xảy ra lỗi
    /// </summary>
    /// <param name="result"></param>
    /// <param name="length"></param>
    /// <param name="msg"></param>
    /// <returns></returns>
    protected bool AssertNetworkException(bool result, int length, object msg = null)
    {
        if (result) return true;

        StringBuilder msgBuilder = new StringBuilder("AssertException: ");
        msgBuilder.Append(length);
        if (null != msg)
        {
            msgBuilder.Append(", Socket: ").Append(msg.ToString());
        }
        KTDebug.LogError(msgBuilder.ToString());

        return false;
    }

    /// <summary>
    /// Thực hiện xử lý các gói tin nhận được
    /// </summary>
    /// <param name="e"></param>
    protected void ProcessNetCommand(SocketConnectEventArgs e)
    {
        BasePlayZone.InWaitPingCount = 0;

        if (e.CmdID == (int)(TCPGameServerCmds.CMD_INIT_GAME))
        {
            Super.HideNetWaiting();
            RoleData roleData = DataHelper.BytesToObject<RoleData>(e.bytesData, 0, e.bytesData.Length);
            if (null == roleData)
            {
                Super.ShowMessageBox("Lỗi", "Không tìm thấy dữ liệu nhân vật. Hãy thử ấn OK và khởi động lại trò chơi.", () =>
                {
                    Application.Quit();
                });
                return;
            }
            if (roleData.RoleID < 0)
            {
                String op = "";
                if (-10 == roleData.RoleID)
                {
                    Super.ShowMessageBox("Lỗi", "Tài khoản đã bị khóa vĩnh viễn do vi phạm quy định của trò chơi.", () =>
                    {
                        Application.Quit();
                    });
                }
                else if (-70 == roleData.RoleID)
                {
                    Super.ShowMessageBox("Lỗi", "Tài khoản tạm thời bị khóa. Hãy liên lạc với BQT để được hỗ trợ.", () =>
                    {
                        Application.Quit();
                    });
                    return;
                }
                else if (-30 == roleData.RoleID)
                {
                    /*
                    if (Application.loadedLevelName != "MainGame")
                    {
                        this.ReLoadGame(0);
                        return;
                    }
                    */
                    op = "Mật khẩu cấp 2 chưa được kiểm chứng.";
                }
                else if (-40 == roleData.RoleID)
                {
                    return;
                }
                else if (-60 == roleData.RoleID)
                {
                    this.ReLoadGame(0);
                    return;
                }
                else
                {
                    op = "Khởi tạo dữ liệu đăng nhập thất bại.";
                }
                Super.ShowMessageBox("Lỗi", op, () =>
                {
                    Application.Quit();
                });
            }
            else
            {
                KuaFuLoginManager.OnChangeServerComplete();

                /// Nếu có kích hoạt Bug tốc độ di chuyển
                if (KTGlobal.EnableSpeedCheat)
                {
                    roleData.MoveSpeed = 200;
                }

                if (GameInstance.Game.CurrentSession.roleData != null && roleData.MapCode != GameInstance.Game.CurrentSession.roleData.MapCode)
                {
                    /// Gắn RoleData vào session hiện tại
                    GameInstance.Game.CurrentSession.roleData = roleData;

                    /// Xử lý thiết lập auto
                    KTAutoAttackSetting.SetAutoConfig();

                    /// Hiển thị màn tải bản đồ
                    this.ShowLoadingMap();
                }
                else
                {
                    /// Gắn RoleData vào session hiện tại
                    GameInstance.Game.CurrentSession.roleData = roleData;

                    /// Xử lý thiết lập auto
                    KTAutoAttackSetting.SetAutoConfig();
                    /// Bắt đầu Game
                    this.StartPlayGame();
                }
            }
        }
        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_PLAY_GAME))
        {
            scene.ServerPlayGame(Convert.ToInt32(e.fields[0]));

            this.OnChangeSceneUI();
            this.SetMainUIForScene();

            this.OnEnterGame();

            /// Gửi gói tin tải bản đồ thành công
            GameInstance.Game.SpriteEnterMap();
            /// Cập nhật thời điểm tải bản đồ xong
            KTGlobal.LastChangeMapSuccessfulTicks = KTGlobal.GetCurrentTimeMilis();
        }
        else if (e.CmdID == (int) (TCPGameServerCmds.CMD_SPR_UPDATE_DYNAMIC_OBJECT_LABELS))
        {
            /// Toác thì thôi
            if (Global.Data == null || Global.Data.GameScene == null || Global.Data.GameScene.CurrentMapData == null)
            {
                return;
            }

            /// Các nhãn tương ứng
            string[] labelStrings = e.fields;
            /// Xóa rỗng nhãn hiện tại
            Global.Data.GameScene.CurrentMapData.OpenedDynamicObsLabels.Clear();
            /// Duyệt danh sách nhãn
            foreach (string labelString in labelStrings)
            {
                try
                {
                    /// Nhãn tương ứng
                    byte label = byte.Parse(labelString);
                    /// Thêm vào danh sách
                    Global.Data.GameScene.CurrentMapData.OpenedDynamicObsLabels.Add(label);
                }
                catch (Exception) { }
            }
        }
        else if (e.CmdID == (int) (TCPGameServerCmds.CMD_SPR_STOPMOVE))
        {
            SpriteStopMove stopMove = DataHelper.BytesToObject<SpriteStopMove>(e.bytesData, 0, e.bytesData.Length);
            /// Thực hiện ngừng di chuyển cho đối tượng
            scene.ServerStopMove(stopMove.RoleID, stopMove.PosX, stopMove.PosY, stopMove.MoveSpeed, stopMove.Direction);
        }
        else if (e.CmdID == (int) (TCPGameServerCmds.CMD_SPR_LEVEL_CHANGED))
        {
            KT_TCPHandler.ReceiveRoleLevelChanged(e.fields);
        }
        else if (e.CmdID == (int)TCPGameServerCmds.CMD_KT_G2C_UPDATE_OTHERROLE_EQUIP)
        {
            RoleDataMini roleDataMini = DataHelper.BytesToObject<RoleDataMini>(e.bytesData, 0, e.bytesData.Length);
            if (roleDataMini == null)
            {
                return;
            }

            /// Người chơi tương ứng
            if (Global.Data.OtherRoles.TryGetValue(roleDataMini.RoleID, out RoleData rd))
            {
                /// Nếu không có danh sách trang bị thì tạo mới
                if (rd.GoodsDataList == null)
                {
                    rd.GoodsDataList = new List<GoodsData>();
                }

                /// Làm rỗng danh sách trang bị
                rd.GoodsDataList.Clear();

                static GoodsData GetItemGD(int itemID)
                {
                    if (Loader.Items.TryGetValue(itemID, out ItemData itemData))
                    {
                        return new GoodsData()
                        {
                            GoodsID = itemData.ItemID,
                            GCount = 1,
                            Forge_level = 0,
                        };
                    }
                    else
                    {
                        return null;
                    }
                }
                GoodsData weapon = GetItemGD(roleDataMini.WeaponID);
                if (weapon != null)
                {
                    weapon.Using = (int)KE_EQUIP_POSITION.emEQUIPPOS_WEAPON;
                    weapon.Forge_level = roleDataMini.WeaponEnhanceLevel;
                    weapon.Series = roleDataMini.WeaponSeries;
                    rd.GoodsDataList.Add(weapon);
                }
                GoodsData armor = GetItemGD(roleDataMini.ArmorID);
                if (armor != null)
                {
                    armor.Using = (int)KE_EQUIP_POSITION.emEQUIPPOS_BODY;
                    rd.GoodsDataList.Add(armor);
                }
                GoodsData helm = GetItemGD(roleDataMini.HelmID);
                if (helm != null)
                {
                    helm.Using = (int)KE_EQUIP_POSITION.emEQUIPPOS_HEAD;
                    rd.GoodsDataList.Add(helm);
                }
                GoodsData mantle = GetItemGD(roleDataMini.MantleID);
                if (mantle != null)
                {
                    mantle.Using = (int)KE_EQUIP_POSITION.emEQUIPPOS_MANTLE;
                    rd.GoodsDataList.Add(mantle);
                }
                GoodsData horse = GetItemGD(roleDataMini.HorseID);
                if (horse != null)
                {
                    horse.Using = (int) KE_EQUIP_POSITION.emEQUIPPOS_HORSE;
                    rd.GoodsDataList.Add(horse);
                }
                GoodsData mask = GetItemGD(roleDataMini.HorseID);
                if (mask != null)
                {
                    mask.Using = (int) KE_EQUIP_POSITION.emEQUIPPOS_MASK;
                    rd.GoodsDataList.Add(mask);
                }

                /// Thực hiện tải lại trang bị
                scene.ServerChangeCode(rd.RoleID);
            }
        }
        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_OTHER_ROLE))
        {
            RoleDataMini roleDataMini = DataHelper.BytesToObject<RoleDataMini>(e.bytesData, 0, e.bytesData.Length);

            /// Dữ liệu
            RoleData roleData = KTGlobal.RoleDataMiniToRoleData(roleDataMini);

            /// Toác
            if (roleData == null)
            {
                return;
            }

            /// Lưu vào danh sách
            Global.Data.OtherRoles[roleData.RoleID] = roleData;
            Global.Data.OtherRolesByName[roleData.RoleName] = roleData;

            scene.ToLoadOtherRole(roleData, roleData.PosX, roleData.PosY, roleData.RoleDirection);
        }
        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_OTHER_ROLE_DATA))
        {
            KT_TCPHandler.ReceiveGetOtherPlayerEquipInfo(e.CmdID, e.bytesData, e.bytesData.Length);
        }
        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_SPR_REALIVE))
        {
            MonsterRealiveData monsterRealiveData = DataHelper.BytesToObject<MonsterRealiveData>(e.bytesData, 0, e.bytesData.Length);
            if (null == monsterRealiveData)
            {
                return;
            }

            /// ID đối tượng
            int roleID = monsterRealiveData.RoleID;
            /// Vị trí X
            int posX = monsterRealiveData.PosX;
            /// Vị trí Y
            int posY = monsterRealiveData.PosY;
            /// Hướng quay
            int direction = monsterRealiveData.Direction;
            /// Ngũ hành
            int series = monsterRealiveData.Series;
            /// Sinh lực
            int hp = monsterRealiveData.CurrentHP;
            /// Nội lực
            int mp = monsterRealiveData.CurrentMP;
            /// Thể lực
            int stamina = monsterRealiveData.CurrentStamina;

            this.scene.ServerRealive(roleID, posX, posY, direction, series, hp, mp, stamina);

            /// Nếu là Leader
            if (Global.Data.RoleData.RoleID == roleID)
            {
                /// Đóng bảng hồi sinh
                this.CloseReviveFrame();
            }
            this.NotifyLeaderRoleFace(-1);
        }
        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_SYSTEM_MONSTER))
        {
            MonsterData monsterData = DataHelper.BytesToObject<MonsterData>(e.bytesData, 0, e.bytesData.Length);

            /// Toác
            if (monsterData == null)
            {
                return;
            }

            /// Lưu lại
            Global.Data.SystemMonsters[monsterData.RoleID] = monsterData;
            /// Nếu còn sống
            if (monsterData.HP > 0)
            {
                /// Tải xuống
                scene.ToLoadMonster(monsterData, monsterData.PosX, monsterData.PosY, monsterData.Direction);
            }
        }
        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_SPR_NEWTASK))
        {
            TaskData taskData = DataHelper.BytesToObject<TaskData>(e.bytesData, 0, e.bytesData.Length);
            if (null != taskData)
            {
                /// Thực thi hiệu ứng nhận nhiệm vụ
                KTGlobal.PlayRoleReceiveQuestEffect();

                if (taskData.DbID < 0)
                {
                    int ret = taskData.DbID;
                    if (-1000 == ret)
                    {
                        KTGlobal.AddNotification("Túi đồ đã đầy, không thể nhận nhiệm vụ!");
                    }
                    else
                    {
                        KTGlobal.AddNotification("Không thể nhận nhiệm vụ, hãy thử lại sau!");
                    }
                }
                else
                {
                    if (null == Global.Data.RoleData.TaskDataList)
                    {
                        Global.Data.RoleData.TaskDataList = new List<TaskData>();
                    }
                    Global.Data.RoleData.TaskDataList.Add(taskData);

                    if (this.UIMiniTaskAndTeamFrame != null)
                    {
                        this.UIMiniTaskAndTeamFrame.UIMiniTaskBox.AddNewTask(taskData);
                    }

                    if (this.UITaskBox != null)
                    {
                        this.UITaskBox.AddTask(taskData);
                    }
                }
            }
        }
        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_SPR_MAPCHANGE))
        {
            /// Đóng khung hồi sinh
            this.CloseReviveFrame();

            SCMapChange mapChangeData = DataHelper.BytesToObject<SCMapChange>(e.bytesData, 0, e.bytesData.Length);
            if (mapChangeData == null)
            {
                KTGlobal.AddNotification("Không thể chuyển bản đồ do dữ liệu truyền về bị lỗi!");
                return;
            }

            if (mapChangeData.ErrorCode == -1)
            {
                this.ServerMapConversion(mapChangeData.RoleID, mapChangeData.TeleportID, mapChangeData.MapCode, mapChangeData.PosX, mapChangeData.PosY, mapChangeData.Direction);
            }
            else
            {
                switch (mapChangeData.ErrorCode)
                {
                    case 0:
                        KTGlobal.AddNotification("Bản đồ hiện tại không hợp lệ.");
                        break;
                    case 1:
                        KTGlobal.AddNotification("Đường đi phía trước không thông.");
                        break;
                    case 2:
                        KTGlobal.AddNotification("Cấp độ không đủ.");
                        break;
                    case 3:
                        KTGlobal.AddNotification("Dữ liệu cổng dịch chuyển không tồn tại.");
                        break;
                    case 4:
                        KTGlobal.AddNotification("Không thể tiến nhập bản đồ phía trước.");
                        break;
                    default:
                        KTGlobal.AddNotification("Không thể tiến nhập bản đồ phía trước, lỗi chưa rõ!");
                        break;
                }
            }
        }
        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_SPR_EXECUTERECOMMENDPROPADDPOINT))
        {
            if (!AssertNetworkException(e.fields.Length == 2, e.fields.Length, (TCPGameServerCmds)e.CmdID))
            {
                return;
            }
            int result = Convert.ToInt32(e.fields[0]);
            int roleid = Convert.ToInt32(e.fields[1]);

            if (result == -1)
            {
                KTGlobal.AddNotification("Cộng điểm tiềm năng thất bại!");
            }
            else
            {
                KTGlobal.AddNotification("Cộng điểm tiềm năng thành công!");
                this.CloseUIRoleRemainPoint();
                this.ShowUIRoleRemainPoint();
            }
        }
        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_SPR_LEAVE))
        {
            scene.ServerRoleLeave(int.Parse(e.fields[0]));
        }
        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_SPR_NPC_BUY))
        {
            if (!AssertNetworkException(e.fields.Length == 3, e.fields.Length, (TCPGameServerCmds)e.CmdID))
            {
                return;
            }
            int ret = Convert.ToInt32(e.fields[0]);
            int moneyHaveNew = Convert.ToInt32(e.fields[1]);
            int buybackItemDbID = Convert.ToInt32(e.fields[2]);

            /// Nếu là mua lại
            if (ret == 1)
            {
                if (this.UIShop != null)
                {
                    this.UIShop.RemoveItemFromBuyBackList(buybackItemDbID);
                }
            }
        }
        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_SPR_NPC_SALEOUT))
        {
            G2C_PlayerSellItemToNPCShop playerSellItemToNPCShop = DataHelper.BytesToObject<G2C_PlayerSellItemToNPCShop>(e.bytesData, 0, e.bytesData.Length);
            if (playerSellItemToNPCShop == null)
            {
                return;
            }

            /// Cập nhật lại số bạc khóa hiện có
            Global.Data.RoleData.BoundMoney = playerSellItemToNPCShop.BoundMoneyHave;
            /// Thông báo tới các UI tương ứng
            UIMoneyManager.Instance.UpdateValue(MoneyType.BacKhoa);
            /// Nếu có thông báo vật phẩm vừa bán vào SHOP

            /// Nếu có khung SHOP và có vật phẩm mới có thể mua lại
            if (this.UIShop != null && playerSellItemToNPCShop.ItemGD != null)
            {
                this.UIShop.AddItemToBuyBack(playerSellItemToNPCShop.ItemGD);
            }
        }
        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_SPR_GET_STORE_MONEY))
        {
            /// ID nhân vật
            int roleID = Convert.ToInt32(e.fields[0]);
            /// Kết quả
            int result = Convert.ToInt32(e.fields[1]);

            /// Nếu không đủ bạc trong túi
            if (result == -1)
            {
                KTGlobal.AddNotification("Số bạc trong túi không đủ!");
                return;
            }
            /// Nếu số bạc vượt quá số trong thương khố
            else if (result == -2)
            {
                KTGlobal.AddNotification("Số bạc trong thương khố không đủ!");
                return;
            }
            /// Nếu số bạc trên người đã đạt tối đa
            else if (result == -3)
            {
                KTGlobal.AddNotification("Số bạc mang theo đã vượt quá giới hạn!");
                return;
            }
            /// Nếu rút thành công
            else if (result == 0)
            {
                KTGlobal.AddNotification("Rút bạc thành công!");
                return;
            }
            /// Nếu gửi thành công
            else if (result == 1)
            {
                KTGlobal.AddNotification("Gửi bạc thành công!");
                return;
            }
            /// Nếu mã lỗi gì đó
            else
            {
                KTGlobal.AddNotification(string.Format("Thao tác thất bại, mã lỗi: {0}!", result));
                return;
            }
        }
        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_SPR_STORE_MONEY_CHANGE))
        {
            /// ID nhân vật
            int roleID = Convert.ToInt32(e.fields[0]);
            /// Số bạc trong thương khố
            int storeMoney = Convert.ToInt32(e.fields[1]);

            /// Cập nhật lại số bạc trong thương khố hiện có
            Global.Data.RoleData.StoreMoney = storeMoney;

            /// Nếu đang hiện khung thương khố
            if (this.UIPortableBag != null)
            {
                /// Làm mới hiển thị số bạc trong thương khố
                this.UIPortableBag.RefreshStoreMoney();
            }
        }
        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_SPR_MOD_GOODS))
        {
            SCModGoods ModGoods = null;
            try
            {
                var tmp = new SCModGoods();
                tmp.fromBytes(e.bytesData, 0, e.bytesData.Length);
                ModGoods = tmp;
            }
            catch (Exception) { }

            if (!AssertNetworkException(ModGoods != null, e.CmdID))
            {
                return;
            }

            /// Kết quả trả về
            int ret = ModGoods.State;

            /// Nếu có kết quả trả về
            if (ret >= 0)
            {
                /// Vị trí
                int site = ModGoods.Site;

                /// Dữ liệu vật phẩm tại túi đồ
                GoodsData goodsData = Global.Data.RoleData.GoodsDataList?.Where(x => x.Id == ModGoods.ID).FirstOrDefault();

                /// Nếu có dữ liệu vật phẩm tại túi đồ
                if (goodsData != null)
                {
                    int newHint = ModGoods.NewHint;
                    /// Tổng số cũ
                    int oldGCount = goodsData.GCount;
                    /// Vị trí cũ trong túi
                    int oldBagIndex = goodsData.BagIndex;

                    /// Nếu số vật phẩm > 0, tức đổi vị trí
                    if (ModGoods.Count > 0)
                    {
                        /// Thay đổi vị trí nếu là trang bị trên người
                        goodsData.Using = ModGoods.IsUsing;
                        goodsData.Site = ModGoods.Site;
                        goodsData.GCount = ModGoods.Count;
                        /// Thay đổi vị trí nếu là đổi chỗ trong túi
                        goodsData.BagIndex = ModGoods.BagIndex;
                    }
                    /// Nếu số vật phẩm < 0 tức xóa
                    else
                    {
                        goodsData.GCount = 0;
                        /// Xóa vật phẩm khỏi túi
                        Global.Data.RoleData.GoodsDataList.Remove(goodsData);
                    }

                    /// Loại thao tác
                    int modType = ModGoods.ModType;
                    if (modType == (int)(ModGoodsTypes.Abandon) || modType == (int)(ModGoodsTypes.ModValue) || modType == (int)(ModGoodsTypes.Destroy) || modType == (int)(ModGoodsTypes.SaleToNpc))
                    {
                        if (newHint > 0 && goodsData.GCount > oldGCount)
                        {
                            int goodsCount = goodsData.GCount - oldGCount;
                            KTGlobal.HintNewGoodsText(goodsData, goodsCount);
                        }
                        UIBagManager.Instance.RefreshItem(goodsData);
                    }
                    /// Nếu là thay đổi trang bị trên người
                    else
                    {
                        if (modType == (int)(ModGoodsTypes.EquipLoad))
                        {
                            if (this.UIRoleInfo != null)
                            {
                                this.UIRoleInfo.RefreshRoleEquips();
                            }
                            /// Khóa luôn trang bị vào
                            goodsData.Binding = 1;
                            UIBagManager.Instance.RefreshItem(goodsData);
                            KTGlobal.AddNotification("Trang bị thành công!");
                            scene.ServerChangeCode(Global.Data.RoleData.RoleID);

                            /// Nếu đang mở khung Pet
                            if (PlayZone.Instance.UIPet != null)
                            {
                                /// Làm mới dữ liệu khung
                                PlayZone.Instance.UIPet.RefreshEquip();
                            }
                        }
                        else if (modType == (int)(ModGoodsTypes.EquipUnload))
                        {
                            if (this.UIRoleInfo != null)
                            {
                                this.UIRoleInfo.RefreshRoleEquips();
                            }
                            UIBagManager.Instance.RefreshItem(goodsData);
                            KTGlobal.AddNotification("Gỡ trang bị thành công!");
                            scene.ServerChangeCode(Global.Data.RoleData.RoleID);

                            /// Nếu đang mở khung Pet
                            if (PlayZone.Instance.UIPet != null)
                            {
                                /// Làm mới dữ liệu khung
                                PlayZone.Instance.UIPet.RefreshEquip();
                            }
                        }
                    }
                }
                /// Trường hợp tại thương khố
                else
                {
                    /// Lấy vật phẩm trong thương khố
                    goodsData = Global.Data.PortableGoodsDataList?.Where(x => x.Id == ModGoods.ID).FirstOrDefault();
                    /// Nếu tồn tại vật phẩm
                    if (goodsData != null)
                    {
                        /// Đánh dấu cập nhật UI túi đồ
                        bool updateUIBag = false;

                        /// Nếu số lượng > 0, tức đổi vị trí
                        if (ModGoods.Count > 0)
                        {
                            goodsData.Using = ModGoods.IsUsing;
                            goodsData.Site = ModGoods.Site;
                            goodsData.GCount = ModGoods.Count;
                            goodsData.BagIndex = ModGoods.BagIndex;

                            /// Nếu nằm ở túi đồ
                            if (goodsData.Site == 0)
                            {
                                updateUIBag = true;
                                /// Thêm vật phẩm vào túi
                                Global.Data.RoleData.GoodsDataList.Add(goodsData);
                                /// Xóa vật phẩm khỏi kho
                                Global.Data.PortableGoodsDataList.Remove(goodsData);

                                /// Cập nhật lại thông tin vật phẩm trong kho
                                if (this.UIPortableBag != null)
                                {
                                    this.UIPortableBag.BagGrid.RemoveItem(goodsData);
                                }
                            }
                        }
                        /// Nếu là thao tác xóa
                        else
                        {
                            /// Thiết lập số lượng 0
                            goodsData.GCount = 0;
                            /// Xóa vật phẩm khỏi kho
                            Global.Data.PortableGoodsDataList.Remove(goodsData);

                            /// Cập nhật lại thông tin vật phẩm trong kho
                            if (this.UIPortableBag != null)
                            {
                                this.UIPortableBag.BagGrid.RefreshItem(goodsData);
                            }
                        }

                        /// Nếu có đánh dấu cập nhật khung túi đồ
                        if (updateUIBag)
                        {
                            int modType = ModGoods.ModType;
                            /// Nếu thao tác là đổi vị trí
                            if (modType == (int)(ModGoodsTypes.ModValue))
                            {
                                UIBagManager.Instance.RefreshItem(goodsData);
                            }
                        }
                    }
                }
            }
            else if (ret == -1000)
            {
                KTGlobal.AddNotification("Túi đã đầy, không thể tháo trang bị!");
            }
            else if (-4 == ret)
            {
                //Super.HintMainText(Global.GetLang("当前精灵栏位已被占满"));
            }
            else if (-3 == ret)
            {
                KTGlobal.AddNotification("Túi đã đầy!");
            }
        }
        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_SPR_CHGCODE))
        {
            ChangeEquipData changeEquipData = DataHelper.BytesToObject<ChangeEquipData>(e.bytesData, 0, e.bytesData.Length);
            if (null != changeEquipData)
            {
                /// Dữ liệu nhân vật
                RoleData roleData = null;

                if (changeEquipData.RoleID == Global.Data.RoleData.RoleID)
                {
                    roleData = Global.Data.RoleData;
                }
                else
                {
                    if (Global.Data.OtherRoles.ContainsKey(changeEquipData.RoleID))
                    {
                        roleData = Global.Data.OtherRoles[changeEquipData.RoleID];
                    }
                }

                /// Nếu không tìm thấy
                if (roleData == null)
                {
                    return;
                }


                /// Nếu là thao tác mặc trang bị lên người
                if (changeEquipData.Type == -1)
                {
                    /// Trang bị tại vị trí tương ứng
                    GoodsData itemGD = roleData.GoodsDataList.Where(x => x.Using == changeEquipData.EquipGoodsData.Using).FirstOrDefault();
                    /// Nếu trang bị tồn tại
                    if (itemGD != null)
                    {
                        itemGD.GoodsID = changeEquipData.EquipGoodsData.GoodsID;
                        itemGD.Forge_level = changeEquipData.EquipGoodsData.Forge_level;
                        itemGD.Series = changeEquipData.EquipGoodsData.Series;
                    }
                    else
                    {
                        itemGD = new GoodsData()
                        {
                            GoodsID = changeEquipData.EquipGoodsData.GoodsID,
                            Forge_level = changeEquipData.EquipGoodsData.Forge_level,
                            Series = changeEquipData.EquipGoodsData.Series,
                            Using = changeEquipData.EquipGoodsData.Using,
                        };
                        roleData.GoodsDataList.Add(itemGD);
                    }
                }
                /// Nếu là thao tác tháo trang bị xuống
                else
                {
                    /// Trang bị tại vị trí tương ứng
                    GoodsData itemGD = roleData.GoodsDataList.Where(x => x.Using == changeEquipData.Type).FirstOrDefault();
                    if (itemGD != null)
                    {
                        roleData.GoodsDataList.Remove(itemGD);
                    }
                }

                /// Thực hiện tải lại trang bị
                scene.ServerChangeCode(changeEquipData.RoleID);

                if (this.UIRoleInfo != null && changeEquipData.RoleID == Global.Data.RoleData.RoleID)
                {
                    this.UIRoleInfo.RefreshRoleEquips();
                }
            }
        }
        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_SPR_MONEYCHANGE))
        {
            if (!AssertNetworkException(e.fields.Length == 4, e.fields.Length, (TCPGameServerCmds)e.CmdID))
            {
                return;
            }

            /// ID nhân vật
            int roleID = int.Parse(e.fields[0]);
            /// Bạc
            int Money = int.Parse(e.fields[1]);
            /// Bạc khóa
            int money2 = int.Parse(e.fields[2]);

            int GuildMoney = int.Parse(e.fields[3]);

            /// Nếu là ID nhân vật
            if (roleID == Global.Data.RoleData.RoleID)
            {
                int oldMoney = Global.Data.RoleData.Money;
                int oldMoney2 = Global.Data.RoleData.BoundMoney;

                if (Money > oldMoney)
                {
                    int nAddMoney = Money - oldMoney;
                    KTGlobal.ShowTextForExpMoneyOrGatherMakePoint(BottomTextDecorationType.Money, string.Format("Bạc +{0}", KTGlobal.GetDisplayMoney(nAddMoney)));

                    if (this.UIChatBoxMini != null)
                    {
                        this.UIChatBoxMini.AddMessage(new SpriteChat()
                        {
                            Channel = (int)ChatChannel.Default,
                            Content = string.Format("<color=red>Nhận được <color=yellow>{0} Bạc</color></color>", KTGlobal.GetDisplayMoney(nAddMoney)),
                        });
                    }
                }

                if (money2 > oldMoney2)
                {
                    int nAddMoney2 = money2 - oldMoney2;
                    KTGlobal.ShowTextForExpMoneyOrGatherMakePoint(BottomTextDecorationType.BoundMoney, string.Format("Bạc khóa +{0}", KTGlobal.GetDisplayMoney(nAddMoney2)));
                    this.UIChatBoxMini.AddMessage(new SpriteChat()
                    {
                        Channel = (int)ChatChannel.Default,
                        Content = string.Format("<color=red>Nhận được <color=yellow>{0} Bạc khóa</color></color>", KTGlobal.GetDisplayMoney(nAddMoney2)),
                    });
                }

                /// Thiết lập giá trị
                Global.Data.RoleData.Money = Money;
                Global.Data.RoleData.BoundMoney = money2;
                Global.Data.RoleData.GuildMoney = GuildMoney;

                UIMoneyManager.Instance.UpdateValue(MoneyType.Bac);
                UIMoneyManager.Instance.UpdateValue(MoneyType.BacKhoa);
                UIMoneyManager.Instance.UpdateValue(MoneyType.GuildMoney);
            }
        }
        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_SPR_MODTASK))
        {
            if (!AssertNetworkException(e.fields.Length == 5, e.fields.Length, (TCPGameServerCmds)e.CmdID))
            {
                return;
            }
            int dbID = Convert.ToInt32(e.fields[0]);
            int taskID = Convert.ToInt32(e.fields[1]);

            /// Thông tin nhiệm vụ
            TaskData taskData = KTGlobal.GetTaskData(dbID);

            /// Nếu nhiệm vụ không tồn tại
            if (!Loader.Tasks.TryGetValue(taskData.DoingTaskID, out TaskDataXML taskDataXML))
            {
                return;
            }

            if (taskData != null)
            {
                /// Trạng thái đã hoàn thành chưa
                taskData.DoingTaskVal1 = Convert.ToInt32(e.fields[2]);
                taskData.DoingTaskVal2 = Convert.ToInt32(e.fields[3]);
                taskData.DoingTaskFocus = Convert.ToInt32(e.fields[4]);

                /// Nếu là xóa nhiệm vụ
                if (taskData.DoingTaskVal1 == -1 || taskData.DoingTaskVal1 == -2)
                {
                    if (this.UIMiniTaskAndTeamFrame != null)
                    {
                        this.UIMiniTaskAndTeamFrame.UIMiniTaskBox.RemoveTask(taskData);
                    }

                    Global.Data.RoleData.TaskDataList.Remove(taskData);

                    /// Nếu là thông báo hoàn thành nhiệm vụ
                    if (taskData.DoingTaskVal1 == -2)
                    {
                        KTGlobal.PlayRoleCompleteQuestEffect();

                        /// Đánh dấu đã hoàn thành nhiệm vụ tương ứng
                        QuestInfo qInfo = Global.Data.RoleData.QuestInfo.Where(x => x.TaskClass == taskDataXML.TaskClass).FirstOrDefault();
                        if (qInfo != null)
                        {
                            /// Đánh dấu nhiệm vụ hoàn thành gần nhất
                            qInfo.CurTaskIndex = taskData.DoingTaskID;

                            /// Thêm nhiệm vụ vào danh sách đã hoàn thành
                            Global.Data.CompletedTasks[taskDataXML.TaskClass].Add(taskData.DoingTaskID);
                        }

                        /// Cập nhật dữ liệu cho khung nhiệm vụ
                        if (this.UITaskBox != null)
                        {
                            this.UITaskBox.CompleteTask(taskData);
                        }
                    }
                    /// Nếu là hủy nhiệm vụ
                    else
                    {
                        KTGlobal.AddNotification("Hủy nhiệm vụ thành công!");

                        /// Cập nhật dữ liệu cho khung nhiệm vụ
                        if (this.UITaskBox != null)
                        {
                            this.UITaskBox.DoAbandonTask(taskData);
                        }
                    }
                }
                else
                {
                    if (this.UIMiniTaskAndTeamFrame != null)
                    {
                        this.UIMiniTaskAndTeamFrame.UIMiniTaskBox.UpdateTask(taskData);
                    }

                    if (this.UITaskBox != null)
                    {
                        this.UITaskBox.UpdateTask(taskData);
                    }
                }
            }
        }
        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_SPR_ADD_GOODS))
        {
            AddGoodsData addGoodsData = DataHelper.BytesToObject<AddGoodsData>(e.bytesData, 0, e.bytesData.Length);
            if (null == addGoodsData)
            {
                return;
            }

            int ret = addGoodsData.ID;
            if (Global.Data.RoleData.RoleID == addGoodsData.RoleID && ret >= 0)
            {
                /// Nếu là trong túi đồ
                if (addGoodsData.Site == 0)
                {
                    string endTime = addGoodsData.NewEndTime;
                    endTime = Global.StringReplaceAll(endTime, "$", ":");
                    GoodsData goodsData = new GoodsData()
                    {
                        Id = ret,
                        GoodsID = addGoodsData.GoodsID,
                        Using = addGoodsData.Using,
                        Forge_level = addGoodsData.ForgeLevel,
                        Starttime = "1900-01-01 12:00:00",
                        Endtime = endTime,
                        Site = addGoodsData.Site,
                        Props = addGoodsData.Props,
                        GCount = addGoodsData.GoodsNum,
                        Binding = addGoodsData.Binding,
                        Strong = addGoodsData.Strong,
                        BagIndex = addGoodsData.BagIndex,
                        Series = addGoodsData.Series,
                        OtherParams = addGoodsData.OtherParams,
                    };

                    if (Global.Data.RoleData.GoodsDataList == null)
                    {
                        Global.Data.RoleData.GoodsDataList = new List<GoodsData>();
                    }

                    Global.Data.RoleData.GoodsDataList.Add(goodsData);
                    int hint = addGoodsData.NewHint;
                    if (hint > 0)
                    {
                        int goodsCount = addGoodsData.GoodsNum;
                        KTGlobal.HintNewGoodsText(goodsData, goodsCount);
                    }

                    UIBagManager.Instance.AddItem(goodsData);


                }
                /// Nếu trong thương khố
                else if (addGoodsData.Site == 1)
                {
                    string endTime = addGoodsData.NewEndTime;
                    endTime = Global.StringReplaceAll(endTime, "$", ":");
                    GoodsData goodsData = new GoodsData();
                    {
                        goodsData.Id = ret;
                        goodsData.GoodsID = addGoodsData.GoodsID;
                        goodsData.Using = addGoodsData.Using;
                        goodsData.Forge_level = addGoodsData.ForgeLevel;
                        goodsData.Starttime = "1900-01-01 12:00:00";
                        goodsData.Endtime = endTime;
                        goodsData.Site = addGoodsData.Site;
                        goodsData.Props = addGoodsData.Props;
                        goodsData.GCount = addGoodsData.GoodsNum;
                        goodsData.Binding = addGoodsData.Binding;
                        goodsData.Strong = addGoodsData.Strong;
                        goodsData.BagIndex = addGoodsData.BagIndex;
                        goodsData.Series = addGoodsData.Series;
                        goodsData.OtherParams = addGoodsData.OtherParams;
                    }

                    if (Global.Data.PortableGoodsDataList == null)
                    {
                        Global.Data.PortableGoodsDataList = new List<GoodsData>();
                    }

                    Global.Data.PortableGoodsDataList.Add(goodsData);
                    int hint = addGoodsData.NewHint;
                    if (hint > 0)
                    {
                        int goodsCount = addGoodsData.GoodsNum;
                        KTGlobal.HintNewGoodsText(goodsData, goodsCount);
                    }

                    /// Nếu đang hiện khung thương khố
                    if (this.UIPortableBag != null)
                    {
                        this.UIPortableBag.BagGrid.AddItem(goodsData);
                    }
                }
            }
        }
        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_SPR_EXPCHANGE))
        {


            if (!AssertNetworkException(e.fields.Length >= 4, e.fields.Length, (TCPGameServerCmds)e.CmdID))
            {
                return;
            }

            int oldLevel = Global.Data.RoleData.Level;
            long oldExp = Global.Data.RoleData.Experience;
            int roleID = Convert.ToInt32(e.fields[0]);
            int level = (int)Convert.ToInt64(e.fields[1]);
            long exp = Convert.ToInt64(e.fields[2]);
            long nextLevelExp = Convert.ToInt64(e.fields[3]);

            if (Global.Data.RoleData.RoleID == roleID)
            {
                Global.Data.RoleData.Level = level;
                Global.Data.RoleData.Experience = exp;
                Global.Data.RoleData.MaxExperience = nextLevelExp;
                this.RefreshLeaderExpAndLevel();
            }


           
            /// Thông báo cho UI đối tượng đang theo dõi
            this.NotifyRoleFace(roleID, -1);

            
            /// Nếu giá trị kinh nghiệm thay đổi
            if (exp - oldExp > 0)
            {
                long nAdd = exp - oldExp;
                KTGlobal.ShowTextForExpMoneyOrGatherMakePoint(FS.VLTK.Entities.Enum.BottomTextDecorationType.Exp, string.Format("Kinh nghiệm +{0}", nAdd));

                if (this.UIChatBoxMini != null)
                {
                    this.UIChatBoxMini.AddMessage(new SpriteChat()
                    {
                        Channel = (int)ChatChannel.Default,
                        Content = string.Format("<color=red>Nhận được <color=yellow>{0} Kinh nghiệm</color></color>", nAdd),
                    });
                }
            }

            /// Nếu cấp độ thay đổi
            if (level != oldLevel)
            {
                if (this.UIChatBoxMini != null)
                {
                    this.UIChatBoxMini.AddMessage(new SpriteChat()
                    {
                        Channel = (int)ChatChannel.Default,
                        Content = string.Format("<color=red>Thăng cấp thành công lên <color=yellow>{0}</color></color>", level),
                    });
                }
                /// Thực hiện hiệu ứng nhân vật thăng cấp
                KTGlobal.PlayRoleLevelUpEffect(Global.Data.Leader);
            }
        }
        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_SPR_GETFRIENDS))
        {
            Global.Data.FriendDataList = DataHelper.BytesToObject<List<FriendData>>(e.bytesData, 0, e.bytesData.Length);
            /// Hiện khung bạn bè
            this.ShowUIFriendBox();
        }
        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_SPR_ADDFRIEND))
        {
            FriendData friendData = DataHelper.BytesToObject<FriendData>(e.bytesData, 0, e.bytesData.Length);

            if (friendData.DbID < 0)
            {
                /// Làm mới hiển thị
                if (this.UIFriendBox != null)
                {
                    this.UIFriendBox.Refresh();
                }

                /// Nếu người chơi không tồn tại
                if (friendData.DbID == -10000)
                {
                    KTGlobal.AddNotification("Người chơi không tồn tại!");
                }
                /// Nếu nhóm đã đầy
                else if (friendData.DbID == -10001)
                {
                    string friendTypeName = "";
                    if (friendData.FriendType == 0)
                    {
                        friendTypeName = "Hảo hữu";
                    }
                    else if (friendData.FriendType == 1)
                    {
                        friendTypeName = "Chặn";
                    }
                    else
                    {
                        friendTypeName = "Kẻ thù";
                    }

                    KTGlobal.AddNotification(string.Format("Danh sách {0} đã đầy!", friendTypeName));
                }
            }
            else
            {
                /// Nếu danh sách bạn bè đang NULL
                if (null == Global.Data.FriendDataList)
                {
                    Global.Data.FriendDataList = new List<FriendData>();
                }

                /// ID trong danh sách cũ
                int findIndex = -1;
                for (int i = 0; i < Global.Data.FriendDataList.Count; i++)
                {
                    if (Global.Data.FriendDataList[i].DbID == friendData.DbID)
                    {
                        findIndex = i;
                        break;
                    }
                }

                /// Nếu tìm thấy thì cập nhật thông tin
                if (findIndex >= 0)
                {
                    Global.Data.FriendDataList[findIndex] = friendData;
                }
                /// Nếu chưa tìm thấy thì thêm mới
                else
                {
                    Global.Data.FriendDataList.Add(friendData);

                    /// Xóa khỏi danh sách yêu cầu
                    if (Global.Data.AskToBeFriendList != null)
                    {
                        Global.Data.AskToBeFriendList.RemoveAll(rd => rd.RoleID == friendData.OtherRoleID);
                        /// Nếu đang hiển thị khung
                        if (this.UIFriendBox != null)
                        {
                            this.UIFriendBox.RefreshAskToBeFriendList();
                        }
                    }

                }

                /// Nếu đang hiển thị khung
                if (this.UIFriendBox != null)
                {
                    this.UIFriendBox.Refresh();
                }

                string friendTypeName = "";
                if (friendData.FriendType == 0)
                {
                    friendTypeName = "Hảo hữu";
                }
                else if (friendData.FriendType == 1)
                {
                    friendTypeName = "Chặn";
                }
                else
                {
                    friendTypeName = "Kẻ thù";
                }

                /// Thông báo
                KTGlobal.AddNotification(string.Format("Thêm {0} vào danh sách {1} thành công!", friendData.OtherRoleName, friendTypeName));
            }
        }
        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_SPR_REMOVEFRIEND))
        {
            if (!AssertNetworkException(e.fields.Length == 3, e.fields.Length, (TCPGameServerCmds)e.CmdID))
            {
                return;
            }

            int dbID = Convert.ToInt32(e.fields[0]);
            string otherRoleName = e.fields[1];

            if (null != Global.Data.FriendDataList)
            {
                /// Tìm bạn tương ứng ở danh sách và xóa
                for (int i = 0; i < Global.Data.FriendDataList.Count; i++)
                {
                    if (Global.Data.FriendDataList[i].DbID == dbID)
                    {
                        Global.Data.FriendDataList.RemoveAt(i);
                    }
                }
            }

            if (this.UIFriendBox != null)
            {
                this.UIFriendBox.Refresh();
            }

            /// Thông báo
            KTGlobal.AddNotification(string.Format("Xóa quan hệ với {0}!", otherRoleName));
        }
        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_SPR_ASKFRIEND))
        {
            RoleDataMini rd = DataHelper.BytesToObject<RoleDataMini>(e.bytesData, 0, e.bytesData.Length);
            if (rd == null)
            {
                return;
            }

            /// Thông báo có người muốn thêm bạn
            KTGlobal.AddNotification(string.Format("{0} muốn kết hảo hữu với bạn!", rd.RoleName));
            /// Nếu chưa tồn tại ở danh sách cũ
            if (!Global.Data.AskToBeFriendList.Any(x => x.RoleID == rd.RoleID))
            {
                /// Thêm vào danh sách
                Global.Data.AskToBeFriendList.Add(rd);
            }

            /// Nếu đang mở khung
            if (this.UIFriendBox != null)
            {
                /// Cập nhật UI
                this.UIFriendBox.RefreshAskToBeFriendList();
            }
        }
        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_SPR_REJECTFRIEND))
        {
            if (!AssertNetworkException(e.fields.Length == 2, e.fields.Length, (TCPGameServerCmds)e.CmdID))
            {
                return;
            }

            /// ID đối tượng từ chối
            int roleID = Convert.ToInt32(e.fields[0]);

            /// Nếu là bản thân
            if (roleID == Global.Data.RoleData.RoleID)
            {
                /// ID đối tượng bị từ chối
                int otherRoleID = Convert.ToInt32(e.fields[1]);

                /// Thông tin người chơi ở danh sách cũ
                RoleDataMini rd = Global.Data.AskToBeFriendList.Where(x => x.RoleID == otherRoleID).FirstOrDefault();
                /// Nếu tìm thấy
                if (rd != null)
                {
                    /// Xóa khỏi danh sách
                    Global.Data.AskToBeFriendList.Remove(rd);
                }

                /// Nếu đang mở khung
                if (this.UIFriendBox != null)
                {
                    /// Cập nhật UI
                    this.UIFriendBox.RefreshAskToBeFriendList();
                }
            }
            else
            {
                /// Tên đối tượng từ chối
                string roleName = e.fields[1];

                /// Thông báo
                KTGlobal.AddNotification(string.Format("{0} từ chối lời mời kết bạn của bạn.", roleName));
            }
        }
        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_SPR_NEWGOODSPACK))
        {
            NewGoodsPackData newGoodsPackData = DataHelper.BytesToObject<NewGoodsPackData>(e.bytesData, 0, e.bytesData.Length);
            if (null == newGoodsPackData)
            {
                return;
            }

            scene.ServerNewGoodsPack(newGoodsPackData);
        }
        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_SPR_DELGOODSPACK))
        {
            scene.ServerDelGoodsPack(Convert.ToInt32(e.fields[0]));
        }
        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_SPR_CHGPKMODE))
        {
            if (!AssertNetworkException(e.fields.Length == 3, e.fields.Length, (TCPGameServerCmds)e.CmdID))
            {
                return;
            }
            int roleID = Convert.ToInt32(e.fields[0]);
            int pkMode = Convert.ToInt32(e.fields[1]);
            int camp = Convert.ToInt32(e.fields[2]);

            /// Cập nhật trạng thái PK
            scene.ServerChangePKMode(roleID, pkMode, camp);

            /// Cập nhật dữ liệu lên UI
            if (this.UIRolePart != null)
            {
                if (Global.Data.RoleData.RoleID == roleID)
                {
                    this.UIRolePart.PKMode = pkMode;
                }
            }
        }
        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_SPR_CHGPKVAL))
        {
            if (!AssertNetworkException(e.fields.Length == 2, e.fields.Length, (TCPGameServerCmds)e.CmdID))
            {
                return;
            }
            int roleID = Convert.ToInt32(e.fields[0]);
            int pkValue = Convert.ToInt32(e.fields[1]);
            scene.ServerChangePKValue(roleID, pkValue);

            /// Nếu là Leader
            if (roleID == Global.Data.RoleData.RoleID)
            {
                if (this.UIRoleInfo != null)
                {
                    this.UIRoleInfo.UpdateRoleData();
                }
            }
        }
        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_SPR_UPDATENPCSTATE))
        {
            int npcID = Convert.ToInt32(e.fields[0]);
            int taskState = Convert.ToInt32(e.fields[1]);

            NPCTaskState state = Global.Data.RoleData.NPCTaskStateList?.Where(x => x != null && x.NPCID == npcID).FirstOrDefault();
            if (state != null)
            {
                state.TaskState = taskState;

                /// Nếu đang hiện khung bản đồ khu vực
                if (this.UILocalMap != null && this.UILocalMap.Visible)
                {
                    this.UILocalMap.RefreshNPCTaskStates();
                }
            }

            scene.ServerUpdateNPCTaskState(npcID, taskState);
        }
        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_SPR_CHAT))
        {
            SpriteChat chat = DataHelper.BytesToObject<SpriteChat>(e.bytesData, 0, e.bytesData.Length);
            if (chat == null)
            {
                return;
            }
            chat.TickTime = KTGlobal.GetCurrentTimeMilis();

            if (this.UIChat != null)
            {
                this.UIChat.AddMessage(chat);
            }
            else
            {
                FS.VLTK.UI.Main.UIChat.AddMessageWithoutNotify(chat);
            }

            if (this.UIChatBoxMini != null)
            {
                this.UIChatBoxMini.AddMessage(chat);
            }

            /// Nếu là kênh chat đặc biệt
            if (chat.Channel == (int)ChatChannel.Special)
            {
                /// Hiện nội dung ở kênh đặc biệt
                this.UISpecialChatBox.Content = string.Format("<color=#61bff5>[{0}]</color>: {1}", chat.FromRoleName, chat.Content);
                /// Hiện khung
                this.UISpecialChatBox.Visible = true;
            }
            /// Nếu là kênh chat lân cận hoặc tổ đội
            else if (chat.Channel == (int) ChatChannel.Near || chat.Channel == (int) ChatChannel.Team)
            {
                /// Đối tượng tương ứng
                GSprite sprite = KTGlobal.FindSpriteByID(chat.FromRoleID);
                /// Nếu tìm thấy
                if (sprite != null)
                {
                    sprite.ComponentCharacter?.ShowChat(KTGlobal.DisplayPlayerHeaderChat(chat));
                }
            }

            /// Nếu là dòng chữ chạy ngang phía trên
            if (chat.Channel == (int)ChatChannel.System_Broad_Chat)
            {
                this.AddSystemNotification(chat.Content);
            }
        }
        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_SPR_CHANGEPOS))
        {
            int roleID = Convert.ToInt32(e.fields[0]);
            if (roleID == Global.Data.RoleData.RoleID)
            {
                /// Dừng thu thập
                this.HideProgressBar();
            }

            scene.ServerChangePos(Convert.ToInt32(e.fields[0]), Convert.ToInt32(e.fields[1]), Convert.ToInt32(e.fields[2]), Convert.ToInt32(e.fields[3]));
        }
        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_SPR_NOTIFYCHGMAP))
        {
            if (!AssertNetworkException(e.fields.Length == 6, e.fields.Length, (TCPGameServerCmds)e.CmdID))
            {
                return;
            }
            MapConversion(Convert.ToInt32(e.fields[1]), Convert.ToInt32(e.fields[2]), Convert.ToInt32(e.fields[3]), Convert.ToInt32(e.fields[4]), Convert.ToInt32(e.fields[5]));
        }
        // Gói tin báo cho người chơi biết đang bị người chơi khác tấn công KNB thơi FORCE AUTO tấn công lại nếu dang bật tự phản kháng
        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_KT_TAKEDAMAGE))
        {
           
            if (!AssertNetworkException(e.fields.Length == 2, e.fields.Length, (TCPGameServerCmds)e.CmdID))
            {
           
                return;
            }

            if (Convert.ToInt32(e.fields[0]) == Global.Data.RoleData.RoleID)
            {

              
                int AttackRoleID = Convert.ToInt32(e.fields[1]);
            
                KTAutoFightManager.Instance.ForceTaget(AttackRoleID);
            }
        }
        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_SPR_FORGE))
        {
            if (!AssertNetworkException(e.fields.Length == 5, e.fields.Length, (TCPGameServerCmds)e.CmdID))
            {
                return;
            }

            if (Convert.ToInt32(e.fields[1]) == Global.Data.RoleData.RoleID)
            {
                int result = Convert.ToInt32(e.fields[0]);
                int dbID = Convert.ToInt32(e.fields[2]);
                int forgeLevel = Convert.ToInt32(e.fields[3]);
                int binding = Convert.ToInt32(e.fields[4]);

                GoodsData itemGD = Global.Data.RoleData.GoodsDataList?.Where(x => x.Id == dbID).FirstOrDefault();
                if (null != itemGD)
                {
                    itemGD.Forge_level = forgeLevel;
                    itemGD.Binding = binding;

                    if (result > 0)
                    {
                        UIBagManager.Instance.RefreshItem(itemGD);
                        if (this.UIRoleInfo != null && itemGD.Using >= 0)
                        {
                            this.UIRoleInfo.RefreshRoleEquips();
                        }
                    }
                }
            }
        }
        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_SPR_TokenCHANGE))
        {
            if (!AssertNetworkException(e.fields.Length == 3, e.fields.Length, (TCPGameServerCmds)e.CmdID))
            {
                return;
            }

            /// ID nhân vật
            int roleID = int.Parse(e.fields[0]);
            /// KNB
            int token = int.Parse(e.fields[1]);
            /// KNB khóa
            int boundToken = int.Parse(e.fields[2]);

            /// Nếu là ID nhân vật
            if (roleID == Global.Data.RoleData.RoleID)
            {
                int oldToken = Global.Data.RoleData.Token;
                int oldBoundToken = Global.Data.RoleData.BoundToken;

                if (token > oldToken)
                {
                    int nAddMoney = token - oldToken;
                    KTGlobal.ShowTextForExpMoneyOrGatherMakePoint(BottomTextDecorationType.Coupon, string.Format("KNB +{0}", KTGlobal.GetDisplayMoney(nAddMoney)));

                    if (this.UIChatBoxMini != null)
                    {
                        this.UIChatBoxMini.AddMessage(new SpriteChat()
                        {
                            Channel = (int)ChatChannel.Default,
                            Content = string.Format("<color=red>Nhận được <color=yellow>{0} KNB</color></color>", KTGlobal.GetDisplayMoney(nAddMoney)),
                        });
                    }
                }

                if (boundToken > oldBoundToken)
                {
                    int nAddMoney2 = boundToken - oldBoundToken;
                    KTGlobal.ShowTextForExpMoneyOrGatherMakePoint(BottomTextDecorationType.Coupon_Bound, string.Format("KNB khóa +{0}", KTGlobal.GetDisplayMoney(nAddMoney2)));

                    if (this.UIChatBoxMini != null)
                    {
                        this.UIChatBoxMini.AddMessage(new SpriteChat()
                        {
                            Channel = (int)ChatChannel.Default,
                            Content = string.Format("<color=red>Nhận được <color=yellow>{0} KNB khóa</color></color>", KTGlobal.GetDisplayMoney(nAddMoney2)),
                        });
                    }
                }

                /// Thiết lập giá trị
                Global.Data.RoleData.Token = token;
                Global.Data.RoleData.BoundToken = boundToken;

                UIMoneyManager.Instance.UpdateValue(MoneyType.Dong);
                UIMoneyManager.Instance.UpdateValue(MoneyType.DongKhoa);
            }
        }
        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_SPR_GOODSEXCHANGE))
        {
            if (!AssertNetworkException(e.fields.Length >= 3, e.fields.Length, (TCPGameServerCmds)e.CmdID))
            {
                return;
            }
            this.ProcessGoodsExchange(e.fields);
        }
        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_SPR_EXCHANGEDATA))
        {
            Global.Data.ExchangeDataItem = DataHelper.BytesToObject<ExchangeData>(e.bytesData, 0, e.bytesData.Length);
            if (Global.Data.ExchangeDataItem != null)
            {
                if (this.UIExchange != null)
                {
                    this.UIExchange.RefreshExchangeData();
                }
            }
        }
        else if ((int)TCPGameServerCmds.CMD_SPR_KF_SWITCH_SERVER == e.CmdID)
        {
            KuaFuServerLoginData kuaFuServerLoginData = DataHelper.BytesToObject<KuaFuServerLoginData>(e.bytesData, 0, e.bytesData.Length);
            if (!AssertNetworkException(kuaFuServerLoginData != null, e.bytesData.Length, (TCPGameServerCmds)e.CmdID))
            {
                return;
            }

            if (kuaFuServerLoginData.RoleId > 0)
            {
                KuaFuLoginManager.ChangeToKuaFuServer(kuaFuServerLoginData);
            }
            else
            {
                KuaFuLoginManager.ChangeToOriginalServer();
            }

            /// Thực hiện Reload Game với type là đổi máy chủ liên SV
            this.ReLoadGame((int)ReConnectType.ChangeServer);
        }
        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_SPR_MOVEGOODSDATA))
        {
            if (!AssertNetworkException(e.fields.Length == 2, e.fields.Length, (TCPGameServerCmds)e.CmdID))
            {
                return;
            }
            int roleID = Convert.ToInt32(e.fields[0]);
            int goodsDbID = Convert.ToInt32(e.fields[1]);

            /// Vật phẩm nằm trong túi
            GoodsData goodsData = Global.Data.RoleData.GoodsDataList?.Where(x => x.Id == goodsDbID).FirstOrDefault();
            /// Nếu tồn tại vật phẩm nằm trong túi
            if (goodsData != null)
            {
                goodsData.GCount = 0;
                Global.Data.RoleData.GoodsDataList.Remove(goodsData);
                /// Làm mới danh sách vật phẩm
                UIBagManager.Instance.RefreshItem(goodsData);
            }
            /// Nếu không thì ở thương khố
            else
            {
                /// Vật phẩm ở thương khố
                goodsData = Global.Data.PortableGoodsDataList?.Where(x => x.Id == goodsDbID).FirstOrDefault();
                /// Nếu tồn tại
                if (goodsData != null)
                {
                    goodsData.GCount = 0;
                    Global.Data.PortableGoodsDataList.Remove(goodsData);
                    /// Làm mới hiển thị
                    if (this.UIPortableBag != null)
                    {
                        this.UIPortableBag.BagGrid.RefreshItem(goodsData);
                    }
                }
            }
        }
        
        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_SPR_DEAD))
        {
            if (!AssertNetworkException(e.fields.Length == 1, e.fields.Length, (TCPGameServerCmds)e.CmdID))
            {
                return;
            }

            int roleID = Convert.ToInt32(e.fields[0]);
            GSprite target = KTGlobal.FindSpriteByID(roleID);

            /// Nếu là bản thân
            if (Global.Data.RoleData.RoleID == roleID)
            {
                Global.Data.RoleData.CurrentHP = 0;
                if (target != null)
                {
                    target.ComponentCharacter.UpdateHP();
                }
                /// Hủy tự đánh
                KTAutoFightManager.Instance.StopAutoFight();
                /// Ngừng tự làm nhiệm vụ
                AutoQuest.Instance.StopAutoQuest();
                /// Ngừng tự tìm đường
                AutoPathManager.Instance.StopAutoPath();
                /// Hủy dòng chữ tự tìm đường
                PlayZone.Instance.HideTextAutoFindPath();
            }
            /// Nếu là người chơi khác
            else if (Global.Data.OtherRoles.TryGetValue(roleID, out RoleData rd))
            {
                rd.CurrentHP = 0;
                if (target != null)
                {
                    target.ComponentCharacter.UpdateHP();
                }
            }
            /// Nếu là quái
            else if (Global.Data.SystemMonsters.TryGetValue(roleID, out MonsterData md))
            {
                md.HP = 0;
                if (target != null)
                {
                    target.ComponentMonster.UpdateHP();
                }
            }

            /// Nếu đối tượng tồn tại
            if (target != null)
            {
                /// Thực hiện chết
                target.DoDeath();
            }
        }
        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_GETGOODSLISTBYSITE))
        {
            List<GoodsData> goodsDataList = DataHelper.BytesToObject<List<GoodsData>>(e.bytesData, 0, e.bytesData.Length);
            Global.Data.PortableGoodsDataList = goodsDataList;

            /// Nếu chưa hiện khung thương khố
            if (this.UIPortableBag == null)
            {
                this.OpenUIPortableBag();
            }
            else
            {
                this.UIPortableBag.BagGrid.Reload();
            }
        }
        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_SPR_LOADALREADY))
        {
            LoadAlreadyData loadAlreadyData = DataHelper.BytesToObject<LoadAlreadyData>(e.bytesData, 0, e.bytesData.Length);
            if (null == loadAlreadyData)
            {
                return;
            }

            int otherRoleID = loadAlreadyData.RoleID;
            int currentX = loadAlreadyData.PosX;
            int currentY = loadAlreadyData.PosY;
            int currentDirection = loadAlreadyData.Direction;
            int action = loadAlreadyData.Action;
            string pathString = loadAlreadyData.PathString;
            int toX = loadAlreadyData.ToX;
            int toY = loadAlreadyData.ToY;
            int camp = loadAlreadyData.Camp;

            scene.ServerLoadAlready(otherRoleID, currentX, currentY, toX, toY, currentDirection, action, pathString, camp);
        }
        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_SPR_NPCSTATELIST))
        {
            List<NPCTaskState> npcTaskStateList = DataHelper.BytesToObject<List<NPCTaskState>>(e.bytesData, 0, e.bytesData.Length);
            Global.Data.RoleData.NPCTaskStateList = npcTaskStateList;
            if (null != npcTaskStateList)
            {
                for (int i = 0; i < npcTaskStateList.Count; i++)
                {
                    scene.ServerUpdateNPCTaskState(npcTaskStateList[i].NPCID, npcTaskStateList[i].TaskState);
                }
            }
        }
        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_SPR_RESETBAG))
        {
            List<GoodsData> goodsDataList = DataHelper.BytesToObject<List<GoodsData>>(e.bytesData, 0, e.bytesData.Length);
            if (null != goodsDataList)
            {
                Global.Data.RoleData.GoodsDataList = goodsDataList;
                UIBagManager.Instance.Reload();
            }
        }
        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_SPR_RESETPORTABLEBAG))
        {
            List<GoodsData> goodsDataList = DataHelper.BytesToObject<List<GoodsData>>(e.bytesData, 0, e.bytesData.Length);
            Global.Data.PortableGoodsDataList = goodsDataList;

            if (this.UIPortableBag != null)
            {
                this.UIPortableBag.BagGrid.Reload();
            }
        }
        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_SPR_PUSH_VERSION))
        {
            int hintNeedUpdate = int.Parse(e.fields[0]);
            string serverVersion = e.fields[1];

            /// Nếu cần Update
            if (hintNeedUpdate == 1)
            {
                Super.ShowMessageBox("Thông báo", string.Format("Phiên bản Client hiện tại <color=green>{0}</color> đã cũ (phiên bản mới là <color=yellow>{1}</color>), hãy thoát game và tiến hành cập nhật để có trải nghiệm tốt nhất!", MainGame.Instance.Version,  serverVersion), () =>
                {
                    Application.Quit();
                });
            }
        }
        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_SPR_QUERYUPLEVELGIFTINFO))
        {
            LevelUpGiftConfig levelUp = DataHelper.BytesToObject<LevelUpGiftConfig>(e.bytesData, 0, e.bytesData.Length);
            /// Ẩn bảng thông báo đợi
            KTGlobal.HideLoadingFrame();

            /// Nếu lỗi
            if (levelUp == null)
            {
                KTGlobal.AddNotification("Có lỗi khi tải dữ liệu, hãy thông báo với hỗ trợ!");
                /// Nếu đang mở khung phúc lợi
                if (this.UIWelfare != null)
                {
                    this.HideUIWelfare();
                }
                return;
            }

            /// Nếu đang mở khung phúc lợi
            if (this.UIWelfare != null)
            {
                this.UIWelfare.UILevelUp.Data = levelUp;
                this.UIWelfare.UILevelUp.Refresh();
            }
        }
        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_SPR_GETUPLEVELGIFTAWARD))
        {
            int ret = Convert.ToInt32(e.fields[0]);
            int id = Convert.ToInt32(e.fields[1]);

            /// Nhận thành công
            if (ret == 0)
            {
                if (this.UIWelfare != null)
                {
                    this.UIWelfare.UILevelUp.RefreshState(id, 2);
                }
            }
            else if (ret == -101)
            {
                KTGlobal.AddNotification("Cấp độ của bạn chưa đủ để nhận quà phúc lợi này.");
                return;
            }
            else if (ret == -103)
            {
                KTGlobal.AddNotification("Bạn đã nhận quà ở mốc này rồi, không thể nhận thêm.");
                return;
            }
        }
        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_SPR_NEWNPC))
        {
            //KTDebug.LogError("Tạo mới NPC -> CMD_SPR_NEWNPC");

            NPCRole npc = DataHelper.BytesToObject<NPCRole>(e.bytesData, 0, e.bytesData.Length);
            if (null != npc)
            {
                scene.ServerNewNPC(npc);
            }
        }
        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_SPR_DELNPC))
        {
            int npcID = Convert.ToInt32(e.fields[0]);
            scene.ServerDelNPC(npcID);
        }
        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_SPR_GETUSERMAILLIST))
        {
            List<MailData> mailDataList = DataHelper.BytesToObject<List<MailData>>(e.bytesData, 0, e.bytesData.Length);
            /// Đánh dấu dữ liệu thư
            Global.Data.MailDataList = mailDataList;

            /// Nếu đang mở khung
            if (this.UIMailBox != null)
            {
                this.UIMailBox.Refresh();
            }
            /// Nếu chưa mở khung
            else
            {
                /// Mở khung
                this.ShowUIMailBox();
            }
        }
        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_SPR_GETUSERMAILDATA))
        {
            MailData mailData = DataHelper.BytesToObject<MailData>(e.bytesData, 0, e.bytesData.Length);
            if (null == mailData)
            {
                KTGlobal.AddNotification("Không tìm thấy dữ liệu thư!");
                return;
            }
            /// Thiết lập vào danh sách tương ứng
            MailData _mailData = Global.Data.MailDataList.Where(x => x.MailID == mailData.MailID).FirstOrDefault();
            _mailData.IsRead = 1;
            _mailData.GoodsList = mailData.GoodsList;
            _mailData.Content = mailData.Content;

            /// Nếu đang mở khung
            if (this.UIMailBox != null)
            {
                this.UIMailBox.UpdateCurrentMailData(_mailData);
            }
        }
        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_SPR_FETCHMAILGOODS))
        {
            if (!AssertNetworkException(e.fields.Length == 3, e.fields.Length, (TCPGameServerCmds)e.CmdID))
            {
                return;
            }

            int result = Convert.ToInt32(e.fields[0]);
            int roleID = Convert.ToInt32(e.fields[1]);
            int mailID = Convert.ToInt32(e.fields[2]);

            /// Nếu có lỗi
            if (result != 1)
            {
                /// Nếu thư không tồn tại
                if (result == -100)
                {
                    KTGlobal.AddNotification("Thư không tồn tại!");
                    return;
                }
                /// Nếu ID người nhận khác bản thân
                else if (result == -115)
                {
                    KTGlobal.AddNotification("Thư này không gửi cho bạn!");
                    return;
                }
                /// Nếu thư không cho phép lấy vật phẩm
                else if (result == -121)
                {
                    KTGlobal.AddNotification("Thư này không có vật phẩm hoặc tiền đính kèm!");
                    return;
                }
                /// Nếu không có gì để nhận
                else if (result == -120)
                {
                    KTGlobal.AddNotification("Thư này không có vật phẩm hoặc tiền đính kèm!");
                    return;
                }
                /// Nếu không thể thêm vật phẩm
                else if (result == -125)
                {
                    KTGlobal.AddNotification("Túi đã đầy, không thể nhận vật phẩm đính kèm!");
                    return;
                }
                /// Lỗi khác
                else
                {
                    KTGlobal.AddNotification(string.Format("Không thể nhận vật phẩm đính kèm, mã lối: {0}!", result));
                    return;
                }
            }

            /// Nếu danh sách Mail rỗng
            if (Global.Data.MailDataList == null)
            {
                return;
            }
            /// Nếu có lỗi xảy ra
            if (result != 1)
            {
                KTGlobal.AddNotification(string.Format("Có lỗi xảy ra khi nhận vật phẩm và tiền đính kèm trong thư. Mã lỗi: {0}", result));
                return;
            }

            /// Thư tương ứng
            MailData mailData = Global.Data.MailDataList.Where(x => x.MailID == mailID).FirstOrDefault();
            mailData.HasFetchAttachment = 0;
            mailData.Token = 0;
            mailData.Money = 0;
            mailData.GoodsList = null;

            /// Nếu đang hiện khung
            if (this.UIMailBox != null)
            {
                this.UIMailBox.UpdateCurrentMailData(mailData);
            }
        }
        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_SPR_DELETEUSERMAIL))
        {
            if (!AssertNetworkException(e.fields.Length == 3, e.fields.Length, (TCPGameServerCmds)e.CmdID))
            {
                return;
            }

            int roleID = Convert.ToInt32(e.fields[0]);
            int mailID = Convert.ToInt32(e.fields[1]);
            int result = Convert.ToInt32(e.fields[2]);

            /// Nếu tồn tại vật phẩm hoặc tiền chưa nhận
            if (result == -100)
            {
                KTGlobal.AddNotification("Thư không tồn tại!");
                return;
            }
            else if (result == -101)
            {
                KTGlobal.AddNotification("Có vật phẩm đính kèm hoặc tiền chưa nhận, không thể xóa thư!");
                return;
            }
            /// Nếu danh sách Mail rỗng
            else if (Global.Data.MailDataList == null)
            {
                return;
            }

            /// Xóa thư tương ứng khỏi danh sách
            Global.Data.MailDataList.RemoveAll(x => x.MailID == mailID);

            /// Nếu đang hiện khung
            if (this.UIMailBox != null)
            {
                this.UIMailBox.Refresh();
            }
        }
        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_SPR_NOTIFYGOODSINFO))
        {
            /// Vật phẩm tương ứng
            GoodsData newGoods = DataHelper.BytesToObject<GoodsData>(e.bytesData, 0, e.bytesData.Length);
            /// Toác
            if (newGoods == null)
            {
                return;
            }

            /// Vật phẩm tương ứng trong túi đồ
            GoodsData itemGD = Global.Data.RoleData.GoodsDataList?.Where(x => x.Id == newGoods.Id).FirstOrDefault();

            /// Nếu tìm thấy
            if (itemGD != null)
            {
                /// Nếu vị trí khác nhau
                if (newGoods.Site != itemGD.Site)
                {
                    /// Xóa khỏi danh sách trong túi đồ
                    Global.Data.RoleData.GoodsDataList?.Remove(itemGD);
                    /// Thiết lập số lượng về 0 để nó xóa
                    itemGD.GCount = 0;
                    /// Xóa khỏi UI
                    UIBagManager.Instance.RefreshItem(itemGD);

                    /// Nếu vị trí mới nằm ở thương khố
                    if (newGoods.Site == 1)
                    {
                        /// Thêm vào danh sách
                        Global.Data.PortableGoodsDataList?.Add(newGoods);
                        /// Cập nhật lại thông tin vật phẩm trong kho
                        if (this.UIPortableBag != null)
                        {
                            this.UIPortableBag.BagGrid.AddItem(newGoods);
                        }
                    }
                }
                /// Nếu nằm cùng vị trí
                else
                {
                    /// Cập nhật thuộc tính
                    itemGD.GoodsID = newGoods.GoodsID;
                    itemGD.Binding = newGoods.Binding;
                    itemGD.Props = newGoods.Props;
                    itemGD.Series = newGoods.Series;
                    itemGD.Site = newGoods.Site;
                    itemGD.BagIndex = newGoods.BagIndex;
                    itemGD.Strong = newGoods.Strong;
                    itemGD.OtherParams = newGoods.OtherParams;
                    itemGD.Endtime = newGoods.Endtime;
                    itemGD.Forge_level = newGoods.Forge_level;
                    itemGD.Using = newGoods.Using;

                    /// Cập nhật UI
                    UIBagManager.Instance.RefreshItem(itemGD);
                }
            }
            /// Nếu không tìm thấy
            else
            {
                /// Nếu vị trí mới nằm ở túi đồ
                if (newGoods.Site == 0)
                {
                    /// Thêm vào túi đồ
                    Global.Data.RoleData.GoodsDataList?.Add(newGoods);
                    /// Cập nhật UI
                    UIBagManager.Instance.AddItem(newGoods);
                }

                /// Vật phẩm ở thương khố
                itemGD = Global.Data.PortableGoodsDataList?.Where(x => x.Id == newGoods.Id).FirstOrDefault();

                /// Nếu tìm thấy
                if (itemGD != null)
                {
                    /// Xóa khỏi thương khố
                    Global.Data.PortableGoodsDataList?.Remove(itemGD);
                    /// Cập nhật lại thông tin vật phẩm trong kho
                    if (this.UIPortableBag != null)
                    {
                        this.UIPortableBag.BagGrid.RemoveItem(itemGD);
                    }
                }
            }
        }
        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_SPR_CLIENTHEART))
        {
            SCClientHeart ClientHeart = DataHelper.BytesToObject<SCClientHeart>(e.bytesData, 0, e.bytesData.Length);
            int roleID = ClientHeart.RoleID;
            int randToken = ClientHeart.RandToken;
            long serverTicks = ClientHeart.Ticks;

            scene.ServerClientHeart(roleID, serverTicks);
        }
        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_SPR_UPDATEEVERYDAYONLINEAWARDGIFTINFO))
        {
            EveryDayOnLineEvent everydayOnlineEvent = DataHelper.BytesToObject<EveryDayOnLineEvent>(e.bytesData, 0, e.bytesData.Length);
            /// Ẩn bảng thông báo đợi
            KTGlobal.HideLoadingFrame();

            /// Nếu lỗi
            if (everydayOnlineEvent == null)
            {
                KTGlobal.AddNotification("Có lỗi khi tải dữ liệu, hãy thông báo với hỗ trợ!");
                /// Nếu đang mở khung phúc lợi
                if (this.UIWelfare != null)
                {
                    this.HideUIWelfare();
                }
                return;
            }

            /// Đánh dấu thời điểm nhận được gói tin
            everydayOnlineEvent.ReceiveTick = KTGlobal.GetCurrentTimeMilis();

            /// Nếu đang mở khung phúc lợi
            if (this.UIWelfare != null)
            {
                this.UIWelfare.UIEverydayOnline.Data = everydayOnlineEvent;
                this.UIWelfare.UIEverydayOnline.Refresh();
            }
        }
        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_SPR_GETEVERYDAYONLINEAWARDGIFT))
        {
            if (!AssertNetworkException(e.fields.Length == 5, e.fields.Length, (TCPGameServerCmds)e.CmdID))
            {
                return;
            }

            int roleID = Convert.ToInt32(e.fields[0]);
            int ret = Convert.ToInt32(e.fields[1]);
            int stepID = Convert.ToInt32(e.fields[2]);
            int onlineRecord = Convert.ToInt32(e.fields[3]);
            string goodInfo = e.fields[4];

            string[] para = goodInfo.Split(',');
            int itemID = Convert.ToInt32(para[0]);
            int number = Convert.ToInt32(para[1]);

            /// Nếu thành công
            if (ret == 1)
            {
                if (this.UIWelfare != null)
                {
                    /// Cho Delay 7 giây rồi thực hiện Update cho Client
                    KTTimerManager.Instance.SetTimeout(7f, () =>
                    {
                        /// Làm mới hiển thị
                        this.UIWelfare.UIEverydayOnline.UpdateState(onlineRecord, stepID);
                    });
                    /// Thực hiện quay
                    this.UIWelfare.UIEverydayOnline.StartRoll(stepID, itemID, number);
                }
            }
        }
        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_SPR_UPDATEEVERYDAYSERIESLOGININFO))
        {
            SevenDayEvent sevenDayEvent = DataHelper.BytesToObject<SevenDayEvent>(e.bytesData, 0, e.bytesData.Length);
            /// Ẩn bảng thông báo đợi
            KTGlobal.HideLoadingFrame();

            /// Nếu lỗi
            if (sevenDayEvent == null)
            {
                KTGlobal.AddNotification("Có lỗi khi tải dữ liệu, hãy thông báo với hỗ trợ!");
                /// Nếu đang mở khung phúc lợi
                if (this.UIWelfare != null)
                {
                    this.HideUIWelfare();
                }
                return;
            }

            /// Nếu đang mở khung phúc lợi
            if (this.UIWelfare != null)
            {
                this.UIWelfare.UISevenDayLogin.Data = sevenDayEvent;
                this.UIWelfare.UISevenDayLogin.Refresh();
            }
        }
        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_SPR_GETEVERYDAYSERIESLOGINAWARDGIFT))
        {
            if (!AssertNetworkException(e.fields.Length == 5, e.fields.Length, (TCPGameServerCmds)e.CmdID))
            {
                return;
            }

            int roleID = Convert.ToInt32(e.fields[0]);
            int ret = Convert.ToInt32(e.fields[1]);
            int dayID = Convert.ToInt32(e.fields[2]);
            string goodInfo = e.fields[3];
            int type = Convert.ToInt32(e.fields[4]);

            string[] para = goodInfo.Split(',');
            int itemID = Convert.ToInt32(para[0]);
            int number = Convert.ToInt32(para[1]);

            /// Nếu thành công
            if (ret == 1)
            {
                if (this.UIWelfare != null)
                {
                    /// Cho Delay 7 giây rồi thực hiện Update cho Client
                    KTTimerManager.Instance.SetTimeout(7f, () =>
                    {
                        /// Làm mới hiển thị
                        if (type == 0)
                        {

                            this.UIWelfare.UISevenDayLogin.UpdateState(dayID, itemID, number);
                        }
                        else
                        {
                            this.UIWelfare.UISevenDayLogin.UpdateStateContinuous(dayID, itemID, number);
                        }
                    });
                    /// Thực hiện quay
                    if (type == 0)
                    {
                        this.UIWelfare.UISevenDayLogin.StartRoll(dayID, itemID, number);
                    }
                    else
                    {
                        this.UIWelfare.UISevenDayLogin.StartRollContinuous(dayID, itemID, number);
                    }
                }
            }
        }
        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_KT_CHECKPOINT_INFO))
        {
            CheckPontConfig data = DataHelper.BytesToObject<CheckPontConfig>(e.bytesData, 0, e.bytesData.Length);
            /// Ẩn bảng thông báo đợi
            KTGlobal.HideLoadingFrame();

            /// Nếu lỗi
            if (data == null)
            {
                KTGlobal.AddNotification("Có lỗi khi tải dữ liệu, hãy thông báo với hỗ trợ!");
                /// Nếu đang mở khung phúc lợi
                if (this.UIWelfare != null)
                {
                    this.HideUIWelfare();
                }
                return;
            }

            /// Nếu đang mở khung phúc lợi
            if (this.UIWelfare != null)
            {
                this.UIWelfare.UISeriesLogin.Data = data;
                this.UIWelfare.UISeriesLogin.Refresh();
            }
        }
        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_KT_CHECKPOINT_GETAWARD))
        {
            if (!AssertNetworkException(e.fields.Length == 3, e.fields.Length, (TCPGameServerCmds)e.CmdID))
            {
                return;
            }

            int roleID = Convert.ToInt32(e.fields[0]);
            int ret = Convert.ToInt32(e.fields[1]);
            int day = Convert.ToInt32(e.fields[2]);

            /// Thao tác thành công
            if (ret == 0)
            {
                if (this.UIWelfare != null)
                {
                    this.UIWelfare.UISeriesLogin.RefreshDataCurrentDay(1);
                }
            }
        }
        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_SPR_MOVE))
        {
            SpriteNotifyOtherMoveData moveData = DataHelper.BytesToObject<SpriteNotifyOtherMoveData>(e.bytesData, 0, e.bytesData.Length);
            if (!AssertNetworkException(null != moveData, e.CmdID))
            {
                return;
            }

            scene.ServerLinearMove(moveData.RoleID, moveData.FromX, moveData.FromY, moveData.ToX, moveData.ToY, moveData.PathString, moveData.Action);
        }
        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_SPR_RELIFE))
        {
            SpriteRelifeData relifeData = DataHelper.BytesToObject<SpriteRelifeData>(e.bytesData, 0, e.bytesData.Length);
            if (relifeData == null)
            {
                return;
            }

            scene.ServerRelife(relifeData.RoleID, relifeData.PosX, relifeData.PosY, relifeData.Direction, relifeData.HP, relifeData.MP, relifeData.Stamina);
            if (Global.Data.RoleData != null)
            {
                this.NotifyRoleFace(relifeData.RoleID, -1);
            }
        }
        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_SPR_REFRESH_ICON_STATE))
        {
            ActivityIconStateData data = DataHelper.BytesToObject<ActivityIconStateData>(e.bytesData, 0, e.bytesData.Length);
            if (data != null)
            {
                /// Duyệt danh sách trạng thái ICON
                foreach (KeyValuePair<int, int> pair in data.IconState)
                {
                    FunctionButtonType type = (FunctionButtonType)pair.Key;
                    FunctionButtonAction action = (FunctionButtonAction)pair.Value;

                    /// Thực hiện đổi trạng thái tương ứng
                    this.UITopFunctionButtons.ChangeButtonState(type, action);
                }
            }
        }
        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_SPR_CHECK))
        {
            if (!AssertNetworkException(e.fields.Length == 1, e.fields.Length, (TCPGameServerCmds)e.CmdID))
            {
                return;
            }

            BasePlayZone.IsSpeedCheck = true;
            BasePlayZone.InWaitPingCount = 0;
            if (this.UIRolePart != null && this.UIRolePart.UIPingInfo != null)
            {
                this.UIRolePart.UIPingInfo.ReceivePing();
            }
        }
        #region Kiếm Thế
        else if (e.CmdID == (int)TCPGameServerCmds.CMD_KT_ROLE_ATRIBUTES)
        {
            KT_TCPHandler.ReceiveRoleAttributes(e.CmdID, e.bytesData, e.bytesData.Length);
        }

        else if (e.CmdID == (int)TCPGameServerCmds.CMD_KT_SHOW_NOTIFICATIONTIP)
        {
            KT_TCPHandler.ReceiveShowNotificationTip(e.CmdID, e.bytesData, e.bytesData.Length);
        }

        else if (e.CmdID == (int)TCPGameServerCmds.CMD_KT_FACTIONROUTE_CHANGED)
        {
            KTFactionSocketManager.ReceiveFactionRouteChanged(e.CmdID, e.bytesData, e.bytesData.Length);
        }

        else if (e.CmdID == (int)TCPGameServerCmds.CMD_KT_G2C_NPCDIALOG)
        {
            KT_TCPHandler.ReceiveOpenNPCDialog(e.CmdID, e.bytesData, e.bytesData.Length);
        }

        else if (e.CmdID == (int)TCPGameServerCmds.CMD_KT_G2C_ITEMDIALOG)
        {
            KT_TCPHandler.ReceiveOpenItemDialog(e.CmdID, e.bytesData, e.bytesData.Length);
        }

        else if (e.CmdID == (int)TCPGameServerCmds.CMD_KT_G2C_RENEW_SKILLLIST)
        {
            KTTCPSkillManager.ReceiveRenewSkillList(e.CmdID, e.bytesData, e.bytesData.Length);
        }

        else if (e.CmdID == (int)TCPGameServerCmds.CMD_KT_C2G_USESKILL)
        {
            KTTCPSkillManager.ReceiveUseSkillResult(e.fields);
        }

        else if (e.CmdID == (int)TCPGameServerCmds.CMD_KT_G2C_USESKILL)
        {
            KTTCPSkillManager.ReceiveObjectUseSkill(e.CmdID, e.bytesData, e.bytesData.Length);
        }

        else if (e.CmdID == (int)TCPGameServerCmds.CMD_KT_G2C_NOTIFYSKILLCOOLDOWN)
        {
            KTTCPSkillManager.ReceiveSkillCooldown(e.CmdID, e.bytesData, e.bytesData.Length);
        }

        else if (e.CmdID == (int)TCPGameServerCmds.CMD_KT_G2C_BULLETEXPLODE)
        {
            KTTCPSkillManager.ReceiveBulletExplode(e.CmdID, e.bytesData, e.bytesData.Length);
        }

        else if (e.CmdID == (int)TCPGameServerCmds.CMD_KT_G2C_BULLETEXPLODES)
        {
            KTTCPSkillManager.ReceiveBulletExplodes(e.CmdID, e.bytesData, e.bytesData.Length);
        }

        else if (e.CmdID == (int)TCPGameServerCmds.CMD_KT_G2C_CREATEBULLET)
        {
            KTTCPSkillManager.ReceiveCreateBullet(e.CmdID, e.bytesData, e.bytesData.Length);
        }

        else if (e.CmdID == (int)TCPGameServerCmds.CMD_KT_G2C_CREATEBULLETS)
        {
            KTTCPSkillManager.ReceiveCreateBullets(e.CmdID, e.bytesData, e.bytesData.Length);
        }

        else if (e.CmdID == (int)TCPGameServerCmds.CMD_KT_G2C_SKILLRESULT)
        {
            Global.Data.GameScene.ReceiveSkillResult(e.CmdID, e.bytesData, e.bytesData.Length);
        }

        else if (e.CmdID == (int)TCPGameServerCmds.CMD_KT_G2C_SKILLRESULTS)
        {
            Global.Data.GameScene.ReceiveSkillResults(e.CmdID, e.bytesData, e.bytesData.Length);
        }

        else if (e.CmdID == (int)TCPGameServerCmds.CMD_KT_G2C_OBJECTINVISIBLESTATECHANGED)
        {
            KTTCPSkillManager.ReceiveObjectInvisibleStateChanged(e.CmdID, e.bytesData, e.bytesData.Length);
        }

        else if (e.CmdID == (int)TCPGameServerCmds.CMD_KT_G2C_RESETSKILLCOOLDOWN)
        {
            KTTCPSkillManager.ReceiveResetSkillCooldown(e.fields);
        }

        else if (e.CmdID == (int)TCPGameServerCmds.CMD_KT_G2C_SPRITEBUFF)
        {
            KTTCPSkillManager.ReceiveSpriteBuff(e.CmdID, e.bytesData, e.bytesData.Length);
        }

        else if (e.CmdID == (int)TCPGameServerCmds.CMD_KT_G2C_BLINKTOPOSITION)
        {
            KTTCPSkillManager.ReceiveTeleportToPosition(e.CmdID, e.bytesData, e.bytesData.Length);
        }

        else if (e.CmdID == (int)TCPGameServerCmds.CMD_KT_G2C_FLYTOPOSITION)
        {
            KTTCPSkillManager.ReceiveFlyToPosition(e.CmdID, e.bytesData, e.bytesData.Length);
        }

        else if (e.CmdID == (int)TCPGameServerCmds.CMD_KT_G2C_MOVESPEEDCHANGED)
        {
            KTTCPSkillManager.ReceiveTargetMoveSpeedChanged(e.CmdID, e.bytesData, e.bytesData.Length);
        }

        else if (e.CmdID == (int)TCPGameServerCmds.CMD_KT_G2C_ATTACKSPEEDCHANGED)
        {
            KTTCPSkillManager.ReceiveTargetAttackSpeedChanged(e.CmdID, e.bytesData, e.bytesData.Length);
        }

        else if (e.CmdID == (int)TCPGameServerCmds.CMD_KT_G2C_CHANGEACTION)
        {
            KTTCPSkillManager.ReceiveSpriteChangeAction(e.CmdID, e.bytesData, e.bytesData.Length);
        }

        else if (e.CmdID == (int)TCPGameServerCmds.CMD_KT_G2C_SPRITESERIESSTATE)
        {
            KTTCPSkillManager.ReceiveSpriteSeriesState(e.CmdID, e.bytesData, e.bytesData.Length);
        }

        else if (e.CmdID == (int)TCPGameServerCmds.CMD_KT_G2C_SHOWDEBUGOBJECTS)
        {
            KT_TCPHandler.ReceiveShowDebugObjects(e.CmdID, e.bytesData, e.bytesData.Length);
        }

        else if (e.CmdID == (int)TCPGameServerCmds.CMD_KT_G2C_SHOWREVIVEFRAME)
        {
            KT_TCPHandler.ReceiveShowReviveFrame(e.CmdID, e.bytesData, e.bytesData.Length);
        }

        else if (e.CmdID == (int)TCPGameServerCmds.CMD_KT_SPR_NEWTRAP)
        {
            //KTDebug.LogError("Thêm Trap -> CMD_KT_SPR_NEWTRAP");

            KTTCPSkillManager.ReceiveNewTrap(e.CmdID, e.bytesData, e.bytesData.Length);
        }

        else if (e.CmdID == (int)TCPGameServerCmds.CMD_KT_SPR_DELTRAP)
        {
            //KTDebug.LogError("Xóa Trap -> CMD_KT_SPR_DELTRAP");

            int trapID = Convert.ToInt32(e.fields[0]);

            KTTCPSkillManager.ReceiveDelTrap(trapID);
        }

        else if (e.CmdID == (int)TCPGameServerCmds.CMD_KT_SPR_NEWTRAP)
        {
            //KTDebug.LogError("Thêm Trap -> CMD_KT_SPR_NEWTRAP");

            KTTCPSkillManager.ReceiveNewTrap(e.CmdID, e.bytesData, e.bytesData.Length);
        }

        else if (e.CmdID == (int)TCPGameServerCmds.CMD_KT_CREATETEAM)
        {
            KT_TCPHandler.ReceiveCreateTeam(e.fields);
        }

        else if (e.CmdID == (int)TCPGameServerCmds.CMD_KT_INVITETOTEAM)
        {
            KT_TCPHandler.ReceiveInviteToTeam(e.fields);
        }

        else if (e.CmdID == (int)TCPGameServerCmds.CMD_KT_AGREEJOINTEAM)
        {
            KT_TCPHandler.ReceiveAgreeToJoinTeam(e.CmdID, e.bytesData, e.bytesData.Length);
        }

        else if (e.CmdID == (int)TCPGameServerCmds.CMD_KT_REFUSEJOINTEAM)
        {
            KT_TCPHandler.ReceiveRefuseToJoinTeam(e.fields);
        }

        else if (e.CmdID == (int)TCPGameServerCmds.CMD_KT_GETTEAMINFO)
        {
            KT_TCPHandler.ReceiveGetTeamInfo(e.CmdID, e.bytesData, e.bytesData.Length);
        }

        else if (e.CmdID == (int)TCPGameServerCmds.CMD_KT_KICKOUTTEAMMATE)
        {
            KT_TCPHandler.ReceiveKickOutTeammate(e.CmdID, e.bytesData, e.bytesData.Length);
        }

        else if (e.CmdID == (int)TCPGameServerCmds.CMD_KT_APPROVETEAMLEADER)
        {
            KT_TCPHandler.ReceiveApproveTeamLeader(e.fields);
        }

        else if (e.CmdID == (int)TCPGameServerCmds.CMD_KT_REFRESHTEAMMEMBERATTRIBUTES)
        {
            KT_TCPHandler.ReceiveRefreshTeamMemberAttributes(e.CmdID, e.bytesData, e.bytesData.Length);
        }

        else if (e.CmdID == (int)TCPGameServerCmds.CMD_KT_TEAMMEMBERCHANGED)
        {
            KT_TCPHandler.ReceiveTeamMemberChanged(e.CmdID, e.bytesData, e.bytesData.Length);
        }

        else if (e.CmdID == (int)TCPGameServerCmds.CMD_KT_LEAVETEAM)
        {
            KT_TCPHandler.ReceiveLeaveTeam(e.fields);
        }

        else if (e.CmdID == (int)TCPGameServerCmds.CMD_KT_G2C_UPDATESPRITETEAMDATA)
        {
            KT_TCPHandler.ReceiveSpriteUpdateTeamInfo(e.fields);
        }

        else if (e.CmdID == (int)TCPGameServerCmds.CMD_KT_ASKTOJOINTEAM)
        {
            KT_TCPHandler.ReceiveAskToJoinTeam(e.CmdID, e.bytesData, e.bytesData.Length);
        }

        else if (e.CmdID == (int)TCPGameServerCmds.CMD_KT_G2C_CLOSENPCITEMDIALOG)
        {
            KT_TCPHandler.ReceiveCloseDialog();
        }

        else if (e.CmdID == (int) TCPGameServerCmds.CMD_KT_ASK_CHALLENGE)
        {
            KT_TCPHandler.ReceiveAskChallenge(e.fields);
        }

        else if (e.CmdID == (int) TCPGameServerCmds.CMD_KT_DO_CHALLENGE_COMMAND)
        {
            KT_TCPHandler.ReceiveDoChallengeCommand(e.fields);
        }

        else if (e.CmdID == (int) TCPGameServerCmds.CMD_KT_RECEIVE_CHALLENGE_INFO)
        {
            KT_TCPHandler.ReceiveNewChallengeInfo(e.CmdID, e.bytesData, e.bytesData.Length);
        }

        else if (e.CmdID == (int)TCPGameServerCmds.CMD_KT_G2C_NEW_GROWPOINT)
        {
            GrowPointObject growPointObject = DataHelper.BytesToObject<GrowPointObject>(e.bytesData, 0, e.bytesData.Length);
            if (growPointObject == null)
            {
                return;
            }

            this.scene.ServerNewGrowPoint(growPointObject);
        }

        else if (e.CmdID == (int)TCPGameServerCmds.CMD_KT_G2C_DEL_GROWPOINT)
        {
            int id = Convert.ToInt32(e.fields[0]);

            this.scene.ServerDelGrowPoint(id);
        }

        else if (e.CmdID == (int)TCPGameServerCmds.CMD_KT_G2C_NEW_DYNAMICAREA)
        {
            DynamicArea dynArea = DataHelper.BytesToObject<DynamicArea>(e.bytesData, 0, e.bytesData.Length);
            if (dynArea == null)
            {
                return;
            }

            this.scene.ServerNewDynamicArea(dynArea);
        }

        else if (e.CmdID == (int)TCPGameServerCmds.CMD_KT_G2C_DEL_DYNAMICAREA)
        {
            int id = Convert.ToInt32(e.fields[0]);

            this.scene.ServerDelDynamicArea(id);
        }

        else if (e.CmdID == (int)TCPGameServerCmds.CMD_KT_G2C_NEW_BOT)
        {
            DecoBotData botData = DataHelper.BytesToObject<DecoBotData>(e.bytesData, 0, e.bytesData.Length);
            if (botData == null)
            {
                return;
            }

            /// Dữ liệu
            RoleData roleData = KTGlobal.DecoBotDataToRoleData(botData);
            /// Toác
            if (roleData == null)
            {
                return;
            }

            /// Lưu dữ liệu
            Global.Data.Bots[roleData.RoleID] = roleData;
            /// Tải xuống Bot
            scene.ToLoadBot(roleData);
        }
        else if (e.CmdID == (int)TCPGameServerCmds.CMD_KT_G2C_DEL_BOT)
        {
            int id = Convert.ToInt32(e.fields[0]);

            try
            {
                /// Xóa BOT
                this.scene.DelBot(id);
            }
            catch (Exception ex)
            {
                KTDebug.LogException(ex);
            }
        }

        else if (e.CmdID == (int)TCPGameServerCmds.CMD_KT_G2C_UPDATE_PROGRESSBAR)
        {
            G2C_ProgressBar progressBar = DataHelper.BytesToObject<G2C_ProgressBar>(e.bytesData, 0, e.bytesData.Length);
            if (progressBar == null)
            {
                return;
            }

            /// Loại thao tác
            int type = progressBar.Type;

            /// Nếu là mở thanh Progress hoặc cập nhật trạng thái của thanh
            if (type == 1)
            {
                this.ShowProgressBar(progressBar.CurrentLifeTime, progressBar.Duration, progressBar.Text);
            }
            /// Nếu là đóng thanh Progress
            else if (type == 2)
            {
                this.HideProgressBar();
            }
        }

        else if (e.CmdID == (int)TCPGameServerCmds.CMD_KT_TESTPACKET)
        {
            /// Sử dụng dữ liệu được gửi ở Packet dạng này (dạng bytes array thì sẽ có 3 tham biến cần dùng là e.CmdID, e.bytesData, e.bytesData.Length, dạng string thì sẽ có 1 mảng e.fields)
            KT_TCPHandler.ReceiveTestFromServer(e.fields);
        }

        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_KT_C2G_OPENSHOP))
        {
            ShopTab shopData = DataHelper.BytesToObject<ShopTab>(e.bytesData, 0, e.bytesData.Length);
            this.OpenUIShop(shopData);
        }

        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_KT_OPEN_TOKENSHOP))
        {
            TokenShop tokenShop = DataHelper.BytesToObject<TokenShop>(e.bytesData, 0, e.bytesData.Length);
            if (tokenShop == null)
            {
                return;
            }

            /// Mở khung Kỳ Trân Các
            this.OpenTokenShop(tokenShop);
        }

        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_KT_G2C_OPEN_UI))
        {
            KT_TCPHandler.ReceiveOpenUI(e.CmdID, e.bytesData, e.bytesData.Length);
        }

        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_KT_G2C_CLOSE_UI))
        {
            KT_TCPHandler.ReceiveCloseUI(e.CmdID, e.bytesData, e.bytesData.Length);
        }

        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_KT_TOGGLE_HORSE_STATE))
        {
            KT_TCPHandler.ReceiveHorseStateChanged(e.fields);
        }

        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_KT_SIGNET_ENHANCE))
        {
            KT_TCPHandler.ReceiveSignetEnhance(e.fields);
        }

        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_KT_EQUIP_ENHANCE))
        {
            KT_TCPHandler.ReceiveEquipEnhance(e.fields);
        }

        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_KT_COMPOSE_CRYSTALSTONES))
        {
            KT_TCPHandler.ReceiveComposeCrystalStones(e.fields);
        }

        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_KT_SPLIT_CRYSTALSTONES))
        {
            KT_TCPHandler.ReceiveSplitCrystalStones(e.fields);
        }

        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_KT_G2C_START_ACTIVEFIGHT))
        {
            KT_TCPHandler.ReceiveBeginActiveFight(e.fields);
        }

        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_KT_G2C_STOP_ACTIVEFIGHT))
        {
            KT_TCPHandler.RecieveStopActiveFight(e.fields);
        }

        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_KT_CHANGE_AVARTA))
        {
            KT_TCPHandler.ReceiveRoleAvartaChanged(e.fields);
        }

        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_KT_BEGIN_CRAFT))
        {
            KT_TCPHandler.ReceiveBeginCraft(e.fields);
        }

        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_KT_G2C_FINISH_CRAFT))
        {
            KT_TCPHandler.ReceiveFinishCraft(e.fields);
        }

        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_KT_G2C_UPDATE_ROLE_GATHERMAKEPOINT))
        {
            KT_TCPHandler.ReceiveUpdateGatherMakePoint(e.fields);
        }

        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_KT_G2C_UPDATE_LIFESKILL_LEVEL))
        {
            KT_TCPHandler.ReceiveUpdateLifeSkillLevel(e.fields);
        }

        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_KT_SHOW_MESSAGEBOX))
        {
            KT_TCPHandler.ReceiveMessageBox(e.CmdID, e.bytesData, e.bytesData.Length);
        }

        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_KT_EVENT_NOTIFICATION))
        {
            KT_TCPHandler.ReceiveEventNotification(e.CmdID, e.bytesData, e.bytesData.Length);
        }

        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_KT_KILLSTREAK))
        {
            KT_TCPHandler.ReceiveStreakKill(e.CmdID, e.bytesData, e.bytesData.Length);
        }

        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_KT_EVENT_STATE))
        {
            KT_TCPHandler.ReceiveEventStateChange(e.CmdID, e.bytesData, e.bytesData.Length);
        }

        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_KT_SONGJINBATTLE_RANKING))
        {
            KT_TCPHandler.ReceiveSongJinRankingInfo(e.CmdID, e.bytesData, e.bytesData.Length);
        }

        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_KT_BROWSE_PLAYER))
        {
            KT_TCPHandler.ReceiveBrowsePlayer(e.CmdID, e.bytesData, e.bytesData.Length);
        }

        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_KT_CHECK_PLAYER_LOCATION))
        {
            KT_TCPHandler.ReceiveCheckPlayerLocation(e.CmdID, e.bytesData, e.bytesData.Length);
        }

        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_KT_GET_PLAYER_INFO))
        {
            KT_TCPHandler.ReceiveCheckPlayerInfo(e.CmdID, e.bytesData, e.bytesData.Length);
        }

        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_KT_UPDATE_TITLE))
        {
            KT_TCPHandler.ReceiveTitleChanged(e.CmdID, e.bytesData, e.bytesData.Length);
        }

        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_KT_UPDATE_NAME))
        {
            KT_TCPHandler.ReceiveNameChanged(e.CmdID, e.bytesData, e.bytesData.Length);
        }

        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_KT_UPDATE_TOTALVALUE))
        {
            KT_TCPHandler.ReciveRoleValueChanged(e.fields);
        }

        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_KT_UPDATE_REPUTE))
        {
            KT_TCPHandler.ReceiveReputeChanged(e.CmdID, e.bytesData, e.bytesData.Length);
        }

        else if (e.CmdID == (int)TCPGameServerCmds.CMD_SPR_QUERY_REPAYACTIVEINFO)
        {
            RechageAcitivty rechargeActivity = DataHelper.BytesToObject<RechageAcitivty>(e.bytesData, 0, e.bytesData.Length);
            /// Ẩn bảng thông báo đợi
            KTGlobal.HideLoadingFrame();

            /// Nếu lỗi
            if (rechargeActivity == null)
            {
                KTGlobal.AddNotification("Có lỗi khi tải dữ liệu, hãy thông báo với hỗ trợ!");
                /// Nếu đang mở khung phúc lợi
                if (this.UIWelfare != null)
                {
                    this.HideUIWelfare();
                }
                return;
            }

            /// Nếu đang mở khung phúc lợi
            if (this.UIWelfare != null)
            {
                this.UIWelfare.UIRecharge.Data = rechargeActivity;
                this.UIWelfare.UIRecharge.Refresh();
            }
        }

        else if (e.CmdID == (int)TCPGameServerCmds.CMD_SPR_GET_REPAYACTIVEAWARD)
        {
            if (!AssertNetworkException(e.fields.Length == 3, e.fields.Length, (TCPGameServerCmds)e.CmdID))
            {
                return;
            }

            int result = Convert.ToInt32(e.fields[0]);
            int nActivityType = Convert.ToInt32(e.fields[1]);
            string nRetValue = e.fields[2];

            switch (nActivityType)
            {
                case (int)ActivityTypes.InputFirst:
                    {
                        if (result == -1)
                        {
                            KTGlobal.AddNotification("Bạn đã nhận thưởng ở mốc này rồi!");
                            return;
                        }
                        else if (result == -10 || result == -30)
                        {
                            KTGlobal.AddNotification("Bạn chưa nạp thẻ!");
                            return;
                        }
                        else if (result == -20)
                        {
                            KTGlobal.AddNotification("Túi của bạn đã đầy, không thể nhận thưởng!");
                            return;
                        }

                        /// Cập nhật trạng thái
                        if (this.UIWelfare != null)
                        {
                            this.UIWelfare.UIRecharge.UIFirstRecharge.UpdateState(Convert.ToInt32(nRetValue));
                        }

                        break;
                    }
                case (int)ActivityTypes.MeiRiChongZhiHaoLi:
                    {
                        if (result == (int)-1)
                        {
                            KTGlobal.AddNotification("Phúc lợi không tồn tại!");
                            return;
                        }
                        else if (result == (int)-20)
                        {
                            KTGlobal.AddNotification("Túi đồ của bạn đã đầy, không thể nhận thưởng!");
                            return;
                        }
                        else if (result == (int)-10)
                        {
                            KTGlobal.AddNotification("Bạn đã nhận thưởng gói phúc lợi này rồi!");
                            return;
                        }
                        else if (result == (int)-5)
                        {
                            KTGlobal.AddNotification("Hôm nay bạn chưa nạp đủ số lượng để nhận quà phúc lợi này!");
                            return;
                        }

                        /// Trạng thái
                        List<int> states = new List<int>();

                        /// Thông tin trạng thái Button
                        string[] infoParams = nRetValue.Split(',');
                        try
                        {
                            /// Duyệt danh sách và lấy ra trạng thái Button
                            for (int i = 0; i < infoParams.Length; i++)
                            {
                                states.Add(Convert.ToInt32(infoParams[i]));
                            }
                        }
                        catch (Exception)
                        {
                            return;
                        }

                        /// Cập nhật trạng thái
                        if (this.UIWelfare != null)
                        {
                            this.UIWelfare.UIRecharge.UIEverydayRecharge.UpdateState(states);
                        }

                        break;
                    }
                case (int)ActivityTypes.TotalCharge:
                    {
                        if (result == (int)-1)
                        {
                            KTGlobal.AddNotification("Phúc lợi không tồn tại!");
                            return;
                        }
                        else if (result == (int)-20)
                        {
                            KTGlobal.AddNotification("Túi đồ của bạn đã đầy, không thể nhận thưởng!");
                            return;
                        }
                        else if (result == (int)-10)
                        {
                            KTGlobal.AddNotification("Bạn đã nhận thưởng gói phúc lợi này rồi!");
                            return;
                        }
                        else if (result == (int)-30)
                        {
                            KTGlobal.AddNotification("Bạn chưa nạp đủ số lượng để nhận quà phúc lợi này!");
                            return;
                        }

                        /// Trạng thái
                        List<int> states = new List<int>();

                        /// Thông tin trạng thái Button
                        string[] infoParams = nRetValue.Split(',');
                        try
                        {
                            /// Duyệt danh sách và lấy ra trạng thái Button
                            for (int i = 0; i < infoParams.Length; i++)
                            {
                                states.Add(Convert.ToInt32(infoParams[i]));
                            }
                        }
                        catch (Exception)
                        {
                            return;
                        }

                        /// Cập nhật trạng thái
                        if (this.UIWelfare != null)
                        {
                            this.UIWelfare.UIRecharge.UITotalRecharge.UpdateState(states);
                        }

                        break;
                    }
                case (int)ActivityTypes.TotalConsume:
                    {
                        if (result == (int)-1)
                        {
                            KTGlobal.AddNotification("Phúc lợi không tồn tại!");
                            return;
                        }
                        else if (result == (int)-20)
                        {
                            KTGlobal.AddNotification("Túi đồ của bạn đã đầy, không thể nhận thưởng!");
                            return;
                        }
                        else if (result == (int)-10)
                        {
                            KTGlobal.AddNotification("Bạn đã nhận thưởng gói phúc lợi này rồi!");
                            return;
                        }
                        else if (result == (int)-30)
                        {
                            KTGlobal.AddNotification("Bạn chưa tiêu đủ số lượng để nhận quà phúc lợi này!");
                            return;
                        }

                        /// Trạng thái
                        List<int> states = new List<int>();

                        /// Thông tin trạng thái Button
                        string[] infoParams = nRetValue.Split(',');
                        try
                        {
                            /// Duyệt danh sách và lấy ra trạng thái Button
                            for (int i = 0; i < infoParams.Length; i++)
                            {
                                states.Add(Convert.ToInt32(infoParams[i]));
                            }
                        }
                        catch (Exception)
                        {
                            return;
                        }

                        /// Cập nhật trạng thái
                        if (this.UIWelfare != null)
                        {
                            this.UIWelfare.UIRecharge.UITotalConsume.UpdateState(states);
                        }

                        break;
                    }
            }
        }
        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_SPR_GET_YUEKA_DATA))
        {
            KTGlobal.HideLoadingFrame();
            YueKaData yueKaData = DataHelper.BytesToObject<YueKaData>(e.bytesData, 0, e.bytesData.Length);

            /// Nếu đang hiện khung
            if (PlayZone.Instance.UIWelfare != null)
            {
                PlayZone.Instance.UIWelfare.MonthCard.Data = yueKaData;
                PlayZone.Instance.UIWelfare.MonthCard.Refresh();
            }
        }
        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_SPR_GET_YUEKA_AWARD))
        {
            /// Mã lỗi
            int errorCode = int.Parse(e.fields[1]);
            /// Nếu thành công
            if (errorCode == (int)YueKaError.YK_Success)
            {
                /// Nếu đang hiện khung
                if (PlayZone.Instance.UIWelfare != null)
                {
                    PlayZone.Instance.UIWelfare.MonthCard.RefreshDataCurrentDay(1);
                }
            }
            else
            {
                switch ((YueKaError)errorCode)
                {
                    case YueKaError.YK_CannotAward_AlreadyAward:
                        {
                            KTGlobal.AddNotification("Đã nhận quà ngày hôm nay rồi!");
                            break;
                        }
                    case YueKaError.YK_CannotAward_BagNotEnough:
                        {
                            KTGlobal.AddNotification("Túi đồ không đủ khoảng trống!");
                            break;
                        }
                    case YueKaError.YK_CannotAward_ConfigError:
                        {
                            KTGlobal.AddNotification("Thiết lập hệ thống bị lỗi!");
                            break;
                        }
                    case YueKaError.YK_CannotAward_DayHasPassed:
                        {
                            KTGlobal.AddNotification("Đã quá hạn nhận thưởng!");
                            break;
                        }
                    case YueKaError.YK_CannotAward_DBError:
                        {
                            KTGlobal.AddNotification("Thiết lập hệ thống bị lỗi!");
                            break;
                        }
                    case YueKaError.YK_CannotAward_HasNotYueKa:
                        {
                            KTGlobal.AddNotification("Không có thẻ tháng, không thể nhận thưởng!");
                            break;
                        }
                    case YueKaError.YK_CannotAward_ParamInvalid:
                        {
                            KTGlobal.AddNotification("Lỗi tham số!");
                            break;
                        }
                    case YueKaError.YK_CannotAward_TimeNotReach:
                        {
                            KTGlobal.AddNotification("Chưa đén thời gian nhận quà này!");
                            break;
                        }
                }

                /// Nếu đang hiện khung
                if (PlayZone.Instance.UIWelfare != null)
                {
                    PlayZone.Instance.UIWelfare.MonthCard.RefreshDataCurrentDay(-1);
                }
            }

        }
        else if (e.CmdID == (int)TCPGameServerCmds.CMD_KT_GET_BONUS_DOWNLOAD)
        {
            BonusDownload awards = DataHelper.BytesToObject<BonusDownload>(e.bytesData, 0, e.bytesData.Length);
            /// Nếu không giải mã được gói tin
            if (awards == null)
            {
                /// Đóng khung tải lần đầu nhận quà
                this.CloseGameResDownload();
                return;
            }

            /// Dữ liệu cần tải
            List<UpdateZipFile> needToDownloadFiles = KTResourceChecker.GetListMissingMapResources();
            /// Nếu đã nhận quà trước đó và không có dữ liệu cần tải
            if (!awards.CanRevice && (needToDownloadFiles == null || needToDownloadFiles.Count <= 0))
            {
                /// Đóng khung tải lần đầu nhận quà
                this.CloseGameResDownload();
                return;
            }

            /// Hiện khung quà tải lần đầu
            this.OpenGameResDownload(awards, needToDownloadFiles);
        }
        else if (e.CmdID == (int)TCPGameServerCmds.CMD_KT_SEASHELL_CIRCLE)
        {
            KT_TCPHandler.ReceiveSeashellCircleResponse(e.fields);
        }
        else if (e.CmdID == (int)TCPGameServerCmds.CMD_KT_QUERY_PLAYERRANKING)
        {
            KT_TCPHandler.ReceivePlayerRanking(e.CmdID, e.bytesData, e.bytesData.Length);
        }
        else if (e.CmdID == (int)TCPGameServerCmds.CMD_KT_SHOW_INPUTITEMS)
        {
            KT_TCPHandler.ReceiveShowInputItems(e.CmdID, e.bytesData, e.bytesData.Length);
        }
        else if (e.CmdID == (int)TCPGameServerCmds.CMD_KT_SHOW_INPUTEQUIPANDMATERIALS)
        {
            KT_TCPHandler.ReceiveShowInputEquipAndMaterials(e.CmdID, e.bytesData, e.bytesData.Length);
        }
        else if (e.CmdID == (int)TCPGameServerCmds.CMD_KT_YOULONG)
        {
            KT_TCPHandler.ProcessYouLongPacket(e.fields);
        }

        #region Bang hội
        else if (e.CmdID == (int) TCPGameServerCmds.CMD_KT_GUILD_CREATE)
        {
            KT_TCPHandler.ReceiveCreateGuild(e.fields);
        }
        else if (e.CmdID == (int) TCPGameServerCmds.CMD_KT_GUILD_GETINFO)
        {
            KT_TCPHandler.ReceiveGetGuildInfo(e.CmdID, e.bytesData, e.bytesData.Length);
        }
        else if (e.CmdID == (int) TCPGameServerCmds.CMD_KT_GUILD_GETMEMBERLIST)
        {
            KT_TCPHandler.ReceiveGetGuildMembers(e.CmdID, e.bytesData, e.bytesData.Length);
        }
        else if (e.CmdID == (int) TCPGameServerCmds.CMD_KT_GUILD_SKILL_LIST)
        {
            KT_TCPHandler.ReceiveGetGuildSkills(e.CmdID, e.bytesData, e.bytesData.Length);
        }
        else if (e.CmdID == (int) TCPGameServerCmds.CMD_KT_GUILD_QUEST_LIST)
        {
            KT_TCPHandler.ReceiveGetGuildQuest(e.CmdID, e.bytesData, e.bytesData.Length);
        }
        else if (e.CmdID == (int) TCPGameServerCmds.CMD_KT_GUILD_CHANGERANK)
        {
            KT_TCPHandler.ReceiveChangeGuildMemberRank(e.fields);
        }
        else if (e.CmdID == (int) TCPGameServerCmds.CMD_KT_GUILD_KICK_ROLE)
        {
            KT_TCPHandler.ReceiveKickoutMember(e.fields);
        }
        else if (e.CmdID == (int) TCPGameServerCmds.CMD_KT_GUILD_CHANGE_NOTIFY)
        {
            KT_TCPHandler.ReceiveChangeGuildNotification(e.CmdID, e.bytesData, e.bytesData.Length);
        }
        else if (e.CmdID == (int) TCPGameServerCmds.CMD_KT_GUILD_REQUEST_JOIN_LIST)
        {
            KT_TCPHandler.ReceiveGetAskToJoinList(e.CmdID, e.bytesData, e.bytesData.Length);
        }
        else if (e.CmdID == (int) TCPGameServerCmds.CMD_KT_GUILD_AUTO_ACCPECT_SETTING)
        {
            KT_TCPHandler.ReceiveSaveAutoAcceptJoinGuildSetting(e.fields);
        }
        else if (e.CmdID == (int) TCPGameServerCmds.CMD_KT_GUILD_ASKJOIN)
        {
            KT_TCPHandler.ReceiveAskToJoinGuild(e.fields);
        }
        else if (e.CmdID == (int) TCPGameServerCmds.CMD_KT_GUILD_INVITE)
        {
            KT_TCPHandler.ReceiveInviteToGuild(e.fields);
        }
        else if (e.CmdID == (int) TCPGameServerCmds.CMD_KT_UPDATE_GUILDRANK)
        {
            KT_TCPHandler.ReceiveRoleGuildRankChanged(e.fields);
        }
        else if (e.CmdID == (int) TCPGameServerCmds.CMD_KT_GUILD_OTHER_LIST)
        {
            KT_TCPHandler.ReceiveGetGuildList(e.CmdID, e.bytesData, e.bytesData.Length);
        }
        else if (e.CmdID == (int) TCPGameServerCmds.CMD_KT_GUILD_QUIT)
        {
            KT_TCPHandler.ReceiveQuitGuild(e.fields);
        }
        else if (e.CmdID == (int) TCPGameServerCmds.CMD_KT_GUILD_WARINFO)
        {
            KT_TCPHandler.ReceiveGuildWarData(e.CmdID, e.bytesData, e.bytesData.Length);
        }
        #endregion

        else if (e.CmdID == (int)(TCPGameServerCmds.CMD_KT_FACTION_PVP_RANKING_INFO))
        {
            KT_TCPHandler.ReceiveFactionBattleData(e.CmdID, e.bytesData, e.bytesData.Length);
        }

        else if (e.CmdID == (int)TCPGameServerCmds.CMD_KT_UPDATE_CURRENT_ROLETITLE)
        {
            KT_TCPHandler.ReceivePlayerRoleTitleChanged(e.fields);
        }
        else if (e.CmdID == (int)TCPGameServerCmds.CMD_KT_G2C_MOD_ROLETITLE)
        {
            KT_TCPHandler.ReceiveModRoleTitle(e.fields);
        }
        else if (e.CmdID == (int)TCPGameServerCmds.CMD_KT_G2C_UPDATE_PRESTIGE_AND_HONOR)
        {
            KT_TCPHandler.ReceivePrestigeAndHonorChanged(e.fields);
        }
        else if (e.CmdID == (int)TCPGameServerCmds.CMD_KT_G2C_PLAYERPRAY)
        {
            KT_TCPHandler.ReceivePlayerPrayData(e.fields);
        }
        else if (e.CmdID == (int)TCPGameServerCmds.CMD_KT_CLIENT_DO_REFINE)
        {
            KT_TCPHandler.ReceiveEquipRefine();
        }
        else if (e.CmdID == (int)TCPGameServerCmds.CMD_KT_C2G_SPLIT_EQUIP_INTO_FS)
        {
            KT_TCPHandler.ReceiveEquipRefineIntoFS();
        }
        else if (e.CmdID == (int)TCPGameServerCmds.CMD_KT_DO_SECONDPASSWORD_CMD)
        {
            KT_TCPHandler.ReceiveDoSecondPasswordCmd(e.fields);
        }
        else if (e.CmdID == (int)TCPGameServerCmds.CMD_KT_UPDATE_LOCALMAP_MONSTER)
        {
            KT_TCPHandler.ReceiveUpdateLocalMapSpecialMonster(e.CmdID, e.bytesData, e.bytesData.Length);
        }
        else if (e.CmdID == (int)TCPGameServerCmds.CMD_DB_TEAMBATTLE)
        {
            KT_TCPHandler.ReceiveTeamBattleRanking(e.CmdID, e.bytesData, e.bytesData.Length);
        }
        else if (e.CmdID == (int)TCPGameServerCmds.CMD_KT_CAPTCHA)
        {
            KT_TCPHandler.ReceiveCaptcha(e.CmdID, e.bytesData, e.bytesData.Length);
        }
        else if (e.CmdID == (int) TCPGameServerCmds.CMD_KT_LUCKYCIRCLE)
        {
            KT_TCPHandler.ReceiveLuckyCircleResponse(e.CmdID, e.bytesData, e.bytesData.Length);
        }
        else if (e.CmdID == (int) TCPGameServerCmds.CMD_KT_TURNPLATE)
        {
            KT_TCPHandler.ReceiveTurnPlateResponse(e.fields);
        }
        else if (e.CmdID == (int) TCPGameServerCmds.CMD_KT_NEW_PET)
        {
            PetDataMini petData = DataHelper.BytesToObject<PetDataMini>(e.bytesData, 0, e.bytesData.Length);
            if (petData == null)
            {
                return;
            }
            
            /// Thêm vào danh sách theo dõi của Leader
            Global.Data.SystemPets[petData.ID] = petData;

            /// Load pet
            this.scene.ServerNewPet(petData);

            /// Nếu chủ nhân là bản thân
            if (petData.RoleID == Global.Data.RoleData.RoleID)
            {
                /// Thiết lập lại
                Global.Data.RoleData.CurrentPetID = petData.ID - (int) ObjectBaseID.Pet;
                /// Lưu lại ID pet lần trước
                Global.Data.LastPetID = petData.ID - (int) ObjectBaseID.Pet;
            }

            /// Nếu là pet hiện tại của bản thân
            if (Global.Data.RoleData.CurrentPetID != -1 && petData.ID == Global.Data.RoleData.CurrentPetID)
            {
                /// Nếu hiển thị khung
                if (PlayZone.Instance.UIPet != null)
                {
                    /// Cập nhật máu
                    PlayZone.Instance.UIPet.UpdateHP(Global.Data.RoleData.CurrentPetID, petData.HP, petData.MaxHP);
                }
            }
        }
        else if (e.CmdID == (int) TCPGameServerCmds.CMD_KT_DEL_PET)
        {
            int id = Convert.ToInt32(e.fields[0]);

            /// Nếu là Pet của bản thân
            if (Global.Data.RoleData != null && Global.Data.RoleData.CurrentPetID + (int) ObjectBaseID.Pet == id)
            {
                /// Thiết lập lại
                Global.Data.RoleData.CurrentPetID = -1;
            }
            /// Xóa khỏi danh sách theo dõi của Leader
            Global.Data.SystemPets.Remove(id);

            this.scene.ServerDelPet(id);
        }
        else if (e.CmdID == (int) TCPGameServerCmds.CMD_KT_PET_CHANGEPOS)
        {
            int id = Convert.ToInt32(e.fields[0]);
            int posX = Convert.ToInt32(e.fields[1]);
            int posY = Convert.ToInt32(e.fields[2]);
            this.scene.ServerPetChangePos(id, posX, posY);
        }
        else if (e.CmdID == (int) TCPGameServerCmds.CMD_KT_PET_USE_SKILL)
        {
            KT_TCPHandler.ReceivePetUseSkillResult(e.fields);
        }
        else if (e.CmdID == (int) TCPGameServerCmds.CMD_KT_DO_PET_COMMAND)
        {
            KT_TCPHandler.ReceiveDoPetCommand(e.fields);
        }
        else if (e.CmdID == (int) TCPGameServerCmds.CMD_KT_GET_PET_LIST)
        {
            KT_TCPHandler.ReceiveGetPetList(e.CmdID, e.bytesData, e.bytesData.Length);
        }
        else if (e.CmdID == (int) TCPGameServerCmds.CMD_KT_PET_UPDATE_PETLIST)
        {
            KT_TCPHandler.ReceiveUpdatePetList(e.CmdID, e.bytesData, e.bytesData.Length);
        }
        else if (e.CmdID == (int) TCPGameServerCmds.CMD_KT_PET_CHANGENAME)
        {
            KT_TCPHandler.ReceiveChangePetName(e.fields);
        }
        else if (e.CmdID == (int) TCPGameServerCmds.CMD_KT_PET_STUDYSKILL)
        {
            KT_TCPHandler.ReceivePetStudySkill(e.fields);
        }
        else if (e.CmdID == (int) TCPGameServerCmds.CMD_KT_PET_UPDATE_BASE_ATTRIBUTES)
        {
            KT_TCPHandler.ReceiveUpdatePetBaseAttributes(e.fields);
        }
        else if (e.CmdID == (int) TCPGameServerCmds.CMD_KT_PET_UPDATE_ATTRIBUTES)
        {
            KT_TCPHandler.ReceiveUpdatePetAttributes(e.CmdID, e.bytesData, e.bytesData.Length);
        }
        else if (e.CmdID == (int) TCPGameServerCmds.CMD_KT_PET_RELEASE)
        {
            KT_TCPHandler.ReceiveReleasePet(e.fields);
        }
        else if (e.CmdID == (int) TCPGameServerCmds.CMD_KT_PET_UPDATE_LEVEL)
        {
            KT_TCPHandler.ReceivePetLevelChanged(e.fields);
        }
        else if (e.CmdID == (int) TCPGameServerCmds.CMD_KT_PET_GET_PLAYER_LIST)
        {
            KT_TCPHandler.ReceiveGetPlayerPetList(e.CmdID, e.bytesData, e.bytesData.Length);
        }
        else if (e.CmdID == (int) TCPGameServerCmds.CMD_KT_TOPRANKING_INFO)
        {
            KT_TCPHandler.ReceiveTopRankingInfo(e.CmdID, e.bytesData, e.bytesData.Length);
        }
        else if (e.CmdID == (int) TCPGameServerCmds.CMD_KT_TOPRANKING_GETAWARD)
        {
            KT_TCPHandler.ReceiveGetTopRankingAward(e.fields);
        }
        else if (e.CmdID == (int) TCPGameServerCmds.CMD_KT_NEW_TRADER_CARRIAGE)
        {
            TraderCarriageData data = DataHelper.BytesToObject<TraderCarriageData>(e.bytesData, 0, e.bytesData.Length);
            if (data == null)
            {
                return;
            }

            /// Thêm vào danh sách theo dõi của Leader
            Global.Data.TraderCarriages[data.RoleID] = data;

            /// Load pet
            this.scene.ServerNewTraderCarriage(data);
        }
        else if (e.CmdID == (int) TCPGameServerCmds.CMD_KT_DEL_TRADER_CARRIAGE)
        {
            int roleID = Convert.ToInt32(e.fields[0]);
            /// Xóa khỏi danh sách theo dõi của Leader
            Global.Data.TraderCarriages.Remove(roleID);

            this.scene.ServerDelTraderCarriage(roleID);
        }
        else if (e.CmdID == (int) TCPGameServerCmds.CMD_KT_NEW_CARGO_CARRIAGE_TASK)
        {
            KT_TCPHandler.ReceiveTakeNewCargoCarriageTask(e.CmdID, e.bytesData, e.bytesData.Length);
        }
        else if (e.CmdID == (int) TCPGameServerCmds.CMD_KT_UPDATE_CARGO_CARRIAGE_TASK_STATE)
        {
            KT_TCPHandler.ReceiveUpdateCargoCarriageTaskState(e.fields);
        }
        else if (e.CmdID == (int) TCPGameServerCmds.CMD_KT_GET_EXPSKILLS)
        {
            KTTCPSkillManager.ReceiveExpSkillList(e.CmdID, e.bytesData, e.bytesData.Length);
        }
        else if (e.CmdID == (int) TCPGameServerCmds.CMD_KT_SET_EXPSKILL)
        {
            KTTCPSkillManager.ReceiveSetExpSkillList(e.fields);
        }
        else if (e.CmdID == (int) TCPGameServerCmds.CMD_KT_FHLC_SCOREBOARD)
        {
            KT_TCPHandler.ReceiveFHLCScoreboard(e.CmdID, e.bytesData, e.bytesData.Length);
        }


        else if (e.CmdID == (int) TCPGameServerCmds.CMD_KT_NEW_STALLBOT)
        {
            StallBotData botData = DataHelper.BytesToObject<StallBotData>(e.bytesData, 0, e.bytesData.Length);
            if (botData == null)
            {
                return;
            }
            
            /// Thêm vào Cache
            Global.Data.StallBots[botData.RoleID] = botData;
            /// Tải xuống Bot bán hàng
            scene.ToLoadStallBot(botData);
        }

        else if (e.CmdID == (int) TCPGameServerCmds.CMD_KT_DEL_STALLBOT)
        {
            /// Loại thao tác
            int type = int.Parse(e.fields[0]);
            /// ID bot bán hàng
            int id = int.Parse(e.fields[1]);

            /// Nếu là Hủy bot khi quá tầm nhìn
            if (type == 0)
            {
                /// Xóa Bot
                this.scene.DelStallBot(id);
            }
            /// Nếu là hủy bot chủ động do hệ thống
            else
            {
                /// Xóa Bot
                this.scene.DelStallBotWithAnimation(id);
            }
        }

        #region Sạp hàng cá nhân
        else if (e.CmdID == (int) (TCPGameServerCmds.CMD_SPR_ROLE_START_STALL))
        {
            /// Dữ liệu
            StallAction data = DataHelper.BytesToObject<StallAction>(e.bytesData, 0, e.bytesData.Length);
            /// Toác
            if (data == null)
            {
                /// Bỏ qua
                return;
            }

            KT_TCPHandler.ReceiveRoleNewStall(data.Fields);
        }
        else if (e.CmdID == (int) (TCPGameServerCmds.CMD_SPR_ROLE_STOP_STALL))
        {
            KT_TCPHandler.ReceiveRoleStopStall(e.fields);
        }
        else if (e.CmdID == (int) (TCPGameServerCmds.CMD_SPR_GOODSSTALL))
        {
            /// Dữ liệu
            StallAction data = DataHelper.BytesToObject<StallAction>(e.bytesData, 0, e.bytesData.Length);
            /// Toác
            if (data == null)
            {
                /// Bỏ qua
                return;
            }

            /// Loại thao tác
            switch (data.Type)
            {
                /// Bắt đầu bày bán
                case (int) GoodsStallCmds.Start:
                {
                    KT_TCPHandler.ReceiveStartStall(data.Fields);
                    break;
                }
                /// Hủy bày bán
                case (int) GoodsStallCmds.Cancel:
                {
                    KT_TCPHandler.ReceiveStopStall();
                    break;
                }
                /// Thêm vật phẩm
                case (int) GoodsStallCmds.AddGoods:
                {
                    KT_TCPHandler.ReceiveAddItemToStall(data.GoodsData, data.Fields);
                    break;
                }
                /// Xóa vật phẩm
                case (int) GoodsStallCmds.RemoveGoods:
                {
                    KT_TCPHandler.ReceiveRemoveItemFromStall(data.Fields);
                    break;
                }
            }
        }
        else if (e.CmdID == (int) (TCPGameServerCmds.CMD_SPR_STALLDATA))
        {
            KT_TCPHandler.ReceiveStallData(e.CmdID, e.bytesData, e.bytesData.Length);
        }
        #endregion
        #endregion
    }

    #endregion

    #region Giao dịch
    /// <summary>
    /// Xử lý phản hồi giao dịch từ Server gửi về
    /// </summary>
    /// <param name="fields"></param>
    public void ProcessGoodsExchange(string[] fields)
    {
        try
        {
            int ret = Convert.ToInt32(fields[0]);
            int roleID = Convert.ToInt32(fields[1]);
            int otherRoleID = Convert.ToInt32(fields[2]);
            int exchangeType = Convert.ToInt32(fields[3]);
            /// Nếu bản thân là người mở giao dịch
            if (Global.Data.RoleData.RoleID == roleID)
            {
                /// Nếu đối phượng không tồn tại
                if (!Global.Data.OtherRoles.ContainsKey(otherRoleID))
                {
                    return;
                }

                /// Thông tin đối phương
                RoleData roleData = Global.Data.OtherRoles[otherRoleID];

                /// Nếu là mời giao dịch
                if (exchangeType == (int) (GoodsExchangeCmds.Request))
                {
                    if (ret < 0)
                    {
                        if (-1 == ret)
                        {
                            KTGlobal.AddNotification("Đối phương đã Offline, không thể thực hiện giao dịch!");
                        }
                        else if (-2 == ret)
                        {
                            KTGlobal.AddNotification("Đối phương đang thực hiện giao dịch cùng người khác!");
                        }
                        else if (-3 == ret || -4 == ret)
                        {
                            KTGlobal.AddNotification("Khoảng cách đến đối phương quá xa!");
                        }
                        else if (-10 == ret)
                        {
                            KTGlobal.AddNotification("Đã gửi yêu cầu giao dịch đến đối phương trước đó, hãy kiên nhẫn chờ phản hồi!");
                        }
                        else
                        {
                            KTGlobal.AddNotification(string.Format("Gửi yêu cầu giao dịch thất bại, mã lỗi: {0}!", ret));
                        }
                    }
                    else
                    {
                        /// Cập nhật ID phiên giao dịch
                        Global.Data.ExchangeID = ret;
                    }
                }
                /// Nếu là từ chối
                else if (exchangeType == (int) (GoodsExchangeCmds.Refuse))
                {
                }
                /// Nếu là đồng ý
                else if (exchangeType == (int) (GoodsExchangeCmds.Agree))
                {
                    if (ret < 0)
                    {
                        if (-1 == ret)
                        {
                            KTGlobal.AddNotification("Đối phương đã Offline, không thể giao dịch!");
                        }
                        else if (-2 == ret)
                        {
                            KTGlobal.AddNotification("Đối phương đang thực hiện giao dịch cùng người khác!");
                        }
                        else if (-3 == ret || -4 == ret)
                        {
                            KTGlobal.AddNotification("Khoảng cách đến đối phương quá xa!");
                        }
                        else
                        {
                            KTGlobal.AddNotification(string.Format("Giao dịch thất bại, mã lỗi: {0}!", ret));
                        }
                    }
                    else
                    {
                        /// Mở khung giao dịch
                        this.ShowUIExchange(otherRoleID);
                    }
                }
                /// Nếu là từ chối
                else if (exchangeType == (int) (GoodsExchangeCmds.Cancel))
                {
                }
                /// Nếu là thêm vật phẩm
                else if (exchangeType == (int) (GoodsExchangeCmds.AddGoods))
                {
                }
                /// Nếu là xóa vật phẩm
                else if (exchangeType == (int) (GoodsExchangeCmds.RemoveGoods))
                {
                }
                /// Nếu là thêm tiền
                else if (exchangeType == (int) (GoodsExchangeCmds.UpdateMoney))
                {
                }
                /// Nếu là khóa
                else if (exchangeType == (int) (GoodsExchangeCmds.Lock))
                {
                }
                /// Nếu là mở khóa
                else if (exchangeType == (int) (GoodsExchangeCmds.Unlock))
                {
                    KTGlobal.AddNotification(string.Format("Bạn đã hủy xác nhận giao dịch với {0}", roleData.RoleName));
                }
                /// Nếu là hoàn tất giao dịch
                else if (exchangeType == (int) (GoodsExchangeCmds.Done))
                {
                    /// Đóng khung giao dịch
                    this.CloseUIExchange();

                    /// Xóa phiên giao dịch
                    Global.Data.ExchangeID = -1;
                    /// Xóa thông tin giao dịch
                    Global.Data.ExchangeDataItem = null;

                    if (ret >= 0)
                    {
                        KTGlobal.AddNotification("Giao dịch thành công!");
                    }
                    else
                    {
                        if (-1 == ret)
                        {
                            KTGlobal.AddNotification("Giao dịch thất bại, túi đồ của đối phương đã đầy!");
                        }
                        else if (-1001 == ret)
                        {
                            KTGlobal.AddNotification("Giao dịch thất bại, túi đồ của đối phương đã đầy!");
                        }
                        else if (-11 == ret)
                        {
                            KTGlobal.AddNotification("Giao dịch thất bại, túi đồ của bạn đã đầy!");
                        }
                        else if (-1011 == ret)
                        {
                            KTGlobal.AddNotification("Giao dịch thất bại, túi đồ của bạn đã đầy!");
                        }
                        else if (-2 == ret)
                        {
                            KTGlobal.AddNotification("Giao dịch thất bại, số bạc mang theo không đủ!");
                        }
                        else if (-12 == ret)
                        {
                            KTGlobal.AddNotification("Giao dịch thất bại, số bạc của đối phương mang theo không đủ!");
                        }
                    }
                }
            }
            else
            {
                /// Nếu thông tin đối phương không tồn tại
                if (!Global.Data.OtherRoles.ContainsKey(roleID))
                {
                    return;
                }

                /// Thông tin đối phương
                RoleData roleData = Global.Data.OtherRoles[roleID];

                /// Nếu là yêu cầu giao dịch
                if (exchangeType == (int) (GoodsExchangeCmds.Request))
                {
                    /// Nếu có thiết lập từ chối giao dịch
                    if (KTAutoAttackSetting.Config.General.RefuseExchange)
                    {
                        GameInstance.Game.SpriteGoodsExchange(roleID, (int) (GoodsExchangeCmds.Refuse), ret);
                        return;
                    }

                    /// Mở bảng yêu cầu mở giao dịch
                    KTGlobal.ShowMessageBox("Yêu cầu giao dịch", string.Format("<color=#47b9ff>{0}</color> muốn giao dịch cùng bạn, xác nhận không?", roleData.RoleName), () => {
                        Global.Data.ExchangeID = ret;
                        GameInstance.Game.SpriteGoodsExchange(roleID, (int) (GoodsExchangeCmds.Agree), ret);
                    }, () => {
                        GameInstance.Game.SpriteGoodsExchange(roleID, (int) (GoodsExchangeCmds.Refuse), ret);
                    });
                }
                /// Nếu là từ chối giao dịch
                else if (exchangeType == (int) (GoodsExchangeCmds.Refuse))
                {
                    KTGlobal.AddNotification("Đối phương từ chối giao dịch cùng bạn!");
                }
                /// Nếu là đồng ý giao dịch
                else if (exchangeType == (int) (GoodsExchangeCmds.Agree))
                {
                    if (ret < 0)
                    {
                        if (-1 == ret)
                        {
                            KTGlobal.AddNotification("Đối phương đã Offline, không thể thực hiện giao dịch!");
                        }
                        else if (-2 == ret)
                        {
                            KTGlobal.AddNotification("Đối phương đang tiến hành giao dịch cùng người khác!");
                        }
                        else if (-3 == ret || -4 == ret)
                        {
                            KTGlobal.AddNotification("Khoảng cách quá xa!");
                        }
                        else if (-10 == ret)
                        {
                            KTGlobal.AddNotification("Bạn đang thực hiện giao dịch, không thể thực hiện thêm!");
                        }
                        else
                        {
                            KTGlobal.AddNotification(string.Format("Giao dịch thất bại, mã lỗi: {0}!", ret));
                        }
                    }
                    else
                    {
                        /// Mở khung giao dịch
                        this.ShowUIExchange(roleID);
                    }
                }
                /// Nếu là hủy giao dịch
                else if (exchangeType == (int) (GoodsExchangeCmds.Cancel))
                {
                    /// Đóng khung giao dịch
                    this.CloseUIExchange();

                    /// Xóa phiên giao dịch
                    Global.Data.ExchangeID = -1;
                    /// Xóa dữ liệu phiên giao dịch
                    Global.Data.ExchangeDataItem = null;

                    KTGlobal.AddNotification("Đối phương đã hủy giao dịch!");
                }
                /// Nếu là thêm vật phẩm
                else if (exchangeType == (int) (GoodsExchangeCmds.AddGoods))
                {
                }
                /// Nếu là xóa vật phẩm
                else if (exchangeType == (int) (GoodsExchangeCmds.RemoveGoods))
                {
                }
                /// Nếu là thêm tiền
                else if (exchangeType == (int) (GoodsExchangeCmds.UpdateMoney))
                {
                }
                /// Nếu là khóa
                else if (exchangeType == (int) (GoodsExchangeCmds.Lock))
                {
                }
                /// Nếu là bỏ khóa
                else if (exchangeType == (int) (GoodsExchangeCmds.Unlock))
                {
                    KTGlobal.AddNotification("Đối phương đã hủy xác nhận!");
                }
                else if (exchangeType == (int) (GoodsExchangeCmds.Done))
                {
                    /// Đóng khung giao dịch
                    this.CloseUIExchange();

                    /// Xóa phiên giao dịch
                    Global.Data.ExchangeID = -1;
                    /// Xóa dữ liệu phiên giao dịch
                    Global.Data.ExchangeDataItem = null;

                    if (ret >= 0)
                    {
                        KTGlobal.AddNotification("Giao dịch thành công!");
                    }
                    else
                    {
                        if (-1 == ret)
                        {
                            KTGlobal.AddNotification("Giao dịch thất bại, túi của bạn đã đầy!");
                        }
                        else if (-11 == ret)
                        {
                            KTGlobal.AddNotification("Giao dịch thất bại, túi của đối phương đã đầy!");
                        }
                        else if (-2 == ret)
                        {
                            KTGlobal.AddNotification("Giao dịch thất bại, số bạc của đối phương mang theo không đủ!");
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            KTDebug.LogException(e);
        }
    }
    #endregion

    #region Chuyển bản đồ
    /// <summary>
    /// Hiển thị màn hình tải bản đồ
    /// </summary>
    protected void ShowLoadingMap(Action done = null)
    {
        /// Xóa toàn bộ đối tượng trong bản đồ cũ
        this.ClearScene();
        /// Cập nhật thời điểm tải bản đồ xong
        KTGlobal.LastChangeMapSuccessfulTicks = -1;
        Super.DestroyLoadingMap();
        Super.ShowLoadingMap(() => {
            this.DoneLoadingMap();
            done?.Invoke();
        });
    }

    /// <summary>
    /// Hoàn tất tải xuống bản đồ
    /// </summary>
    private void DoneLoadingMap()
    {
        this.StartPlayGame();
    }

    /// <summary>
    /// Thực hiện lệnh chuyển Map từ GS trả về
    /// </summary>
    /// <param name="roleID"></param>
    /// <param name="teleportID"></param>
    /// <param name="mapCode"></param>
    /// <param name="mapX"></param>
    /// <param name="mapY"></param>
    /// <param name="direction"></param>
    public void ServerMapConversion(int roleID, int teleportID, int mapCode, int mapX, int mapY, int direction)
    {
        try
        {
            if (Global.Data.RoleData.RoleID == roleID)
            {
                Global.Data.RoleData.MapCode = mapCode;
                Global.Data.RoleData.PosX = mapX;
                Global.Data.RoleData.PosY = mapY;
                Global.Data.RoleData.RoleDirection = direction;

                /// Hiển thị màn hình tải bản đồ
                this.ShowLoadingMap();
            }
        }
        catch (Exception e)
        {
            KTDebug.LogException(e);
        }
    }
    #endregion

    #region Socket
    /// <summary>
    /// Khởi tạo Socket
    /// </summary>
    protected void InitNetwork()
    {
        GameInstance.Game.SocketFailed += GameSocketFailed;
        GameInstance.Game.SocketSuccess += GameSocketSuccess;
        GameInstance.Game.SocketCommand += GameSocketCommand;
    }
    /// <summary>
    /// Làm mới Socket
    /// </summary>
    protected void ClearNetwork()
    {
        GameInstance.Game.SocketFailed -= GameSocketFailed;
        GameInstance.Game.SocketSuccess -= GameSocketSuccess;
        GameInstance.Game.SocketCommand -= GameSocketCommand;
    }

    /// <summary>
    /// Đóng Socket
    /// </summary>
    protected void CloseNetwork()
    {
        ClearNetwork();
        GameInstance.Game.Disconnect();
    }

    #endregion
}
