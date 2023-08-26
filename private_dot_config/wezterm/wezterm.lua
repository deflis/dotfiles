-- Pull in the wezterm API
local wezterm = require 'wezterm'

-- This table will hold the configuration.
local config = {}

-- In newer versions of wezterm, use the config_builder which will
-- help provide clearer error messages
if wezterm.config_builder then
  config = wezterm.config_builder()
end


config.launch_menu = {}

if wezterm.target_triple == 'x86_64-pc-windows-msvc' then
  config.default_prog = { 'pwsh' }
end
config.keys = {
  { key = 'l', mods = 'ALT', action = wezterm.action.ShowLauncher },
}

config.color_scheme = 'OneHalfDark'
config.window_background_opacity = 0.8
config.window_frame = {
  -- The font used in the tab bar.
  -- Roboto Bold is the default; this font is bundled
  -- with wezterm.
  -- Whatever font is selected here, it will have the
  -- main font setting appended to it to pick up any
  -- fallback fonts you may have used there.
  font = wezterm.font { family = 'HackGen Console', weight = 'Bold' },

  -- The size of the font in the tab bar.
  -- Default to 10. on Windows but 12.0 on other systems
  font_size = 12.0,

  -- The overall background color of the tab bar when
  -- the window is focused
  active_titlebar_bg = '#333333',

  -- The overall background color of the tab bar when
  -- the window is not focused
  inactive_titlebar_bg = '#333333',
}
config.window_decorations = "RESIZE"
config.font = wezterm.font_with_fallback {
  'HackGen Console NF',
  'HackGen Console',
  'JetBrains Mono',
  'Consolas',
  'SF Mono',
  'Noto Color Emoji',
}

-- and finally, return the configuration to wezterm
return config
