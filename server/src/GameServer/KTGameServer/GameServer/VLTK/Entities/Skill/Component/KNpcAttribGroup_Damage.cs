using GameServer.Logic;

namespace GameServer.KiemThe.Entities
{
    /// <summary>
    /// Quản lý nhóm thuộc tính sát thương
    /// </summary>
    public class KNpcAttribGroup_Damage
    {
        #region Define
        /// <summary>
        /// Ngoại
        /// </summary>
        public int m_nEnhanceDamage { get; set; }

        /// <summary>
        /// Nội
        /// </summary>
        public int m_nEnhanceMagic { get; set; }

        /// <summary>
        /// Kháng cơ bản
        /// </summary>
        public int m_nResist { get; private set; }

        /// <summary>
        /// Kháng hiện tại
        /// </summary>
        public int m_nCurResist { get; private set; }

        /// <summary>
        /// Tăng % sát thương nhận được
        /// </summary>
        public int m_nReceivePercent { get; private set; }
        #endregion

        #region Public methods
        /// <summary>
        /// Trả về kháng cơ bản
        /// </summary>
        /// <returns></returns>
        public int GetResist()
        {
            return this.m_nResist;
        }

        /// <summary>
        /// Trả về kháng hiện tại
        /// </summary>
        /// <returns></returns>
        public int GetCurResist()
        {
            return this.m_nCurResist;
        }

        /// <summary>
        /// Trả về sát thương ngoại
        /// </summary>
        /// <returns></returns>
        public int GetEnhanceDamage()
        {
            return this.m_nEnhanceDamage;
        }
        public KNpcAttribGroup_Damage()
        {
            ClearAll();
        }
        /// <summary>
        /// Trả về sátt thương nội
        /// </summary>
        /// <returns></returns>
        public int GetEnhanceMagic()
        {
            return this.m_nEnhanceMagic;
        }

        /// <summary>
        /// Trả về % sát thương nhận được
        /// </summary>
        /// <returns></returns>
        public int GetReceivePercent()
        {
            return this.m_nReceivePercent;
        }

        /// <summary>
        /// Thêm kháng cơ bản
        /// </summary>
        /// <param name="nAdd"></param>
        public void AddResist(int nAdd)
        {
            this.m_nResist += nAdd;
            this.m_nCurResist += nAdd;
        }

        /// <summary>
        /// Thêm kháng hiện tại
        /// </summary>
        /// <param name="nAdd"></param>
        public void AddCurResist(int nAdd)
        {
            this.m_nCurResist += nAdd;
        }

        /// <summary>
        /// Thêm sát thương ngoại
        /// </summary>
        /// <param name="nAdd"></param>
        public void AddEnhanceDamage(int nAdd)
        {
            this.m_nEnhanceDamage += nAdd;
        }

        /// <summary>
        /// Thêm sát thương nội
        /// </summary>
        /// <param name="nAdd"></param>
        public void AddEnhanceMagic(int nAdd)
        {
            this.m_nEnhanceMagic += nAdd;
        }

        /// <summary>
        /// Thiết lập sát thương ngoại
        /// </summary>
        /// <param name="nValue"></param>
        public void SetEnhanceDamage(int nValue)
        {
            this.m_nEnhanceDamage = nValue;
        }

        /// <summary>
        /// Thiết lập sát thương nội
        /// </summary>
        /// <param name="nValue"></param>
        public void SetEnhanceMagic(int nValue)
        {
            this.m_nEnhanceMagic = nValue;
        }

        /// <summary>
        /// Thêm giá trị % sát thương nhận được
        /// </summary>
        /// <param name="nAdd"></param>
        public void AddReceivePercent(int nAdd)
        {
            this.m_nReceivePercent += nAdd;
        }

        /// <summary>
        /// Thiết lập Cường hóa sát thương nội ngoại
        /// </summary>
        /// <param name="nValue"></param>
        public void SetEnhance(int nValue)
        {
            this.m_nEnhanceDamage = nValue;
            this.m_nEnhanceMagic = nValue;
        }

        /// <summary>
        /// Nhân % sát thương nội ngoại
        /// </summary>
        /// <param name="nPercent"></param>
        public void MulEnhance(int nPercent)
        {
            this.m_nEnhanceDamage = this.m_nEnhanceDamage * nPercent / 100;
            this.m_nEnhanceMagic = this.m_nEnhanceMagic * nPercent / 100;
        }

        /// <summary>
        /// Thiết lập kháng thuộc tính cơ bản
        /// </summary>
        /// <param name="nValue"></param>
        public void SetResist(int nValue)
        {
            this.m_nCurResist = this.m_nResist = nValue;
        }

        /// <summary>
        /// Thiết lập kháng thuộc tính hiện tại
        /// </summary>
        /// <param name="nValue"></param>
        public void SetCurResist(int nValue)
        {
            this.m_nCurResist = nValue;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
        /// <summary>
        /// Làm mới toàn bộ giá trị
        /// </summary>
        public void ClearAll()
        {
            this.m_nEnhanceDamage = 0;
            this.m_nEnhanceMagic = 0;
            this.m_nResist = 0;
            this.m_nCurResist = 0;
            this.m_nReceivePercent = 100;
        }

        /// <summary>
        /// Khôi phục lại toàn bộ giá trị
        /// </summary>
        public void Restore()
        {
            this.m_nEnhanceDamage = 0;
            this.m_nEnhanceMagic = 0;
            this.m_nCurResist = this.m_nResist;
            this.m_nReceivePercent = 100;
        }
        #endregion

        #region Static methods
        /// <summary>
        /// Tăng sát thương ngoại
        /// </summary>
        /// <param name="type"></param>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        public static void ModEnhanceDamage(DAMAGE_TYPE type, GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.AddSeriesDamagePhysics(type, magic.nValue[0] * stackCount);
        }

        /// <summary>
        /// Tăng sát thương ngoại %
        /// </summary>
        /// <param name="type"></param>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        public static void ModEnhanceDamageP(KE_SERIES_TYPE type, GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.AddSeriesDamagePhysicsP(type, magic.nValue[0] * stackCount);
        }

        /// <summary>
        /// Tăng sát thương nội %
        /// </summary>
        /// <param name="type"></param>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        public static void ModEnhanceMagicP(KE_SERIES_TYPE type, GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.AddSeriesDamageMagicsP(type, magic.nValue[0] * stackCount);
        }

        /// <summary>
        /// Tăng sát thương nội
        /// </summary>
        /// <param name="type"></param>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        public static void ModEnhanceMagic(DAMAGE_TYPE type, GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.AddSeriesDamageMagics(type, magic.nValue[0] * stackCount);
        }

        /// <summary>
        /// Tăng kháng thuộc tính
        /// </summary>
        /// <param name="type"></param>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        public static void ModResist(DAMAGE_TYPE type, GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.AddCurResist(type, magic.nValue[0] * stackCount);
        }

        /// <summary>
        /// Tăng % sát thương nhận được
        /// </summary>
        /// <param name="type"></param>
        /// <param name="obj"></param>
        /// <param name="magic"></param>
        /// <param name="nSkillId"></param>
        public static void ModReceivePercent(DAMAGE_TYPE type, GameObject obj, KMagicAttrib magic, int stackCount, int nSkillId)
        {
            obj.m_damage[(int)type].AddCurResist(magic.nValue[0] * stackCount);
        }
        #endregion
    }
}