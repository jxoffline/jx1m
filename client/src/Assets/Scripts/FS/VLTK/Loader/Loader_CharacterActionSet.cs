using FS.VLTK.Entities.ActionSet.Character;
using FS.VLTK.Entities.Config;
using FS.VLTK.Factory.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK.Loader
{
    /// <summary>
    /// Đối tượng chứa danh sách các cấu hình trong game
    /// </summary>
    public static partial class Loader
    {
        #region CharacterActionSet
        /// <summary>
        /// XML quy định động tác nhân vật
        /// </summary>
        public static CharacterActionSetXML CharacterActionSetXML { get; private set; }

        /// <summary>
        /// Tải CharacterActionSet.xml
        /// </summary>
        /// <param name="xmlNode"></param>
        public static void LoadCharacterActionSetXML(XElement xmlNode)
        {
            Loader.CharacterActionSetXML = CharacterActionSetXML.Parse(xmlNode);
        }

        /// <summary>
        /// Danh sách sắp xếp Layer động tác
        /// </summary>
        public static CharacterActionSetLayerSort CharacterActionSetLayerSort { get; private set; }

        /// <summary>
        /// Tải CharacterActionSet/CharacterActionSetLayerSort.xml
        /// </summary>
        /// <param name="xmlNode"></param>
        public static void LoadCharacterActionSetLayerSort(XElement xmlNode)
        {
            Loader.CharacterActionSetLayerSort = CharacterActionSetLayerSort.Parse(xmlNode);
        }

        /// <summary>
        /// Tải ActionConfig
        /// </summary>
        /// <param name="bytes"></param>
        public static void LoadActionConfig(byte[] bytes)
        {
            int i = 0;

            int normalActionSize = (int) bytes[i];
            i++;
            for (int k = 1; k <= normalActionSize; k++)
            {
                int strLength;
                string str;

                strLength = (int) bytes[i];
                i++;

                str = "";
                for (int j = 1; j <= strLength; j++)
                {
                    str += Loader.charTable[bytes[i]];
                    i++;
                }
                string weaponName = str;

                if (!CharacterAnimationManager.Instance.ActionNames.ContainsKey(weaponName))
                {
                    CharacterAnimationManager.Instance.ActionNames[weaponName] = new CharacterAnimationManager.ActionByWeapon()
                    {
                        WeaponID = weaponName,
                        Actions = new Dictionary<PlayerActionType, CharacterAnimationManager.PlayerAction>()
                        {
                            { PlayerActionType.FightStand, new CharacterAnimationManager.PlayerAction() { Type = PlayerActionType.FightStand, Normal = "", Ride = "" } },
                            { PlayerActionType.NormalStand, new CharacterAnimationManager.PlayerAction() { Type = PlayerActionType.NormalStand, Normal = "", Ride = "" } },
                            { PlayerActionType.Walk, new CharacterAnimationManager.PlayerAction() { Type = PlayerActionType.Walk, Normal = "", Ride = "" } },
                            { PlayerActionType.Run, new CharacterAnimationManager.PlayerAction() { Type = PlayerActionType.Run, Normal = "", Ride = "" } },
                            { PlayerActionType.Wound, new CharacterAnimationManager.PlayerAction() { Type = PlayerActionType.Wound, Normal = "", Ride = "" } },
                            { PlayerActionType.Die, new CharacterAnimationManager.PlayerAction() { Type = PlayerActionType.Die, Normal = "", Ride = "" } },
                            { PlayerActionType.NormalAttack, new CharacterAnimationManager.PlayerAction() { Type = PlayerActionType.NormalAttack, Normal = "", Ride = "" } },
                            { PlayerActionType.CritAttack, new CharacterAnimationManager.PlayerAction() { Type = PlayerActionType.CritAttack, Normal = "", Ride = "" } },
                            { PlayerActionType.Magic, new CharacterAnimationManager.PlayerAction() { Type = PlayerActionType.Magic, Normal = "", Ride = "" } },
                            { PlayerActionType.Sit, new CharacterAnimationManager.PlayerAction() { Type = PlayerActionType.Sit, Normal = "", Ride = "" } },
                            { PlayerActionType.Jump, new CharacterAnimationManager.PlayerAction() { Type = PlayerActionType.Jump, Normal = "", Ride = "" } },
                            { PlayerActionType.RunAttack, new CharacterAnimationManager.PlayerAction() { Type = PlayerActionType.RunAttack, Normal = "", Ride = "" } },
                        },
                    };
                }

                strLength = (int) bytes[i];
                i++;

                str = "";
                for (int j = 1; j <= strLength; j++)
                {
                    str += Loader.charTable[bytes[i]];
                    i++;
                }
                CharacterAnimationManager.Instance.ActionNames[weaponName].Actions[PlayerActionType.FightStand].Normal = str;

                strLength = (int) bytes[i];
                i++;

                str = "";
                for (int j = 1; j <= strLength; j++)
                {
                    str += Loader.charTable[bytes[i]];
                    i++;
                }
                CharacterAnimationManager.Instance.ActionNames[weaponName].Actions[PlayerActionType.NormalStand].Normal = str;

                strLength = (int) bytes[i];
                i++;

                str = "";
                for (int j = 1; j <= strLength; j++)
                {
                    str += Loader.charTable[bytes[i]];
                    i++;
                }
                CharacterAnimationManager.Instance.ActionNames[weaponName].Actions[PlayerActionType.Walk].Normal = str;

                strLength = (int) bytes[i];
                i++;

                str = "";
                for (int j = 1; j <= strLength; j++)
                {
                    str += Loader.charTable[bytes[i]];
                    i++;
                }
                CharacterAnimationManager.Instance.ActionNames[weaponName].Actions[PlayerActionType.Run].Normal = str;
                CharacterAnimationManager.Instance.ActionNames[weaponName].Actions[PlayerActionType.RunAttack].Normal = str;

                strLength = (int) bytes[i];
                i++;

                str = "";
                for (int j = 1; j <= strLength; j++)
                {
                    str += Loader.charTable[bytes[i]];
                    i++;
                }
                CharacterAnimationManager.Instance.ActionNames[weaponName].Actions[PlayerActionType.Wound].Normal = str;

                strLength = (int) bytes[i];
                i++;

                str = "";
                for (int j = 1; j <= strLength; j++)
                {
                    str += Loader.charTable[bytes[i]];
                    i++;
                }
                CharacterAnimationManager.Instance.ActionNames[weaponName].Actions[PlayerActionType.Die].Normal = str;

                strLength = (int) bytes[i];
                i++;

                str = "";
                for (int j = 1; j <= strLength; j++)
                {
                    str += Loader.charTable[bytes[i]];
                    i++;
                }
                CharacterAnimationManager.Instance.ActionNames[weaponName].Actions[PlayerActionType.NormalAttack].Normal = str;

                strLength = (int) bytes[i];
                i++;

                str = "";
                for (int j = 1; j <= strLength; j++)
                {
                    str += Loader.charTable[bytes[i]];
                    i++;
                }
                CharacterAnimationManager.Instance.ActionNames[weaponName].Actions[PlayerActionType.CritAttack].Normal = str;

                strLength = (int) bytes[i];
                i++;

                str = "";
                for (int j = 1; j <= strLength; j++)
                {
                    str += Loader.charTable[bytes[i]];
                    i++;
                }
                CharacterAnimationManager.Instance.ActionNames[weaponName].Actions[PlayerActionType.Magic].Normal = str;

                strLength = (int) bytes[i];
                i++;

                str = "";
                for (int j = 1; j <= strLength; j++)
                {
                    str += Loader.charTable[bytes[i]];
                    i++;
                }
                CharacterAnimationManager.Instance.ActionNames[weaponName].Actions[PlayerActionType.Sit].Normal = str;

                strLength = (int) bytes[i];
                i++;

                str = "";
                for (int j = 1; j <= strLength; j++)
                {
                    str += Loader.charTable[bytes[i]];
                    i++;
                }
                CharacterAnimationManager.Instance.ActionNames[weaponName].Actions[PlayerActionType.Jump].Normal = str;
            }

            int rideActionSize = (int) bytes[i];
            i++;
            for (int k = 1; k <= rideActionSize; k++)
            {
                int strLength;
                string str;

                strLength = (int) bytes[i];
                i++;

                str = "";
                for (int j = 1; j <= strLength; j++)
                {
                    str += Loader.charTable[bytes[i]];
                    i++;
                }
                string weaponName = str;

                if (!CharacterAnimationManager.Instance.ActionNames.ContainsKey(weaponName))
                {
                    CharacterAnimationManager.Instance.ActionNames[weaponName] = new CharacterAnimationManager.ActionByWeapon()
                    {
                        WeaponID = weaponName,
                        Actions = new Dictionary<PlayerActionType, CharacterAnimationManager.PlayerAction>()
                        {
                            { PlayerActionType.FightStand, new CharacterAnimationManager.PlayerAction() { Type = PlayerActionType.FightStand, Normal = "", Ride = "" } },
                            { PlayerActionType.NormalStand, new CharacterAnimationManager.PlayerAction() { Type = PlayerActionType.NormalStand, Normal = "", Ride = "" } },
                            { PlayerActionType.Walk, new CharacterAnimationManager.PlayerAction() { Type = PlayerActionType.Walk, Normal = "", Ride = "" } },
                            { PlayerActionType.Run, new CharacterAnimationManager.PlayerAction() { Type = PlayerActionType.Run, Normal = "", Ride = "" } },
                            { PlayerActionType.Wound, new CharacterAnimationManager.PlayerAction() { Type = PlayerActionType.Wound, Normal = "", Ride = "" } },
                            { PlayerActionType.Die, new CharacterAnimationManager.PlayerAction() { Type = PlayerActionType.Die, Normal = "", Ride = "" } },
                            { PlayerActionType.NormalAttack, new CharacterAnimationManager.PlayerAction() { Type = PlayerActionType.NormalAttack, Normal = "", Ride = "" } },
                            { PlayerActionType.CritAttack, new CharacterAnimationManager.PlayerAction() { Type = PlayerActionType.CritAttack, Normal = "", Ride = "" } },
                            { PlayerActionType.Magic, new CharacterAnimationManager.PlayerAction() { Type = PlayerActionType.Magic, Normal = "", Ride = "" } },
                            { PlayerActionType.Sit, new CharacterAnimationManager.PlayerAction() { Type = PlayerActionType.Sit, Normal = "", Ride = "" } },
                            { PlayerActionType.Jump, new CharacterAnimationManager.PlayerAction() { Type = PlayerActionType.Jump, Normal = "", Ride = "" } },
                            { PlayerActionType.RunAttack, new CharacterAnimationManager.PlayerAction() { Type = PlayerActionType.RunAttack, Normal = "", Ride = "" } },
                        },
                    };
                }

                strLength = (int) bytes[i];
                i++;

                str = "";
                for (int j = 1; j <= strLength; j++)
                {
                    str += Loader.charTable[bytes[i]];
                    i++;
                }
                CharacterAnimationManager.Instance.ActionNames[weaponName].Actions[PlayerActionType.FightStand].Ride = str;

                strLength = (int) bytes[i];
                i++;

                str = "";
                for (int j = 1; j <= strLength; j++)
                {
                    str += Loader.charTable[bytes[i]];
                    i++;
                }
                CharacterAnimationManager.Instance.ActionNames[weaponName].Actions[PlayerActionType.NormalStand].Ride = str;

                strLength = (int) bytes[i];
                i++;

                str = "";
                for (int j = 1; j <= strLength; j++)
                {
                    str += Loader.charTable[bytes[i]];
                    i++;
                }
                CharacterAnimationManager.Instance.ActionNames[weaponName].Actions[PlayerActionType.Walk].Ride = str;

                strLength = (int) bytes[i];
                i++;

                str = "";
                for (int j = 1; j <= strLength; j++)
                {
                    str += Loader.charTable[bytes[i]];
                    i++;
                }
                CharacterAnimationManager.Instance.ActionNames[weaponName].Actions[PlayerActionType.Run].Ride = str;
                CharacterAnimationManager.Instance.ActionNames[weaponName].Actions[PlayerActionType.RunAttack].Ride = str;

                strLength = (int) bytes[i];
                i++;

                str = "";
                for (int j = 1; j <= strLength; j++)
                {
                    str += Loader.charTable[bytes[i]];
                    i++;
                }
                CharacterAnimationManager.Instance.ActionNames[weaponName].Actions[PlayerActionType.Wound].Ride = str;

                strLength = (int) bytes[i];
                i++;

                str = "";
                for (int j = 1; j <= strLength; j++)
                {
                    str += Loader.charTable[bytes[i]];
                    i++;
                }
                CharacterAnimationManager.Instance.ActionNames[weaponName].Actions[PlayerActionType.Die].Ride = str;

                strLength = (int) bytes[i];
                i++;

                str = "";
                for (int j = 1; j <= strLength; j++)
                {
                    str += Loader.charTable[bytes[i]];
                    i++;
                }
                CharacterAnimationManager.Instance.ActionNames[weaponName].Actions[PlayerActionType.NormalAttack].Ride = str;

                strLength = (int) bytes[i];
                i++;

                str = "";
                for (int j = 1; j <= strLength; j++)
                {
                    str += Loader.charTable[bytes[i]];
                    i++;
                }
                CharacterAnimationManager.Instance.ActionNames[weaponName].Actions[PlayerActionType.CritAttack].Ride = str;

                strLength = (int) bytes[i];
                i++;

                str = "";
                for (int j = 1; j <= strLength; j++)
                {
                    str += Loader.charTable[bytes[i]];
                    i++;
                }
                CharacterAnimationManager.Instance.ActionNames[weaponName].Actions[PlayerActionType.Magic].Ride = str;

                strLength = (int) bytes[i];
                i++;

                str = "";
                for (int j = 1; j <= strLength; j++)
                {
                    str += Loader.charTable[bytes[i]];
                    i++;
                }
                CharacterAnimationManager.Instance.ActionNames[weaponName].Actions[PlayerActionType.Sit].Ride = str;

                strLength = (int) bytes[i];
                i++;

                str = "";
                for (int j = 1; j <= strLength; j++)
                {
                    str += Loader.charTable[bytes[i]];
                    i++;
                }
                CharacterAnimationManager.Instance.ActionNames[weaponName].Actions[PlayerActionType.Jump].Ride = str;
            }
        }

        /// <summary>
        /// Đường dẫn File Bundle chứa âm thanh người
        /// </summary>
        public static string CharacterActionSetSoundBundleDir { get; private set; }

        /// <summary>
        /// Tải file CharacterActionSetSound
        /// </summary>
        /// <param name="byteData"></param>
        public static void LoadCharacterActionSetSound(byte[] byteData)
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
            Loader.CharacterActionSetSoundBundleDir = bundleDir;

            while (i < bytes.Length)
            {
                int actionNameLength = (int) bytes[i];
                i++;

                string actionName = "";
                for (int j = 1; j <= actionNameLength; j++)
                {
                    actionName += Loader.charTable[bytes[i]];
                    i++;
                }

                int maleSoundLength = (int) bytes[i];
                i++;

                string maleSound = "";
                for (int j = 1; j <= maleSoundLength; j++)
                {
                    maleSound += Loader.charTable[bytes[i]];
                    i++;
                }

                int femaleSoundLength = (int) bytes[i];
                i++;

                string femaleSound = "";
                for (int j = 1; j <= femaleSoundLength; j++)
                {
                    femaleSound += Loader.charTable[bytes[i]];
                    i++;
                }

                CharacterAnimationManager.Instance.MaleSounds[actionName] = new CharacterAnimationManager.SoundByAction()
                {
                    ActionName = actionName,
                    SoundName = maleSound.Replace(".wav", "").Replace(".mp3", ""),
                };
                CharacterAnimationManager.Instance.FemaleSounds[actionName] = new CharacterAnimationManager.SoundByAction()
                {
                    ActionName = actionName,
                    SoundName = femaleSound.Replace(".wav", "").Replace(".mp3", ""),
                };
            }
        }
        #endregion
    }
}
