using FS.GameEngine.Logic;
using FS.GameFramework.Logic;
using FS.VLTK.Entities.Config;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK.Loader
{
    /// <summary>
    /// Đối tượng chứa danh sách các cấu hình trong game
    /// </summary>
    public static partial class Loader
    {
        #region Item
        /// <summary>
        /// Danh sách vật phẩm
        /// </summary>
        public static Dictionary<int, ItemData> Items { get; private set; } = new Dictionary<int, ItemData>();

        /// <summary>
        /// Danh sách thuộc tính theo cấp
        /// </summary>
        public static List<MagicAttribLevel> MagicAttribLevels { get; private set; } = new List<MagicAttribLevel>();

        /// <summary>
        /// Danh sách thuộc tính theo bộ
        /// </summary>
        public static List<SuiteActiveProp> SuiteActiveProps { get; private set; } = new List<SuiteActiveProp>();

        /// <summary>
        /// Kích hoạt ngũ hành
        /// </summary>
        public static Dictionary<KE_EQUIP_POSITION, ActiveByItem> g_anEquipActive { get; private set; } = new Dictionary<KE_EQUIP_POSITION, ActiveByItem>();

        /// <summary>
        /// Danh sách tính toán chỉ số vật phẩm
        /// </summary>
        public static ItemValueCaculation ItemValues { get; private set; } = null;

        /// <summary>
        /// Danh sách Exp cần để cường hóa ngũ hành ấn theo cấp độ
        /// </summary>
        public static Dictionary<int, SingNetExp> SignetExps { get; private set; } = new Dictionary<int, SingNetExp>();


        /// <summary>
        /// Vị trí trang bị
        /// </summary>
        public static int[] g_anEquipPos = { (int)KE_EQUIP_POSITION.emEQUIPPOS_WEAPON, (int)KE_EQUIP_POSITION.emEQUIPPOS_WEAPON, (int)KE_EQUIP_POSITION.emEQUIPPOS_BODY, (int)KE_EQUIP_POSITION.emEQUIPPOS_RING, (int)KE_EQUIP_POSITION.emEQUIPPOS_AMULET, (int)KE_EQUIP_POSITION.emEQUIPPOS_FOOT, (int)KE_EQUIP_POSITION.emEQUIPPOS_BELT, (int)KE_EQUIP_POSITION.emEQUIPPOS_HEAD, (int)KE_EQUIP_POSITION.emEQUIPPOS_CUFF, (int)KE_EQUIP_POSITION.emEQUIPPOS_PENDANT, (int)KE_EQUIP_POSITION.emEQUIPPOS_HORSE, (int)KE_EQUIP_POSITION.emEQUIPPOS_MASK, (int)KE_EQUIP_POSITION.emEQUIPPOS_BOOK, (int)KE_EQUIP_POSITION.emEQUIPPOS_ORNAMENT, (int)KE_EQUIP_POSITION.emEQUIPPOS_SIGNET, (int)KE_EQUIP_POSITION.emEQUIPPOS_MANTLE, (int)KE_EQUIP_POSITION.emEQUIPPOS_CHOP, (int)KE_EQUIP_POSITION.emEQUIPPOS_RING_2 };

        /// <summary>
        /// Vị trí trang bị pet
        /// </summary>
        public static int[] g_anEquipPetPos = { (int)KE_EQUIP_PET_POSTION.emEQUIPPOS_HEAD, (int)KE_EQUIP_PET_POSTION.emEQUIPPOS_BODY, (int)KE_EQUIP_PET_POSTION.emEQUIPPOS_BELT, (int)KE_EQUIP_PET_POSTION.emEQUIPPOS_WEAPON, (int)KE_EQUIP_PET_POSTION.emEQUIPPOS_FOOT, (int)KE_EQUIP_PET_POSTION.emEQUIPPOS_CUFF, (int)KE_EQUIP_PET_POSTION.emEQUIPPOS_ORNAMENT, (int)KE_EQUIP_PET_POSTION.emEQUIPPOS_RING, (int)KE_EQUIP_PET_POSTION.emEQUIPPOS_RING_2, (int)KE_EQUIP_PET_POSTION.emEQUIPPOS_PENDANT };

        #region Ngũ hành tương khắc
        public static int[] g_nAccrueSeries = new int[(int)Elemental.COUNT]; // TƯƠNG SINH
        public static int[] g_nConquerSeries = new int[(int)Elemental.COUNT]; // TƯƠNG KHẮC
        public static int[] g_nAccruedSeries = new int[(int)Elemental.COUNT]; // ĐƯỢC TƯƠNG SINH
        public static int[] g_nConqueredSeries = new int[(int)Elemental.COUNT]; // BỊ KHẮC

        /// <summary>
        /// Thông tin ngũ hành tương khắc
        /// </summary>
        public static void LoadAccrueSeries()
        {
            g_nAccrueSeries[(int)Elemental.NONE] = (int)Elemental.NONE;
            g_nConquerSeries[(int)Elemental.NONE] = (int)Elemental.NONE;
            g_nAccruedSeries[(int)Elemental.NONE] = (int)Elemental.NONE;
            g_nConqueredSeries[(int)Elemental.NONE] = (int)Elemental.NONE;

            g_nAccrueSeries[(int)Elemental.METAL] = (int)Elemental.WATER;
            g_nConquerSeries[(int)Elemental.METAL] = (int)Elemental.WOOD;
            g_nAccruedSeries[(int)Elemental.METAL] = (int)Elemental.EARTH;
            g_nConqueredSeries[(int)Elemental.METAL] = (int)Elemental.FIRE;
            g_nAccrueSeries[(int)Elemental.WOOD] = (int)Elemental.FIRE;
            g_nConquerSeries[(int)Elemental.WOOD] = (int)Elemental.EARTH;
            g_nAccruedSeries[(int)Elemental.WOOD] = (int)Elemental.WATER;
            g_nConqueredSeries[(int)Elemental.WOOD] = (int)Elemental.METAL;
            g_nAccrueSeries[(int)Elemental.WATER] = (int)Elemental.WOOD;
            g_nConquerSeries[(int)Elemental.WATER] = (int)Elemental.FIRE;
            g_nAccruedSeries[(int)Elemental.WATER] = (int)Elemental.METAL;
            g_nConqueredSeries[(int)Elemental.WATER] = (int)Elemental.EARTH;
            g_nAccrueSeries[(int)Elemental.FIRE] = (int)Elemental.EARTH;
            g_nConquerSeries[(int)Elemental.FIRE] = (int)Elemental.METAL;
            g_nAccruedSeries[(int)Elemental.FIRE] = (int)Elemental.WOOD;
            g_nConqueredSeries[(int)Elemental.FIRE] = (int)Elemental.WATER;
            g_nAccrueSeries[(int)Elemental.EARTH] = (int)Elemental.METAL;
            g_nConquerSeries[(int)Elemental.EARTH] = (int)Elemental.WATER;
            g_nAccruedSeries[(int)Elemental.EARTH] = (int)Elemental.FIRE;
            g_nConqueredSeries[(int)Elemental.EARTH] = (int)Elemental.WOOD;
        }

        public static bool g_IsAccrue(int nSrcSeries, int nDesSeries)
        {
            return g_InternalIsAccrueConquer(g_nAccrueSeries, nSrcSeries, nDesSeries);
        }

        public static bool g_IsConquer(int nSrcSeries, int nDesSeries)
        {
            return g_InternalIsAccrueConquer(g_nConquerSeries, nSrcSeries, nDesSeries);
        }

        public static bool g_InternalIsAccrueConquer(int[] pAccrueConquerTable, int nSrcSeries, int nDesSeries)
        {
            if (nSrcSeries < (int)Elemental.NONE || nSrcSeries >= (int)Elemental.COUNT)
                return false;

            return nDesSeries == pAccrueConquerTable[nSrcSeries];
        }
        #endregion


        /// <summary>
        /// Thiết lập cấu hình ngũ hành vật phẩm
        /// </summary>
        public static void ActivateItemConfig()
        {


            ActiveByItem _Active = new ActiveByItem();
            _Active.Pos1 = (int)KE_EQUIP_POSITION.emEQUIPPOS_BODY;
            _Active.Pos2 = (int)KE_EQUIP_POSITION.emEQUIPPOS_AMULET;
            g_anEquipActive.Add(KE_EQUIP_POSITION.emEQUIPPOS_HEAD, _Active);


            _Active = new ActiveByItem();
            _Active.Pos1 = (int)KE_EQUIP_POSITION.emEQUIPPOS_HEAD;
            _Active.Pos2 = (int)KE_EQUIP_POSITION.emEQUIPPOS_WEAPON;
            g_anEquipActive.Add(KE_EQUIP_POSITION.emEQUIPPOS_FOOT, _Active);



            _Active = new ActiveByItem();
            _Active.Pos1 = (int)KE_EQUIP_POSITION.emEQUIPPOS_HEAD;
            _Active.Pos2 = (int)KE_EQUIP_POSITION.emEQUIPPOS_WEAPON;
            g_anEquipActive.Add(KE_EQUIP_POSITION.emEQUIPPOS_RING_2, _Active);


            _Active = new ActiveByItem();
            _Active.Pos1 = (int)KE_EQUIP_POSITION.emEQUIPPOS_FOOT;
            _Active.Pos2 = (int)KE_EQUIP_POSITION.emEQUIPPOS_RING_2;
            g_anEquipActive.Add(KE_EQUIP_POSITION.emEQUIPPOS_CUFF, _Active);



            _Active = new ActiveByItem();
            _Active.Pos1 = (int)KE_EQUIP_POSITION.emEQUIPPOS_FOOT;
            _Active.Pos2 = (int)KE_EQUIP_POSITION.emEQUIPPOS_RING_2;
            g_anEquipActive.Add(KE_EQUIP_POSITION.emEQUIPPOS_PENDANT, _Active);


            _Active = new ActiveByItem();
            _Active.Pos1 = (int)KE_EQUIP_POSITION.emEQUIPPOS_PENDANT;
            _Active.Pos2 = (int)KE_EQUIP_POSITION.emEQUIPPOS_CUFF;
            g_anEquipActive.Add(KE_EQUIP_POSITION.emEQUIPPOS_RING, _Active);


            _Active = new ActiveByItem();
            _Active.Pos1 = (int)KE_EQUIP_POSITION.emEQUIPPOS_PENDANT;
            _Active.Pos2 = (int)KE_EQUIP_POSITION.emEQUIPPOS_CUFF;
            g_anEquipActive.Add(KE_EQUIP_POSITION.emEQUIPPOS_BELT, _Active);


            _Active = new ActiveByItem();
            _Active.Pos1 = (int)KE_EQUIP_POSITION.emEQUIPPOS_BELT;
            _Active.Pos2 = (int)KE_EQUIP_POSITION.emEQUIPPOS_RING;
            g_anEquipActive.Add(KE_EQUIP_POSITION.emEQUIPPOS_AMULET, _Active);


            _Active = new ActiveByItem();
            _Active.Pos1 = (int)KE_EQUIP_POSITION.emEQUIPPOS_BELT;
            _Active.Pos2 = (int)KE_EQUIP_POSITION.emEQUIPPOS_RING;
            g_anEquipActive.Add(KE_EQUIP_POSITION.emEQUIPPOS_BODY, _Active);

            _Active = new ActiveByItem();
            _Active.Pos1 = (int)KE_EQUIP_POSITION.emEQUIPPOS_AMULET;
            _Active.Pos2 = (int)KE_EQUIP_POSITION.emEQUIPPOS_BODY;
            g_anEquipActive.Add(KE_EQUIP_POSITION.emEQUIPPOS_WEAPON, _Active);


        }

        /// <summary>
        /// Tải vật phẩm từ tên thành phần tương ứng
        /// </summary>
        /// <param name="bundle"></param>
        /// <param name="pathName"></param>
        private static void LoadItemFromPath(AssetBundle bundle, string pathName)
        {
            XElement xmlNode = FS.VLTK.Loader.ResourceLoader.LoadXMLFromBundle(bundle, pathName);
            foreach (XElement node in xmlNode.Elements())
            {
                int itemID = -1;
                string itemName = "";
                try
                {
                    ItemData itemData = new ItemData();
                    itemData.ItemID = int.Parse(node.Attribute("ItemID").Value);
                    itemID = itemData.ItemID;
                    itemData.Name = node.Attribute("Name") == null ? "" : node.Attribute("Name").Value;
                    itemName = itemData.Name;
                    // Bản võ lâm đéo có thuộc tính này khá là sida khi phân loại nó là thuộc vũ khí nào
                    //itemData.Category = node.Attribute("Category") == null ? (sbyte) -1 : sbyte.Parse(node.Attribute("Category").Value);
                    itemData.Genre = node.Attribute("Genre") == null ? (sbyte)-1 : sbyte.Parse(node.Attribute("Genre").Value);
                    itemData.DetailType = node.Attribute("DetailType") == null ? (short)-1 : short.Parse(node.Attribute("DetailType").Value);
                    itemData.ParticularType = node.Attribute("ParticularType") == null ? (short)-1 : short.Parse(node.Attribute("ParticularType").Value);
                    itemData.Level = node.Attribute("Level") == null ? (byte)0 : byte.Parse(node.Attribute("Level").Value);
                    itemData.SuiteID = node.Attribute("SuiteID") == null ? (int)-1 : int.Parse(node.Attribute("SuiteID").Value);

                    itemData.MapSpriteBundleDir = node.Attribute("IconBundleDir").Value;
                    itemData.MapSpriteAtlasName = node.Attribute("IconAtlasName").Value;

                    itemData.Image = node.Attribute("Image") == null ? "" : node.Attribute("Image").Value;


                    itemData.IconBundleDir = node.Attribute("IconBundleDir").Value;
                    itemData.IconAtlasName = node.Attribute("IconAtlasName").Value;


                    itemData.Descript = node.Attribute("Descript") == null ? "" : node.Attribute("Descript").Value;
                    itemData.BindType = node.Attribute("BindType") == null ? (sbyte)-1 : sbyte.Parse(node.Attribute("BindType").Value);
                    itemData.Series = node.Attribute("Series") == null ? (sbyte)-1 : sbyte.Parse(node.Attribute("Series").Value);
                    itemData.Price = node.Attribute("Price") == null ? -1 : int.Parse(node.Attribute("Price").Value);
                    //itemData.ItemValue = node.Attribute("ItemValue") == null ? -1 : int.Parse(node.Attribute("ItemValue").Value);

                    itemData.ResManID = node.Attribute("ResManID") == null ? (int) -1 : int.Parse(node.Attribute("ResManID").Value);
                    itemData.ResWomanID = node.Attribute("ResWomanID") == null ? (int) -1 : int.Parse(node.Attribute("ResWomanID").Value);
                
                    itemData.ScriptID = node.Attribute("ScriptID") == null ? -1 : int.Parse(node.Attribute("ScriptID").Value);
                    itemData.Category = node.Attribute("Category") == null ? 0 : int.Parse(node.Attribute("Category").Value);
                    itemData.FightPower = node.Attribute("FightPower") == null ? -1 : int.Parse(node.Attribute("FightPower").Value);
                    itemData.MaskResID = node.Attribute("MaskResID") == null ? "" : node.Attribute("MaskResID").Value;

                    if (node.Element("ListBasicProp") != null)
                    {
                        itemData.ListBasicProp = new List<BasicProp>();
                        foreach (XElement subNode in node.Elements("ListBasicProp"))
                        {
                            BasicProp basicProp = new BasicProp()
                            {
                                Index = subNode.Attribute("Index") == null ? (sbyte)-1 : sbyte.Parse(subNode.Attribute("Index").Value),
                                BasicPropPA1Min = subNode.Attribute("BasicPropPA1Min") == null ? (int)0 : int.Parse(subNode.Attribute("BasicPropPA1Min").Value),
                                BasicPropPA1Max = subNode.Attribute("BasicPropPA1Max") == null ? (int)0 : int.Parse(subNode.Attribute("BasicPropPA1Max").Value),
                                BasicPropPA2Min = subNode.Attribute("BasicPropPA2Min") == null ? (int)0 : int.Parse(subNode.Attribute("BasicPropPA2Min").Value),
                                BasicPropPA2Max = subNode.Attribute("BasicPropPA2Max") == null ? (int)0 : int.Parse(subNode.Attribute("BasicPropPA2Max").Value),
                                BasicPropPA3Min = subNode.Attribute("BasicPropPA3Min") == null ? (int)0 : int.Parse(subNode.Attribute("BasicPropPA3Min").Value),
                                BasicPropPA3Max = subNode.Attribute("BasicPropPA3Max") == null ? (int)0 : int.Parse(subNode.Attribute("BasicPropPA3Max").Value),
                                BasicPropType = subNode.Attribute("BasicPropType") == null ? "" : subNode.Attribute("BasicPropType").Value,
                            };
                            itemData.ListBasicProp.Add(basicProp);
                        }
                    }

                    if (node.Element("ListReqProp") != null)
                    {
                        itemData.ListReqProp = new List<ReqProp>();
                        foreach (XElement subNode in node.Elements("ListReqProp"))
                        {



                            ReqProp reqProp = new ReqProp()
                            {
                                ReqPropType = subNode.Attribute("ReqPropType") == null ? (short)0 : short.Parse(subNode.Attribute("ReqPropType").Value),
                                ReqPropValue = subNode.Attribute("ReqPropValue") == null ? (int)0 : int.Parse(subNode.Attribute("ReqPropValue").Value),
                            };
                            itemData.ListReqProp.Add(reqProp);
                        }
                    }

                    if (node.Element("ListEnhance") != null)
                    {
                        itemData.ListEnhance = new List<ENH>();
                        foreach (XElement subNode in node.Elements("ListEnhance"))
                        {
                            ENH enh = new ENH()
                            {
                                EnhMAName = subNode.Attribute("EnhMAName") == null ? "" : subNode.Attribute("EnhMAName").Value,
                                EnhTimes = subNode.Attribute("EnhTimes") == null ? (byte)0 : byte.Parse(subNode.Attribute("EnhTimes").Value),
                                EnhMAPA1Min = subNode.Attribute("EnhMAPA1Min") == null ? (short)0 : short.Parse(subNode.Attribute("EnhMAPA1Min").Value),
                                EnhMAPA1Max = subNode.Attribute("EnhMAPA1Max") == null ? (short)0 : short.Parse(subNode.Attribute("EnhMAPA1Max").Value),
                                EnhMAPA2Min = subNode.Attribute("EnhMAPA2Min") == null ? (short)0 : short.Parse(subNode.Attribute("EnhMAPA2Min").Value),
                                EnhMAPA2Max = subNode.Attribute("EnhMAPA2Max") == null ? (short)0 : short.Parse(subNode.Attribute("EnhMAPA2Max").Value),
                                EnhMAPA3Min = subNode.Attribute("EnhMAPA3Min") == null ? (short)0 : short.Parse(subNode.Attribute("EnhMAPA3Min").Value),
                                EnhMAPA3Max = subNode.Attribute("EnhMAPA3Max") == null ? (short)0 : short.Parse(subNode.Attribute("EnhMAPA3Max").Value),
                            };
                            itemData.ListEnhance.Add(enh);
                        }
                    }



                    if (node.Element("MedicineProp") != null)
                    {
                        itemData.MedicineProp = new List<Medicine>();
                        foreach (XElement subNode in node.Elements("MedicineProp"))
                        {
                            Medicine medicine = new Medicine()
                            {
                                MagicName = subNode.Attribute("MagicName") == null ? "" : subNode.Attribute("MagicName").Value,
                                Value = subNode.Attribute("Value") == null ? (short)0 : short.Parse(subNode.Attribute("Value").Value),
                            };
                            itemData.MedicineProp.Add(medicine);
                        }
                    }

                    if (node.Element("GreenProp") != null)
                    {
                        itemData.GreenProp = new List<PropMagic>();
                        foreach (XElement subNode in node.Elements("GreenProp"))
                        {
                            PropMagic propMagic = new PropMagic()
                            {
                                MagicName = subNode.Attribute("MagicName") == null ? "" : subNode.Attribute("MagicName").Value,
                                MagicLevel = subNode.Attribute("MagicLevel") == null ? "" : subNode.Attribute("MagicLevel").Value,
                                IsActive = subNode.Attribute("IsActive") != null && bool.Parse(subNode.Attribute("IsActive").Value),
                                Index = subNode.Attribute("Index") == null ? (byte)0 : byte.Parse(subNode.Attribute("Index").Value),
                            };
                            itemData.GreenProp.Add(propMagic);
                        }
                    }

                    if (node.Element("HiddenProp") != null)
                    {
                        itemData.HiddenProp = new List<PropMagic>();
                        foreach (XElement subNode in node.Elements("HiddenProp"))
                        {
                            PropMagic propMagic = new PropMagic()
                            {
                                MagicName = subNode.Attribute("MagicName") == null ? "" : subNode.Attribute("MagicName").Value,
                                MagicLevel = subNode.Attribute("MagicLevel") == null ? "" : subNode.Attribute("MagicLevel").Value,
                                IsActive = subNode.Attribute("IsActive") != null && bool.Parse(subNode.Attribute("IsActive").Value),
                                Index = subNode.Attribute("Index") == null ? (byte)0 : byte.Parse(subNode.Attribute("Index").Value),
                            };
                            itemData.HiddenProp.Add(propMagic);
                        }
                    }

                    if (node.Element("RiderProp") != null)
                    {
                        itemData.RiderProp = new List<RiderProp>();
                        foreach (XElement subNode in node.Elements("RiderProp"))
                        {
                            RiderProp riderProp = new RiderProp()
                            {
                                RidePropType = subNode.Attribute("RidePropType") == null ? "" : subNode.Attribute("RidePropType").Value,
                                RidePropPA1Min = subNode.Attribute("RidePropPA1Min") == null ? (short)0 : short.Parse(subNode.Attribute("RidePropPA1Min").Value),
                                RidePropPA1Max = subNode.Attribute("RidePropPA1Max") == null ? (short)0 : short.Parse(subNode.Attribute("RidePropPA1Max").Value),
                                RidePropPA2Min = subNode.Attribute("RidePropPA2Min") == null ? (short)0 : short.Parse(subNode.Attribute("RidePropPA2Min").Value),
                                RidePropPA2Max = subNode.Attribute("RidePropPA2Max") == null ? (short)0 : short.Parse(subNode.Attribute("RidePropPA2Max").Value),
                                RidePropPA3Min = subNode.Attribute("RidePropPA3Min") == null ? (short)0 : short.Parse(subNode.Attribute("RidePropPA3Min").Value),
                                RidePropPA3Max = subNode.Attribute("RidePropPA3Max") == null ? (short)0 : short.Parse(subNode.Attribute("RidePropPA3Max").Value),
                            };
                            itemData.RiderProp.Add(riderProp);
                        }
                    }

                    Loader.Items.Add(itemData.ItemID, itemData);

                }
                catch (Exception)
                {
                    Super.ShowMessageBox("Toác VKL", "Toác ITEM VKL: " + itemID + " - " + itemName);
                    throw;
                }
            }
        }

        /// <summary>
        /// Tải xuống danh sách vật phẩm và trang bị
        /// </summary>
        public static IEnumerator LoadItems(AssetBundle bundle, Action<int> reportProgress)
        {
            Loader.Items.Clear();
            Loader.MagicAttribLevels.Clear();
            Loader.SuiteActiveProps.Clear();



            // Code lại loading full item võ lâm
            Loader.LoadItemFromPath(bundle, Consts.BasicItem_amulet);
            reportProgress?.Invoke(52);
            yield return null;

            Loader.LoadItemFromPath(bundle, Consts.BasicItem_armor);
            reportProgress?.Invoke(54);
            yield return null;

            Loader.LoadItemFromPath(bundle, Consts.BasicItem_belt);
            reportProgress?.Invoke(56);
            yield return null;

            Loader.LoadItemFromPath(bundle, Consts.BasicItem_boots);
            reportProgress?.Invoke(58);
            yield return null;

            Loader.LoadItemFromPath(bundle, Consts.BasicItem_goldequip);
            reportProgress?.Invoke(60);
            yield return null;

            Loader.LoadItemFromPath(bundle, Consts.BasicItem_cuff);
            reportProgress?.Invoke(62);
            yield return null;

            Loader.LoadItemFromPath(bundle, Consts.BasicItem_helm);
            reportProgress?.Invoke(64);
            yield return null;

            Loader.LoadItemFromPath(bundle, Consts.BasicItem_horse);
            reportProgress?.Invoke(66);
            yield return null;

            Loader.LoadItemFromPath(bundle, Consts.BasicItem_magicscript);
            reportProgress?.Invoke(68);
            yield return null;

            Loader.LoadItemFromPath(bundle, Consts.BasicItem_mask);
            reportProgress?.Invoke(70);
            yield return null;

            Loader.LoadItemFromPath(bundle, Consts.BasicItem_meleeweapon);
            reportProgress?.Invoke(72);
            yield return null;

            Loader.LoadItemFromPath(bundle, Consts.BasicItem_platinaequip);
            reportProgress?.Invoke(74);
            yield return null;


            Loader.LoadItemFromPath(bundle, Consts.BasicItem_metal);
            reportProgress?.Invoke(75);
            yield return null;

            Loader.LoadItemFromPath(bundle, Consts.BasicItem_potion);
            reportProgress?.Invoke(76);
            yield return null;

            Loader.LoadItemFromPath(bundle, Consts.BasicItem_rangeweapon);
            reportProgress?.Invoke(78);
            yield return null;

            Loader.LoadItemFromPath(bundle, Consts.BasicItem_ring);
            reportProgress?.Invoke(80);
            yield return null;

            Loader.LoadItemFromPath(bundle, Consts.BasicItem_ShiPin);
            reportProgress?.Invoke(82);
            yield return null;

            Loader.LoadItemFromPath(bundle, Consts.BasicItem_pendant);
            reportProgress?.Invoke(84);
            yield return null;

            Loader.LoadItemFromPath(bundle, Consts.BasicItem_YinJian);
            reportProgress?.Invoke(86);
            yield return null;


            Loader.LoadItemFromPath(bundle, Consts.BasicItem_stuffitem);
            reportProgress?.Invoke(87);
            yield return null;

            Loader.LoadItemFromPath(bundle, Consts.Pet_Equip);
            reportProgress?.Invoke(88);
            yield return null;

            XElement xmlNode = FS.VLTK.Loader.ResourceLoader.LoadXMLFromBundle(bundle, Consts.MagicAttribLevel);
            foreach (XElement node in xmlNode.Elements())
            {
                MagicAttribLevel magicAttribLevel = new MagicAttribLevel()
                {
                    Name = node.Attribute("Name").Value,
                    Level = int.Parse(node.Attribute("Level").Value),
                    MagicName = node.Attribute("MagicName").Value,
                    MAGIC_ID = int.Parse(node.Attribute("MAGIC_ID").Value),
                    MA1Min = int.Parse(node.Attribute("MA1Min").Value),
                    MA1Max = int.Parse(node.Attribute("MA1Max").Value),
                    MA2Min = int.Parse(node.Attribute("MA2Min").Value),
                    MA2Max = int.Parse(node.Attribute("MA2Max").Value),
                    MA3Min = int.Parse(node.Attribute("MA3Min").Value),
                    MA3Max = int.Parse(node.Attribute("MA3Max").Value),
                    // ReqLevel = double.Parse(node.Attribute("ReqLevel").Value),
                    ItemValue = int.Parse(node.Attribute("ItemValue").Value),
                    Series = int.Parse(node.Attribute("Series").Value),
                };
                Loader.MagicAttribLevels.Add(magicAttribLevel);
            }

            XElement xmlNode2 = FS.VLTK.Loader.ResourceLoader.LoadXMLFromBundle(bundle, Consts.SuiteActiveProp);
            foreach (XElement node in xmlNode2.Elements())
            {
                SuiteActiveProp suiteActiveProp = new SuiteActiveProp()
                {
                    SuiteID = sbyte.Parse(node.Attribute("SuiteID").Value),
                    Name = node.Attribute("Name").Value,
                    Head = node.Attribute("Head").Value,
                    Body = node.Attribute("Body").Value,
                    Belt = node.Attribute("Belt").Value,
                    Weapon = node.Attribute("Weapon").Value,
                    Foot = node.Attribute("Foot").Value,
                    Cuff = node.Attribute("Cuff").Value,
                    Amulet = node.Attribute("Amulet").Value,
                    Ring = node.Attribute("Ring").Value,
                    Necklace = node.Attribute("Necklace").Value,
                    Pendant = node.Attribute("Pendant").Value,
                    ListActive = new List<SuiteActive>(),
                };
                foreach (XElement subNode in node.Elements("ListActive"))
                {
                    SuiteActive suiteActive = new SuiteActive()
                    {
                        Index = short.Parse(subNode.Attribute("Index").Value),
                        RequestNum = short.Parse(subNode.Attribute("RequestNum").Value),
                        SuiteMAPA1 = short.Parse(subNode.Attribute("SuiteMAPA1").Value),
                        SuiteMAPA2 = short.Parse(subNode.Attribute("SuiteMAPA2").Value),
                        SuiteMAPA3 = short.Parse(subNode.Attribute("SuiteMAPA3").Value),
                        SuiteName = subNode.Attribute("SuiteName").Value,
                    };
                    suiteActiveProp.ListActive.Add(suiteActive);
                }

                Loader.SuiteActiveProps.Add(suiteActiveProp);
            }

            reportProgress?.Invoke(89);
            yield return null;

            Loader.ActivateItemConfig();
            Loader.LoadAccrueSeries();
        }

        /// <summary>
        /// Tải dữ liệu ItemValue
        /// </summary>
        /// <param name="xmlNode"></param>
        public static void LoadItemValue(XElement xmlNode)
        {
            Loader.ItemValues = ItemValueCaculation.Parse(xmlNode);
        }


        public static void LoadSignetExp(XElement xmlNode)
        {
            Loader.SignetExps.Clear();
            foreach (XElement node in xmlNode.Elements("SingNetExp"))
            {
                SingNetExp signnetExp = SingNetExp.Parse(node);
                Loader.SignetExps[signnetExp.Level] = signnetExp;
            }
        }

        #endregion

        #region Luyện hóa trang bị
        /// <summary>
        /// Danh sách công thức luyện hóa trang bị
        /// </summary>
        public static List<EquipRefineXML> EquipRefineRecipes { get; private set; } = new List<EquipRefineXML>();

        /// <summary>
        /// Tải dữ liệu công thức luyện hóa trang bị
        /// </summary>
        /// <param name="xmlNode"></param>
        public static void LoadEquipRefine(XElement xmlNode)
        {
            Loader.EquipRefineRecipes.Clear();
            foreach (XElement node in xmlNode.Elements("Refine"))
            {
                EquipRefineXML equipRefine = EquipRefineXML.Parse(node);
                Loader.EquipRefineRecipes.Add(equipRefine);
            }
        }
        #endregion
    }
}
