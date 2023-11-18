using GameServer.Core.Executor;
using GameServer.KiemThe;
using GameServer.KiemThe.Core.Activity.RechageEvent;
using GameServer.KiemThe.Core.IconStateManager;
using GameServer.KiemThe.Logic;
using GameServer.Server;
using Server.Data;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace GameServer.Logic.RefreshIconState
{
    /// summary
    /// Kiểm tra trạng thái ICON của client. Thời gian đếm ngược. Lóe Sáng..vvv
    /// summary
    public class IconStateManager
    {
        /// <summary>
        /// Đối tượng quản lý
        /// </summary>
        private static IconManager _Event = new IconManager();

        /// <summary>
        /// File XML quy định
        /// </summary>
        private const string KTEveryDayEvent_XML = "Config/KT_Activity/KTIconManager.xml";

        /// <summary>
        /// Danh sách trạng thái Icon
        /// </summary>
        private readonly Dictionary<int, int> IconState = new Dictionary<int, int>();

        /// <summary>
        /// Thiết lập
        /// </summary>
        public static void Setup()
        {
            string Files = KTGlobal.GetDataPath(IconStateManager.KTEveryDayEvent_XML);

            using (var stream = System.IO.File.OpenRead(Files))
            {
                var serializer = new XmlSerializer(typeof(IconManager));
                _Event = serializer.Deserialize(stream) as IconManager;
            }
        }

        /// <summary>
        /// Đồng bộ Icon về Client
        /// </summary>
        /// <param name="client"></param>
        private void SysnIcon(KPlayer client)
        {
            IconState.Clear();

            List<MainButton> Icons = _Event.Icons;

            foreach (MainButton _Icon in Icons)
            {
                IconState.Add((int) _Icon.IconID, (int) GetIconState(_Icon.IconID, client));
            }

            ActivityIconStateData _IconState = new ActivityIconStateData();
            _IconState.IconState = IconState;

            client.SendPacket<ActivityIconStateData>((int)TCPGameServerCmds.CMD_SPR_REFRESH_ICON_STATE, _IconState);
        }

        /// <summary>
        /// Trả về trạng thái icon button tương ứng
        /// </summary>
        /// <param name="IconID"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        private FunctionButtonAction GetIconState(FunctionButtonType IconID, KPlayer client)
        {
            FunctionButtonAction state = FunctionButtonAction.Show;
            switch (IconID)
            {
                case FunctionButtonType.OpenTokenShop:
                {
                    if (this.HasDiscount()) // Nếu có shop giảm giá
                    {
                        state = FunctionButtonAction.Hint;
                    }
                    break;
                }
                case FunctionButtonType.OpenRoleInfo:
                {
                    if (this.IsHaveRemainProtect(client)) // Nếu có điểm tiềm năng chưa phân phối
                    {
                        state = FunctionButtonAction.Hint;
                    }
                    break;
                }
                case FunctionButtonType.OpenBag:
                {
                    if (this.BagFull(client))  //  Nếu đầy túi đồ thì cảnh báo
                    {
                        state = FunctionButtonAction.Hint;
                    }
                    break;
                }
                case FunctionButtonType.OpenSkill:
                {
                    if (this.IsHaveSkillPoint(client))  //  NẾu có điểm skill chưa cộng
                    {
                        state = FunctionButtonAction.Hint;
                    }
                    break;
                }
                case FunctionButtonType.OpenTaskBox:
                {
                    if (this.HasCompleteTask(client))  //  Nếu có quest có thể trả
                    {
                        state = FunctionButtonAction.Show;
                    }
                    break;
                }
                case FunctionButtonType.OpenGuildBox:
                {
                    if (this.HasGuildNotify(client))  //  Nếu bang hội có thể notify
                    {
                        state = FunctionButtonAction.Hint;
                    }
                    break;
                }
                  
                case FunctionButtonType.OpenWelfareFirstRecharge: // Cái nạp đầu 
                {
                    int State = this.HasWelfareFirstRecharge(client);

                    if (State == 2)
                    {
                        state = FunctionButtonAction.Hide;
                    }
                    else if (State == 1)
                    {
                        state = FunctionButtonAction.Hint;
                    }
                    else
                    {
                        state = FunctionButtonAction.Show;
                    }
                    break;
                }
                case FunctionButtonType.OpenWelfare:
                {
                    if (this.HasWelfare(client)) // Nếu có phúc lợi chưa nhận
                    {
                        state = FunctionButtonAction.Hint;
                    }
                    break;
                }
                case FunctionButtonType.OpenActivityList:
                {
                    if (this.HasActivity(client)) // Nếu có hoạt động có thể tham gia
                    {
                        state = FunctionButtonAction.Hint;
                    }
                    break;
                }
                case FunctionButtonType.OpenMailBox:
                {
                    if (KT_TCPHandler.CheckEmailCount(client)) // Nếu có email chưa đọc thì sẽ báo
                    {
                        state = FunctionButtonAction.Hint;
                    }
                    break;
                }
                default:
                {
                    state = FunctionButtonAction.Show;
                    break;
                }
            }

            return state;
        }

        /// <summary>
        /// Có giảm giá không
        /// </summary>
        /// <returns></returns>
        private bool HasDiscount()
        {
            return false;
        }

        /// <summary>
        /// Có phúc lợi không
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        private bool HasWelfare(KPlayer client)
        {




            return false;
        }

        /// <summary>
        /// Có hoạt động gì không
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        private bool HasActivity(KPlayer client)
        {
            return false;
        }

        /// <summary>
        /// Có nhiệm vụ đã hoàn thành không
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        private bool HasCompleteTask(KPlayer client)
        {
            return false;
        }

        /// <summary>
        /// Có phúc lợi nạp đầu không
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        private int HasWelfareFirstRecharge(KPlayer client)
        {
           int totalChongZhiMoney = KT_TCPHandler.QueryTotalRecharge(client);

            totalChongZhiMoney = Global.TransMoneyToYuanBao(totalChongZhiMoney);

            int resoult = RechageManager.GetBtnIndexState(totalChongZhiMoney, 1, !(KTGlobal.CanGetWelfareFirstRecharge(client)));

            return resoult;
        }


     
       


        /// <summary>
        /// Túi đồ đầy không
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        private bool BagFull(KPlayer client)
        {
            if (!KTGlobal.IsHaveSpace(1, client))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Có thông báo bang hội không
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        private bool HasGuildNotify(KPlayer client)
        {
         
            return false;
        }

        /// <summary>
        /// Có điểm tiềm năng chưa cộng không
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        private bool IsHaveRemainProtect(KPlayer client)
        {
            if (client.GetRemainPotential() > 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Có điểm kỹ năng chưa cộng không
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        private bool IsHaveSkillPoint(KPlayer client)
        {
            if (client.GetCurrentSkillPoints() > 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Kiểm tra xem có quà thẻ tháng có thể nhận không
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        private bool CheckFuLiYueKaFanLi(KPlayer client)
        {
            if (client == null)
            {
                return false;
            }

            int dayIdx = client.YKDetail.CurDayOfPerYueKa() - 1;
            if (client.YKDetail.HasYueKa == 1 && dayIdx >= 0 && dayIdx < client.YKDetail.AwardInfo.Length && client.YKDetail.AwardInfo[dayIdx] == '1')
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private long m_LastTicks = 0;

        /// <summary>
        /// Send Icon State theo thời gian về client
        /// </summary>
        /// <param name="client"></param>
        public void DoSpriteIconTicks(KPlayer client)
        {
            //long startTicks = TimeUtil.NOW();
            //if (startTicks - m_LastTicks > 20000)
            //{
            //    m_LastTicks = startTicks;
            //    /// Thực thi đồng bộ Icon State về Client
            //    this.SysnIcon(client);
            //}
        }
    }
}