/*
 Navicat Premium Data Transfer

 Source Server         : zzn
 Source Server Type    : MySQL
 Source Server Version : 80040
 Source Host           : localhost:3306
 Source Schema         : fgo-pve

 Target Server Type    : MySQL
 Target Server Version : 80040
 File Encoding         : 65001

 Date: 18/07/2026 15:00:01
*/

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ----------------------------
-- Table structure for detail_status_record
-- ----------------------------
DROP TABLE IF EXISTS `detail_status_record`;
CREATE TABLE `detail_status_record`  (
  `id` bigint unsigned NOT NULL COMMENT '主键',
  `roughly_id` bigint unsigned NOT NULL COMMENT '关联粗略战斗ID',
  `room_id` bigint unsigned NOT NULL COMMENT '房间ID',
  `round` int unsigned NOT NULL COMMENT '回合数',
  `round_type_number` tinyint unsigned NOT NULL COMMENT '回合状态编码(1-8)',
  `round_type_name` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '回合状态名称',
  `player_id` bigint unsigned NOT NULL COMMENT '操作玩家ID',
  `player_mana` int unsigned NOT NULL COMMENT '当前魔力',
  `player_stamina` int unsigned NOT NULL COMMENT '当前体力',
  `player_card_left` int unsigned NOT NULL COMMENT '剩余卡牌数',
  `char_hp` json NOT NULL COMMENT '角色HP',
  `card_used` json NOT NULL COMMENT '本回合使用卡牌',
  `round_start_time` bigint unsigned NOT NULL COMMENT '回合开始时间戳',
  `round_end_time` bigint unsigned NOT NULL COMMENT '回合结束时间戳',
  `create_time` bigint unsigned NOT NULL COMMENT '记录时间戳',
  PRIMARY KEY (`id`) USING BTREE,
  INDEX `idx_roughly_id`(`roughly_id`) USING BTREE,
  INDEX `idx_room_round`(`room_id`, `round`) USING BTREE,
  INDEX `idx_player_id`(`player_id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_unicode_ci COMMENT = '【战斗服】战斗详细记录表' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for room_template
-- ----------------------------
DROP TABLE IF EXISTS `room_template`;
CREATE TABLE `room_template`  (
  `id` bigint unsigned NOT NULL COMMENT '主键',
  `room_id` bigint unsigned NOT NULL COMMENT '房间ID',
  `map_id` bigint unsigned NOT NULL COMMENT '所属地图ID',
  `room_type` tinyint unsigned NOT NULL COMMENT '房间类型：0PVE 1普通PVP 2终极PVP 3高端PVP',
  `room_type_name` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '房间类型名',
  `status` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT '空闲' COMMENT '状态：空闲/等待/已满',
  `bag_item_id` bigint unsigned NULL COMMENT '入场道具ID',
  `bag_item_count` int unsigned NULL COMMENT '入场道具数量',
  `mana` int unsigned NOT NULL COMMENT '基础魔力消耗',
  `stamina` int unsigned NOT NULL COMMENT '基础体力消耗',
  `is_pvp` tinyint unsigned NOT NULL COMMENT '是否PVP：1是 0否',
  `create_time` bigint unsigned NOT NULL COMMENT '创建时间戳',
  `update_time` bigint unsigned NOT NULL COMMENT '更新时间戳',
  `target_player_id` bigint unsigned NOT NULL COMMENT '掠夺副本目标玩家ID',
  `region_id` bigint unsigned NOT NULL COMMENT '副本所属区域ID',
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE INDEX `uk_room_id`(`room_id`) USING BTREE,
  INDEX `idx_room_type`(`room_type`) USING BTREE,
  INDEX `idx_is_pvp`(`is_pvp`) USING BTREE,
  INDEX `idx_map_id`(`map_id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 2 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_unicode_ci COMMENT = '【战斗服】房间模板表' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of room_template
-- ----------------------------
INSERT INTO `room_template` VALUES (1, 100004, 1, 0, '副本战斗局', '已满', NULL, NULL, 20, 50, 0, 1777808897, 1777808897, 0, 0);

-- ----------------------------
-- Table structure for roughly_status_record
-- ----------------------------
DROP TABLE IF EXISTS `roughly_status_record`;
CREATE TABLE `roughly_status_record`  (
  `id` bigint unsigned NOT NULL COMMENT '主键',
  `room_id` bigint unsigned NOT NULL COMMENT '房间ID',
  `room_type` tinyint unsigned NOT NULL COMMENT '房间类型',
  `region_id` bigint unsigned NOT NULL COMMENT '区域ID',
  `player1_id` bigint unsigned NOT NULL COMMENT '玩家1ID',
  `player2_id` bigint unsigned NOT NULL COMMENT '玩家2ID(PVE为机器人)',
  `winner_id` bigint unsigned NULL COMMENT '获胜者ID',
  `start_time` bigint unsigned NOT NULL COMMENT '战斗开始时间戳',
  `end_time` bigint unsigned NOT NULL COMMENT '战斗结束时间戳',
  `total_round` int unsigned NOT NULL COMMENT '总回合数',
  `end_reason` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '结束原因',
  `create_time` bigint unsigned NOT NULL COMMENT '记录时间戳',
  PRIMARY KEY (`id`) USING BTREE,
  INDEX `idx_room_id`(`room_id`) USING BTREE,
  INDEX `idx_player1_id`(`player1_id`) USING BTREE,
  INDEX `idx_player2_id`(`player2_id`) USING BTREE,
  INDEX `idx_region_id`(`region_id`) USING BTREE,
  INDEX `idx_start_end_time`(`start_time`, `end_time`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_unicode_ci COMMENT = '【战斗服】战斗粗略记录表' ROW_FORMAT = Dynamic;

SET FOREIGN_KEY_CHECKS = 1;
