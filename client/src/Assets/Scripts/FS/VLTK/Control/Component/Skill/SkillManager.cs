using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FS.VLTK.Control.Component
{
    /// <summary>
    /// Quản lý tổng thể kỹ năng
    /// </summary>
    public static partial class SkillManager
    {
        /// <summary>
        /// Làm mới đối tượng
        /// </summary>
        public static void Refresh()
        {
            /// Duyệt danh sách Bullet đã tạo ra và xóa
            {
                List<int> keys = SkillManager.bullets.Keys.ToList();
                foreach (int key in keys)
                {
                    if (!SkillManager.bullets.ContainsKey(key))
					{
                        continue;
					}
                    /// Xóa ngay lập tức
                    SkillManager.DestroyBulletImmediately(SkillManager.bullets[key]);
                }
            }

            /// Duyệt danh sách hiệu ứng nổ đã tạo ra và xóa
            {
                List<GameObject> _keys = SkillManager.explodeEffects.Keys.ToList();
                foreach (GameObject _key in _keys)
                {
                    List<int> keys = SkillManager.explodeEffects[_key].Keys.ToList();
                    /// Duyệt danh sách hiệu ứng nổ
                    foreach (int key in keys)
                    {
                        if (!SkillManager.explodeEffects.ContainsKey(_key))
                        {
                            continue;
                        }
                        else if (!SkillManager.explodeEffects[_key].ContainsKey(key))
						{
                            return;
						}
                        /// Xóa ngay lập tức
                        SkillManager.DestroyExplosionImmediately(SkillManager.explodeEffects[_key][key]);
                    }
                }
            }

            SkillManager.playingSounds.Clear();
            SkillManager.bullets.Clear();
            SkillManager.explodeEffects.Clear();
            if (SkillManager.autoCollectGarbageCoroutine != null)
            {
                PlayZone.Instance.StopCoroutine(SkillManager.autoCollectGarbageCoroutine);
            }
            SkillManager.autoCollectGarbageCoroutine = PlayZone.Instance.StartCoroutine(SkillManager.AutoCollectGarbage());
        }
    }
}
