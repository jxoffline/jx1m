using FS.GameEngine.Logic;
using FS.GameEngine.Network;
using FS.GameEngine.Sprite;
using FS.VLTK;
using FS.VLTK.Network;
using FS.VLTK.UI;
using FS.VLTK.UI.Main;
using FS.VLTK.UI.Main.GuildEx;
using FS.VLTK.UI.Main.MainUI;
using Server.Data;
using System.Collections.Generic;
using UnityEngine;
using static FS.VLTK.Entities.Enum;

/// <summary>
/// Quản lý các khung giao diện trong màn chơi
/// </summary>
public partial class PlayZone
{
    #region Khung danh sách người chơi mới hoặc xin gia nhập bang
    /// <summary>
    /// Khung danh sách người chơi mời vào nhóm
    /// </summary>
    public UIGuildRequestList UIGuildRequestList { get; protected set; }

    /// <summary>
    /// Tạo khung danh sách người chơi mời vào nhóm
    /// </summary>
    protected void InitGuildRequestList()
    {
        if (this.UIGuildRequestList != null)
        {
            return;
        }

        CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
        this.UIGuildRequestList = canvas.LoadUIPrefab<UIGuildRequestList>("MainGame/MainUI/UIGuildRequestList");
        canvas.AddMainUI(this.UIGuildRequestList);

        /// Sự kiện
        this.UIGuildRequestList.Agree = (roleID, guildID) => {
            /// Nếu bản thân là bang chủ hoặc phó bang chủ
            if (Global.Data.RoleData.GuildID > 0 && (Global.Data.RoleData.GuildRank == (int) GuildRank.Master || Global.Data.RoleData.GuildRank == (int) GuildRank.ViceMaster))
            {
                KT_TCPHandler.SendResponseAskToJoinGuildRequest(1, roleID);
            }
            /// Nếu bản thân không có bang
            else if (Global.Data.RoleData.GuildID <= 0)
            {
                KT_TCPHandler.SendResponseInviteToGuild(roleID, guildID, 1);
            }
        };
        this.UIGuildRequestList.Reject = (roleID, guildID) =>
        {
            /// Nếu bản thân là bang chủ hoặc phó bang chủ
            if (Global.Data.RoleData.GuildID > 0 && (Global.Data.RoleData.GuildRank == (int) GuildRank.Master || Global.Data.RoleData.GuildRank == (int) GuildRank.ViceMaster))
            {
                KT_TCPHandler.SendResponseAskToJoinGuildRequest(0, roleID);
            }
            /// Nếu bản thân không có bang
            else if (Global.Data.RoleData.GuildID <= 0)
            {
                KT_TCPHandler.SendResponseInviteToGuild(roleID, guildID, 0);
            }
        };
    }
    #endregion

    #region Bang hội
    #region Tạo bang
    /// <summary>
    /// Khung tạo bang
    /// </summary>
    public UIGuildEx_CreateNew UICreateGuild { get; protected set; }

    /// <summary>
    /// Mở khung tạo bang
    /// </summary>
    public void OpenUICreateGuild()
    {
        if (this.UICreateGuild != null)
        {
            return;
        }

        CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
        this.UICreateGuild = canvas.LoadUIPrefab<UIGuildEx_CreateNew>("MainGame/Guild/UIGuildEx_CreateNew");
        canvas.AddUI(this.UICreateGuild);

        this.UICreateGuild.Close = this.CloseUICreateGuild;
        this.UICreateGuild.CreateGuild = (guildName) =>
        {
            /// Gửi yêu cầu tạo bang
            KT_TCPHandler.SendCreateGuild(guildName);
            /// Đóng khung
            this.CloseUICreateGuild();
            /// Nếu đang mở khung danh sách bang hội
            if (this.UIGuildList != null)
            {
                /// Đóng lại
                this.CloseUIGuildList();
            }
        };
    }

    /// <summary>
    /// Đóng khung tạo bang
    /// </summary>
    public void CloseUICreateGuild()
    {
        if (this.UICreateGuild != null)
        {
            GameObject.Destroy(this.UICreateGuild.gameObject);
            this.UICreateGuild = null;
        }
    }
    #endregion

    #region Bang hội tổng quan
    /// <summary>
    /// Khung thông tin bang hội tổng quan
    /// </summary>
    public UIGuildEx_Main UIGuild { get; protected set; }

    /// <summary>
    /// Mở khung thông tin bang hội tổng quan
    /// </summary>
    /// <param name="guildInfo"></param>
    public void OpenUIGuild(GuildInfo guildInfo)
    {
        if (this.UIGuild != null)
        {
            return;
        }

        CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
        this.UIGuild = canvas.LoadUIPrefab<UIGuildEx_Main>("MainGame/Guild/UIGuildEx_Main");
        canvas.AddUI(this.UIGuild);

        this.UIGuild.Data = guildInfo;
        this.UIGuild.Close = this.CloseUIGuild;
        this.UIGuild.OpenGuildMemberList = () =>
        {
            /// Gửi yêu cầu truy vấn thông tin thành viên bang
            KT_TCPHandler.SendGetGuildMembers(1);
        };
        this.UIGuild.OpenGuildDedication = () =>
        {
            /// Mở khung cống hiến bang hội
            this.OpenUIGuildDedication(guildInfo);
        };
        this.UIGuild.OpenGuildBattle = () =>
        {
            /// Mở khung công thành chiến
            KT_TCPHandler.SendOpenGuildWar();
        };
        this.UIGuild.QuitGuild = () =>
        {
            /// Gửi yêu cầu thoát khỏi bang
            KT_TCPHandler.SendQuitGuild();
        };
        this.UIGuild.ChangeNotification = (slogan) =>
        {
            /// Gửi yêu cầu thay đổi công cáo bang hội
            KT_TCPHandler.SendChangeGuildNotification(slogan);
        };
        this.UIGuild.LevelUp = () =>
        {
            /// Gửi yêu cầu thăng cấp bang hội
            KT_TCPHandler.SendGuildLevelUp();
        };
        this.UIGuild.OpenGuildSkills = () =>
        {
            /// Gửi yêu cầu mở khung kỹ năng bang hội
            KT_TCPHandler.SendGetGuildSkills();
        };
        this.UIGuild.OpenGuildAllies = () =>
        {
            /// Gửi yêu cầu mở khung liên minh bang hội
            KTGlobal.AddNotification("Chức năng đang được phát triển!");
        };
        this.UIGuild.OpenGuildQuest = () =>
        {
            /// Gửi yêu cầu mở khung nhiệm vụ bang hội
            KT_TCPHandler.SendGetGuildQuest();
        };
        this.UIGuild.OpenGuildShop = () =>
        {
            /// Gửi yêu cầu mở khung cửa hàng bang hội
            KT_TCPHandler.SendOpenGuildShop();
        };
    }

    /// <summary>
    /// Đóng khung thông tin bang hội tổng quan
    /// </summary>
    public void CloseUIGuild()
    {
        if (this.UIGuild != null)
        {
            GameObject.Destroy(this.UIGuild.gameObject);
            this.UIGuild = null;
        }
    }
    #endregion

    #region Danh sách thành viên bang hội
    /// <summary>
    /// Khung danh sách thành viên bang hội
    /// </summary>
    public UIGuildEx_MemberList UIGuildMemberList { get; protected set; }

    /// <summary>
    /// Mở khung danh sách thành viên bang hội
    /// </summary>
    /// <param name="data"></param>
    public void OpenUIGuildMemberList(GuildMemberData data)
    {
        if (this.UIGuildMemberList != null)
        {
            return;
        }

        CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
        this.UIGuildMemberList = canvas.LoadUIPrefab<UIGuildEx_MemberList>("MainGame/Guild/UIGuildEx_MemberList");
        canvas.AddUI(this.UIGuildMemberList);

        this.UIGuildMemberList.Data = data;
        this.UIGuildMemberList.Close = this.CloseUIGuildMemberList;
        this.UIGuildMemberList.KickOut = (playerID) =>
        {
            /// Gửi yêu cầu trục người chơi khỏi bang
            KT_TCPHandler.SendGuildKickoutMember(playerID);
        };
        this.UIGuildMemberList.Approve = (roleID, rank) =>
        {
            /// Gửi yêu cầu bổ nhiệm thành viên
            KT_TCPHandler.SendChangeGuildMemberRank(roleID, rank);
        };
        this.UIGuildMemberList.QueryMemberList = (pageID) =>
        {
            /// Gửi yêu cầu truy vấn thông tin thành viên ở trang tương ứng
            KT_TCPHandler.SendGetGuildMembers(pageID);
        };
        this.UIGuildMemberList.OpenAskToJoinList = () =>
        {
            /// Gửi yêu cầu truy vấn danh sách người chơi xin vào bang
            KT_TCPHandler.SendGetAskToJoinList(1);
        };
    }

    /// <summary>
    /// Đóng khung danh sách thành viên bang hội
    /// </summary>
    public void CloseUIGuildMemberList()
    {
        if (this.UIGuildMemberList != null)
        {
            GameObject.Destroy(this.UIGuildMemberList.gameObject);
            this.UIGuildMemberList = null;
        }
    }
    #endregion

    #region Danh sách người chơi xin vào bang hội
    /// <summary>
    /// Khung danh sách người chơi xin vào bang hội
    /// </summary>
    public UIGuildEx_AskToJoinList UIGuildAskToJoinList { get; protected set; }

    /// <summary>
    /// Mở khung danh sách người chơi xin vào bang hội
    /// </summary>
    /// <param name="data"></param>
    public void OpenUIGuildAskToJoinList(RequestJoinInfo data)
    {
        if (this.UIGuildAskToJoinList != null)
        {
            return;
        }

        CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
        this.UIGuildAskToJoinList = canvas.LoadUIPrefab<UIGuildEx_AskToJoinList>("MainGame/Guild/UIGuildEx_AskToJoinList");
        canvas.AddUI(this.UIGuildAskToJoinList);

        this.UIGuildAskToJoinList.Data = data;
        this.UIGuildAskToJoinList.Close = this.CloseUIGuildAskToJoinList;
        this.UIGuildAskToJoinList.Accept = (playerID) =>
        {
            /// Gửi yêu cầu đồng ý cho người chơi vào bang
            KT_TCPHandler.SendResponseAskToJoinGuildRequest(1, playerID);
        };
        this.UIGuildAskToJoinList.Reject = (playerID) =>
        {
            /// Gửi yêu cầu từ chối cho người chơi vào bang
            KT_TCPHandler.SendResponseAskToJoinGuildRequest(0, playerID);
        };
        this.UIGuildAskToJoinList.SaveAutoAcceptRule = (parameterString) =>
        {
            /// Gửi yêu cầu lưu thiết lập tự duyệt người chơi vào bang
            KT_TCPHandler.SendSaveAutoAcceptJoinGuildSetting(parameterString);
        };
        this.UIGuildAskToJoinList.QueryAskToJoinPlayers = (pageID) =>
        {
            /// Gửi yêu cầu truy vấn danh sách người chơi xin vào bang ở trang tương ứng
            KT_TCPHandler.SendGetAskToJoinList(pageID);
        };
    }

    /// <summary>
    /// Đóng khung danh sách người chơi xin vào bang hội
    /// </summary>
    public void CloseUIGuildAskToJoinList()
    {
        if (this.UIGuildAskToJoinList != null)
        {
            GameObject.Destroy(this.UIGuildAskToJoinList.gameObject);
            this.UIGuildAskToJoinList = null;
        }
    }
    #endregion

    #region Cống hiến bang hội
    /// <summary>
    /// Khung cống hiến bang hội
    /// </summary>
    public UIGuildEx_Dedicate UIGuildDedication { get; protected set; }

    /// <summary>
    /// Mở khung cống hiến bang hội
    /// </summary>
    /// <param name="guildInfo"></param>
    public void OpenUIGuildDedication(GuildInfo guildInfo)
    {
        if (this.UIGuildDedication != null)
        {
            return;
        }

        CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
        this.UIGuildDedication = canvas.LoadUIPrefab<UIGuildEx_Dedicate>("MainGame/Guild/UIGuildEx_Dedicate");
        canvas.AddUI(this.UIGuildDedication);

        this.UIGuildDedication.Close = this.CloseUIGuildDedication;
        this.UIGuildDedication.Data = guildInfo;
        this.UIGuildDedication.DedicateMoney = (amount) =>
        {
            /// Gửi yêu cầu cống hiến vào bang
            KT_TCPHandler.SendDedicateMoneyToGuild(amount);
        };
        this.UIGuildDedication.DedicateItems = (items) =>
        {
            /// Gửi yêu cầu cống hiến vào bang
            KT_TCPHandler.SendDedicateItemsToGuild(items);
        };
    }

    /// <summary>
    /// Đóng khung cống hiến bang hội
    /// </summary>
    public void CloseUIGuildDedication()
    {
        if (this.UIGuildDedication != null)
        {
            GameObject.Destroy(this.UIGuildDedication.gameObject);
            this.UIGuildDedication = null;
        }
    }
    #endregion

    #region Nhiệm vụ bang hội
    /// <summary>
    /// Khung kỹ năng bang hội
    /// </summary>
    public UIGuildEx_Quest UIGuildQuest { get; protected set; }

    /// <summary>
    /// Mở khung kỹ năng bang hội
    /// </summary>
    /// <param name="skills"></param>
    public void OpenUIGuildQuest(GuildTask tasks)
    {
        if (this.UIGuildQuest != null)
        {
            return;
        }

        CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
        this.UIGuildQuest = canvas.LoadUIPrefab<UIGuildEx_Quest>("MainGame/Guild/UIGuildEx_Quest");
        canvas.AddUI(this.UIGuildQuest);

        this.UIGuildQuest.Close = this.CloseUIGuildQuest;
        this.UIGuildQuest.Data = tasks;
        this.UIGuildQuest.ChangeTask = () =>
        {
            /// Gửi yêu cầu đổi nhiệm vụ bang
            KT_TCPHandler.SendChangeGuildTask();
        };
        this.UIGuildQuest.AbandonTask = () =>
        {
            /// Gửi yêu cầu bỏ nhiệm vụ bang
            KT_TCPHandler.SendAbandonGuildTask();
        };
    }

    /// <summary>
    /// Đóng khung kỹ năng bang hội
    /// </summary>
    public void CloseUIGuildQuest()
    {
        if (this.UIGuildQuest != null)
        {
            GameObject.Destroy(this.UIGuildQuest.gameObject);
            this.UIGuildQuest = null;
        }
    }
    #endregion

    #region Kỹ năng bang hội
    /// <summary>
    /// Khung kỹ năng bang hội
    /// </summary>
    public UIGuildEx_Skill UIGuildSkill { get; protected set; }

    /// <summary>
    /// Mở khung kỹ năng bang hội
    /// </summary>
    /// <param name="skills"></param>
    public void OpenUIGuildSkills(List<SkillDef> skills)
    {
        if (this.UIGuildSkill != null)
        {
            return;
        }

        CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
        this.UIGuildSkill = canvas.LoadUIPrefab<UIGuildEx_Skill>("MainGame/Guild/UIGuildEx_Skill");
        canvas.AddUI(this.UIGuildSkill);

        this.UIGuildSkill.Close = this.CloseUIGuildSkills;
        this.UIGuildSkill.Data = skills;
        this.UIGuildSkill.LevelUp = (skillID) =>
        {
            /// Gửi yêu cầu thăng cấp kỹ năng bang
            KT_TCPHandler.SendGuildSkillLevelUp(skillID);
        };
    }

    /// <summary>
    /// Đóng khung kỹ năng bang hội
    /// </summary>
    public void CloseUIGuildSkills()
    {
        if (this.UIGuildSkill != null)
        {
            GameObject.Destroy(this.UIGuildSkill.gameObject);
            this.UIGuildSkill = null;
        }
    }
    #endregion

    #region Danh sách bang hội
    /// <summary>
    /// Khung kỹ năng bang hội
    /// </summary>
    public UIGuildEx_GuildList UIGuildList { get; protected set; }

    /// <summary>
    /// Mở khung kỹ năng bang hội
    /// </summary>
    /// <param name="guilds"></param>
    public void OpenUIGuildList(List<MiniGuildInfo> guilds)
    {
        if (this.UIGuildList != null)
        {
            return;
        }

        CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
        this.UIGuildList = canvas.LoadUIPrefab<UIGuildEx_GuildList>("MainGame/Guild/UIGuildEx_GuildList");
        canvas.AddUI(this.UIGuildList);

        this.UIGuildList.Close = this.CloseUIGuildList;
        this.UIGuildList.Data = guilds;
        this.UIGuildList.AskToJoin = (guildID) =>
        {
            /// Gửi yêu cầu xin vào bang hội tương ứng
            KT_TCPHandler.SendRequestJoinGuild(guildID);
        };
        this.UIGuildList.OpenCreateGuild = () =>
        {
            /// Mở khung nhập thông tin tạo bang hội
            this.OpenUICreateGuild();
        };
    }

    /// <summary>
    /// Đóng khung kỹ năng bang hội
    /// </summary>
    public void CloseUIGuildList()
    {
        if (this.UIGuildList != null)
        {
            GameObject.Destroy(this.UIGuildList.gameObject);
            this.UIGuildList = null;
        }
    }
    #endregion

    #region Công thành chiến
    /// <summary>
    /// Khung kỹ năng bang hội
    /// </summary>
    public UIGuildEx_Battle UIGuildBattle { get; protected set; }

    /// <summary>
    /// Mở khung công thành chiến
    /// </summary>
    /// <param name="data"></param>
    public void OpenUIGuildBattle(GuildWarInfo data)
    {
        if (this.UIGuildBattle != null)
        {
            return;
        }

        CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
        this.UIGuildBattle = canvas.LoadUIPrefab<UIGuildEx_Battle>("MainGame/Guild/UIGuildEx_Battle");
        canvas.AddUI(this.UIGuildBattle);

        this.UIGuildBattle.Close = this.CloseUIGuildBattle;
        this.UIGuildBattle.Data = data;
        this.UIGuildBattle.Register = (memberIDs) =>
        {
            /// Thực thi sự kiện đăng ký công thành chiến
            KT_TCPHandler.SendRegisterGuildWar(memberIDs.ToArray());
        };
    }

    /// <summary>
    /// Đóng khung công thành chiến
    /// </summary>
    public void CloseUIGuildBattle()
    {
        if (this.UIGuildBattle != null)
        {
            GameObject.Destroy(this.UIGuildBattle.gameObject);
            this.UIGuildBattle = null;
        }
    }
    #endregion
    #endregion
}
