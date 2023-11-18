using FS.VLTK.Entities.Config;
using FS.VLTK.Entities.Object;
using FS.VLTK.Factory;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FS.VLTK.Control.Component
{
    /// <summary>
    /// Quản lý hiệu ứng quái
    /// </summary>
    public partial class Monster
    {
        /// <summary>
        /// Danh sách hiệu ứng trên người
        /// </summary>
        private readonly Dictionary<int, Effect> listEffects = new Dictionary<int, Effect>();

        /// <summary>
        /// Thêm hiệu ứng vào quái
        /// </summary>
        /// <param name="effectID"></param>
        /// <param name="type"></param>
        public void AddEffect(int effectID, Entities.Enum.EffectType type)
        {
            IEnumerator AddEffectLater()
            {
                while (!this.isStarted)
                {
                    yield return null;
                }

                if (this.listEffects.TryGetValue(effectID, out Effect oldEffect))
                {
                    if (oldEffect.Data == null || !oldEffect.Data.Loop)
                    {
                        oldEffect.Destroy();
                        yield return null;
                    }
                    else
                    {
                        yield break;
                    }
                }

                Effect effect = SkillManager.CreateEffect(effectID, this.gameObject);
                if (effect == null)
                {
                    yield break;
                }
                this.listEffects[effectID] = effect;
                effect.Destroyed = () => {
                    this.listEffects.Remove(effectID);
                };
                if (type == Entities.Enum.EffectType.CastEffect)
                {
                    effect.Data.Loop = false;
                }
                effect.Type = type;
                effect.RefreshAction();
                effect.Play();
            }
            this.StartCoroutine(AddEffectLater());
        }

        /// <summary>
        /// Xóa hiệu ứng tương ứng khỏi quái
        /// </summary>
        /// <param name="effectID"></param>
        public void RemoveEffect(int effectID)
        {
            if (this.listEffects.TryGetValue(effectID, out Effect effect))
            {
                effect.Destroy();
            }
        }

        /// <summary>
        /// Xóa tất cả hiệu ứng khỏi nhân vật
        /// </summary>
        public void RemoveAllEffects()
        {
            List<int> toDeleteListByKey = this.listEffects.Keys.ToList();
            foreach (int key in toDeleteListByKey)
            {
                this.listEffects[key].Destroy();
            }
        }
    }
}
