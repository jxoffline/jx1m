using FS.GameEngine.GoodsPack;
using FS.GameEngine.Logic;
using FS.GameEngine.Sprite;
using FS.VLTK.Control.Component;
using FS.VLTK.Entities;
using FS.VLTK.Entities.Config;
using FS.VLTK.Logic.Settings;
using FS.VLTK.Network;
using GameServer.VLTK.Utilities;
using Server.Data;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK.Logic
{
    /// <summary>
    /// Thực thi tự đánh
    /// </summary>
    public partial class KTAutoFightManager
    {
        /// <summary>
        /// Danh sách lửa trại bị bỏ qua (vì thử đốt mà không được)
        /// </summary>
        private readonly List<GSprite> ignoredFireCamps = new List<GSprite>();

        /// <summary>
        /// Thực hiện tự động đánh
        /// </summary>
        /// <returns></returns>
        private IEnumerator ProcessAutoFight(bool SkipResetStartPoint=false)
        {
            /// Gán thời gian gần đây check bán là khi nào
            this.AutoFightLastCheckAutoSell = KTGlobal.GetCurrentTimeMilis();
            this.TrainResetRadius = Stopwatch.StartNew();

            this.LastArletNoTarget = KTGlobal.GetCurrentTimeMilis();

            /// Nếu đang trong trạng thái bán hàng
            if (Global.Data.StallDataItem != null && Global.Data.StallDataItem.Start == 1 && !Global.Data.StallDataItem.IsBot)
            {
                this.StopAutoFight();
                yield break;
            }

            /// Nếu không có thiết lập kỹ năng đánh
            if (KTAutoAttackSetting.SkillAutoTrain == null || KTAutoAttackSetting.SkillAutoPK == null)
            {
                KTGlobal.AddNotification("Chưa thiết lập chọn kỹ năng tự đánh!");
                this.StopAutoFight();
                yield break;
            }

            List<KeyValuePair<SkillDataEx, SkillData>> autoUseTrainSkills = new List<KeyValuePair<SkillDataEx, SkillData>>();

            List<KeyValuePair<SkillDataEx, SkillData>> autoUsePKSkills = new List<KeyValuePair<SkillDataEx, SkillData>>();

            /// Duyệt danh sách kỹ năng, kiểm tra xem kỹ năng nào thỏa mãn điều kiện thì đưa vào List
            foreach (SkillDataEx autoUseSkill in KTAutoAttackSetting.SkillAutoTrain)
            {
                if (autoUseSkill == null)
                {
                    continue;
                }

                /// Nếu kỹ năng không tồn tại
                if (!Loader.Loader.Skills.TryGetValue(autoUseSkill.ID, out _))
                {
                    continue;
                }
                /// Dữ liệu kỹ năng nhân vật
                SkillData serverData = Global.Data.RoleData.SkillDataList.Where(x => x.SkillID == autoUseSkill.ID).FirstOrDefault();
                /// Nếu chưa học đc kỹ năng tương ứng
                if (serverData == null || serverData.SkillLevel <= 0)
                {
                    continue;
                }
                /// Nếu thiết lập môn phái, hệ phái không phù hợp
                else if (!SkillManager.ValidateUseSkill(autoUseSkill, false))
                {
                    continue;
                }

                serverData.Prioritized = 0;

                /// Thêm kỹ năng vào danh sách được phép dùng
                autoUseTrainSkills.Add(new KeyValuePair<SkillDataEx, SkillData>(autoUseSkill, serverData));
            }

            /// Duyệt danh sách kỹ năng, kiểm tra xem kỹ năng nào thỏa mãn điều kiện thì đưa vào List
            foreach (SkillDataEx autoUseSkill in KTAutoAttackSetting.SkillAutoPK)
            {
                if (autoUseSkill == null)
                {
                    continue;
                }

                /// Nếu kỹ năng không tồn tại
                if (!Loader.Loader.Skills.TryGetValue(autoUseSkill.ID, out _))
                {
                    continue;
                }
                /// Dữ liệu kỹ năng nhân vật
                SkillData serverData = Global.Data.RoleData.SkillDataList.Where(x => x.SkillID == autoUseSkill.ID).FirstOrDefault();
                /// Nếu chưa học đc kỹ năng tương ứng
                if (serverData == null || serverData.SkillLevel <= 0)
                {
                    continue;
                }
                /// Nếu thiết lập môn phái, hệ phái không phù hợp
                else if (!SkillManager.ValidateUseSkill(autoUseSkill, false))
                {
                    continue;
                }

                serverData.Prioritized = 0;

                /// Thêm kỹ năng vào danh sách được phép dùng
                autoUsePKSkills.Add(new KeyValuePair<SkillDataEx, SkillData>(autoUseSkill, serverData));
            }

            /// Mục tiêu trước đó
            this.AutoFightLastTarget = null;
            /// Mức máu lần trước của mục tiêu
            this.AutoFightLastTargetHP = 0;
            /// Thời gian tấn công mục tiêu thành công
            this.AutoFightLastSuccessAttackTick = 0;

            //Số lần vị trí nhân vật đứng bị trùng nhau
            this.BugPostion = 0;
            /// Leader
            GSprite leader = Global.Data.Leader;

            if(!SkipResetStartPoint)
            {
                /// Vị trí ban đầu trước khi dùng Auto
                this.StartPos = leader.PositionInVector2;

            }    
           

            // Nếu là đánh quanh điểm
          
            TmpPostion = leader.PositionInVector2;
            /// Kỹ năng vũ khí tương ứng
            SkillDataEx weaponSkillData = null;

            /// Kỹ năng tân thủ đánh thường quyền
            SkillDataEx newbieHandAttackSkillData = null;
            /// Nếu không tồn tại kỹ năng tân thủ đánh thường quyền
            if (!Loader.Loader.Skills.TryGetValue(KTGlobal.NewbieHandAttackSkill, out newbieHandAttackSkillData))
            {
                KTGlobal.AddNotification("Dữ liệu kỹ năng tân thủ bị lỗi!");
                this.StopAutoFight();
                yield break;
            }

            /// Làm rỗng danh sách bỏ qua lửa trại
            this.ignoredFireCamps.Clear();

            /// Danh sách mục tiêu bị bỏ qua (vì đánh Miss quá nhiều)
            List<GSprite> ignoredTargets = new List<GSprite>();

            //Chế độ dành cho lure quái
            List<GSprite> HitTagetList = new List<GSprite>();

            /// Thông tin ngựa
            GoodsData horseGD = Global.Data.RoleData.GoodsDataList?.Where(x => x.Using == (int)KE_EQUIP_POSITION.emEQUIPPOS_HORSE).FirstOrDefault();

            /// Lặp liên tục để thực hiện Logic
            while (true)
            {
                /// Nếu đã chết
                if (leader.IsDeath || Global.Data.StallDataItem != null)
                {
                    this.StopAutoFight();
                    break;
                }
                /// Nếu bị khống chế hoặc đang chuyển bản đồ
                else if (!leader.CanDoLogic || Global.Data.WaitingForMapChange)
                {
                    /// Bỏ qua Frame
                    yield return null;
                    /// Tiếp tục vòng lặp
                    continue;
                }
                /// Nếu đang đợi gọi pet
                else if (this.IsCallingPet)
                {
                    /// Bỏ qua Frame
                    yield return null;
                    /// Tiếp tục vòng lặp
                    continue;
                }

                if (TmpPostion == leader.PositionInVector2)
                {
                    BugPostion++;
                }
                else
                {
                    BugPostion = 0;
                }

                /// Hủy vị trí mục tiêu kỹ năng tay phải
                Global.Data.GameScene.RemoveSkillMarkTargetPos();

                /// Tự uống rượu
                this.AutoDrinkWine();
                /// Nga My tự buff
                this.AutoEMBuff();
                /// Tự động kích hoạt các kỹ năng Buff chủ động
                yield return this.AutoActivatePositiveBuffs();
                /// Tự làm rỗng danh sách vật phẩm không nhặt
                this.ResetListIgnoreGoodsPack();
                /// Tự mời người chơi khác vào nhóm
                this.ProcessInviteToTeam();
                /// Xóa đống củi bị bỏ qua
                this.RemoveFireCampWoodsFromIgnoreList();

                /// Nếu đang đợi phản hồi dùng kỹ năng từ GS
                if (SkillManager.IsWaitingToUseSkill)
                {
                    /// Bỏ qua Frame
                    yield return null;
                    /// Tiếp tục vòng lặp
                    continue;
                }

                if (TrigerTime.IsRunning)
                {
                    /// Nếu qua 10s set lại trạng thái bị triger là không bị để quay lại việc train quái
                    if (TrigerTime.Elapsed.TotalSeconds >= 20)
                    {
                        this.TrigerAttackID = -1;
                        /// Stop đồng hồ
                        this.TrigerTime.Stop();
                        this.IsTriger = false;
                    }
                }
                /// TẠM BỎ ĐOẠN NÀY CHƯA BIẾT THẾ NÀO CẢ
                //if (!KTLeaderMovingManager.LeaderPositiveMoveToPos.Equals(default) && KTLeaderMovingManager.LeaderPositiveMoveToPos != startPos)
                //{
                //    /// Cập nhật lại
                //    /// vị trí ban đầu trước khi dùng Auto
                //    startPos = KTLeaderMovingManager.LeaderPositiveMoveToPos;
                //}

                /// Nếu không phải đang di chuyển và đã hoàn thành động tác trước đó
                if (!leader.IsMoving && Global.Data.Leader.IsReadyToMove && KTGlobal.FinishedUseSkillAction)
                {
                    /// Tự đốt lửa trại
                    yield return this.AutoFireCamp(this.ignoredFireCamps, (fireCamp) =>
                    {
                        /// Thêm vào danh sách bỏ qua
                        this.ignoredFireCamps.Add(fireCamp);
                    });

                    /// Tự bán vật phẩm nếu đầy túi đồ
                    yield return this.DoSellItem();
                    /// Tự mua thuốc nếu cần
                    yield return this.DoBuyItem();

                    /// Nếu đang bán đồ, gọi pet hoặc đang đốt lửa trại thì bỏ qua hết các thao tác khác
                    if (!this.DoingAutoSell && !this.DoingBuyItem && !this.DoingAutoFireCamp && !this.IsCallingPet)
                    {
                        /// Tự động nhặt vật phẩm nếu có
                        GGoodsPack gp = this.AutoPickUpItems();
                        /// Nếu phát hiện vật phẩm
                        if (gp != null)
                        {
                            /// Thực hiện nhặt
                            Global.Data.GameScene.DropItemClick(gp);
                            /// Thêm vào danh sách không nhặt
                            this.ListIgnoreGoodsPack.Add(gp.BaseID);

                            /// Bỏ qua Frame
                            yield return new WaitForSeconds(this.AutoFight_TickSec * 2);
                            /// Tiếp tục vòng lặp
                            continue;
                        };

                        /// CHUYỂN ĐOẠN CHỌN SKILL ĐÁNH LÊN TRÊN CÙNG để chọn ra skill trước sau đó mới chọn mục tiêu phù hợp với skill này
                        KeyValuePair<SkillDataEx, SkillData> skillInfo = new KeyValuePair<SkillDataEx, SkillData>();

                        /// Nếu ko bị triger và đang ở chế độ train
                        if (!KTAutoAttackSetting.Config.EnableAutoPK && !IsTriger)
                        {
                            /// Chọn 1 kỹ năng không trong trạng thái phục hồi và thỏa mãn điều kiện yêu cầu như nội lực, thể lực,... để sử dụng
                            skillInfo = autoUseTrainSkills.Where(x => !SkillManager.IsSkillCooldown(x.Key.ID) && SkillManager.IsAbleToUseSkill(x.Key, x.Value.Level, false)).OrderBy(x => x.Value.Prioritized).FirstOrDefault();
                        }
                        /// Nếu đang ở chế độ pk hoặc bị trigger
                        else if (KTAutoAttackSetting.Config.EnableAutoPK || IsTriger)
                        {
                            /// Chọn 1 kỹ năng không trong trạng thái phục hồi và thỏa mãn điều kiện yêu cầu như nội lực, thể lực,... để sử dụng
                            skillInfo = autoUsePKSkills.Where(x => !SkillManager.IsSkillCooldown(x.Key.ID) && SkillManager.IsAbleToUseSkill(x.Key, x.Value.Level, false)).OrderBy(x => x.Value.Prioritized).FirstOrDefault();
                        }

                        /// Nếu là chế độ train quái hoặc là chế độ PK nhưng có thiết lập sử dụng base skill thì mới tìm skillt ân thủ
                        if ((!KTAutoAttackSetting.Config.EnableAutoPK && KTAutoAttackSetting.Config.Farm.UseNewbieSkill) || (KTAutoAttackSetting.Config.EnableAutoPK && KTAutoAttackSetting.Config.PK.UseNewbieSkill == true))
                        {
                            /// Nếu không tìm thấy kỹ năng nào tức là tất cả các kỹ năng đều trong trạng thái phục hồi hoặc không đủ điều kiện dùng
                            if (skillInfo.Equals(default) || skillInfo.Key == null || skillInfo.Value == null)
                            {
                                /// Thông báo không tìm thấy kỹ năng
                                if (KTGlobal.GetCurrentTimeMilis() - this.AutoFightLastNotifyNoSkillTick >= this.AutoFight_NotifyNoSkillTick)
                                {
                                    this.AutoFightLastNotifyNoSkillTick = KTGlobal.GetCurrentTimeMilis();

                                    /// Loại vũ khí tương ứng
                                    weaponSkillData = KTGlobal.GetNewbieSkillCorrespondingToCurrentWeapon();

                                    /// Nếu kỹ năng tân thủ không tìm thấy
                                    if (weaponSkillData == null)
                                    {
                                        /// Lấy kỹ năng tân thủ đánh thường quyền
                                        weaponSkillData = newbieHandAttackSkillData;
                                    }
                                }
                            }
                            /// Nếu có kỹ năng thỏa mãn thì bỏ kỹ năng tân thủ
                            else
                            {
                                weaponSkillData = null;
                            }
                        }

                        /// Kỹ năng sẽ được dùng
                        SkillDataEx skill = weaponSkillData ?? skillInfo.Key;
                        /// Nếu không có kỹ năng
                        if (skill == null)
                        {
                            /// Bỏ qua Frame
                            yield return new WaitForSeconds(this.AutoFight_TickSec);
                            /// Tiếp tục vòng lặp
                            continue;
                        }

                        /// Nếu kỹ năng này có không thể sử dụng trên ngựa thì sẽ xuống ngựa
                        if (skill.HorseLimit && Global.Data.Leader.ComponentCharacter.Data.IsRiding)
                        {
                            KT_TCPHandler.SendChangeToggleHorseState();
                            yield return new WaitForSeconds(0.5f);
                            continue;
                        }

                        /// Phạm vi tấn công của kỹ năng
                        int skillCastRange = skill.AttackRadius;
                        /// Nếu không phải là skill tân thủ lấy ra tầm đánh của skill
                        if (!KTGlobal.ListNewbieAttackSkill.Contains(skill.ID))
                        {
                            SkillData _SkilLData = skillInfo.Value;

                            PropertyDictionary skillPd = skill.Properties[_SkilLData.Level];

                            if (skillPd.ContainsKey((int)MAGIC_ATTRIB.magic_skill_attackradius))
                            {
                                skillCastRange = skillPd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.magic_skill_attackradius).nValue[0];
                            }
                        }

                        GSprite nextTarget = null;
                        /// Nếu có mục tiêu trước đó
                        if (this.AutoFightLastTarget != null)
                        {
                            /// Mục tiêu đánh dấu là mục tiêu trước đó
                            nextTarget = this.AutoFightLastTarget;

                            /// Dữ liệu đối tượng
                            if (Global.Data.SystemMonsters.TryGetValue(nextTarget.RoleID, out MonsterData md))
                            {
                                if (md.HP <= 0)
                                {
                                    nextTarget = null;
                                }
                            }
                            else if (Global.Data.OtherRoles.TryGetValue(nextTarget.RoleID, out RoleData rd))
                            {
                                if (rd.CurrentHP > 0)
                                {
                                    nextTarget = null;
                                }
                            }
                            else
                            {
                                nextTarget = null;
                            }
                        }

                        /// Nếu không có mục tiêu hoặc mục tiêu đã chết
                        if (nextTarget == null || nextTarget.IsDeath || nextTarget.HP <= 0)
                        {
                            /// Phải là chế độ trian quái thì mới quay về bãi còn không thì đéo.
                            if (this.TrainResetRadius.Elapsed.TotalSeconds > 10 && !KTAutoAttackSetting.Config.EnableAutoPK)
                            {
                                /// Reset lại thời gian về bãi
                                this.TrainResetRadius = Stopwatch.StartNew();
                                /// Nếu là đánh quanh điểm check xem có quá range không
                                if (KTAutoAttackSetting.Config.Farm.FarmAround)
                                {
                                    if (Vector2.Distance(leader.PositionInVector2, this.StartPos) >= KTAutoAttackSetting.Config.Farm.ScanRange)
                                    {
                                        /// Nếu chưa thực hiện xong động tác xuất chiêu
                                        if (!KTGlobal.FinishedUseSkillAction)
                                        {
                                            /// Bỏ qua Frame
                                            yield return new WaitForSeconds(this.AutoFight_TickSec);
                                            /// Tiếp tục vòng lặp
                                            continue;
                                        }
                                        /// Nếu không thể di chuyển hoặc chưa thực hiện xong động tác trước đó
                                        else if (!leader.CanPositiveMove && !leader.IsReadyToMove)
                                        {
                                            /// Bỏ qua Frame
                                            yield return new WaitForSeconds(this.AutoFight_TickSec);
                                            /// Tiếp tục vòng lặp
                                            continue;
                                        }

                                        /// Ngừng di chuyển
                                        KTLeaderMovingManager.StopMoveImmediately();
                                        /// Bỏ qua Frame
                                        yield return new WaitForSeconds(this.AutoFight_TickSec);
                                        /// Chạy về vị trí ban đầu
                                        KTGlobal.AddNotification("Quay về vị trí bắt đầu auto vì chạy quá xa bán kinh đánh quái : Phạm vi <color=red>" + KTAutoAttackSetting.Config.Farm.ScanRange + "</color>");
                                        KTLeaderMovingManager.AutoFindRoad(new Drawing.Point((int)this.StartPos.x, (int)this.StartPos.y));
                                        /// Bỏ qua Frame
                                        yield return new WaitForSeconds(this.AutoFight_TickSec);
                                        /// Tiếp tục vòng lặp
                                        continue;
                                    }
                                }
                                else
                                {
                                    /// Tìm mục tiêu gần nhất thỏa mãn
                                    nextTarget = this.FindBestTarget(null, false, skillCastRange);
                                }
                            }
                            else
                            {
                                /// Tìm mục tiêu gần nhất thỏa mãn
                                nextTarget = this.FindBestTarget(null, false, skillCastRange);
                            }
                        }

                        /// Nếu không có mục tiêu gần nhất
                        if (nextTarget == null || nextTarget.IsDeath || nextTarget.HP <= 0)
                        {
                            /// Ẩn mặt đối tượng
                            PlayZone.Instance.HideAllFace();

                            ///// Bỏ qua Frame
                            //yield return new WaitForSeconds(KTGlobal.SitActionTime + 0.2f);
                            yield return null;
                            /// Tiếp tục vòng lặp
                            continue;
                        }
                        /// Thiết lập mục tiêu
                        else
                        {
                            this.AutoFightLastTarget = nextTarget;
                            SkillManager.SelectedTarget = nextTarget;
                        }

                        /// Thực hiện dùng kỹ năng
                        SkillManager.LeaderUseSkill(skill.ID, false, false, true);

                        if (skillInfo.Value != null)
                            skillInfo.Value.Prioritized++;

                        /// Nếu mục tiêu khác với mục tiêu trước đó
                        if (SkillManager.SelectedTarget == null)
                        {
                            this.ChangeAutoFightTarget();
                        }
                        else if (this.AutoFightLastTarget != SkillManager.SelectedTarget)
                        {
                            this.ChangeAutoFightTarget(SkillManager.SelectedTarget);
                        }
                        /// Nếu mục tiêu trùng với mục tiêu trước đó
                        else
                        {
                            bool IsCanNextTaget = false;
                            /// Mức máu trước đó của mục tiêu
                            int lastTargetHP = SkillManager.GetTargetTrueHP(this.AutoFightLastTarget);
                            /// Nếu mức máu khác mức máu trước đó
                            if (this.AutoFightLastTarget != null && lastTargetHP > 0 && lastTargetHP != this.AutoFightLastTargetHP)
                            {
                                /// Reset thời điểm đánh trúng mục tiêu
                                this.AutoFightLastSuccessAttackTick = KTGlobal.GetCurrentTimeMilis();
                                /// Xóa danh sách mục tiêu bỏ qua
                                ignoredTargets.Clear();

                                IsCanNextTaget = true;
                                /// Cập nhật máu mục tiêu
                                this.AutoFightLastTargetHP = lastTargetHP;
                            }

                            // Nếu HP trước đó mà vẫn = HP sau khi tấn công và nhân vật đứng 1 chỗ ===> BUG VỊ TRÍ QUÁI HOẶC NHÂN VẬT ==> Đánh không mất máu ==> đổi taget khác
                            if ((KTGlobal.GetCurrentTimeMilis() - this.AutoFightLastSuccessAttackTick >= this.AutoFight_MissTickToChangeEnemy && BugPostion > 100) || BugPostion > 999)
                            {
                                BugPostion = 0;

                                //KTGlobal.AddNotification("Phát hiện ra bug mục tiêu tự đổi mục tiêu khác");
                                /// Thêm vào danh sách mục tiêu bỏ qua
                                ignoredTargets.Add(this.AutoFightLastTarget);
                                /// Đổi mục tiêu
                                this.ChangeAutoFightTarget(ignoredTargets);
                            }

                            /// Nếu số lần Miss vượt quá ngưỡng kiểm tra thì đổi mục tiêu và phải thêm điều kiện nữa là nhân vật không di chuyển chứ có di chuyển thì tức là nó đang chạy tới tấn công mục tiêu
                            if (KTGlobal.GetCurrentTimeMilis() - this.AutoFightLastSuccessAttackTick >= this.AutoFight_MissTickToChangeEnemy && !leader.IsMoving)
                            {
                                /// Thêm vào danh sách mục tiêu bỏ qua
                                ignoredTargets.Add(this.AutoFightLastTarget);
                                /// Đổi mục tiêu
                                this.ChangeAutoFightTarget(ignoredTargets);
                            }

                            /// Nếu không phải đơn mục tiêu và đang ở chế độ train quái
                            if (!KTAutoAttackSetting.Config.Farm.SingleTarget && !KTAutoAttackSetting.Config.EnableAutoPK && IsCanNextTaget)
                            {
                                /// nếu mà danh sách đánh đã quá 5 con thì lại clear danh sách để cho nó có thể quay lại đánh từ từ con đầu tiên
                                if (HitTagetList.Count > 5)
                                {
                                    HitTagetList.Clear();
                                }
                                /// Thêm vào danh sách mục tiêu bỏ qua
                                HitTagetList.Add(this.AutoFightLastTarget);
                                /// Đổi mục tiêu
                                this.ChangeAutoFightTarget(HitTagetList);
                            }
                        }
                    }
                }

                TmpPostion = leader.PositionInVector2;
                /// Dừng lại, sau đó tiếp tục yêu cầu dùng kỹ năng
                yield return new WaitForSeconds(this.AutoFight_TickSec);
            }
        }
    }
}