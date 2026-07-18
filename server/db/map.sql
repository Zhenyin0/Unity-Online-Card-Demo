/*
 Navicat Premium Data Transfer

 Source Server         : zzn
 Source Server Type    : MySQL
 Source Server Version : 80040
 Source Host           : localhost:3306
 Source Schema         : map

 Target Server Type    : MySQL
 Target Server Version : 80040
 File Encoding         : 65001

 Date: 18/07/2026 15:00:37
*/

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ----------------------------
-- Table structure for bag
-- ----------------------------
DROP TABLE IF EXISTS `bag`;
CREATE TABLE `bag`  (
  `player_id` bigint unsigned NOT NULL COMMENT '玩家ID',
  `id` bigint unsigned NOT NULL COMMENT '物品ID',
  `count` bigint unsigned NOT NULL COMMENT '物品数量',
  PRIMARY KEY (`player_id`, `id`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_unicode_ci COMMENT = '【地图服】玩家背包表' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for build_config
-- ----------------------------
DROP TABLE IF EXISTS `build_config`;
CREATE TABLE `build_config`  (
  `build_id` bigint unsigned NOT NULL COMMENT '建筑ID(1-6)',
  `build_name` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '建筑名称',
  `require_items` json NOT NULL COMMENT '建造消耗物品',
  `build_time` int unsigned NOT NULL COMMENT '建造耗时(秒)',
  `product_items` json NOT NULL COMMENT '生产物资配置',
  `product_time` int unsigned NOT NULL COMMENT '生产周期(秒)',
  PRIMARY KEY (`build_id`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_unicode_ci COMMENT = '【静态】建筑建造&生产配置表' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of build_config
-- ----------------------------
INSERT INTO `build_config` VALUES (1, '产木的茅草屋', '[{\"count\": 10, \"bag-item_id\": 10000003}]', 60, '[{\"count\": 1, \"bag-item_id\": 10000003}]', 60);
INSERT INTO `build_config` VALUES (2, '产石的平房', '[{\"count\": 5, \"bag-item_id\": 10000003}, {\"count\": 20, \"bag-item_id\": 10000004}]', 360, '[{\"count\": 3, \"bag-item_id\": 10000004}]', 120);
INSERT INTO `build_config` VALUES (3, '炼造金属的大鼎', '[{\"count\": 50, \"bag-item_id\": 10000005}, {\"count\": 20, \"bag-item_id\": 10000006}]', 360, '[{\"count\": 3, \"bag-item_id\": 10000005}]', 60);
INSERT INTO `build_config` VALUES (4, '兑换钱币的商户', '[{\"count\": 50, \"bag-item_id\": 10000003}, {\"count\": 20, \"bag-item_id\": 10000004}, {\"count\": 5, \"bag-item_id\": 10000005}, {\"count\": 200, \"bag-item_id\": 10000006}]', 360, '[{\"count\": 10, \"bag-item_id\": 10000006}]', 60);
INSERT INTO `build_config` VALUES (5, '守关的大佛', '[{\"count\": 200, \"bag-item_id\": 10000004}, {\"count\": 50, \"bag-item_id\": 10000005}, {\"count\": 300, \"bag-item_id\": 10000006}]', 360, '[{\"count\": 10, \"bag-item_id\": 10000007}]', 60);
INSERT INTO `build_config` VALUES (6, '设施被击破的废墟', '[]', 0, '[]', 0);

-- ----------------------------
-- Table structure for card_template
-- ----------------------------
DROP TABLE IF EXISTS `card_template`;
CREATE TABLE `card_template`  (
  `id` int unsigned NOT NULL COMMENT '卡牌唯一ID(1~87)',
  `name` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT '' COMMENT '卡牌名称',
  `effect` varchar(1024) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT '' COMMENT '效果描述',
  `category` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT '' COMMENT '卡牌类型',
  `mana` int unsigned NULL COMMENT '魔力消耗',
  `stamina` int unsigned NULL COMMENT '体力消耗',
  `is_noble` tinyint unsigned NOT NULL COMMENT '是否宝具：1是 0否',
  `is_all_round` tinyint unsigned NOT NULL COMMENT '是否全场：1是 0否',
  `max_count` int unsigned NOT NULL COMMENT '最大持有数',
  PRIMARY KEY (`id`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_unicode_ci COMMENT = '【静态】卡牌模板表' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of card_template
-- ----------------------------
INSERT INTO `card_template` VALUES (1, '炽天使咒印', '造成150伤害', '仪式', 3, NULL, 0, 0, 3);
INSERT INTO `card_template` VALUES (2, '十字磔咒印', '抵抗100伤害', '场地', 1, NULL, 0, 0, 3);
INSERT INTO `card_template` VALUES (3, '应源泉咒印', '回复200点生命值', '永续', 2, NULL, 0, 0, 3);
INSERT INTO `card_template` VALUES (4, '宝石剑泽尔里奇', '造成300点伤害', '反击', 3, NULL, 0, 0, 1);
INSERT INTO `card_template` VALUES (5, '凛的吊坠', '抵抗致命伤害.存续50', '装备', 0, NULL, 0, 1, 1);
INSERT INTO `card_template` VALUES (6, '红宝石ruby', '增幅伤害100点', '装备', 1, NULL, 0, 0, 1);
INSERT INTO `card_template` VALUES (7, '蓝宝石sapphire', '降低受伤150', '装备', 1, NULL, 0, 0, 1);
INSERT INTO `card_template` VALUES (8, '炽天覆七重圆环', '抵抗受伤 500', '仪式', 5, NULL, 1, 0, 1);
INSERT INTO `card_template` VALUES (9, '中心竞争者', '破坏敌方装备，并造成 100 伤害', '速攻', NULL, 1, 0, 0, 1);
INSERT INTO `card_template` VALUES (10, '黑键迎击', '无视对方防御造成 300 伤害', '速攻', NULL, 6, 0, 0, 2);
INSERT INTO `card_template` VALUES (11, '黑键蓄力', '受到伤害反伤对方 200', '反击', NULL, 6, 0, 0, 2);
INSERT INTO `card_template` VALUES (12, '西格玛的肖像', '【Lancer】仅仅是个替身而已（ATK/0 DEF/1000）', '灵摆', 6, NULL, 0, 0, 1);
INSERT INTO `card_template` VALUES (13, '同伴的惨死', '造成 250 点伤害', '仪式', NULL, 3, 0, 0, 3);
INSERT INTO `card_template` VALUES (14, '冷酷持枪', '造成 150 点伤害', '通常', NULL, 1, 0, 0, 7);
INSERT INTO `card_template` VALUES (15, '麻木世界的觉悟', '抵抗 300 点伤害', '永续', NULL, 3, 0, 0, 1);
INSERT INTO `card_template` VALUES (16, '记录于书籍的故事', '清除对方的灵摆或者是怪兽或者是替身', '装备', NULL, 9, 0, 0, 10);
INSERT INTO `card_template` VALUES (17, '命运转折的警示', '默认抵抗 100 伤害', '装备', NULL, 1, 0, 0, 1);
INSERT INTO `card_template` VALUES (18, '故事中的西格玛', '【Lancer】明显带着不祥的恶意（ATK/50 DEF/250）', '怪兽', 3, NULL, 0, 0, 10);
INSERT INTO `card_template` VALUES (19, '团队的讨论', '恢复 250 血', '仪式', NULL, 3, 1, 0, 10);
INSERT INTO `card_template` VALUES (20, '有所举措的行为', '造成 50 伤害，回复 100 血，抵抗伤害 50', '通常', NULL, 2, 0, 0, 7);
INSERT INTO `card_template` VALUES (21, '突然的袭击', '该回合不及时丢出，失去灵摆或者是怪兽或者是替身，或者是自我造成 200 伤害', '通常', NULL, 50, 0, 1, 1);
INSERT INTO `card_template` VALUES (22, '狂信徒', '追杀圣杯参与者的一位信使ATK/ 500 DEF/ 200', '怪兽', 5, NULL, 0, 0, 18);
INSERT INTO `card_template` VALUES (23, '妄想心音', '造成 500 点伤害', '速攻', 5, NULL, 1, 0, 1);
INSERT INTO `card_template` VALUES (24, '暴力的破窗突入', '造成 100 点伤害，抵抗 100 伤害', '场地', 4, NULL, 0, 0, 1);
INSERT INTO `card_template` VALUES (25, '幻想血统', '完整复制对方的加成', '仪式', 10, NULL, 1, 0, 1);
INSERT INTO `card_template` VALUES (26, '狂烈信徒的迥异', '抵抗 200 伤害', '通常', NULL, 2, 0, 0, 18);
INSERT INTO `card_template` VALUES (27, '空想电脑', '造成 500 点伤害', '速攻', 5, NULL, 0, 0, 1);
INSERT INTO `card_template` VALUES (28, '狂想闪影', '造成 50 点伤害，回复 200 点生命', '永续', 3, NULL, 0, 0, 1);
INSERT INTO `card_template` VALUES (29, '梦想髓液', '整体造成 100 伤害', '永续', 6, NULL, 0, 0, 1);
INSERT INTO `card_template` VALUES (30, '冥想神经', '抵抗 150 伤害，恢复 50 生命', '仪式', 2, NULL, 0, 0, 1);
INSERT INTO `card_template` VALUES (31, '妄想心音击中-即死', '造成 1000 伤害，清除自身所有提升', '速攻', 9, NULL, 1, 0, 1);
INSERT INTO `card_template` VALUES (32, '螺湮城教本', '抵抗 200 点伤害，解放双怪兽槽，并使本场，场上海魔抵抗一次致命攻击', '装备', 10, NULL, 0, 0, 1);
INSERT INTO `card_template` VALUES (33, '战局降临', '抵抗 200 点伤害，回复 100 点生命，切换战局位置，改为任意前方或后方', '场地', 5, NULL, 0, 0, 2);
INSERT INTO `card_template` VALUES (34, '迎击-跨越数百年的我', '每回合默认攻击 50 点伤害', '装备', 5, NULL, 0, 1, 2);
INSERT INTO `card_template` VALUES (35, '感到忧郁的两人', '抵抗 150 点伤害，将本回合的受到的伤害转移', '永续', 9, NULL, 0, 1, 2);
INSERT INTO `card_template` VALUES (36, '无垠的花海', '回复自身 500 点血量，并清除对方所有的加成', '场地', 10, NULL, 0, 0, 2);
INSERT INTO `card_template` VALUES (37, '污染的裂口海星', '【海魔】荆皮类型的海洋生物（ATK/ 100 DEF/ 100）', '怪兽', 2, NULL, 0, 0, 10);
INSERT INTO `card_template` VALUES (38, '涌现的污浊蛟龙', '【海魔】浑浊污水下翻涌的恶龙（ATK/ 500 DEF/ 700）', '怪兽', 13, NULL, 0, 0, 3);
INSERT INTO `card_template` VALUES (39, '怨秽骸骨', '【暴力】极度强烈的愤怒杀意（ATK/5000 DEF/7000）', '怪兽', 77, NULL, 0, 0, 1);
INSERT INTO `card_template` VALUES (40, '畸形深潜者-血肉之墙', '【暴力】不可直视之物，每回合回复 10% 的血（ATK/3000 DEF/5000）', '怪兽', 44, NULL, 0, 1, 1);
INSERT INTO `card_template` VALUES (41, '吞噬的畸胎', '【卵】不断地膨胀着，每回合回复 25% 的血（ATK/ 50 DEF/ 500）', '怪兽', 10, NULL, 0, 1, 7);
INSERT INTO `card_template` VALUES (42, '最古早的中西会晤', '抵抗 2000 点伤害，回复 500 点血，本回合内免受从者的伤害', '场地', NULL, 29, 0, 1, 2);
INSERT INTO `card_template` VALUES (43, '晨曦的万里长城', '默认每回合清除，低于 500 的来源伤害', '装备', NULL, 10, 0, 1, 1);
INSERT INTO `card_template` VALUES (44, '休闲乘风', '回复单个从者 100% 的血量', '永续', 27, NULL, 0, 0, 1);
INSERT INTO `card_template` VALUES (45, '锦绣江山', '抵抗 3000 点伤害', '永续', 30, NULL, 1, 0, 1);
INSERT INTO `card_template` VALUES (46, '兵马俑', '【人】秦始皇遗留下来的最后军队（ATK/ 150 DEF/ 500）', '灵摆', 5, NULL, 0, 0, 6);
INSERT INTO `card_template` VALUES (47, '半人马型项羽', '【人】是永世秦帝国仙术与神经机械学的结晶，生命达到 10% 时，每回合攻击两次（ATK/3000 DEF/5000）', '怪兽', 72, NULL, 0, 1, 1);
INSERT INTO `card_template` VALUES (48, '秦弩', '默认每回合增加 500 点伤害', '装备', NULL, 15, 0, 1, 1);
INSERT INTO `card_template` VALUES (49, '秦始皇陵墓', '抵抗 5000 点伤害', '场地', NULL, 50, 0, 0, 1);
INSERT INTO `card_template` VALUES (50, '秦始皇骑北极熊', '造成 2500 点伤害', '速攻', 25, NULL, 0, 0, 2);
INSERT INTO `card_template` VALUES (51, '秦始皇骑核弹', '造成伤害，使得敌方整体减少 50% 的生命', '速攻', 66, NULL, 0, 0, 2);
INSERT INTO `card_template` VALUES (52, '伟大作家的抖机灵', '抵抗 200 点伤害，恢复 150 点生命', '永续', NULL, 5, 0, 0, 7);
INSERT INTO `card_template` VALUES (53, '创作宝具', '造成 50 点伤害，从卡库抽取两张卡', '仪式', 5, NULL, 1, 0, 8);
INSERT INTO `card_template` VALUES (54, '轰击五星', '默认接下来五回合，每回合结束造成对方 500 点伤害', '速攻', 25, NULL, 1, 1, 5);
INSERT INTO `card_template` VALUES (55, '万物必将破戒', '清除对方在场的单个从者或者是，单个怪兽', '装备', 10, NULL, 1, 0, 1);
INSERT INTO `card_template` VALUES (56, '赝品-天地归离之星', '将对方的整体血量压制在 10% 之下', '装备', 100, NULL, 1, 0, 1);
INSERT INTO `card_template` VALUES (57, '穿刺之枪', '造成一千点伤害，无抵抗伤害时，必杀从者', '装备', 27, NULL, 1, 0, 1);
INSERT INTO `card_template` VALUES (58, '阿瓦隆剑鞘', '默认每回合 100% 反弹首次伤害', '装备', 18, NULL, 1, 1, 1);
INSERT INTO `card_template` VALUES (59, '爱因兹贝伦魔术礼装', '以最低 1 魔力打出，造成 20 点伤害，以本回合最高魔力打出，造成 2 的幂乘以 20 的伤害：第 9 魔力打出，造成 128*20=2560 伤害，默认以最高魔力打出', '装备', 9, NULL, 0, 1, 1);
INSERT INTO `card_template` VALUES (60, '释提桓因?金刚杵', '造成 1000 点伤害，或清除对方当前的所有伤害抵抗', '装备', 50, NULL, 1, 0, 1);
INSERT INTO `card_template` VALUES (61, '机关枪刃（Gunblade）', '造成三百点伤害，可由体力打出，也可以由魔力打出', '装备', 3, 3, 0, 0, 2);
INSERT INTO `card_template` VALUES (62, '仇招复制-射杀百头', '造成 9000 点伤害', '反击', 81, NULL, 1, 0, 1);
INSERT INTO `card_template` VALUES (63, '性感的亚马逊女王', '造成 3000 点伤害，并且使受击怪兽和灵摆该回合沉默无法攻击', '反击', 60, NULL, 0, 1, 3);
INSERT INTO `card_template` VALUES (64, '给予腰带的历史一刻', '抵抗 2000 点伤害，回复 500 点生命', '永续', 17, NULL, 0, 0, 2);
INSERT INTO `card_template` VALUES (65, '准确射击', '造成 500 点伤害', '速攻', NULL, 5, 0, 0, 11);
INSERT INTO `card_template` VALUES (66, '奋力投击', '造成 800 点伤害', '速攻', 8, NULL, 0, 0, 11);
INSERT INTO `card_template` VALUES (67, '腰带解放', '抵抗 3000 点伤害，减去 50% 宝具造成的伤害', '装备', 60, NULL, 1, 0, 1);
INSERT INTO `card_template` VALUES (68, '仰视警戒', '抵抗 500 点伤害，恢复 1000 点生命', '仪式', NULL, 15, 0, 0, 2);
INSERT INTO `card_template` VALUES (69, '崩塌脱险', '抵抗 500 点伤害，若是以最后一张手牌打出，马上从牌库里抽取两张手牌', '永续', NULL, 9, 0, 0, 2);
INSERT INTO `card_template` VALUES (70, '土著民族的出征', '造成 3000 点伤害，抵抗 3000 点伤害，回复 3000 点生命', '场地', NULL, 90, 0, 0, 1);
INSERT INTO `card_template` VALUES (71, '无法让人遗忘的12壮举', '将预消耗的体力转换成魔力', '仪式', NULL, 50, 0, 0, 5);
INSERT INTO `card_template` VALUES (72, '兽人泛血的巨斧', '造成 500 点伤害', '速攻', NULL, 5, 0, 0, 10);
INSERT INTO `card_template` VALUES (73, '兽人嗜血的嘴唇', '恢复 100 点生命', '永续', NULL, 3, 0, 0, 10);
INSERT INTO `card_template` VALUES (74, '兽人战锤迎击', '抵抗 1000 点伤害', '永续', NULL, 10, 0, 0, 10);
INSERT INTO `card_template` VALUES (75, '兽人的聚集', '增加两个怪兽槽，并召唤两个怪兽', '仪式', 10, NULL, 0, 0, 5);
INSERT INTO `card_template` VALUES (76, '燃起生命之火', '恢复 500 点生命', '仪式', 5, NULL, 0, 0, 10);
INSERT INTO `card_template` VALUES (77, '魔王的巨砸', '造成 2000 点伤害', '速攻', NULL, 20, 0, 0, 10);
INSERT INTO `card_template` VALUES (78, '全身上甲', '默认每回合抵抗 500 点伤害', '装备', NULL, 5, 0, 1, 10);
INSERT INTO `card_template` VALUES (79, '对峙', '当前回合下消除伤害小于 300 的伤害', '场地', NULL, 8, 0, 1, 10);
INSERT INTO `card_template` VALUES (80, '骷髅的挥棒', '造成 100 点伤害', '速攻', NULL, 1, 0, 0, 10);
INSERT INTO `card_template` VALUES (81, '骷髅的抵抗', '抵抗 50 点伤害', '永续', NULL, 0, 0, 0, 10);
INSERT INTO `card_template` VALUES (82, '骷髅的冷静', '恢复 50 点生命', '永续', NULL, 1, 0, 0, 10);
INSERT INTO `card_template` VALUES (83, '亡灵之夜', '恢复场地上的所有怪兽', '场地', 33, NULL, 0, 0, 5);
INSERT INTO `card_template` VALUES (84, '巨物的吞噬', '造成 5000 点伤害', '速攻', NULL, 17, 0, 0, 10);
INSERT INTO `card_template` VALUES (85, '巨物的咆哮', '抵抗 2000 点伤害', '速攻', NULL, 13, 0, 0, 10);
INSERT INTO `card_template` VALUES (86, '巨物的饮水', '恢复 500 点生命', '永续', NULL, 1, 0, 0, 10);
INSERT INTO `card_template` VALUES (87, '突变武装', '默认每回合造成 500 伤害，抵抗 500 点伤害，恢复 500 点生命值', '装备', NULL, 25, 0, 1, 10);

-- ----------------------------
-- Table structure for char_template
-- ----------------------------
DROP TABLE IF EXISTS `char_template`;
CREATE TABLE `char_template`  (
  `id` int unsigned NOT NULL COMMENT '角色ID(1~19)',
  `name` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT '' COMMENT '角色名称',
  `star` tinyint unsigned NOT NULL COMMENT '星级',
  `hp` int unsigned NOT NULL COMMENT '生命值',
  `atk` int unsigned NOT NULL COMMENT '攻击力',
  `mana` int unsigned NOT NULL COMMENT '魔力上限',
  `stamina` int unsigned NULL COMMENT '体力',
  `race` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT '' COMMENT '种族',
  `own_card_ids` json NULL COMMENT '角色自带/专属卡牌ID数组',
  PRIMARY KEY (`id`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_unicode_ci COMMENT = '【静态】角色模板表' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of char_template
-- ----------------------------
INSERT INTO `char_template` VALUES (1, '咕哒子', 5, 500, 50, 10, 50, '人', '[1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11]');
INSERT INTO `char_template` VALUES (2, '西格玛', 3, 500, 200, 10, NULL, '人', '[12, 13, 14, 15, 16, 17, 18, 19, 20, 21]');
INSERT INTO `char_template` VALUES (3, '狂信子', 4, 1500, 300, 10, NULL, '人', '[22, 23, 24, 25, 26, 27, 28, 29, 30, 31]');
INSERT INTO `char_template` VALUES (4, '弗朗索瓦', 4, 1000, 25, 10, NULL, '人', '[32, 33, 34, 35, 36, 37, 38, 39, 40, 41]');
INSERT INTO `char_template` VALUES (5, '秦始皇', 5, 10000, 2000, 10, NULL, '人', '[42, 43, 44, 45, 46, 47, 48, 49, 50, 51]');
INSERT INTO `char_template` VALUES (6, '大仲马', 4, 500, 50, 10, NULL, '人', '[52, 53, 54, 55, 56, 57, 58, 59, 60, 61]');
INSERT INTO `char_template` VALUES (7, '希伯吕忒', 4, 2000, 500, 10, NULL, '人', '[62, 63, 64, 65, 66, 67, 68, 69, 70, 71]');
INSERT INTO `char_template` VALUES (8, '红骷髅', 1, 300, 50, 10, NULL, '骷髅', '[80, 81, 82, 83]');
INSERT INTO `char_template` VALUES (9, '锥瓶骷髅', 2, 500, 100, 10, NULL, '骷髅', '[80, 81, 82, 83]');
INSERT INTO `char_template` VALUES (10, '骷髅老板', 3, 600, 150, 10, 50, '骷髅', '[80, 81, 82, 83]');
INSERT INTO `char_template` VALUES (11, '西红柿人', 2, 600, 50, 10, NULL, '兽人', '[72, 73, 74, 75]');
INSERT INTO `char_template` VALUES (12, '黄鳝人', 2, 900, 150, 10, 100, '兽人', '[72, 73, 74, 75]');
INSERT INTO `char_template` VALUES (13, '鸽子人', 2, 700, 100, 10, NULL, '兽人', '[72, 73, 74, 75]');
INSERT INTO `char_template` VALUES (14, '岩铠蟹', 3, 2500, 300, 10, NULL, '魔兽', '[76, 77, 78, 79]');
INSERT INTO `char_template` VALUES (15, '异形', 3, 2500, 700, 10, 150, '魔兽', '[76, 77, 78, 79]');
INSERT INTO `char_template` VALUES (16, '蛟龙', 3, 1500, 500, 10, NULL, '魔兽', '[76, 77, 78, 79]');
INSERT INTO `char_template` VALUES (17, '血肉狗', 4, 3500, 100, 50, NULL, '怪兽', '[84, 85, 86, 87]');
INSERT INTO `char_template` VALUES (18, '血液', 4, 2500, 1000, 50, NULL, '怪兽', '[84, 85, 86, 87]');
INSERT INTO `char_template` VALUES (19, '巨大的淫秽物', 5, 10000, 5000, 50, 200, '怪兽', '[84, 85, 86, 87]');
INSERT INTO `char_template` VALUES (20, '召唤种', 5, 0, 0, 0, 0, '召唤种', '[72, 73, 74, 75, 76, 77, 78, 79, 80, 81, 82, 83, 84, 85, 86, 87]');

-- ----------------------------
-- Table structure for game_map_config
-- ----------------------------
DROP TABLE IF EXISTS `game_map_config`;
CREATE TABLE `game_map_config`  (
  `map_id` bigint unsigned NOT NULL COMMENT '地图ID(1/2/3/4)',
  `map_name` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '地图名称',
  `region_ids` json NOT NULL COMMENT '包含区域ID数组',
  `default_open` tinyint(1) NOT NULL DEFAULT 0 COMMENT '默认开放：1是 0否',
  `sort` int unsigned NOT NULL COMMENT '显示排序',
  PRIMARY KEY (`map_id`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_unicode_ci COMMENT = '【静态】游戏地图配置表' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of game_map_config
-- ----------------------------
INSERT INTO `game_map_config` VALUES (1, '炎上污染的都市', '[1, 2, 3, 4, 5, 6, 7, 8]', 1, 1);
INSERT INTO `game_map_config` VALUES (2, '邪龟百年战争', '[9, 10, 11, 12, 13, 14, 15, 16, 17, 18]', 0, 2);
INSERT INTO `game_map_config` VALUES (3, '永续疯狂帝国', '[19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29]', 0, 3);
INSERT INTO `game_map_config` VALUES (4, '封锁终局四海', '[30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40]', 0, 4);

-- ----------------------------
-- Table structure for map_region_config
-- ----------------------------
DROP TABLE IF EXISTS `map_region_config`;
CREATE TABLE `map_region_config`  (
  `region_id` bigint unsigned NOT NULL COMMENT '区域ID(1-40)',
  `map_id` bigint unsigned NOT NULL COMMENT '所属地图ID',
  `status` tinyint unsigned NULL COMMENT '状态：0锁定 1解锁',
  `region_name` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '区域名称',
  `enemy_config` json NOT NULL COMMENT '敌军配置',
  `reward_config` json NOT NULL COMMENT '通关奖励配置',
  `sort` int unsigned NOT NULL COMMENT '区域排序',
  PRIMARY KEY (`region_id`) USING BTREE,
  INDEX `idx_map_id`(`map_id`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_unicode_ci COMMENT = '【静态】地图区域配置表' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of map_region_config
-- ----------------------------
INSERT INTO `map_region_config` VALUES (1, 1, 0, '冬木港口塔', '[{\"card\": [{\"card_id\": 80, \"card_count\": 10}, {\"card_id\": 82, \"card_count\": 10}], \"count\": 2, \"enemy_id\": 8}]', '{\"count\": 10, \"bag-item_id\": 10000003}', 1);
INSERT INTO `map_region_config` VALUES (2, 1, 0, '冬木港口塔', '[{\"card\": [{\"card_id\": 80, \"card_count\": 10}, {\"card_id\": 82, \"card_count\": 10}], \"count\": 2, \"enemy_id\": 8}]', '{\"count\": 10, \"bag-item_id\": 10000003}', 2);
INSERT INTO `map_region_config` VALUES (3, 1, 0, '冬木洞穴', '[{\"card\": [{\"card_id\": 80, \"card_count\": 10}, {\"card_id\": 82, \"card_count\": 10}], \"count\": 2, \"enemy_id\": 8}]', '{\"count\": 10, \"bag-item_id\": 10000003}', 3);
INSERT INTO `map_region_config` VALUES (4, 1, 0, '冬木大桥', '[{\"card\": [{\"card_id\": 80, \"card_count\": 10}, {\"card_id\": 82, \"card_count\": 10}], \"count\": 2, \"enemy_id\": 8}]', '{\"count\": 10, \"bag-item_id\": 10000003}', 4);
INSERT INTO `map_region_config` VALUES (5, 1, 0, '冬木居民区', '[{\"card\": [{\"card_id\": 80, \"card_count\": 10}, {\"card_id\": 82, \"card_count\": 10}], \"count\": 2, \"enemy_id\": 8}]', '{\"count\": 10, \"bag-item_id\": 10000003}', 5);
INSERT INTO `map_region_config` VALUES (6, 1, 0, '冬木豪宅', '[{\"card\": [{\"card_id\": 80, \"card_count\": 10}, {\"card_id\": 82, \"card_count\": 10}], \"count\": 2, \"enemy_id\": 8}]', '{\"count\": 10, \"bag-item_id\": 10000003}', 6);
INSERT INTO `map_region_config` VALUES (7, 1, 0, '冬木广场', '[{\"card\": [{\"card_id\": 80, \"card_count\": 10}, {\"card_id\": 82, \"card_count\": 10}], \"count\": 2, \"enemy_id\": 8}]', '{\"count\": 10, \"bag-item_id\": 10000003}', 7);
INSERT INTO `map_region_config` VALUES (8, 1, 0, '冬木宅邸', '[{\"card\": [{\"card_id\": 80, \"card_count\": 10}, {\"card_id\": 82, \"card_count\": 10}], \"count\": 2, \"enemy_id\": 8}]', '{\"count\": 10, \"bag-item_id\": 10000003}', 8);
INSERT INTO `map_region_config` VALUES (9, 2, 0, '巴黎', '[{\"card\": [{\"card_id\": 80, \"card_count\": 10}, {\"card_id\": 82, \"card_count\": 10}], \"count\": 2, \"enemy_id\": 8}]', '{\"count\": 10, \"bag-item_id\": 10000003}', 9);
INSERT INTO `map_region_config` VALUES (10, 2, 0, '沃库勒尔', '[{\"card\": [{\"card_id\": 80, \"card_count\": 10}, {\"card_id\": 82, \"card_count\": 10}], \"count\": 2, \"enemy_id\": 8}]', '{\"count\": 10, \"bag-item_id\": 10000003}', 10);
INSERT INTO `map_region_config` VALUES (11, 2, 0, '栋雷米（贞德的故乡）', '[{\"card\": [{\"card_id\": 80, \"card_count\": 10}, {\"card_id\": 82, \"card_count\": 10}], \"count\": 2, \"enemy_id\": 8}]', '{\"count\": 10, \"bag-item_id\": 10000003}', 11);
INSERT INTO `map_region_config` VALUES (12, 2, 0, '奥尔良', '[{\"card\": [{\"card_id\": 80, \"card_count\": 10}, {\"card_id\": 82, \"card_count\": 10}], \"count\": 2, \"enemy_id\": 8}]', '{\"count\": 10, \"bag-item_id\": 10000003}', 12);
INSERT INTO `map_region_config` VALUES (13, 2, 0, '拉沙里泰', '[{\"card\": [{\"card_id\": 80, \"card_count\": 10}, {\"card_id\": 82, \"card_count\": 10}], \"count\": 2, \"enemy_id\": 8}]', '{\"count\": 10, \"bag-item_id\": 10000003}', 13);
INSERT INTO `map_region_config` VALUES (14, 2, 0, '汝拉（地区 / 山脉）', '[{\"card\": [{\"card_id\": 80, \"card_count\": 10}, {\"card_id\": 82, \"card_count\": 10}], \"count\": 2, \"enemy_id\": 8}]', '{\"count\": 10, \"bag-item_id\": 10000003}', 14);
INSERT INTO `map_region_config` VALUES (15, 2, 0, '第戎', '[{\"card\": [{\"card_id\": 80, \"card_count\": 10}, {\"card_id\": 82, \"card_count\": 10}], \"count\": 2, \"enemy_id\": 8}]', '{\"count\": 10, \"bag-item_id\": 10000003}', 15);
INSERT INTO `map_region_config` VALUES (16, 2, 0, '里昂', '[{\"card\": [{\"card_id\": 80, \"card_count\": 10}, {\"card_id\": 82, \"card_count\": 10}], \"count\": 2, \"enemy_id\": 8}]', '{\"count\": 10, \"bag-item_id\": 10000003}', 16);
INSERT INTO `map_region_config` VALUES (17, 2, 0, '波尔多', '[{\"card\": [{\"card_id\": 80, \"card_count\": 10}, {\"card_id\": 82, \"card_count\": 10}], \"count\": 2, \"enemy_id\": 8}]', '{\"count\": 10, \"bag-item_id\": 10000003}', 17);
INSERT INTO `map_region_config` VALUES (18, 2, 0, '马赛', '[{\"card\": [{\"card_id\": 80, \"card_count\": 10}, {\"card_id\": 82, \"card_count\": 10}], \"count\": 2, \"enemy_id\": 8}]', '{\"count\": 10, \"bag-item_id\": 10000003}', 18);
INSERT INTO `map_region_config` VALUES (19, 3, 0, '不列颠', '[{\"card\": [{\"card_id\": 80, \"card_count\": 10}, {\"card_id\": 82, \"card_count\": 10}], \"count\": 2, \"enemy_id\": 8}]', '{\"count\": 10, \"bag-item_id\": 10000003}', 19);
INSERT INTO `map_region_config` VALUES (20, 3, 0, '高卢', '[{\"card\": [{\"card_id\": 80, \"card_count\": 10}, {\"card_id\": 82, \"card_count\": 10}], \"count\": 2, \"enemy_id\": 8}]', '{\"count\": 10, \"bag-item_id\": 10000003}', 20);
INSERT INTO `map_region_config` VALUES (21, 3, 0, '日耳曼', '[{\"card\": [{\"card_id\": 80, \"card_count\": 10}, {\"card_id\": 82, \"card_count\": 10}], \"count\": 2, \"enemy_id\": 8}]', '{\"count\": 10, \"bag-item_id\": 10000003}', 21);
INSERT INTO `map_region_config` VALUES (22, 3, 0, '梅迪奥兰', '[{\"card\": [{\"card_id\": 80, \"card_count\": 10}, {\"card_id\": 82, \"card_count\": 10}], \"count\": 2, \"enemy_id\": 8}]', '{\"count\": 10, \"bag-item_id\": 10000003}', 22);
INSERT INTO `map_region_config` VALUES (23, 3, 0, '马萨利亚', '[{\"card\": [{\"card_id\": 80, \"card_count\": 10}, {\"card_id\": 82, \"card_count\": 10}], \"count\": 2, \"enemy_id\": 8}]', '{\"count\": 10, \"bag-item_id\": 10000003}', 23);
INSERT INTO `map_region_config` VALUES (24, 3, 0, '弗洛伦提亚', '[{\"card\": [{\"card_id\": 80, \"card_count\": 10}, {\"card_id\": 82, \"card_count\": 10}], \"count\": 2, \"enemy_id\": 8}]', '{\"count\": 10, \"bag-item_id\": 10000003}', 24);
INSERT INTO `map_region_config` VALUES (25, 3, 0, '联邦首都', '[{\"card\": [{\"card_id\": 80, \"card_count\": 10}, {\"card_id\": 82, \"card_count\": 10}], \"count\": 2, \"enemy_id\": 8}]', '{\"count\": 10, \"bag-item_id\": 10000003}', 25);
INSERT INTO `map_region_config` VALUES (26, 3, 0, '有形之岛', '[{\"card\": [{\"card_id\": 80, \"card_count\": 10}, {\"card_id\": 82, \"card_count\": 10}], \"count\": 2, \"enemy_id\": 8}]', '{\"count\": 10, \"bag-item_id\": 10000003}', 26);
INSERT INTO `map_region_config` VALUES (27, 3, 0, '罗马', '[{\"card\": [{\"card_id\": 80, \"card_count\": 10}, {\"card_id\": 82, \"card_count\": 10}], \"count\": 2, \"enemy_id\": 8}]', '{\"count\": 10, \"bag-item_id\": 10000003}', 27);
INSERT INTO `map_region_config` VALUES (28, 3, 0, '阿皮亚大道', '[{\"card\": [{\"card_id\": 80, \"card_count\": 10}, {\"card_id\": 82, \"card_count\": 10}], \"count\": 2, \"enemy_id\": 8}]', '{\"count\": 10, \"bag-item_id\": 10000003}', 28);
INSERT INTO `map_region_config` VALUES (29, 3, 0, '埃特纳火山', '[{\"card\": [{\"card_id\": 80, \"card_count\": 10}, {\"card_id\": 82, \"card_count\": 10}], \"count\": 2, \"enemy_id\": 8}]', '{\"count\": 10, \"bag-item_id\": 10000003}', 29);
INSERT INTO `map_region_config` VALUES (30, 4, 0, '火山口之岛', '[{\"card\": [{\"card_id\": 80, \"card_count\": 10}, {\"card_id\": 82, \"card_count\": 10}], \"count\": 2, \"enemy_id\": 8}]', '{\"count\": 10, \"bag-item_id\": 10000003}', 30);
INSERT INTO `map_region_config` VALUES (31, 4, 0, '翼龙之岛', '[{\"card\": [{\"card_id\": 80, \"card_count\": 10}, {\"card_id\": 82, \"card_count\": 10}], \"count\": 2, \"enemy_id\": 8}]', '{\"count\": 10, \"bag-item_id\": 10000003}', 31);
INSERT INTO `map_region_config` VALUES (32, 4, 0, '洋流交汇之海', '[{\"card\": [{\"card_id\": 80, \"card_count\": 10}, {\"card_id\": 82, \"card_count\": 10}], \"count\": 2, \"enemy_id\": 8}]', '{\"count\": 10, \"bag-item_id\": 10000003}', 32);
INSERT INTO `map_region_config` VALUES (33, 4, 0, '风暴海域', '[{\"card\": [{\"card_id\": 80, \"card_count\": 10}, {\"card_id\": 82, \"card_count\": 10}], \"count\": 2, \"enemy_id\": 8}]', '{\"count\": 10, \"bag-item_id\": 10000003}', 33);
INSERT INTO `map_region_config` VALUES (34, 4, 0, '群岛', '[{\"card\": [{\"card_id\": 80, \"card_count\": 10}, {\"card_id\": 82, \"card_count\": 10}], \"count\": 2, \"enemy_id\": 8}]', '{\"count\": 10, \"bag-item_id\": 10000003}', 34);
INSERT INTO `map_region_config` VALUES (35, 4, 0, '丰饶之海', '[{\"card\": [{\"card_id\": 80, \"card_count\": 10}, {\"card_id\": 82, \"card_count\": 10}], \"count\": 2, \"enemy_id\": 8}]', '{\"count\": 10, \"bag-item_id\": 10000003}', 35);
INSERT INTO `map_region_config` VALUES (36, 4, 0, '地图上记载的岛屿', '[{\"card\": [{\"card_id\": 80, \"card_count\": 10}, {\"card_id\": 82, \"card_count\": 10}], \"count\": 2, \"enemy_id\": 8}]', '{\"count\": 10, \"bag-item_id\": 10000003}', 36);
INSERT INTO `map_region_config` VALUES (37, 4, 0, '暗礁海域', '[{\"card\": [{\"card_id\": 80, \"card_count\": 10}, {\"card_id\": 82, \"card_count\": 10}], \"count\": 2, \"enemy_id\": 8}]', '{\"count\": 10, \"bag-item_id\": 10000003}', 37);
INSERT INTO `map_region_config` VALUES (38, 4, 0, '君王居住之岛', '[{\"card\": [{\"card_id\": 80, \"card_count\": 10}, {\"card_id\": 82, \"card_count\": 10}], \"count\": 2, \"enemy_id\": 8}]', '{\"count\": 10, \"bag-item_id\": 10000003}', 38);
INSERT INTO `map_region_config` VALUES (39, 4, 0, '海盗岛', '[{\"card\": [{\"card_id\": 80, \"card_count\": 10}, {\"card_id\": 82, \"card_count\": 10}], \"count\": 2, \"enemy_id\": 8}]', '{\"count\": 10, \"bag-item_id\": 10000003}', 39);
INSERT INTO `map_region_config` VALUES (40, 4, 0, '海盗船', '[{\"card\": [{\"card_id\": 80, \"card_count\": 10}, {\"card_id\": 82, \"card_count\": 10}], \"count\": 2, \"enemy_id\": 8}]', '{\"count\": 10, \"bag-item_id\": 10000003}', 40);

-- ----------------------------
-- Table structure for player_card
-- ----------------------------
DROP TABLE IF EXISTS `player_card`;
CREATE TABLE `player_card`  (
  `player_id` bigint unsigned NOT NULL COMMENT '玩家ID',
  `card_id` int unsigned NOT NULL COMMENT '卡牌ID',
  `count` int unsigned NOT NULL COMMENT '持有数量',
  PRIMARY KEY (`player_id`, `card_id`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_unicode_ci COMMENT = '【地图服】玩家卡牌表' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for player_char
-- ----------------------------
DROP TABLE IF EXISTS `player_char`;
CREATE TABLE `player_char`  (
  `player_id` bigint unsigned NOT NULL COMMENT '玩家ID',
  `char_id` int unsigned NOT NULL COMMENT '角色ID',
  `unlocked` tinyint unsigned NOT NULL COMMENT '是否解锁：1是 0否',
  PRIMARY KEY (`player_id`, `char_id`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_unicode_ci COMMENT = '【地图服】玩家角色解锁表' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for player_char_togame
-- ----------------------------
DROP TABLE IF EXISTS `player_char_togame`;
CREATE TABLE `player_char_togame`  (
  `id` bigint unsigned NOT NULL COMMENT '主键',
  `player_id` bigint unsigned NOT NULL COMMENT '玩家ID',
  `room_id` bigint unsigned NOT NULL COMMENT '房间ID',
  `char_id` int unsigned NOT NULL COMMENT '出战角色ID',
  `carry_card_max` int unsigned NOT NULL COMMENT '最大带牌数',
  `is_current` tinyint unsigned NOT NULL COMMENT '当前出战：1是 0否',
  `create_time` bigint unsigned NOT NULL COMMENT '创建时间戳',
  `update_time` bigint unsigned NOT NULL COMMENT '更新时间戳',
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE INDEX `uk_player_current`(`player_id`, `is_current`) USING BTREE,
  INDEX `idx_player_char`(`player_id`, `char_id`) USING BTREE,
  INDEX `idx_room_id`(`room_id`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_unicode_ci COMMENT = '【地图服】玩家出战配置表' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for player_char_togame_card
-- ----------------------------
DROP TABLE IF EXISTS `player_char_togame_card`;
CREATE TABLE `player_char_togame_card`  (
  `id` bigint unsigned NOT NULL COMMENT '主键',
  `player_id` bigint unsigned NOT NULL COMMENT '玩家ID',
  `char_id` int unsigned NOT NULL COMMENT '出战角色ID',
  `card_id` int unsigned NOT NULL COMMENT '手牌ID',
  `card_count` int unsigned NOT NULL COMMENT '带入数量',
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE INDEX `uk_player_char_card`(`player_id`, `char_id`, `card_id`) USING BTREE,
  INDEX `idx_player_char`(`player_id`, `char_id`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_unicode_ci COMMENT = '【地图服】出战手牌明细表' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for player_map
-- ----------------------------
DROP TABLE IF EXISTS `player_map`;
CREATE TABLE `player_map`  (
  `player_id` bigint unsigned NOT NULL COMMENT '玩家ID',
  `map_id` bigint unsigned NOT NULL COMMENT '地图ID',
  `is_open` tinyint(1) NOT NULL DEFAULT 0 COMMENT '是否开放：1是 0否',
  `update_time` bigint unsigned NOT NULL COMMENT '更新时间戳',
  PRIMARY KEY (`player_id`, `map_id`) USING BTREE,
  INDEX `idx_player_id`(`player_id`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_unicode_ci COMMENT = '【玩家】地图开放状态表' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of player_map
-- ----------------------------
INSERT INTO `player_map` VALUES (1777479993812, 1, 1, 1778478763);

-- ----------------------------
-- Table structure for player_region_build
-- ----------------------------
DROP TABLE IF EXISTS `player_region_build`;
CREATE TABLE `player_region_build`  (
  `player_id` bigint unsigned NOT NULL COMMENT '玩家ID',
  `region_id` bigint unsigned NOT NULL COMMENT '区域ID',
  `is_open` tinyint(1) NOT NULL DEFAULT 0 COMMENT '区域可通关：1是 0否',
  `is_jump` tinyint(1) NOT NULL DEFAULT 0 COMMENT '已通关可跳过：1是 0否',
  `clear_star` tinyint unsigned NULL COMMENT '通关星数(1-5)',
  `is_building` tinyint(1) NOT NULL DEFAULT 0 COMMENT '建筑状态：1建造中/已建成 0未建造',
  `build_id` bigint unsigned NULL COMMENT '建造建筑ID',
  `build_start_time` bigint unsigned NULL COMMENT '建造开始时间戳',
  `build_end_time` bigint unsigned NULL COMMENT '建造完成时间戳',
  `last_product_time` bigint unsigned NULL COMMENT '上次生产时间戳',
  `update_time` bigint unsigned NOT NULL COMMENT '更新时间戳',
  PRIMARY KEY (`player_id`, `region_id`) USING BTREE,
  INDEX `idx_player_id`(`player_id`) USING BTREE,
  INDEX `idx_region_id`(`region_id`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_unicode_ci COMMENT = '【玩家】区域进度+建筑状态表' ROW_FORMAT = Dynamic;

SET FOREIGN_KEY_CHECKS = 1;
