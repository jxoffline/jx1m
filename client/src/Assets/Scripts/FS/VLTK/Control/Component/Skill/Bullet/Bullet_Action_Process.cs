using FS.VLTK.Entities.Config;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace FS.VLTK.Control.Component.Skill
{
    /// <summary>
    /// Quản lý động tác
    /// </summary>
    public partial class Bullet
    {
        /// <summary>
        /// Thực hiện động tác
        /// </summary>
        private void PlayAnimation()
		{
            /// Ngừng động tác cũ
            this.StopAnimation();
            /// Thời gian thực thi hiệu ứng
            float animDuration = this.AnimationLifeTime;
            /// Thông tin Bullet
            if (Loader.Loader.BulletActionSetXML.ResDatas.TryGetValue(this.ResID, out BulletActionSetXML.BulletResData resData))
            {
                animDuration = resData.FlyAnimDuration / 18f;
                if (animDuration < 0.1f)
                {
                    animDuration = 0.1f;
                }
                /// Kích hoạt hiệu ứng đổ bóng nếu có
                if (resData.UseTrailEffect)
                {
                    this.Trail_Body.Period = resData.TrailPeriod / 18f;
                    this.Trail_Body.Duration = resData.TrailDuration / 18f;
                    this.ActivateTrailEffect(true);
                }
            }
            /// Thực thi động tác mới
            this.actionCoroutine = this.StartCoroutine(this.animation.DoFlyAsync(this.Direction8, this.Direction, this.Direction32, animDuration, this.RepeatAnimation));
        }

        /// <summary>
        /// Ngừng thực hiện động tác
        /// </summary>
        private void StopAnimation()
		{
            /// Nếu có động tác cũ
            if (this.actionCoroutine != null)
            {
                this.StopCoroutine(this.actionCoroutine);
                this.actionCoroutine = null;
            }
        }

        /// <summary>
        /// Thực hiện hiệu ứng tan biến
        /// </summary>
        private void PlayFadeOut()
		{
            /// Ngừng động tác cũ
            this.StopAnimation();
            /// Thời gian thực thi hiệu ứng
            float animDuration = this.AnimationLifeTime;
            /// Thông tin Bullet
            if (Loader.Loader.BulletActionSetXML.ResDatas.TryGetValue(this.ResID, out BulletActionSetXML.BulletResData resData))
            {
                animDuration = resData.FadeOutAnimDuration / 18f;
                if (animDuration < 0.1f)
                {
                    animDuration = 0.1f;
                }
            }
            /// Thực thi động tác mới
            this.actionCoroutine = this.StartCoroutine(this.animation.DoFadeOutAsync(this.Direction8, this.Direction, this.Direction32, animDuration, false, 0f, false, () => {
                /// Ngừng thực hiện động tác
                this.StopAnimation();
                /// Tự hủy
                this.Destroy();
            }));
        }

        /// <summary>
        /// Thực hiện di chuyển theo đường thẳng giữa 2 vị trí
        /// </summary>
        /// <param name="fromPos"></param>
        /// <param name="toPos"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        private IEnumerator LinearMove(Vector2 fromPos, Vector2 toPos, float duration)
		{
            /// Thời gian tồn tại
            float lifeTime = 0f;

            /// Lặp liên tục chừng nào chưa hết thời gian
            while (lifeTime <= duration)
            {
                /// % thời gian đã qua
                float percent = lifeTime / duration;
                /// Vị trí mới
                Vector2 newPos = Vector2.Lerp(fromPos, toPos, percent);
                /// Cập nhật vị trí mới
                this.transform.localPosition = newPos;
                /// Tăng thời gian đã thực thi
                lifeTime += Time.deltaTime;
                /// Bỏ qua Frame
                yield return null;
            }

            /// Cập nhật vị trí đích
            this.transform.localPosition = toPos;
        }

        /// <summary>
        /// Thực hiện bay theo đường tròn
        /// </summary>
        /// <returns></returns>
        private IEnumerator DoCircleFly()
        {
            /// Sử dụng 32 hướng không
            bool use32Dir = false;
            /// Sử dụng 16 hướng không
            bool use16Dir = false;
            /// Sử dụng 8 hướng không
            bool use8Dir = false;
            /// Dữ liệu từ File XML
            if (Loader.Loader.BulletActionSetXML.ResDatas.TryGetValue(this.ResID, out BulletActionSetXML.BulletResData resData))
            {
                use32Dir = resData.Use32Dir;
                use16Dir = resData.Use16Dir;
                use8Dir = resData.Use8Dir;
            }

            /// Thực hiện hiệu ứng
            if (!this.animation.IsPausing)
            {
                this.PlayAnimation();
            }

            /// Thời gian tồn tại
            float lifeTime = 0f;
            /// Thời gian đổi hướng
            float timeChangedDir = 0f;
            /// Lặp liên tục
            while (true)
            {
                /// Nếu quá thời gian
                if (lifeTime >= this.MaxLifeTime)
                {
                    /// Thoát
                    break;
                }

                /// Tọa độ tâm
                Vector2 centerPos = this.FromPos;
                /// Nếu theo đối tượng ra chiêu
                if (this.CircleFollowCaster)
                {
                    /// Cập nhật bằng vị trí của đối tượng ra chiêu
                    centerPos = this.Caster.PositionInVector2;
                }

                /// % thời gian đã qua
                float percent = lifeTime / this.MaxLifeTime;
                /// Tổng quãng đường đi được
                float totalDistance = lifeTime * this.Velocity;
                /// Góc quay tương ứng đi được
                float angle = 180 * totalDistance / (Mathf.PI * this.CircleMoveRadius);

                /// Vị trí cũ
                Vector2 oldPos = this.transform.localPosition;
                /// Vector chỉ hướng từ tâm đến vị trí đang đến
                Vector2 currentVector = KTMath.RotateVector(this.CircleDirVector, angle);
                /// Đảo chiều
                currentVector = -currentVector;
                /// Vị trí
                Vector2 currentPos = KTMath.FindPointInVectorWithDistance(KTMath.FindPointInVectorWithDistance(centerPos, this.CircleDirVector, this.CircleMoveRadius), currentVector, this.CircleMoveRadius);
                /// Vector chỉ hướng di chuyển
                Vector2 moveVector = currentPos - oldPos;
                /// Nếu không tồn tại
                if (percent <= 0)
                {
                    /// Mặc định
                    moveVector = Vector2.left;
                }
                /// Cập nhật vị trí mới
                this.transform.localPosition = currentPos;

                /// Cập nhật thời gian lần trước đổi hướng
                timeChangedDir += Time.deltaTime;
                /// Nếu đã đến thời gian đổi hướng
                if (timeChangedDir >= 0.1f)
                {
                    /// Góc quay tương ứng của tia đạn
                    float degree = KTMath.GetAngle360WithXAxis(moveVector);
                    /// Nếu sử dụng 8 hướng
                    if (use8Dir)
                    {
                        /// Hướng cũ
                        Entities.Enum.Direction oldDir = this.Direction8;
                        /// Hướng bay tương ứng
                        this.Direction8 = KTMath.GetDirectionByAngle360(degree);
                        /// Không có hướng quay 32 hướng
                        this.Direction32 = Entities.Enum.Direction32.None;
                        /// Không có hướng quay 16 hướng
                        this.Direction = Entities.Enum.Direction16.None;

                        /// Nếu hướng thay đổi
                        if (oldDir != this.Direction8)
                        {
                            /// Thực hiện biểu diễn hiệu ứng
                            if (!this.animation.IsPausing)
                            {
                                this.PlayAnimation();
                            }
                        }
                    }
                    /// Nếu sử dụng 16 hướng
                    else if (use16Dir)
                    {
                        /// Hướng cũ
                        Entities.Enum.Direction16 oldDir = this.Direction;
                        /// Hướng bay tương ứng
                        this.Direction = KTMath.GetDirection16ByAngle360(degree);
                        /// Không có hướng quay 32 hướng
                        this.Direction32 = Entities.Enum.Direction32.None;
                        /// Không có hướng quay 8 hướng
                        this.Direction8 = Entities.Enum.Direction.NONE;

                        /// Nếu hướng thay đổi
                        if (oldDir != this.Direction)
                        {
                            /// Thực hiện biểu diễn hiệu ứng
                            if (!this.animation.IsPausing)
                            {
                                this.PlayAnimation();
                            }
                        }
                    }
                    /// Nếu sử dụng 32 hướng
                    else if (use32Dir)
                    {
                        /// Hướng cũ
                        Entities.Enum.Direction32 oldDir = this.Direction32;
                        /// Không có hướng quay 16 hướng
                        this.Direction = Entities.Enum.Direction16.None;
                        /// Hướng quay 32 hướng
                        this.Direction32 = KTMath.GetDirection32ByAngle360(degree);
                        /// Không có hướng quay 8 hướng
                        this.Direction8 = Entities.Enum.Direction.NONE;

                        /// Nếu hướng thay đổi
                        if (oldDir != this.Direction32)
                        {
                            /// Thực hiện biểu diễn hiệu ứng
                            if (!this.animation.IsPausing)
                            {
                                this.PlayAnimation();
                            }
                        }
                    }
                    /// Toác
                    else
                    {
                        /// Không có hướng quay 8 hướng
                        this.Direction8 = Entities.Enum.Direction.NONE;
                        /// Không có hướng quay 16 hướng
                        this.Direction = Entities.Enum.Direction16.None;
                        /// Không có hướng quay 32 hướng
                        this.Direction32 = Entities.Enum.Direction32.None;

                        /// Nếu có tự xoay
                        if (resData.AutoRotate)
                        {
                            /// Góc xoay
                            float _angle = KTMath.GetDir16Angle(degree);
                            /// Xoay
                            this.Body.transform.localRotation = Quaternion.Euler(0, 0, _angle);
                        }
                    }
                    /// Reset thời gian đổi hướng
                    timeChangedDir = 0f;
                }

                /// Nếu có đích đến
                if (this.ToPos != default)
                {
                    /// Nếu đã đến đích
                    if (Vector2.Distance(currentPos, this.ToPos) <= 10f)
                    {
                        /// Ngừng thực hiện động tác
                        this.StopAnimation();
                        /// Tự hủy
                        this.Destroy();
                        break;
                    }
                }

                /// Bỏ qua Frame
                yield return null;
                /// Tăng thời gian tồn tại
                lifeTime += Time.deltaTime;
            }

            /// Ngừng thực hiện động tác
            this.StopAnimation();
            /// Tự hủy
            this.Destroy();
            /// Hủy luồng bay
            this.flyCoroutine = null;
        }

        /// <summary>
        /// Thực hiện bay từ vị trí ra chiêu đến vị trí xuất phát
        /// </summary>
        /// <returns></returns>
        private IEnumerator DoFlyToStartPos()
        {
            /// Sử dụng 32 hướng không
            bool use32Dir = false;
            /// Sử dụng 16 hướng không
            bool use16Dir = false;
            /// Sử dụng 8 hướng không
            bool use8Dir = false;
            /// Dữ liệu từ File XML
            if (Loader.Loader.BulletActionSetXML.ResDatas.TryGetValue(this.ResID, out BulletActionSetXML.BulletResData resData))
            {
                use32Dir = resData.Use32Dir;
                use16Dir = resData.Use16Dir;
                use8Dir = resData.Use8Dir;
            }

            /// Vector chỉ hướng bay
            Vector2 dirVector = this.FromPos - this.Caster.PositionInVector2;
            /// Góc quay tương ứng của tia đạn
            float degree = KTMath.GetAngle360WithXAxis(dirVector);
            /// Nếu sử dụng 8 hướng
            if (use8Dir)
            {
                /// Hướng bay tương ứng
                this.Direction8 = KTMath.GetDirectionByAngle360(degree);
                /// Không có hướng quay 32 hướng
                this.Direction32 = Entities.Enum.Direction32.None;
                /// Không có hướng quay 16 hướng
                this.Direction = Entities.Enum.Direction16.None;
            }
            /// Nếu sử dụng 16 hướng
            else if (use16Dir)
            {
                /// Hướng bay tương ứng
                this.Direction = KTMath.GetDirection16ByAngle360(degree);
                /// Không có hướng quay 32 hướng
                this.Direction32 = Entities.Enum.Direction32.None;
                /// Không có hướng quay 8 hướng
                this.Direction8 = Entities.Enum.Direction.NONE;
            }
            /// Nếu sử dụng 32 hướng
            else if (use32Dir)
            {
                /// Không có hướng quay 16 hướng
                this.Direction = Entities.Enum.Direction16.None;
                /// Hướng quay 32 hướng
                this.Direction32 = KTMath.GetDirection32ByAngle360(degree);
                /// Không có hướng quay 8 hướng
                this.Direction8 = Entities.Enum.Direction.NONE;
            }
            /// Toác
            else
            {
                /// Không có hướng quay 8 hướng
                this.Direction8 = Entities.Enum.Direction.NONE;
                /// Không có hướng quay 16 hướng
                this.Direction = Entities.Enum.Direction16.None;
                /// Không có hướng quay 32 hướng
                this.Direction32 = Entities.Enum.Direction32.None;

                /// Nếu có tự xoay
                if (resData.AutoRotate)
                {
                    /// Góc xoay
                    float angle = KTMath.GetDir16Angle(degree);
                    /// Xoay
                    this.Body.transform.localRotation = Quaternion.Euler(0, 0, angle);
                }
            }

            /// Thời gian bay
            float duration = Math.Min(Vector2.Distance(this.FromPos, this.Caster.PositionInVector2) / this.Velocity, this.MaxLifeTime);
            duration = Math.Max(duration, 0.1f);

            /// Thực hiện hiệu ứng
            if (!this.animation.IsPausing)
            {
                this.PlayAnimation();
            }

            /// Thực hiện di chuyển
            yield return this.LinearMove(this.Caster.PositionInVector2, this.FromPos, duration);
        }

        /// <summary>
        /// Thực hiện bay từ vị trí A đến vị trí B
        /// </summary>
        /// <returns></returns>
        private IEnumerator DoLinearFly()
		{
            /// Nếu phải bay từ vị trí ra chiêu trước
            if (this.FlyToStartPosFirst)
            {
                /// Thực hiện bay
                yield return this.DoFlyToStartPos();
            }

            /// Sử dụng 32 hướng không
            bool use32Dir = false;
            /// Sử dụng 16 hướng không
            bool use16Dir = false;
            /// Sử dụng 8 hướng không
            bool use8Dir = false;
            /// Dữ liệu từ File XML
            if (Loader.Loader.BulletActionSetXML.ResDatas.TryGetValue(this.ResID, out BulletActionSetXML.BulletResData resData))
            {
                use32Dir = resData.Use32Dir;
                use16Dir = resData.Use16Dir;
                use8Dir = resData.Use8Dir;
            }

            /// Vector chỉ hướng bay
            Vector2 dirVector = this.ToPos - this.FromPos;
            /// Góc quay tương ứng của tia đạn
            float degree = KTMath.GetAngle360WithXAxis(dirVector);
            /// Nếu sử dụng 8 hướng
            if (use8Dir)
            {
                /// Hướng bay tương ứng
                this.Direction8 = KTMath.GetDirectionByAngle360(degree);
                /// Không có hướng quay 32 hướng
                this.Direction32 = Entities.Enum.Direction32.None;
                /// Không có hướng quay 16 hướng
                this.Direction = Entities.Enum.Direction16.None;
            }
            /// Nếu sử dụng 16 hướng
            else if (use16Dir)
            {
                /// Hướng bay tương ứng
                this.Direction = KTMath.GetDirection16ByAngle360(degree);
                /// Không có hướng quay 32 hướng
                this.Direction32 = Entities.Enum.Direction32.None;
                /// Không có hướng quay 8 hướng
                this.Direction8 = Entities.Enum.Direction.NONE;
            }
            /// Nếu sử dụng 32 hướng
            else if (use32Dir)
            {
                /// Không có hướng quay 16 hướng
                this.Direction = Entities.Enum.Direction16.None;
                /// Hướng quay 32 hướng
                this.Direction32 = KTMath.GetDirection32ByAngle360(degree);
                /// Không có hướng quay 8 hướng
                this.Direction8 = Entities.Enum.Direction.NONE;
            }
            /// Toác
            else
            {
                /// Không có hướng quay 8 hướng
                this.Direction8 = Entities.Enum.Direction.NONE;
                /// Không có hướng quay 16 hướng
                this.Direction = Entities.Enum.Direction16.None;
                /// Không có hướng quay 32 hướng
                this.Direction32 = Entities.Enum.Direction32.None;

                /// Nếu có tự xoay
                if (resData.AutoRotate)
                {
                    /// Góc xoay
                    float angle = KTMath.GetDir16Angle(degree);
                    /// Xoay
                    this.Body.transform.localRotation = Quaternion.Euler(0, 0, angle);
                }
            }
            
            /// Thời gian bay
            float duration = Math.Min(Vector2.Distance(this.FromPos, this.ToPos) / this.Velocity, this.MaxLifeTime);
            duration = Math.Max(duration, 0.1f);

            /// Thực hiện hiệu ứng
            if (!this.animation.IsPausing)
			{
                this.PlayAnimation();
            }

            /// Thực hiện di chuyển
            yield return this.LinearMove(this.FromPos, this.ToPos, duration);

            /// Nếu quay trở lại
            if (this.Comeback)
            {
                /// Thực hiện di chuyển
                yield return this.LinearMove(this.ToPos, this.FromPos, duration);
            }

            /// Ngừng thực hiện động tác
            this.StopAnimation();
            /// Tự hủy
            this.Destroy();
            /// Hủy luồng bay
            this.flyCoroutine = null;
        }

        /// <summary>
        /// Thực hiện ném từ vị trí ra chiêu đến vị trí B
        /// </summary>
        /// <returns></returns>
        private IEnumerator DoThrow()
        {
            /// Sử dụng 32 hướng không
            bool use32Dir = false;
            /// Sử dụng 16 hướng không
            bool use16Dir = false;
            /// Sử dụng 8 hướng không
            bool use8Dir = false;
            /// Dữ liệu từ File XML
            if (Loader.Loader.BulletActionSetXML.ResDatas.TryGetValue(this.ResID, out BulletActionSetXML.BulletResData resData))
            {
                use32Dir = resData.Use32Dir;
                use16Dir = resData.Use16Dir;
                use8Dir = resData.Use8Dir;
            }

            /// Vị trí bắt đầu
            Vector2 fromPos = this.Caster.PositionInVector2;
            /// Vector chỉ hướng bay
            Vector2 dirVector = this.ToPos - fromPos;
            /// Góc quay tương ứng của tia đạn
            float degree = KTMath.GetAngle360WithXAxis(dirVector);
            /// Nếu sử dụng 8 hướng
            if (use8Dir)
            {
                /// Hướng bay tương ứng
                this.Direction8 = KTMath.GetDirectionByAngle360(degree);
                /// Không có hướng quay 32 hướng
                this.Direction32 = Entities.Enum.Direction32.None;
                /// Không có hướng quay 16 hướng
                this.Direction = Entities.Enum.Direction16.None;
            }
            /// Nếu sử dụng 16 hướng
            else if (use16Dir)
            {
                /// Hướng bay tương ứng
                this.Direction = KTMath.GetDirection16ByAngle360(degree);
                /// Không có hướng quay 32 hướng
                this.Direction32 = Entities.Enum.Direction32.None;
                /// Không có hướng quay 8 hướng
                this.Direction8 = Entities.Enum.Direction.NONE;
            }
            /// Nếu sử dụng 32 hướng
            else if (use32Dir)
            {
                /// Không có hướng quay 16 hướng
                this.Direction = Entities.Enum.Direction16.None;
                /// Hướng quay 32 hướng
                this.Direction32 = KTMath.GetDirection32ByAngle360(degree);
                /// Không có hướng quay 8 hướng
                this.Direction8 = Entities.Enum.Direction.NONE;
            }
            /// Toác
            else
            {
                /// Không có hướng quay 8 hướng
                this.Direction8 = Entities.Enum.Direction.NONE;
                /// Không có hướng quay 16 hướng
                this.Direction = Entities.Enum.Direction16.None;
                /// Không có hướng quay 32 hướng
                this.Direction32 = Entities.Enum.Direction32.None;

                /// Nếu có tự xoay
                if (resData.AutoRotate)
                {
                    /// Góc xoay
                    float angle = KTMath.GetDir16Angle(degree);
                    /// Xoay
                    this.Body.transform.localRotation = Quaternion.Euler(0, 0, angle);
                }
            }

            /// Thời gian bay
            float duration = Math.Min(Vector2.Distance(fromPos, this.ToPos) / this.Velocity, this.MaxLifeTime);
            duration = Math.Max(duration, 0.1f);

            float high = Mathf.Min(150, Vector2.Distance(fromPos, this.ToPos) / 2);
            float vertexY = Mathf.Max(fromPos.y, this.ToPos.y) + high;
            KTMath.Parabol parabol = KTMath.GetParabolFromTwoPointsAndVertexY(fromPos, this.ToPos, vertexY, true);

            /// Thực hiện hiệu ứng
            if (!this.animation.IsPausing)
            {
                this.PlayAnimation();
            }

            /// Thời gian tồn tại
            float lifeTime = 0f;
            /// Lặp liên tục
            while (true)
            {
                /// Nếu đã quả thời gian
                if (lifeTime >= duration)
                {
                    /// Thoát
                    break;
                }
                /// % đi được
                float percent = lifeTime / duration;
                /// Vị trí tiếp theo
                Vector2 nextPos = fromPos + dirVector * percent;

                /// Nếu có Parabol
                if (parabol != null)
                {
                    float newY = parabol.A * nextPos.x * nextPos.x + parabol.B * nextPos.x + parabol.C;
                    nextPos.y = newY;
                }
                /// Nếu không có Parabol
                else
                {
                    float newY;
                    /// Nếu là pha ném lên
                    if (percent <= 0.5f)
                    {
                        newY = fromPos.y + (vertexY - fromPos.y) * percent * 2;
                    }
                    /// Nếu là pha đáp xuống
                    else
                    {
                        newY = this.ToPos.y + (vertexY - this.ToPos.y) * (1 - (percent - 0.5f) * 2);
                    }
                    nextPos.y = newY;
                }

                /// Thiết lập vị trí
                this.transform.localPosition = nextPos;

                /// Bỏ qua frame
                yield return null;
                /// Tăng thời gian đã qua
                lifeTime += Time.deltaTime;
            }

            /// Ngừng thực hiện động tác
            this.StopAnimation();
            /// Tự hủy
            this.Destroy();
            /// Hủy luồng bay
            this.flyCoroutine = null;
        }

        /// <summary>
        /// Thực hiện nổ tại vị trí tương ứng
        /// </summary>
        /// <returns></returns>
        private IEnumerator DoStaticExplode()
		{
            int startHeight = 10;
            if (Loader.Loader.BulletConfigs.TryGetValue(this.ResID, out BulletConfig bulletConfig))
            {
                startHeight = bulletConfig.StartHeight;
            }
            startHeight -= 10;

            /// Không có hướng
            this.Direction = Entities.Enum.Direction16.None;
            this.Direction32 = Entities.Enum.Direction32.None;
            this.Direction8 = Entities.Enum.Direction.NONE;

            /// Thực hiện biểu diễn hiệu ứng
            if (!this.animation.IsPausing)
            {
                this.PlayAnimation();
            }

            /// Thời gian thực hiện hiệu ứng rơi
            float duration = bulletConfig != null ? bulletConfig.LifeTime / 18f : this.AnimationLifeTime;

            /// Nếu có độ cao ban đầu
            if (startHeight > 0)
            {
                /// Vị trí bắt đầu rơi
                Vector2 sPos = new Vector2(this.FromPos.x, this.FromPos.y + startHeight);
                /// Thực hiện hiệu ứng rơi từ trên xuống
                yield return this.LinearMove(sPos, this.FromPos, duration);
            }
            else
            {
                /// Thiết lập vị trí
                this.gameObject.transform.localPosition = this.FromPos;

                /// Nếu hiệu ứng không lặp lại thì chờ hết thời gian hiệu ứng
                if (!this.RepeatAnimation)
                {
                    /// Thời gian thực thi hiệu ứng
                    float animDuration = this.AnimationLifeTime;
                    /// Thông tin Bullet
                    if (Loader.Loader.BulletActionSetXML.ResDatas.TryGetValue(this.ResID, out BulletActionSetXML.BulletResData resData))
                    {
                        animDuration = resData.FlyAnimDuration / 18f;
                        if (animDuration < 0.1f)
                        {
                            animDuration = 0.1f;
                        }
                    }
                    yield return new WaitForSeconds(animDuration);
                }
                /// Nếu hiệu ứng lặp lại thì chờ hết thời gian lặp
                else
                {
                    yield return new WaitForSeconds(this.MaxLifeTime);
                }
            }

            /// Thực hiện động tác biến
            this.PlayFadeOut();
            /// Hủy luồng bay
            this.flyCoroutine = null;
        }

        /// <summary>
        /// Thực hiện đạn bay đuổi theo mục tiêu
        /// </summary>
        /// <returns></returns>
        private IEnumerator DoChaseTarget()
        {
            /// Sử dụng 32 hướng không
            bool use32Dir = false;
            /// Sử dụng 16 hướng không
            bool use16Dir = false;
            /// Sử dụng 8 hướng không
            bool use8Dir = false;
            /// Dữ liệu từ File XML
            if (Loader.Loader.BulletActionSetXML.ResDatas.TryGetValue(this.ResID, out BulletActionSetXML.BulletResData resData))
            {
                use32Dir = resData.Use32Dir;
                use16Dir = resData.Use16Dir;
                use8Dir = resData.Use8Dir;
            }

            /// Cập nhật vị trí xuất phát
            this.transform.localPosition = this.FromPos;
            /// Thời gian lần trước đổi hướng
            float timeChangedDir = 0.1f;
            /// Lặp liên tục
            while (true)
			{
                /// Nếu mục tiêu không tồn tại
                if (!this.ChaseTarget || !this.ChaseTarget.activeSelf)
                {
                    /// Ngừng thực hiện động tác
                    this.StopAnimation();
                    /// Tự hủy
                    this.Destroy();
                    break;
                }

                /// Cập nhật thời gian lần trước đổi hướng
                timeChangedDir += Time.deltaTime;
                /// Nếu đã đến thời gian đổi hướng
                if (timeChangedDir >= 0.1f)
                {
                    /// Nếu sử dụng 8 hướng
                    if (use8Dir)
                    {
                        /// Hướng cũ
                        Entities.Enum.Direction oldDir = this.Direction8;
                        /// Vector hướng bay
                        Vector2 dirVector = (Vector2) this.ChaseTarget.transform.localPosition - this.FromPos;
                        /// Góc quay so với trục Ox
                        float degree = KTMath.GetAngle360WithXAxis(dirVector);

                        /// Nếu có tự xoay
                        if (resData.AutoRotate)
                        {
                            /// Góc xoay
                            float angle = KTMath.GetDir16Angle(degree);
                            /// Xoay
                            this.Body.transform.localRotation = Quaternion.Euler(0, 0, angle);
                        }

                        /// Hướng bay hiện tại
                        this.Direction8 = KTMath.GetDirectionByAngle360(degree);

                        /// Nếu hướng thay đổi
                        if (oldDir != this.Direction8)
                        {
                            /// Thực hiện biểu diễn hiệu ứng
                            if (!this.animation.IsPausing)
                            {
                                this.PlayAnimation();
                            }
                        }
                    }
                    /// Nếu sử dụng 16 hướng
                    else if (use16Dir)
                    {
                        /// Hướng cũ
                        Entities.Enum.Direction16 oldDir = this.Direction;
                        /// Vector hướng bay
                        Vector2 dirVector = (Vector2) this.ChaseTarget.transform.localPosition - this.FromPos;
                        /// Góc quay so với trục Ox
                        float degree = KTMath.GetAngle360WithXAxis(dirVector);

                        /// Nếu có tự xoay
                        if (resData.AutoRotate)
                        {
                            /// Góc xoay
                            float angle = KTMath.GetDir16Angle(degree);
                            /// Xoay
                            this.Body.transform.localRotation = Quaternion.Euler(0, 0, angle);
                        }

                        /// Hướng bay hiện tại
                        this.Direction = KTMath.GetDirection16ByAngle360(degree);

                        /// Nếu hướng thay đổi
                        if (oldDir != this.Direction)
                        {
                            /// Thực hiện biểu diễn hiệu ứng
                            if (!this.animation.IsPausing)
                            {
                                this.PlayAnimation();
                            }
                        }
                    }
                    /// Nếu sử dụng 32 hướng
                    else if (use32Dir)
                    {
                        /// Hướng cũ
                        Entities.Enum.Direction32 oldDir = this.Direction32;
                        /// Vector hướng bay
                        Vector2 dirVector = (Vector2) this.ChaseTarget.transform.localPosition - this.FromPos;
                        /// Góc quay so với trục Ox
                        float degree = KTMath.GetAngle360WithXAxis(dirVector);
                        /// Hướng bay hiện tại
                        this.Direction32 = KTMath.GetDirection32ByAngle360(degree);

                        /// Nếu hướng thay đổi
                        if (oldDir != this.Direction32)
                        {
                            /// Thực hiện biểu diễn hiệu ứng
                            if (!this.animation.IsPausing)
                            {
                                this.PlayAnimation();
                            }
                        }
                    } 
                }

                /// Vị trí của mục tiêu
                Vector2 targetPos = this.ChaseTarget.transform.localPosition;
                /// Vị trí hiện tại của tia đạn
                Vector2 currentPos = this.transform.localPosition;
                /// Khoảng dịch chuyển được
                float distance = Mathf.Min(Vector2.Distance(currentPos, targetPos), this.Velocity * Time.deltaTime);
                /// Vị trí dịch đến
                currentPos = Vector2.MoveTowards(currentPos, targetPos, distance);
                /// Thiết lập vị trí
                this.transform.localPosition = currentPos;

                /// Nếu đã chạm mục tiêu
                if (Vector2.Distance(currentPos, targetPos) <= 10f)
                {
                    /// Ngừng thực hiện động tác
                    this.StopAnimation();
                    /// Tự hủy
                    this.Destroy();
                    break;
                }

                /// Bỏ qua Frame
                yield return null;
            }
            /// Hủy luồng bay
            this.flyCoroutine = null;
        }
	}
}
