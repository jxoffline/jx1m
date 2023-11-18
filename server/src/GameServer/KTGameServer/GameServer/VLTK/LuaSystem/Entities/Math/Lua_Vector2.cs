using GameServer.KiemThe.Utilities;
using MoonSharp.Interpreter;

namespace GameServer.KiemThe.LuaSystem.Entities.Math
{
    /// <summary>
    /// Đối tượng biểu diễn Vector trên mặt phẳng 2 chiều
    /// </summary>
    [MoonSharpUserData]
    public class Lua_Vector2
    {
        /// <summary>
        /// Tọa độ X
        /// </summary>
        [MoonSharpHidden]
        public float X { get; set; }

        /// <summary>
        /// Tọa độ Y
        /// </summary>
        [MoonSharpHidden]
        public float Y { get; set; }

        /// <summary>
        /// Đối tượng UnityEngine.Vector2
        /// </summary>
        [MoonSharpHidden]
        public UnityEngine.Vector2 UnityVector2 { get; private set; }

        /// <summary>
        /// Tạo đối tượng từ UnityEngine.Vector2
        /// </summary>
        /// <param name="vector2"></param>
        /// <returns></returns>
        public static Lua_Vector2 FromUnityVector2(UnityEngine.Vector2 vector2)
        {
            return new Lua_Vector2(vector2.x, vector2.y);
        }

        /// <summary>
        /// Đối tượng biểu diễn Vector trên mặt phẳng 2 chiều
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public Lua_Vector2(float x, float y)
        {
            this.X = x;
            this.Y = y;
            this.UnityVector2 = new UnityEngine.Vector2(x, y);
        }

        /// <summary>
        /// Trả về tọa độ X
        /// </summary>
        /// <returns></returns>
        public float GetX()
        {
            return this.X;
        }

        /// <summary>
        /// Trả về tọa độ Y
        /// </summary>
        /// <returns></returns>
        public float GetY()
        {
            return this.Y;
        }

        /// <summary>
        /// Trả về độ dài Vector
        /// </summary>
        /// <returns></returns>
        public float Length()
        {
            return this.UnityVector2.magnitude;
        }

        /// <summary>
        /// Trả về khoảng cách giữa 2 điểm
        /// </summary>
        /// <param name="vector1"></param>
        /// <param name="vector2"></param>
        /// <returns></returns>
        public static float Distance(Lua_Vector2 vector1, Lua_Vector2 vector2)
        {
            if (vector1 is null || vector2 is null)
            {
                return -1;
            }
            return UnityEngine.Vector2.Distance(vector1.UnityVector2, vector2.UnityVector2);
        }

        /// <summary>
        /// Trả về điểm cách Vector1 1 đoạn tương ứng nằm trên Vector hướng từ vector1 đến vector2
        /// </summary>
        /// <param name="vector1"></param>
        /// <param name="vector2"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static Lua_Vector2 Lerp(Lua_Vector2 vector1, Lua_Vector2 vector2, float distance)
        {
            if (vector1 is null || vector2 is null)
            {
                return null;
            }

            UnityEngine.Vector2 dirVector = (vector2 - vector1).UnityVector2;
            UnityEngine.Vector2 destVector = KTMath.FindPointInVectorWithDistance(vector1.UnityVector2, dirVector, distance);
            return Lua_Vector2.FromUnityVector2(destVector);
        }

        /// <summary>
        /// Trả về Vector chỉ phương tạo bởi 2 điểm
        /// </summary>
        /// <param name="vector1"></param>
        /// <param name="vector2"></param>
        /// <returns></returns>
        public static Lua_Vector2 Direction(Lua_Vector2 vector1, Lua_Vector2 vector2)
        {
            if (vector1 is null || vector2 is null)
            {
                return null;
            }

            return Lua_Vector2.FromUnityVector2(vector2.UnityVector2 - vector1.UnityVector2);
        }

        public static Lua_Vector2 operator +(Lua_Vector2 vector1, Lua_Vector2 vector2)
        {
            if (vector1 is null || vector2 is null)
            {
                return null;
            }
            return Lua_Vector2.FromUnityVector2(vector1.UnityVector2 + vector2.UnityVector2);
        }

        public static Lua_Vector2 operator -(Lua_Vector2 vector1, Lua_Vector2 vector2)
        {
            if (vector1 is null || vector2 is null)
            {
                return null;
            }
            return Lua_Vector2.FromUnityVector2(vector1.UnityVector2 - vector2.UnityVector2);
        }

        public static Lua_Vector2 operator *(Lua_Vector2 vector1, float number)
        {
            if (vector1 is null || vector1 is null)
            {
                return null;
            }
            return Lua_Vector2.FromUnityVector2(vector1.UnityVector2 * number);
        }

        public static Lua_Vector2 operator /(Lua_Vector2 vector1, float number)
        {
            if (vector1 is null || vector1 is null)
            {
                return null;
            }
            return Lua_Vector2.FromUnityVector2(vector1.UnityVector2 / number);
        }

        public static bool operator ==(Lua_Vector2 vector1, Lua_Vector2 vector2)
        {
            if (vector1 is null && vector2 is null)
            {
                return true;
            }
            else if (vector1 is null || vector2 is null)
            {
                return false;
            }
            return vector1.UnityVector2 == vector2.UnityVector2;
        }

        public static bool operator !=(Lua_Vector2 vector1, Lua_Vector2 vector2)
        {
            if (vector1 is null && vector2 is null)
            {
                return false;
            }
            else if (vector1 is null || vector2 is null)
            {
                return true;
            }
            return vector1.UnityVector2 != vector2.UnityVector2;
        }

        /// <summary>
        /// So sánh bằng
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        [MoonSharpHidden]
        public override bool Equals(object o)
        {
            if (!(o is Lua_Vector2))
            {
                return false;
            }
            return (Lua_Vector2) o == this;
        }

        [MoonSharpHidden]
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
            return string.Format("Vector2({0}, {1})", this.X, this.Y);
        }
    }
}
