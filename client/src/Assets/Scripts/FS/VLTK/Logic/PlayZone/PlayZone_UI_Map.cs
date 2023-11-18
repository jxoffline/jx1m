using FS.Drawing;
using FS.GameEngine.Logic;
using FS.GameEngine.Network;
using FS.GameEngine.Sprite;
using FS.VLTK;
using FS.VLTK.Control.Component;
using FS.VLTK.Entities.Config;
using FS.VLTK.Loader;
using FS.VLTK.Logic;
using FS.VLTK.Logic.Settings;
using FS.VLTK.Network;
using FS.VLTK.UI;
using FS.VLTK.UI.Main.LocalMap;
using FS.VLTK.UI.Main.MainUI;
using Server.Data;
using System;
using System.Linq;
using UnityEngine;
using static FS.VLTK.Entities.Enum;

/// <summary>
/// Quản lý các khung giao diện trong màn chơi
/// </summary>
public partial class PlayZone
{
    #region Radar Map
    /// <summary>
    /// Radar Map
    /// </summary>
    public UIRadarMap UIRadarMap { get; private set; } = null;

    /// <summary>
    /// Khởi tạo Radar Map
    /// </summary>
    protected void InitRadarMap()
    {
        if (this.UIRadarMap != null)
        {
            GameObject.Destroy(this.UIRadarMap.gameObject);
        }

        CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
        this.UIRadarMap = canvas.LoadUIPrefab<UIRadarMap>("MainGame/MainUI/UIRadarMap");
        canvas.AddMainUI(this.UIRadarMap);

        this.UIRadarMap.GoToLocalMap = () => {
            this.ShowWorldNavigationWindow();
        };
        this.UIRadarMap.UseItem = (itemGD) => {
            /// Nếu đang trong trạng thái bị khống chế
            if (!Global.Data.Leader.CanDoLogic)
            {
                KTGlobal.AddNotification("Trong trạng thái bị khống chế không thể sử dụng vật phẩm!");
                return;
            }

            /// Cấu hình vật phẩm
            ItemData itemData = null;
            if (!Loader.Items.TryGetValue(itemGD.GoodsID, out itemData))
            {
                KTGlobal.AddNotification("Vật phẩm bị lỗi, hãy thông báo với hỗ trợ để được xử lý!");
                return;
            }

            /// Nếu không thể sử dụng, và không phải thuốc
            if (!itemData.IsScriptItem && !itemData.IsMedicine)
            {
                KTGlobal.AddNotification("Vật phẩm này không thể sử dụng được!");
                return;
            }

            /// Sử dụng
            GameInstance.Game.SpriteUseGoods(itemGD.Id);
        };
        this.UIRadarMap.ItemSelected = (items) => {

            /// Lưu thiết lập vào hệ thống
            KT_TCPHandler.SendSaveQuickItems();
        };
        this.LoadRadarMap();
    }

    /// <summary>
    /// Tải xuống bản đồ
    /// </summary>
    protected void LoadRadarMap()
    {
        if (this.UIRadarMap == null)
        {
            return;
        }

        try
        {
            if (FS.VLTK.Loader.Loader.Maps.TryGetValue(this.scene.MapCode, out FS.VLTK.Entities.Config.Map map))
            {
                this.UIRadarMap.MapName = map.Name;
            }
        }
        catch (Exception ex)
        {
            KTGlobal.ShowMessageBox("Lỗi phát sinh", "Lỗi phát sinh khi tải bản đồ thu nhỏ của bản đồ: " + ex.Message + ". Hãy báo lại với hỗ trợ, sau đó ấn OK để bỏ qua", true);
        }
    }
    #endregion

    #region Bản đồ
    /// <summary>
    /// Bản đồ
    /// </summary>
    public UILocalMap UILocalMap { get; private set; }

    /// <summary>
    /// Đóng bản đồ
    /// </summary>
    protected void CloseWorldNavigationWindow()
    {
        if (this.UILocalMap != null)
        {
            this.UILocalMap.Hide();
        }
    }

    /// <summary>
    /// Hiển thị bản đồ
    /// </summary>
    /// <param name="caching"></param>
    protected void ShowWorldNavigationWindow()
    {
        if (this.UILocalMap == null)
        {
            this.UILocalMap = CanvasManager.Instance.LoadUIPrefab<UILocalMap>("MainGame/UILocalMap");
            CanvasManager.Instance.AddUI(this.UILocalMap);
        }

        if (Loader.Maps.TryGetValue(this.scene.MapCode, out FS.VLTK.Entities.Config.Map map))
        {
            this.UILocalMap.RealMapSize = new Vector2(map.Width, map.Height);
            this.UILocalMap.LocalMapName = map.Name;
            this.UILocalMap.LocalMapSprite = Global.CurrentMap.LocalMapSprite;
            this.UILocalMap.ListNPCs = this.scene.CurrentMapData.MinimapNPCList;
            this.UILocalMap.ListTeleport = this.scene.CurrentMapData.Teleport;
            this.UILocalMap.ListTrainArea = this.scene.CurrentMapData.MinimapMonsterList;
            this.UILocalMap.ListZone = this.scene.CurrentMapData.Zone;
            this.UILocalMap.ListGrowPoint = this.scene.CurrentMapData.MinimapGrowPointList;
            this.UILocalMap.LocalMapClicked = (position) => {
                /// Nếu đang trong trạng thái khinh công thì không thao tác
                if (Global.Data.Leader.CurrentAction == KE_NPC_DOING.do_jump)
                {
                    return;
                }
                /// Nếu Leader đã chết
                else if (Global.Data.Leader.IsDeath || Global.Data.Leader.HP <= 0)
                {
                    return;
                }
                /// Nếu đang bày bán
                else if (Global.Data.StallDataItem != null && Global.Data.StallDataItem.Start == 1 && !Global.Data.StallDataItem.IsBot)
                {
                    KTGlobal.AddNotification("Trong trạng thái bán hàng không thể di chuyển!");
                    return;
                }
                /// Nếu Leader đang bị khóa bởi kỹ năng
                else if (!Global.Data.Leader.CanPositiveMove)
                {
                    KTGlobal.AddNotification("Đang trong trạng thái bị khống chế, không thể di chuyển!");
                    return;
                }
                /// Nếu chưa thực hiện xong động tác trước
                else if (!Global.Data.Leader.IsReadyToMove)
                {
                    return;
                }
                /// Nếu đang trong thời gian thực hiện động tác dùng kỹ năng
                else if (!KTGlobal.FinishedUseSkillAction)
                {
                    return;
                }
                /// Nếu đang đợi dùng kỹ năng
                else if (SkillManager.IsWaitingToUseSkill)
                {
                    return;
                }

                /// Nếu có ngựa nhưng không trong trạng thái cưỡi
                GoodsData horseGD = Global.Data.RoleData.GoodsDataList?.Where(x => x.Using == (int) KE_EQUIP_POSITION.emEQUIPPOS_HORSE).FirstOrDefault();
                if (horseGD != null && !Global.Data.Leader.ComponentCharacter.Data.IsRiding)
                {
                    KT_TCPHandler.SendChangeToggleHorseState();
                }

                /// TODO xử lý thêm nếu có Truyền Tống Phù

                KTLeaderMovingManager.AutoFindRoad(new Point((int) position.x, (int) position.y));
            };
            this.UILocalMap.Close = () => {
                this.CloseWorldNavigationWindow();
            };
            this.UILocalMap.GoToMap = (mapCode) => {
                /// Ẩn khung
                this.UILocalMap.Hide();

                KTGlobal.QuestAutoFindPathToMap(mapCode, () => {
                    AutoQuest.Instance.StopAutoQuest();
                    AutoPathManager.Instance.StopAutoPath();
                });
            };
            this.UILocalMap.Show();
        }
    }
    #endregion
}
