using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Logic;
using GameServer.Logic;
using KF.Client;
using Server.Data;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.KiemThe.Core.Item
{
    /// <summary>
    /// Định nghĩa khi gọi 1 item mới in game để tính toán +- thuộc tính
    /// </summary>
    public class KItem
    {
        public int ItemDBID
        {
            get
            {
                return _GoodDatas.Id;
            }
        }

        public int SuiteID
        {
            get
            {
                return GetBaseItem.SuiteID;
            }
        }

        public ItemData GetBaseItem
        {
            get
            {
                return _ItemData;
            }
        }

        /// <summary>
        /// base Atribute
        /// </summary>
        public List<KMagicAttrib> TotalBaseAttrib { get; set; }

        /// <summary>
        /// Yêu cầu mặc đồ
        /// </summary>
        public List<KREQUIRE_ATTR> TotalRequest { get; set; }

        /// <summary>
        /// Tất cả thuộc tính cường hóa
        /// </summary>
        public List<KSOCKET> TotalEnhance { get; set; }

        /// <summary>
        /// Danh sách các dòng xanh
        /// </summary>
        public List<KMagicAttrib> TotalGreenProp { get; set; }

        /// <summary>
        /// Socket này có được kích hoạt hay không
        /// </summary>
        public List<KSOCKET> TotalHiddenProp { get; set; }

        /// <summary>
        /// Thuộc tính thú cưỡi
        /// </summary>
        public List<KMagicAttrib> TotalRiderProp { get; set; }

        /// <summary>
        /// Ảnh của vật phẩm này
        /// </summary>
        public ItemData _ItemData { get; set; }

        /// <summary>
        /// Loại trang bị là trang bị gì
        /// </summary>
        public KE_ITEM_EQUIP_DETAILTYPE TypeItem { get; set; }

        /// <summary>
        /// Có phải là nhẫn 2 hay không
        /// </summary>
        public bool IsSubRing { get; set; }

        /// <summary>
        /// Có phải trnag bị hay không
        /// </summary>
        public bool ISEQUIP { get; set; }

        /// <summary>
        /// Đã attack thuộc tính vào người chưa để biết đường mà detack nếu mà vật phẩm này có tháo ra cũng ko bị lỗi
        /// </summary>
        public bool IsAttack { get; set; }

        /// <summary>
        /// Độ ưu tiên Attack chỉ số vào người từ thấp tới cao
        /// </summary>
        public int InitLevel { get; set; } = 0;

        /// <summary>
        /// Cấp độ cường hóa
        /// </summary>
        public int Forge_Level { get; set; }

        public GoodsData _GoodDatas { get; set; }

        /// <summary>
        /// Attack Effect TO PLAYER
        /// </summary>
        /// <param name="_Player"></param>
        public void AttackEffect(GameObject _Player, bool IsDetack)
        {
            /// Nếu là Ngũ Hành Ấn
            if (ItemManager.KD_ISSIGNET(this._ItemData.DetailType))
            {
                /// Lấy ra thông tin dòng cường hóa ngũ hành tương khắc
                if (this._GoodDatas.OtherParams.TryGetValue(ItemPramenter.Pram_1, out string seriesEnhance))
                {
                    try
                    {
                        string[] fields = seriesEnhance.Split('|');
                        int level = int.Parse(fields[0]);

                        /// Tạo KMagicAttrib
                        KMagicAttrib magicAttrib = new KMagicAttrib()
                        {
                            nAttribType = MAGIC_ATTRIB.magic_seriesenhance,
                            nValue = new int[] { level, 0, 0 }
                        };
                        /// Attach thuộc tính
                        KTAttributesModifier.AttachProperty(magicAttrib, _Player, IsDetack);
                    }
                    catch (Exception ex)
                    {
                        LogManager.WriteLog(LogTypes.Exception, ex.ToString());
                    }
                }
                /// Lấy ra thông tin dòng nhược hóa ngũ hành tương khắc
                if (this._GoodDatas.OtherParams.TryGetValue(ItemPramenter.Pram_2, out string seriesConque))
                {
                    try
                    {
                        string[] fields = seriesConque.Split('|');
                        int level = int.Parse(fields[0]);

                        /// Tạo KMagicAttrib
                        KMagicAttrib magicAttrib = new KMagicAttrib()
                        {
                            nAttribType = MAGIC_ATTRIB.magic_seriesabate,
                            nValue = new int[] { level, 0, 0 }
                        };
                        /// Attach thuộc tính
                        KTAttributesModifier.AttachProperty(magicAttrib, _Player, IsDetack);
                    }
                    catch (Exception ex)
                    {
                        LogManager.WriteLog(LogTypes.Exception, ex.ToString());
                    }
                }
            }
            else
            {
                if (this.TotalBaseAttrib != null)
                {
                    if (this.TotalBaseAttrib.Count > 0)
                    {
                        // Attack Base EFFECT
                        foreach (KMagicAttrib _KMagic in this.TotalBaseAttrib)
                        {
                            KTAttributesModifier.AttachProperty(_KMagic, _Player, IsDetack);
                        }
                    }
                }
            }

            if (TotalGreenProp != null)
            {
                if (this.TotalGreenProp.Count > 0)
                {
                    //Attack Green Prob
                    foreach (KMagicAttrib _KMagic in this.TotalGreenProp)
                    {
                        KTAttributesModifier.AttachProperty(_KMagic, _Player, IsDetack);
                    }
                }
            }

            if (this.TotalEnhance != null)
            {
                if (this.TotalEnhance.Count > 0)
                {
                    // Attack Enhance Efect
                    foreach (KSOCKET _KSocket in this.TotalEnhance)
                    {
                        if (_KSocket.bActive)
                        {
                            KTAttributesModifier.AttachProperty(_KSocket.sMagicAttrib, _Player, IsDetack);
                        }
                    }
                }
            }
            if (this.TotalHiddenProp != null)
            {
                if (this.TotalHiddenProp.Count > 0)
                {
                    // Attack Hidden Prob
                    foreach (KSOCKET _KSocket in this.TotalHiddenProp)
                    {
                        if (_KSocket.bActive)
                        {
                            KTAttributesModifier.AttachProperty(_KSocket.sMagicAttrib, _Player, IsDetack);
                        }
                    }
                }
            }



            //Set detack = pha khác của attack
            this.IsAttack = !IsDetack;
        }

        /// <summary>
        /// Kích hoạt thuộc tính cưỡi ngựa
        /// </summary>
        /// <param name="player"></param>
        /// <param name="isDetach"></param>
        public void AttachRiderEffect(KPlayer player, bool isDetach)
        {
            if (this.TotalRiderProp != null)
            {
                if (this.TotalRiderProp.Count > 0)
                {
                    foreach (KMagicAttrib _KMagic in this.TotalRiderProp)
                    {
                        KTAttributesModifier.AttachProperty(_KMagic, player, isDetach);
                    }
                }
            }
        }

        /// <summary>
        /// Hàm này viết ra phục vụ cho nhiệm vụ
        /// </summary>
        /// <returns></returns>
        public bool IsExisSymboy(int SymboyID, int RequestValue)
        {

            if (TotalGreenProp != null)
            {
                var FindGreenProb = TotalGreenProp.Where(x => (int)x.nAttribType == SymboyID).FirstOrDefault();

                if (FindGreenProb != null)
                {
                    if (FindGreenProb.nValue[0] >= RequestValue)
                    {
                        return true;
                    }
                }
            }

            if (TotalHiddenProp != null)
            {
                // Tìm tiếp
                var FindHiddenProb = TotalHiddenProp.Where(x => x.sMagicAttrib.SymboyID == SymboyID).FirstOrDefault();

                if (FindHiddenProb != null)
                {
                    if (FindHiddenProb.sMagicAttrib.nValue[0] >= RequestValue)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// DONE
        /// </summary>
        public void InitTotalEnhance()
        {
            this.Forge_Level = _GoodDatas.Forge_level;

            List<KSOCKET> TotalEnhance = new List<KSOCKET>();

            List<ENH> ListEnhance = this._ItemData.ListEnhance;

            if (ListEnhance.Count > 0)
            {
                foreach (ENH _Ech in ListEnhance)
                {
                    KSOCKET _Sock = new KSOCKET();

                    _Sock.Index = _Ech.Index;

                    KMagicAttrib Magic = new KMagicAttrib();

                    if (Magic.Init(_Ech.EnhMAName, _Ech.EnhMAPA1Min, _Ech.EnhMAPA2Min, _Ech.EnhMAPA3Min))
                    {
                        _Sock.sMagicAttrib = Magic;
                        if (this.Forge_Level >= _Ech.EnhTimes)
                        {
                            //if (_Ech.EnhMAName == "active_all_ornament")
                            //{
                            //    active_all_ornament = true;
                            //}
                            //if (_Ech.EnhMAName == "active_suit")
                            //{
                            //    active_suit = true;
                            //}

                            /// Nếu là dòng ngũ hành tương khắc hệ gì đó
                            if (_Ech.EnhMAName == "damage_series_resist")
                            {
                                Magic.nValue[2] = GetResValue(this._GoodDatas.Series);
                            }
                            /// Nếu là dòng tỷ lệ bỏ qua kháng
                            else if (_Ech.EnhMAName == "ignoreresist_p")
                            {
                                Magic.nValue[2] = this._GoodDatas.Series;
                            }

                            _Sock.bActive = true;
                        }
                        else
                        {
                            _Sock.bActive = false;
                        }

                        TotalEnhance.Add(_Sock);
                    }
                    else
                    {
                        throw new System.ArgumentException("Symbol not found : " + _Ech.EnhMAName);
                    }
                }
            }

            this.TotalEnhance = TotalEnhance;
        }

        public void Update(KPlayer _Player, bool IsActiveOrnament = false, bool IsActiveAllSuite = false)
        {
            int ACTIVE = 0;

            int PlayerSeri = (int)_Player.m_Series;

            int ItemSeri = this._GoodDatas.Series;

            // Nếu nhân vật và vật phẩm có quan hệ tương sinh thì + 1
            if (KTGlobal.g_IsAccrue(PlayerSeri, ItemSeri))
            {
                ACTIVE++;
            }

            int nPlace = ItemManager.g_anEquipPos[_ItemData.DetailType];

            // Nếu đang đeo là nhẫn thì check xem nó là nhẫn 1 hay nhẫn 2
            if (nPlace == (int)KE_EQUIP_POSITION.emEQUIPPOS_RING)
            {
                if (this.IsSubRing)
                {
                    // Nếu đây là nhẫn trên thì swap nplace
                    nPlace = (int)KE_EQUIP_POSITION.emEQUIPPOS_RING_2;
                }
            }

            /// Nếu không có dòng ẩn
            if (!ItemManager.g_anEquipActive.TryGetValue((KE_EQUIP_POSITION)nPlace, out ActiveByItem activeCheck))
            {
                return;
            }

            int Postion1 = activeCheck.Pos1;

            int Postion2 = activeCheck.Pos2;

            KItem _Item1 = _Player._KPlayerEquipBody.GetItemByPostion(Postion1);

            if (_Item1 != null)
            {
                // Nếu vật phẩm và vật phẩm thứ 1 yêu cầu kích hoạt có quan hệ tương sinh thì +1
                if (KTGlobal.g_IsAccrue(_Item1._GoodDatas.Series, ItemSeri))
                {
                    ACTIVE++;
                }
            }

            KItem _Item2 = _Player._KPlayerEquipBody.GetItemByPostion(Postion2);
            if (_Item2 != null)
            {
                // Nếu vật phẩm và vật phẩm thứ 2 yêu cầu kích hoạt có quan hệ tương sinh thì +1
                if (KTGlobal.g_IsAccrue(_Item2._GoodDatas.Series, ItemSeri))
                {
                    ACTIVE++;
                }
            }

            // DETACK ALL SOCKET
            if (this.TotalHiddenProp != null)
            {
                if (this.TotalHiddenProp.Count > 0)
                {
                    foreach (KSOCKET _Socket in this.TotalHiddenProp)
                    {
                        _Socket.bActive = false;
                    }
                }
            }

            if (ACTIVE >= 1)
            {
                if (this.TotalHiddenProp != null)
                {
                    if (this.TotalHiddenProp.Count > 0)
                    {
                        this.TotalHiddenProp[0].bActive = true;
                    }
                }
            }

            if (ACTIVE >= 2)
            {
                if (this.TotalHiddenProp != null)
                {
                    if (this.TotalHiddenProp.Count > 1)
                    {
                        this.TotalHiddenProp[1].bActive = true;
                    }
                }
            }
            if (ACTIVE >= 3)
            {
                if (this.TotalHiddenProp != null)
                {
                    if (this.TotalHiddenProp.Count > 2)
                    {
                        this.TotalHiddenProp[2].bActive = true;
                    }
                }
            }

            if (IsActiveOrnament)
            {
                if (ItemManager.KD_ISORNAMENT(this._ItemData.DetailType))
                {
                    if (this.TotalHiddenProp != null)
                    {
                        if (this.TotalHiddenProp.Count > 0)
                        {
                            this.TotalHiddenProp[0].bActive = true;
                        }

                        if (this.TotalHiddenProp.Count > 1)
                        {
                            this.TotalHiddenProp[1].bActive = true;
                        }
                        if (this.TotalHiddenProp.Count > 2)
                        {
                            this.TotalHiddenProp[2].bActive = true;
                        }
                    }
                }
            }

            if (IsActiveAllSuite)
            {
                if (this.TotalHiddenProp != null)
                {
                    if (this.TotalHiddenProp.Count > 0)
                    {
                        this.TotalHiddenProp[0].bActive = true;
                    }

                    if (this.TotalHiddenProp.Count > 1)
                    {
                        this.TotalHiddenProp[1].bActive = true;
                    }
                    if (this.TotalHiddenProp.Count > 2)
                    {
                        this.TotalHiddenProp[2].bActive = true;
                    }
                }
            }
        }

        /// <summary>
        /// Khởi tạo thuộc tính đã mã hóa trong database
        /// </summary>
        /// <exception cref="System.ArgumentException"></exception>
        public void InitHiddenProbs()
        {
            // Nếu không có thuộc tính thì chim cút
            if (string.IsNullOrEmpty(_GoodDatas.Props) || _GoodDatas.Props.Contains("ERORR"))
            {
                return;
            }
            byte[] Base64Decode = Convert.FromBase64String(_GoodDatas.Props);

            // Giải mã toàn bộ chỉ số đã ghi trong gamedb
            ItemDataByteCode _ItemBuild = DataHelper.BytesToObject<ItemDataByteCode>(Base64Decode, 0, Base64Decode.Length);

            if (_ItemBuild != null)
            {
                if (_ItemBuild.HiddenProbsCount > 0)
                {
                    // khởi tạo socket để lưu trữ
                    List<KSOCKET> TotalHiddenEffect = new List<KSOCKET>();

                    // Loại bỏ hoàn toàn việc lấy thông tin từ TEMP vì dòng này được tạo ra ngẫu nhiên chứ không cố định theo TEMP
                    //List<PropMagic> List = _ItemData.HiddenProp.OrderBy(x => x.Index).ToList();
                    int Index = 0;

                    foreach (KMagicInfo KmagicInfo in _ItemBuild.HiddenProbs)
                    {
                        // Tạo mới 1 MAGICATRI BUTE để lưu trữ thuộc tính đồ
                        KMagicAttrib Atribute = new KMagicAttrib();
                        // Tạo mới 1 socket để xử lý việc kích hoạt đồ

                        KSOCKET _Sock = new KSOCKET();

                        int MagicType = KmagicInfo.nAttribType;

                        int Value1 = 0;
                        int Value2 = 0;
                        int Value3 = 0;

                        if (KmagicInfo.nAttribType == 1801)
                        {
                            MagicType = KmagicInfo.nAttribType;
                            Value1 = KmagicInfo.Value_1;
                            Value2 = KmagicInfo.Value_2;
                            Value3 = KmagicInfo.Value_3;
                        }
                        else
                        {
                            // lấy ra cấp độ dịch cuaur magic level
                            int LevelMagic = KmagicInfo.nLevel + this._GoodDatas.Forge_level;

                            // Lấy ra chỉ số ban đầu để tính khoảng dịch theo %
                            MagicAttribLevel FindOrginalValue = ItemManager.TotalMagicAttribLevel.Where(x => x.MAGIC_ID == MagicType && x.Level == _ItemData.Level).FirstOrDefault();

                            // Find ra chỉ số magic sau khi đã cường hóa
                            MagicAttribLevel FindMagic = ItemManager.TotalMagicAttribLevel.Where(x => x.MAGIC_ID == MagicType && x.Level == LevelMagic).FirstOrDefault();

                            // Nếu mà có OP null thì toác luôn
                            if (FindOrginalValue == null || FindMagic == null)
                            {
                                LogManager.WriteLog(LogTypes.Error, "MagicAttribLevel NOT FOUND :" + MagicType);
                                //  throw new System.ArgumentException("Magic not found : " + MagicType);
                            }

                            // Tính toán lại khoảng dịch cho OP1
                            if (KmagicInfo.Value_1 != -1)
                            {
                                // Tính toán ra tỉ lệ % khoảng dịch
                                int Percent = RecaculationPercent(FindOrginalValue.MA1Min, FindOrginalValue.MA1Max, KmagicInfo.Value_1);

                                // Nếu có tỉ lệ %
                                if (Percent > 0)
                                {
                                    int AddValue = ((FindMagic.MA1Max - FindMagic.MA1Min) * Percent) / 100;

                                    // Set giá trị cho Value1
                                    Value1 = FindMagic.MA1Min + AddValue;
                                }
                                else
                                {
                                    // Set giá trị cho Value1
                                    Value1 = FindMagic.MA1Min;
                                }
                            }

                            // Tính toán lại khoảng dịch cho OP 2
                            if (KmagicInfo.Value_2 != -1)
                            {
                                int Percent = RecaculationPercent(FindOrginalValue.MA2Min, FindOrginalValue.MA2Max, KmagicInfo.Value_2);

                                if (Percent > 0)
                                {
                                    int AddValue = ((FindMagic.MA2Max - FindMagic.MA2Min) * Percent) / 100;
                                    // Set giá trị cho Value2
                                    Value2 = FindMagic.MA2Min + AddValue;
                                }
                                else
                                {
                                    // Set giá trị cho Value2
                                    Value2 = FindMagic.MA2Min;
                                }
                            }
                            if (KmagicInfo.Value_3 != -1)
                            {
                                int Percent = RecaculationPercent(FindOrginalValue.MA3Min, FindOrginalValue.MA3Max, KmagicInfo.Value_3);

                                if (Percent > 0)
                                {
                                    int AddValue = ((FindMagic.MA3Max - FindMagic.MA3Min) * Percent) / 100;
                                    // Set giá trị cho Value3
                                    Value3 = FindMagic.MA3Min + AddValue;
                                }
                                else
                                {
                                    // Set giá trị cho Value3
                                    Value3 = FindMagic.MA3Min;
                                }
                            }
                        }

                        // Khởi tạo thử thuộc tính này
                        if (Atribute.Init(MagicType, Value1, Value2, Value3))
                        {
                            // Set value cho socket
                            _Sock.Index = Index;
                            _Sock.sMagicAttrib = Atribute;
                            //Set là chưa kích hoạt cho vật phẩm này
                            _Sock.bActive = false;

                            // Thực hiện add vào mảng
                            TotalHiddenEffect.Add(_Sock);
                        }
                        else
                        {
                            LogManager.WriteLog(LogTypes.Error, "MagicAttribLevel NOT FOUND 1 :" + MagicType);
                        }
                        Index++;
                    }
                    this.TotalHiddenProp = TotalHiddenEffect;
                }
            }
        }

        /// <summary>
        /// Function tính toán lại chỉ số dịch ở magic atribute
        /// </summary>
        /// <param name="MinValue"></param>
        /// <param name="MaxValue"></param>
        /// <param name="CurenValue"></param>
        /// <returns></returns>
        public int RecaculationPercent(int MinValue, int MaxValue, int CurenValue)
        {
            if (MaxValue == MinValue)
            {
                return 0;
            }
            return (CurenValue * 100) / (MaxValue - MinValue);
        }

        /// <summary>
        /// Lấy ra độ bền tối đa của 1 vật phẩm
        /// </summary>
        /// <returns></returns>
        public int GetDurability()
        {
            try
            {
                byte[] Base64Decode = Convert.FromBase64String(_GoodDatas.Props);

                ItemDataByteCode _ItemBuild = DataHelper.BytesToObject<ItemDataByteCode>(Base64Decode, 0, Base64Decode.Length);

                List<KMagicInfo> _BasicProb = _ItemBuild.BasicProp;

                var FindValue = _BasicProb.Where(x => x.nAttribType == 2).FirstOrDefault();
                if (FindValue != null)
                {
                    return FindValue.Value_1;
                }
            }
            catch
            {
                return 100;
            }

            return -1;
        }

        /// <summary>
        /// Khởi tạo thuộc tính Base của Item
        /// Bao gồm Base Atrib + Green Atrib
        /// Giải mã thuộc tính từ trong DB
        /// </summary>
        public void InitBaseAttribParse()
        {
            // Nếu không có thuộc tính thì chim cút
            if (string.IsNullOrEmpty(_GoodDatas.Props) || _GoodDatas.Props.Contains("ERORR"))
            {
                return;
            }
            /// Byte code chuỗi mã hóa trong gameDB
            byte[] Base64Decode = Convert.FromBase64String(_GoodDatas.Props);

            ItemDataByteCode _ItemBuild = DataHelper.BytesToObject<ItemDataByteCode>(Base64Decode, 0, Base64Decode.Length);

            if (_ItemBuild != null)
            {
                if (_ItemBuild.BasicPropCount > 0)
                {
                    List<KMagicAttrib> TotalBaseAttrib = new List<KMagicAttrib>();

                    // Ở đây mình sét chết value luôn vì giá trị BasicProp không thay đổi khi có tác động vào vật phẩm
                    foreach (KMagicInfo KmagicInfo in _ItemBuild.BasicProp)
                    {
                        KMagicAttrib KMagicAttrib = new KMagicAttrib();

                        int MagicType = KmagicInfo.nAttribType;

                        int Value1 = 0;
                        int Value2 = 0;
                        int Value3 = 0;

                        if (KmagicInfo.Value_1 != -1)
                        {
                            Value1 = KmagicInfo.Value_1;
                        }
                        if (KmagicInfo.Value_2 != -1)
                        {
                            Value2 = KmagicInfo.Value_2;
                        }
                        if (KmagicInfo.Value_3 != -1)
                        {
                            Value3 = KmagicInfo.Value_3;
                        }

                        // Khởi tạo BaseAtrib
                        if (KMagicAttrib.Init(MagicType, Value1, Value2, Value3))
                        {
                            TotalBaseAttrib.Add(KMagicAttrib);
                        }
                        else
                        {
                            LogManager.WriteLog(LogTypes.EquipBody, "[" + this._GoodDatas.Id + "][" + this._GoodDatas.ItemName + "]Symbol not found : " + KmagicInfo.nAttribType.ToString());
                        }
                    }

                    this.TotalBaseAttrib = TotalBaseAttrib;
                }

                //Thực hiện fill hết các dòng xanh vào thường thì nó sẽ active luôn
                // Các dòng này có thay đổi dựa khi có tác động vào vật phẩm như cường hóa vv

                if (_ItemBuild.GreenPropCount > 0)
                {
                    // Khởi tạo 1 chuỗi KMagicAttrib để lưu trữ các thuộc tính
                    List<KMagicAttrib> TotalGreenProb = new List<KMagicAttrib>();

                    //Loop toàn bộ thuộc tính đã lưu trữ tỏng DB
                    foreach (KMagicInfo KmagicInfo in _ItemBuild.GreenProp)
                    {
                        KMagicAttrib _KMagicAttrib = new KMagicAttrib();
                        // Level cơ bản của đồ mới khai sinh ra + với cấp độ cường hóa sẽ ra MAGICLEVEL cuối cùng
                        int MagicType = KmagicInfo.nAttribType;

                        // Nếu trên đồ này có các dòng xanh + thẳng chỉ số cho nhân vật thì ưu tiên attack vào người nó trước để tạo điều kiện mặc các đồ sau
                        if (MagicType == 100 || MagicType == 101 || MagicType == 102 || MagicType == 103)
                        {
                            // Tăng thứ tự ưu tiên kích hoạt của đồ này lên
                            this.InitLevel++;
                        }

                        int Value1 = 0;
                        int Value2 = 0;
                        int Value3 = 0;

                        // nếu như là bound skill point thì không cần phải  tính toán khoảng nhảy
                        if (KmagicInfo.nAttribType == 1801)
                        {
                            MagicType = KmagicInfo.nAttribType;
                            Value1 = KmagicInfo.Value_1;
                            Value2 = KmagicInfo.Value_2;
                            Value3 = KmagicInfo.Value_3;
                        }
                        else
                        {
                            // Đoạn này ssexalafm thế này

                            // Lấy ra cấp độ cuối cùng
                            int LevelMagic = KmagicInfo.nLevel + this._GoodDatas.Forge_level;

                            // Lấy ra chỉ số ban đầu để tính khoảng dịch theo %
                            MagicAttribLevel FindOrginalValue = ItemManager.TotalMagicAttribLevel.Where(x => x.MAGIC_ID == MagicType && x.Level == _ItemData.Level).FirstOrDefault();

                            // Find ra chỉ số magic sau khi đã cường hóa
                            MagicAttribLevel FindMagic = ItemManager.TotalMagicAttribLevel.Where(x => x.MAGIC_ID == MagicType && x.Level == LevelMagic).FirstOrDefault();

                            // Nếu mà có OP null thì toác luôn
                            if (FindOrginalValue == null || FindMagic == null)
                            {
                                throw new System.ArgumentException("Magic not found : " + MagicType);
                            }

                            // Tính toán lại khoảng dịch cho OP1
                            if (KmagicInfo.Value_1 != -1)
                            {
                                // Tính toán ra tỉ lệ % khoảng dịch
                                int Percent = RecaculationPercent(FindOrginalValue.MA1Min, FindOrginalValue.MA1Max, KmagicInfo.Value_1);

                                // Nếu có tỉ lệ %
                                if (Percent > 0)
                                {
                                    int AddValue = ((FindMagic.MA1Max - FindMagic.MA1Min) * Percent) / 100;

                                    // Set giá trị cho Value1
                                    Value1 = FindMagic.MA1Min + AddValue;
                                }
                                else
                                {
                                    // Set giá trị cho Value1
                                    Value1 = FindMagic.MA1Min;
                                }
                            }

                            // Tính toán lại khoảng dịch cho OP 2
                            if (KmagicInfo.Value_2 != -1)
                            {
                                int Percent = RecaculationPercent(FindOrginalValue.MA2Min, FindOrginalValue.MA2Max, KmagicInfo.Value_2);

                                if (Percent > 0)
                                {
                                    int AddValue = ((FindMagic.MA2Max - FindMagic.MA2Min) * Percent) / 100;
                                    // Set giá trị cho Value2
                                    Value2 = FindMagic.MA2Min + AddValue;
                                }
                                else
                                {
                                    // Set giá trị cho Value2
                                    Value2 = FindMagic.MA2Min;
                                }
                            }
                            if (KmagicInfo.Value_3 != -1)
                            {
                                int Percent = RecaculationPercent(FindOrginalValue.MA3Min, FindOrginalValue.MA3Max, KmagicInfo.Value_3);

                                if (Percent > 0)
                                {
                                    int AddValue = ((FindMagic.MA3Max - FindMagic.MA3Min) * Percent) / 100;
                                    // Set giá trị cho Value3
                                    Value3 = FindMagic.MA3Min + AddValue;
                                }
                                else
                                {
                                    // Set giá trị cho Value3
                                    Value3 = FindMagic.MA3Min;
                                }
                            }
                        }
                        // Def
                        if (_KMagicAttrib.Init(MagicType, Value1, Value2, Value3))
                        {
                            TotalGreenProb.Add(_KMagicAttrib);
                        }
                        else
                        {
                            throw new System.ArgumentException("Symbol not found : " + KmagicInfo.nAttribType);
                        }
                    }

                    // Set ngoài vồng FOREACH
                    this.TotalGreenProp = TotalGreenProb;
                }
            }
        }

        private static int GetResValue(int series)
        {
            int resValue = 0;

            switch (series)
            {
                case 1:
                    resValue = 4;
                    break;

                case 2:
                    resValue = 3;
                    break;

                case 3:
                    resValue = 1;
                    break;

                case 4:
                    resValue = 0;
                    break;

                case 5:
                    resValue = 2;
                    break;
            }

            return resValue;
        }

        public void InitRequestItem()
        {
            List<KREQUIRE_ATTR> TotalRequest = new List<KREQUIRE_ATTR>();

            if (_ItemData.ListReqProp.Count > 0)
            {
                foreach (ReqProp req in _ItemData.ListReqProp)
                {
                    KREQUIRE_ATTR _Att = new KREQUIRE_ATTR();
                    _Att.eRequire = (KE_ITEM_REQUIREMENT)req.ReqPropType;
                    _Att.nValue = req.ReqPropValue;
                    TotalRequest.Add(_Att);
                }
                this.TotalRequest = TotalRequest;
            }
        }

        /// <summary>
        /// Tạm thời disable ngũ hành ấn
        /// Bao gồm Base Atrib + Green Atrib
        /// </summary>

        public void AbradeInDeath(int nPercent, KPlayer client)
        {
            // nếu độ bên fhiện tịa đã nhỏ hơn không thì bỏ qua
            if (this._GoodDatas.Strong <= 0)
                return;

            int nSub = this._GoodDatas.Strong * nPercent / 100;

            if (nSub > 0)
            {
                nSub = 1;

                this._GoodDatas.Strong = this._GoodDatas.Strong - 1;

                Dictionary<UPDATEITEM, object> TotalUpdate = new Dictionary<UPDATEITEM, object>();

                TotalUpdate.Add(UPDATEITEM.ROLEID, client.RoleID);
                TotalUpdate.Add(UPDATEITEM.ITEMDBID, this._GoodDatas.Id);
                TotalUpdate.Add(UPDATEITEM.STRONG, this._GoodDatas.Strong);

                client.GoodsData.Update(this._GoodDatas, TotalUpdate, false, true, "");

                KTPlayerManager.ShowNotification(client, "Độ bền của [" + ItemManager.GetNameItem(this._GoodDatas) + "] giảm đi 1");

                // Nếu độ bền mà khác không thì reload lại các chỉ số
                if (this._GoodDatas.Strong <= 0)
                {
                    client.GetPlayEquipBody().ClearAllEffecEquipBody();

                    client.GetPlayEquipBody().AttackAllEquipBody();
                }
            }
        }

        public void Abrade(int Value, KPlayer client)
        {
            // nếu độ bên fhiện tịa đã nhỏ hơn không thì bỏ qua
            if (this._GoodDatas.Strong <= 0)
                return;

            this._GoodDatas.Strong = this._GoodDatas.Strong - Value;

            Dictionary<UPDATEITEM, object> TotalUpdate = new Dictionary<UPDATEITEM, object>();

            TotalUpdate.Add(UPDATEITEM.ROLEID, client.RoleID);
            TotalUpdate.Add(UPDATEITEM.ITEMDBID, this._GoodDatas.Id);
            TotalUpdate.Add(UPDATEITEM.STRONG, this._GoodDatas.Strong);

            client.GoodsData.Update(this._GoodDatas, TotalUpdate, false, true, "");

            KTPlayerManager.ShowNotification(client, "Độ bền của [" + ItemManager.GetNameItem(this._GoodDatas) + "] giảm đi 1");

            // Nếu độ bền mà khác không thì reload lại các chỉ số
            if (this._GoodDatas.Strong <= 0)
            {
                client.GetPlayEquipBody().ClearAllEffecEquipBody();

                client.GetPlayEquipBody().AttackAllEquipBody();
            }
        }

        /// <summary>
        /// Thuộc tính của quan ấn
        /// </summary>
        /// <exception cref="System.ArgumentException"></exception>
        public void InitBaseAttribSignetParse()
        {
            if (string.IsNullOrEmpty(_GoodDatas.Props) || _GoodDatas.Props.Contains("ERORR"))
            {
                return;
            }

            byte[] Base64Decode = Convert.FromBase64String(_GoodDatas.Props);

            //Byte code item ra

            ItemDataByteCode _ItemBuild = DataHelper.BytesToObject<ItemDataByteCode>(Base64Decode, 0, Base64Decode.Length);

            if (_ItemBuild != null)
            {
                if (_ItemBuild.BasicPropCount > 0)
                {
                    List<KMagicAttrib> TotalBaseAttrib = new List<KMagicAttrib>();

                    foreach (KMagicInfo KmagicInfo in _ItemBuild.BasicProp)
                    {
                        KMagicAttrib KMagicAttrib = new KMagicAttrib();

                        int MagicType = KmagicInfo.nAttribType;

                        int Value1 = 0;
                        int Value2 = 0;
                        int Value3 = 0;

                        if (KmagicInfo.Value_1 != -1)
                        {
                            Value1 = KmagicInfo.Value_1;
                        }
                        if (KmagicInfo.Value_2 != -1)
                        {
                            Value2 = KmagicInfo.Value_2;
                        }
                        if (KmagicInfo.Value_3 != -1)
                        {
                            Value3 = KmagicInfo.Value_3;
                        }

                        // Nếu là damge resiget
                        if (MagicType == 775)
                        {
                            Value3 = GetResValue(this._GoodDatas.Series);
                        }
                        else if (MagicType == 223)
                        {
                            Value3 = this._GoodDatas.Series;
                        }

                        if (MagicType == 202)
                        {
                            if (_GoodDatas.OtherParams.TryGetValue(ItemPramenter.Pram_1, out string enhance))
                            {
                                string[] enhancePram = enhance.Split('|');

                                int nLevel = Int32.Parse(enhancePram[0]);

                                if (nLevel > 1)
                                {
                                    Value1 = Value1 + nLevel - 1;
                                }
                            }
                        }

                        if (MagicType == 203)
                        {
                            if (_GoodDatas.OtherParams.TryGetValue(ItemPramenter.Pram_2, out string enhance))
                            {
                                string[] enhancePram = enhance.Split('|');

                                int nLevel = Int32.Parse(enhancePram[0]);

                                if (nLevel > 1)
                                {
                                    Value1 = Value1 + nLevel - 1;
                                }
                            }
                        }

                        if (KMagicAttrib.Init(MagicType, Value1, Value2, Value3))
                        {
                            TotalBaseAttrib.Add(KMagicAttrib);
                        }
                        else
                        {
                            LogManager.WriteLog(LogTypes.EquipBody, "[" + this._GoodDatas.Id + "][" + this._GoodDatas.ItemName + "]Symbol not found : " + KmagicInfo.nAttribType.ToString());
                        }
                    }

                    // DROP HẾT CHỖ NÀY

                    this.TotalBaseAttrib = TotalBaseAttrib;
                }
            }
        }

        public void InitHorseProperty()
        {
            List<RiderProp> RiderProp = this._ItemData.RiderProp;
            List<KMagicAttrib> TotalRiderPropEx = new List<KMagicAttrib>();

            if (RiderProp.Count > 0)
            {
                foreach (RiderProp _Prob in RiderProp)
                {
                    KMagicAttrib Atribute = new KMagicAttrib();

                    Atribute.Init(_Prob.RidePropType, _Prob.RidePropPA1Min, _Prob.RidePropPA2Min, _Prob.RidePropPA3Min);
                    TotalRiderPropEx.Add(Atribute);
                }
            }

            this.TotalRiderProp = TotalRiderPropEx;
        }

        /// <summary>
        /// Đếm tổng số dòng của vật phẩm
        /// </summary>
        /// <returns></returns>
        public int CountLines()
        {
            int Count = 0;

            if (TotalGreenProp != null)
            {
                Count += TotalGreenProp.Count;
            }

            if (TotalHiddenProp != null)
            {
                Count += TotalHiddenProp.Count;
            }

            return Count;
        }

        public KItem(GoodsData goodsData)
        {
            // SetItemData
            _ItemData = ItemManager.GetItemTemplate(goodsData.GoodsID);

            this.ISEQUIP = ItemManager.KD_ISEQUIP(_ItemData.Genre);
            this.IsAttack = false;
            this._GoodDatas = goodsData;
            this.TypeItem = ItemManager.GetItemType(_ItemData.DetailType);
            if (_GoodDatas.Using == 8)
            {
                this.IsSubRing = true;
            }

            if (ISEQUIP)
            {
                //this.active_all_ornament = false;
                //this.active_suit = false;

                switch (TypeItem)
                {
                    case KE_ITEM_EQUIP_DETAILTYPE.equip_meleeweapon:
                    case KE_ITEM_EQUIP_DETAILTYPE.equip_rangeweapon:
                    case KE_ITEM_EQUIP_DETAILTYPE.equip_armor:
                    case KE_ITEM_EQUIP_DETAILTYPE.equip_ring:
                    case KE_ITEM_EQUIP_DETAILTYPE.equip_mask:
                    case KE_ITEM_EQUIP_DETAILTYPE.equip_amulet:
                    case KE_ITEM_EQUIP_DETAILTYPE.equip_boots:
                    case KE_ITEM_EQUIP_DETAILTYPE.equip_belt:
                    case KE_ITEM_EQUIP_DETAILTYPE.equip_helm:
                    case KE_ITEM_EQUIP_DETAILTYPE.equip_ornament:
                    case KE_ITEM_EQUIP_DETAILTYPE.equip_cuff:
                    case KE_ITEM_EQUIP_DETAILTYPE.equip_mantle:
                    case KE_ITEM_EQUIP_DETAILTYPE.equip_chop:
                    case KE_ITEM_EQUIP_DETAILTYPE.equip_pendant:
                        {
                            InitBaseAttribParse();
                            InitRequestItem();
                            InitTotalEnhance();
                            InitHiddenProbs();
                            break;
                        }
                    case KE_ITEM_EQUIP_DETAILTYPE.equip_book:
                        {
                            InitRequestItem();
                            //  InitBookProperty();

                            break;
                        }
                    case KE_ITEM_EQUIP_DETAILTYPE.equip_horse:
                        {
                            InitBaseAttribParse();
                            InitRequestItem();
                            InitHorseProperty();

                            break;
                        }
                    case KE_ITEM_EQUIP_DETAILTYPE.equip_signet:
                        {
                            InitRequestItem();
                            InitBaseAttribSignetParse();

                            break;
                        }
                }
            }
            else if (ItemManager.KD_ISPETEQUIP(_ItemData.Genre)) // NẾU Không phải vật phẩm để mặc thì GS ko quan tâm. Client chỉ cần lấy để hiển thị ra các dòng DESC. Các vật phẩm kích hoạt sẽ sử dụng lua để kích hoạt script
            {
                InitBaseAttribParse();
                InitRequestItem();
            }
        }
    }
}