using System;
using System.Collections.Generic;
using UnityEngine;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK
{
    /// <summary>
    /// Thư viện hàm toán học
    /// </summary>
    public class KTMath
    {
        /// <summary>
        /// Đối tượng Vector2 dạng Short
        /// </summary>
        public struct Vector2Short
        {
            /// <summary>
            /// Tọa độ X
            /// </summary>
            public short X { get; private set; }

            /// <summary>
            /// Tọa độ Y
            /// </summary>
            public short Y { get; private set; }

            /// <summary>
            /// Đối tượng Vector2 dạng Short
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            public Vector2Short(short x, short y)
            {
                this.X = x;
                this.Y = y;
                this.Vector2 = new Vector2(this.X, this.Y);
            }

            /// <summary>
            /// Đối tượng UnityEngine.Vector2
            /// </summary>
            private Vector2 Vector2;

            /// <summary>
            /// Chuyển đối tượng về dạng UnityEngine.Vector2
            /// </summary>
            /// <returns></returns>
            public Vector2 ToVector2()
            {
                return this.Vector2;
            }
        }

        /// <summary>
        /// Đối tượng kích thước
        /// </summary>
        public struct Size
        {
            /// <summary>
            /// Chiều rộng
            /// </summary>
            public int Width { get; set; }

            /// <summary>
            /// Chiều cao
            /// </summary>
            public int Height { get; set; }

            /// <summary>
            /// Đối tượng kích thước
            /// </summary>
            /// <param name="width"></param>
            /// <param name="height"></param>
            public Size(int width, int height)
            {
                this.Width = width;
                this.Height = height;
            }

            /// <summary>
            /// Chuyển đối tượng về dạng String
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return string.Format("({0} x {1})", this.Width, this.Height);
            }
        }

        /// <summary>
        /// Đối tượng điểm trên mặt phẳng
        /// </summary>
        public struct Point2D
        {
            /// <summary>
            /// Tọa độ X
            /// </summary>
            public int X { get; set; }

            /// <summary>
            /// Tọa độ Y
            /// </summary>
            public int Y { get; set; }

            /// <summary>
            /// Đối tượng điểm trên mặt phẳng
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            public Point2D(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }

            /// <summary>
            /// Chuyển đối tượng về dạng String
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return string.Format("({0}, {1})", this.X, this.Y);
            }

            /// <summary>
            /// Chuyển đối tượng sang UnityEngine.Vector2
            /// </summary>
            /// <returns></returns>
            public Vector2 ToVector2()
            {
                return new Vector2(this.X, this.Y);
            }

            /// <summary>
            /// Tạo đối tượng từ UnityEngine.Vector2
            /// </summary>
            /// <param name="vector2"></param>
            /// <returns></returns>
            public static Point2D FromVector2(Vector2 vector2)
            {
                return new Point2D((int)vector2.x, (int)vector2.y);
            }

            public static Point2D operator +(Point2D p1, Point2D p2)
            {
                return new Point2D(p1.X + p2.X, p1.Y + p2.Y);
            }

            public static Point2D operator -(Point2D p1, Point2D p2)
            {
                return new Point2D(p1.X + p2.X, p1.Y + p2.Y);
            }

            public static Point2D operator *(Point2D p1, int alpha)
            {
                return new Point2D(p1.X * alpha, p1.Y * alpha);
            }

            public static Point2D operator /(Point2D p1, int alpha)
            {
                return new Point2D(p1.X / alpha, p1.Y / alpha);
            }

            public static bool operator ==(Point2D p1, Point2D p2)
            {
                return p1.X == p2.X && p1.Y == p2.Y;
            }

            public static bool operator !=(Point2D p1, Point2D p2)
            {
                return p1.X != p2.X || p1.Y != p2.Y;
            }

            public override bool Equals(object point)
            {
                if (!(point is Point2D))
                {
                    return false;
                }
                return (Point2D)point == this;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }

        /// <summary>
        /// Đối tượng đường thẳng Ax + By + C = 0
        /// </summary>
        public class Line
        {
            /// <summary>
            /// Tham số A
            /// </summary>
            public float A { get; set; }

            /// <summary>
            /// Tham số B
            /// </summary>
            public float B { get; set; }

            /// <summary>
            /// Tham số C
            /// </summary>
            public float C { get; set; }

            /// <summary>
            /// Chuyển đối tượng thành biểu diễn string
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return this.A + "x" + " + " + this.B + "y" + " + " + this.C + " = " + "0";
            }
        }

        /// <summary>
        /// Phương trình đường Parabol
        /// <para>(P): y = Ax² + Bx + C</para>
        /// </summary>
        public class Parabol
        {
            /// <summary>
            /// Tham số A
            /// </summary>
            public float A { get; set; }

            /// <summary>
            /// Tham số B
            /// </summary>
            public float B { get; set; }

            /// <summary>
            /// Tham số C
            /// </summary>
            public float C { get; set; }

            /// <summary>
            /// Tọa độ đỉnh của Parabol
            /// </summary>
            public Vector2 Vertex
            {
                get
                {
                    float x = -this.B / (2 * this.A);
                    float y = this.A * x * x + this.B * x + this.C;
                    return new Vector2(x, y);
                }
            }

            /// <summary>
            /// Hướng quay của Parabol có phải hướng lên trên không
            /// </summary>
            public bool IsUp
            {
                get
                {
                    return this.A > 0;
                }
            }

            /// <summary>
            /// Dạng chuỗi biểu diễn Parabol
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return string.Format("P: y = {0}x² + {1}x + {2}", this.A, this.B, this.C);
            }
        }

        /// <summary>
        /// Tìm phương trình đường Parabol từ tọa độ đỉnh và một điểm thuộc Parabol
        /// <para>(P): y = Ax² + Bx + C</para>
        /// </summary>
        /// <param name="vertex"></param>
        /// <returns></returns>
        public static Parabol GetParabolFromVertexAndPoint(Vector2 vertex, Vector2 point)
        {
            Parabol parabol = new Parabol();

            float a = (point.y - vertex.y) / ((point.x - vertex.x) * (point.x - vertex.x));
            float b = -2 * a * vertex.x;
            float c = a * vertex.x * vertex.x + vertex.y;

            parabol.A = a;
            parabol.B = b;
            parabol.C = c;

            return parabol;
        }

        /// <summary>
        /// Trả về phương trình đường Parabol tương ứng, đi qua 2 điểm P0 và P1, có tung độ đỉnh Y = k
        /// <para>(P): y = Ax² + Bx + C</para>
        /// </summary>
        /// <param name="p0">Điểm số 1</param>
        /// <param name="p1">Điểm số 2</param>
        /// <param name="k">Tung độ đỉnh</param>
        /// <param name="isMiddle">Parabol đỉnh nằm giữa 2 điểm p0 và p1</param>
        /// <returns></returns>
        public static Parabol GetParabolFromTwoPointsAndVertexY(Vector2 p0, Vector2 p1, float k, bool isMiddle)
        {
            /// Nếu 2 điểm p0 và p1 có chung X
            if (p0.x == p1.x)
            {
                return null;
            }
            /// Nếu 2 điểm p0 và p1 khác X
            else
            {
                /// <summary>
                /// Trả về phương trình đường Parabol tương ứng, với tọa độ X của đỉnh
                /// </summary>
                /// <param name="h">Tọa độ X của đỉnh Parabol</param>
                /// <returns></returns>
                Parabol GetFromVertexX(float h)
                {
                    Parabol parabol = new Parabol();

                    float a = (p1.y - p0.y) / (p1.x * p1.x - p0.x * p0.x + 2 * p0.x * h - 2 * p1.x * h);
                    float b = -2 * a * h;
                    float c = k + a * h * h;

                    parabol.A = a;
                    parabol.B = b;
                    parabol.C = c;

                    return parabol;
                }

                /// Quy về giải phương trình bậc 2 tìm H
                float pA = p1.y - p0.y;
                float pB = 2 * p0.x * k - 2 * p1.x * k - 2 * p0.x * (p1.y - p0.y) - 2 * p0.x * p0.y + 2 * p1.x * p0.y;
                float pC = (p1.y - p0.y) * p0.x * p0.x + k * p1.x * p1.x - k * p0.x * p0.x - p1.x * p1.x * p0.y + p0.x * p0.x * p0.y;

                /// Nếu A = 0 thì có 1 điểm duy nhất có tọa độ X thỏa mãn phương trình đường thẳng
                if (pA == 0)
                {
                    float h = -pC / pB;

                    Parabol pb = GetParabolFromVertexAndPoint(new UnityEngine.Vector2(h, k), p0);
                    return pb;
                }
                /// Nếu A ~= 0 thì giải phương trình bậc 2 tìm tọa độ của đỉnh thay vào tìm Parabol
                else
                {
                    /// Giải phương trình bậc 2 trên
                    float delta = pB * pB - 4 * pA * pC;

                    /// Nếu delta < 0 thì không có điểm nào thỏa mãn
                    if (delta < 0)
                    {
                        return null;
                    }
                    /// Nếu delta = 0 thì chỉ có một đỉnh duy nhất, và đỉnh đó trùng với một trong 2 đỉnh ban đầu, xảy ra trong trường hợp k = tọa độ Y của một trong 2 đỉnh
                    else if (delta == 0)
                    {
                        float h = -pB / (2 * pA);

                        Parabol pb = GetFromVertexX(h);
                        return pb;
                    }
                    /// Nếu delta > 0 thì có 2 đỉnh
                    else
                    {
                        float h1 = (-pB + Mathf.Sqrt(delta)) / (2 * pA);
                        float h2 = (-pB - Mathf.Sqrt(delta)) / (2 * pA);

                        float minX = Mathf.Min(p0.x, p1.x);
                        float maxX = Mathf.Max(p0.x, p1.x);

                        /// Nếu tọa độ X của điểm h1 nằm giữa tọa độ X của 2 điểm
                        if (minX <= h1 && h1 <= maxX)
                        {
                            /// Nếu lấy điểm ở giữa làm đỉnh parabol
                            if (isMiddle)
                            {
                                Parabol pb = GetFromVertexX(h1);
                                return pb;
                            }
                            /// Nếu lấy đỉnh ở ngoài làm đỉnh parabol
                            else
                            {
                                Parabol pb = GetFromVertexX(h2);
                                return pb;
                            }
                        }
                        /// Nếu tọa độ X của điểm h2 nằm giữa tọa độ X của 2 điểm
                        else if (minX <= h2 && h2 <= maxX)
                        {
                            /// Nếu lấy điểm ở giữa làm đỉnh parabol
                            if (isMiddle)
                            {
                                Parabol pb = GetFromVertexX(h2);
                                return pb;
                            }
                            /// Nếu lấy đỉnh ở ngoài làm đỉnh parabol
                            else
                            {
                                Parabol pb = GetFromVertexX(h1);
                                return pb;
                            }
                        }
                        /// Sẽ có cái ngoại lệ nào đó thì mới xuống đến đây
                        else
                        {
                            return null;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Lấy điểm ngẫu nhiên ở trên đường tròn tâm tương ứng
        /// </summary>
        /// <param name="position">Tọa độ tâm đường tròn cho trước</param>
        /// <param name="radius">Bán kính</param>
        /// <returns></returns>
        public static Vector2 GetRandomPointInCircle(Vector2 position, float radius)
        {
            float randomAngle = UnityEngine.Random.Range(0, 360);

            Vector2 oxVector = KTMath.RotateVector(Vector2.right, randomAngle);
            Vector2 randomPoint = KTMath.FindPointInVectorWithDistance(position, position + oxVector, radius);

            return randomPoint;
        }

        /// <summary>
        /// Lấy điểm ngẫu nhiên ở trên đường tròn tâm tương ứng
        /// </summary>
        /// <param name="position">Tọa độ tâm đường tròn cho trước</param>
        /// <param name="radius">Bán kính</param>
        /// <param name="totalPoints">Tổng số điểm</param>
        /// <returns></returns>
        public static List<Vector2> GetRandomPointInCircle(Vector2 position, float radius, int totalPoints)
        {
            List<Vector2> points = new List<Vector2>();

            for (int i = 1; i <= totalPoints; i++)
            {
                points.Add(KTMath.GetRandomPointInCircle(position, radius));
            }

            return points;
        }

        /// <summary>
        /// Lấy điểm ngẫu nhiên ở xung quanh điểm cho trước
        /// </summary>
        /// <param name="position">Tọa độ điểm cho trước</param>
        /// <param name="radius">Phạm vi</param>
        /// <returns></returns>
        public static Vector2 GetRandomPointAroundPos(Vector2 position, float radius)
        {
            float x = position.x + UnityEngine.Random.Range(0f, radius) - UnityEngine.Random.Range(0f, radius);
            float y = position.y + UnityEngine.Random.Range(0f, radius) - UnityEngine.Random.Range(0f, radius);
            return new Vector2(x, y);
        }

        /// <summary>
        /// Lấy danh sách các điểm ngẫu nhiên ở xung quanh điểm cho trước
        /// </summary>
        /// <param name="position">Tọa độ điểm cho trước</param>
        /// <param name="radius">Phạm vi</param>
        /// <param name="totalPoints">Tổng số điểm</param>
        /// <returns></returns>
        public static List<Vector2> GetRandomPointsAroundPos(Vector2 position, float radius, int totalPoints)
        {
            List<Vector2> points = new List<Vector2>();

            for (int i = 1; i <= totalPoints; i++)
            {
                points.Add(KTMath.GetRandomPointAroundPos(position, radius));
            }

            return points;
        }

        /// <summary>
		/// Lấy danh sách các điểm nằm trên Parabol ở giữa 2 điểm cho trước
		/// <para>Tối thiểu phải có 2 điểm, gồm 2 điểm cho trước và các điểm khác ở giữa</para>
		/// </summary>
		/// <param name="fromPos">Tọa độ điểm 1</param>
		/// <param name="toPos">Tọa độ điểm 2</param>
		/// <param name="p">Parabol</param>
		/// <param name="totalPoints">Tổng số điểm</param>
		/// <returns>Danh sách các điểm thuộc Parabol</returns>
		public static List<Vector2> GetPointsBetweenTwoPointsOnParabol(int vertexY, Vector2 fromPos, Vector2 toPos, int totalPoints)
        {
            List<Vector2> flyPoints = new List<Vector2>();

            if (totalPoints == 1)
            {
                return flyPoints;
            }

            Vector2 fromWorldPos = fromPos;
            Vector2 toWorldPos = toPos;

            /// Phương trình đường Parabol đi qua 2 điểm fromWorldPos và toWorldPos có tọa độ Y của đỉnh Y = vertexY
            Parabol p = KTMath.GetParabolFromTwoPointsAndVertexY(fromWorldPos, toWorldPos, vertexY, true);

            Vector2 dirVector = toWorldPos - fromWorldPos;

            /// Độ cao
            float height = Mathf.Max(vertexY - fromWorldPos.y, vertexY - toWorldPos.y);

            /// Thêm điểm đầu
            flyPoints.Add(fromWorldPos);

            /// Khoảng cách giữa 2 điểm
            float distance = Vector2.Distance(fromWorldPos, toWorldPos);

            /// Khoảng cách giữa các điểm con
            float deltaD = distance / (totalPoints - 1);

            for (int i = 3; i <= totalPoints; i++)
            {
                /// Điểm tiếp theo trên đường đi
                Vector2 nextPos = KTMath.FindPointInVectorWithDistance(fromWorldPos, dirVector, deltaD * (i - 2));

                /// Nếu có Parabol
                if (p != null)
                {
                    float newY = p.A * nextPos.x * nextPos.x + p.B * nextPos.x + p.C;
                    nextPos.y = newY;
                }
                /// Nếu không có Parabol
                else
                {
                    float percent = (i - 1) / (float)totalPoints;
                    float newY;
                    /// Nếu là pha nhảy lên
                    if (percent <= 0.5f)
                    {
                        float newHeight = percent * 2 * height;
                        newY = nextPos.y + newHeight;
                    }
                    /// Nếu là pha đáp xuống
                    else
                    {
                        float newHeight = (1 - (percent - 0.5f) * 2) * height;
                        newY = nextPos.y + newHeight;
                    }
                    nextPos.y = newY;
                }

                flyPoints.Add(nextPos);
            }

            /// Thêm điểm cuối
            flyPoints.Add(toWorldPos);

            /// Trả ra kết quả
            return flyPoints;
        }

        /// <summary>
        /// Trả về góc giữa 2 vector
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static float GetAngleBetweenVector(Vector2 pos, Vector2 target)
        {
            Vector2 diference = target - pos;
            float sign = (target.y < pos.y) ? -1.0f : 1.0f;
            return Vector2.Angle(Vector2.right, diference) * sign;
        }

        /// <summary>
        /// Trả về điểm nằm trên đường thẳng cách điểm cho trước 1 khoảng
        /// </summary>
        /// <param name="point"></param>
        /// <param name="line"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static KeyValuePair<Vector2, Vector2> FindPointInLineWithDistance(Vector2 point, Line line, float distance)
        {
            Vector2 point1 = new Vector2();
            Vector2 point2 = new Vector2();
            if (line.B != 0)
            {
                point1.x = distance / Mathf.Sqrt(1 + (line.A * line.A) / (line.B * line.B)) + point.x;
                point1.y = (-line.A * point1.x - line.C) / line.B;
                point2.x = -distance / Mathf.Sqrt(1 + (line.B * line.B) / (line.B * line.B)) + point.x;
                point2.y = (-line.A * point2.x - line.C) / line.B;
            }
            else
            {
                point1.y = distance + point.y;
                point1.x = (-line.B * point1.y - line.C) / line.A;
                point2.y = -distance + point.y;
                point2.x = (-line.B * point2.y - line.C) / line.A;
            }
            return new KeyValuePair<Vector2, Vector2>(point1, point2);
        }

        /// <summary>
        /// Trả về điểm nằm trên Vector cách điểm cho trước một khoảng
        /// </summary>
        /// <param name="point">Điểm</param>
        /// <param name="dirVector">Vectỏ chỉ phương</param>
        /// <param name="distance">Khoảng cách</param>
        /// <returns></returns>
        public static Vector2 FindPointInVectorWithDistance(Vector2 point, Vector2 dirVector, float distance)
        {
            Vector2 pos = point + dirVector.normalized * distance;
            return pos;
        }

        /// <summary>
        /// Trả về đường thẳng dựa vào Vector pháp tuyến
        /// </summary>
        /// <param name="point"></param>
        /// <param name="normalVector"></param>
        /// <returns></returns>
        public static Line GetLineFromNormalVector(Vector2 point, Vector2 normalVector)
        {
            Line line = new Line();
            line.A = normalVector.x;
            line.B = normalVector.y;
            line.C = -line.A * point.x - line.B * point.y;
            return line;
        }

        /// <summary>
        /// Trả về đường thẳng dựa vào Vector chỉ phương
        /// </summary>
        /// <param name="point"></param>
        /// <param name="dirVector"></param>
        /// <returns></returns>
        public static Line GetLineFromDirectionalVector(Vector2 point, Vector2 dirVector)
        {
            Line line = new Line();
            line.A = -dirVector.y;
            line.B = dirVector.x;
            line.C = -line.A * point.x - line.B * point.y;
            return line;
        }

        /// <summary>
        /// Tìm điểm nằm giữa 2 điểm p1 và p2 với khoảng cách với điểm p2 cho trước
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static Vector2 FindPointBetweenTwoPoints(Vector2 p1, Vector2 p2, float distance)
        {
            float d = Vector2.Distance(p1, p2);
            float dt = distance;
            float t = (d - dt) / d;
            return new Vector2((p1.x + (p2.x - p1.x) * t), (p1.y + (p2.y - p1.y) * t));
        }

        /// <summary>
        /// Quay Vector
        /// </summary>
        /// <param name="v">Vector</param>
        /// <param name="degrees">Độ</param>
        /// <returns></returns>
        public static Vector2 RotateVector(Vector2 v, float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            float sin = Mathf.Sin(radians);
            float cos = Mathf.Cos(radians);

            float tx = v.x;
            float ty = v.y;

            return new Vector2(cos * tx - sin * ty, sin * tx + cos * ty);
        }

        /// <summary>
        /// Di chuyển thẳng lên phía trước theo hướng quay
        /// </summary>
        /// <param name="fromPos"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static Vector2 MoveTowardByDirection(Vector2 fromPos, Direction direction, float distance)
        {
            Vector2 dirVector = KTMath.DirectionToDirVector(direction);
            return KTMath.FindPointInVectorWithDistance(fromPos, dirVector, distance);
        }

        /// <summary>
        /// Chuyển hướng quay sang Vector chỉ hướng
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static Vector2 DirectionToDirVector(Direction direction)
        {
            Vector2 dirVector = Vector2.zero;
            switch (direction)
            {
                case Direction.DOWN:
                    dirVector = new Vector2(0, -1);
                    break;

                case Direction.DOWN_LEFT:
                    dirVector = new Vector2(-1, -1);
                    break;

                case Direction.LEFT:
                    dirVector = new Vector2(-1, 0);
                    break;

                case Direction.UP_LEFT:
                    dirVector = new Vector2(-1, 1);
                    break;

                case Direction.UP:
                    dirVector = new Vector2(0, 1);
                    break;

                case Direction.UP_RIGHT:
                    dirVector = new Vector2(1, 1);
                    break;

                case Direction.RIGHT:
                    dirVector = new Vector2(1, 0);
                    break;

                case Direction.DOWN_RIGHT:
                    dirVector = new Vector2(1, -1);
                    break;
            }
            return dirVector;
        }

        /// <summary>
        /// Trả về góc quay giữa 2 Vector
        /// </summary>
        /// <param name="p1">Vector số 1</param>
        /// <param name="p2">Vector số 2</param>
        /// <param name="o">Điểm bắt đầu của Vector</param>
        /// <returns>Góc quay trong đoạn 0° và 360°.</returns>
        public static float GetAngle360BetweenVectors(Vector2 p1, Vector2 p2, Vector2 o = default(Vector2))
        {
            Vector2 v1, v2;
            if (o == default(Vector2))
            {
                v1 = p1.normalized;
                v2 = p2.normalized;
            }
            else
            {
                v1 = (p1 - o).normalized;
                v2 = (p2 - o).normalized;
            }
            float angle = Vector2.Angle(v1, v2);
            return Mathf.Sign(Vector3.Cross(v1, v2).z) < 0 ? (360 - angle) % 360 : angle;
        }

        /// <summary>
        /// Trả về góc quay giữa Vector với trục Ox
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static float GetAngle360WithXAxis(Vector2 vector)
        {
            float angle = Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg;
            if (angle < 0)
            {
                angle += 360;
            }
            return angle;
        }

        /// <summary>
        /// Trả về hướng quay (8 hướng) theo độ lớn góc quay tính theo trục Ox, chiều kim KNB hồ
        /// </summary>
        /// <param name="degree">Góc quay 360° tính theo trục Ox</param>
        /// <returns></returns>
        public static Direction GetDirectionByAngle360(float degree)
        {
            Direction dir;
            if (degree >= 22.5f && degree < 67.5f)
            {
                dir = Direction.UP_RIGHT;
            }
            else if (degree >= 67.5f && degree < 112.5f)
            {
                dir = Direction.UP;
            }
            else if (degree >= 112.5f && degree < 157.5f)
            {
                dir = Direction.UP_LEFT;
            }
            else if (degree >= 157.5f && degree < 202.5f)
            {
                dir = Direction.LEFT;
            }
            else if (degree >= 202.5f && degree < 247.5f)
            {
                dir = Direction.DOWN_LEFT;
            }
            else if (degree >= 247.5f && degree < 292.5f)
            {
                dir = Direction.DOWN;
            }
            else if (degree >= 292.5f && degree < 337.5f)
            {
                dir = Direction.DOWN_RIGHT;
            }
            else
            {
                dir = Direction.RIGHT;
            }
            return dir;
        }




        /// <summary>
        /// Kiểm tra điểm có nằm trong hình tròn không
        /// </summary>
        /// <param name="point">Điểm</param>
        /// <param name="circlePos">Tâm hình tròn</param>
        /// <param name="radius">Bán kính</param>
        /// <returns></returns>
        public static bool IsPointInsideCircle(Vector2 point, Vector2 circlePos, float radius)
        {
            return Vector2.Distance(circlePos, point) <= radius;
        }

        /// <summary>
        /// Kiểm tra 1 điểm có nằm trong nửa hình tròn tạo bởi Vector cho trước qua tâm không
        /// </summary>
        /// <param name="point">Điểm</param>
        /// <param name="centerPoint">Tâm hình tròn</param>
        /// <param name="dirVector">Vector 0 độ</param>
        /// <param name="radius">Bán kính hình tròn</param>
        /// <returns></returns>
        public static bool IsPointInsideSemiCircle(Vector2 point, Vector2 centerPoint, Vector2 dirVector, float radius)
        {
            if (!KTMath.IsPointInsideCircle(point, centerPoint, radius))
            {
                return false;
            }

            Vector2 pVector = point - centerPoint;

            return pVector.x * dirVector.x + pVector.y * dirVector.y >= 0;
        }

        /// <summary>
        /// Kiểm tra điểm pt có nằm trong tam giác tạo bởi 3 đỉnh cho trước không
        /// </summary>
        /// <param name="pt">Điểm</param>
        /// <param name="v1">Đỉnh tam giác số 1</param>
        /// <param name="v2">Đỉnh tam giác số 2</param>
        /// <param name="v3">Đỉnh tam giác số 3</param>
        /// <returns></returns>
        public static bool IsPointInsideTriagle(Vector2 pt, Vector2 v1, Vector2 v2, Vector2 v3)
        {
            double Sign(Vector2 p1, Vector2 p2, Vector2 p3)
            {
                return (p1.x - p3.x) * (p2.y - p3.y) * (p2.x - p3.x) * (p1.y - p3.y);
            }

            bool b1 = Sign(pt, v1, v2) < 0.0f;
            bool b2 = Sign(pt, v2, v3) < 0.0f;
            bool b3 = Sign(pt, v3, v1) < 0.0f;

            return (b1 == b2 && b2 == b3);
        }

        /// <summary>
        /// Kiểm tra điểm có nằm trong hình chữ nhật tạo bởi 4 đỉnh cho trước không
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static bool IsPointInsideRectangle(Vector2 pt, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            return KTMath.IsPointInsideTriagle(pt, a, b, c) && KTMath.IsPointInsideTriagle(pt, a, d, c);
        }

        /// <summary>
        /// Kiểm tra điểm có nằm phía nào của đoạn thẳng tạo bởi 2 điểm p1 và p2 không
        /// <para>Phía trái tức ở gần điểm p1 có giá trị -1</para>
        /// <para>Phía phải tức ở gần điểm p2 có giá trị 1</para>
        /// <para>Nếu nằm trong đoạn thẳng thì có giá trị 0</para>
        /// </summary>
        /// <param name="pt">Điểm</param>
        /// <param name="p1">Tọa độ đầu mút 1 của đoạn thẳng</param>
        /// <param name="p2">Tọa độ đầu mút 2 của đoạn thẳng</param>
        /// <returns></returns>
        public static int GetSideOfPointInParagraph(Vector2 pt, Vector2 p1, Vector2 p2)
        {
            if (KTMath.IsPointInParagraph(pt, p1, p2))
            {
                return 0;
            }
            float distanceToP1 = Vector2.Distance(pt, p1);
            float distanceToP2 = Vector2.Distance(pt, p2);

            if (distanceToP1 < distanceToP2)
            {
                return -1;
            }
            else
            {
                return 1;
            }
        }

        /// <summary>
        /// Kiểm tra điểm có nằm trên đường thẳng không
        /// </summary>
        /// <param name="point">Điểm</param>
        /// <param name="line">Đường thẳng</param>
        /// <returns></returns>
        public static bool IsPointInLine(Vector2 point, Line line)
        {
            return line.A * point.x + line.B * point.y + line.C == 0;
        }

        /// <summary>
        /// Kiểm tra điểm có nằm trong đoạn thẳng không
        /// </summary>
        /// <param name="pt">Điểm</param>
        /// <param name="p1">Tọa độ đầu mút 1 của đoạn thẳng</param>
        /// <param name="p2">Tọa độ đầu mút 2 của đoạn thẳng</param>
        /// <returns></returns>
        public static bool IsPointInParagraph(Vector2 pt, Vector2 p1, Vector2 p2)
        {
            return Vector2.Distance(pt, p1) + Vector2.Distance(pt, p2) - Vector2.Distance(p1, p2) <= 0.1f;
        }

        /// <summary>
        /// Trả về điểm là hình chiếu vuông góc của điểm cho trước lên đường thẳng
        /// </summary>
        /// <param name="point">Điểm</param>
        /// <param name="line">Đường thẳng</param>
        /// <returns></returns>
        public static Vector2 GetPerpendicularProjectionOfPointInLine(Vector2 point, Line line)
        {
            float x = (line.B * (line.B * point.x - line.A * point.y) - line.A * line.C) / (line.A * line.A + line.B * line.B);
            float y = (line.A * (-line.B * point.x + line.A * point.y) - line.B * line.C) / (line.A * line.A + line.B * line.B);

            return new Vector2(x, y);
        }

        /// <summary>
        /// Trả về khoảng cách từ điểm đến đường thẳng
        /// </summary>
        /// <param name="point">Điểm</param>
        /// <param name="line">Đường thẳng</param>
        /// <returns></returns>
        public static float GetDistanceOfPointToLine(Vector2 point, Line line)
        {
            float up = Mathf.Abs(line.A * point.x + line.B * point.y + line.C);
            float down = Mathf.Sqrt(line.A * line.A + line.B * line.B);

            return up / down;
        }


        #region Geometry
        /// <summary>
        /// Trả về tập hợp các điểm trên cung tròn hướng tương ứng
        /// </summary>
        /// <param name="rootPoint">Tọa độ gốc</param>
        /// <param name="otherPoint">Điểm khác</param>
        /// <param name="radius">Bán kính quạt</param>
        /// <param name="degree">Góc của cung tròn</param>
        /// <param name="totalPoints">Tổng số điểm</param>
        /// <returns></returns>
        public static List<Vector2> GetArcPointsFromVector(Vector2 rootPoint, Vector2 dirVector, float radius, int degree, int totalPoints)
        {
            List<Vector2> points = new List<Vector2>();
            float degreeEach = totalPoints <= 1 ? 0 : (float)degree / (totalPoints - 1);

            if (totalPoints % 2 != 0)
            {
                Vector2 p = KTMath.FindPointInVectorWithDistance(rootPoint, dirVector, radius);
                points.Add(p);
            }

            for (int i = 1; i <= totalPoints / 2; i++)
            {
                Vector2 leftVector;
                Vector2 rightVector;

                if (totalPoints % 2 == 0)
                {
                    leftVector = KTMath.RotateVector(dirVector, degreeEach / 2 + degreeEach * (i - 1));
                    rightVector = KTMath.RotateVector(dirVector, 360 - (degreeEach / 2 + degreeEach * (i - 1)));
                }
                else
                {
                    leftVector = KTMath.RotateVector(dirVector, degreeEach * (i - 1));
                    rightVector = KTMath.RotateVector(dirVector, 360 - (degreeEach * (i - 1)));
                }

                Vector2 p1 = KTMath.FindPointInVectorWithDistance(rootPoint, leftVector, radius);
                Vector2 p2 = KTMath.FindPointInVectorWithDistance(rootPoint, rightVector, radius);

                points.Add(p1);
                points.Add(p2);
            }

            return points;
        }

        /// <summary>
        /// Trả về tập hợp các điểm trên đường tròn
        /// </summary>
        /// <param name="rootPoint">Tọa độ gốc</param>
        /// <param name="radius">Bán kính</param>
        /// <param name="totalPoints">Tổng số điểm</param>
        /// <returns></returns>
        public static List<Vector2> GetCirclePoints(Vector2 rootPoint, float radius, int totalPoints)
        {
            List<Vector2> points = new List<Vector2>();
            float degreeEach = totalPoints <= 0 ? 0 : 360 / totalPoints;

            for (int i = 1; i <= totalPoints; i++)
            {
                Vector2 iVector = KTMath.RotateVector(Vector2.up, degreeEach * i);
                Vector2 pt = KTMath.FindPointInVectorWithDistance(rootPoint, iVector, radius);
                points.Add(pt);
            }

            return points;
        }

        /// <summary>
        /// Trả về tập hợp các điểm trên đường tròn hướng tại Vector cho trước
        /// </summary>
        /// <param name="rootPoint">Tọa độ gốc</param>
        /// <param name="dirVector">Vector hướng ban đầu</param>
        /// <param name="radius">Bán kính</param>
        /// <param name="totalPoints">Tổng số điểm</param>
        /// <returns></returns>
        public static List<Vector2> GetCirclePoints(Vector2 rootPoint, Vector2 dirVector, float radius, int totalPoints)
        {
            List<Vector2> points = new List<Vector2>();
            float degreeEach = totalPoints <= 0 ? 0 : 360 / totalPoints;

            for (int i = 1; i <= totalPoints; i++)
            {
                Vector2 iVector = KTMath.RotateVector(dirVector, degreeEach * i);
                Vector2 pt = KTMath.FindPointInVectorWithDistance(rootPoint, iVector, radius);
                points.Add(pt);
            }

            return points;
        }

        /// <summary>
        /// Trả về tập hợp các điểm nằm trên đoạn thẳng có trung điểm cho trước
        /// </summary>
        /// <param name="line">Đường thẳng</param>
        /// <param name="centerPoint">Tọa độ trung điểm</param>
        /// <param name="distance">Độ dài đoạn thẳng</param>
        /// <param name="totalPoints">Tổng số điểm</param>
        /// <returns></returns>
        public static List<Vector2> GetPointsInParagraphWithDistance(Line line, Vector2 centerPoint, int distance, int totalPoints)
        {
            List<Vector2> points = new List<Vector2>();

            float deltaD = distance / totalPoints;
            if (totalPoints % 2 != 0)
            {
                points.Add(new Vector2(centerPoint.x, centerPoint.y));
            }
            for (int i = 1; i <= totalPoints / 2; i++)
            {
                KeyValuePair<Vector2, Vector2> iPoints = KTMath.FindPointInLineWithDistance(centerPoint, line, deltaD * i);

                points.Add(iPoints.Key);
                points.Add(iPoints.Value);
            }

            return points;
        }

        /// <summary>
        /// Trả về tập hợp các điểm nằm trên 2 cạnh của tam giác cân tạo bởi 2 điểm p1 và p2 là đường cao, với độ dài cạnh đáy
        /// </summary>
        /// <param name="p1">Điểm P1</param>
        /// <param name="p2">Điểm P2</param>
        /// <param name="length">Độ dài cạnh đáy</param>
        /// <param name="totalPoints">Tổng số điểm</param>
        /// <returns></returns>
        public static List<Vector2> GetPointsInEdgesOfIsoTriangleFromVectorWithLength(Vector2 p1, Vector2 p2, int length, int totalPoints)
        {
            List<Vector2> points = new List<Vector2>();

            // Vector hướng của p1 tới p2
            Vector2 dirVector = p2 - p1;
            // Đường thẳng nhận Vector hướng p1 tơi p2 làm pháp tuyến
            Line line = KTMath.GetLineFromNormalVector(p1, dirVector);
            // Tìm 2 điểm đầu cạnh đáy của tam giác cân
            KeyValuePair<Vector2, Vector2> triaglePoints = KTMath.FindPointInLineWithDistance(p1, line, length);

            // Độ dài đường cao của tam giác
            float height = Vector2.Distance(p2, p1);
            // Độ dài cạnh bên của tam giác
            float edgeLength = Mathf.Sqrt((length / 2) * (length / 2) + height * height);
            // Các điểm sẽ nằm trên đoạn từ 2 đầu của cạnh đáy tam giác cân đến điểm p2
            float deltaD = edgeLength / (totalPoints / 2);
            if (totalPoints % 2 != 0)
            {
                points.Add(new Vector2(p2.x, p2.y));
            }

            Vector2 edgeDirVector1 = triaglePoints.Key - p2;
            Vector2 edgeDirVector2 = triaglePoints.Value - p2;
            for (int i = 1; i <= totalPoints / 2; i++)
            {
                Vector2 iP1 = KTMath.FindPointInVectorWithDistance(p2, edgeDirVector1, deltaD * i);
                Vector2 iP2 = KTMath.FindPointInVectorWithDistance(p2, edgeDirVector2, deltaD * i);

                points.Add(iP1);
                points.Add(iP2);
            }

            return points;
        }

        /// <summary>
        /// Vẽ tập hợp tọa độ tâm các hình tròn xung quanh điểm cho trước, tập hợp tạo thành hình chữ nhật
        /// </summary>
        /// <param name="point">Điểm trung tâm</param>
        /// <param name="widthCount">Tổng số hình theo chiều ngang</param>
        /// <param name="heightCount">Tổng số hình theo chiều dọc</param>
        /// <param name="radius">Bán kính hình tròn</param>
        /// <returns></returns>
        public static List<Vector2> DrawCirclesAroundPositionByRectangle(Vector2 point, int widthCount, int heightCount, float radius)
        {
            List<Vector2> points = new List<Vector2>();

            for (int i = 1; i <= widthCount / 2; i++)
            {
                float x, x1;
                if (widthCount % 2 == 0)
                {
                    x = point.x - radius - (widthCount / 2 - i) * 2 * radius;
                    x1 = point.x + radius + (widthCount / 2 - i) * 2 * radius;
                }
                else
                {
                    x = point.x - (widthCount / 2 - i + 1) * 2 * radius;
                    x1 = point.x + (widthCount / 2 - i + 1) * 2 * radius;
                }

                for (int j = 1; j <= heightCount / 2; j++)
                {
                    float y, y1;
                    if (heightCount % 2 == 0)
                    {
                        y = point.y - radius - (heightCount / 2 - j) * 2 * radius;
                        y1 = point.y + radius + (heightCount / 2 - j) * 2 * radius;
                    }
                    else
                    {
                        y = point.y - (heightCount / 2 - j + 1) * 2 * radius;
                        y1 = point.y + (heightCount / 2 - j + 1) * 2 * radius;
                    }

                    if (x == x1)
                    {
                        if (y == y1)
                        {
                            points.Add(new Vector2(x, y));
                        }
                        else
                        {
                            points.Add(new Vector2(x, y));
                            points.Add(new Vector2(x, y1));
                        }
                    }
                    else if (y == y1)
                    {
                        if (x == x1)
                        {
                            points.Add(new Vector2(x, y));
                        }
                        else
                        {
                            points.Add(new Vector2(x, y));
                            points.Add(new Vector2(x1, y));
                        }
                    }
                    else
                    {
                        points.Add(new Vector2(x, y));
                        points.Add(new Vector2(x, y1));
                        points.Add(new Vector2(x1, y));
                        points.Add(new Vector2(x1, y1));
                    }
                }
            }

            if (widthCount % 2 != 0)
            {
                float x = point.x;
                for (int j = 1; j <= heightCount / 2; j++)
                {
                    float y, y1;
                    if (heightCount % 2 == 0)
                    {
                        y = point.y - radius - (heightCount / 2 - j) * 2 * radius;
                        y1 = point.y + radius + (heightCount / 2 - j) * 2 * radius;
                    }
                    else
                    {
                        y = point.y - (heightCount / 2 - j + 1) * 2 * radius;
                        y1 = point.y + (heightCount / 2 - j + 1) * 2 * radius;
                    }

                    if (y == y1)
                    {
                        points.Add(new Vector2(x, y));
                    }
                    else
                    {
                        points.Add(new Vector2(x, y));
                        points.Add(new Vector2(x, y1));
                    }
                }
            }

            if (heightCount % 2 != 0)
            {
                float y = point.y;
                for (int i = 1; i <= widthCount / 2; i++)
                {
                    float x, x1;
                    if (widthCount % 2 == 0)
                    {
                        x = point.x - radius - (widthCount / 2 - i) * 2 * radius;
                        x1 = point.x + radius + (widthCount / 2 - i) * 2 * radius;
                    }
                    else
                    {
                        x = point.x - (widthCount / 2 - i + 1) * 2 * radius;
                        x1 = point.x + (widthCount / 2 - i + 1) * 2 * radius;
                    }

                    if (x == x1)
                    {
                        points.Add(new Vector2(x, y));
                    }
                    else
                    {

                        points.Add(new Vector2(x, y));
                        points.Add(new Vector2(x1, y));
                    }
                }
            }

            if (widthCount % 2 != 0 && heightCount % 2 != 0)
            {
                points.Add(new Vector2(point.x, point.y));
            }

            return points;
        }
        #endregion

        /// <summary>
        /// Trả về hướng quay (32 hướng) theo độ lớn góc quay tính theo trục Ox, chiều kim KNB hồ
        /// </summary>
        /// <param name="degree"></param>
        /// <returns></returns>
        public static Direction32 GetDirection32ByAngle360(float degree)
        {
            /// Trừ đi 5.625 cho thụt lại nửa
            degree -= 5.625f;
            /// Chia cho 11.25 để xem ra ID nào trên đường tròn chiều thuận
            int circleID = Mathf.RoundToInt(degree / 11.25f);
            /// If smaller than 24
            if (circleID <= 24)
            {
                return (Direction32) (24 - circleID);
            }
            /// If from 25 - 32
            else
            {
                return (Direction32) (24 + 32 - circleID);
            }
        }

        /// <summary>
        /// Trả về hướng quay (16 hướng) theo độ lớn góc quay tính theo trục Ox, chiều kim KNB hồ
        /// </summary>
        /// <param name="degree">Góc quay 360° tính theo trục Ox</param>
        /// <returns></returns>
        public static Direction16 GetDirection16ByAngle360(float degree)
        {
            Direction16 dir16;
            if (degree >= 11.25f && degree < 33.75f)
            {
                dir16 = Direction16.Up_Down_Right;
            }
            else if (degree >= 33.75f && degree < 56.25f)
            {
                dir16 = Direction16.Up_Right;
            }
            else if (degree >= 56.25f && degree < 78.75f)
            {
                dir16 = Direction16.Up_Up_Right;
            }
            else if (degree >= 78.75f && degree < 101.25f)
            {
                dir16 = Direction16.Up;
            }
            else if (degree >= 101.25f && degree < 123.72f)
            {
                dir16 = Direction16.Up_Up_Left;
            }
            else if (degree >= 123.75f && degree < 146.25f)
            {
                dir16 = Direction16.Up_Left;
            }
            else if (degree >= 146.25f && degree < 168.75f)
            {
                dir16 = Direction16.Up_Down_Left;
            }
            else if (degree >= 168.75f && degree < 191.25f)
            {
                dir16 = Direction16.Left;
            }
            else if (degree >= 191.25f && degree < 213.75f)
            {
                dir16 = Direction16.Down_Up_Left;
            }
            else if (degree >= 213.75f && degree < 236.25f)
            {
                dir16 = Direction16.Down_Left;
            }
            else if (degree >= 236.25f && degree < 258.75f)
            {
                dir16 = Direction16.Down_Down_Left;
            }
            else if (degree >= 258.75f && degree < 281.25f)
            {
                dir16 = Direction16.Down;
            }
            else if (degree >= 281.25f && degree < 303.75f)
            {
                dir16 = Direction16.Down_Down_Right;
            }
            else if (degree >= 303.75f && degree < 326.25f)
            {
                dir16 = Direction16.Down_Right;
            }
            else if (degree >= 326.25f && degree < 348.75f)
            {
                dir16 = Direction16.Down_Up_Right;
            }
            else
            {
                dir16 = Direction16.Right;
            }
            return dir16;
        }

        /// <summary>
        /// Chuyển đối tượng hướng quay 16 hướng sang Vector tương ứng
        /// </summary>
        /// <param name="dir16"></param>
        /// <returns></returns>
        public static Vector2 Dir16ToDirVector(Direction16 dir16)
        {
            switch (dir16)
            {
                case Direction16.Down:
                    return new Vector2(0, -1);
                case Direction16.Down_Down_Left:
                    return new Vector2(-0.41f, -1);
                case Direction16.Down_Left:
                    return new Vector2(-1, -1);
                case Direction16.Down_Up_Left:
                    return new Vector2(-1, -0.41f);
                case Direction16.Left:
                    return new Vector2(-1, 0);
                case Direction16.Up_Down_Left:
                    return new Vector2(-1, 0.41f);
                case Direction16.Up_Left:
                    return new Vector2(-1, 1);
                case Direction16.Up_Up_Left:
                    return new Vector2(-0.41f, 1);
                case Direction16.Up:
                    return new Vector2(0, 1);
                case Direction16.Up_Up_Right:
                    return new Vector2(0.41f, 1);
                case Direction16.Up_Right:
                    return new Vector2(1, 1);
                case Direction16.Up_Down_Right:
                    return new Vector2(1, 0.41f);
                case Direction16.Right:
                    return new Vector2(1, 0);
                case Direction16.Down_Up_Right:
                    return new Vector2(1, -0.41f);
                case Direction16.Down_Right:
                    return new Vector2(1, -1);
                case Direction16.Down_Down_Right:
                    return new Vector2(0.41f, -1);
                default:
                    return Vector2.zero;
            }
        }

        /// <summary>
        /// Trả về góc của hướng quay 16 hướng tương ứng góc ban đầu
        /// </summary>
        /// <param name="originAngle"></param>
        /// <returns></returns>
        public static float GetDir16Angle(float originAngle)
        {
            /// Hướng quay 16 hướng
            Direction16 dir16 = KTMath.GetDirection16ByAngle360(originAngle);
            /// Vector chỉ hướng
            Vector2 dirVector = KTMath.Dir16ToDirVector(dir16);
            /// Góc với trục Ox
            float angle = KTMath.GetAngle360WithXAxis(dirVector);
            /// Trả về kết quả
            return angle;
        }

        /// <summary>
        /// Lấy giá trị các hướng quay phụ khi đối tượng quay giữa 2 hướng chỉ định
        /// </summary>
        /// <param name="fromDir"></param>
        /// <param name="toDir"></param>
        /// <returns></returns>
        public static List<Direction> GetDiffDirections(Direction fromDir, Direction toDir)
        {
            List<Direction> dirs = new List<Direction>();
            if (fromDir == Direction.DOWN)
            {
                if (toDir == Direction.DOWN_LEFT)
                {
                    dirs.Add(Direction.DOWN_LEFT);
                }
                else if (toDir == Direction.LEFT)
                {
                    dirs.Add(Direction.DOWN_LEFT);
                    dirs.Add(Direction.LEFT);
                }
                else if (toDir == Direction.UP_LEFT)
                {
                    dirs.Add(Direction.DOWN_LEFT);
                    dirs.Add(Direction.LEFT);
                    dirs.Add(Direction.UP_LEFT);
                }
                else if (toDir == Direction.UP)
                {
                    dirs.Add(Direction.DOWN_LEFT);
                    dirs.Add(Direction.LEFT);
                    dirs.Add(Direction.UP_LEFT);
                    dirs.Add(Direction.UP);
                }
                else if (toDir == Direction.UP_RIGHT)
                {
                    dirs.Add(Direction.DOWN_RIGHT);
                    dirs.Add(Direction.RIGHT);
                    dirs.Add(Direction.UP_RIGHT);
                }
                else if (toDir == Direction.RIGHT)
                {
                    dirs.Add(Direction.DOWN_RIGHT);
                    dirs.Add(Direction.RIGHT);
                }
                else if (toDir == Direction.DOWN_RIGHT)
                {
                    dirs.Add(Direction.DOWN_RIGHT);
                }
            }
            else if (fromDir == Direction.DOWN_LEFT)
            {
                if (toDir == Direction.DOWN)
                {
                    dirs.Add(Direction.DOWN);
                }
                else if (toDir == Direction.LEFT)
                {
                    dirs.Add(Direction.LEFT);
                }
                else if (toDir == Direction.UP_LEFT)
                {
                    dirs.Add(Direction.LEFT);
                    dirs.Add(Direction.UP_LEFT);
                }
                else if (toDir == Direction.UP)
                {
                    dirs.Add(Direction.LEFT);
                    dirs.Add(Direction.UP_LEFT);
                    dirs.Add(Direction.UP);
                }
                else if (toDir == Direction.UP_RIGHT)
                {
                    dirs.Add(Direction.LEFT);
                    dirs.Add(Direction.UP_LEFT);
                    dirs.Add(Direction.UP);
                    dirs.Add(Direction.UP_RIGHT);
                }
                else if (toDir == Direction.RIGHT)
                {
                    dirs.Add(Direction.DOWN);
                    dirs.Add(Direction.DOWN_RIGHT);
                    dirs.Add(Direction.RIGHT);
                }
                else if (toDir == Direction.DOWN_RIGHT)
                {
                    dirs.Add(Direction.DOWN);
                    dirs.Add(Direction.DOWN_RIGHT);
                }
            }
            else if (fromDir == Direction.LEFT)
            {
                if (toDir == Direction.DOWN)
                {
                    dirs.Add(Direction.DOWN_LEFT);
                    dirs.Add(Direction.DOWN);
                }
                else if (toDir == Direction.DOWN_LEFT)
                {
                    dirs.Add(Direction.DOWN_LEFT);
                }
                else if (toDir == Direction.UP_LEFT)
                {
                    dirs.Add(Direction.UP_LEFT);
                }
                else if (toDir == Direction.UP)
                {
                    dirs.Add(Direction.UP_LEFT);
                    dirs.Add(Direction.UP);
                }
                else if (toDir == Direction.UP_RIGHT)
                {
                    dirs.Add(Direction.UP_LEFT);
                    dirs.Add(Direction.UP);
                    dirs.Add(Direction.UP_RIGHT);
                }
                else if (toDir == Direction.RIGHT)
                {
                    dirs.Add(Direction.UP_LEFT);
                    dirs.Add(Direction.UP);
                    dirs.Add(Direction.UP_RIGHT);
                    dirs.Add(Direction.RIGHT);
                }
                else if (toDir == Direction.DOWN_RIGHT)
                {
                    dirs.Add(Direction.DOWN_LEFT);
                    dirs.Add(Direction.DOWN);
                    dirs.Add(Direction.DOWN_RIGHT);
                }
            }
            else if (fromDir == Direction.UP_LEFT)
            {
                if (toDir == Direction.DOWN)
                {
                    dirs.Add(Direction.LEFT);
                    dirs.Add(Direction.DOWN_LEFT);
                    dirs.Add(Direction.DOWN);
                }
                else if (toDir == Direction.DOWN_LEFT)
                {
                    dirs.Add(Direction.LEFT);
                    dirs.Add(Direction.DOWN_LEFT);
                }
                else if (toDir == Direction.LEFT)
                {
                    dirs.Add(Direction.LEFT);
                }
                else if (toDir == Direction.UP)
                {
                    dirs.Add(Direction.UP);
                }
                else if (toDir == Direction.UP_RIGHT)
                {
                    dirs.Add(Direction.UP);
                    dirs.Add(Direction.UP_RIGHT);
                }
                else if (toDir == Direction.RIGHT)
                {
                    dirs.Add(Direction.UP);
                    dirs.Add(Direction.UP_RIGHT);
                    dirs.Add(Direction.RIGHT);
                }
                else if (toDir == Direction.DOWN_RIGHT)
                {
                    dirs.Add(Direction.UP);
                    dirs.Add(Direction.UP_RIGHT);
                    dirs.Add(Direction.RIGHT);
                    dirs.Add(Direction.DOWN_RIGHT);
                }
            }
            else if (fromDir == Direction.UP)
            {
                if (toDir == Direction.DOWN)
                {
                    dirs.Add(Direction.UP_LEFT);
                    dirs.Add(Direction.LEFT);
                    dirs.Add(Direction.DOWN_LEFT);
                    dirs.Add(Direction.DOWN);
                }
                else if (toDir == Direction.DOWN_LEFT)
                {
                    dirs.Add(Direction.UP_LEFT);
                    dirs.Add(Direction.LEFT);
                    dirs.Add(Direction.DOWN_LEFT);
                }
                else if (toDir == Direction.LEFT)
                {
                    dirs.Add(Direction.UP_LEFT);
                    dirs.Add(Direction.LEFT);
                }
                else if (toDir == Direction.UP_LEFT)
                {
                    dirs.Add(Direction.UP_LEFT);
                }
                else if (toDir == Direction.UP_RIGHT)
                {
                    dirs.Add(Direction.UP_RIGHT);
                }
                else if (toDir == Direction.RIGHT)
                {
                    dirs.Add(Direction.UP_RIGHT);
                    dirs.Add(Direction.RIGHT);
                }
                else if (toDir == Direction.DOWN_RIGHT)
                {
                    dirs.Add(Direction.UP_RIGHT);
                    dirs.Add(Direction.RIGHT);
                    dirs.Add(Direction.DOWN_RIGHT);
                }
            }
            else if (fromDir == Direction.UP_RIGHT)
            {
                if (toDir == Direction.DOWN)
                {
                    dirs.Add(Direction.RIGHT);
                    dirs.Add(Direction.DOWN_RIGHT);
                    dirs.Add(Direction.DOWN);
                }
                else if (toDir == Direction.DOWN_LEFT)
                {
                    dirs.Add(Direction.RIGHT);
                    dirs.Add(Direction.DOWN_RIGHT);
                    dirs.Add(Direction.DOWN);
                    dirs.Add(Direction.DOWN_LEFT);
                }
                else if (toDir == Direction.LEFT)
                {
                    dirs.Add(Direction.UP);
                    dirs.Add(Direction.UP_LEFT);
                    dirs.Add(Direction.LEFT);
                }
                else if (toDir == Direction.UP_LEFT)
                {
                    dirs.Add(Direction.UP);
                    dirs.Add(Direction.UP_LEFT);
                }
                else if (toDir == Direction.UP)
                {
                    dirs.Add(Direction.UP);
                }
                else if (toDir == Direction.RIGHT)
                {
                    dirs.Add(Direction.RIGHT);
                }
                else if (toDir == Direction.DOWN_RIGHT)
                {
                    dirs.Add(Direction.RIGHT);
                    dirs.Add(Direction.DOWN_RIGHT);
                }
            }
            else if (fromDir == Direction.RIGHT)
            {
                if (toDir == Direction.DOWN)
                {
                    dirs.Add(Direction.DOWN_RIGHT);
                    dirs.Add(Direction.DOWN);
                }
                else if (toDir == Direction.DOWN_LEFT)
                {
                    dirs.Add(Direction.DOWN_RIGHT);
                    dirs.Add(Direction.DOWN);
                    dirs.Add(Direction.DOWN_LEFT);
                }
                else if (toDir == Direction.LEFT)
                {
                    dirs.Add(Direction.DOWN_RIGHT);
                    dirs.Add(Direction.DOWN);
                    dirs.Add(Direction.DOWN_LEFT);
                    dirs.Add(Direction.LEFT);
                }
                else if (toDir == Direction.UP_LEFT)
                {
                    dirs.Add(Direction.UP_RIGHT);
                    dirs.Add(Direction.UP);
                    dirs.Add(Direction.UP_LEFT);
                }
                else if (toDir == Direction.UP)
                {
                    dirs.Add(Direction.UP_RIGHT);
                    dirs.Add(Direction.UP);
                }
                else if (toDir == Direction.UP_RIGHT)
                {
                    dirs.Add(Direction.UP_RIGHT);
                }
                else if (toDir == Direction.DOWN_RIGHT)
                {
                    dirs.Add(Direction.DOWN_RIGHT);
                }
            }
            else if (fromDir == Direction.DOWN_RIGHT)
            {
                if (toDir == Direction.DOWN)
                {
                    dirs.Add(Direction.DOWN);
                }
                else if (toDir == Direction.DOWN_LEFT)
                {
                    dirs.Add(Direction.DOWN);
                    dirs.Add(Direction.DOWN_LEFT);
                }
                else if (toDir == Direction.LEFT)
                {
                    dirs.Add(Direction.DOWN);
                    dirs.Add(Direction.DOWN_LEFT);
                    dirs.Add(Direction.LEFT);
                }
                else if (toDir == Direction.UP_LEFT)
                {
                    dirs.Add(Direction.DOWN);
                    dirs.Add(Direction.DOWN_LEFT);
                    dirs.Add(Direction.LEFT);
                    dirs.Add(Direction.UP_LEFT);
                }
                else if (toDir == Direction.UP)
                {
                    dirs.Add(Direction.RIGHT);
                    dirs.Add(Direction.UP_RIGHT);
                    dirs.Add(Direction.UP);
                }
                else if (toDir == Direction.UP_RIGHT)
                {
                    dirs.Add(Direction.RIGHT);
                    dirs.Add(Direction.UP_RIGHT);
                }
                else if (toDir == Direction.RIGHT)
                {
                    dirs.Add(Direction.RIGHT);
                }
            }
            return dirs;
        }
    }
}
