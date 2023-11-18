using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace GameServer.KiemThe.Utilities.Algorithms
{
    /// <summary>
    /// Biểu thức đại số
    /// </summary>
    public class AlgebraicExpression
    {
        /// <summary>
        /// Dạng đại số của biểu thức
        /// </summary>
        public string Infix { get; private set; }

        /// <summary>
        /// Biểu thức đại số
        /// </summary>
        /// <param name="infix">Chuỗi biểu diễn biểu thức</param>
        public AlgebraicExpression(string infix)
        {
            this.Infix = Regex.Replace(infix, @"\s", "");
        }

        /// <summary>
        /// Trả ra độ ưu tiên của toán tử
        /// </summary>
        /// <param name="op"></param>
        /// <returns></returns>
        private int GetPriority(char op)
        {
            switch (op)
            {
                case '*':
                    return 2;

                case '/':
                    return 2;

                case '+':
                    return 1;

                case '-':
                    return 1;
            }
            return 0;
        }

        /// <summary>
        /// Kiểm cha ký tự đầu vào có phải toán tử không
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private bool IsOperation(char c)
        {
            return c == '+' || c == '-' || c == '*' || c == '/';
        }

        /// <summary>
        /// Kiểm tra ký tự đầu vào có phải dạng số không
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private bool IsNumber(char c)
        {
            return Regex.IsMatch(c.ToString(), @"[0-9\.]");
        }

        /// <summary>
        /// Dạng hậu tố của biểu thức
        /// </summary>
        public string Postfix
        {
            get
            {
                /// Stack lưu vị trí các phần tử
                Stack<string> stack = new Stack<string>();

                string result = "";
                string lastNumberStr = "";

                /// Duyệt toàn bộ các ký tự trong biểu thức đầu vào
                foreach (char c in this.Infix)
                {
                    /// Nếu là số thì lưu lại giá trị
                    if (this.IsNumber(c))
                    {
                        lastNumberStr += c.ToString();
                    }
                    else
                    {
                        /// Nếu có số được lưu trước đó thì đẩy vào Stack
                        if (!string.IsNullOrEmpty(lastNumberStr))
                        {
                            result += " " + lastNumberStr;
                            lastNumberStr = "";
                        }

                        /// Nếu là dấu mở ngoặc thì đẩy vào Stack
                        if (c == '(')
                        {
                            stack.Push(c.ToString());
                        }
                        /// Nếu là dấu đóng ngoặc thì lấy toàn bộ các phần từ trong Stack ra output cho đến khi gặp dấu mở ngoặc
                        else if (c == ')')
                        {
                            while (stack.Count > 0)
                            {
                                if (stack.Peek() == "(")
                                {
                                    stack.Pop();
                                    break;
                                }
                                result += " " + stack.Pop();
                            }
                        }
                        /// Nếu là toán tử, thì kiểm tra chừng nào đỉnh của stack còn là toán tử và có độ ưu tiên lớn hơn toán tử hiện tại thì lấy ra và cho ra output
                        else if (this.IsOperation(c))
                        {
                            while (stack.Count > 0)
                            {
                                string cc = stack.Peek();
                                if (cc.Length != 1 || !this.IsOperation(cc[0]))
                                {
                                    break;
                                }
                                if (this.GetPriority(cc[0]) >= this.GetPriority(c))
                                {
                                    result += " " + cc[0];
                                    stack.Pop();
                                }
                                else
                                {
                                    break;
                                }
                            }
                            stack.Push(c.ToString());
                        }
                    }
                }

                if (!string.IsNullOrEmpty(lastNumberStr))
                {
                    result += " " + lastNumberStr;
                }

                while (stack.Count > 0)
                {
                    result += " " + stack.Pop();
                }

                return result.Trim();
            }
        }

        /// <summary>
        /// Giá trị của biểu thức
        /// </summary>
        /// <returns></returns>
        public float Result
        {
            get
            {
                /// Dạng hậu tố của biểu thức
                string postfix = this.Postfix;

                /// Stack chứa danh sách các toán hạng đang chờ tính toán
                Stack<float> stack = new Stack<float>();

                string[] elements = postfix.Split(' ');

                /// Duyệt đến cuối dãy
                foreach (string element in elements)
                {
                    /// Nếu là toán tử thì tính toán 2 phần tử trên đỉnh Stack lại với nhau
                    if (element.Length == 1 && this.IsOperation(element[0]))
                    {
                        float x = stack.Pop();
                        float y = stack.Count > 0 ? stack.Pop() : 0;
                        float res = 0;

                        switch (element[0])
                        {
                            case '+':
                                res = y + x;
                                break;

                            case '-':
                                res = y - x;
                                break;

                            case '*':
                                res = y * x;
                                break;

                            case '/':
                                res = y / x;
                                break;
                        }
                        stack.Push(res);
                    }
                    /// Nếu là toán hạng thì đẩy vào Stack
                    else
                    {
                        float param = float.Parse(element);
                        stack.Push(param);
                    }
                }

                return stack.Pop();
            }
        }
    }
}