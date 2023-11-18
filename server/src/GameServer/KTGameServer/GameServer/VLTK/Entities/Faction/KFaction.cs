using GameServer.KiemThe.Logic;
using GameServer.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace GameServer.KiemThe.Entities
{
    /// <summary>
    /// Lớp quy định môn phái của hệ thống
    /// </summary>
    public static class KFaction
    {
        /// <summary>
        /// Nhánh môn phái
        /// </summary>
        public class KRoute
        {
            /// <summary>
            /// ID nhánh
            /// </summary>
            public int nID { get; set; }

            /// <summary>
            /// Tên nhánh
            /// </summary>
            public string szName { get; set; }

            /// <summary>
            /// Danh sách kỹ năng
            /// </summary>
            public List<int> arySkills { get; set; } = new List<int>(GameDataDef.KD_FACTION_ROUTE_MAX_SKILL);

            /// <summary>
            /// Tổng số kỹ năng
            /// </summary>
            public int nSkills
            {
                get
                {
                    return this.arySkills.Count;
                }
            }
        }

        /// <summary>
        /// Thuộc tính môn phái
        /// </summary>
        public class KFactionAttirbute
        {
            /// <summary>
            /// ID phái
            /// </summary>
            public int nID { get; set; }

            /// <summary>
            /// Ngũ hành
            /// </summary>
            public KE_SERIES_TYPE nSeries { get; set; }

            /// <summary>
            /// Tên phái
            /// </summary>
            public string szName { get; set; }

            /// <summary>
            /// Danh sách nhánh
            /// </summary>
            public List<KRoute> arRoute { get; set; } = new List<KRoute>(GameDataDef.KD_FACTION_MAX_ROUTE + 1);

            /// <summary>
            /// Tổng số nhánh
            /// </summary>
            public int nRoutes
            {
                get
                {
                    return this.arRoute.Count;
                }
            }

            /// <summary>
            /// Giới hạn giới tính (0: Nam, 1: Nữ, -1: Không giới hạn)
            /// </summary>
            public int nSexLimit { get; set; }
        }

        /// <summary>
        /// Danh sách thuộc tính của từng phái
        /// </summary>
        private static List<KFactionAttirbute> m_sAttribute = new List<KFactionAttirbute>(GameDataDef.KD_MAX_FACTION + 1);

        public static void Init()
        {
            KFaction.m_sAttribute.Clear();

            XElement xmlNode = KTGlobal.ReadXMLData("Config/KT_Faction/Faction.xml");
            foreach (XElement node in xmlNode.Elements("Faction"))
            {
                KFactionAttirbute factionAttirbute = new KFactionAttirbute()
                {
                    nID = int.Parse(node.Attribute("ID").Value),
                    nSeries = (KE_SERIES_TYPE)int.Parse(node.Attribute("Series").Value),
                    szName = node.Attribute("Name").Value,
                    nSexLimit = int.Parse(node.Attribute("Gender").Value),
                };
                KFaction.m_sAttribute.Add(factionAttirbute);

                List<KRoute> routes = new List<KRoute>();
                foreach (XElement subNode in node.Elements("Sub"))
                {
                    KRoute route = new KRoute()
                    {
                        nID = int.Parse(subNode.Attribute("ID").Value),
                        szName = subNode.Attribute("Name").Value,
                    };
                    factionAttirbute.arRoute.Add(route);

                    foreach (XElement skillNode in subNode.Elements("Skill"))
                    {
                        route.arySkills.Add(int.Parse(skillNode.Attribute("ID").Value));
                    }
                }
            }
        }

        /// <summary>
        /// Môn phái có tồn tại không
        /// </summary>
        /// <param name="byFactionId"></param>
        /// <returns></returns>
        public static bool IsFactionExist(int byFactionId)
        {
            KFactionAttirbute faction = KFaction.m_sAttribute.Where(x => x.nID == byFactionId).FirstOrDefault();
            return faction != null;
        }

        /// <summary>
        /// Nhánh môn phái có tồn tại không
        /// </summary>
        /// <param name="byFactionId"></param>
        /// <param name="byRouteId"></param>
        /// <returns></returns>
        public static bool IsRouteExist(int byFactionId, int byRouteId)
        {
            KFactionAttirbute faction = KFaction.m_sAttribute.Where(x => x.nID == byFactionId).FirstOrDefault();
            if (faction == null)
            {
                return false;
            }

            KRoute route = faction.arRoute.Where(x => x.nID == byRouteId).FirstOrDefault();
            return route != null;
        }

        /// <summary>
        /// Trả về danh sách các phái trong hệ thống
        /// </summary>
        /// <returns></returns>
        public static List<KFactionAttirbute> GetFactions()
		{
            return KFaction.m_sAttribute;
		}

        /// <summary>
        /// Trả về ngũ hành môn phái
        /// </summary>
        /// <param name="byFactionId"></param>
        /// <returns></returns>
        public static KE_SERIES_TYPE GetSeries(int byFactionId)
        {
            KFactionAttirbute faction = KFaction.m_sAttribute.Where(x => x.nID == byFactionId).FirstOrDefault();
            if (faction == null)
            {
                return KE_SERIES_TYPE.series_none;
            }

            return faction.nSeries;
        }

        /// <summary>
        /// Trả về tên môn phái
        /// </summary>
        /// <param name="byFactionId"></param>
        /// <returns></returns>
        public static string GetName(int byFactionId)
        {
            KFactionAttirbute faction = KFaction.m_sAttribute.Where(x => x.nID == byFactionId).FirstOrDefault();
            if (faction == null)
            {
                return "";
            }

            return faction.szName;
        }

        /// <summary>
        /// Trả về tổng số nhánh của môn phái
        /// </summary>
        /// <param name="byFactionId"></param>
        /// <returns></returns>
        public static int GetRouteCount(int byFactionId)
        {
            KFactionAttirbute faction = KFaction.m_sAttribute.Where(x => x.nID == byFactionId).FirstOrDefault();
            if (faction == null)
            {
                return 0;
            }
            return faction.nRoutes;
        }

        /// <summary>
        /// Trả về tên nhánh tu luyện
        /// </summary>
        /// <param name="byFactionId"></param>
        /// <param name="byRouteId"></param>
        /// <returns></returns>
        public static string GetRouteName(int byFactionId, int byRouteId)
        {
            KFactionAttirbute faction = KFaction.m_sAttribute.Where(x => x.nID == byFactionId).FirstOrDefault();
            if (faction == null)
            {
                return "";
            }

            KRoute route = faction.arRoute.Where(x => x.nID == byRouteId).FirstOrDefault();
            if (route == null)
            {
                return "";
            }

            return route.szName;
        }

        /// <summary>
        /// Kiểm tra nhánh phái có kỹ năng tương ứng không
        /// </summary>
        /// <param name="byFactionId"></param>
        /// <param name="byRouteId"></param>
        /// <param name="nSkillId"></param>
        /// <returns></returns>
        public static bool IsHaveSkill(int byFactionId, int byRouteId, int nSkillId)
        {
            KFactionAttirbute faction = KFaction.m_sAttribute.Where(x => x.nID == byFactionId).FirstOrDefault();
            if (faction == null)
            {
                return false;
            }

            KRoute route = faction.arRoute.Where(x => x.nID == byRouteId).FirstOrDefault();
            if (route == null)
            {
                return false;
            }

            int skillID = route.arySkills.Where(x => x == nSkillId).FirstOrDefault();

            return skillID != default;
        }

        /// <summary>
        /// Kiểm tra tất cả các nhánh của môn phái có kỹ năng tương ứng không
        /// </summary>
        /// <param name="byFactionId"></param>
        /// <param name="nSkillId"></param>
        /// <returns></returns>
        public static bool IsHaveSkill(int byFactionId, int nSkillId)
        {
            KFactionAttirbute faction = KFaction.m_sAttribute.Where(x => x.nID == byFactionId).FirstOrDefault();
            if (faction == null)
            {
                return false;
            }

            foreach (KRoute route in faction.arRoute)
            {
                int skillID = route.arySkills.Where(x => x == nSkillId).FirstOrDefault();
                if (skillID != 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Trả về danh sách kỹ năng của tất cả các phái
        /// </summary>
        /// <returns></returns>
        public static List<int> GetAllFactionsSkillId()
        {
            List<int> skills = new List<int>();
            foreach (KFactionAttirbute faction in KFaction.m_sAttribute)
            {
                foreach (KRoute route in faction.arRoute)
                {
                    skills.AddRange(route.arySkills);
                }
            }
            return skills;
        }

        /// <summary>
        /// Trả về thông tin phái
        /// </summary>
        /// <param name="byFactionId"></param>
        /// <returns></returns>
        public static KFactionAttirbute GetFactionInfo(int byFactionId)
        {
            KFactionAttirbute faction = KFaction.m_sAttribute.Where(x => x.nID == byFactionId).FirstOrDefault();
            return faction;
        }

        /// <summary>
        /// Trả về danh sách kỹ năng của nhánh
        /// </summary>
        /// <param name="byFactionId"></param>
        /// <param name="byRouteId"></param>
        /// <returns></returns>
        public static List<int> GetRouteSkills(int byFactionId, int byRouteId)
        {
            List<int> skills = new List<int>();
            KFactionAttirbute faction = KFaction.m_sAttribute.Where(x => x.nID == byFactionId).FirstOrDefault();
            if (faction == null)
            {
                return skills;
            }

            KRoute route = faction.arRoute.Where(x => x.nID == byRouteId).FirstOrDefault();
            if (route == null)
            {
                return skills;
            }
            skills.AddRange(route.arySkills);

            return skills;
        }

        /// <summary>
        /// Trả về danh sách kỹ năng của môn phái
        /// </summary>
        /// <param name="byFactionId"></param>
        /// <returns></returns>
        public static List<int> GetFactionSkills(int byFactionId)
        {
            List<int> skills = new List<int>();
            KFactionAttirbute faction = KFaction.m_sAttribute.Where(x => x.nID == byFactionId).FirstOrDefault();
            if (faction == null)
            {
                return skills;
            }

            foreach (KRoute route in faction.arRoute)
            {
                if (route != null)
                {
                    skills.AddRange(route.arySkills);
                }
            }

            return skills;
        }
    }
}