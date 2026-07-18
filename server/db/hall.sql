/*
 Navicat Premium Data Transfer

 Source Server         : zzn
 Source Server Type    : MySQL
 Source Server Version : 80040
 Source Host           : localhost:3306
 Source Schema         : hall

 Target Server Type    : MySQL
 Target Server Version : 80040
 File Encoding         : 65001

 Date: 18/07/2026 15:00:25
*/

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ----------------------------
-- Table structure for email
-- ----------------------------
DROP TABLE IF EXISTS `email`;
CREATE TABLE `email`  (
  `player_id` bigint unsigned NOT NULL COMMENT '玩家ID',
  `guid` bigint unsigned NOT NULL COMMENT '邮件唯一ID',
  `email_type` tinyint unsigned NOT NULL COMMENT '邮件类型',
  `status` tinyint unsigned NOT NULL COMMENT '状态：0未读 1已读 2已领取 3已删除',
  `from_id` bigint unsigned NOT NULL COMMENT '发送方ID',
  `title` varchar(256) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT '' COMMENT '邮件标题',
  `content` varchar(8192) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT '' COMMENT '邮件内容',
  `create_time` bigint unsigned NOT NULL COMMENT '创建时间戳',
  `valid_time` bigint unsigned NOT NULL COMMENT '过期时间戳',
  `item_list` json NOT NULL COMMENT '奖励物品列表',
  PRIMARY KEY (`player_id`, `guid`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_unicode_ci COMMENT = '【大厅服】玩家邮件表' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of email
-- ----------------------------
INSERT INTO `email` VALUES (1777479993812, 1777870515360, 1, 0, 0, '注册欢迎邮件', '欢迎注册上线，赠送初始资源', 1777870512, 0, '[]');
INSERT INTO `email` VALUES (1777534882929, 1777744714109, 1, 0, 0, '注册欢迎邮件', '欢迎注册上线，赠送初始资源', 1777744712, 0, '[]');
INSERT INTO `email` VALUES (1777733303143, 1777747792055, 1, 0, 0, '注册欢迎邮件', '欢迎注册上线，赠送初始资源', 1777747782, 0, '[]');
INSERT INTO `email` VALUES (1777819781689, 1783613678776, 1, 0, 0, '注册欢迎邮件', '欢迎注册上线，赠送初始资源', 1783613674, 0, '[]');
INSERT INTO `email` VALUES (1777962385332, 1777962391726, 1, 0, 0, '注册欢迎邮件', '欢迎注册上线，赠送初始资源', 1777962389, 0, '[]');
INSERT INTO `email` VALUES (1778476368270, 1778476379313, 1, 0, 0, '注册欢迎邮件', '欢迎注册上线，赠送初始资源', 1778476374, 0, '[]');

-- ----------------------------
-- Table structure for email_config
-- ----------------------------
DROP TABLE IF EXISTS `email_config`;
CREATE TABLE `email_config`  (
  `id` bigint unsigned NOT NULL COMMENT '主键ID',
  `file_name` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT '' COMMENT '邮件文件名(如Email-ini.txt/Email-2026-5-2.txt)',
  `email_type` tinyint unsigned NOT NULL COMMENT '邮件类型(与email表一致)',
  `from_id` bigint unsigned NOT NULL COMMENT '发送方ID(系统固定为0)',
  `title` varchar(256) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT '' COMMENT '邮件标题',
  `content` varchar(8192) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT '' COMMENT '邮件内容',
  `valid_days` tinyint unsigned NOT NULL COMMENT '有效天数(0=长期有效)',
  `item_list` json NOT NULL COMMENT '奖励物品列表(JSON格式)',
  `create_time` bigint unsigned NOT NULL COMMENT '创建时间戳',
  `update_time` bigint unsigned NOT NULL COMMENT '最后更新时间戳',
  `is_enabled` tinyint unsigned NOT NULL COMMENT '是否启用：1启用 0禁用',
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE INDEX `uk_file_name`(`file_name`) USING BTREE COMMENT '文件名唯一，防止重复导入',
  INDEX `idx_is_enabled`(`is_enabled`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 2 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_unicode_ci COMMENT = '【大厅服】邮件配置模板表' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of email_config
-- ----------------------------
INSERT INTO `email_config` VALUES (1, 'Email-ini.txt', 1, 0, '注册欢迎邮件', '欢迎注册上线，赠送初始资源', 0, '[]', 1777733946, 1777733946, 1);

-- ----------------------------
-- Table structure for friend
-- ----------------------------
DROP TABLE IF EXISTS `friend`;
CREATE TABLE `friend`  (
  `player_id` bigint unsigned NOT NULL COMMENT '玩家ID',
  `friend_id` bigint unsigned NOT NULL COMMENT '好友ID',
  `create_time` bigint unsigned NOT NULL COMMENT '添加时间戳',
  PRIMARY KEY (`player_id`, `friend_id`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_unicode_ci COMMENT = '【大厅服】玩家好友关系表' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for item
-- ----------------------------
DROP TABLE IF EXISTS `item`;
CREATE TABLE `item`  (
  `player_id` bigint unsigned NOT NULL COMMENT '玩家ID',
  `id` bigint unsigned NOT NULL COMMENT '物品ID',
  `count` bigint unsigned NOT NULL COMMENT '物品数量',
  PRIMARY KEY (`player_id`, `id`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_unicode_ci COMMENT = '【大厅服】玩家物品表' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of item
-- ----------------------------
INSERT INTO `item` VALUES (1777479993812, 10001, 220);
INSERT INTO `item` VALUES (1777479993812, 10002, 50);
INSERT INTO `item` VALUES (1777534882929, 10001, 220);
INSERT INTO `item` VALUES (1777534882929, 10002, 50);
INSERT INTO `item` VALUES (1777733303143, 10001, 220);
INSERT INTO `item` VALUES (1777733303143, 10002, 50);
INSERT INTO `item` VALUES (1777819781689, 10001, 200);
INSERT INTO `item` VALUES (1777819781689, 10002, 50);
INSERT INTO `item` VALUES (1777962385332, 10001, 220);
INSERT INTO `item` VALUES (1777962385332, 10002, 50);
INSERT INTO `item` VALUES (1778476368270, 10001, 220);
INSERT INTO `item` VALUES (1778476368270, 10002, 50);

-- ----------------------------
-- Table structure for player
-- ----------------------------
DROP TABLE IF EXISTS `player`;
CREATE TABLE `player`  (
  `player_id` bigint unsigned NOT NULL COMMENT '玩家ID(主键)',
  `nickname` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT '' COMMENT '玩家昵称',
  `create_time` bigint unsigned NOT NULL COMMENT '创建时间戳',
  `last_login_time` bigint unsigned NOT NULL COMMENT '最后登录时间戳',
  `last_logout_time` bigint unsigned NOT NULL COMMENT '最后登出时间戳',
  `rank_score` int unsigned NOT NULL COMMENT '排位积分',
  PRIMARY KEY (`player_id`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_unicode_ci COMMENT = '【大厅服】玩家基础信息表' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of player
-- ----------------------------
INSERT INTO `player` VALUES (1777479993812, 'Player1777479993812', 1777479993, 1783875185, 1783875188, 0);
INSERT INTO `player` VALUES (1777534882929, 'Player1777534882929', 1777534882, 1777747808, 1777747848, 0);
INSERT INTO `player` VALUES (1777733303143, 'Player1777733303143', 1777733303, 1777747782, 1777747797, 0);
INSERT INTO `player` VALUES (1777819781689, 'Player1777819781689', 1777819781, 1783613835, 1783613856, 0);
INSERT INTO `player` VALUES (1777962385332, 'Player1777962385332', 1777962385, 1777962389, 0, 0);
INSERT INTO `player` VALUES (1778476368270, 'Player1778476368270', 1778476368, 1778476374, 1778476559, 0);

SET FOREIGN_KEY_CHECKS = 1;
