using System.Collections.Generic;

namespace FS.VLTK.Loader
{
	/// <summary>
	/// Đối tượng chứa danh sách các cấu hình trong game
	/// </summary>
	public static partial class Loader
    {
        #region Const
        private static readonly Dictionary<byte, char> charTable = new Dictionary<byte, char>()
        {
            { (byte) 0, '0' },
            { (byte) 1, '1' },
            { (byte) 2, '2' },
            { (byte) 3, '3' },
            { (byte) 4, '4' },
            { (byte) 5, '5' },
            { (byte) 6, '6' },
            { (byte) 7, '7' },
            { (byte) 8, '8' },
            { (byte) 9, '9' },
            { (byte) 10, 'a' },
            { (byte) 11, 'b' },
            { (byte) 12, 'c' },
            { (byte) 13, 'd' },
            { (byte) 14, 'e' },
            { (byte) 15, 'f' },
            { (byte) 16, 'g' },
            { (byte) 17, 'h' },
            { (byte) 18, 'i' },
            { (byte) 19, 'j' },
            { (byte) 20, 'k' },
            { (byte) 21, 'l' },
            { (byte) 22, 'm' },
            { (byte) 23, 'n' },
            { (byte) 24, 'o' },
            { (byte) 25, 'p' },
            { (byte) 26, 'q' },
            { (byte) 27, 'r' },
            { (byte) 28, 's' },
            { (byte) 29, 't' },
            { (byte) 30, 'u' },
            { (byte) 31, 'v' },
            { (byte) 32, 'w' },
            { (byte) 33, 'x' },
            { (byte) 34, 'y' },
            { (byte) 35, 'z' },
            { (byte) 36, '_' },
            { (byte) 37, 'A' },
            { (byte) 38, 'B' },
            { (byte) 39, 'C' },
            { (byte) 40, 'D' },
            { (byte) 41, 'E' },
            { (byte) 42, 'F' },
            { (byte) 43, 'G' },
            { (byte) 44, 'H' },
            { (byte) 45, 'I' },
            { (byte) 46, 'J' },
            { (byte) 47, 'K' },
            { (byte) 48, 'L' },
            { (byte) 49, 'M' },
            { (byte) 50, 'N' },
            { (byte) 51, 'O' },
            { (byte) 52, 'P' },
            { (byte) 53, 'Q' },
            { (byte) 54, 'R' },
            { (byte) 55, 'S' },
            { (byte) 56, 'T' },
            { (byte) 57, 'U' },
            { (byte) 58, 'V' },
            { (byte) 59, 'W' },
            { (byte) 60, 'X' },
            { (byte) 61, 'Y' },
            { (byte) 62, 'Z' },
            { (byte) 63, '.' },
            { (byte) 64, '/' },
        };
        #endregion
    }
}
