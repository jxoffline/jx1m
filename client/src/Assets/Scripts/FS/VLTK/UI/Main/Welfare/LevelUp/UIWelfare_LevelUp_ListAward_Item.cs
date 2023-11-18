using FS.VLTK.UI.Main.ItemBox;
using FS.VLTK.Utilities.UnityUI;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FS.VLTK.UI.Main.Welfare.LevelUp
{
    /// <summary>
    /// Ô vật phẩm trong khung phúc lợi thăng cấp
    /// </summary>
    public class UIWelfare_LevelUp_ListAward_Item : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Ô vật phẩm
        /// </summary>
        [SerializeField]
        private UIItemBox UIItemBox;

        /// <summary>
        /// Hiệu ứng đã nhận
        /// </summary>
        [SerializeField]
        private UIAnimatedSprite UIAnimation_AlreadyGotten;

        /// <summary>
        /// Hiệu ứng có thể nhận
        /// </summary>
        [SerializeField]
        private UIAnimatedSprite UIAnimation_CanGet;

        /// <summary>
        /// Hiệu ứng có sẽ nhận về sau
        /// </summary>
        [SerializeField]
        private UIAnimatedSprite UIAnimation_WillGet;
        #endregion

        #region Properties
        /// <summary>
        /// Dữ liệu vật phẩm
        /// </summary>
        public GoodsData Data
        {
            get
            {
                return this.UIItemBox.Data;
            }
            set
            {
                this.UIItemBox.Data = value;
            }
        }

        private int _State;
        /// <summary>
        /// Trạng thái nhận
        /// <para>0: Chưa đủ điều kiện, 1: Có thể nhận, 2: Đã nhận</para>
        /// </summary>
        public int State
        {
            get
            {
                return this._State;
            }
            set
            {
                this._State = value;

                switch (value)
                {
                    case 0:
                    {
                        this.UIAnimation_CanGet.Visible = false;
                        this.UIAnimation_WillGet.Visible = true;
                        this.UIAnimation_AlreadyGotten.Visible = false;
                        break;
                    }
                    case 1:
                    {
                        this.UIAnimation_CanGet.Visible = true;
                        this.UIAnimation_WillGet.Visible = false;
                        this.UIAnimation_AlreadyGotten.Visible = false;
                        break;
                    }
                    default:
                    {
                        this.UIAnimation_CanGet.Visible = false;
                        this.UIAnimation_WillGet.Visible = false;
                        this.UIAnimation_AlreadyGotten.Visible = true;
                        break;
                    }
                }
            }
        }
        #endregion
    }
}
