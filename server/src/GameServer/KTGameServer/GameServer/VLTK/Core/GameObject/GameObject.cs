using GameServer.Interface;
using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Logic;
using System.Threading;
using System.Windows;

namespace GameServer.Logic
{
    /// <summary>
    /// Các đối tượng trong Game
    /// </summary>
    public abstract partial class GameObject : IObject
    {
        /// <summary>
        /// ID
        /// </summary>
        public virtual int RoleID { get; set; }

        /// <summary>
        /// Cấp độ
        /// </summary>
        public virtual int m_Level { get; set; }

        /// <summary>
        /// Ngũ hành
        /// </summary>
        public virtual KE_SERIES_TYPE m_Series { get; set; }

        /// <summary>
        /// Tên thật của đối tượng
        /// </summary>
        public virtual string RoleName { get; set; }

        private string _Title = "";
        /// <summary>
        /// Danh hiệu của đối tượng
        /// </summary>
        public virtual string Title
        {
            get
            {
                return this._Title;
            }
            set
            {
                this._Title = value;

                /// Thông báo về Client
                KT_TCPHandler.NotifyOthersMyTitleChanged(this);
            }
        }

        private string _TempTitle = "";
        /// <summary>
        /// Danh hiệu tạm thời của đối tượng, sẽ đè vào danh hiệu chính, mất khi thoát Game hoặc mất kết nối
        /// <para>Làm rỗng danh hiệu tạm thời sẽ khôi phục lại danh hiệu chính của đối tượng</para>
        /// </summary>
        public virtual string TempTitle
        {
            get
            {
                return this._TempTitle;
            }
            set
            {
                this._TempTitle = value;

                /// Thông báo về Client
                KT_TCPHandler.NotifyOthersMyTitleChanged(this);
            }
        }

        private int _Camp;
        /// <summary>
        /// CampID đối tượng, dùng để phân biệt đâu là bạn, đâu là địch
        /// </summary>
        public int Camp
        {
            get
            {
                return this._Camp;
            }
            set
            {
				///// Nếu giá trị cũ trùng giá trị hiện tại thì bỏ qua
				//if (this._Camp == value)
				//{
				//	return;
				//}

				this._Camp = value;

                /// Gửi thông báo tới tất cả các đối tượng xung quanh trạng thái PK hoặc Camp của bản thân thay đổi
                KT_TCPHandler.SendToOthersMyPKModeAndCampChanged(this);
            }
        }

        private KE_NPC_DOING _m_eDoing;
        /// <summary>
        /// Trạng thái hiện tại của đối tượng
        /// </summary>
        public KE_NPC_DOING m_eDoing
        {
            get
            {
                return this._m_eDoing;
            }
            set
            {
                this._m_eDoing = value;
            }
        }


        /// <summary>
        /// Cây Buff của đối tượng
        /// </summary>
        public BuffTree Buffs { get; protected set; }

        /// <summary>
        /// Đối tượng gây trạng thái bị trúng độc cho bản thân
        /// </summary>
        public GameObject m_nLastPoisonDamageIdx { get; set; }

        /// <summary>
        /// Vị trí đích đến
        /// </summary>
        public virtual Point ToPos { get; set; }



        /// <summary>
        /// Kinh nghiệm của đối tượng
        /// </summary>
        public long m_Experience { get; set; }

        #region IObject
        /// <summary>
        /// Loại đối tượng
        /// </summary>
        public virtual ObjectTypes ObjectType { get; }

        /// <summary>
        /// Tọa độ lưới
        /// </summary>
        public virtual Point CurrentGrid { get; set; }

        /// <summary>
        /// Tọa độ thực
        /// </summary>
        public virtual Point CurrentPos { get; set; }

        /// <summary>
        /// ID bản đồ hiện tại
        /// </summary>
        public virtual int CurrentMapCode { get; }

        /// <summary>
        /// ID phụ bản hiện tại
        /// </summary>
        public virtual int CurrentCopyMapID { get; set; }

        /// <summary>
        /// Hướng quay hiện tại
        /// </summary>
        public virtual KiemThe.Entities.Direction CurrentDir { get; set; }
        #endregion

        /// <summary>
        /// Chuyển đối tượng về dạng String
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("ID = {0}, Name = {1}", this.RoleID, this.RoleName);
        }
    }
}