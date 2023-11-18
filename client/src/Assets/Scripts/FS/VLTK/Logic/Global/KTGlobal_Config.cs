using System;
using System.Collections.Generic;
using System.Linq;

namespace FS.VLTK
{
    /// <summary>
    /// Các hàm toàn cục dùng trong Kiếm Thế
    /// </summary>
    public static partial class KTGlobal
    {
        #region Config
        /// <summary>
        /// Phí tạo bang (bạc)
        /// </summary>
        public const int CreateGuildFee = 1000000;



        /// <summary>
        /// Số vật phẩm tối đa trong thương khố
        /// </summary>
        public const int PortableBagMaxItems = 100;



        /// <summary>
        /// Thời gian đứng lại nghỉ sau khi chạy để dùng kỹ năng
        /// </summary>
        public const float RunningWaitToUseSkill = 0.2f;

        /// <summary>
        /// Thời gian thực thi động tác ngồi
        /// </summary>
        public const float SitActionTime = 1f;

        /// <summary>
        /// Thời gian thực thi động tác đứng
        /// </summary>
        public const float StandActionTime = 1f;

        /// <summary>
        /// Thời gian thực thi động tác chết
        /// </summary>
        public const float DieActionTime = 0.5f;



        /// <summary>
        /// Thời gian tự thoát game khi bị ngắt kết nối
        /// </summary>
        public const float AutoQuitGameWhenDisconnectedAfter = 60f;

        /// <summary>
        /// Đường dẫn Bundle chứa Icon ở Minimap
        /// </summary>
        public const string MinimapIconBundleDir = "UI/Game/MainUI/Main.unity3d";
        /// <summary>
        /// Tên Atlas chứa Icon ở Minimap
        /// </summary>
        public const string MinimapIconAtlasName = "Main";
        /// <summary>
        /// Tên Sprite Icon ở Minimap của Leader
        /// </summary>
        public const string MinimapLeaderIconSpriteName = "Leader Icon";
        /// <summary>
        /// Tên Sprite Icon ở Minimap của người chơi khác
        /// </summary>
        public const string MinimapOtherRoleIconSpriteName = "Other Icon";
        /// <summary>
        /// Tên Sprite Icon ở Minimap của đồng đội
        /// </summary>
        public const string MinimapTeammateRoleIconSpriteName = "Teammate Icon";
        /// <summary>
        /// Tên Sprite Icon ở Minimap của kẻ địch
        /// </summary>
        public const string MinimapEnemyRoleIconSpriteName = "Enemy Icon";
        /// <summary>
        /// Tên Sprite Icon ở Minimap của quái
        /// </summary>
        public const string MinimapMonsterIconSpriteName = "Monster Icon";
        /// <summary>
        /// Tên Sprite Icon ở Minimap của NPC
        /// </summary>
        public const string MinimapNPCIconSpriteName = "NPC Minimap";
        /// <summary>
        /// Tên Sprite Icon ở Minimap của cổng dịch chuyển
        /// </summary>
        public const string MinimapTeleportIconSpriteName = "TeleportSymbol";
        /// <summary>
        /// Tên Sprite Icon ở Minimap của điểm thu thập
        /// </summary>
        public const string MinimapGrowPointIconSpriteName = "GrowPoint Icon";
        /// <summary>
        /// Tên Sprite Icon ở Minimap của xe tiêu
        /// </summary>
        public const string MinimapTraderCarriageIconSpriteName = "GrowPoint Icon";
        #endregion
    }
}
