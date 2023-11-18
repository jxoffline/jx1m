/*
 Navicat Premium Data Transfer

 Source Server         : LOCALHOST
 Source Server Type    : MySQL
 Source Server Version : 80031
 Source Host           : localhost:3306
 Source Schema         : gamedb

 Target Server Type    : MySQL
 Target Server Version : 80031
 File Encoding         : 65001

 Date: 04/07/2023 17:01:06
*/

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ----------------------------
-- Table structure for t_ban
-- ----------------------------
DROP TABLE IF EXISTS `t_ban`;
CREATE TABLE `t_ban`  (
  `role_id` int NOT NULL COMMENT 'ID nhân vật',
  `start_time` bigint NULL DEFAULT NULL COMMENT 'Thời điểm bắt đầu Ban',
  `duration` bigint NULL DEFAULT NULL COMMENT 'Duy trì',
  `ban_type` tinyint NULL DEFAULT NULL COMMENT 'Loại Ban',
  `banned_by` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'Bị Ban bởi ai',
  PRIMARY KEY (`role_id`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for t_ban_chat
-- ----------------------------
DROP TABLE IF EXISTS `t_ban_chat`;
CREATE TABLE `t_ban_chat`  (
  `role_id` int NOT NULL COMMENT 'ID người chơi',
  `start_time` bigint NULL DEFAULT NULL COMMENT 'Thời điểm bị Ban',
  `duration` bigint NULL DEFAULT NULL COMMENT 'Thời gian Ban',
  `reason` varchar(1000) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'Lý do',
  `banned_by` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'Người Ban',
  PRIMARY KEY (`role_id`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for t_ban_user
-- ----------------------------
DROP TABLE IF EXISTS `t_ban_user`;
CREATE TABLE `t_ban_user`  (
  `role_id` int NOT NULL COMMENT 'ID nhân vật',
  `start_time` bigint NULL DEFAULT NULL COMMENT 'Thời điểm Tick bắt đầu bị Ban',
  `duration` bigint NULL DEFAULT NULL COMMENT 'Thời gian Ban (-1 là mãi mãi)',
  `reason` varchar(1000) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'Lý do Ban',
  `banned_by` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'Người Ban',
  PRIMARY KEY (`role_id`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for t_buffer
-- ----------------------------
DROP TABLE IF EXISTS `t_buffer`;
CREATE TABLE `t_buffer`  (
  `Id` int NOT NULL AUTO_INCREMENT,
  `rid` int NOT NULL DEFAULT 0,
  `bufferid` int NOT NULL DEFAULT 0,
  `starttime` bigint NOT NULL DEFAULT 0,
  `buffersecs` bigint NOT NULL DEFAULT 0,
  `bufferval` bigint NOT NULL DEFAULT 0,
  `custom_property` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  PRIMARY KEY (`Id`) USING BTREE,
  UNIQUE INDEX `rid_bufferid`(`rid`, `bufferid`) USING BTREE
) ENGINE = MyISAM AUTO_INCREMENT = 34844 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = FIXED;

-- ----------------------------
-- Table structure for t_change_name
-- ----------------------------
DROP TABLE IF EXISTS `t_change_name`;
CREATE TABLE `t_change_name`  (
  `Id` int UNSIGNED NOT NULL AUTO_INCREMENT,
  `roleid` int NOT NULL,
  `oldname` varchar(64) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT '',
  `newname` varchar(64) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT '',
  `type` tinyint NOT NULL DEFAULT 0,
  `cost_diamond` int NOT NULL DEFAULT 0,
  `time` datetime(0) NULL DEFAULT NULL,
  PRIMARY KEY (`Id`) USING BTREE,
  INDEX `key_roleid`(`roleid`) USING BTREE,
  INDEX `key_oldname`(`oldname`) USING BTREE,
  INDEX `key_newname`(`newname`) USING BTREE
) ENGINE = MyISAM AUTO_INCREMENT = 1 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for t_config
-- ----------------------------
DROP TABLE IF EXISTS `t_config`;
CREATE TABLE `t_config`  (
  `paramname` char(32) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `paramvalue` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  UNIQUE INDEX `paramname`(`paramname`) USING BTREE
) ENGINE = MyISAM AUTO_INCREMENT = 1 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for t_consumelog
-- ----------------------------
DROP TABLE IF EXISTS `t_consumelog`;
CREATE TABLE `t_consumelog`  (
  `rid` int NOT NULL,
  `amount` int NOT NULL,
  `ctype` int NOT NULL DEFAULT 1,
  `cdate` datetime(0) NOT NULL,
  INDEX `rid_cdata`(`rid`, `cdate`) USING BTREE
) ENGINE = MyISAM AUTO_INCREMENT = 1 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = FIXED;

-- ----------------------------
-- Table structure for t_family
-- ----------------------------
DROP TABLE IF EXISTS `t_family`;
CREATE TABLE `t_family`  (
  `FamilyID` int NOT NULL AUTO_INCREMENT,
  `FamilyName` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `Leader` int NULL DEFAULT NULL,
  `Notify` varchar(1000) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `DateCreate` datetime(0) NULL DEFAULT NULL,
  `RequestNotify` varchar(5000) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `FamilyMoney` int NULL DEFAULT NULL,
  `WeeklyFubenCount` int NULL DEFAULT NULL,
  `FamilyZoneID` int NULL DEFAULT NULL,
  PRIMARY KEY (`FamilyID`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for t_familyrequestjoin
-- ----------------------------
DROP TABLE IF EXISTS `t_familyrequestjoin`;
CREATE TABLE `t_familyrequestjoin`  (
  `ID` int NOT NULL AUTO_INCREMENT,
  `RoleID` int NULL DEFAULT NULL,
  `RoleName` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `RoleFactionID` int NULL DEFAULT NULL,
  `FamilyID` int NULL DEFAULT NULL,
  `RoleLevel` int NULL DEFAULT NULL,
  `RolePrestige` int NULL DEFAULT NULL,
  `TimeRequest` datetime(0) NULL DEFAULT NULL,
  PRIMARY KEY (`ID`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for t_friends
-- ----------------------------
DROP TABLE IF EXISTS `t_friends`;
CREATE TABLE `t_friends`  (
  `Id` int NOT NULL AUTO_INCREMENT,
  `myid` int NOT NULL DEFAULT 0,
  `otherid` int NOT NULL DEFAULT 0,
  `friendType` tinyint UNSIGNED NOT NULL DEFAULT 0,
  `relationship` int NULL DEFAULT NULL,
  PRIMARY KEY (`Id`) USING BTREE,
  UNIQUE INDEX `unique_mo`(`myid`, `otherid`) USING BTREE
) ENGINE = MyISAM AUTO_INCREMENT = 12785 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = FIXED;

-- ----------------------------
-- Table structure for t_goods
-- ----------------------------
DROP TABLE IF EXISTS `t_goods`;
CREATE TABLE `t_goods`  (
  `Id` int NOT NULL AUTO_INCREMENT,
  `rid` int NOT NULL DEFAULT 0,
  `goodsid` int UNSIGNED NOT NULL DEFAULT 0,
  `isusing` int NOT NULL DEFAULT -1,
  `forge_level` int UNSIGNED NOT NULL DEFAULT 1,
  `starttime` datetime(0) NOT NULL DEFAULT '1900-01-01 12:00:00',
  `endtime` datetime(0) NOT NULL DEFAULT '1900-01-01 12:00:00',
  `site` int NOT NULL DEFAULT 0,
  `Props` varchar(2000) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `gcount` int UNSIGNED NOT NULL DEFAULT 0,
  `binding` int UNSIGNED NOT NULL DEFAULT 0,
  `bagindex` int NOT NULL DEFAULT 0,
  `strong` int NOT NULL DEFAULT 0,
  `series` tinyint NULL DEFAULT NULL,
  `otherpramer` varchar(1000) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  PRIMARY KEY (`Id`) USING BTREE,
  INDEX `rid`(`rid`) USING BTREE
) ENGINE = MyISAM AUTO_INCREMENT = 317199425 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for t_goods_bak
-- ----------------------------
DROP TABLE IF EXISTS `t_goods_bak`;
CREATE TABLE `t_goods_bak`  (
  `Id` int NOT NULL AUTO_INCREMENT,
  `rid` int NOT NULL DEFAULT 0,
  `goodsid` int UNSIGNED NOT NULL DEFAULT 0,
  `isusing` int NOT NULL DEFAULT -1,
  `forge_level` int UNSIGNED NOT NULL DEFAULT 1,
  `starttime` datetime(0) NOT NULL DEFAULT '1900-01-01 12:00:00',
  `endtime` datetime(0) NOT NULL DEFAULT '1900-01-01 12:00:00',
  `site` int NOT NULL DEFAULT 0,
  `Props` varchar(2000) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `gcount` int UNSIGNED NOT NULL DEFAULT 0,
  `binding` int UNSIGNED NOT NULL DEFAULT 0,
  `bagindex` int NOT NULL DEFAULT 0,
  `strong` int NOT NULL DEFAULT 0,
  `series` tinyint NULL DEFAULT NULL,
  `otherpramer` varchar(1000) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  PRIMARY KEY (`Id`) USING BTREE,
  INDEX `rid`(`rid`) USING BTREE
) ENGINE = MyISAM AUTO_INCREMENT = 317198607 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for t_goods_bak_1
-- ----------------------------
DROP TABLE IF EXISTS `t_goods_bak_1`;
CREATE TABLE `t_goods_bak_1`  (
  `Id` int NOT NULL AUTO_INCREMENT,
  `rid` int NOT NULL DEFAULT 0,
  `goodsid` int UNSIGNED NOT NULL DEFAULT 0,
  `isusing` int NOT NULL DEFAULT -1,
  `forge_level` int UNSIGNED NOT NULL DEFAULT 1,
  `starttime` datetime(0) NOT NULL DEFAULT '1900-01-01 12:00:00',
  `endtime` datetime(0) NOT NULL DEFAULT '1900-01-01 12:00:00',
  `site` int NOT NULL DEFAULT 0,
  `Props` varchar(2000) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `gcount` int UNSIGNED NOT NULL DEFAULT 0,
  `binding` int UNSIGNED NOT NULL DEFAULT 0,
  `bagindex` int NOT NULL DEFAULT 0,
  `strong` int NOT NULL DEFAULT 0,
  `series` tinyint NULL DEFAULT NULL,
  `otherpramer` varchar(1000) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  PRIMARY KEY (`Id`) USING BTREE,
  INDEX `rid`(`rid`) USING BTREE
) ENGINE = MyISAM AUTO_INCREMENT = 11333475 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for t_groupmail
-- ----------------------------
DROP TABLE IF EXISTS `t_groupmail`;
CREATE TABLE `t_groupmail`  (
  `gmailid` int NOT NULL,
  `subject` varchar(100) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `content` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `conditions` varchar(100) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `inputtime` datetime(0) NOT NULL,
  `endtime` datetime(0) NOT NULL,
  `yinliang` int NOT NULL DEFAULT 0,
  `tongqian` int NOT NULL DEFAULT 0,
  `yuanbao` int NOT NULL DEFAULT 0,
  `goodlist` varchar(200) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  PRIMARY KEY (`gmailid`) USING BTREE
) ENGINE = MyISAM AUTO_INCREMENT = 1 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for t_guild
-- ----------------------------
DROP TABLE IF EXISTS `t_guild`;
CREATE TABLE `t_guild`  (
  `GuildID` int NOT NULL AUTO_INCREMENT COMMENT 'GuilID khóa chính liên kết với t_roles để xác định người nào ở bang nào',
  `GuildName` varchar(32) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'Tên bang hội',
  `MoneyBound` int NULL DEFAULT NULL COMMENT 'Quỹ thưởng bang hội',
  `MoneyStore` int NULL DEFAULT NULL COMMENT 'Quy bang hội',
  `ZoneID` int NULL DEFAULT NULL COMMENT 'ZoneID của bang hội',
  `Notify` varchar(1000) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'Thông báo bang',
  `IsMainCity` int NULL DEFAULT NULL COMMENT 'Có đang là chủ thành hay không',
  `MaxWithDraw` int NULL DEFAULT NULL COMMENT 'Số lượng % tối đa quỹ bang sẽ đem phát thưởng',
  `Leader` int NULL DEFAULT NULL COMMENT 'Thằng nào là chủ bang hội',
  `DateCreate` datetime(0) NULL DEFAULT NULL COMMENT 'Ngày Tạo Bang',
  `GuildLevel` int NULL DEFAULT NULL COMMENT 'Cấp độ của bang',
  `GuildExp` int NULL DEFAULT NULL COMMENT 'Kinh nghiệm của cấp độ hiện tại',
  `AutoAccept` int NULL DEFAULT NULL COMMENT 'Tự động cho phép gia nhập bang',
  `RuleAccept` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'Duyệt người chơi',
  `ItemStore` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'Vật phẩm người chơi cống hiến vào để thăng cấp bang hội',
  `CurQuestID` int NULL DEFAULT NULL COMMENT 'Nhiệm vụ hiện tại của bang',
  `SkillNote` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'Lưu lại thong tin kỹ năng của bang hội khả năng phải mã hóa j đó lưu lại',
  `PreDelete` int NULL DEFAULT NULL COMMENT 'Bang này có đang ấn giải tán hay không',
  `DeleteStartDay` datetime(0) NULL DEFAULT NULL COMMENT 'Ngày bắt đầu xóa là ngày nào',
  `Total_Copy_Scenes_This_Week` tinyint NULL DEFAULT 0 COMMENT 'So luot di phu ban',
  PRIMARY KEY (`GuildID`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 200002 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for t_guild_point
-- ----------------------------
DROP TABLE IF EXISTS `t_guild_point`;
CREATE TABLE `t_guild_point`  (
  `RoleID` int NOT NULL COMMENT 'RoleID là khóa chính',
  `WeekID` int NULL DEFAULT NULL COMMENT 'ID của tuần ',
  `WeekPoint` int NULL DEFAULT NULL COMMENT 'Điểm cống hiến tuần',
  `GuildID` int NULL DEFAULT NULL COMMENT 'Của bang nào',
  PRIMARY KEY (`RoleID`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for t_guild_request_join
-- ----------------------------
DROP TABLE IF EXISTS `t_guild_request_join`;
CREATE TABLE `t_guild_request_join`  (
  `ID` int NOT NULL AUTO_INCREMENT COMMENT 'Khóa chính',
  `RoleID` int NULL DEFAULT NULL,
  `RoleName` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `RoleFactionID` int NULL DEFAULT NULL COMMENT 'ID phái của thằng xin vào',
  `RoleValue` bigint NULL DEFAULT NULL COMMENT 'Tài phú của thằng xin vào',
  `TimeRequest` datetime(0) NULL DEFAULT NULL COMMENT 'Thời gian nó xin vào',
  `GuildID` int NULL DEFAULT NULL COMMENT 'ID bang nos xin và',
  `RoleLevel` int NULL DEFAULT NULL,
  PRIMARY KEY (`ID`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 930 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for t_guild_task
-- ----------------------------
DROP TABLE IF EXISTS `t_guild_task`;
CREATE TABLE `t_guild_task`  (
  `GuildID` int NOT NULL,
  `TaskID` int NULL DEFAULT NULL COMMENT 'ID nhiệm vụ của bang này',
  `TaskValue` int NULL DEFAULT NULL,
  `DayCreate` int NULL DEFAULT NULL,
  `TaskCountInDay` int NULL DEFAULT NULL COMMENT 'Nhiệm Vụ thứ mấy trong ngày',
  PRIMARY KEY (`GuildID`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for t_guildwar
-- ----------------------------
DROP TABLE IF EXISTS `t_guildwar`;
CREATE TABLE `t_guildwar`  (
  `GuildID` int NOT NULL,
  `GuildName` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `TeamList` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `WeekID` int NULL DEFAULT NULL,
  PRIMARY KEY (`GuildID`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for t_inputlog
-- ----------------------------
DROP TABLE IF EXISTS `t_inputlog`;
CREATE TABLE `t_inputlog`  (
  `Id` int(11) UNSIGNED ZEROFILL NOT NULL AUTO_INCREMENT,
  `amount` int UNSIGNED NOT NULL DEFAULT 0,
  `u` char(64) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL DEFAULT '0',
  `order_no` char(64) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `cporder_no` char(64) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `time` int UNSIGNED NOT NULL DEFAULT 0,
  `sign` char(32) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `inputtime` datetime(0) NOT NULL,
  `result` char(32) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `zoneid` int NOT NULL DEFAULT 0,
  `role_action` int NOT NULL,
  PRIMARY KEY (`Id`) USING BTREE,
  INDEX `inputtime`(`inputtime`) USING BTREE,
  INDEX `query_money`(`inputtime`, `u`, `zoneid`, `result`) USING BTREE,
  INDEX `uid`(`u`) USING BTREE
) ENGINE = MyISAM AUTO_INCREMENT = 7714 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = FIXED;

-- ----------------------------
-- Table structure for t_limitgoodsbuy
-- ----------------------------
DROP TABLE IF EXISTS `t_limitgoodsbuy`;
CREATE TABLE `t_limitgoodsbuy`  (
  `rid` int NOT NULL DEFAULT 0,
  `goodsid` int UNSIGNED NOT NULL DEFAULT 0,
  `dayid` int UNSIGNED NOT NULL DEFAULT 0,
  `usednum` int UNSIGNED NOT NULL DEFAULT 0,
  UNIQUE INDEX `rid_goodsid`(`rid`, `goodsid`) USING BTREE
) ENGINE = MyISAM AUTO_INCREMENT = 1 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = FIXED;

-- ----------------------------
-- Table structure for t_login
-- ----------------------------
DROP TABLE IF EXISTS `t_login`;
CREATE TABLE `t_login`  (
  `userid` varchar(64) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `dayid` int NULL DEFAULT 0,
  `rid` bigint NOT NULL,
  `logintime` datetime(0) NULL DEFAULT NULL,
  `logouttime` datetime(0) NULL DEFAULT NULL,
  `ip` varchar(16) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `mac` varchar(30) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `zoneid` mediumint NULL DEFAULT NULL,
  `onlinesecs` mediumint NULL DEFAULT 0,
  `loginnum` mediumint NULL DEFAULT 0,
  `c1` varchar(64) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `c2` varchar(64) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  UNIQUE INDEX `userid_dayid_ip`(`userid`, `dayid`, `ip`) USING BTREE,
  INDEX `logintime`(`logintime`) USING BTREE
) ENGINE = MyISAM AUTO_INCREMENT = 1 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for t_mail
-- ----------------------------
DROP TABLE IF EXISTS `t_mail`;
CREATE TABLE `t_mail`  (
  `mailid` int NOT NULL AUTO_INCREMENT,
  `senderrid` int NOT NULL DEFAULT 0,
  `senderrname` varchar(256) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `sendtime` datetime(0) NOT NULL DEFAULT '1900-01-01 12:00:00',
  `receiverrid` int NOT NULL DEFAULT 0,
  `reveiverrname` varchar(256) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `readtime` datetime(0) NOT NULL DEFAULT '1900-01-01 12:00:00',
  `isread` tinyint UNSIGNED NOT NULL DEFAULT 0,
  `mailtype` tinyint UNSIGNED NOT NULL DEFAULT 0,
  `hasfetchattachment` tinyint UNSIGNED NOT NULL DEFAULT 0,
  `subject` varchar(256) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `content` varchar(10000) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `bound_money` int UNSIGNED NOT NULL DEFAULT 0,
  `bound_token` int UNSIGNED NOT NULL DEFAULT 0,
  PRIMARY KEY (`mailid`) USING BTREE,
  INDEX `receiverrid`(`receiverrid`) USING BTREE,
  INDEX `senderrid_idx`(`senderrid`) USING BTREE
) ENGINE = MyISAM AUTO_INCREMENT = 205474 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = FIXED;

-- ----------------------------
-- Table structure for t_mailgoods
-- ----------------------------
DROP TABLE IF EXISTS `t_mailgoods`;
CREATE TABLE `t_mailgoods`  (
  `Id` int NOT NULL AUTO_INCREMENT,
  `mailid` int NOT NULL DEFAULT 0,
  `goodsid` int UNSIGNED NOT NULL DEFAULT 0,
  `forge_level` int UNSIGNED NOT NULL DEFAULT 1,
  `Props` varchar(500) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL DEFAULT '',
  `gcount` int UNSIGNED NOT NULL DEFAULT 0,
  `binding` int UNSIGNED NOT NULL DEFAULT 0,
  `series` tinyint NULL DEFAULT NULL,
  `otherpramer` varchar(256) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `strong` int UNSIGNED NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`) USING BTREE,
  INDEX `mailid`(`mailid`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for t_mailtemp
-- ----------------------------
DROP TABLE IF EXISTS `t_mailtemp`;
CREATE TABLE `t_mailtemp`  (
  `Id` int NOT NULL AUTO_INCREMENT,
  `mailid` int NOT NULL DEFAULT 0,
  `receiverrid` int UNSIGNED NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE = MyISAM AUTO_INCREMENT = 8701 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = FIXED;

-- ----------------------------
-- Table structure for t_mark
-- ----------------------------
DROP TABLE IF EXISTS `t_mark`;
CREATE TABLE `t_mark`  (
  `ID` int NOT NULL AUTO_INCREMENT,
  `RoleID` int NULL DEFAULT NULL,
  `TimeRanger` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `MarkValue` int NULL DEFAULT NULL,
  `MarkType` int NULL DEFAULT NULL,
  PRIMARY KEY (`ID`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for t_money
-- ----------------------------
DROP TABLE IF EXISTS `t_money`;
CREATE TABLE `t_money`  (
  `userid` char(64) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL DEFAULT '0',
  `money` int UNSIGNED NOT NULL DEFAULT 0,
  `realmoney` int UNSIGNED NOT NULL DEFAULT 0,
  `giftid` int NOT NULL DEFAULT 0,
  `giftjifen` int NOT NULL DEFAULT 0,
  `points` int NOT NULL DEFAULT 0,
  `specjifen` int NOT NULL DEFAULT 0,
  UNIQUE INDEX `userid`(`userid`) USING BTREE
) ENGINE = MyISAM AUTO_INCREMENT = 1 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = FIXED;

-- ----------------------------
-- Table structure for t_name_check
-- ----------------------------
DROP TABLE IF EXISTS `t_name_check`;
CREATE TABLE `t_name_check`  (
  `Id` int UNSIGNED NOT NULL AUTO_INCREMENT,
  `name` varchar(64) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL DEFAULT '',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE = MyISAM AUTO_INCREMENT = 199 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for t_pet
-- ----------------------------
DROP TABLE IF EXISTS `t_pet`;
CREATE TABLE `t_pet`  (
  `id` int NOT NULL AUTO_INCREMENT,
  `role_id` int NULL DEFAULT NULL,
  `res_id` int NULL DEFAULT NULL,
  `name` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `level` int NULL DEFAULT NULL,
  `exp` int NULL DEFAULT NULL,
  `skills` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `enlightenment` int NULL DEFAULT NULL,
  `equips` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `str` int NULL DEFAULT NULL,
  `dex` int NULL DEFAULT NULL,
  `sta` int NULL DEFAULT NULL,
  `ene` int NULL DEFAULT NULL,
  `hp` int NULL DEFAULT NULL,
  `remain_points` int NULL DEFAULT NULL,
  `joyful` int NULL DEFAULT NULL,
  `life` int NULL DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 207882 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for t_ranking
-- ----------------------------
DROP TABLE IF EXISTS `t_ranking`;
CREATE TABLE `t_ranking`  (
  `rid` int NOT NULL AUTO_INCREMENT,
  `rname` varchar(200) CHARACTER SET latin1 COLLATE latin1_swedish_ci NULL DEFAULT NULL,
  `level` int NULL DEFAULT NULL,
  `occupation` int NULL DEFAULT NULL,
  `sub_id` int NULL DEFAULT NULL,
  `experience` bigint NULL DEFAULT NULL,
  `monphai` int NULL DEFAULT NULL,
  `taiphu` bigint NULL DEFAULT NULL,
  `volam` int NULL DEFAULT NULL,
  `liendau` int NULL DEFAULT NULL,
  `uydanh` int NULL DEFAULT NULL,
  `RankingEventType0Value` int NULL DEFAULT -1 COMMENT 'Sự kiện đua top 0 lưu lại',
  `RankingEventType0Status` int NULL DEFAULT -1 COMMENT 'Sự kiện đua top 0 status',
  `RankingEventType1Value` int NULL DEFAULT -1 COMMENT 'Sự kiện đua top 1 lưu lại ',
  `RankingEventType1Status` int NULL DEFAULT -1 COMMENT 'Sự kiện đua top 1 status',
  `CurentRank0Value` int NULL DEFAULT -1,
  `CurentRank1Value` int NULL DEFAULT -1,
  PRIMARY KEY (`rid`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 606026 CHARACTER SET = latin1 COLLATE = latin1_swedish_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for t_recore
-- ----------------------------
DROP TABLE IF EXISTS `t_recore`;
CREATE TABLE `t_recore`  (
  `ID` int NOT NULL AUTO_INCREMENT,
  `RoleID` int NULL DEFAULT NULL,
  `RoleName` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `RecoryType` int NULL DEFAULT NULL,
  `DateRecore` datetime(0) NULL DEFAULT NULL,
  `ValueRecore` int NULL DEFAULT NULL,
  `ZoneID` int NULL DEFAULT NULL,
  PRIMARY KEY (`ID`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for t_rolegmail_record
-- ----------------------------
DROP TABLE IF EXISTS `t_rolegmail_record`;
CREATE TABLE `t_rolegmail_record`  (
  `roleid` int NOT NULL,
  `gmailid` int NOT NULL,
  `mailid` int NULL DEFAULT 0,
  PRIMARY KEY (`roleid`, `gmailid`) USING BTREE
) ENGINE = MyISAM AUTO_INCREMENT = 1 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = FIXED;

-- ----------------------------
-- Table structure for t_roleparams
-- ----------------------------
DROP TABLE IF EXISTS `t_roleparams`;
CREATE TABLE `t_roleparams`  (
  `rid` int NOT NULL DEFAULT 0,
  `pname` char(32) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `pvalue` char(128) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  UNIQUE INDEX `rid_pname`(`rid`, `pname`) USING BTREE
) ENGINE = MyISAM AUTO_INCREMENT = 1 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = FIXED;

-- ----------------------------
-- Table structure for t_roleparams_2
-- ----------------------------
DROP TABLE IF EXISTS `t_roleparams_2`;
CREATE TABLE `t_roleparams_2`  (
  `rid` int NOT NULL,
  `pname` char(32) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
  `pvalue` varchar(50000) CHARACTER SET ascii COLLATE ascii_general_ci NULL DEFAULT NULL,
  PRIMARY KEY (`rid`, `pname`) USING BTREE
) ENGINE = MyISAM AUTO_INCREMENT = 1 CHARACTER SET = ascii COLLATE = ascii_general_ci ROW_FORMAT = FIXED;

-- ----------------------------
-- Table structure for t_roleparams_char
-- ----------------------------
DROP TABLE IF EXISTS `t_roleparams_char`;
CREATE TABLE `t_roleparams_char`  (
  `rid` int NOT NULL,
  `idx` int NOT NULL,
  `v0` char(128) CHARACTER SET ascii COLLATE ascii_general_ci NULL DEFAULT '',
  `v1` char(128) CHARACTER SET ascii COLLATE ascii_general_ci NULL DEFAULT '',
  `v2` char(128) CHARACTER SET ascii COLLATE ascii_general_ci NULL DEFAULT '',
  `v3` char(128) CHARACTER SET ascii COLLATE ascii_general_ci NULL DEFAULT '',
  `v4` char(128) CHARACTER SET ascii COLLATE ascii_general_ci NULL DEFAULT '',
  `v5` char(128) CHARACTER SET ascii COLLATE ascii_general_ci NULL DEFAULT '',
  `v6` char(128) CHARACTER SET ascii COLLATE ascii_general_ci NULL DEFAULT '',
  `v7` char(128) CHARACTER SET ascii COLLATE ascii_general_ci NULL DEFAULT '',
  `v8` char(128) CHARACTER SET ascii COLLATE ascii_general_ci NULL DEFAULT '',
  `v9` char(128) CHARACTER SET ascii COLLATE ascii_general_ci NULL DEFAULT '',
  PRIMARY KEY (`rid`, `idx`) USING BTREE
) ENGINE = MyISAM AUTO_INCREMENT = 1 CHARACTER SET = ascii COLLATE = ascii_general_ci ROW_FORMAT = FIXED;

-- ----------------------------
-- Table structure for t_roleparams_long
-- ----------------------------
DROP TABLE IF EXISTS `t_roleparams_long`;
CREATE TABLE `t_roleparams_long`  (
  `rid` int NOT NULL,
  `idx` int NOT NULL,
  `v0` bigint NULL DEFAULT 0,
  `v1` bigint NULL DEFAULT 0,
  `v2` bigint NULL DEFAULT 0,
  `v3` bigint NULL DEFAULT 0,
  `v4` bigint NULL DEFAULT 0,
  `v5` bigint NULL DEFAULT 0,
  `v6` bigint NULL DEFAULT 0,
  `v7` bigint NULL DEFAULT 0,
  `v8` bigint NULL DEFAULT 0,
  `v9` bigint NULL DEFAULT 0,
  `v10` bigint NULL DEFAULT 0,
  `v11` bigint NULL DEFAULT 0,
  `v12` bigint NULL DEFAULT 0,
  `v13` bigint NULL DEFAULT 0,
  `v14` bigint NULL DEFAULT 0,
  `v15` bigint NULL DEFAULT 0,
  `v16` bigint NULL DEFAULT 0,
  `v17` bigint NULL DEFAULT 0,
  `v18` bigint NULL DEFAULT 0,
  `v19` bigint NULL DEFAULT 0,
  `v20` bigint NULL DEFAULT 0,
  `v21` bigint NULL DEFAULT 0,
  `v22` bigint NULL DEFAULT 0,
  `v23` bigint NULL DEFAULT 0,
  `v24` bigint NULL DEFAULT 0,
  `v25` bigint NULL DEFAULT 0,
  `v26` bigint NULL DEFAULT 0,
  `v27` bigint NULL DEFAULT 0,
  `v28` bigint NULL DEFAULT 0,
  `v29` bigint NULL DEFAULT 0,
  `v30` bigint NULL DEFAULT 0,
  `v31` bigint NULL DEFAULT 0,
  `v32` bigint NULL DEFAULT 0,
  `v33` bigint NULL DEFAULT 0,
  `v34` bigint NULL DEFAULT 0,
  `v35` bigint NULL DEFAULT 0,
  `v36` bigint NULL DEFAULT 0,
  `v37` bigint NULL DEFAULT 0,
  `v38` bigint NULL DEFAULT 0,
  `v39` bigint NULL DEFAULT 0,
  PRIMARY KEY (`rid`, `idx`) USING BTREE
) ENGINE = MyISAM AUTO_INCREMENT = 1 CHARACTER SET = ascii COLLATE = ascii_bin ROW_FORMAT = FIXED;

-- ----------------------------
-- Table structure for t_roles
-- ----------------------------
DROP TABLE IF EXISTS `t_roles`;
CREATE TABLE `t_roles`  (
  `rid` int NOT NULL AUTO_INCREMENT COMMENT 'Id của nhân vât',
  `userid` char(64) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL COMMENT 'Id Tài khoản',
  `rname` char(32) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'Tên nhân vật',
  `sex` tinyint UNSIGNED NOT NULL DEFAULT 0 COMMENT 'Giới tính',
  `occupation` tinyint UNSIGNED NOT NULL DEFAULT 0 COMMENT 'Phái nào',
  `sub_id` tinyint NOT NULL COMMENT 'Nhánh của phái',
  `level` smallint UNSIGNED NOT NULL DEFAULT 1 COMMENT 'Cấp độ',
  `pic` int UNSIGNED NOT NULL DEFAULT 0 COMMENT 'hình đại diện',
  `money1` int NOT NULL DEFAULT 0 COMMENT 'Bạc',
  `money2` int NOT NULL DEFAULT 0 COMMENT 'Bạc khóa',
  `experience` bigint UNSIGNED NOT NULL DEFAULT 0 COMMENT 'kinh nghiệm',
  `pkmode` tinyint UNSIGNED NOT NULL DEFAULT 0 COMMENT 'Chế độ pk hiện tại',
  `pkvalue` int NOT NULL DEFAULT 0 COMMENT 'giá trị pk hiện tại',
  `position` char(32) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL DEFAULT '-1:0:-1:-1' COMMENT 'vị trí nhân vật',
  `regtime` datetime(0) NOT NULL DEFAULT '1900-01-01 12:00:00' COMMENT 'Thời gian đăng ký',
  `lasttime` datetime(0) NOT NULL DEFAULT '1900-01-01 12:00:00' COMMENT 'Thời gian đăng nhập gần đây',
  `isdel` tinyint UNSIGNED NOT NULL DEFAULT 0 COMMENT 'Đã bị xóa chưa',
  `deltime` datetime(0) NOT NULL DEFAULT '1900-01-01 12:00:00' COMMENT 'Thời gian bắt đầu xóa',
  `predeltime` datetime(0) NULL DEFAULT NULL COMMENT 'Thười gian sẽ xóa',
  `bagnum` int UNSIGNED NOT NULL DEFAULT 0 COMMENT 'Số ô trong túi đồ',
  `main_quick_keys` char(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL DEFAULT '' COMMENT 'Main Skill',
  `other_quick_keys` char(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL DEFAULT '' COMMENT 'Vòng sáng',
  `loginnum` int UNSIGNED NOT NULL DEFAULT 0 COMMENT 'Số lần đăng nhập',
  `leftfightsecs` int UNSIGNED NOT NULL DEFAULT 0 COMMENT 'Số giây thách đầu con lại',
  `totalonlinesecs` int NOT NULL DEFAULT 0 COMMENT 'tổng thời gian online',
  `antiaddictionsecs` int NOT NULL DEFAULT 0 COMMENT 'Thời gian có hại đăng nhập',
  `logofftime` datetime(0) NOT NULL DEFAULT '1900-01-01 12:00:00' COMMENT 'Thời gian đăng xuất',
  `yinliang` int UNSIGNED NOT NULL DEFAULT 0 COMMENT 'ĐỒng khóa',
  `maintaskid` int NOT NULL DEFAULT 0 COMMENT 'Nhiệm vụ chính tuyến ID',
  `pkpoint` int NOT NULL DEFAULT 0 COMMENT 'Điểm PK',
  `killboss` int NOT NULL DEFAULT 0 COMMENT 'Số lần giết boss',
  `cztaskid` int NOT NULL DEFAULT 0 COMMENT 'giữ liệu nạp thẻ',
  `logindayid` int NOT NULL DEFAULT 0 COMMENT 'đăng nhập ngày bao nhiêu',
  `logindaynum` int NOT NULL DEFAULT 0 COMMENT 'Đăng nhập ngày nàop',
  `zoneid` int NOT NULL DEFAULT 0,
  `username` char(64) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `lastmailid` int UNSIGNED NOT NULL DEFAULT 0,
  `onceawardflag` bigint UNSIGNED NOT NULL DEFAULT 0 COMMENT 'Đánh dấu sự kiện nhận thưởng',
  `banchat` int UNSIGNED NOT NULL DEFAULT 0 COMMENT 'Band chát',
  `banlogin` int UNSIGNED NOT NULL DEFAULT 0 COMMENT 'Badn login',
  `isflashplayer` int NOT NULL DEFAULT 0 COMMENT 'Có phải người chơi mới',
  `admiredcount` int NOT NULL DEFAULT 0 COMMENT 'thời gian bảo vệ',
  `store_yinliang` bigint UNSIGNED NOT NULL DEFAULT 0 COMMENT 'Kho chứa đồng',
  `store_money` bigint UNSIGNED NOT NULL DEFAULT 0 COMMENT 'Kho chứa bạc',
  `ban_trade_to_ticks` bigint NOT NULL DEFAULT 0,
  `familyid` int NOT NULL DEFAULT 0,
  `familyname` varchar(100) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `familyrank` tinyint NOT NULL DEFAULT 0,
  `guildname` varchar(100) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `guildid` int UNSIGNED NOT NULL DEFAULT 0,
  `guildrank` int NOT NULL DEFAULT 0,
  `roleprestige` int NOT NULL DEFAULT 0,
  `guildmoney` int NOT NULL DEFAULT 0,
  `quick_items` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'Danh sách vật phẩm dùng nhanh',
  `second_password` varchar(8) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'Mật khẩu cấp 2',
  UNIQUE INDEX `rid`(`rid`) USING BTREE,
  UNIQUE INDEX `rname_zoneid`(`rname`, `zoneid`) USING BTREE,
  INDEX `userid`(`userid`) USING BTREE,
  INDEX `idx_faction`(`guildid`) USING BTREE
) ENGINE = MyISAM AUTO_INCREMENT = 606026 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = FIXED;

-- ----------------------------
-- Table structure for t_skills
-- ----------------------------
DROP TABLE IF EXISTS `t_skills`;
CREATE TABLE `t_skills`  (
  `Id` int NOT NULL AUTO_INCREMENT,
  `rid` int NOT NULL DEFAULT 0,
  `skillid` int UNSIGNED NOT NULL DEFAULT 0,
  `skilllevel` int UNSIGNED NOT NULL DEFAULT 0,
  `lastusedtick` bigint NOT NULL DEFAULT 0,
  `exp` int NOT NULL,
  `cooldowntick` int NOT NULL,
  PRIMARY KEY (`Id`) USING BTREE,
  UNIQUE INDEX `rid_skillid`(`rid`, `skillid`) USING BTREE,
  INDEX `rid`(`rid`) USING BTREE
) ENGINE = MyISAM AUTO_INCREMENT = 300561590 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = FIXED;

-- ----------------------------
-- Table structure for t_stalldata
-- ----------------------------
DROP TABLE IF EXISTS `t_stalldata`;
CREATE TABLE `t_stalldata`  (
  `RoleID` int NOT NULL,
  `StartSellDate` int NULL DEFAULT NULL,
  `StallData` varchar(6000) CHARACTER SET latin1 COLLATE latin1_swedish_ci NULL DEFAULT NULL,
  PRIMARY KEY (`RoleID`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = latin1 COLLATE = latin1_swedish_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for t_systemglobalvalue
-- ----------------------------
DROP TABLE IF EXISTS `t_systemglobalvalue`;
CREATE TABLE `t_systemglobalvalue`  (
  `id` int NOT NULL,
  `value` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE = MyISAM AUTO_INCREMENT = 1 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for t_tasks
-- ----------------------------
DROP TABLE IF EXISTS `t_tasks`;
CREATE TABLE `t_tasks`  (
  `Id` int NOT NULL AUTO_INCREMENT,
  `taskid` int NOT NULL DEFAULT 0,
  `rid` int NOT NULL DEFAULT 0,
  `focus` int UNSIGNED NOT NULL DEFAULT 0,
  `value1` int UNSIGNED NOT NULL DEFAULT 0,
  `value2` int UNSIGNED NOT NULL DEFAULT 0,
  `isdel` tinyint UNSIGNED NOT NULL DEFAULT 0,
  `addtime` datetime(0) NOT NULL,
  `starlevel` int NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`) USING BTREE,
  INDEX `rid`(`rid`) USING BTREE
) ENGINE = MyISAM AUTO_INCREMENT = 2067059 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = FIXED;

-- ----------------------------
-- Table structure for t_taskslog
-- ----------------------------
DROP TABLE IF EXISTS `t_taskslog`;
CREATE TABLE `t_taskslog`  (
  `rid` int NOT NULL DEFAULT 0,
  `taskid` int NOT NULL DEFAULT 0,
  `count` int UNSIGNED NOT NULL DEFAULT 0,
  `taskclass` int NULL DEFAULT NULL,
  UNIQUE INDEX `taskid_rid`(`rid`, `taskid`) USING BTREE,
  INDEX `rid`(`rid`) USING BTREE
) ENGINE = MyISAM AUTO_INCREMENT = 1 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = FIXED;

-- ----------------------------
-- Table structure for t_teambattle_team
-- ----------------------------
DROP TABLE IF EXISTS `t_teambattle_team`;
CREATE TABLE `t_teambattle_team`  (
  `last_win_time` datetime(0) NULL DEFAULT NULL,
  `has_awards` int NULL DEFAULT 0,
  `stage` int NOT NULL,
  `total_battles` int NOT NULL,
  `point` int NOT NULL,
  `register_time` datetime(0) NOT NULL,
  `member_4_id` int NOT NULL DEFAULT -1,
  `member_3_id` int NOT NULL DEFAULT -1,
  `member_2_id` int NOT NULL DEFAULT -1,
  `member_1_id` int NOT NULL DEFAULT -1,
  `name` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `aid` int UNSIGNED NOT NULL AUTO_INCREMENT,
  PRIMARY KEY (`aid`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for t_territory
-- ----------------------------
DROP TABLE IF EXISTS `t_territory`;
CREATE TABLE `t_territory`  (
  `ID` int NOT NULL AUTO_INCREMENT,
  `MapID` int NULL DEFAULT NULL,
  `MapName` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `GuildID` int NULL DEFAULT NULL,
  `Star` int NULL DEFAULT NULL,
  `Tax` int NULL DEFAULT NULL,
  `ZoneID` int NULL DEFAULT NULL,
  `IsMainCity` int NULL DEFAULT NULL,
  PRIMARY KEY (`ID`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for t_usemoney_log
-- ----------------------------
DROP TABLE IF EXISTS `t_usemoney_log`;
CREATE TABLE `t_usemoney_log`  (
  `Id` int NOT NULL AUTO_INCREMENT,
  `DBId` int NULL DEFAULT NULL,
  `userid` varchar(64) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `ObjName` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `optFrom` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `currEnvName` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `tarEnvName` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `optType` char(6) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `optTime` datetime(0) NULL DEFAULT NULL,
  `optAmount` int NULL DEFAULT NULL,
  `zoneID` int NULL DEFAULT NULL,
  `optSurplus` int NULL DEFAULT NULL,
  PRIMARY KEY (`Id`) USING BTREE,
  INDEX `DBId`(`DBId`) USING BTREE,
  INDEX `tarEnvName`(`tarEnvName`) USING BTREE
) ENGINE = MyISAM AUTO_INCREMENT = 308 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for t_user_active_info
-- ----------------------------
DROP TABLE IF EXISTS `t_user_active_info`;
CREATE TABLE `t_user_active_info`  (
  `Account` varchar(64) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `createTime` date NOT NULL,
  `seriesLoginCount` int NOT NULL DEFAULT 0,
  `lastSeriesLoginTime` date NOT NULL,
  PRIMARY KEY (`Account`) USING BTREE
) ENGINE = MyISAM AUTO_INCREMENT = 1 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for t_voteguild
-- ----------------------------
DROP TABLE IF EXISTS `t_voteguild`;
CREATE TABLE `t_voteguild`  (
  `ID` int NOT NULL AUTO_INCREMENT,
  `ZoneID` int NULL DEFAULT NULL,
  `GuildID` int NULL DEFAULT NULL,
  `RoleVote` int NULL DEFAULT NULL,
  `VoteCount` int NULL DEFAULT NULL,
  `WeekID` int NULL DEFAULT NULL,
  `RoleReviceVote` int NULL DEFAULT NULL,
  PRIMARY KEY (`ID`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for t_welfare
-- ----------------------------
DROP TABLE IF EXISTS `t_welfare`;
CREATE TABLE `t_welfare`  (
  `rid` int NOT NULL COMMENT 'RoleID là khóa chính luôn',
  `lastdaylogin` int NULL DEFAULT NULL COMMENT 'Ngày gần đây nhất đã đăng nhập là ngày nào sử dụng để tính toán đăng nhập liên tục',
  `logincontinus` int NULL DEFAULT NULL COMMENT 'Đăng nhập liên tục trong bao lâu',
  `sevenday_continus_step` int NULL DEFAULT NULL COMMENT 'Đã nhận mốc nào rồi trong cái đăng nhập 7 ngày liên tục | Cái này sẽ ko reset suốt đời',
  `sevenday_continus_note` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'Quà đã nhận khi đăng nhập liên tiếp trong 7 ngày | Cái này sẽ ko reset suốt đời',
  `sevendaylogin_note` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'Quà đã nhận khi đăng nhập 7 ngày DAYID|ITEMID|COUNT_DAYID|ITEMID|COUNT',
  `sevendaylogin_step` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'Đã nhận mốc nào rồi trong cái đăng nhập 7 ngày  VD : 1|2|3|4',
  `createdayid` int NULL DEFAULT NULL COMMENT 'Ghi lại ngày đầu tiên nhân vật được tạo và kích hoạt chuỗi sự kiện 7 ngày',
  `logindayid` int NULL DEFAULT NULL COMMENT 'Ghi lại ngày đăng nhập vào là ngày nào | để tính toán cái online nhận thưởng=> sang ngày mới là clear chuỗi nhận',
  `loginweekid` int NULL DEFAULT NULL COMMENT 'Ghi lại tuần đăng nhập là tuần nào',
  `online_step` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'Ghi lại DayLoginID và STEP nào ví dụ 254_1|2 tức là vào ngày 254 người này đã online nhận 2 mốc thưởng | Sang ngày mới thì tự động reset',
  `levelup_step` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'Ghi lại xem thằng nhân vật này đã nhận quà thăng cấp mốc nào rồi ghi theo chuỗi 1_2_3 tức là đã nhận mốc 1 2 3 nếu mà chuỗi là 2_3 thì tức là chỉ nhận mốc 2 và 3',
  `monthid` int NULL DEFAULT NULL COMMENT 'Ghi lại xem tháng đăng nhập này là tháng nào để xử lý quà điểm danh 30 ngày',
  `checkpoint` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'Ghi lại xem đã điểm danh ngày nào trong tháng | Nếu tháng này mà khác tháng cũ thì sẽ reset checkpoint đánh dấu 2_3_4 tức là đã nhận ngày 2 3 4',
  `fist_recharge_step` int NULL DEFAULT NULL COMMENT 'Ghi lại xem đã nhận quà nạp lần đầu hay chưa | 0 là chưa nhận | 1 là đã nhận',
  `totarechage_step` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'Ghi lại xem đã hốc mốc quà tích nạp nào rồi 2_3_4 tức là đã nhận mốc 2 3 4',
  `totalconsume_step` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'Ghi lại xem đã hốc mốc quà tích tiêu nào rồi nào rồi 2_3_4 tức là đã nhận mốc 2 3 4',
  `day_rechage_step` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'Ghi lại tích lũy nạp ngày bao gồm ngày ID và STEP nào ví dụ 254_1_2_3 tức là vào ngày 254 người này đã nhận mốc 1 2 3 | Sang ngày mới bản ghi này tự động reset ',
  PRIMARY KEY (`rid`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = DYNAMIC;

SET FOREIGN_KEY_CHECKS = 1;
