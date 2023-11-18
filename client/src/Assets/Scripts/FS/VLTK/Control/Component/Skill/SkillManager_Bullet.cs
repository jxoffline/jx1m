using FS.GameEngine.Logic;
using FS.GameEngine.Sprite;
using FS.VLTK.Control.Component.Skill;
using FS.VLTK.Factory;
using FS.VLTK.Loader;
using FS.VLTK.Logic.Settings;
using FS.VLTK.Utilities.Threading;
using FS.VLTK.Utilities.UnityComponent;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FS.VLTK.Control.Component
{
    /// <summary>
    /// Quản lý hiệu ứng đạn
    /// </summary>
    public static partial class SkillManager
    {
        #region Bullet Sprites

        /// <summary>
        /// Danh sách đạn
        /// </summary>
        private static readonly Dictionary<int, Bullet> bullets = new Dictionary<int, Bullet>();

        /// <summary>
        /// Danh sách hiệu ứng nổ đang thực thi trên đối tượng
        /// </summary>
        private static readonly Dictionary<GameObject, Dictionary<int, ExplodeEffect>> explodeEffects = new Dictionary<GameObject, Dictionary<int, ExplodeEffect>>();

        /// <summary>
        /// Thời gian tối đa giữ đạn tồn tại
        /// </summary>
        private const long BulletKeepTicks = 5000;

        /// <summary>
        /// Thời gian tồn tại bẫy
        /// </summary>
        private const long TrapKeepTicks = 30000;

        /// <summary>
        /// Thời gian tối đa giữ hiệu ứng nổ tồn tại
        /// </summary>
        private const long ExplodeEffectKeepTicks = 3000;

        /// <summary>
        /// Thời gian tự thu thập rác
        /// </summary>
        private const float AutoCollectGarbageSec = 1f;

        /// <summary>
        /// Tạo viên đạn bay theo hình tròn từ vị trí của đối tượng ra chiêu
        /// </summary>
        /// <param name="caster">Đối tượng xuất chiêu</param>
        /// <param name="delay">Thời gian Delay đến khi tạo</param>
        /// <param name="bulletID">ID đạn truyền từ GS</param>
        /// <param name="resID">ID Res</param>
        /// <param name="lifeTime">Thời gian tồn tại</param>
        /// <param name="radius">Bán kính</param>
        /// <param name="dirVector">Hướng ban đầu</param>
        /// <param name="fromPos">Vị trí đầu</param>
        /// <param name="toPos">Vị trí cuối</param>
        /// <param name="followCaster">Theo đối tượng ra chiêu không</param>
        /// <param name="velocity">Vận tốc bay</param>
        /// <param name="repeatAnimation">Lặp đi lặp lại hiệu ứng đạn bay cho tới khi tan biến</param>
        public static void CreateBulletFlyByCircle(GSprite caster, float delay, int bulletID, int resID, float lifeTime, float radius, Vector2 dirVector, Vector2 fromPos, Vector2 toPos, bool followCaster, float velocity, bool repeatAnimation = true)
        {
            /// Nếu FPS thấp thì không thực hiện
            if (MainGame.Instance.GetRenderQuality() == MainGame.RenderQuality.Low)
            {
                return;
            }

            /// Nếu là Bullet thật
            if (bulletID != -1)
            {
                /// Nếu tồn tại tia đạn cũ thì xóa
                if (SkillManager.bullets.TryGetValue(bulletID, out Bullet oldBullet))
                {
                    oldBullet.Destroy();
                    SkillManager.bullets.Remove(bulletID);
                }
            }

            /// Nếu thiết lập hệ thống không hiển thị đạn
            if (KTSystemSetting.HideSkillBullet)
            {
                return;
            }

            Bullet bullet = KTObjectPoolManager.Instance.Instantiate<Bullet>("Bullet");
            /// Nếu không thể tạo Bullet do Pool đã đầy hoặc lỗi gì đó
            if (bullet == null)
            {
                return;
            }
            bullet.ResID = resID;
            bullet.RefreshAction();
            bullet.CreateTick = KTGlobal.GetCurrentTimeMilis();
            bullet.Caster = caster;
            bullet.FromPos = fromPos;
            bullet.ToPos = toPos;
            bullet.CircleMoveRadius = radius;
            bullet.CircleFollowCaster = followCaster;
            bullet.CircleDirVector = dirVector;
            bullet.Velocity = velocity;
            bullet.RepeatAnimation = repeatAnimation;
            bullet.Delay = delay;
            bullet.MaxLifeTime = lifeTime;
            bullet.Destroyed = () => {
                SkillManager.DestroyBullet(bulletID);
            };
            bullet.Fly();

            /// Nếu là Bullet thật
            if (bulletID != -1)
            {
                SkillManager.bullets[bulletID] = bullet;
            }
        }

        /// <summary>
        /// Tạo đạn bay từ vị trí fromPos đến vị trí toPos vận tốc tương ứng
        /// </summary>
        /// <param name="caster">Đối tượng xuất chiêu</param>
        /// <param name="delay">Thời gian Delay đến khi tạo</param>
        /// <param name="bulletID">ID đạn truyền từ GS</param>
        /// <param name="resID">ID Res</param>
        /// <param name="fromPos">Vị trí đầu</param>
        /// <param name="toPos">Vị trí cuối</param>
        /// <param name="velocity">Vận tốc bay</param>
        /// <param name="comeback">Quay trở lại vị trí ban đầu không</param>
        /// <param name="repeatAnimation">Lặp đi lặp lại hiệu ứng đạn bay cho tới khi tan biến</param>
        public static void CreateBullet(GSprite caster, float delay, int bulletID, int resID, Vector2 fromPos, Vector2 toPos, float velocity, bool comeback, bool repeatAnimation = true)
        {
            /// Nếu FPS thấp thì không thực hiện
            if (MainGame.Instance.GetRenderQuality() == MainGame.RenderQuality.Low)
            {
                return;
            }

            /// Nếu là Bullet thật
            if (bulletID != -1)
            {
                /// Nếu tồn tại tia đạn cũ thì xóa
                if (SkillManager.bullets.TryGetValue(bulletID, out Bullet oldBullet))
                {
                    oldBullet.Destroy();
                    SkillManager.bullets.Remove(bulletID);
                }
            }

            /// Nếu thiết lập hệ thống không hiển thị đạn
            if (KTSystemSetting.HideSkillBullet)
            {
                return;
            }

            Bullet bullet = KTObjectPoolManager.Instance.Instantiate<Bullet>("Bullet");
            /// Nếu không thể tạo Bullet do Pool đã đầy hoặc lỗi gì đó
            if (bullet == null)
            {
                return;
            }
            bullet.ResID = resID;
            bullet.RefreshAction();
            bullet.CreateTick = KTGlobal.GetCurrentTimeMilis();
            bullet.Caster = caster;
            bullet.FromPos = fromPos;
            bullet.ToPos = toPos;
            bullet.Velocity = velocity;
            bullet.RepeatAnimation = repeatAnimation;
            bullet.Delay = delay;
            bullet.Comeback = comeback;
            bullet.Destroyed = () => {
                SkillManager.DestroyBullet(bulletID);
            };
            bullet.Fly();

            /// Nếu là Bullet thật
            if (bulletID != -1)
            {
                SkillManager.bullets[bulletID] = bullet;
            }
        }

        /// <summary>
        /// Tạo đạn bay từ vị trí ra chiêu đến vị trí FromPos sau đó bay đến ToPos
        /// </summary>
        /// <param name="caster">Đối tượng xuất chiêu</param>
        /// <param name="bulletID">ID đạn truyền từ GS</param>
        /// <param name="resID">ID Res</param>
        /// <param name="fromPos">Vị trí đầu</param>
        /// <param name="toPos">Vị trí cuối</param>
        /// <param name="velocity">Vận tốc bay</param>
        /// <param name="repeatAnimation">Lặp đi lặp lại hiệu ứng đạn bay cho tới khi tan biến</param>
        public static void CreateBulletLinearFlyThenGoToPos(GSprite caster, int bulletID, int resID, Vector2 fromPos, Vector2 toPos, float velocity, bool repeatAnimation = true)
        {
            /// Nếu FPS thấp thì không thực hiện
            if (MainGame.Instance.GetRenderQuality() == MainGame.RenderQuality.Low)
            {
                return;
            }

            /// Nếu là Bullet thật
            if (bulletID != -1)
            {
                /// Nếu tồn tại tia đạn cũ thì xóa
                if (SkillManager.bullets.TryGetValue(bulletID, out Bullet oldBullet))
                {
                    oldBullet.Destroy();
                    SkillManager.bullets.Remove(bulletID);
                }
            }

            /// Nếu thiết lập hệ thống không hiển thị đạn
            if (KTSystemSetting.HideSkillBullet)
            {
                return;
            }

            Bullet bullet = KTObjectPoolManager.Instance.Instantiate<Bullet>("Bullet");
            /// Nếu không thể tạo Bullet do Pool đã đầy hoặc lỗi gì đó
            if (bullet == null)
            {
                return;
            }
            bullet.ResID = resID;
            bullet.RefreshAction();
            bullet.CreateTick = KTGlobal.GetCurrentTimeMilis();
            bullet.Caster = caster;
            bullet.FromPos = fromPos;
            bullet.ToPos = toPos;
            bullet.Velocity = velocity;
            bullet.RepeatAnimation = repeatAnimation;
            bullet.Delay = 0f;
            bullet.FlyToStartPosFirst = true;
            bullet.Destroyed = () => {
                SkillManager.DestroyBullet(bulletID);
            };
            bullet.Fly();

            /// Nếu là Bullet thật
            if (bulletID != -1)
            {
                SkillManager.bullets[bulletID] = bullet;
            }
        }

        /// <summary>
        /// Tạo đạn bay từ vị trí fromPos đuổi theo mục tiêu với vận tốc tương ứng
        /// </summary>
        /// <param name="caster">Đối tượng xuất chiêu</param>
        /// <param name="delay">Thời gian Delay đến khi tạo</param>
        /// <param name="bulletID">ID đạn truyền từ GS</param>
        /// <param name="resID">ID Res</param>
        /// <param name="fromPos">Vị trí bắt đầu</param>
        /// <param name="chaseTarget">Mục tiêu đuổi</param>
        /// <param name="velocity">Vận tốc bay</param>
        /// <param name="maxLifeTime">Thời gian tồn tại tối đa</param>
        /// <param name="animationLifeTime">Thời gian hiệu ứng</param>
        /// <param name="repeatAnimation">Lặp đi lặp lại hiệu ứng đạn bay cho tới khi tan biến</param>
        public static void CreateBulletChaseTarget(GSprite caster, float delay, int bulletID, int resID, Vector2 fromPos, GameObject chaseTarget, float velocity, bool repeatAnimation = true)
        {
            /// Nếu FPS thấp thì không thực hiện
            if (MainGame.Instance.GetRenderQuality() == MainGame.RenderQuality.Low)
            {
                return;
            }

            /// Nếu là Bullet thật
            if (bulletID != -1)
            {
                /// Nếu tồn tại tia đạn cũ thì xóa
                if (SkillManager.bullets.TryGetValue(bulletID, out Bullet oldBullet))
                {
                    oldBullet.Destroy();
                    SkillManager.bullets.Remove(bulletID);
                }
            }

            /// Nếu thiết lập hệ thống không hiển thị đạn
            if (KTSystemSetting.HideSkillBullet)
            {
                return;
            }

            /// Nếu vận tốc quá nhỏ thì BUG gì đó
            if (velocity <= 10)
			{
                return;
			}

            Bullet bullet = KTObjectPoolManager.Instance.Instantiate<Bullet>("Bullet");
            /// Nếu không thể tạo Bullet do Pool đã đầy hoặc lỗi gì đó
            if (bullet == null)
            {
                return;
            }
            bullet.ResID = resID;
            bullet.RefreshAction();
            bullet.CreateTick = KTGlobal.GetCurrentTimeMilis();
            bullet.Caster = caster;
            bullet.FromPos = fromPos;
            bullet.ChaseTarget = chaseTarget;
            bullet.Velocity = velocity;
            bullet.RepeatAnimation = repeatAnimation;
            bullet.Delay = delay;
            bullet.Destroyed = () => {
                SkillManager.DestroyBullet(bulletID);
            };
            bullet.Fly();

            /// Nếu là Bullet thật
            if (bulletID != -1)
            {
                SkillManager.bullets[bulletID] = bullet;
            }
        }

        /// <summary>
        /// Tạo đạn ném từ vị trí hiện tại đến tại vị trí tương ứng
        /// </summary>
        /// <param name="delay">Thời gian Delay đến khi tạo</param>
        /// <param name="bulletID">ID đạn truyền từ GS</param>
        /// <param name="resID">ID Res</param>
        /// <param name="position">Vị trí xuất hiện</param>
        /// <param name="maxLifeTime">Thời gian tồn tại tối đa</param>
        /// <param name="velocity">Tốc độ ném</param>
        /// <param name="repeatAnimation">Lặp đi lặp lại hiệu ứng đạn cho tới khi tan biến</param>
        public static void CreateBulletThrowing(GSprite caster, float delay, int bulletID, int resID, Vector2 position, float maxLifeTime, float velocity, bool repeatAnimation = true)
        {
            /// Nếu FPS thấp thì không thực hiện
            if (MainGame.Instance.GetRenderQuality() == MainGame.RenderQuality.Low)
            {
                return;
            }

            /// Nếu là Bullet thật
            if (bulletID != -1)
            {
                /// Nếu tồn tại tia đạn cũ thì xóa
                if (SkillManager.bullets.TryGetValue(bulletID, out Bullet oldBullet))
                {
                    oldBullet.Destroy();
                    SkillManager.bullets.Remove(bulletID);
                }
            }

            /// Nếu thiết lập hệ thống không hiển thị đạn
            if (KTSystemSetting.HideSkillBullet)
            {
                return;
            }

            Bullet bullet = KTObjectPoolManager.Instance.Instantiate<Bullet>("Bullet");
            /// Nếu không thể tạo Bullet do Pool đã đầy hoặc lỗi gì đó
            if (bullet == null)
            {
                return;
            }
            bullet.ResID = resID;
            bullet.RefreshAction();
            bullet.CreateTick = KTGlobal.GetCurrentTimeMilis();
            bullet.Caster = caster;
            bullet.FromPos = position;
            bullet.ToPos = position;
            bullet.MaxLifeTime = maxLifeTime;
            bullet.Velocity = velocity;
            bullet.RepeatAnimation = repeatAnimation;
            bullet.Delay = delay;
            bullet.IsTrap = false;
            bullet.IsThrowing = true;
            if (bulletID != -1)
            {
                bullet.Destroyed = () => {
                    SkillManager.DestroyBullet(bulletID);
                };
            }
            bullet.Fly();

            /// Nếu là Bullet thật
            if (bulletID != -1)
            {
                SkillManager.bullets[bulletID] = bullet;
            }
        }

        /// <summary>
        /// Tạo đạn đứng yên tại vị trí tương ứng
        /// </summary>
        /// <param name="delay">Thời gian Delay đến khi tạo</param>
        /// <param name="bulletID">ID đạn truyền từ GS</param>
        /// <param name="resID">ID Res</param>
        /// <param name="position">Vị trí xuất hiện</param>
        /// <param name="maxLifeTime">Thời gian tồn tại tối đa</param>
        /// <param name="repeatAnimation">Lặp đi lặp lại hiệu ứng đạn cho tới khi tan biến</param>
        /// <param name="isTrap">Có phải bẫy không</param>
        public static void CreateBulletStatic(GSprite caster, float delay, int bulletID, int resID, Vector2 position, float maxLifeTime, bool repeatAnimation = true, bool isTrap = false)
        {
            /// Nếu FPS thấp thì không thực hiện
            if (MainGame.Instance.GetRenderQuality() == MainGame.RenderQuality.Low)
            {
                return;
            }

            /// Nếu là Bullet thật
            if (bulletID != -1)
            {
                /// Nếu tồn tại tia đạn cũ thì xóa
                if (SkillManager.bullets.TryGetValue(bulletID, out Bullet oldBullet))
                {
                    oldBullet.Destroy();
                    SkillManager.bullets.Remove(bulletID);
                }
            }

            /// Nếu thiết lập hệ thống không hiển thị đạn
            if (KTSystemSetting.HideSkillBullet)
            {
                return;
            }

            Bullet bullet = KTObjectPoolManager.Instance.Instantiate<Bullet>("Bullet");
            /// Nếu không thể tạo Bullet do Pool đã đầy hoặc lỗi gì đó
            if (bullet == null)
            {
                return;
            }
            bullet.ResID = resID;
            bullet.RefreshAction();
            bullet.CreateTick = KTGlobal.GetCurrentTimeMilis();
            bullet.Caster = caster;
            bullet.FromPos = position;
            bullet.ToPos = position;
            bullet.MaxLifeTime = maxLifeTime;
            bullet.Velocity = 0f;
            bullet.RepeatAnimation = repeatAnimation;
            bullet.Delay = delay;
            bullet.IsTrap = isTrap;
            if (bulletID != -1)
            {
                bullet.Destroyed = () => {
                    SkillManager.DestroyBullet(bulletID);
                };
            }
            bullet.Fly();

            /// Nếu là Bullet thật
            if (bulletID != -1)
            {
                SkillManager.bullets[bulletID] = bullet;
            }
        }

        /// <summary>
        /// Tạo hiệu ứng đạn nổ tại vị trí tương ứng
        /// </summary>
        /// <param name="delay">Thời gian Delay</param>
        /// <param name="bulletResID">ID Res của đạn</param>
        /// <param name="position">Vị trí xuất hiện</param>
        public static void CreateExplosion(float delay, int bulletResID, Vector2 position, GameObject target = null)
        {
            /// Nếu FPS thấp thì không thực hiện
            if (MainGame.Instance.GetRenderQuality() == MainGame.RenderQuality.Low)
            {
                return;
            }

            /// Nếu thiết lập hệ thống không hiển thị hiệu ứng nổ
            if (KTSystemSetting.HideSkillExplodeEffect)
            {
                return;
            }

            /// Nếu có mục tiêu
            if (target != null)
            {
                if (SkillManager.explodeEffects.TryGetValue(target, out Dictionary<int, ExplodeEffect> listExplosion))
                {
                    /// Nếu trên mục tiêu vẫn còn hiệu ứng nổ
                    if (listExplosion.ContainsKey(bulletResID))
                    {
                        /// Hiệu ứng nổ cũ
                        ExplodeEffect oldExplodeEffect = listExplosion[bulletResID];
                        /// Thực hiện xóa sau khoảng Delay tương ứng
                        KTTimerManager.Instance.SetTimeout(delay, () => {
                            /// Nếu hiệu ứng cũ vẫn tồn tại
                            if (oldExplodeEffect != null && oldExplodeEffect.gameObject.activeSelf)
                            {
                                /// Thực hiện xóa
                                oldExplodeEffect.Destroy();
                            }
                        });

                        ///// Xóa hiệu ứng nổ cũ
                        //listExplosion[bulletResID].Destroy();
                        /// Xóa hiệu ứng nổ khỏi danh sách
                        listExplosion.Remove(bulletResID);
                    }
                }
                else
                {
                    SkillManager.explodeEffects[target] = new Dictionary<int, ExplodeEffect>();
                }
            }

            ExplodeEffect explodeEffect = KTObjectPoolManager.Instance.Instantiate<ExplodeEffect>("ExplodeEffect");
            /// Nếu không thể tạo Effect do Pool đã đầy hoặc lỗi gì đó
            if (explodeEffect == null)
            {
                return;
            }
            if (target != null)
            {
                /// Thêm vào danh sách hiệu ứng nổ trên đối tượng
                SkillManager.explodeEffects[target][bulletResID] = explodeEffect;
            }
            explodeEffect.CreateTick = KTGlobal.GetCurrentTimeMilis();
            explodeEffect.ResID = bulletResID;
            explodeEffect.Position = position;
            explodeEffect.Delay = delay;
            explodeEffect.Target = target;
            explodeEffect.Destroyed = () => {
                /// Nếu có mục tiêu
                if (target != null)
                {
                    if (SkillManager.explodeEffects.TryGetValue(target, out Dictionary<int, ExplodeEffect> listExplosion))
                    {
                        /// Nếu trên mục tiêu vẫn còn hiệu ứng nổ
                        if (listExplosion.ContainsKey(bulletResID))
                        {
                            /// Xóa hiệu ứng nổ khỏi danh sách
                            listExplosion.Remove(bulletResID);
                        }
                    }
                }
            };
            explodeEffect.RefreshAction();
            explodeEffect.Play();
        }

        /// <summary>
        /// Xóa đạn bay với ID tương ứng
        /// </summary>
        /// <param name="bulletID"></param>
        private static void DestroyBullet(int bulletID)
        {
            if (SkillManager.bullets.TryGetValue(bulletID, out Bullet bullet))
            {
                SkillManager.bullets.Remove(bulletID);
            }
        }

        /// <summary>
        /// Xóa đạn ngay lập tức
        /// </summary>
        /// <param name="bullet"></param>
        public static void DestroyBulletImmediately(int bulletID)
        {
            if (SkillManager.bullets.TryGetValue(bulletID, out Bullet bullet))
            {
                bullet.Destroy();
            }
        }

        /// <summary>
        /// Xóa đạn ngay lập tức
        /// </summary>
        /// <param name="bullet"></param>
        public static void DestroyBulletImmediately(Bullet bullet)
        {
            bullet.Destroy();
        }

        /// <summary>
        /// Xóa hiệu ứng nổ ngay lập tức
        /// </summary>
        /// <param name="explode"></param>
        public static void DestroyExplosionImmediately(ExplodeEffect explode)
        {
            explode.Destroy();
        }
        #endregion

        #region Effect
        /// <summary>
        /// Tạo hiệu ứng có ResID tương ứng
        /// </summary>
        /// <param name="resID"></param>
        /// <param name="owner"></param>
        public static Effect CreateEffect(int resID, GameObject owner)
        {
            ///// Nếu FPS thấp thì không thực hiện
            //if (MainGame.Instance.GetRenderQuality() == MainGame.RenderQuality.Low)
            //{
            //    return null;
            //}

            if (!Loader.Loader.ListEffects.TryGetValue(resID, out Entities.Config.StateEffectXML effectData))
            {
                return null;
            }

            Effect effect = KTObjectPoolManager.Instance.Instantiate<Effect>("Effect");
            /// Nếu không thể tạo Effect do Pool đã đầy hoặc lỗi gì đó
            if (effect == null)
            {
                return null;
            }
            effect.Data = effectData;
            effect.Owner = owner;

            return effect;
        }
        #endregion

        #region Sound
        /// <summary>
        /// Danh sách âm thanh hiệu ứng đang được phát
        /// </summary>
        private static readonly Dictionary<string, AudioPlayer> playingSounds = new Dictionary<string, AudioPlayer>();

        /// <summary>
        /// Tạo một đối tượng phát âm thanh không lặp lại, nếu đã tồn tại thì không làm gì
        /// </summary>
        /// <param name="soundName"></param>
        public static bool CreateStandaloneAudioPlayer(string soundName, AudioClip audio)
        {
            /// Nếu FPS thấp thì không thực hiện
            if (MainGame.Instance.GetRenderQuality() == MainGame.RenderQuality.Low)
            {
                return false;
            }

            if (SkillManager.playingSounds.TryGetValue(soundName, out _))
            {
                return false;
            }

            AudioPlayer player = KTObjectPoolManager.Instance.Instantiate<AudioPlayer>("StandaloneAudioPlayer");
            /// Nếu không thể tạo AudioPlayer do Pool đã đầy hoặc lỗi gì đó
            if (player == null)
            {
                return false;
            }
            SkillManager.playingSounds[soundName] = player;
            player.Sound = audio;
            player.IsRepeat = false;
            player.RepeatTimer = 0;
            player.OnStop = () => {
                SkillManager.playingSounds.Remove(soundName);
                player.Destroy();
            };
            player.Volume = KTSystemSetting.SkillVolume / 100f;
            player.Play();
            return true;
        }
        #endregion

        #region Garbage Collection
        /// <summary>
        /// Luồng tự dọn rác
        /// </summary>
        private static Coroutine autoCollectGarbageCoroutine = null;

        /// <summary>
        /// Tự thu dọn tài nguyên không dùng
        /// </summary>
        /// <returns></returns>
        private static IEnumerator AutoCollectGarbage()
        {
            /// Lặp liên tục
            while (true)
            {
                /// Duyệt danh sách Bullet đã tạo ra và kiểm tra tự xóa nếu cần
                {
                    List<int> keys = SkillManager.bullets.Keys.ToList();
                    foreach (int key in keys)
                    {
                        /// Lỗi gì đó dẫn tới không tự xóa
                        if (KTGlobal.GetCurrentTimeMilis() - SkillManager.bullets[key].CreateTick >= (!SkillManager.bullets[key].IsTrap ? SkillManager.BulletKeepTicks : SkillManager.TrapKeepTicks))
                        {
                            /// Xóa ngay lập tức
                            SkillManager.DestroyBulletImmediately(SkillManager.bullets[key]);
                            /// Xóa khỏi danh sách
                            SkillManager.bullets.Remove(key);
                        }
                    }
                }

                /// Duyệt danh sách hiệu ứng nổ đã tạo ra và kiểm tra tự xóa nếu cần
                {
                    List<GameObject> _keys = SkillManager.explodeEffects.Keys.ToList();
                    foreach (GameObject _key in _keys)
                    {
                        List<int> keys = SkillManager.explodeEffects[_key].Keys.ToList();
                        /// Duyệt danh sách hiệu ứng nổ
                        foreach (int key in keys)
                        {
                            /// Lỗi gì đó không tự xóa
                            if (KTGlobal.GetCurrentTimeMilis() - SkillManager.explodeEffects[_key][key].CreateTick >= SkillManager.ExplodeEffectKeepTicks)
                            {
                                /// Xóa ngay lập tức
                                SkillManager.DestroyExplosionImmediately(SkillManager.explodeEffects[_key][key]);
                                /// Xóa khỏi danh sách
                                SkillManager.explodeEffects[_key].Remove(key);
                            }
                        }
                        /// Nếu danh sách hiệu ứng rỗng
                        if (SkillManager.explodeEffects[_key].Count <= 0)
						{
                            SkillManager.explodeEffects.Remove(_key);

                        }
                    }
                }
                /// Đợi khoảng
                yield return new WaitForSeconds(SkillManager.AutoCollectGarbageSec);
            }
        }
        #endregion
    }
}
