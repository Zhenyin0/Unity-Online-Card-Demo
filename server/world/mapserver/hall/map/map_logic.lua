-- map_logic.lua
-- 地图核心业务逻辑

local log = require "skynet-fly.log"
local orm_table_client = require "skynet-fly.client.orm_table_client"
local state_data = require "skynet-fly.hotfix.state_data"
local map_msg = require "msg.map_msg"
local time_util = require "skynet-fly.utils.time_util"
local errorcode = require "common.enum.errorcode"

local M = {}

local g_map_config_cli = orm_table_client:instance("game_map_config")
local g_player_map_cli = orm_table_client:instance("player_map")

local g_local_info = state_data.alloc_table("g_local_info")

function M.init(interface_mgr)
    g_local_info.map_msg = map_msg:new(interface_mgr)
end

-- 获取玩家地图列表
function M.get_player_maps(player_id)
    return g_player_map_cli:get_entry(player_id)
end

-- 获取地图配置
function M.get_map_config(map_id)
    return g_map_config_cli:get_one_entry(map_id)
end

-- 进入地图（初始化玩家地图数据）
function M.on_enter_map(player_id)
    -- 检查是否已有地图数据
    local maps = g_player_map_cli:get_entry(player_id)
    if #maps > 0 then
        return true
    end

    -- 获取默认开放地图
    local all_maps = g_map_config_cli:get_all_entry()
    local now = time_util.time()
    for _, cfg in ipairs(all_maps) do
        if cfg.default_open == 1 then
            g_player_map_cli:create_one_entry({
                player_id = player_id,
                map_id = cfg.map_id,
                is_open = 1,
                update_time = now,
            })
        end
    end
    return true
end

function M.on_login(player_id)
    M.on_enter_map(player_id)
end

-- CMD: 获取玩家地图信息（供RPC调用）
function M.cmd_get_player_map(player_id)
    return g_player_map_cli:get_entry(player_id)
end

-- CMD: 开放新地图
function M.cmd_unlock_map(player_id, map_id)
    local entry = g_player_map_cli:get_one_entry(player_id, map_id)
    if entry then
        entry:set("is_open", 1)
        entry:set("update_time", time_util.time())
        return true
    end
    return g_player_map_cli:create_one_entry({
        player_id = player_id,
        map_id = map_id,
        is_open = 1,
        update_time = time_util.time(),
    })
end

return M