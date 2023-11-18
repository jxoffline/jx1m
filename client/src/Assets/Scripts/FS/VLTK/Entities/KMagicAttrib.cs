using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FS.VLTK.Entities
{
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

        /// <summary>
        /// Loại thuộc tính
        /// 0: Mặc định
        /// 1: Thêm
        /// 2: Nhân
        /// 3: Thay thế
        /// </summary>
        public short[] nType { get; set; } = new short[] { 0, 0, 0 };

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
                nType = new short[]
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

    public enum MAGIC_ATTRIB
    {
        magic_item_begin = 0x0000,

        /// <summary>
        /// Né tránh của trang bị
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_armordefense_v,

        /// <summary>
        /// Độ bền trang bị
        /// </summary>
        magic_durability_v,

        /// <summary>
        ///  Không thể phá hủy
        /// </summary>
        magic_indestructible_b,

        magic_normal_begin = 80,

        /// <summary>
        /// Vật công ngoại tối thiểu của trang bị
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_weapondamagemin_v,
        /// <summary>
        /// Vật công ngoại tối đa của trang bị
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_weapondamagemax_v,
        /// <summary>
        /// Vật công nội tối thiểu của trang bị
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_weaponmagicmin_v,
        /// <summary>
        /// Vật công nội tối đa của trang bị
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_weaponmagicmax_v,

        /// <summary>
        /// Sinh lực tối đa
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_lifemax_v,
        /// <summary>
        /// % sinh lực tối đa
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_lifemax_p,
        /// <summary>
        /// Sinh lực hiện tại
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_life_v,
        /// <summary>
        /// Hiệu quả phục hồi sinh lực của kỹ năng
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_lifereplenish_v,
        /// <summary>
        /// Hiệu quả phục hồi sinh lực của kỹ năng
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_fastlifereplenish_v,
        /// <summary>
        /// Nội lực tối đa
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_manamax_v,
        /// <summary>
        /// % nội lực tối đa
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_manamax_p,
        /// <summary>
        /// Nội lực hiện tại
        /// </summary>
        magic_mana_v,
        /// <summary>
        /// Hiệu quả phục hồi nội lực của kỹ năng
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_manareplenish_v,
        /// <summary>
        /// Hiệu quả phục hồi nội lực của kỹ năng
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_fastmanareplenish_v,
        /// <summary>
        /// Thể lực tối đa
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_staminamax_v,
        /// <summary>
        /// % thể lực tói đa
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_staminamax_p,
        /// <summary>
        /// Thể lực hiện tại
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_stamina_v,
        /// <summary>
        /// Hiệu quả phục hồi thể lực của kỹ năng
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_staminareplenish_v,
        /// <summary>
        /// Hiệu quả phục hồi thể lực của kỹ năng
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_faststaminareplenish_v,

        /// <summary>
        /// Sức
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_strength_v,
        /// <summary>
        /// Thân
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_dexterity_v,
        /// <summary>
        /// Ngoại
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_vitality_v,
        /// <summary>
        /// Nội
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_energy_v,

        /// <summary>
        /// Giảm thời gian trúng độc
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_poisontimereduce_p,
        /// <summary>
        /// Giảm sát thương độc
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_poisondamagereduce_v,

        /// <summary>
        /// % tốc độ di chuyển
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_fastwalkrun_p,
        /// <summary>
        /// Tốc độ di chuyển
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_fastwalkrun_v,

        /// <summary>
        /// Tốc độ xuất chiêu hệ ngoại công
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_attackspeed_v,
        /// <summary>
        /// Tốc độ xuất chiêu hệ nội công
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_castspeed_v,

        /// <summary>
        /// Phản đòn cận chiến
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_meleedamagereturn_v,
        /// <summary>
        /// Phản đòn % sát thương cận chiến
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_meleedamagereturn_p,
        /// <summary>
        /// Phản đòn tầm xa
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_rangedamagereturn_v,
        /// <summary>
        /// Phản đòn % sát thơng tầm xa
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_rangedamagereturn_p,

        /// <summary>
        /// Vật công ngoại
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_addphysicsdamage_v,

        /// <summary>
        /// Tăng hỏa công ngoại
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_addfiredamage_v,
        /// <summary>
        /// Tăng băng công ngoại
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_addcolddamage_v,
        /// <summary>
        /// Tăng lôi công ngoại
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_addlightingdamage_v,
        /// <summary>
        /// Tăng độc công ngoại
        /// <para>Value[0]: Giá trị Min</para>
        /// <para>Value[1]: Thời gian</para>
        /// <para>Value[2]: Giá trị Max</para>
        /// </summary>
        magic_addpoisondamage_v,

        /// <summary>
        /// % vật công ngoại
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_addphysicsdamage_p,
        /// <summary>
        /// % vật công nội
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_addphysicsmagic_p,

        /// <summary>
        /// Giảm tốc độ bay của đạn
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_slowmissile_b,
        /// <summary>
        /// Mê hoặc đối thủ
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_changecamp_b,
        magic_damage2addmana_p,                     // ÉËº¦×ª»¯ÎªÄÚÁ¦

        /// <summary>
        /// May mắn
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_lucky_v,


        /// <summary>
        /// Sát thương chuyển hóa thành sinh lực
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]: Tỷ lệ % (mặc định là 100%)</para>
        /// <para>Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_steallifeenhance_p,
        /// <summary>
        /// Sát thương chuyển hóa thành nội lực
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]: Tỷ lệ % (mặc định là 100%)</para>
        /// <para>Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_stealmanaenhance_p,
        /// <summary>
        /// Sát thương chuyển hóa thành thể lực
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]: Tỷ lệ % (mặc định là 100%)</para>
        /// <para>Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_stealstaminaenhance_p,


        /// <summary>
        /// Tăng cấp kỹ năng
        /// <para>Value[0]: Cấp độ</para>
        /// <para>Value[1]: Vô nghĩa</para>
        /// <para>Value[2]: ID kỹ năng</para>
        /// </summary>
        magic_allskill_v,


        /// <summary>
        /// Kỹ năng hệ kim
        /// <para>Hiện chưa dùng</para>
        /// </summary>
        magic_metalskill_v,
        /// <summary>
        /// Kỹ năng hệ mộc
        /// <para>Hiện chưa dùng</para>
        /// </summary>
        magic_woodskill_v,
        /// <summary>
        /// Kỹ năng hệ băng
        /// <para>Hiện chưa dùng</para>
        /// </summary>
        magic_waterskill_v,
        /// <summary>
        /// Kỹ năng hệ hỏa
        /// <para>Hiện chưa dùng</para>
        /// </summary>
        magic_fireskill_v,
        /// <summary>
        /// Kỹ năng hệ thổ
        /// <para>Hiện chưa dùng</para>
        /// </summary>
        magic_earthskill_v,


        /// <summary>
        /// Chí mạng
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_deadlystrikeenhance_r,

        /// <summary>
        /// % hút sát thương của khiên nội lực khi nội lực trên 15%
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_manashield_p,

        /// <summary>
        /// Né tránh
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_adddefense_v,
        /// <summary>
        /// % né tránh
        /// </summary>
        magic_adddefense_p,

        /// <summary>
        /// Xác suất làm tấn công chí mạng
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_fatallystrikeenhance_p,


        /// <summary>
        /// Mỗi nửa giây phục hồi sinh lực, duy trì
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]: Thời gian duy trì</para>
        /// <para>Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_lifepotion_v,
        /// <summary>
        /// Mỗi nửa giây phục hồi nội lực, duy trì
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]: Thời gian duy trì</para>
        /// <para>Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_manapotion_v,

        /// <summary>
        /// Chính xác
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_attackratingenhance_v,
        /// <summary>
        /// % chính xác
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_attackratingenhance_p,

        /// <summary>
        /// Vật công nội
        /// <para>Value[0]: Giá trị MIN</para>
        /// <para>Value[1]: Vô nghĩa</para>
        /// <para>Value[2]: Giá trị MAX</para>
        /// </summary>
        magic_addphysicsmagic_v,


        /// <summary>
        /// Tăng băng công nội
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_addcoldmagic_v,
        /// <summary>
        /// Tăng hỏa công nội
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_addfiremagic_v,
        /// <summary>
        /// Tăng lôi công nội
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_addlightingmagic_v,
        /// <summary>
        /// Tăng độc công nội
        /// <para>Value[0]: Giá trị Min</para>
        /// <para>Value[1]: Thời gian</para>
        /// <para>Value[2]: Giá trị Max</para>
        /// </summary>
        magic_addpoisonmagic_v,


        /// <summary>
        /// Trạng thái miễn dịch sát thương
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_statusimmunity_b,


        /// <summary>
        /// Tăng kinh nghiệm nhận được khi đánh quái
        /// <para>Value[0]: Giá trị Min</para>
        /// <para>Value[1]: Vô nghĩa</para>
        /// <para>Value[2]: Giá trị Max</para>
        /// </summary>
        magic_expenhance_v,
        /// <summary>
        /// Tăng % kinh nghiệm nhận được khi đánh quái
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_expenhance_p,

        /// <summary>
        /// Kháng ngũ hành tương khắc
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_seriesres_p,
        /// <summary>
        /// Tăng cường ngũ hành tương khắc (gồm cả cường hóa và nhược hóa)
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_seriesconquar_r,
        /// <summary>
        /// Cường hóa ngũ hành tương khắc
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_seriesenhance_r,


        magic_createnpc,

        /// <summary>
        /// Tự động tấn công
        /// <para>Hiện chưa dùng</para>
        /// </summary>
        magic_autoattacknpc,

        /// <summary>
        /// Hóa giải sát thương không vượt quá % sát thương ban đầu
        /// <para>Value[0]: Điểm sát thương hóa giải</para>
        /// <para>Value[1]: % sát thương ban đầu</para>
        /// <para>Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_dynamicmagicshield_v,

        /// <summary>
        /// Hạn chế tốc độ di chuyển
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_nomovespeed,


        /// <summary>
        /// Kỹ năng ngụy trang 1
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_changefeature1,
        /// <summary>
        /// Kỹ năng ngụy trang 2
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_changefeature2,
        /// <summary>
        /// Kỹ năng ngụy trang còn số lần
        /// <para>Hiện chưa dùng</para>
        /// </summary>
        magic_stealfeature,
        /// <summary>
        /// Thêm số lần kỹ năng ngụy trang còn
        /// <para>Value[0], Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_addstealfeatureskill,


        /// <summary>
        /// May mắn của KNB hành
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_lucky_v_partner,

        magic_listen_msg,

        /// <summary>
        /// Hiệu suất phục hồi sinh lực
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_lifereplenish_p,

        /// <summary>
        /// Xác suất phản đòn bùa chú
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_returnskill_p,

        /// <summary>
        /// Phản đòn sát thương độc công
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_poisondamagereturn_v,

        /// <summary>
        /// Phản đòn % sát thương độc công
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_poisondamagereturn_p,

        /// <summary>
        /// Giảm % giây giãn cách xuất hiện của kỹ năng
        /// <para>Value[0]: ID kỹ năng</para>
        /// <para>Value[1]: Tỷ lệ giảm</para>
        /// <para>Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_autoskill,


        /// <summary>
        /// Sử dụng kỹ năng không làm mất ẩn thân
        /// <para>Value[0]: Vô nghĩa</para>
        /// <para>Value[1]: Số lần</para>
        /// <para>Value[2]: ID nhóm</para>
        /// </summary>
        magic_hide,

        /// <summary>
        /// Nội lực bị mất khi chịu sát thương độc
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_poison2decmana_p,


        /// <summary>
        /// Sinh mệnh của khiên nội lực
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]: Thời gian</para>
        /// <para>Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_staticmagicshield_v,
        /// <summary>
        /// Sinh mệnh tối đa của khiên nội lực
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]: Thời gian</para>
        /// <para>Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_staticmagicshieldmax_p,


        /// <summary>
        /// % kinh nghiệm nhận được của kỹ năng 120
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_add120skillexpenhance_p,

        /// <summary>
        /// Vô địch, không chiu sát thương
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_prop_invincibility,

        /// <summary>
        /// Có thể phát hiện đối thủ ẩn thân
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_prop_showhide,

        /// <summary>
        /// Xác suất bỏ qua cạm bẫy
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_prop_ignoretrap,

        /// <summary>
        /// Hiệu suất phục hồi nội lực
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_manareplenish_p,


        /// <summary>
        /// Hồi sinh
        /// <para>Value[0]: Phục hồi sinh lực</para>
        /// <para>Value[1]: Phục hồi nội lực</para>
        /// <para>Value[2]: Phục hồi thể lực</para>
        /// </summary>
        magic_revive,

        /// <summary>
        /// Xác suất né hoàn toàn kỹ năng
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]: Vô nghĩa</para>
        /// <para>Value[2]: ID nhóm (1: Ngoại công, 2: Nội công, 3: Nội ngoại công)</para>
        /// </summary>
        magic_ignoreskill,

        magic_ignoreinitiative,

        magic_infectcurse,

        magic_infectpoison,

        /// <summary>
        /// Bỏ qua né tránh đối thủ
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_ignoredefenseenhance_v,
        /// <summary>
        /// Bỏ qua % né tránh đối thủ
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_ignoredefenseenhance_p,

        /// <summary>
        /// Dùng toàn bộ nội lực chuyển hóa thành khiên nội lực, tối thiểu còn 15% nội lực
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]: Thời gian (1/18 giây)</para>
        /// <para>Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_staticmagicshieldcur_p,

        /// <summary>
        /// Mục tiêu bị trạng thái này sau khoảng thời gian có tỷ lệ bị trọng thương
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]: Thời gian (1/18 giây)</para>
        /// <para>Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_suddendeath,

        /// <summary>
        /// Tấn công khi đánh chí mạng
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_deadlystrikedamageenhance_p,


        magic_expxiuwei_v,                          // ÐÞÎª

        /// <summary>
        /// Thiết lập trạng thái ẩn thân
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_setblurstate,


        /// <summary>
        /// Mỗi nửa giây phục hồi sinh lực
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]: Thời gian</para>
        /// <para>Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_lifegrow_v,
        /// <summary>
        /// Mỗi nửa giây phục hồi nội lực
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]: Thời gian</para>
        /// <para>Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_managrow_v,
        /// <summary>
        /// Mỗi nửa giây phục hồi thể lực
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]: Thời gian</para>
        /// <para>Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_staminagrow_v,


        /// <summary>
        /// Nhận kinh nghiệm khi đồng đội đánh quái
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_addexpshare,
        /// <summary>
        /// Kinh nghiệm hao tổn khi chết
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_subexplose,


        /// <summary>
        /// Cách mỗi khoảng thời gian, bỏ qua nửa giây tấn công
        /// <para>Value[0]: Thời gian (1/18 giây)</para>
        /// <para>Value[1]: Hệ số giãn cách (mặc định sẽ là nửa giây tương ứng số 9)</para>
        /// <para>Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_ignoreattackontime,


        /// <summary>
        /// Thời gian duy trì độc %
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_poisontimeenhance_p,


        /// <summary>
        /// Thiết lập trạng thái bảo vệ
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_protected,


        /// <summary>
        /// Hủy bỏ trạng thái của kỹ năng
        /// <para>Value[0]: ID kỹ năng</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_removestate,

        /// <summary>
        /// Xóa khiên nội lực
        /// <para>Value[0]; Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_removeshield,


        /// <summary>
        /// Phát huy lực tấn công cơ bản
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_skilldamageptrim,

        /// <summary>
        /// Chịu sát thương chí mạng
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_defencedeadlystrikedamagetrim,


        /// <summary>
        /// Cường hóa ngũ hành tương khắc
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_seriesenhance,
        /// <summary>
        /// Nhược hóa ngũ hành tương khắc
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_seriesabate,

        /// <summary>
        /// Tấn công cơ bản của vũ khí
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_weaponbasedamagetrim,


        /// <summary>
        /// Hóa giải sát thương độc, không vượt quá % sát thương ban đầu
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]: % sát thương ban đầu</para>
        /// <para>Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_posionweaken,

        /// <summary>
        /// Phát huy lực tấn công kỹ năng
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_skillselfdamagetrim,

        /// <summary>
        /// Tăng % kinh nghiệm
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_skillexpaddtion_p,

        /// <summary>
        /// Thời gian bị trạng thái ngũ hành
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_allseriesstateresisttime,
        /// <summary>
        /// Xác suất bị trạng thái ngũ hành
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_allseriesstateresistrate,
        /// <summary>
        /// Thời gian bị trạng thái bất lợi
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_allspecialstateresisttime,
        /// <summary>
        /// Xác suất bị trạng thái bất lợi
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_allspecialstateresistrate,

        magic_domainchangeself,                     // ÁìÍÁÕù¶áÕ½µÄ±äÉíÄ§·¨ÊôÐÔ


        magic_adddomainskill1,                      // Ôö¼ÓÇøÓòÕù¶áÕ½¼¼ÄÜ1
        magic_adddomainskill2,                      // Ôö¼ÓÇøÓòÕù¶áÕ½¼¼ÄÜ2
        magic_adddomainskill3,                      // Ôö¼ÓÇøÓòÕù¶áÕ½¼¼ÄÜ3
        magic_stealskillstate,

        /// <summary>
        /// Hút % nội lực của mục tiêu
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_wastemanap,


        /// <summary>
        /// Mỗi khoảng thời gian bỏ qua nửa giây tấn công
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]: Thay đổi</para>
        /// <para>Value[2]: Công kích cơ bản</para>
        /// </summary>
        magic_ignoreattack,

        /// <summary>
        /// Tăng thêm sát thương
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_damage_added,

        /// <summary>
        /// Thời gian sát thương thêm của ngũ hành hiện tại
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_seriesstate_added,

        /// <summary>
        /// Cường hóa kỹ năng
        /// <para>Value[0]: ID</para>
        /// <para>Value[1]: Cấp độ</para>
        /// <para>Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_addenchant,


        /// <summary>
        /// Khóa mục tiêu
        /// <para>Value[0]: Cấm di chuyển</para>
        /// <para>Value[1]: Cấm dùng kỹ năng</para>
        /// <para>Value[2]: Cấm dùng vật phẩm</para>
        /// </summary>
        magic_locked,

        /// <summary>
        /// Tỷ lệ bỏ qua kháng
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]: Chưa rõ</para>
        /// <para>Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_ignoreresist_p,

        /// <summary>
        /// Né trạng thái
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_defense_state,

        /// <summary>
        /// Ẩn toàn bộ
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_hide_all,

        /// <summary>
        /// Xóa Cooldown kỹ năng
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_clearcd,

        /// <summary>
        /// Giảm sinh lực tương đương khoảng cách
        /// <para>Value[0]: Số lần cộng dồn</para>
        /// <para>Value[1]: Khoảng cách tối đa</para>
        /// <para>Value[2]: ID kỹ năng sát thương</para>
        /// </summary>
        magic_rdclifewithdis,

        /// <summary>
        /// Phụ bản có bao nhiêu kẻ địch
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_runattackmany,

        /// <summary>
        /// Kỹ năng cộng thêm với mỗi kẻ địch xung quanh
        /// <para>Value[0]: ID kỹ năng</para>
        /// <para>Value[1]: Cộng dồn tối đa</para>
        /// <para>Value[2]: Phạm vi kẻ địch</para>
        /// </summary>
        magic_addedwithenemycount,

        /// <summary>
        /// % sát thương cộng thêm sau mỗi lần chạm mục tiêu
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_runattack_damageadded,

        /// <summary>
        /// Tăng sát thương dựa vào % nội lực hiện có
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]: Vô nghĩa</para>
        /// <para>Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_attackenhancebycostmana_p,

        /// <summary>
        /// Bỏ qua trạng thái bất lợi
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_ignore_debuff,

        // TODO:liuchang ×ö³ÉÀàËÆ×ÓÐ­ÒéÄÇÖÖ
        magic_state_begin = 0x0200,

        // ½£ÊÀÐÂ¼ÓÊôÐÔ
        magic_state_hurt,                       // ×´Ì¬×é - ÊÜÉË¶¯×÷

        magic_state_hurt_end = magic_state_hurt + 9,
        magic_state_weak,                       // ×´Ì¬×é - ÐéÈõ
        magic_state_weak_end = magic_state_weak + 9,
        magic_state_slowall,                    // ×´Ì¬×é - ³Ù»º
        magic_state_slowall_end = magic_state_slowall + 9,
        magic_state_burn,                       // ×´Ì¬×é - ×ÆÉË
        magic_state_burn_end = magic_state_burn + 9,
        magic_state_stun,                       // ×´Ì¬×é - Ñ£ÔÎ
        magic_state_stun_end = magic_state_stun + 9,
        magic_state_fixed,                      // ×´Ì¬×é - ¶¨Éí
        magic_state_fixed_end = magic_state_fixed + 9,
        magic_state_palsy,                      // ×´Ì¬×é - Âé±Ô
        magic_state_palsy_end = magic_state_palsy + 9,
        magic_state_slowrun,                    // ×´Ì¬×é - ÅÜËÙ½µµÍ
        magic_state_slowrun_end = magic_state_slowrun + 9,
        magic_state_freeze,                     // ×´Ì¬×é - ¶³½á
        magic_state_freeze_end = magic_state_freeze + 9,
        magic_state_confuse,                    // ×´Ì¬×é - »ìÂÒ
        magic_state_confuse_end = magic_state_confuse + 9,
        magic_state_knock,                      // ×´Ì¬×é - »÷ÍË
        magic_state_knock_end = magic_state_knock + 9,
        magic_state_drag,                       // ×´Ì¬×é - À­³¶
        magic_state_drag_end = magic_state_drag + 9,
        magic_state_silence,                    // ×´Ì¬×é - ³ÁÄ¬
        magic_state_silence_end = magic_state_silence + 9,
        magic_state_zhican,                 // ×´Ì¬×é - ÖÂ²Ð
        magic_state_zhican_end = magic_state_zhican + 9,
        magic_state_float,                  // ×´Ì¬×é - ¸¡¿Õ
        magic_state_float_end = magic_state_float + 9,

        magic_state_end,

        magic_damage_resist_begin = 0x300,

        /// <summary>
        /// Kháng tất cả
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_damage_all_resist,

        /// <summary>
        /// Vật phòng
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_damage_physics_resist,
        /// <summary>
        /// Độc phòng
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_damage_poison_resist,
        /// <summary>
        /// Băng phòng
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_damage_cold_resist,
        /// <summary>
        /// Hỏa phòng
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_damage_fire_resist,
        /// <summary>
        /// Lôi phòng
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_damage_light_resist,
        /// <summary>
        /// Kháng ngũ hành
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_damage_series_resist,

        magic_damage_resist_end,

        magic_damage_receive_begin = 0x350,
        /// <summary>
        /// % vật công phải chịu
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_damage_physics_receive_p,
        /// <summary>
        /// % độc công phải chịu
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_damage_poison_receive_p,
        /// <summary>
        /// % băng công phải chịu
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_damage_cold_receive_p,
        /// <summary>
        /// % hỏa công phải chịu
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_damage_fire_receive_p,
        /// <summary>
        /// % lôi công phải chịu
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_damage_light_receive_p,
        /// <summary>
        /// Kháng phản đòn thọ thương
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_damage_return_receive_p,

        magic_damage_receive_end,

        //Ö÷¶¯¹¥»÷ÉËº¦ÊôÐÔ
        magic_trice_eff_begin = 0x0400,

        /// <summary>
        /// Chính xác
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_trice_eff_attackrating_v,
        /// <summary>
        /// % chính xác
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_trice_eff_attackrating_p,

        magic_trice_eff_ignoredefense_v,
        magic_trice_eff_ignoredefense_p,

        /// <summary>
        /// Vật công
        /// <para>Value[0]: Giá trị min</para>
        /// <para>Value[1]: Vô nghĩa</para>
        /// <para>Value[2]: Giá trị max</para>
        /// </summary>
        magic_trice_eff_physicsenhance_v,

        /// <summary>
        /// Băng công
        /// <para>Value[0]: Giá trị min</para>
        /// <para>Value[1]: Vô nghĩa</para>
        /// <para>Value[2]: Giá trị max</para>
        /// </summary>
        magic_trice_eff_colddamage_v,

        /// <summary>
        /// Hỏa công
        /// <para>Value[0]: Giá trị min</para>
        /// <para>Value[1]: Vô nghĩa</para>
        /// <para>Value[2]: Giá trị max</para>
        /// </summary>
        magic_trice_eff_firedamage_v,

        /// <summary>
        /// Lôi công
        /// <para>Value[0]: Giá trị min</para>
        /// <para>Value[1]: Vô nghĩa</para>
        /// <para>Value[2]: Giá trị max</para>
        /// </summary>
        magic_trice_eff_lightingdamage_v,
        /// <summary>
        /// Độc công / nửa giây
        /// <para>Value[0]: Giá trị min</para>
        /// <para>Value[1]: Thời gian</para>
        /// <para>Value[2]: Giá trị max</para>
        /// </summary>
        magic_trice_eff_poisondamage_v,

        /// <summary>
        /// Ngũ hành công
        /// <para>Value[0]: Giá trị min</para>
        /// <para>Value[1]: Vô nghĩa</para>
        /// <para>Value[2]: Giá trị max</para>
        /// </summary>
        magic_trice_eff_magicdamage_v,

        /// <summary>
        /// % vật công
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_trice_eff_physicsenhance_p,

        /// <summary>
        /// Hút sinh lực
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]: Tỷ lệ %</para>
        /// <para>Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_trice_eff_steallife_p,
        /// <summary>
        /// Hút nội lực
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]: Tỷ lệ %</para>
        /// <para>Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_trice_eff_stealmana_p,
        /// <summary>
        /// Hút thể lực
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]: Tỷ lệ %</para>
        /// <para>Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_trice_eff_stealstamina_p,


        magic_trice_eff_fatallystrike_p,

        /// <summary>
        /// Ngũ hành tương khắc
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]; Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_trice_eff_seriesdamage_r,

        magic_trice_eff_physicsdamage_v,
        magic_trice_eff_angerdamage_p,

        /// <summary>
        /// Đánh cắp trạng thái của đối thủ, chuyển sang bản thân hoặc đồng đội
        /// <para>Value[0]: Tổng số đánh cắp tối đa</para>
        /// <para>Value[1]: Tỷ lệ đánh cắp</para>
        /// <para>Value[2]: Cấp độ thi triển lên bản thân hoặc đồng đội</para>
        /// </summary>
        magic_trice_eff_stealstate,

        magic_trice_eff_state_begin,
        magic_trice_eff_hurt = magic_trice_eff_state_begin,
        magic_trice_eff_weak,
        magic_trice_eff_slowall,
        magic_trice_eff_burn,
        magic_trice_eff_stun,
        magic_trice_eff_fixed,
        magic_trice_eff_palsy,
        magic_trice_eff_slowrun,
        magic_trice_eff_freeze,
        magic_trice_eff_confuse,
        magic_trice_eff_knock,
        magic_trice_eff_drag,
        magic_trice_eff_silence,
        magic_trice_eff_zhican,
        magic_trice_eff_float,
        magic_trice_eff_state_end = magic_trice_eff_float,
        magic_trice_eff_damage_end,

        magic_normal_end,

        //¸Ä±ä¼¼ÄÜµÄÊôÐÔ
        magic_skill_begin = 0x0500,

        /// <summary>
        /// Nội lực mất
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_skill_cost_v,

        magic_skill_costtype_v,                         //

        /// <summary>
        /// Giãn cách thi triển
        /// <para>Value[0]: Thời gian (1/18 giây)</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_skill_mintimepercast_v,

        magic_skill_missilenum_v,                       //
        magic_skill_missilesform_v,
        magic_skill_param1_v,
        magic_skill_param2_v,
        magic_skill_skillexp_v,                         // ¼¼ÄÜÏÂ¸öµÈ¼¶ÐèÒªµÄÊìÁ·¾­Ñé
        magic_skill_waittime,                           // ¼¼ÄÜµÄµÈ´ýÊ±¼ä

        /// <summary>
        /// Duy trì
        /// <para>Value[0]: Giá trị (1/18 giây)</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_skill_statetime,

        /// <summary>
        /// Giãn cách thi triển khi cưỡi
        /// <para>Value[0]: Thời gian (1/18 giây)</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_skill_mintimepercastonhorse_v,

        magic_skill_appendskill,                        // ×·¼Ó¼¼ÄÜ£¬½«¶à¸ö¼¼ÄÜµÄÄ§·¨ÊôÐÔºÏ²¢µ½µ±Ç°¼¼ÄÜµÄ×Óµ¯ÉÏ
        magic_skill_eventskilllevel,
        magic_skill_deadlystrike_r,                     // »áÐÄÒ»»÷µÄ¼¸ÂÊ
        /// <summary>
        /// Hỗ trợ tăng % sát thương cho kỹ năng 1
        /// <para>Value[0]: Kỹ năng</para>
        /// <para>Value[1]: % sát thương</para>
        /// <para>Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_skill_addskilldamage1,
        /// <summary>
        /// Hỗ trợ tăng % sát thương cho kỹ năng 2
        /// <para>Value[0]: Kỹ năng</para>
        /// <para>Value[1]: % sát thương</para>
        /// <para>Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_skill_addskilldamage2,
        /// <summary>
        /// Hỗ trợ tăng % sát thương cho kỹ năng 3
        /// <para>Value[0]: Kỹ năng</para>
        /// <para>Value[1]: % sát thương</para>
        /// <para>Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_skill_addskilldamage3,
        /// <summary>
        /// Hỗ trợ tăng % sát thương cho kỹ năng 4
        /// <para>Value[0]: Kỹ năng</para>
        /// <para>Value[1]: % sát thương</para>
        /// <para>Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_skill_addskilldamage4,
        /// <summary>
        /// Hỗ trợ tăng % sát thương cho kỹ năng 5
        /// <para>Value[0]: Kỹ năng</para>
        /// <para>Value[1]: % sát thương</para>
        /// <para>Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_skill_addskilldamage5,
        /// <summary>
        /// Hỗ trợ tăng % sát thương cho kỹ năng 6
        /// <para>Value[0]: Kỹ năng</para>
        /// <para>Value[1]: % sát thương</para>
        /// <para>Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_skill_addskilldamage6,

        /// <summary>
        /// Cự ly thi triển tăng
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_skill_attackradius,

        /// <summary>
        /// Đồng thời thi triển kỹ năng
        /// <para>Value[0]: ID kỹ năng</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_skill_startevent,

        /// <summary>
        /// Gọi kỹ năng bay
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_skill_flyevent,
        /// <summary>
        /// Gọi kỹ năng khi chạm mục tiêu
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_skill_collideevent,
        /// <summary>
        /// Gọi kỹ năng khi tan biến
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_skill_vanishedevent,


        magic_skill_showevent,

        /// <summary>
        /// Số lần ra chiêu
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_skill_shotnumber,

        /// <summary>
        /// Số cạm bẫy tối đa cùng loại
        /// <para>Value[0]: ID kỹ năng</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_skill_maxmissile,

        magic_skill_appendskill2,
        magic_skill_appendskill3,
        magic_skill_appendskill4,
        magic_skill_appendskill5,
        magic_skill_appendskill6,

        /// <summary>
        /// % sát thương kỹ năng
        /// <para>Value[0]: ID kỹ năng</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_skilldamage_p,

        /// <summary>
        /// Phát huy lực tấn công cơ bản
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_appenddamage_p,

        magic_skill_end,

        //¸Ä±ä·¢³ö×Óµ¯µÄÊôÐÔ
        magic_missile_begin = 0x0600,

        magic_missile_movekind_v,                       // ×Óµ¯¸ñÊ½

        /// <summary>
        /// Tốc độ bay của đạn
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_missile_speed_v,

        /// <summary>
        /// Thời gian duy trì bẫy
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_missile_lifetime_v,
        magic_missile_height_v,                         // ×Óµ¯µÄ¸ß¶È
        magic_missile_damagerange_v,                    // ×Óµ¯ÆÆ»µ·¶Î§
        /// <summary>
        /// Phạm vi nổ của đạn (số ô)
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_missile_radius_v,
        magic_missile_missrate,                         // ×Óµ¯¡°²»ÃüÖÐ¡±ÂÊ£¬0Îª±ØÖÐ

        /// <summary>
        /// Số mục tiêu ảnh hưởng tối đa
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_missile_hitcount,
        /// <summary>
        /// Cự ly nổ
        /// <para>Value[0]: Giá trị Min</para>
        /// <para>Value[1]: Vô nghĩa</para>
        /// <para>Value[2]: Giá trị Max</para>
        /// </summary>
        magic_missile_range,
        magic_missile_dmginterval,
        magic_missile_zspeed,
        /// <summary>
        /// Tỷ lệ xuyên suốt mục tiêu
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_missile_ablility,
        magic_missile_param,
        magic_missile_callnpc,                          // ×Óµ¯ÏûÍöÊ±²úÉúNpc
        magic_missile_drag,                             // À­³¶µ½×Óµ¯
        magic_missile_random,
        magic_missile_end,

        magic_skilladdition_begin = 0x0700,

        /// <summary>
        /// Tăng KMagicAttrib của kỹ năng
        /// <para>Value[0]: Giá trị 1</para>
        /// <para>Value[1]: Giá trị 2</para>
        /// <para>Value[2]: Giá trị 3</para>
        /// </summary>
        magic_skilladdition_adddamagepercent,
        /// <summary>
        /// Tăng KMagicAttrib của kỹ năng
        /// <para>Value[0]: Giá trị 1</para>
        /// <para>Value[1]: Giá trị 2</para>
        /// <para>Value[2]: Giá trị 3</para>
        /// </summary>
        magic_skilladdition_adddamagepercent2,
        /// <summary>
        /// Tăng KMagicAttrib của kỹ năng
        /// <para>Value[0]: Giá trị 1</para>
        /// <para>Value[1]: Giá trị 2</para>
        /// <para>Value[2]: Giá trị 3</para>
        /// </summary>
        magic_skilladdition_adddamagepercent3,
        /// <summary>
        /// Tăng KMagicAttrib của kỹ năng
        /// <para>Value[0]: Giá trị 1</para>
        /// <para>Value[1]: Giá trị 2</para>
        /// <para>Value[2]: Giá trị 3</para>
        /// </summary>
        magic_skilladdition_adddamagepercent4,
        /// <summary>
        /// Tăng KMagicAttrib của kỹ năng
        /// <para>Value[0]: Giá trị 1</para>
        /// <para>Value[1]: Giá trị 2</para>
        /// <para>Value[2]: Giá trị 3</para>
        /// </summary>
        magic_skilladdition_adddamagepercent5,
        /// <summary>
        /// Tăng KMagicAttrib của kỹ năng
        /// <para>Value[0]: Giá trị 1</para>
        /// <para>Value[1]: Giá trị 2</para>
        /// <para>Value[2]: Giá trị 3</para>
        /// </summary>
        magic_skilladdition_adddamagepercent6,


        /// <summary>
        /// Tăng KMagicAttrib thời gian ẩn thân của kỹ năng
        /// <para>Value[0]: Giá trị 1</para>
        /// <para>Value[1]: Giá trị 2</para>
        /// <para>Value[2]: Giá trị 3</para>
        /// </summary>
        magic_skilladdition_addhidetime,
        /// <summary>
        /// Tăng KMagicAttrib của thời gian xuất chiêu
        /// <para>Value[0]: Giá trị 1</para>
        /// <para>Value[1]: Giá trị 2</para>
        /// <para>Value[2]: Giá trị 3</para>
        /// </summary>
        magic_skilladdition_decreasepercasttime,

        /// <summary>
        /// Tăng KMagicAttrib của cấp độ kỹ năng
        /// <para>Value[0]: Giá trị 1</para>
        /// <para>Value[1]: Giá trị 2</para>
        /// <para>Value[2]: Giá trị 3</para>
        /// </summary>
        magic_skilladdition_addskilllevel,


        magic_skilladdition_addallskilllevel,


        magic_skilladdition_addsingleskilllevel,

        /// <summary>
        /// Tăng KMagicAttrib tỷ lệ xuyên suốt mục tiêu của đạn
        /// <para>Value[0]: Giá trị 1</para>
        /// <para>Value[1]: Giá trị 2</para>
        /// <para>Value[2]: Giá trị 3</para>
        /// </summary>
        magic_skilladdition_addmissilethroughrate,

        /// <summary>
        /// Tăng % sát thương mỗi lần xuyên mục tiêu của đạn
        /// <para>Value[0]: ID kỹ năng</para>
        /// <para>Value[1]: Tăng % sát thương mỗi lần</para>
        /// <para>Value[2]: Tối đa %</para>
        /// </summary>
        magic_skilladdition_addpowerwhencol,

        /// <summary>
        /// Tăng phạm vi nổ mỗi lần xuyên mục tiêu của đạn
        /// <para>Value[0]: ID kỹ năng</para>
        /// <para>Value[1]: Tăng phạm số ô mỗi lần</para>
        /// <para>Value[2]: Tối đa số ô</para>
        /// </summary>
        magic_skilladdition_addrangewhencol,

        /// <summary>
        /// Tăng KMagicAttrib kỹ năng đi kèm lúc xuất chiêu
        /// <para>Value[0]: ID kỹ năng</para>
        /// <para>Value[1]: ID kỹ năng đi kèm</para>
        /// <para>Value[2]: Cấp độ</para>
        /// </summary>
        magic_skilladdition_addstartskill,

        /// <summary>
        /// Tăng KMagicAttrib giảm thời gian giãn cách xuất hiện của kỹ năng
        /// <para>Value[0]: ID kỹ năng</para>
        /// <para>Value[1]: Giá trị 2</para>
        /// <para>Value[2]: Thời gian</para>
        /// </summary>
        magic_skilladdition_decautoskillcdtime,

        /// <summary>
        /// Tăng KMagicAttrib số lượng đạn bay
        /// <para>Value[0]: Giá trị 1</para>
        /// <para>Value[1]: Giá trị 2</para>
        /// <para>Value[2]: Giá trị 3</para>
        /// </summary>
        magic_skilladdition_addmissilenum,

        /// <summary>
        /// Tăng KMagicAttrib kỹ năng đạn bay khi bẫy nổ
        /// <para>Value[0]: ID kỹ năng</para>
        /// <para>Value[1]: ID kỹ năng đi kèm</para>
        /// <para>Value[2]: Cấp độ</para>
        /// </summary>
        magic_skilladdition_addflyskill,

        /// <summary>
        /// Tăng KMagicAttrib kỹ năng đi kèm khi biến mất
        /// <para>Value[0]: ID kỹ năng</para>
        /// <para>Value[1]: ID kỹ năng đi kèm</para>
        /// <para>Value[2]: Cấp độ</para>
        /// </summary>
        magic_skilladdition_addvanishskill,

        /// <summary>
        /// Tăng thời gian bị trạng thái ngũ hành tương ứng căn cứ khoảng cách
        /// <para>Value[0]: ID trạng thái (KE_STATE)</para>
        /// <para>Value[1]: Tỷ lệ cộng thêm (chia 100 thì ra đơn vị %)</para>
        /// <para>Value[2]: Khoảng cách tối đa</para>
        /// </summary>
        magic_skilladdition_addmagicbydist,

        /// <summary>
        /// Cộng dồn tối đa số lần
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_skilladdition_superpose_magic,

        /// <summary>
        /// Sử dụng kỹ năng không bị mất ẩn thân
        /// <para>Value[0]: ID kỹ năng</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_skilladdition_keephide,

        magic_skilladdition_end,

        magic_missileaddition_begin = 0x850,
        magic_missileaaddition_addrange,                // ¸Ä±ä×Óµ¯µÄÉËº¦·¶Î§
        magic_missileaaddition_addrange2,
        magic_missileaaddition_addrange3,
        magic_missileaaddition_addrange4,
        magic_missileaaddition_addrange5,
        magic_missileaaddition_addrange6,
        magic_missileaddition_end,

        magic_equip_active_begin = 0x900,                   // ¼¤»î×°±¸ÊôÐÔ£¨·Ç¼¼ÄÜ·Ç³£¹æÊôÐÔ£¬²»×ß³£¹æÊôÐÔ»úÖÆ£¬µ«ÎªÍ³Ò»£¬·ÅÔÚÕâÀï×ö¶¨Òå£©
        magic_equip_active_all_ornament,                // ¼¤»îÈ«²¿Ê×ÊÎ°µÊôÐÔ
        magic_equip_active_suit,                        // ¼¤»îÄ³¸öÌ××°µÄÌ××°ÊôÐÔ
        magic_equip_active_end,

        magic_damage_append_begin = 0x1000,                 // ÉËº¦ÊôÐÔ£¬ÓÃÓÚAppend¼¼ÄÜµÄÊ±ºò
        magic_damage_append_hitrate,                    // ÃüÖÐ
        magic_damage_append_series,                     // ÎåÐÐÉËº¦
        magic_damage_append_fatallystrike,              // ÖÂÃüÒ»»÷
        magic_damage_append_series_begin,
        magic_damage_append_physics = magic_damage_append_series_begin,                 // ÎïÀíÉËº¦
        magic_damage_append_cold,                       // ±ùÉËº¦
        magic_damage_append_fire,                       // »ðÉËº¦
        magic_damage_append_light,                      // À×ÉËº¦
        magic_damage_append_poison,                     // ¶¾ÉËº¦
        magic_damage_append_series_end = magic_damage_append_poison,
        magic_damage_append_magic,                      // Ä§·¨ÉËº¦
        magic_damage_append_steallife,                  // ÍµÑª
        magic_damage_append_stealmana,                  // ÍµÄÚ
        magic_damage_append_stealstamina,               // ÍµÌå
        magic_damage_append_stealstate,                 // Íµ×´Ì¬

        magic_damage_append_state_begin,
        magic_damage_append_hurt = magic_damage_append_state_begin,
        magic_damage_append_weak,
        magic_damage_append_slowall,
        magic_damage_append_burn,
        magic_damage_append_stun,
        magic_damage_append_fixed,
        magic_damage_append_palsy,
        magic_damage_append_slowrun,
        magic_damage_append_freeze,
        magic_damage_append_confuse,
        magic_damage_append_knock,
        magic_damage_append_drag,
        magic_damage_append_silence,
        magic_damage_append_zhican,
        magic_damage_append_float,
        magic_damage_append_state_end = magic_damage_append_float,
        magic_damage_append_end,

        magic_state_effect_begin = 0x1100,

        /// <summary>
        /// Tỷ lệ gây thọ thương
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]: Duy trì (1/18 giây)</para>
        /// <para>Value[2]: Vô nghĩa</para>
        /// </summary>
        state_hurt_attack,
        /// <summary>
        /// Thời gian bị thọ thương
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        state_hurt_resisttime,
        /// <summary>
        /// Xác suất bị thọ thương
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        state_hurt_resistrate,
        /// <summary>
        /// Thời gian gây thọ thương
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        state_hurt_attacktime,
        /// <summary>
        /// Xác suất làm thọ thương
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        state_hurt_attackrate,

        /// <summary>
        /// Tỷ lệ gây suy yếu
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]: Duy trì (1/18 giây)</para>
        /// <para>Value[2]: Vô nghĩa</para>
        /// </summary>
        state_weak_attack,
        /// <summary>
        /// Thời gian bị suy yếu
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        state_weak_resisttime,
        /// <summary>
        /// Xác suất bị suy yếu
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        state_weak_resistrate,
        /// <summary>
        /// Thời gian gây suy yếu
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        state_weak_attacktime,
        /// <summary>
        /// Xác suất làm suy yếu
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        state_weak_attackrate,

        /// <summary>
        /// Tỷ lệ gây làm chậm
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]: Duy trì (1/18 giây)</para>
        /// <para>Value[2]: Vô nghĩa</para>
        /// </summary>
        state_slowall_attack,
        /// <summary>
        /// Thời gian bị làm chậm
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        state_slowall_resisttime,
        /// <summary>
        /// Xác suất bị làm chậm
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        state_slowall_resistrate,
        /// <summary>
        /// Thời gian gây làm chậm
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        state_slowall_attacktime,
        /// <summary>
        /// Xác suất làm làm chậm
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        state_slowall_attackrate,

        /// <summary>
        /// Tỷ lệ gây bỏng
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]: Duy trì</para>
        /// <para>Value[2]: Vô nghĩa</para>
        /// </summary>
        state_burn_attack,
        /// <summary>
        /// Thời gian bị bỏng
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        state_burn_resisttime,
        /// <summary>
        /// Xác suất bị bỏng
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        state_burn_resistrate,
        /// <summary>
        /// Thời gian gây bỏng
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        state_burn_attacktime,
        /// <summary>
        /// Xác suất làm bỏng
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        state_burn_attackrate,

        /// <summary>
        /// Tỷ lệ gây choáng
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]: Duy trì (1/18 giây)</para>
        /// <para>Value[2]: Vô nghĩa</para>
        /// </summary>
        state_stun_attack,
        /// <summary>
        /// Thời gian bị choáng
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        state_stun_resisttime,
        /// <summary>
        /// Xác suất bị choáng
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        state_stun_resistrate,
        /// <summary>
        /// Thời gian gây choáng
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        state_stun_attacktime,
        /// <summary>
        /// Xác suất làm choáng
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        state_stun_attackrate,

        /// <summary>
        /// Định thân
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]: Duy trì (1/18 giây)</para>
        /// <para>Value[2]: Vô nghĩa</para>
        /// </summary>
        state_fixed_attack,
        /// <summary>
        /// Thời gian bị định thân
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        state_fixed_resisttime,
        /// <summary>
        /// Xác suất bị định thân
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        state_fixed_resistrate,
        /// <summary>
        /// Thời gian gây định thân
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        state_fixed_attacktime,
        /// <summary>
        /// Xác suất làm định thân
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        state_fixed_attackrate,

        /// <summary>
        /// Tỷ lệ gây tê liệt
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]: Duy trì (1/18 giây)</para>
        /// <para>Value[2]: Vô nghĩa</para>
        /// </summary>
        state_palsy_attack,
        /// <summary>
        /// Thời gian bị tê liệt
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        state_palsy_resisttime,
        /// <summary>
        /// Xác suất bị tê liệt
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        state_palsy_resistrate,
        /// <summary>
        /// Thời gian gây tê liệt
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        state_palsy_attacktime,
        /// <summary>
        /// Xác suất làm tê liệt
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        state_palsy_attackrate,

        /// <summary>
        /// Tỷ lệ gây giảm tốc
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]: Duy trì (1/18 giây)</para>
        /// <para>Value[2]: Vô nghĩa</para>
        /// </summary>
        state_slowrun_attack,
        /// <summary>
        /// Thời gian bị giảm tốc
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        state_slowrun_resisttime,
        /// <summary>
        /// Xác suất bị giảm tốc
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        state_slowrun_resistrate,
        /// <summary>
        /// Thời gian gây giảm tốc
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        state_slowrun_attacktime,
        /// <summary>
        /// Xác suất làm giảm tốc
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        state_slowrun_attackrate,

        /// <summary>
        /// Tỷ lệ gây đóng băng
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]: Duy trì (1/18 giây)</para>
        /// <para>Value[2]: Vô nghĩa</para>
        /// </summary>
        state_freeze_attack,
        /// <summary>
        /// Thời gian bị đóng băng
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        state_freeze_resisttime,
        /// <summary>
        /// Xác suất bị đóng băng
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        state_freeze_resistrate,
        /// <summary>
        /// Thời gian gây đóng băng
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        state_freeze_attacktime,
        /// <summary>
        /// Xác suất làm đóng băng
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        state_freeze_attackrate,

        /// <summary>
        /// Tỷ lệ gây hỗn loạn
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]: Duy trì (1/18 giây)</para>
        /// <para>Value[2]: Vô nghĩa</para>
        /// </summary>
        state_confuse_attack,
        /// <summary>
        /// Thời gian bị hỗn loạn
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        state_confuse_resisttime,
        /// <summary>
        /// Xác suất bị hỗn loạn
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        state_confuse_resistrate,
        /// <summary>
        /// Thời gian gây hỗn loạn
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        state_confuse_attacktime,
        /// <summary>
        /// Xác suất làm hỗn loạn
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        state_confuse_attackrate,

        /// <summary>
        /// Tỷ lệ gây đẩy lui
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]: Duy trì (1/18 giây)</para>
        /// <para>Value[2]: Khoảng cách dịch được trong mỗi Frame (1/18 giây)</para>
        /// </summary>
        state_knock_attack,
        /// <summary>
        /// Thời gian bị đẩy lui
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        state_knock_resisttime,
        /// <summary>
        /// Xác suất bị đẩy lui
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        state_knock_resistrate,
        /// <summary>
        /// Thời gian gây đẩy lui
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        state_knock_attacktime,
        /// <summary>
        /// Xác suất làm đẩy lui
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        state_knock_attackrate,

        /// <summary>
        /// Tỷ lệ gây bất lực
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]: Duy trì (1/18 giây)</para>
        /// <para>Value[2]: Vô nghĩa</para>
        /// </summary>
        state_silence_attack,
        /// <summary>
        /// Thời gian bị bất lực
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        state_silence_resisttime,
        /// <summary>
        /// Xác suất bị bất lực
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        state_silence_resistrate,
        /// <summary>
        /// Thời gian gây bất lực
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        state_silence_attacktime,
        /// <summary>
        /// Xác suất làm bất lực
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        state_silence_attackrate,

        /// <summary>
        /// Tỷ lệ gây kéo lại
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1]: Duy trì (1/18 giây)</para>
        /// <para>Value[2]: Khoảng cách dịch được trong mỗi Frame (1/18 giây)</para>
        /// </summary>
        state_drag_attack,
        /// <summary>
        /// Thời gian bị kéo lại
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        state_drag_resisttime,
        /// <summary>
        /// Xác suất bị kéo lại
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        state_drag_resistrate,
        /// <summary>
        /// Thời gian gây kéo lại
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        state_drag_attacktime,
        /// <summary>
        /// Xác suất làm kéo lại
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        state_drag_attackrate,


        state_zhican_attack,
        state_zhican_resisttime,
        state_zhican_resistrate,
        state_zhican_attacktime,
        state_zhican_attackrate,


        state_float_attack,
        state_float_resisttime,
        state_float_resistrate,
        state_float_attacktime,
        state_float_attackrate,

        /// <summary>
        /// Hóa giải và miễn nhiễm thọ thương
        /// </summary>
        state_hurt_ignore,
        /// <summary>
        /// Hóa giải và miễn nhiễm suy yếu
        /// </summary>
        state_weak_ignore,
        /// <summary>
        /// Hóa giải và miễn nhiễm làm chậm
        /// </summary>
        state_slowall_ignore,
        /// <summary>
        /// Hóa giải và miễn nhiễm bỏng
        /// </summary>
        state_burn_ignore,
        /// <summary>
        /// Hóa giải và miễn nhiễm choáng
        /// </summary>
        state_stun_ignore,
        /// <summary>
        /// Hóa giải và miễn nhiễm bất động
        /// </summary>
        state_fixed_ignore,
        /// <summary>
        /// Hóa giải và miễn nhiễm tê liệt
        /// </summary>
        state_palsy_ignore,
        /// <summary>
        /// Hóa giải và miễn nhiễm làm chậm
        /// </summary>
        state_slowrun_ignore,
        /// <summary>
        /// Hóa giải và miễn nhiễm đóng băng
        /// </summary>
        state_freeze_ignore,
        /// <summary>
        /// Hóa giải và miễn nhiễm hỗn loạn
        /// </summary>
        state_confuse_ignore,
        /// <summary>
        /// Hóa giải và miễn nhiễm đẩy lui
        /// </summary>
        state_knock_ignore,
        /// <summary>
        /// Hóa giải và miễn nhiễm bất lực
        /// </summary>
        state_silence_ignore,
        /// <summary>
        /// Hóa giải và miễn nhiễm kéo lại
        /// </summary>
        state_drag_ignore,

        state_zhican_ignore,

        /// <summary>
        /// Hóa giải và miễn nhiễm hút lên cao
        /// </summary>
        state_float_ignore,

        /// <summary>
        /// Hóa giải sát thương của người chơi có quan hàm thấp hơn
        /// </summary>
        magic_absorb_lower_chop_damage,

        /// <summary>
        /// Tăng sát thương hệ kim
        /// </summary>
        magic_me2metaldamage_p,
        /// <summary>
        /// Tăng sát thương hệ thủy
        /// </summary>
        magic_me2waterdamage_p,

        /// <summary>
        /// Tăng sát thương hệ hỏa
        /// </summary>
        magic_me2firedamage_p,

        /// <summary>
        /// Tăng sát thương hệ thổ
        /// </summary>
        magic_me2earthdamage_p,


        /// <summary>
        /// Tăng sát thương hệ mộc
        /// </summary>
        magic_me2wooddamage_p,

        /// <summary>
        /// Tỷ lệ bỏ qua bùa chú
        /// </summary>
        magic_ignore_curse_p,

        /// <summary>
        /// Xác suất miễn nhiễm với kỹ năng
        /// </summary>
        magic_immune_skill_1,
        magic_immune_skill_2,
        magic_immune_skill_3,
        magic_immune_skill_4,
        magic_immune_skill_5,
        magic_immune_skill_6,
        magic_immune_skill_7,
        magic_immune_skill_8,
        magic_immune_skill_9,
        magic_immune_skill_10,
        magic_immune_skill_11,
        magic_immune_skill_12,
        magic_immune_skill_13,
        magic_immune_skill_14,
        magic_immune_skill_15,
        magic_immune_skill_16,
        magic_immune_skill_17,
        magic_immune_skill_18,
        magic_immune_skill_19,
        magic_immune_skill_20,

        /// <summary>
        /// Giảm sát thương nội công tầm gần
        /// </summary>
        magic_reduce_near_magic_damage,
        /// <summary>
        /// Giảm sát thương nội công tầm xa
        /// </summary>
        magic_reduce_far_magic_damage,
        /// <summary>
        /// Giảm sát thương ngoại công tầm gần
        /// </summary>
        magic_reduce_near_physic_damage,
        /// <summary>
        /// Giảm sát thương ngoại công tầm xa
        /// </summary>
        magic_reduce_far_physic_damage,

        /// <summary>
        /// Tăng % sát thương khi nội lực đầy
        /// </summary>
        magic_manatoskill_enhance,

        /// <summary>
        /// % hỏa công ngoại
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_addfiredamage_p,

        /// <summary>
        /// % băng công ngoại
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_addcolddamage_p,

        /// <summary>
        /// % lôi công ngoại
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_addlightingdamage_p,

        /// <summary>
        /// % độc công ngoại
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_addpoisondamage_p,

        /// <summary>
        /// % băng công nội
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_addcoldmagic_p,

        /// <summary>
        /// % hỏa công nội
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_addfiremagic_p,

        /// <summary>
        /// % lôi công nội
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_addlightingmagic_p,

        /// <summary>
        /// % độc công nội
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_addpoisonmagic_p,

        /// <summary>
        /// Tạo ảo ảnh của bản thân
        /// <para>Value[0]: Thời gian duy trì</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_create_illusion,

        /// <summary>
        /// Tăng sát thương gây ra bởi bản thân
        /// <para>Value[0]: Thời gian duy trì</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_damage_inc_p,

        /// <summary>
        /// Giảm sát thương gây ra bởi bản thân
        /// <para>Value[0]: Thời gian duy trì</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_redeivedamage_dec_p2,

        /// <summary>
        /// Cấm sử dụng kỹ năng 1
        /// <para>Value[0]: ID kỹ năng</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_ban_skill_1,

        /// <summary>
        /// Cấm sử dụng kỹ năng 2
        /// <para>Value[0]: ID kỹ năng</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_ban_skill_2,

        /// <summary>
        /// Cấm sử dụng kỹ năng 3
        /// <para>Value[0]: ID kỹ năng</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_ban_skill_3,

        /// <summary>
        /// Cấm sử dụng kỹ năng 4
        /// <para>Value[0]: ID kỹ năng</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_ban_skill_4,

        /// <summary>
        /// Cấm sử dụng kỹ năng 5
        /// <para>Value[0]: ID kỹ năng</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_ban_skill_5,

        /// <summary>
        /// Cấm sử dụng kỹ năng 6
        /// <para>Value[0]: ID kỹ năng</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_ban_skill_6,

        /// <summary>
        /// Kỹ năng tiêu hao Buff tương ứng 1
        /// <para>Value[0]: ID Bufff</para>
        /// <para>Value[1]: Số tầng (-1 là toàn bộ)</para>
        /// <para>Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_skill_cost_buff1layers_v,

        /// <summary>
        /// Kỹ năng tiêu hao Buff tương ứng 2
        /// <para>Value[0]: ID Bufff</para>
        /// <para>Value[1]: Số tầng (-1 là toàn bộ)</para>
        /// <para>Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_skill_cost_buff2layers_v,

        /// <summary>
        /// Kỹ năng tiêu hao Buff tương ứng 3
        /// <para>Value[0]: ID Bufff</para>
        /// <para>Value[1]: Số tầng (-1 là toàn bộ)</para>
        /// <para>Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_skill_cost_buff3layers_v,

        /// <summary>
        /// Kỹ năng tiêu hao Buff tương ứng 4
        /// <para>Value[0]: ID Bufff</para>
        /// <para>Value[1]: Số tầng (-1 là toàn bộ)</para>
        /// <para>Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_skill_cost_buff4layers_v,

        /// <summary>
        /// Kỹ năng tiêu hao Buff tương ứng 5
        /// <para>Value[0]: ID Bufff</para>
        /// <para>Value[1]: Số tầng (-1 là toàn bộ)</para>
        /// <para>Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_skill_cost_buff5layers_v,

        /// <summary>
        /// Kỹ năng tiêu hao Buff tương ứng 6
        /// <para>Value[0]: ID Bufff</para>
        /// <para>Value[1]: Số tầng (-1 là toàn bộ)</para>
        /// <para>Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_skill_cost_buff6layers_v,

        /// <summary>
        /// Nội lực tối đa chuyển thành sinh lực tối đa
        /// <para>Value[0]: ID kỹ năng</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_addmaxhpbymaxmp_p,

        /// <summary>
        /// Mỗi nửa giây phục hồi sinh lực dựa theo % Ngoại hiện tại
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_fastlifereplenish_byvitality,

        /// <summary>
        /// Phát huy lực tấn công kỹ năng % với mỗi 1% sinh lực đã mất tính từ thời điểm kích hoạt
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_skilldamageptrimbylesshp,

        /// <summary>
        /// Phục hồi sinh lực %
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_immediatereplbymaxstate_p,

        /// <summary>
        /// Tăng vật công cơ bản (điểm) dựa theo số lần ngoại công hiện tại
        /// <para>Value[0]: Giá trị</para>
        /// <para>Value[1], Value[2]: Vô nghĩa</para>
        /// </summary>
        magic_addweaponbasedamagetrimbyvitality,

        magic_end,
    };
}
