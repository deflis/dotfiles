-- nyaosのgit向けluaコマンド
-- print('loading... "_nya_git.lua"')
-- $git submodule rm 
-- サブモジュールの削除
-- http://labs.timedia.co.jp/2011/03/git-removing-a-submodule.html
nyaos.command['git submodule rm'] = function(name)
    local modulepath = nyaos.eval('git config --file .gitmodules --get submodule.' .. name .. '.path');
    nyaos.exec('git rm --cached ' .. modulepath);
    nyaos.exec('git config --file .gitmodules --remove-section submodule.' .. name);
    nyaos.exec('git commit -m "Remove submodule '..name..'"')
end

-- コマンド補完
-- MIT License (http://d.hatena.ne.jp/wantora/20101212/1292141801)
-- add git commands by @azu_re
function ArrayUnique(arr)
    local key = {};
    local ret = {};
    table.foreachi(
        arr,
        function (k,v)
            if type(key[v]) == [[nil]] then
                key[v] = v;
                table.insert(ret,v);
            end
        end
    );
    return ret;
end
local completes_cache = {}
local completes = {
    hg = function()
        local cmds = {}
        local function insert_cmd(str)
            for line in (str:match('\nlist of commands:\n\n(.-)\n\n') or ''):gmatch('[^\n]+') do
                local name = line:match('^ ([^%s:,]+)')
                if name then table.insert(cmds, name) end
            end
        end
        local HGPLAIN = os.getenv('HGPLAIN') or ''
        nyaos.exec('set HGPLAIN=1')
        
        h = nyaos.eval('hg help -v')
        insert_cmd(h)
        for line in (h:match('\nenabled extensions:\n\n(.-)\n\n') or ''):gmatch('[^\n]+') do
            insert_cmd(nyaos.eval('hg help '..line:match('^ ([^%s]*)')..' -v'))
        end
        
        nyaos.exec('set HGPLAIN='..HGPLAIN)
        return cmds
    end,
    gem = function()
        local cmds = {}
        for line in nyaos.eval('gem help commands'):gmatch('[^\n]+') do
            local name = line:match('^    ([^%s]*)')
            if #cmds > 0 and (not name) then break end
            if name and #name > 0         then table.insert(cmds, name) end
        end
        return cmds
    end,
    git = function()
        local cmds = {}
        -- git help all
        for line in nyaos.eval('git help -a'):gmatch('[^\n]+') do
            if line:match('^%s') then 
                for name in line:gmatch('%s+([a-z\-]*)') do
                    if #cmds > 0 and (not name) then break end
                    if name and #name > 1 then
                        -- print(name)
                        table.insert(cmds, name)
                    end
                end
            end
        end
        -- git help
        for line in nyaos.eval('git help'):gmatch('[^\n]+') do
            local name = line:match('^   ([^%s]*)')
            if name and #name > 0 then
                -- print(name)
                table.insert(cmds, name)
            end
        end
        --[[補完候補一覧
        for i=1, #ccmd, 1 do
            print(i .. " " ..  ccmd[i])
        end
        --]]
        return ArrayUnique(cmds)
    end,
}

function nyaos.complete(basestring, pos, misc)
    local cmd = misc.text:sub(1, pos):gsub('%s+$', ''):gsub('^(%S+)%.[^/\\.]+$', '%1')
    
    for name, comp in pairs(completes) do
        if cmd == name then
            if not completes_cache[name] then completes_cache[name] = comp() end
            return completes_cache[name]
        end
    end
    
    return nyaos.default_complete(basestring, pos)
end
