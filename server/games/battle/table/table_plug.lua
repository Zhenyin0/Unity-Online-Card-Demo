-- table_plug.lua
-- 房间插件（PVP 版本）

local skynet = require "skynet"
local pb_netpack = require "skynet-fly.netpack.pb_netpack"
local ws_pbnet_byrpc = require "skynet-fly.utils.net.ws_pbnet_byrpc"
local log = require "skynet-fly.log"
local module_cfg = require "skynet-fly.etc.module_info".get_cfg()
local pack_helper = require "common.pack_helper"
local table_logic = require "table.table_logic"

do
    pb_netpack.load('../../commonlualib/protos/common')
    pb_netpack.load('../../commonlualib/protos/battleserver')
    pack_helper.set_pack_id_names()
end

local PACK = pack_helper.PACK
local errors_msg = require "common.msg.errors_msg"
local rsp_msg = require "common.msg.rsp_msg"

local string = string
local assert = assert
local ipairs = ipairs
local table = table
local math = math
local pairs = pairs
local next = next
local os = os
local tonumber = tonumber
local tunpack = table.unpack

local M = {}
local g_interface_mgr = nil

function M.init(interface_mgr)
    g_interface_mgr = interface_mgr
    assert(module_cfg.table_conf.player_num, "not player_num")
end

M.ws_send = ws_pbnet_byrpc.send
M.ws_broadcast = ws_pbnet_byrpc.broadcast
M.rpc_pack = require "skynet-fly.utils.net.rpc_server"

function M.table_creator(table_id, table_name, play_type)
    local m_interface_mgr = g_interface_mgr:new(table_id)
    local m_errors_msg = errors_msg:new(m_interface_mgr)
    local m_rsp_msg = rsp_msg:new(m_interface_mgr)
    local m_logic = table_logic:new(table_id, m_interface_mgr, play_type)

    return {
        enter = function(player_id)
            return m_logic:enter(player_id)
        end,
        leave = function(player_id, reason)
            return m_logic:leave(player_id, reason)
        end,
        disconnect = function(player_id)
            return m_logic:disconnect(player_id)
        end,
        reconnect = function(player_id)
            m_logic:reconnect(player_id)
        end,
        handle = {
            [PACK.battleserver_room.GameStateReq] = function(player_id, pack_id, pack_body)
                return m_logic:game_state_req(player_id)
            end,
            [PACK.battleserver_room.MoveReq] = function(player_id, pack_id, pack_body)
                return m_logic:move_req(player_id, pack_body)
            end,
        },
        handle_end_rpc = function(player_id, pack_id, pack_body, rsp_session, handle_res)
            local ret, errcode, errmsg = tunpack(handle_res)
            if ret then
                if ret ~= true and rsp_session then
                    m_rsp_msg:rsp_msg(player_id, pack_id, ret, rsp_session)
                end
            else
                log.info("handle_end_rpc err >>> ", player_id, pack_id, ret, errcode, errmsg)
                m_errors_msg:errors(player_id, errcode, errmsg, pack_id, rsp_session)
            end
        end,
    }
end

return M