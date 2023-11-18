using GameServer.Logic;
using System.Xml.Linq;

namespace GameServer.KiemThe.Entities
{
    /// <summary>
    /// Thiết lập thông số cơ bản của đối tượng
    /// </summary>
    public static class KNpcSetting
    {
        /// <summary>
        /// Chỉ số cơ bản của người chơi
        /// </summary>
        public class PlayerBaseValue
        {
            /// <summary>
            /// Số Frame động tác đứng (0: Nam, 1: Nữ)
            /// </summary>
            public int[] nStandFrame { get; set; } = new int[2];

            /// <summary>
            /// Số Frame động tác đi bộ (0: Nam, 1: Nữ)
            /// </summary>
            public int[] nWalkFrame { get; set; } = new int[2];

            /// <summary>
            /// Số Frame động tác chạy (0: Nam, 1: Nữ)
            /// </summary>
            public int[] nRunFrame { get; set; } = new int[2];

            /// <summary>
            /// Tốc độ đi bộ
            /// </summary>
            public int nWalkSpeed { get; set; }

            /// <summary>
            /// Tốc chạy
            /// </summary>
            public int nRunSpeed { get; set; }

            /// <summary>
            /// Số Frame động tác đánh vật công ngoại
            /// </summary>
            public int nAttackFrame { get; set; }

            /// <summary>
            /// Số Frame động tác đánh vật công nội
            /// </summary>
            public int nCastFrame { get; set; }

            /// <summary>
            /// Số Frame động tác bị thọ thương
            /// </summary>
            public int nHurtFrame { get; set; }
        };

        /// <summary>
        /// Tốc độ phục hổi thể lực ở trạng thái thường
        /// </summary>
        public static int m_nStaminaNormalAdd { get; private set; }

        /// <summary>
        /// Giảm thể lực ở chế độ truyền công
        /// </summary>
        public static int m_nStaminaExerciseRunSub { get; private set; }

        /// <summary>
        /// Giảm thể lực ở chế độ chiến đấu
        /// </summary>
        public static int m_nStaminaFightRunSub { get; private set; }

        /// <summary>
        /// Giảm thể lực ở chế độ đồ sát
        /// </summary>
        public static int m_nStaminaKillRunSub { get; private set; }

        /// <summary>
        /// Phục hồi thể lực khi ngồi thiền (phần nghìn)
        /// </summary>
        public static int m_nStaminaSitAdd { get; private set; }

        /// <summary>
        /// Chỉ số cơ bản cấu hình hệ thống của nhân vật
        /// </summary>
        private static PlayerBaseValue m_cPlayerBaseValue;

        /// <summary>
        /// Khởi tạo đối tượng
        /// </summary>
        public static void Init()
        {
            XElement xmlNode = KTGlobal.ReadXMLData("Config/KT_Setting/KNpcSetting.xml");
            KNpcSetting.m_cPlayerBaseValue = new PlayerBaseValue();

            {
                XElement node = xmlNode.Element("Common");

                KNpcSetting.m_cPlayerBaseValue.nHurtFrame = int.Parse(node.Attribute("HurtFrame").Value);
                KNpcSetting.m_cPlayerBaseValue.nRunSpeed = int.Parse(node.Attribute("RunSpeed").Value);
                KNpcSetting.m_cPlayerBaseValue.nWalkSpeed = int.Parse(node.Attribute("WalkSpeed").Value);
                KNpcSetting.m_cPlayerBaseValue.nAttackFrame = int.Parse(node.Attribute("AttackFrame").Value);
                KNpcSetting.m_cPlayerBaseValue.nCastFrame = int.Parse(node.Attribute("CastFrame").Value);
            }

            {
                XElement node = xmlNode.Element("Male");
                KNpcSetting.m_cPlayerBaseValue.nWalkFrame[0] = int.Parse(node.Attribute("WalkFrame").Value);
                KNpcSetting.m_cPlayerBaseValue.nRunFrame[0] = int.Parse(node.Attribute("RunFrame").Value);
                KNpcSetting.m_cPlayerBaseValue.nStandFrame[0] = int.Parse(node.Attribute("StandFrame").Value);
            }

            {
                XElement node = xmlNode.Element("Female");
                KNpcSetting.m_cPlayerBaseValue.nWalkFrame[1] = int.Parse(node.Attribute("WalkFrame").Value);
                KNpcSetting.m_cPlayerBaseValue.nRunFrame[1] = int.Parse(node.Attribute("RunFrame").Value);
                KNpcSetting.m_cPlayerBaseValue.nStandFrame[1] = int.Parse(node.Attribute("StandFrame").Value);
            }

            {
                XElement node = xmlNode.Element("Stamina");
                KNpcSetting.m_nStaminaNormalAdd = int.Parse(node.Attribute("NormalAdd").Value);
                KNpcSetting.m_nStaminaSitAdd = int.Parse(node.Attribute("SitAdd").Value);
                KNpcSetting.m_nStaminaExerciseRunSub = int.Parse(node.Attribute("ExerciseRunSub").Value);
                KNpcSetting.m_nStaminaFightRunSub = int.Parse(node.Attribute("FightRunSub").Value);
                KNpcSetting.m_nStaminaKillRunSub = int.Parse(node.Attribute("KillRunSub").Value);
            }
        }

        /// <summary>
        /// Trả về tốc độ đi bộ được thiết lập
        /// </summary>
        /// <returns></returns>
        public static int GetPlayerWalkSpeed()
        {
            return KNpcSetting.m_cPlayerBaseValue.nWalkSpeed;
        }

        /// <summary>
        /// Trả về tốc chạy được thiết lập
        /// </summary>
        /// <returns></returns>
        public static int GetPlayerRunSpeed()
        {
            return KNpcSetting.m_cPlayerBaseValue.nRunSpeed;
        }

        /// <summary>
        /// Trả về số Frame động tác đánh hệ ngoại công
        /// </summary>
        /// <returns></returns>
        public static int GetPlayerAttackFrame()
        {
            return KNpcSetting.m_cPlayerBaseValue.nAttackFrame;
        }

        /// <summary>
        /// Trả về số Frame động tác đánh hệ nội công
        /// </summary>
        /// <returns></returns>
        public static int GetPlayerCastFrame()
        {
            return KNpcSetting.m_cPlayerBaseValue.nCastFrame;
        }

        /// <summary>
        /// Trả về số Frame động tác bị thọ thương
        /// </summary>
        /// <returns></returns>
        public static int GetPlayerHurtFrame()
        {
            return KNpcSetting.m_cPlayerBaseValue.nHurtFrame;
        }

        /// <summary>
        /// Trả về số Frame động tác đứng của nhân vật
        /// </summary>
        /// <param name="bMale"></param>
        /// <returns></returns>
        public static int GetPlayerStandFrame(bool bMale)
        {
            if (bMale)
            {
                return m_cPlayerBaseValue.nStandFrame[0];
            }
            else
            {
                return m_cPlayerBaseValue.nStandFrame[1];
            }
        }

        /// <summary>
        /// Trả về số Frame động tác đi bộ của nhân vật
        /// </summary>
        /// <param name="bMale"></param>
        /// <returns></returns>
        public static int GetPlayerWalkFrame(bool bMale)
        {
            if (bMale)
            {
                return m_cPlayerBaseValue.nWalkFrame[0];
            }
            else
            {
                return m_cPlayerBaseValue.nWalkFrame[1];
            }
        }

        /// <summary>
        /// Trả về số Frame động tác chạy của nhân vật
        /// </summary>
        /// <param name="bMale"></param>
        /// <returns></returns>
        public static int GetPlayerRunFrame(bool bMale)
        {
            if (bMale)
            {
                return m_cPlayerBaseValue.nRunFrame[0];
            }
            else
            {
                return m_cPlayerBaseValue.nRunFrame[1];
            }
        }
    }
}