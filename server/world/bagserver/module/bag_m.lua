-- bag_m.lua
-- 背包模块入口

local skynet = require "skynet"
local orm_table_client = require "skynet-fly.client.orm_table_client"
local container_client = require "skynet-fly.client.container_client"
local log = require "skynet-fly.log"

container_client:register("share_config_m")

local g_host = nil
local CMD = {}

function CMD.get_host()
    return g_host
end

function CMD.start()
    skynet.fork(function()
        local confclient = container_client:new("share_config_m")
        local room_game_login = confclient:mod_call("query", "room_game_login")
        g_host = room_game_login.wsgateconf.host
    end)
    return true
end

function CMD.exit()
    return true
end

return CMD