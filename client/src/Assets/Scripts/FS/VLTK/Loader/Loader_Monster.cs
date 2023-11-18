using FS.VLTK.Entities.Config;
using FS.VLTK.Factory.Animation;
using System.Collections.Generic;
using System.Xml.Linq;

namespace FS.VLTK.Loader
{
    /// <summary>
    /// Đối tượng chứa danh sách các cấu hình trong game
    /// </summary>
    public static partial class Loader
    {
        #region Monster
        /// <summary>
        /// Danh sách quái
        /// </summary>
        public static Dictionary<int, MonsterDataXML> ListMonsters { get; private set; } = new Dictionary<int, MonsterDataXML>();

        /// <summary>
        /// Tải danh sách quái vật từ XML Node
        /// </summary>
        /// <param name="xmlNode"></param>
        public static void LoadMonsters(XElement xmlNode)
        {
            foreach (XElement node in xmlNode.Elements("Monster"))
            {
                int id = int.Parse(node.Attribute("ID").Value);
                string resID = node.Attribute("ResName").Value;
                string name = node.Attribute("Name").Value;

                Loader.ListMonsters[id] = new MonsterDataXML()
                {
                    ID = id,
                    ResID = resID,
                    Name = name,
                };
            }
        }

        /// <summary>
        /// Tải danh sách NPC từ XML Node
        /// </summary>
        /// <param name="xmlNode"></param>
        public static void LoadNPCs(XElement xmlNode)
        {
            xmlNode = xmlNode.Element("NPCs");
            foreach (XElement node in xmlNode.Elements("NPC"))
            {
                int id = int.Parse(node.Attribute("ID").Value);
                string resID = node.Attribute("ResName").Value;
                string name = node.Attribute("Name").Value;

                Loader.ListMonsters[id] = new MonsterDataXML()
                {
                    ID = id,
                    ResID = resID,
                    Name = name,
                };
            }
        }

        #region MonsterActionSet
        /// <summary>
        /// XML quy định động tác quái
        /// </summary>
        public static MonsterActionSetXML MonsterActionSetXML { get; private set; }

        /// <summary>
        /// Tải MonsterActionSet.xml
        /// </summary>
        /// <param name="xmlNode"></param>
        public static void LoadMonsterActionSetXML(XElement xmlNode)
        {
            Loader.MonsterActionSetXML = MonsterActionSetXML.Parse(xmlNode);
        }

        /// <summary>
        /// Đường dẫn file AssetBundle chứa âm thanh động tác quái
        /// </summary>
        public static string MonsterActionSetSoundBundleDir { get; set; }

        /// <summary>
        /// Tải file MonsterActionSetSound
        /// </summary>
        /// <param name="byteData"></param>
        public static void LoadMonsterActionSetSound(byte[] byteData)
        {
            byte[] bytes = byteData;

            int i = 0;

            int bundleDirLength = (int) bytes[i];
            i++;

            string bundleDir = "";
            for (int j = 1; j <= bundleDirLength; j++)
            {
                bundleDir += Loader.charTable[bytes[i]];
                i++;
            }
            Loader.MonsterActionSetSoundBundleDir = bundleDir;

            while (i < bytes.Length)
            {
                int resIDLength = (int) bytes[i];
                i++;

                string resID = "";
                for (int j = 1; j <= resIDLength; j++)
                {
                    resID += Loader.charTable[bytes[i]];
                    i++;
                }

                int fightStandLength = (int) bytes[i];
                i++;

                string fightStand = "";
                for (int j = 1; j <= fightStandLength; j++)
                {
                    fightStand += Loader.charTable[bytes[i]];
                    i++;
                }

                int normalStandLength = (int) bytes[i];
                i++;

                string normalStand = "";
                for (int j = 1; j <= normalStandLength; j++)
                {
                    normalStand += Loader.charTable[bytes[i]];
                    i++;
                }

                int runLength = (int) bytes[i];
                i++;

                string run = "";
                for (int j = 1; j <= runLength; j++)
                {
                    run += Loader.charTable[bytes[i]];
                    i++;
                }

                int woundLength = (int) bytes[i];
                i++;

                string wound = "";
                for (int j = 1; j <= woundLength; j++)
                {
                    wound += Loader.charTable[bytes[i]];
                    i++;
                }

                int dieLength = (int) bytes[i];
                i++;

                string die = "";
                for (int j = 1; j <= dieLength; j++)
                {
                    die += Loader.charTable[bytes[i]];
                    i++;
                }

                int normalAttackLength = (int) bytes[i];
                i++;

                string normalAttack = "";
                for (int j = 1; j <= normalAttackLength; j++)
                {
                    normalAttack += Loader.charTable[bytes[i]];
                    i++;
                }

                int critAttackLength = (int) bytes[i];
                i++;

                string critAttack = "";
                for (int j = 1; j <= critAttackLength; j++)
                {
                    critAttack += Loader.charTable[bytes[i]];
                    i++;
                }

                MonsterAnimationManager.Instance.Sounds[resID] = new MonsterAnimationManager.SoundByRes()
                {
                    ResID = resID,
                    FightStand = fightStand,
                    NormalStand = normalStand,
                    Run = run,
                    Wound = wound,
                    Die = die,
                    NormalAttack = normalAttack,
                    CritAttack = critAttack,
                };
            }
        }
        #endregion
        #endregion
    }
}
