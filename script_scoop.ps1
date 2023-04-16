# scoop 関連のアップデートの更新

scoop update
scoop update *
scoop export | ConvertFrom-Json | Sort-Object name | ConvertTo-Json -Depth 100 | Set-Content ./scoop.json
