using FS.GameEngine.Logic;
using FS.GameEngine.Network;
using FS.VLTK.Control.Component;
using FS.VLTK.Entities.Config;
using FS.VLTK.Network;
using FS.VLTK.Utilities.Threading;
using Server.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK.Logic
{
    /// <summary>
    /// Xử lý Logic tìm đường
    /// </summary>
    public partial class AutoPathManager
    {
        #region Constants
        /// <summary>
        /// Thời gian nghỉ kiểm tra tự tìm đường
        /// </summary>
        private const float AutoPathSleepTime = 1f;
        #endregion

        #region Private fields
        /// <summary>
        /// Đường đi hiện tại đang chạy
        /// NULL nếu không có đường đi
        /// </summary>
        private List<int> currentPaths = null;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện hoàn tất quãng đường dịch chuyển
        /// </summary>
        public Action FinishMoveByPaths { get; set; } = null;
        #endregion

        #region Private methods
        /// <summary>
        /// Ẩn dòng chữ tự tìm đường
        /// </summary>
        private void HideTextAutoFindPath()
        {
            if (PlayZone.Instance != null)
            {
                PlayZone.Instance.HideTextAutoFindPath();
            }
        }

        /// <summary>
        /// Hiện dòng chữ tự tìm đường
        /// </summary>
        private void ShowTextAutoFightPath()
        {
            if (PlayZone.Instance != null)
            {
                PlayZone.Instance.ShowTextAutoFindPath();
            }
        }

        /// <summary>
        /// Luồng thực hiện Logic dịch chuyển
        /// </summary>
        /// <returns></returns>
        private IEnumerator DoLogic()
        {
            while (true)
            {
                /// Nếu không phải trong màn hình Game hoặc không có Leader
                if (Global.Data == null || Global.Data.GameScene == null || Global.Data.Leader == null)
                {
                    /// Nghỉ 1 lát
                    yield return new WaitForSeconds(AutoPathManager.AutoPathSleepTime);
                    /// Tiếp tục vòng lặp
                    continue;
                }
                /// Nếu đang đợi tải xuống bản đồ
                else if (Global.Data.WaitingForMapChange || (KTGlobal.LastChangeMapSuccessfulTicks != -1 && KTGlobal.GetCurrentTimeMilis() - KTGlobal.LastChangeMapSuccessfulTicks < 1000))
                {
                    /// Nghỉ 1 lát
                    yield return new WaitForSeconds(AutoPathManager.AutoPathSleepTime);
                    /// Tiếp tục vòng lặp
                    continue;
                }
                /// Nếu không có đường đi
                else if (this.currentPaths == null)
                {
                    /// Nghỉ 1 lát
                    yield return new WaitForSeconds(AutoPathManager.AutoPathSleepTime);
                    /// Tiếp tục vòng lặp
                    continue;
                }
                /// Nếu đang di chuyển
                else if (Global.Data.Leader.IsMoving)
                {
                    /// Nghỉ 1 lát
                    yield return new WaitForSeconds(AutoPathManager.AutoPathSleepTime);
                    /// Tiếp tục vòng lặp
                    continue;
                }

                /// Nếu có ngựa nhưng không trong trạng thái cưỡi
                GoodsData horseGD = Global.Data.RoleData.GoodsDataList?.Where(x => x.Using == (int) KE_EQUIP_POSITION.emEQUIPPOS_HORSE).FirstOrDefault();
                if (horseGD != null && !Global.Data.Leader.ComponentCharacter.Data.IsRiding)
                {
                    KT_TCPHandler.SendChangeToggleHorseState();
                }

                /// nếu không phải đang bán đồ thì hủy tự đánh
                if (!KTAutoFightManager.Instance.DoingAutoSell && !KTAutoFightManager.Instance.DoingBuyItem)
                {  
                    /// Hủy tự động đánh
                    KTAutoFightManager.Instance.StopAutoFight();
                }    
              
                /// Hiện dòng chữ tự tìm đường
                this.ShowTextAutoFightPath();

                /// Vị trí bản đồ hiện tại trong danh sách đường đi
                int currentMapPos = this.currentPaths.IndexOf(Global.Data.RoleData.MapCode);
                /// Nếu đã vượt quá kích thước gốc
                if (currentMapPos >= this.currentPaths.Count - 1)
                {
                    /// Nghỉ 1 lát
                    yield return new WaitForSeconds(AutoPathManager.AutoPathSleepTime);

                    //KTDebug.LogError("Finish => Callback is NULL: " + this.FinishMoveByPaths == null);
                    /// Thực hiện hàm Callback dịch chuyển hoàn tất
                    this.FinishMoveByPaths?.Invoke();
                    /// Ngừng tự tìm đường
                    this.StopAutoPath();
                    /// Nghỉ 1 lát
                    yield return new WaitForSeconds(AutoPathManager.AutoPathSleepTime);
                    /// Tiếp tục vòng lặp
                    continue;
                }

                /// ID bản đồ tiếp theo
                int nextMapCode = this.currentPaths[currentMapPos + 1];

                /// Tìm danh sách cạnh có cùng trọng số nhỏ nhất
                if (this.teleportItemUsing != -1)
                {
                    /// Nếu không có truyền tống phù tương ứng
                    if (!KTGlobal.HaveItem(this.teleportItemUsing))
                    {
                        KTGlobal.AddNotification("Không có truyền tống phù hoặc đã hết hạn sử dụng, chuyển qua tìm đường thường!");
                        /// Chuyển qua tìm đường không dùng truyền tống phù
                        this.FindPathWithoutTeleportItem(Global.Data.RoleData.MapCode, this.currentPaths.Last());
                    }
                    else
                    {
                        /// Danh sách đường đi thỏa mãn từ vị trí hiện tại
                        List<AutoPathXML.Node> edges = this.teleportItemEdges[this.teleportItemUsing][Global.Data.RoleData.MapCode].Where(x => x.ToMapCode == nextMapCode).ToList();
                        /// Nếu không tìm thấy đường đi
                        if (edges == null)
                        {
                            /// Hủy dòng chữ tự tìm đường
                            this.HideTextAutoFindPath();
                            /// Thông báo không có đường đi
                            KTGlobal.AddNotification("Không tìm thấy đường đi, hãy thử lại!");
                            /// Ngừng tự tìm đường
                            this.StopAutoPath();
                            /// Nghỉ 1 lát
                            yield return new WaitForSeconds(AutoPathManager.AutoPathSleepTime);
                            /// Tiếp tục vòng lặp
                            continue;
                        }

                        /// Lấy đường đi có trọng số ngắn nhất
                        int minWeight = edges.Min(x => x.Weight);
                        /// Duyệt danh sách cạnh có trọng số nhỏ nhất, lấy vị trí có khoảng cách so với hiện tại ngắn nhất
                        AutoPathXML.Node edge = edges.Where(x => x.Weight == minWeight).MinBy(x => Vector2.Distance(new Vector2(x.PosX, x.PosY), Global.Data.Leader.PositionInVector2));

                        /// Nếu không cần vị trí
                        if (edge.PosX == -1 && edge.PosY == -1)
                        {
                            KT_TCPHandler.SendAutoPathChangeMap(edge.ToMapCode, this.teleportItemUsing, false);
                        }
                        else
                        {
                            /// Vị trí điểm hiện tại và điểm đích đến
                            Vector2 edgePos = new Vector2(edge.PosX, edge.PosY);
                            Vector2 selfPos = Global.Data.Leader.PositionInVector2;

                            /// Nếu chưa đến đích
                            if (Vector2.Distance(edgePos, selfPos) > 10)
                            {
                                /// Thực hiện tìm đường từ vị trí hiện tại đến vị trí cạnh tương ứng
                                KTLeaderMovingManager.AutoFindRoad(new Drawing.Point(edge.PosX, edge.PosY), () =>
                                {
                                    /// Nếu đây không phải dịch chuyển tại cổng teleport
                                    if (edge.Weight != (int) AutoPathXML.PathFindingPriority.Teleport)
                                    {
                                        /// Vị trí hiện tại
                                        selfPos = Global.Data.Leader.PositionInVector2;
                                        /// Kiểm tra lại khoảng cách lần nữa để chắc chắn không có phát sinh lỗi gì
                                        float distance = Vector2.Distance(edgePos, selfPos);
                                        /// Nếu khoảng cách thỏa mãn
                                        if (distance <= 10)
                                        {
                                            /// Đánh dấu đang đợi Auto chuyển Map
                                            KT_TCPHandler.SendAutoPathChangeMap(edge.ToMapCode, this.teleportItemUsing, edge.Weight == (int) AutoPathXML.PathFindingPriority.TransferNPC);
                                        }
                                    }
                                });
                            }
                            /// Nếu đã đến đích
                            else
                            {
                                /// Đánh dấu đang đợi Auto chuyển Map
                                KT_TCPHandler.SendAutoPathChangeMap(edge.ToMapCode, this.teleportItemUsing, edge.Weight == (int) AutoPathXML.PathFindingPriority.TransferNPC);
                            }
                        }
                    }
                }
                else
                {
                    /// Danh sách đường đi thỏa mãn từ vị trí hiện tại
                    List<AutoPathXML.Node> edges = this.commonEdges[Global.Data.RoleData.MapCode].Where(x => x.ToMapCode == nextMapCode).ToList();
                    /// Nếu không tìm thấy đường đi
                    if (edges == null)
                    {
                        /// Hủy dòng chữ tự tìm đường
                        this.HideTextAutoFindPath();
                        /// Thông báo không có đường đi
                        KTGlobal.AddNotification("Không tìm thấy đường đi, hãy thử lại!");
                        /// Ngừng tự tìm đường
                        this.StopAutoPath();
                        /// Nghỉ 1 lát
                        yield return new WaitForSeconds(AutoPathManager.AutoPathSleepTime);
                        /// Tiếp tục vòng lặp
                        continue;
                    }

                    /// Lấy đường đi có trọng số ngắn nhất
                    int minWeight = edges.Min(x => x.Weight);
                    /// Duyệt danh sách cạnh có trọng số nhỏ nhất, lấy vị trí có khoảng cách so với hiện tại ngắn nhất
                    AutoPathXML.Node edge = edges.Where(x => x.Weight == minWeight).MinBy(x => Vector2.Distance(new Vector2(x.PosX, x.PosY), Global.Data.Leader.PositionInVector2));

                    /// Vị trí điểm hiện tại và điểm đích đến
                    Vector2 edgePos = new Vector2(edge.PosX, edge.PosY);
                    Vector2 selfPos = Global.Data.Leader.PositionInVector2;

                    /// Nếu chưa đến đích
                    if (Vector2.Distance(edgePos, selfPos) > 10)
                    {
                        /// Thực hiện tìm đường từ vị trí hiện tại đến vị trí cạnh tương ứng
                        KTLeaderMovingManager.AutoFindRoad(new Drawing.Point(edge.PosX, edge.PosY), () =>
                        {
                            /// Nếu đây không phải dịch chuyển tại cổng teleport
                            if (edge.Weight != (int) AutoPathXML.PathFindingPriority.Teleport)
                            {
                                /// Vị trí hiện tại
                                selfPos = Global.Data.Leader.PositionInVector2;
                                /// Kiểm tra lại khoảng cách lần nữa để chắc chắn không có phát sinh lỗi gì
                                float distance = Vector2.Distance(edgePos, selfPos);
                                /// Nếu khoảng cách thỏa mãn
                                if (distance <= 10)
                                {
                                    /// Đánh dấu đang đợi Auto chuyển Map
                                    KT_TCPHandler.SendAutoPathChangeMap(edge.ToMapCode, -1, true);
                                }
                            }
                        });
                    }
                    /// Nếu đã đến đích
                    else
                    {
                        /// Đánh dấu đang đợi Auto chuyển Map
                        KT_TCPHandler.SendAutoPathChangeMap(edge.ToMapCode, -1, true);
                    }
                }

                /// Nghỉ 1 lát
                yield return new WaitForSeconds(AutoPathManager.AutoPathSleepTime);
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Ngừng tự tìm đường
        /// </summary>
        public void StopAutoPath()
        {
            if (this.currentPaths != null)
			{
                this.currentPaths.Clear();
            }
            this.currentPaths = null;
            this.FinishMoveByPaths = null;
            //KTGlobal.AddNotification("<color=red>Ngừng tự tìm đường.</color>");
        }
        #endregion
    }
}
