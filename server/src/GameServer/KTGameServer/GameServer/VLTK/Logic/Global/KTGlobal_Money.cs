using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Logic;
using GameServer.KiemThe.Logic.Manager.Shop;
using GameServer.Logic;
using GameServer.Server;
using GameServer.VLTK.Core.GuildManager;
using System;

namespace GameServer.KiemThe
{
    /// <summary>
    /// Các phương thức và đối tượng toàn cục của Kiếm Thế
    /// </summary>
    public static partial class KTGlobal
    {
        /// <summary>
        /// Số tiền được mang theo tối đa
        /// </summary>
        public const int Max_Role_Money = 1000000000;

        #region Quỹ bang

        /// <summary>
        /// Update tiền cho bang hội
        /// </summary>
        /// <param name="GuildMoney"></param>
        /// <param name="GuildID"></param>
        /// <returns></returns>
        public static bool UpdateGuildMoney(int GuildMoney, int GuildID, KPlayer client)
        {
            if (client.GuildID == GuildID)
            {
                string strcmd = string.Format("{0}:{1}", GuildMoney, GuildID);

                string[] dbFields = Global.ExecuteDBCmd((int)TCPGameServerCmds.CMD_KT_UPDATE_ROLEGUILDMONEY, strcmd, GameManager.ServerId);
                if (null == dbFields)
                    return false;
                if (dbFields.Length != 2)
                {
                    return false;
                }
                if (Convert.ToInt32(dbFields[1]) < 0)
                {
                    return false;
                }

                KTGlobal.SendDefaultChat(client, KTGlobal.CreateStringByColor("Bang cống bang hội gia tăng: " + GuildMoney + "", ColorType.Accpect));
            }
            return true;
        }

        /// <summary>
        /// Thực hiện update tô thuế cho bang hội
        /// </summary>
        /// <param name="Money"></param>
        /// <param name="GuildID"></param>
        /// <returns></returns>
        public static bool UpdateGuildBoundMoney(int Money, int GuildID)
        {
            string VALUE = GuildManager.GetGuildResource(GuildID, GUILD_RESOURCE.GUILD_BOUND_MONEY);
            if (VALUE == "-1")
            {
                return false;
            }
            else
            {
                MiniGuildInfo _Info = GuildManager.getInstance()._GetInfoGuildByGuildID(GuildID);
                if (_Info == null)
                {
                    return false;
                }
                else
                {
                    int MoneyBound = Int32.Parse(VALUE);
                    int AfterAdd = Money + MoneyBound;
                    //Set thông tin vào bang hiện tại
                    _Info.MoneyBound = AfterAdd;

                    return GuildManager.UpdateGuildResource(GuildID, GUILD_RESOURCE.GUILD_BOUND_MONEY, AfterAdd + "");
                }
            }
        }

        #endregion Quỹ bang

        #region Tiền hiện có

        /// <summary>
        /// Trả về số tiền hiện có
        /// </summary>
        /// <param name="player"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static int GetMoney(KPlayer player, MoneyType type)
        {
            switch (type)
            {
                case MoneyType.Bac:
                    {
                        return player.Money;
                    }
                case MoneyType.BacKhoa:
                    {
                        return player.BoundMoney;
                    }
                case MoneyType.Dong:
                    {
                        return player.Token;
                    }
                case MoneyType.DongKhoa:
                    {
                        return player.BoundToken;
                    }
            }

            return 0;
        }

        /// <summary>
        /// Kiểm tra người chơi có đủ số tiền tương ứng không
        /// </summary>
        /// <param name="player"></param>
        /// <param name="MoneyNeed"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static bool IsHaveMoney(KPlayer player, int MoneyNeed, MoneyType Type)
        {
            int MoneyHave = 0;

            if (Type == MoneyType.Bac)
            {
                MoneyHave = player.Money;
            }
            else if (Type == MoneyType.BacKhoa)
            {
                MoneyHave = player.BoundMoney;
            }
            else if (Type == MoneyType.Dong)
            {
                MoneyHave = player.Token;
            }
            else if (Type == MoneyType.DongKhoa)
            {
                MoneyHave = player.BoundToken;
            }
            else if (Type == MoneyType.GuildMoney)
            {
                MoneyHave = player.RoleGuildMoney;
            }

            if (MoneyHave < MoneyNeed)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Trừ tiền của người chơi
        /// </summary>
        /// <param name="player"></param>
        /// <param name="MoneySub"></param>
        /// <param name="Type"></param>
        /// <param name="From"></param>
        /// <returns></returns>
        public static SubRep SubMoney(KPlayer player, int MoneySub, MoneyType Type, string From, bool UpdateConsume = false)
        {
            SubRep _SubRep = new SubRep();
            _SubRep.IsOK = false;
            _SubRep.CountLess = 0;
            if (MoneySub <= 0)
            {
                _SubRep.IsOK = false;
                return _SubRep;
            }

            switch (Type)
            {
                case MoneyType.Bac:
                    {
                        _SubRep.IsOK = KTPlayerManager.SubMoney(player, MoneySub, From);
                        _SubRep.CountLess = player.Money;

                        return _SubRep;
                    }
                case MoneyType.BacKhoa:
                    {
                        _SubRep.IsOK = KTPlayerManager.SubBoundMoney(player, MoneySub, From);

                        _SubRep.CountLess = player.BoundMoney;

                        return _SubRep;
                    }
                case MoneyType.Dong:
                    {
                        _SubRep.IsOK = KTPlayerManager.SubToken(player, MoneySub, From, 1, true, UpdateConsume);

                        _SubRep.CountLess = player.Token;

                        return _SubRep;
                    }
                case MoneyType.DongKhoa:
                    {
                        _SubRep.IsOK = KTPlayerManager.SubBoundToken(player, MoneySub, From);
                        _SubRep.CountLess = player.BoundToken;

                        return _SubRep;
                    }

                case MoneyType.GuildMoney:
                    {
                        _SubRep.IsOK = KTPlayerManager.SubGuildMoney(player, MoneySub, From);
                        _SubRep.CountLess = player.RoleGuildMoney;

                        return _SubRep;
                    }
            }

            return _SubRep;
        }

        /// <summary>
        /// Hàm cộng tiền cho người chơi
        /// </summary>
        /// <param name="player"></param>
        /// <param name="MoneyAdd"></param>
        /// <param name="Type"></param>
        /// <param name="Source"></param>
        /// <returns></returns>
        public static SubRep AddMoney(KPlayer player, int MoneyAdd, MoneyType Type, string Source = "")
        {
            SubRep _SubRep = new SubRep();
            _SubRep.IsOK = false;
            _SubRep.CountLess = 0;

            switch (Type)
            {
                case MoneyType.Bac:
                    {
                        if (player.Money + MoneyAdd > 1000000000)
                        {
                            KTPlayerManager.ShowMessageBox(player, "Lỗi", "Bạc mang theo không thể quá 1 tỉ");
                        }
                        else
                        {
                            _SubRep.IsOK = KTPlayerManager.AddMoney(player, MoneyAdd, Source);
                            _SubRep.CountLess = player.Money;
                        }

                        return _SubRep;
                    }
                case MoneyType.BacKhoa:
                    {
                        if (player.BoundMoney + MoneyAdd > 1000000000)
                        {
                            KTPlayerManager.ShowMessageBox(player, "Lỗi", "Bạc khóa mang theo không thể quá 1 tỉ");
                        }
                        else
                        {
                            _SubRep.IsOK = KTPlayerManager.AddBoundMoney(player, MoneyAdd, Source);

                            _SubRep.CountLess = player.BoundMoney;
                        }

                        return _SubRep;
                    }
                case MoneyType.Dong:
                    {
                        if (player.Token + MoneyAdd > 1000000000)
                        {
                            KTPlayerManager.ShowMessageBox(player, "Lỗi", "Đồng mang theo không thể quá 1 tỉ");
                        }
                        else
                        {
                            _SubRep.IsOK = KTPlayerManager.AddToken(player, MoneyAdd, Source);

                            _SubRep.CountLess = player.Token;
                        }

                        return _SubRep;
                    }
                case MoneyType.DongKhoa:
                    {
                        if (player.BoundToken + MoneyAdd > 1000000000)
                        {
                            KTPlayerManager.ShowMessageBox(player, "Lỗi", "Đồng khóa mang theo không thể quá 1 tỉ");
                        }
                        else
                        {
                            _SubRep.IsOK = KTPlayerManager.AddBoundToken(player, MoneyAdd, Source);
                            _SubRep.CountLess = player.BoundToken;
                        }

                        return _SubRep;
                    }

                case MoneyType.GuildMoney:
                    {
                        if (player.RoleGuildMoney + MoneyAdd > 1000000000)
                        {
                            KTPlayerManager.ShowMessageBox(player, "Lỗi", "Tiền bang hội mang theo không thể quá 1 tỉ");
                        }
                        else
                        {
                            _SubRep.IsOK = KTPlayerManager.AddGuildMoney(player, MoneyAdd, Source);
                            _SubRep.CountLess = player.RoleGuildMoney;
                        }

                        return _SubRep;
                    }
            }

            return _SubRep;
        }

        #endregion Tiền hiện có
    }
}