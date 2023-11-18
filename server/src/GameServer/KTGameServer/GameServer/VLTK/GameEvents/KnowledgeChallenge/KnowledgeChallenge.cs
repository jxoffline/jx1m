using GameServer.Logic;
using System.Collections.Generic;
using System.Xml.Linq;

namespace GameServer.KiemThe.GameEvents.KnowledgeChallenge
{
    /// <summary>
    /// Sự kiện đoán hoa đăng
    /// </summary>
    public static class KnowledgeChallenge
    {
        #region Define
        /// <summary>
        /// Thiết lập tổng quan
        /// </summary>
        public class KC_Config
        {
            /// <summary>
            /// Cấp độ yêu cầu
            /// </summary>
            public int LimitLevel { get; set; }

            /// <summary>
            /// Chuyển đối tượng từ XMLNode
            /// </summary>
            /// <param name="xmlNode"></param>
            /// <returns></returns>
            public static KC_Config Parse(XElement xmlNode)
            {
                return new KC_Config()
                {
                    LimitLevel = int.Parse(xmlNode.Attribute("LimitLevel").Value),
                };
            }
        }

        /// <summary>
        /// Thông tin câu hỏi
        /// </summary>
        public class KC_Question
        {
            /// <summary>
            /// ID tự tăng
            /// </summary>
            private static int AutoID { get; set; }

            /// <summary>
            /// ID câu hỏi
            /// </summary>
            public int ID { get; set; }

            /// <summary>
            /// Mô tả câu hỏi
            /// </summary>
            public string Content { get; set; }

            /// <summary>
            /// Danh sách câu trả lời
            /// </summary>
            public Dictionary<int, string> Answers { get; set; }

            /// <summary>
            /// ID câu trả lời đúng
            /// </summary>
            public int CorrectAnswerID { get; set; }

            /// <summary>
            /// Chuyển đối tượng từ XMLNode
            /// </summary>
            /// <param name="xmlNode"></param>
            /// <returns></returns>
            public static KC_Question Parse(XElement xmlNode)
            {
                /// Tăng ID tự động
                KC_Question.AutoID++;
                KC_Question question = new KC_Question()
                {
                    ID = KC_Question.AutoID,
                    Content = xmlNode.Attribute("Content").Value,
                    Answers = new Dictionary<int, string>()
                    {
                        { 1, xmlNode.Attribute("RightAnswer").Value },
                        { 2, xmlNode.Attribute("OtherAnswer_1").Value },
                        { 3, xmlNode.Attribute("OtherAnswer_2").Value },
                        { 4, xmlNode.Attribute("OtherAnswer_3").Value },
                    },
                    CorrectAnswerID = 1,
                };
                return question;
            }
        }

        /// <summary>
        /// Thiết lập phần thưởng mỗi lượt trả lời đúng
        /// </summary>
        public class KC_Award
        {
            /// <summary>
            /// Đồng khóa
            /// </summary>
            public int BoundToken { get; set; }

            /// <summary>
            /// Bạc khóa
            /// </summary>
            public int BoundMoney { get; set; }

            /// <summary>
            /// Uy danh
            /// </summary>
            public int Prestige { get; set; }

            /// <summary>
            /// Kinh nghiệm
            /// </summary>
            public int Exp { get; set; }

            /// <summary>
            /// Chuyển đối tượng từ XMLNode
            /// </summary>
            /// <param name="xmlNode"></param>
            /// <returns></returns>
            public static KC_Award Parse(XElement xmlNode)
            {
                return new KC_Award()
                {
                    BoundToken = int.Parse(xmlNode.Attribute("BoundToken").Value),
                    BoundMoney = int.Parse(xmlNode.Attribute("BoundMoney").Value),
                    Prestige = int.Parse(xmlNode.Attribute("Prestige").Value),
                    Exp = int.Parse(xmlNode.Attribute("Exp").Value),
                };
            }
        }

        /// <summary>
        /// Thông tin sự kiện
        /// </summary>
        public class KC_Event
        {
            /// <summary>
            /// Thiết lập sự kiện
            /// </summary>
            public class EventConfig
            {
                /// <summary>
                /// Số câu trả lời tối đa trong cả sự kiện
                /// </summary>
                public int MaxQuestions { get; set; }

                /// <summary>
                /// Tổng thời gian NPC đứng lại cho người chơi trả lời câu hỏi (mili-giây)
                /// </summary>
                public int NPCStandDuration { get; set; }

                /// <summary>
                /// Thời gian Delay giữa mỗi lần mở câu hỏi (mili-giây)
                /// </summary>
                public int DelayTicksEachQuestion { get; set; }

                /// <summary>
                /// Chuyển đối tượng từ XMLNode
                /// </summary>
                /// <param name="xmlNode"></param>
                /// <returns></returns>
                public static EventConfig Parse(XElement xmlNode)
                {
                    return new EventConfig()
                    {
                        MaxQuestions = int.Parse(xmlNode.Attribute("MaxQuestions").Value),
                        NPCStandDuration = int.Parse(xmlNode.Attribute("NPCStandDuration").Value),
                        DelayTicksEachQuestion = int.Parse(xmlNode.Attribute("DelayTicksEachQuestion").Value),
                    };
                }
            }

            /// <summary>
            /// NPC trong sự kiện
            /// </summary>
            public class EventNPC
            {
                /// <summary>
                /// ID NPC
                /// </summary>
                public int NPCID { get; set; }

                /// <summary>
                /// ID quái
                /// </summary>
                public int MonsterID { get; set; }

                /// <summary>
                /// Tên NPC (bỏ trống sẽ lấy ở File cấu hình)
                /// </summary>
                public string Name { get; set; }

                /// <summary>
                /// Danh hiệu NPC (bỏ trống sẽ lấy ở File cấu hình)
                /// </summary>
                public string Title { get; set; }

                /// <summary>
                /// Danh sách vị trí theo bản đồ
                /// </summary>
                public Dictionary<int, List<UnityEngine.Vector2Int>> Positions { get; set; }

                /// <summary>
                /// Chuyển đối tượng từ XMLNode
                /// </summary>
                /// <param name="xmlNode"></param>
                /// <returns></returns>
                public static EventNPC Parse(XElement xmlNode)
                {
                    EventNPC npc = new EventNPC()
                    {
                        NPCID = int.Parse(xmlNode.Attribute("NPCID").Value),
                        MonsterID = int.Parse(xmlNode.Attribute("MonsterID").Value),
                        Name = xmlNode.Attribute("Name").Value,
                        Title = xmlNode.Attribute("Title").Value,
                        Positions = new Dictionary<int, List<UnityEngine.Vector2Int>>(),
                    };

                    foreach (XElement node in xmlNode.Elements("VillagePosition"))
                    {
                        int mapID = int.Parse(node.Attribute("MapID").Value);
                        List<UnityEngine.Vector2Int> positions = new List<UnityEngine.Vector2Int>();
                        foreach (string positionString in node.Attribute("Positions").Value.Split(';'))
                        {
                            string[] fields = positionString.Split('_');
                            int posX = int.Parse(fields[0]);
                            int posY = int.Parse(fields[1]);
                            positions.Add(new UnityEngine.Vector2Int(posX, posY));
                        }
                        npc.Positions[mapID] = positions;
                    }

                    return npc;
                }
            }

            /// <summary>
            /// Thiết lập sự kiện
            /// </summary>
            public EventConfig Config { get; set; }

            /// <summary>
            /// Thông tin NPC
            /// </summary>
            public EventNPC NPC { get; set; }

            /// <summary>
            /// Chuyển đối tượng từ XMLNode
            /// </summary>
            /// <param name="xmlNode"></param>
            /// <returns></returns>
            public static KC_Event Parse(XElement xmlNode)
            {
                return new KC_Event()
                {
                    Config = EventConfig.Parse(xmlNode.Element("Config")),
                    NPC = EventNPC.Parse(xmlNode.Element("NPC")),
                };
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Thiết lập tổng quan
        /// </summary>
        public static KC_Config Config { get; set; }

        /// <summary>
        /// Danh sách câu hỏi theo ID
        /// </summary>
        public static Dictionary<int, KC_Question> Questions { get; set; }

        /// <summary>
        /// Danh sách ID câu hỏi
        /// </summary>
        public static List<int> QuestionIDs { get; set; }

        /// <summary>
        /// Chi tiết sự kiện
        /// </summary>
        public static KC_Event Event { get; set; }

        /// <summary>
        /// Thông tin quà thưởng mỗi lượt trả lời đúng
        /// </summary>
        public static KC_Award Award { get; set; }
        #endregion

        #region Public methods
        /// <summary>
        /// Khởi tạo dữ liệu Đoán hoa đăng
        /// </summary>
        public static void Init()
        {
            XElement xmlNode = KTGlobal.ReadXMLData("Config/KT_GameEvents/KnowledgeChallenge.xml");
            KnowledgeChallenge.Config = KC_Config.Parse(xmlNode.Element("Config"));
            KnowledgeChallenge.Questions = new Dictionary<int, KC_Question>();
            KnowledgeChallenge.QuestionIDs = new List<int>();
            foreach (XElement node in xmlNode.Element("Questions").Elements("Question"))
            {
                KC_Question question = KC_Question.Parse(node);
                KnowledgeChallenge.Questions[question.ID] = question;
                KnowledgeChallenge.QuestionIDs.Add(question.ID);
            }
            KnowledgeChallenge.Event = KC_Event.Parse(xmlNode.Element("Event"));
            KnowledgeChallenge.Award = KC_Award.Parse(xmlNode.Element("Award"));
        }
        #endregion
    }
}
