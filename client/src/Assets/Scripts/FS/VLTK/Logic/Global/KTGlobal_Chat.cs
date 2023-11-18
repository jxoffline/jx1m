using FS.GameEngine.Logic;
using FS.GameEngine.Sprite;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK
{
    /// <summary>
    /// Các hàm toàn cục dùng trong Kiếm Thế
    /// </summary>
    public static partial class KTGlobal
    {
        #region Chat Message
        /// <summary>
        /// Tên người chơi Chat mật lần trước
        /// </summary>
        public static string LastPrivateToRoleName { get; set; } = "";

        /// <summary>
        /// Thêm nội dung tin nhắn vào khung ChatMini
        /// </summary>
        /// <param name="message"></param>
        public static void AddNotificationChatBoxMini(string message)
        {
            if (PlayZone.Instance.UIChatBoxMini != null)
            {
                PlayZone.Instance.UIChatBoxMini.AddMessage(new SpriteChat()
                {
                    Channel = (int) ChatChannel.Default,
                    Content = string.Format("<color=red>{0}</color>", message),
                });
            }
        }

        /// <summary>
        /// Hiển thị tin nhắn Chat tương ứng
        /// </summary>
        /// <param name="spriteChat"></param>
        /// <returns></returns>
        public static string DisplayChat(SpriteChat spriteChat)
        {
            string text = "";

            switch (spriteChat.Channel)
            {
                case (int) ChatChannel.Near:
                {
                    text = string.Format("<color=#bdfffe>[Lân cận]</color> <link=\"SelectRole\"><color=#5dabfd>{0}</color></link>: {1}", spriteChat.FromRoleName, spriteChat.Content);
                    break;
                }
                case (int) ChatChannel.Team:
                {
                    text = string.Format("<color=#a072fe>[Đội ngũ]</color> <link=\"SelectRole\"><color=#5dabfd>{0}</color></link>: {1}", spriteChat.FromRoleName, spriteChat.Content);
                    break;
                }
                case (int) ChatChannel.Faction:
                {
                    text = string.Format("<color=#fef472>[Môn phái]</color> <link=\"SelectRole\"><color=#5dabfd>{0}</color></link>: {1}", spriteChat.FromRoleName, spriteChat.Content);
                    break;
                }
                case (int) ChatChannel.Guild:
                {
                    text = string.Format("<color=#feb07c>[Bang hội]</color> <link=\"SelectRole\"><color=#5dabfd>{0}</color></link>: {1}", spriteChat.FromRoleName, spriteChat.Content);
                    break;
                }
                case (int) ChatChannel.Allies:
                {
                    text = string.Format("<color=#b67cfe>[Liên minh]</color> <link=\"SelectRole\"><color=#5dabfd>{0}</color></link>: {1}", spriteChat.FromRoleName, spriteChat.Content);
                    break;
                }
                case (int) ChatChannel.Global:
                {
                    text = string.Format("<color=#5dfd6b>[Thế giới]</color> <link=\"SelectRole\"><color=#5dabfd>{0}</color></link>: {1}", spriteChat.FromRoleName, spriteChat.Content);
                    break;
                }
                case (int) ChatChannel.KuaFuLine:
                {
                    text = string.Format("<color=#9cff38>[Liên máy chủ]</color> <link=\"SelectRole\"><color=#5dabfd>{0}</color></link>: {1}", spriteChat.FromRoleName, spriteChat.Content);
                    break;
                }
                case (int) ChatChannel.Special:
                {
                    text = string.Format("<color=#f561e1>[Đặc biệt]</color> <link=\"SelectRole\"><color=#5dabfd>{0}</color></link>: {1}", spriteChat.FromRoleName, spriteChat.Content);
                    break;
                }
                case (int) ChatChannel.Private:
                {
                    if (spriteChat.FromRoleName == Global.Data.RoleData.RoleName)
                    {
                        text = string.Format("<color=#fe9a9a>[Mật]</color> Bạn nói với <link=\"SelectRole\"><color=#5dabfd>{0}</color></link>: {1}", spriteChat.ToRoleName, spriteChat.Content);
                    }
                    else if (spriteChat.ToRoleName == Global.Data.RoleData.RoleName)
                    {
                        text = string.Format("<color=#fe9a9a>[Mật]</color> <link=\"SelectRole\"><color=#5dabfd>{0}</color></link> nói với bạn: {1}", spriteChat.FromRoleName, spriteChat.Content);
                    }

                    break;
                }
                case (int) ChatChannel.System:
                case (int) ChatChannel.System_Broad_Chat:
                {
                    text = string.Format("<color=#fd58fd>[Hệ thống]</color>: {0}", spriteChat.Content);
                    break;
                }
                default:
                {
                    text = spriteChat.Content;
                    break;
                }
            }

            return text;
        }

        /// <summary>
        /// Hiển thị tin nhắn Chat của người chơi tương ứng
        /// </summary>
        /// <param name="spriteChat"></param>
        /// <returns></returns>
        public static string DisplayPlayerHeaderChat(SpriteChat spriteChat)
        {
            string text = "";

            switch (spriteChat.Channel)
            {
                case (int) ChatChannel.Near:
                {
                    text = string.Format("{0}", spriteChat.Content);
                    break;
                }
                case (int) ChatChannel.Team:
                {
                    text = string.Format("<color=#a072fe>[Đội ngũ]</color>: {0}", spriteChat.Content);
                    break;
                }
                default:
                {
                    text = spriteChat.Content;
                    break;
                }
            }

            return text;
        }

        /// <summary>
        /// Mở khung chat mật với đối tượng tương ứng
        /// </summary>
        /// <param name="playerName"></param>
        public static void OpenPrivateChatBoxWith(string playerName)
        {
            /// Thiết lập đang Chat mật với người chơi
            KTGlobal.LastPrivateToRoleName = playerName;

            /// Mở khung Chat
            PlayZone.Instance.ShowUIChat();
        }
        #endregion Chat Message
    }
}
