using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Utilities;
using GameServer.Logic;
using System;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Thực thi hiệu ứng ngũ hành
    /// </summary>
    public class KSpecialStateManager
    {
        /// <summary>
        /// Danh sách trạng thái ngũ hành
        /// </summary>
        public static ArrayOfKSpecialState g_arSpecialState { get; private set; } = new ArrayOfKSpecialState();

        /// <summary>
        /// Khởi tạo dữ liệu
        /// </summary>
        public static void Init()
        {
            string file = KTGlobal.GetDataPath("Config/KT_Skill/SpecialState.xml");
            using (TextReader reader = new StreamReader(file))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(ArrayOfKSpecialState));
                KSpecialStateManager.g_arSpecialState = (ArrayOfKSpecialState)serializer.Deserialize(reader);
            }
        }

        /// <summary>
        /// Nhận trạng thái ngũ hành
        /// </summary>
        /// <param name="target"></param>
        /// <param name="magic"></param>
        /// <param name="eState"></param>
        /// <param name="attacker"></param>
        /// <param name="skill"></param>
        /// <param name="skillPos"></param>
        /// <param name="skillPd"></param>
        public static void OnReceiveState(GameObject target, KMagicAttrib magic, KE_STATE eState, GameObject attacker, SkillLevelRef skill, UnityEngine.Vector2 skillPos, PropertyDictionary skillPd)
        {

           // Console.WriteLine(magic);
            if (eState < KE_STATE.emSTATE_BEGIN || eState >= KE_STATE.emSTATE_NUM)
            {
                return;
            }

            /// Nếu đối tượng đang trong trạng thái khinh công thì bỏ qua
            if (target.m_eDoing == KE_NPC_DOING.do_jump)
            {
                return;
            }

            int nRate = magic.nValue[0];
            int nTime = magic.nValue[1];

            if (nRate <= 0 || nTime <= 0)
            {
                return;
            }
            int nMaxFrame = magic.nValue[1];

            if (magic.nValue[2] <= 0)
            {
                string param = KSpecialStateManager.g_arSpecialState.KSpecialState.Where(x => x.StateID == Enum.GetName(eState.GetType(), eState)).FirstOrDefault()?.MaxFrame;
                if (!string.IsNullOrEmpty(param))
                {
                    nMaxFrame = int.Parse(param);
                }
            }
            else
            {
                nMaxFrame = magic.nValue[2];
            }

            /// Trạng thái của thằng Target
            KNpcAttribGroup_State targetstate = target.m_state[(int)eState];

            /// Nếu đang bị trạng thái này rồi thì thôi
            if (!targetstate.IsOver)
            {
                return;
            }
            /// Nếu có thể bỏ qua trạng thái này
            if (targetstate.IgnoreRate)
            {
                return;
            }

            int nPercent = 0;
            int nNewTime = 0;

            int nStateBaseRateParam = KTGlobal.StateBaseRateParam;
            int nStateBaseTimeParam = KTGlobal.StateBaseTimeParam;

            /// Toác gì đó
            if (targetstate.StateRestRate + nStateBaseRateParam <= 0 || targetstate.StateRestRate + nStateBaseTimeParam <= 0)
            {
                return;
            }

            /// Trừ đi rate từ thằng bị tấn công
            int stateRestRateFinal = targetstate.StateRestRate + nStateBaseRateParam;
            nPercent = nRate - (stateRestRateFinal == 0 ? 0 : (nRate * targetstate.StateRestRate) / stateRestRateFinal);
            /// Trừ đi thời gian của thằng phòng thủ
            int stateRestTimeFinal = targetstate.StateRestTime + nStateBaseTimeParam;
            nNewTime = nTime - (stateRestTimeFinal == 0 ? 0 : (nTime * targetstate.StateRestTime) / stateRestTimeFinal);

            /// Nếu có Symbol căn cứ khoảng cách gây trạng thái tương ứng
            if (skillPd.ContainsKey((int)MAGIC_ATTRIB.magic_skilladdition_addmagicbydist))
            {
                KMagicAttrib magicAttrib = skillPd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.magic_skilladdition_addmagicbydist);

                /// Loại hiệu ứng
                int stateID = magicAttrib.nValue[0];

                /// Nếu trùng với trạng thái ngũ hành hiện tại
                if (stateID == (int)eState)
                {
                    /// Khoảng cách giữa bản thân và mục tiêu
                    float distance = KTGlobal.GetDistanceBetweenGameObjects(attacker, target);

                    /// Lấy Min giữa khoảng cách này và khoảng cách tối đa
                    distance = Math.Min(distance, magicAttrib.nValue[2]);

                    /// Hệ số nhân
                    float percent = magicAttrib.nValue[1] / 100f;

                    /// Tỷ lệ % hiệu ứng cộng thêm
                    nNewTime = (int)(nNewTime + nNewTime * percent / 100f);
                }
            }

            /// Nếu vượt quá giá trị Max
            if (nNewTime > nMaxFrame)
            {
                nNewTime = nMaxFrame;
            }

            int nRand = KTGlobal.GetRandomNumber(0, 99);
            /// Thực hiện random xem có thực thi hiệu ứng không
            if (nRand < nPercent)
            {
                if (skill.Data.StartPoint == 6)
                {
                    skillPos = new UnityEngine.Vector2((int)attacker.CurrentPos.X, (int)attacker.CurrentPos.Y);
                }
                /// Thêm trạng thái vào đối tượng
                target.AddSpecialState(attacker, skillPos, eState, nNewTime, magic.nValue[2], true);
            }
        }
    }
}