# scoop 関連のアップデートの更新

scoop update
scoop update *

# Scoopの出力を安定した形にする
function SortJSON($json) {
  if($json -is [array]) {
    $newArray = @()
    foreach($item in $json | Sort-Object Name) {
      $newArray += SortJSON $item
    }
    return $newArray
  } elseif($json -is [hashtable]) {
    $ordered = [ordered]@{}
    foreach ($key in $json.Keys | Sort-Object) {
      $ordered[$key] = SortJSON $json[$key]
    }
    return $ordered
  } else {
    return $json
  }
}

$json = scoop export | ConvertFrom-Json -AsHashtable
SortJSON $json | ConvertTo-Json -Depth 100 | Set-Content ./scoop.json

