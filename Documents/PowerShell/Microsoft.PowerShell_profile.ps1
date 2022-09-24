# oh-my-posh --init --shell pwsh --config "$(scoop prefix oh-my-posh)/themes/night-owl.omp.json" | Invoke-Expression
oh-my-posh --init --shell pwsh --config "~/.config/powershell/theme.yaml" | Invoke-Expression
Enable-PoshTransientPrompt
Import-Module posh-git
$env:POSH_GIT_ENABLED = $true

# タブ補完のやり方を変更
Set-PSReadLineKeyHandler -Key Tab -Function Complete

$env:EDITOR = "code --wait"
$env:FZF_DEFAULT_OPTS = "--height 40% --layout=reverse"

# 今すぐalias登録すべきPowerShellワンライナー
# https://qiita.com/mu_sette/items/3954759daee8ae9ad26f
function CustomListChildItems { Get-ChildItem $args[0] -force | Sort-Object -Property @{ Expression = 'LastWriteTime'; Descending = $true }, @{ Expression = 'Name'; Ascending = $true } | Format-Table -AutoSize -Property Mode, Length, LastWriteTime, Name }
Set-Alias ll CustomListChildItems

Invoke-Expression (& {
    $hook = if ($PSVersionTable.PSVersion.Major -lt 6) { 'prompt' } else { 'pwd' }
    (zoxide init --hook $hook powershell | Out-String)
})

Invoke-Expression (& {
    $hook = if ($PSVersionTable.PSVersion.Major -lt 6) { 'prompt' } else { 'pwd' }
    (zoxide init --hook $hook powershell --cmd cd | Out-String)
})
