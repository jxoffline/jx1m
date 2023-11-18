using GameServer.Logic;
using GameServer.Server;
using System;
using System.Collections.Generic;

namespace GameServer.VLTK.Core.GuildManager
{
    public partial class GuildManager
    {
        /// <summary>
        /// Mã hóa skill vứt vào DB
        /// </summary>
        /// <param name="InputSkill"></param>
        /// <returns></returns>
        public static string SkillEncode(List<SkillDef> InputSkill)
        {
            string BUILD = "";

            if (InputSkill.Count > 0)
            {
                int i = 0;
                foreach (var _Def in InputSkill)
                {
                    BUILD += _Def.SkillID + "_" + _Def.Level;
                    // Cộng thêm dấu phẩy ở mỗi cái
                    if (i < InputSkill.Count - 1)
                    {
                        BUILD += "|";
                    }

                    i++;
                }
            }

            return BUILD;
        }

        /// <summary>
        /// Mã hóa đống vật phẩm thành string arrray
        /// </summary>
        /// <param name="InputItem"></param>
        /// <returns></returns>
        public static string ItemEncode(List<ItemRequest> InputItem)
        {
            string BUILD = "";

            if (InputItem.Count > 0)
            {
                int i = 0;
                foreach (var _Def in InputItem)
                {
                    BUILD += _Def.ItemID + "_" + _Def.ItemNum;
                    // Cộng thêm dấu phẩy ở mỗi cái
                    if (i < InputItem.Count - 1)
                    {
                        BUILD += "|";
                    }

                    i++;
                }
            }

            return BUILD;
        }

        /// <summary>
        /// Giải mã skill gửi từ DB Ra
        /// </summary>
        /// <param name="SkillNote"></param>
        /// <returns></returns>
        public static List<SkillDef> SkillDecode(string SkillNote)
        {
            List<SkillDef> skillDefList = new List<SkillDef>();

            if (SkillNote.Length > 0)
            {
                foreach (string _Skill in SkillNote.Split('|'))
                {
                    int SkillID = Int32.Parse(_Skill.Split('_')[0]);
                    int Level = Int32.Parse(_Skill.Split('_')[1]);

                    SkillDef _Def = new SkillDef();
                    _Def.SkillID = SkillID;
                    _Def.Level = Level;
                    skillDefList.Add(_Def);
                }
            }
            return skillDefList;
        }

        /// <summary>
        /// Giải mã đống vật phẩm ở trong DB
        /// </summary>
        /// <param name="ItemEncode"></param>
        /// <returns></returns>
        public static List<ItemRequest> ItemDecode(string ItemEncode)
        {
            List<ItemRequest> itemList = new List<ItemRequest>();

            if (ItemEncode.Length > 0)
            {
                foreach (string _Item in ItemEncode.Split('|'))
                {
                    int Item_ID = Int32.Parse(_Item.Split('_')[0]);
                    int Item_Num = Int32.Parse(_Item.Split('_')[1]);

                    ItemRequest _Def = new ItemRequest();
                    _Def.ItemID = Item_ID;
                    _Def.ItemNum = Item_Num;

                    itemList.Add(_Def);
                }
            }
            return itemList;
        }

        /// <summary>
        /// Read Resource From DB
        /// </summary>
        /// <param name="GuildID"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static string GetGuildResource(int GuildID, GUILD_RESOURCE Type)
        {
            string[] Pram = Global.SendToDB((int)TCPGameServerCmds.CMD_KT_GUILD_GET_RESOURCE_INFO, GuildID + ":" + (int)Type, GameManager.LocalServerId);

            if (Pram == null)
            {
                //Hệ thống hiện tại đang bận
                return "-1";
            }

            if (Pram.Length != 2)
            {
                return "-1";
            }

            return Pram[0];
        }

     
        /// <summary>
        /// Cập nhật tài nguyên bang hội
        /// </summary>
        /// <param name="GuildID"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static bool UpdateGuildResource(int GuildID, GUILD_RESOURCE Type, string Value)
        {
            string[] Pram = Global.SendToDB((int)TCPGameServerCmds.CMD_KT_GUILD_SET_RESOURCE_INFO, GuildID + ":" + (int)Type + ":" + Value, GameManager.LocalServerId);

            if (Pram == null)
            {
                return false;
            }

            if (Pram.Length != 2)
            {
                return false;
            }

            return true;
        }
    }
}