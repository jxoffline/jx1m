using FS.GameEngine.Logic;
using FS.VLTK;
using FS.VLTK.Factory;
using FS.VLTK.UI;
using FS.VLTK.UI.CoreUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FS.GameEngine.Sprite
{
    /// <summary>
    /// Quản lý UI
    /// </summary>
    public partial class GSprite
    {
        #region Dòng chữ bay trên đầu
        /// <summary>
        /// Thời gian chờ mỗi lần hiển thị sát thương
        /// </summary>
        private const float TextFlyAddTime = 0.1f;

        /// <summary>
        /// Danh sách chữ bay trên đầu đang chờ thực thi
        /// </summary>
        private readonly Queue<Action> queueFlyingText = new Queue<Action>();

        /// <summary>
        /// Thời điểm thực thi biểu diễn chữ bay trước đó
        /// </summary>
        private long lastProcessHeadTextTick = 0;

        /// <summary>
        /// Thực hiện biểu diễn chữ bay phía trên đầu
        /// </summary>
        private void ProcessHeadText()
        {
            /// Nếu danh sách rỗng
            if (this.queueFlyingText.Count <= 0)
            {
                return;
            }

            /// Nếu chưa đến thời gian
            if (KTGlobal.GetCurrentTimeMilis() - this.lastProcessHeadTextTick < GSprite.TextFlyAddTime * 1000)
            {
                return;
            }

            /// Cập nhật thời điểm thực thi biểu diễn chữ bay
            this.lastProcessHeadTextTick = KTGlobal.GetCurrentTimeMilis();

            /// Thực thi biểu diễn đối tượng ở đầu hàng đợi
            Action action = this.queueFlyingText.Dequeue();
            action?.Invoke();
        }

        /// <summary>
        /// Hiển thị chữ bay phía trên đầu
        /// </summary>
        /// <param name="type"></param>
        /// <param name="content"></param>
        public void AddHeadText(FS.VLTK.Entities.Enum.DamageType type, string content)
        {
            VLTK.Control.Component.Character character = this.ComponentCharacter;
            VLTK.Control.Component.Monster monster = this.ComponentMonster;

            switch (type)
            {
                case FS.VLTK.Entities.Enum.DamageType.DAMAGE_TAKEN:
                {
                    if (character != null)
                    {
                        this.queueFlyingText.Enqueue(() => {
                            UIFlyingText text = KTObjectPoolManager.Instance.Instantiate<UIFlyingText>("UITextLeaderDamageTaken");
                            if (text == null)
                            {
                                return;
                            }
                            text.ReferenceObject = character.gameObject;
                            text.Offset = new Vector2(0, 120);
                            text.Text = content;
                            text.Duration = 1.5f;
                            text.Play();
                        });
                    }
                    else if (monster != null)
                    {
                        this.queueFlyingText.Enqueue(() => {
                            UIFlyingText text = KTObjectPoolManager.Instance.Instantiate<UIFlyingText>("UITextLeaderDamageTaken");
                            if (text == null)
                            {
                                return;
                            }
                            text.ReferenceObject = monster.gameObject;
                            text.Offset = new Vector2(0, 90);
                            text.Text = content;
                            text.Duration = 1.5f;
                            text.Play();
                        });
                    }
                    break;
                }
                case FS.VLTK.Entities.Enum.DamageType.CRIT_DAMAGE_DEALT:
                {
                    if (character != null)
                    {
                        this.queueFlyingText.Enqueue(() => {
                            UIFlyingText text = KTObjectPoolManager.Instance.Instantiate<UIFlyingText>("UITextLeaderDamageCrit");
                            if (text == null)
                            {
                                return;
                            }
                            text.ReferenceObject = character.gameObject;
                            text.Offset = new Vector2(0, 120);
                            text.Text = content;
                            text.Duration = 1.5f;
                            text.Play();
                        });
                    }
                    else if (monster != null)
                    {
                        this.queueFlyingText.Enqueue(() => {
                            UIFlyingText text = KTObjectPoolManager.Instance.Instantiate<UIFlyingText>("UITextLeaderDamageCrit");
                            if (text == null)
                            {
                                return;
                            }
                            text.ReferenceObject = monster.gameObject;
                            text.Offset = new Vector2(0, 90);
                            text.Text = content;
                            text.Duration = 1.5f;
                            text.Play();
                        });
                    }
                    break;
                }
                case FS.VLTK.Entities.Enum.DamageType.PET_CRIT_DAMAGE_DEALT:
                {
                    if (character != null)
                    {
                        this.queueFlyingText.Enqueue(() => {
                            UIFlyingText text = KTObjectPoolManager.Instance.Instantiate<UIFlyingText>("UITextPetDamageCrit");
                            if (text == null)
                            {
                                return;
                            }
                            text.ReferenceObject = character.gameObject;
                            text.Offset = new Vector2(0, 120);
                            text.Text = content;
                            text.Duration = 1.5f;
                            text.Play();
                        });
                    }
                    else if (monster != null)
                    {
                        this.queueFlyingText.Enqueue(() => {
                            UIFlyingText text = KTObjectPoolManager.Instance.Instantiate<UIFlyingText>("UITextPetDamageCrit");
                            if (text == null)
                            {
                                return;
                            }
                            text.ReferenceObject = monster.gameObject;
                            text.Offset = new Vector2(0, 90);
                            text.Text = content;
                            text.Duration = 1.5f;
                            text.Play();
                        });
                    }
                    break;
                }
                case FS.VLTK.Entities.Enum.DamageType.DAMAGE_DEALT:
                {
                    if (character != null)
                    {
                        this.queueFlyingText.Enqueue(() =>
                        {
                            UIFlyingText text = KTObjectPoolManager.Instance.Instantiate<UIFlyingText>("UITextLeaderDamage");
                            if (text == null)
                            {
                                return;
                            }
                            text.ReferenceObject = character.gameObject;
                            text.Offset = new Vector2(0, 120);
                            text.Text = content;
                            text.Duration = 1.5f;
                            text.Play();
                        });
                    }
                    else if (monster != null)
                    {
                        this.queueFlyingText.Enqueue(() =>
                        {
                            UIFlyingText text = KTObjectPoolManager.Instance.Instantiate<UIFlyingText>("UITextLeaderDamage");
                            if (text == null)
                            {
                                return;
                            }
                            text.ReferenceObject = monster.gameObject;
                            text.Offset = new Vector2(0, 90);
                            text.Text = content;
                            text.Duration = 1.5f;
                            text.Play();
                        });
                    }
                    break;
                }
                case FS.VLTK.Entities.Enum.DamageType.PET_DAMAGE_DEALT:
                {
                    if (character != null)
                    {
                        this.queueFlyingText.Enqueue(() => {
                            UIFlyingText text = KTObjectPoolManager.Instance.Instantiate<UIFlyingText>("UITextPetDamage");
                            if (text == null)
                            {
                                return;
                            }
                            text.ReferenceObject = character.gameObject;
                            text.Offset = new Vector2(0, 120);
                            text.Text = content;
                            text.Duration = 1.5f;
                            text.Play();
                        });
                    }
                    else if (monster != null)
                    {
                        this.queueFlyingText.Enqueue(() => {
                            UIFlyingText text = KTObjectPoolManager.Instance.Instantiate<UIFlyingText>("UITextPetDamage");
                            if (text == null)
                            {
                                return;
                            }
                            text.ReferenceObject = monster.gameObject;
                            text.Offset = new Vector2(0, 90);
                            text.Text = content;
                            text.Duration = 1.5f;
                            text.Play();
                        });
                    }
                    break;
                }
                case FS.VLTK.Entities.Enum.DamageType.DODGE:
                {
                    if (character != null)
                    {
                        this.queueFlyingText.Enqueue(() => {
                            UIFlyingText text = KTObjectPoolManager.Instance.Instantiate<UIFlyingText>("UITextMiss");
                            if (text == null)
                            {
                                return;
                            }
                            text.ReferenceObject = character.gameObject;
                            text.Offset = new Vector2(0, 120);
                            text.Text = "Trượt";
                            text.Duration = 1.5f;
                            text.Play();
                        });
                    }
                    else if (monster != null)
                    {
                        this.queueFlyingText.Enqueue(() => {
                            UIFlyingText text = KTObjectPoolManager.Instance.Instantiate<UIFlyingText>("UITextMiss");
                            if (text == null)
                            {
                                return;
                            }
                            text.ReferenceObject = monster.gameObject;
                            text.Offset = new Vector2(0, 90);
                            text.Text = "Trượt";
                            text.Duration = 1.5f;
                            text.Play();
                        });
                    }
                    break;
                }
                case FS.VLTK.Entities.Enum.DamageType.IMMUNE:
                {
                    if (character != null)
                    {
                        this.queueFlyingText.Enqueue(() => {
                            UIFlyingText text = KTObjectPoolManager.Instance.Instantiate<UIFlyingText>("UITextMiss");
                            if (text == null)
                            {
                                return;
                            }
                            text.ReferenceObject = character.gameObject;
                            text.Offset = new Vector2(0, 120);
                            text.Text = "Miễn dịch";
                            text.Duration = 1.5f;
                            text.Play();
                        });
                    }
                    else if (monster != null)
                    {
                        this.queueFlyingText.Enqueue(() => {
                            UIFlyingText text = KTObjectPoolManager.Instance.Instantiate<UIFlyingText>("UITextMiss");
                            if (text == null)
                            {
                                return;
                            }
                            text.ReferenceObject = monster.gameObject;
                            text.Offset = new Vector2(0, 90);
                            text.Text = "Miễn dịch";
                            text.Duration = 1.5f;
                            text.Play();
                        });
                    }
                    break;
                }
                case FS.VLTK.Entities.Enum.DamageType.ADJUST:
                {
                    if (character != null)
                    {
                        this.queueFlyingText.Enqueue(() => {
                            UIFlyingText text = KTObjectPoolManager.Instance.Instantiate<UIFlyingText>("UITextMiss");
                            if (text == null)
                            {
                                return;
                            }
                            text.ReferenceObject = character.gameObject;
                            text.Offset = new Vector2(0, 120);
                            text.Text = "Hóa giải";
                            text.Duration = 1.5f;
                            text.Play();
                        });
                    }
                    else if (monster != null)
                    {
                        this.queueFlyingText.Enqueue(() => {
                            UIFlyingText text = KTObjectPoolManager.Instance.Instantiate<UIFlyingText>("UITextMiss");
                            if (text == null)
                            {
                                return;
                            }
                            text.ReferenceObject = monster.gameObject;
                            text.Offset = new Vector2(0, 90);
                            text.Text = "Hóa giải";
                            text.Duration = 1.5f;
                            text.Play();
                        });
                    }
                    break;
                }
            }
        }

        #endregion
    }
}
