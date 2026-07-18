/*
 Navicat Premium Data Transfer

 Source Server         : zzn
 Source Server Type    : MySQL
 Source Server Version : 80040
 Source Host           : localhost:3306
 Source Schema         : center

 Target Server Type    : MySQL
 Target Server Version : 80040
 File Encoding         : 65001

 Date: 18/07/2026 14:59:49
*/

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ----------------------------
-- Table structure for account
-- ----------------------------
DROP TABLE IF EXISTS `account`;
CREATE TABLE `account`  (
  `id` bigint unsigned NOT NULL COMMENT '主键ID',
  `account` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT '' COMMENT '登录账号',
  `password` varchar(256) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT '' COMMENT '加密密码',
  `auth_key` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT '' COMMENT '鉴权密钥(原key，规避保留字)',
  `player_id` bigint unsigned NOT NULL COMMENT '关联玩家ID',
  `hall_server_id` smallint unsigned NOT NULL COMMENT '大厅服务器ID',
  `create_time` bigint unsigned NOT NULL COMMENT '创建时间戳',
  `last_login_time` bigint unsigned NOT NULL COMMENT '最后登录时间戳',
  `channel` smallint(0) NOT NULL DEFAULT 0 COMMENT '渠道ID',
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE INDEX `uk_account`(`account`) USING BTREE,
  INDEX `idx_player_id`(`player_id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 7 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_unicode_ci COMMENT = '【中心服】账号基础表' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of account
-- ----------------------------
INSERT INTO `account` VALUES (1, 'AAANB123', '123456', '9a3bb44d84044c7d', 1777479993812, 0, 1777479993, 1783874958, 1);
INSERT INTO `account` VALUES (2, 'AANNB123', '123456', 'bfc2fa3a30924d0d', 1777534882929, 0, 1777534882, 1777747808, 1);
INSERT INTO `account` VALUES (3, 'ANNNB123', '123456', '1487217373984a0c', 1777733303143, 0, 1777733303, 1777747782, 1);
INSERT INTO `account` VALUES (4, 'BBBNB123', '123456', '080ccb6c4fa244f5', 1777819781689, 0, 1777819781, 1783613674, 1);
INSERT INTO `account` VALUES (5, 'BBNNB123', '123456', 'b709174fac0f497d', 1777962385332, 0, 1777962385, 1777962389, 1);
INSERT INTO `account` VALUES (6, 'CCCNB123', '123456', 'dd16640f75ff442d', 1778476368270, 0, 1778476368, 1778478076, 1);

SET FOREIGN_KEY_CHECKS = 1;
