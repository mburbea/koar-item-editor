local json = {}

local encode

local escape_char_map = {
  [ "\\" ] = "\\",
  [ "\"" ] = "\"",
  [ "\b" ] = "b",
  [ "\f" ] = "f",
  [ "\n" ] = "n",
  [ "\r" ] = "r",
  [ "\t" ] = "t",
}

local escape_char_map_inv = { [ "/" ] = "/" }
for k, v in pairs(escape_char_map) do
  escape_char_map_inv[v] = k
end

local function escape_char(c)
  return "\\" .. (escape_char_map[c] or string.format("u%04x", c:byte()))
end

local function encode_nil(val)
  return "null"
end

local function try_get_editbox_str(val)
  WINDOW.populate_edit_box(name_win.m_editbox, val)
  return WINDOW.get_editbox_text(name_win.m_editbox)
end

local function get_editbox_str(val)
  if name_win.m_editbox ~= nil then
    local status, str = pcall(try_get_editbox_str, val)
    if(status) then
      return str
    end
  end
  return nil
end

local function encode_ui64(val)
  if name_win.m_editbox == nil then
    return tostring(val)
  end
  local str = get_editbox_str(val)
  if(str ~= nil and str ~= '<Invalid Loc Key>' and str ~= '<Loc Key Not Set>') then
    return encode(str)
  else
    return 'null'
  end
end

local function encode_table(val)
  local res = {}

  if name_win.m_editbox ~= nil and rawget(val,'SL1') ~= nil and rawget(val,'SL2') ~= nil then
    local str = get_editbox_str(val)
    return encode(str)
  end

  if rawget(val, 1) ~= nil or next(val) == nil then
    -- Treat as array -- check keys are valid and it is not sparse
    local n = 0
    for k in pairs(val) do
      n = n + 1
    end
    -- Encode
    for i, v in ipairs(val) do
      table.insert(res, encode(v))
    end
    return "[" .. table.concat(res, ",") .. "]"

  else
    -- Treat as an object
    for k, v in pairs(val) do
      table.insert(res, encode(k) .. ":" .. encode(v))
    end
    return "{" .. table.concat(res, ",") .. "}"
  end
end

local function encode_string(val)
  return '"' .. val:gsub('[%z\1-\31\\"]', escape_char) .. '"'
end

local function encode_number(val)
  return tostring(val)
end

local type_func_map = {
  [ "nil"     ] = encode_nil,
  [ "table"   ] = encode_table,
  [ "string"  ] = encode_string,
  [ "number"  ] = encode_number,
  [ "boolean" ] = tostring,
  [ "ui64"    ] = encode_ui64,
  [ "userdata"] = function() return '"userdata"' end
}

encode = function(val, stack)
  local t = type(val)
  local f = type_func_map[t]
  if f then
    return f(val)
  end
  error("unexpected type '" .. t .. "'")
end

json['encode'] = encode
return json
