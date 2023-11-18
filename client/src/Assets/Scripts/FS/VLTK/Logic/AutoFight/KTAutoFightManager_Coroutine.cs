using System.Collections;
using UnityEngine;

namespace FS.VLTK.Logic
{
    /// <summary>
    /// Luồng thực thi tự đánh
    /// </summary>
    public partial class KTAutoFightManager
    {
        /// <summary>
        /// Luồng thực thi tự ăn thức ăn và thuốc
        /// </summary>
        /// <returns></returns>
        private IEnumerator AutoUseFoodAndMedicine()
		{
            /// Lặp liên tục
            while (true)
			{
                /// Tự dùng thuốc nếu có thiết lập
                this.AutoUseMedicine();

                this.AutoEatX2();
                /// Nghỉ giãn cách
                yield return new WaitForSeconds(1f);
			}
		}
	}
}
