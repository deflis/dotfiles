@set args=%*
@pwsh -Command "iex((@('')*3+(cat '%~f0'|select -skip 3))-join[char]10)"
@exit /b %ERRORLEVEL%

# Scoop インストールしたりする initial script
# PowerShell 7 がインストールされてる前提

Function Test-CommandExists
{
  Param ($command)
  $oldPreference = $ErrorActionPreference
  $ErrorActionPreference = ‘stop’
  try {if(Get-Command $command){“$command exists”}}
  Catch {“$command does not exist”}
  Finally {$ErrorActionPreference=$oldPreference}
}

If(!(Test-CommandExists scoop)) {
  Invoke-Expression (New-Object System.Net.WebClient).DownloadString('https://get.scoop.sh')
}

If(!(Test-CommandExists oh-my-posh)) {
  scoop install https://github.com/JanDeDobbeleer/oh-my-posh/releases/latest/download/oh-my-posh.json
}
If(!(Test-CommandExists ghq)) {
  scoop install ghq
}
If(!(Test-CommandExists sudo)) {
  scoop install sudo
}
If(!(Test-CommandExists git)) {
  scoop install git
}

PowerShellGet\Install-Module posh-git -Scope CurrentUser
