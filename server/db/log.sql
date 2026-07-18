/*
 Navicat Premium Data Transfer

 Source Server         : skynet
 Source Server Type    : MySQL
 Source Server Version : 80045
 Source Host           : localhost:3306
 Source Schema         : log

 Target Server Type    : MySQL
 Target Server Version : 80045
 File Encoding         : 65001

 Date: 18/07/2026 15:01:05
*/

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ----------------------------
-- Table structure for add_email_log
-- ----------------------------
DROP TABLE IF EXISTS `add_email_log`;
CREATE TABLE `add_email_log`  (
  `player_id` bigint(0) NOT NULL DEFAULT 0,
  `guid` bigint(0) NOT NULL DEFAULT 0,
  `from_id` bigint(0) NOT NULL DEFAULT 0,
  `email_type` bigint(0) NOT NULL DEFAULT 0,
  `title` varchar(256) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL DEFAULT '',
  `content` varchar(8192) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL DEFAULT '',
  `vaild_time` bigint(0) NOT NULL DEFAULT 0,
  `item_list` blob NULL,
  `_guid` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL DEFAULT '',
  `_time` int unsigned NOT NULL,
  `_svr_type` tinyint unsigned NOT NULL,
  `_svr_id` smallint unsigned NOT NULL,
  PRIMARY KEY (`_guid`) USING BTREE,
  INDEX `player_index`(`player_id`) USING BTREE,
  INDEX `type_index`(`email_type`) USING BTREE,
  INDEX `svr_index`(`_svr_type`, `_svr_id`) USING BTREE,
  INDEX `time_index`(`_time`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of add_email_log
-- ----------------------------
INSERT INTO `add_email_log` VALUES (1000100010000001, 550305569701889, 0, 2, '20260717-233904登录奖励', '今天是您注册游戏的第1天，奉上礼包，祝您玩的愉快！', 1785512344, 0x5B7B22636F756E74223A3230302C226964223A31303030303030327D2C7B22636F756E74223A3530302C226964223A31303030303030317D5D, '05-0001-0000002a-6a5a4c98-000005', 1784302744, 5, 1);
INSERT INTO `add_email_log` VALUES (1999900010000001, 550305569701889, 0, 2, '20260717-211702登录奖励', '今天是您注册游戏的第1天，奉上礼包，祝您玩的愉快！', 1785503822, 0x5B7B22636F756E74223A3230302C226964223A31303030303030327D2C7B22636F756E74223A3530302C226964223A31303030303030317D5D, '05-0001-0000002d-6a5a2b4f-000000', 1784294222, 5, 1);
INSERT INTO `add_email_log` VALUES (1999900010000002, 550305569701890, 0, 2, '20260717-211703登录奖励', '今天是您注册游戏的第1天，奉上礼包，祝您玩的愉快！', 1785503823, 0x5B7B22636F756E74223A3230302C226964223A31303030303030327D2C7B22636F756E74223A3530302C226964223A31303030303030317D5D, '05-0001-0000002e-6a5a2b4f-000004', 1784294223, 5, 1);
INSERT INTO `add_email_log` VALUES (1000100010000006, 550305569701893, 0, 2, '20260718-120027登录奖励', '今天是您注册游戏的第1天，奉上礼包，祝您玩的愉快！', 1785556827, 0x5B7B22636F756E74223A3230302C226964223A31303030303030327D2C7B22636F756E74223A3530302C226964223A31303030303030317D5D, '05-0001-00000040-6a5afa5b-000004', 1784347227, 5, 1);
INSERT INTO `add_email_log` VALUES (1000100010000001, 550305569701890, 0, 2, '20260718-110313登录奖励', '今天是您注册游戏的第2天，奉上礼包，祝您玩的愉快！', 1785553393, 0x5B7B22636F756E74223A3230302C226964223A31303030303030327D2C7B22636F756E74223A3530302C226964223A31303030303030317D5D, '05-0001-00000042-6a5aecf1-000004', 1784343793, 5, 1);
INSERT INTO `add_email_log` VALUES (1000100010000007, 550305569701894, 0, 2, '20260718-131642登录奖励', '今天是您注册游戏的第1天，奉上礼包，祝您玩的愉快！', 1785561402, 0x5B7B22636F756E74223A3230302C226964223A31303030303030327D2C7B22636F756E74223A3530302C226964223A31303030303030317D5D, '05-0001-00000042-6a5b0c3a-000000', 1784351802, 5, 1);
INSERT INTO `add_email_log` VALUES (1000100010000002, 550305569701891, 0, 2, '20260718-110513登录奖励', '今天是您注册游戏的第2天，奉上礼包，祝您玩的愉快！', 1785553513, 0x5B7B22636F756E74223A3230302C226964223A31303030303030327D2C7B22636F756E74223A3530302C226964223A31303030303030317D5D, '05-0001-0000004a-6a5aed69-000004', 1784343913, 5, 1);
INSERT INTO `add_email_log` VALUES (1000100010000005, 550305569701892, 0, 2, '20260718-111921登录奖励', '今天是您注册游戏的第1天，奉上礼包，祝您玩的愉快！', 1785554361, 0x5B7B22636F756E74223A3230302C226964223A31303030303030327D2C7B22636F756E74223A3530302C226964223A31303030303030317D5D, '05-0001-0000004d-6a5af0b9-000005', 1784344761, 5, 1);
INSERT INTO `add_email_log` VALUES (1999900020000001, 550855325515777, 0, 2, '20260717-211702登录奖励', '今天是您注册游戏的第1天，奉上礼包，祝您玩的愉快！', 1785503822, 0x5B7B22636F756E74223A3230302C226964223A31303030303030327D2C7B22636F756E74223A3530302C226964223A31303030303030317D5D, '05-0002-0000002a-6a5a2b4f-000000', 1784294223, 5, 2);
INSERT INTO `add_email_log` VALUES (1999900020000002, 550855325515778, 0, 2, '20260717-211704登录奖励', '今天是您注册游戏的第1天，奉上礼包，祝您玩的愉快！', 1785503824, 0x5B7B22636F756E74223A3230302C226964223A31303030303030327D2C7B22636F756E74223A3530302C226964223A31303030303030317D5D, '05-0002-0000002c-6a5a2b50-000000', 1784294224, 5, 2);

-- ----------------------------
-- Table structure for error_log
-- ----------------------------
DROP TABLE IF EXISTS `error_log`;
CREATE TABLE `error_log`  (
  `_guid` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL DEFAULT '',
  `_svr_type` tinyint unsigned NOT NULL,
  `_svr_id` smallint unsigned NOT NULL,
  `_time` int unsigned NOT NULL,
  `_err_str` text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL,
  PRIMARY KEY (`_guid`) USING BTREE,
  INDEX `svr_index`(`_svr_type`, `_svr_id`) USING BTREE,
  INDEX `time_index`(`_time`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of error_log
-- ----------------------------
INSERT INTO `error_log` VALUES ('05-0001-00000001-6a5a4ed2-000002', 5, 1, 1784303314, '[:00000034][20260717 23:48:34 42][error][05-0001-00000034-6a5a4ed2-000002][room_game_login][../../commonlualib/common/plug/login_plug.lua:74]\"token err \" {\n	[\"player_id\"] = 1000100020000002,\n	[\"token\"] = \"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYmYiOjE3ODQzMDMzMTMsImlzcyI6ImhhbGxzZXJ2ZXI6MiIsInBsYXllcl9pZCI6MTAwMDEwMDAyMDAwMDAwMiwiZXhwIjoxNzg0MzA2OTEzfQ.YTQ1ZDI3OGQ1NTU1NDk3YWFmYWY4ZTBmOTZlMzBhNzRmNGZiOGVmZmIzNjVhNjU4ODY1MGNjOWE1YzQwOTg4Ng\",\n}\n ');
INSERT INTO `error_log` VALUES ('05-0001-00000001-6a5aecc8-000000', 5, 1, 1784343752, '[:00000034][20260718 11:02:32 32][error][05-0001-00000034-6a5aecc8-000002][room_game_login][../../commonlualib/common/plug/login_plug.lua:74]\"token err \" {\n	[\"player_id\"] = 1000100020000001,\n	[\"token\"] = \"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYmYiOjE3ODQzNDM3NTEsImlzcyI6ImhhbGxzZXJ2ZXI6MiIsInBsYXllcl9pZCI6MTAwMDEwMDAyMDAwMDAwMSwiZXhwIjoxNzg0MzQ3MzUxfQ.Mzg2YjhjM2ViMWQwZGE3NWY4OWY4Yjg3NmE0ZjhiZDI3OTQzYjdhOTQzOTJhZWY2OTYyODJiN2Q0YjM1N2NmZA\",\n}\n ');
INSERT INTO `error_log` VALUES ('05-0001-00000001-6a5aed1f-000000', 5, 1, 1784343839, '[:00000034][20260718 11:03:59 64][error][05-0001-00000034-6a5aed1f-000002][room_game_login][../../commonlualib/common/plug/login_plug.lua:74]\"token err \" {\n	[\"player_id\"] = 1000100020000001,\n	[\"token\"] = \"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYmYiOjE3ODQzNDM4MzksImlzcyI6ImhhbGxzZXJ2ZXI6MiIsInBsYXllcl9pZCI6MTAwMDEwMDAyMDAwMDAwMSwiZXhwIjoxNzg0MzQ3NDM5fQ.MWVkZWIxOGI4YzRhZWZhNDk0MGE0YzEwYTg2Njc2NmE4YTRmZTJkY2Y1M2ExMjdlZDk0MDIyNTY5ZDFmZDQyYw\",\n}\n ');
INSERT INTO `error_log` VALUES ('05-0001-00000001-6a5af74e-000000', 5, 1, 1784346446, '[:00000034][20260718 11:47:26 03][error][05-0001-00000034-6a5af74e-000000][room_game_login][../../commonlualib/common/plug/login_plug.lua:74]\"token err \" {\n	[\"player_id\"] = 1000100020000005,\n	[\"token\"] = \"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYmYiOjE3ODQzNDY0NDUsImlzcyI6ImhhbGxzZXJ2ZXI6MiIsInBsYXllcl9pZCI6MTAwMDEwMDAyMDAwMDAwNSwiZXhwIjoxNzg0MzUwMDQ1fQ.ODY0MGNiZmI3ZGM3MTRlNmRmMTgwMzJlZTUwMzNkMmJjZWViNjE5ZTRlNzk2NDYzNDMwNmQ0Nzc3ZjA2ZDhlOQ\",\n}\n ');
INSERT INTO `error_log` VALUES ('05-0001-00000001-6a5af75b-000000', 5, 1, 1784346459, '[:00000034][20260718 11:47:39 06][error][05-0001-00000034-6a5af75b-000000][room_game_login][../../commonlualib/common/plug/login_plug.lua:74]\"token err \" {\n	[\"player_id\"] = 1000100020000005,\n	[\"token\"] = \"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYmYiOjE3ODQzNDY0NDUsImlzcyI6ImhhbGxzZXJ2ZXI6MiIsInBsYXllcl9pZCI6MTAwMDEwMDAyMDAwMDAwNSwiZXhwIjoxNzg0MzUwMDQ1fQ.ODY0MGNiZmI3ZGM3MTRlNmRmMTgwMzJlZTUwMzNkMmJjZWViNjE5ZTRlNzk2NDYzNDMwNmQ0Nzc3ZjA2ZDhlOQ\",\n}\n ');
INSERT INTO `error_log` VALUES ('05-0001-00000001-6a5af75b-000001', 5, 1, 1784346459, '[:00000034][20260718 11:47:39 09][error][05-0001-00000034-6a5af75b-000001][room_game_login][../../commonlualib/common/plug/login_plug.lua:74]\"token err \" {\n	[\"player_id\"] = 1000100020000005,\n	[\"token\"] = \"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYmYiOjE3ODQzNDY0NDUsImlzcyI6ImhhbGxzZXJ2ZXI6MiIsInBsYXllcl9pZCI6MTAwMDEwMDAyMDAwMDAwNSwiZXhwIjoxNzg0MzUwMDQ1fQ.ODY0MGNiZmI3ZGM3MTRlNmRmMTgwMzJlZTUwMzNkMmJjZWViNjE5ZTRlNzk2NDYzNDMwNmQ0Nzc3ZjA2ZDhlOQ\",\n}\n ');
INSERT INTO `error_log` VALUES ('05-0001-00000001-6a5b016d-000001', 5, 1, 1784349037, '[:00000034][20260718 12:30:37 92][error][05-0001-00000034-6a5b016d-000002][room_game_login][../../commonlualib/common/plug/login_plug.lua:74]\"token err \" {\n	[\"player_id\"] = 1000100020000006,\n	[\"token\"] = \"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYmYiOjE3ODQzNDkwMzcsImlzcyI6ImhhbGxzZXJ2ZXI6MiIsInBsYXllcl9pZCI6MTAwMDEwMDAyMDAwMDAwNiwiZXhwIjoxNzg0MzUyNjM3fQ.NzE2OWU5MGY1NzYyYWRhOTUzNzM2NWQ2YTFkZjIzYzUxNzgzMDUyYjJmYTMwNjYwZGE3N2Y0NDhhOTJmYjFmMQ\",\n}\n ');
INSERT INTO `error_log` VALUES ('05-0001-00000001-6a5b019b-000001', 5, 1, 1784349083, '[:00000034][20260718 12:31:23 97][error][05-0001-00000034-6a5b019b-000002][room_game_login][../../commonlualib/common/plug/login_plug.lua:74]\"token err \" {\n	[\"player_id\"] = 1000100020000006,\n	[\"token\"] = \"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYmYiOjE3ODQzNDkwMzcsImlzcyI6ImhhbGxzZXJ2ZXI6MiIsInBsYXllcl9pZCI6MTAwMDEwMDAyMDAwMDAwNiwiZXhwIjoxNzg0MzUyNjM3fQ.NzE2OWU5MGY1NzYyYWRhOTUzNzM2NWQ2YTFkZjIzYzUxNzgzMDUyYjJmYTMwNjYwZGE3N2Y0NDhhOTJmYjFmMQ\",\n}\n ');
INSERT INTO `error_log` VALUES ('05-0001-00000001-6a5b01a7-000000', 5, 1, 1784349095, '[:00000034][20260718 12:31:35 37][error][05-0001-00000034-6a5b01a7-000002][room_game_login][../../commonlualib/common/plug/login_plug.lua:74]\"token err \" {\n	[\"player_id\"] = 1000100020000006,\n	[\"token\"] = \"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYmYiOjE3ODQzNDkwMzcsImlzcyI6ImhhbGxzZXJ2ZXI6MiIsInBsYXllcl9pZCI6MTAwMDEwMDAyMDAwMDAwNiwiZXhwIjoxNzg0MzUyNjM3fQ.NzE2OWU5MGY1NzYyYWRhOTUzNzM2NWQ2YTFkZjIzYzUxNzgzMDUyYjJmYTMwNjYwZGE3N2Y0NDhhOTJmYjFmMQ\",\n}\n ');

-- ----------------------------
-- Table structure for gather_info
-- ----------------------------
DROP TABLE IF EXISTS `gather_info`;
CREATE TABLE `gather_info`  (
  `cluster_name` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL DEFAULT '',
  `file_name` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL DEFAULT '',
  `cur_date` int unsigned NOT NULL,
  `offset` int unsigned NOT NULL,
  `file_size` int unsigned NOT NULL,
  `linenum` int unsigned NOT NULL,
  PRIMARY KEY (`cluster_name`, `file_name`, `cur_date`) USING BTREE,
  INDEX `date_index`(`cur_date`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of gather_info
-- ----------------------------
INSERT INTO `gather_info` VALUES ('hallserver:1', 'error.log', 20260717, 596, 596, 1);
INSERT INTO `gather_info` VALUES ('hallserver:1', 'error.log', 20260718, 4768, 4768, 8);
INSERT INTO `gather_info` VALUES ('hallserver:1', 'user.log', 20260717, 1245, 1245, 3);
INSERT INTO `gather_info` VALUES ('hallserver:1', 'user.log', 20260718, 2075, 2075, 5);
INSERT INTO `gather_info` VALUES ('hallserver:2', 'user.log', 20260717, 830, 830, 2);
INSERT INTO `gather_info` VALUES ('hallserver:2', 'user.log', 20260718, 0, 0, 0);

-- ----------------------------
-- Table structure for item_change_log
-- ----------------------------
DROP TABLE IF EXISTS `item_change_log`;
CREATE TABLE `item_change_log`  (
  `player_id` bigint(0) NOT NULL DEFAULT 0,
  `item_id` bigint(0) NOT NULL DEFAULT 0,
  `change_num` bigint(0) NOT NULL DEFAULT 0,
  `cur_num` bigint(0) NOT NULL DEFAULT 0,
  `source` int unsigned NOT NULL,
  `_guid` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL DEFAULT '',
  `_time` int unsigned NOT NULL,
  `_svr_type` tinyint unsigned NOT NULL,
  `_svr_id` smallint unsigned NOT NULL,
  PRIMARY KEY (`_guid`) USING BTREE,
  INDEX `item_index`(`item_id`, `change_num`) USING BTREE,
  INDEX `source_index`(`source`) USING BTREE,
  INDEX `time_index`(`_time`) USING BTREE,
  INDEX `svr_index`(`_svr_type`, `_svr_id`) USING BTREE,
  INDEX `player_index`(`player_id`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci ROW_FORMAT = Dynamic;

SET FOREIGN_KEY_CHECKS = 1;
