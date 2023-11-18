using GameServer.KiemThe.Utilities;
using GameServer.Logic;
using static GameServer.KiemThe.Utilities.PropertyDefine;

namespace GameServer.KiemThe.Entities
{
    public class KREQUIRE_ATTR
    {
        public KE_ITEM_REQUIREMENT eRequire { get; set; }                         // ÐèÇóÀàÐÍ
        public int nValue { get; set; }                             // ²ÎÊýÖµ
    };

    /// <summary>
    /// Loại thuộc tính 3 giá trị
    /// </summary>
    public class KMagicAttrib
    {
        /// <summary>
        /// Kiểu thuộc tính
        /// </summary>
        public MAGIC_ATTRIB nAttribType { get; set; }

        /// <summary>
        /// Danh sách các giá trị
        /// </summary>
        public int[] nValue { get; set; } = new int[3];

        public string GetSymboyName()
        {
            Property _Property = PropertyDefine.PropertiesByID[(int)this.nAttribType];

            return _Property.SymbolName;
        }


        public int SymboyID
        {
            get { return (int)this.nAttribType; }
        }
        public bool Init(string Name, int Value0, int Value1, int Value2)
        {
            if (PropertyDefine.PropertiesBySymbolName.ContainsKey(Name.Trim()))
            {
                int MagicID = PropertyDefine.PropertiesBySymbolName[Name.Trim()].ID;

                this.nAttribType = (MAGIC_ATTRIB)MagicID;

                this.nValue[0] = Value0;
                this.nValue[1] = Value1;
                this.nValue[2] = Value2;

                return true;
            }
            else
            {
                return false;
            }
        }


        public bool Init(int SymboyID, int Value0, int Value1, int Value2)
        {

            if (PropertiesByID.TryGetValue(SymboyID, out Property Value))
            {
                this.nAttribType = (MAGIC_ATTRIB)SymboyID;

                this.nValue[0] = Value0;
                this.nValue[1] = Value1;
                this.nValue[2] = Value2;

                return true;
            }
            else
            {
                return false;
            }
        }


        public KMagicAttrib(MAGIC_ATTRIB Type)
        {
            this.nAttribType = Type;
        }

        public KMagicAttrib()
        {
        }

        public KMagicAttrib(int Value0, int Value1, int Value2, MAGIC_ATTRIB Type)
        {
            this.nAttribType = Type;
            this.nValue[0] = Value0;
            this.nValue[1] = Value1;
            this.nValue[2] = Value2;
        }

        /// <summary>
        /// Loại thuộc tính
        /// 0: Mặc định
        /// 1: Thêm
        /// 2: Nhân
        /// 3: Thay thế
        /// </summary>
        public int[] nType { get; set; } = new int[] { 0, 0, 0 };

        public static KMagicAttrib operator +(KMagicAttrib attrib1, KMagicAttrib attrib2)
        {
            if (attrib1 == null || attrib2 == null)
            {
                return null;
            }
            else if (attrib1.nAttribType != attrib2.nAttribType)
            {
                return null;
            }
            return new KMagicAttrib()
            {
                nAttribType = attrib1.nAttribType,
                nValue = new int[]
                {
                    attrib1.nValue[0] + attrib2.nValue[0],
                    attrib1.nValue[1] + attrib2.nValue[1],
                    attrib1.nValue[2] + attrib2.nValue[2],
                },
            };
        }

        public static KMagicAttrib operator -(KMagicAttrib attrib1, KMagicAttrib attrib2)
        {
            if (attrib1 == null || attrib2 == null)
            {
                return null;
            }
            else if (attrib1.nAttribType != attrib2.nAttribType)
            {
                return null;
            }
            return new KMagicAttrib()
            {
                nAttribType = attrib1.nAttribType,
                nValue = new int[]
                {
                    attrib1.nValue[0] - attrib2.nValue[0],
                    attrib1.nValue[1] - attrib2.nValue[1],
                    attrib1.nValue[2] - attrib2.nValue[2],
                },
            };
        }

        public static KMagicAttrib operator *(KMagicAttrib attrib1, KMagicAttrib attrib2)
        {
            if (attrib1 == null || attrib2 == null)
            {
                return null;
            }
            else if (attrib1.nAttribType != attrib2.nAttribType)
            {
                return null;
            }
            return new KMagicAttrib()
            {
                nAttribType = attrib1.nAttribType,
                nValue = new int[]
                {
                    attrib1.nValue[0] * attrib2.nValue[0],
                    attrib1.nValue[1] * attrib2.nValue[1],
                    attrib1.nValue[2] * attrib2.nValue[2],
                },
            };
        }

        public static KMagicAttrib operator *(KMagicAttrib attrib1, int number)
        {
            if (attrib1 == null)
            {
                return null;
            }

            return new KMagicAttrib()
            {
                nAttribType = attrib1.nAttribType,
                nValue = new int[]
                {
                    attrib1.nValue[0] * number,
                    attrib1.nValue[1] * number,
                    attrib1.nValue[2] * number,
                },
            };
        }

        public static KMagicAttrib operator /(KMagicAttrib attrib1, KMagicAttrib attrib2)
        {
            if (attrib1 == null || attrib2 == null)
            {
                return null;
            }
            else if (attrib1.nAttribType != attrib2.nAttribType)
            {
                return null;
            }
            return new KMagicAttrib()
            {
                nAttribType = attrib1.nAttribType,
                nValue = new int[]
                {
                    attrib1.nValue[0] / attrib2.nValue[0],
                    attrib1.nValue[1] / attrib2.nValue[1],
                    attrib1.nValue[2] / attrib2.nValue[2],
                },
            };
        }

        public static KMagicAttrib operator /(KMagicAttrib attrib1, int number)
        {
            if (attrib1 == null)
            {
                return null;
            }
            return new KMagicAttrib()
            {
                nAttribType = attrib1.nAttribType,
                nValue = new int[]
                {
                    attrib1.nValue[0] / number,
                    attrib1.nValue[1] / number,
                    attrib1.nValue[2] / number,
                },
            };
        }

        public static bool operator ==(KMagicAttrib attrib1, KMagicAttrib attrib2)
        {
            if (attrib1 is null && attrib2 is null)
            {
                return true;
            }
            else if (attrib1 is null || attrib2 is null)
            {
                return false;
            }
            else if (attrib1.nAttribType != attrib2.nAttribType)
            {
                return false;
            }
            return attrib1.nValue[0] == attrib2.nValue[0] && attrib1.nValue[1] == attrib2.nValue[1] && attrib1.nValue[2] == attrib2.nValue[2];
        }

        public static bool operator !=(KMagicAttrib attrib1, KMagicAttrib attrib2)
        {
            if ((attrib1 is null && !(attrib2 is null)) || (!(attrib1 is null) && attrib2 is null))
            {
                return true;
            }
            else if (attrib1 is null || attrib2 is null)
            {
                return false;
            }
            else if (attrib1.nAttribType != attrib2.nAttribType)
            {
                return false;
            }
            return attrib1.nValue[0] != attrib2.nValue[0] || attrib1.nValue[1] != attrib2.nValue[1] || attrib1.nValue[2] != attrib2.nValue[2];
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }
            else if (!(obj is KMagicAttrib))
            {
                return false;
            }
            return this == (KMagicAttrib)obj;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Chuyển đối tượng về dạng String
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (this.nValue == null)
            {
                return "{N/A}";
            }
            else
            {
                return "{" + string.Format("{0}, {1}, {2}", this.nValue[0], this.nValue[1], this.nValue[2]) + "}";
            }
        }

        /// <summary>
        /// Tạo bản sao của đối tượng
        /// </summary>
        /// <returns></returns>
        public KMagicAttrib Clone()
        {
            return new KMagicAttrib()
            {
                nAttribType = this.nAttribType,
                nType = new int[]
                {
                    this.nType[0], this.nType[1], this.nType[2],
                },
                nValue = new int[]
                {
                    this.nValue[0], this.nValue[1], this.nValue[2],
                }
            };
        }
    }

    /// <summary>
    /// Mỗi khoảng thời gian bỏ qua nử giây tấn công
    /// </summary>
    public class KNPC_IGNOREATTACK
    {
        public int nIgnoreAttackBase { get; set; }
        public int nValueModify { get; set; }
        public int nIgnoreAttack { get; set; }
    }

    /// <summary>
    /// Tỷ lệ bỏ qua kháng
    /// </summary>
    public class KNPC_IGNORERESIST
    {
        /// <summary>
        /// Tỷ lệ bỏ qua Min
        /// </summary>
        public int nIgnoreResistPMin { get; set; }

        /// <summary>
        /// Tỷ lệ bỏ qua Max
        /// </summary>
        public int nIgnoreResistPMax { get; set; }
    };

    /// <summary>
    /// Sát thương gây ra
    /// </summary>
    public class NPC_CALC_DAMAGE_PARAM
    {
        /// <summary>
        /// Tổng số sát thương
        /// </summary>
        public int nTotalDamage { get; set; }

        /// <summary>
        /// Ngũ hành tương khắc
        /// </summary>
        public int nSeriesConquarRes { get; set; }
    };

    /// <summary>
    /// Sát thương căn cứ khoảng dịch chuyển tương ứng
    /// </summary>
    public class KNPC_RDCLIFEWITHDIS
    {
        /// <summary>
        /// Hệ số nhân
        /// </summary>
        public int nMultiple { get; set; }

        /// <summary>
        /// Khoảng cách tối đa
        /// </summary>
        public int nMaxDis { get; set; }

        /// <summary>
        /// Kỹ năng gây sát thương
        /// </summary>
        public int nDamageSkillId { get; set; }

        /// <summary>
        /// % sát thương cộng thêm
        /// <para>Thuộc tính này sẽ được tính toán ở ngoài, làm INPUT đầu vào duy nhất cho hàm tính sát thương</para>
        /// </summary>
        public int nDamageAddedP { get; set; }

        /// <summary>
        /// Đối tượng gây sát thương
        /// </summary>
        public GameObject nLauncher { get; set; }

        /// <summary>
        /// Vị trí X hiện tại
        /// </summary>
        public int nPrePosX { get; set; }

        /// <summary>
        /// Vị trí Y hiện tại
        /// </summary>
        public int nPrePosY { get; set; }

        /// <summary>
        /// ID kỹ năng
        /// </summary>
        public int nSkillId { get; set; }

        /// <summary>
        /// Cấp độ kỹ năng gây sát thương
        /// </summary>
        public int nSkillLevel { get; set; }
    }
}