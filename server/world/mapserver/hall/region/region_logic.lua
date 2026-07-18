-- region_logic.lua
-- 区域核心业务逻辑

local log = require "skynet-fly.log"
local orm_table_client = require "skynet-fly.client.orm_table_client"
local state_data = require "skynet-fly.hotfix.state_data"
local map_msg = require "msg.map_msg"
local time_util = require "skynet-fly.utils.time_util"
local errorcode = require "common.enum.errorcode"

local M = {}

local g_region_config_cli = orm_table_client:instance("map_region_config")
local g_player_region_cli = orm_table_client:instance("player_region_build")
local g_build_config_cli = orm_table_client:instance("build_config")

local g_local_info = state_data.alloc_table("g_local_info")

function M.init(interface_mgr)
    g_local_info.map_msg = map_msg:new(interface_mgr)
end

-- 获取玩家区域列表
function M.get_player_regions(player_id)
    return g_player_region_cli:get_entry(player_id)
end

-- 初始化玩家区域数据（进入地图时调用）
function M.init_player_regions(player_id, map_id)
    local all_regions = g_region_config_cli:get_all_entry()
    local now = time_util.time()
    for _, cfg in ipairs(all_regions) do
        if cfg.map_id == map_id and cfg.status == 1 then
            g_player_region_cli:create_one_entry({
                player_id = player_id,
                region_id = cfg.region_id,
                is_open = 1,
                is_jump = 0,
                clear_star = 0,
                is_building = 0,
                build_id = 0,
                build_start_time = 0,
                build_end_time = 0,
                last_product_time = 0,
                update_time = now,
            })
        end
    end
end

function M.on_login(player_id)
    -- 登录时不做初始化，由 map 模块触发
end

-- 选择区域
function M.do_select_region(player_id, pack_body)
    local region_id = pack_body.region_id or 0
    local entry = g_player_region_cli:get_one_entry(player_id, region_id)
    if not entry then
        return nil, errorcode.REQ_PARAM_ERR, "region not found"
    end
    return {
        region_id = entry:get("region_id"),
        is_open = entry:get("is_open"),
        is_jump = entry:get("is_jump"),
        clear_star = entry:get("clear_star"),
        is_building = entry:get("is_building"),
        build_id = entry:get("build_id") or 0,
    }
end

-- 修改建筑状态
function M.do_change_build(player_id, pack_body)
    local region_id = pack_body.region_id or 0
    local is_building = pack_body.is_building or 0
    local build_id = pack_body.build_id or 0

    local entry = g_player_region_cli:get_one_entry(player_id, region_id)
    if not entry then
        return nil, errorcode.REQ_PARAM_ERR, "region not found"
    end

    local now = time_util.time()
    entry:set("is_building", is_building)
    entry:set("build_id", build_id)
    if is_building == 1 and build_id > 0 then
        local build_cfg = g_build_config_cli:get_one_entry(build_id)
        if build_cfg then
            entry:set("build_start_time", now)
            entry:set("build_end_time", now + build_cfg:get("build_time"))
        end
    else
        entry:set("build_start_time", 0)
        entry:set("build_end_time", 0)
    end
    entry:set("update_time", now)

    return {
        region_id = region_id,
        is_open = entry:get("is_open"),
        is_jump = entry:get("is_jump"),
        clear_star = entry:get("clear_star"),
        is_building = entry:get("is_building"),
        build_id = entry:get("build_id") or 0,
    }
end

-- CMD: 获取玩家区域信息（供RPC调用）
function M.cmd_get_player_region(player_id, region_id)
    return g_player_region_cli:get_one_entry(player_id, region_id)
end

-- CMD: 通关区域
function M.cmd_clear_region(player_id, region_id, star)
    local entry = g_player_region_cli:get_one_entry(player_id, region_id)
    if not entry then
        return false
    end
    entry:set("is_open", 1)
    entry:set("is_jump", 1)
    entry:set("clear_star", star or 5)
    entry:set("update_time", time_util.time())
    return true
end

return M