@set args=%*
@pwsh -Command "iex((@('')*3+(cat '%~f0'|select -skip 3))-join[char]10)"
@exit /b %ERRORLEVEL%

$chezmoi_config = "$Env:USERPROFILE\.config\windows_terminal\settings.json"
$system_config = "$Env:LOCALAPPDATA\Packages\Microsoft.WindowsTerminal_8wekyb3d8bbwe\LocalState\settings.json"

$data1 = Get-Content $system_config -Raw | ConvertFrom-Json
$data2 = Get-Content $chezmoi_config -Raw | ConvertFrom-Json

function Join-Objects($source, $extend){
  if($source.GetType().Name -eq "PSCustomObject" -and $extend.GetType().Name -eq "PSCustomObject"){
    foreach($Property in $source | Get-Member -type NoteProperty, Property){
      if($extend.$($Property.Name) -eq $null){
        continue;
      }
      $source.$($Property.Name) = Join-Objects $source.$($Property.Name) $extend.$($Property.Name)
    }
  }else{
    $source = $extend;
  }
  return $source
}
function AddPropertyRecurse($source, $toExtend){
  if($source.GetType().Name -eq "PSCustomObject"){
    foreach($Property in $source | Get-Member -type NoteProperty, Property){
      if($toExtend.$($Property.Name) -eq $null){
        $toExtend | Add-Member -MemberType NoteProperty -Value $source.$($Property.Name) -Name $Property.Name `
      }
      else{
        $toExtend.$($Property.Name) = AddPropertyRecurse $source.$($Property.Name) $toExtend.$($Property.Name)
      }
    }
  }
  return $toExtend
}
function Json-Merge($source, $extend){
  $merged = Join-Objects $source $extend
  $extended = AddPropertyRecurse $source $merged
  return $extended
}

Json-Merge $data1 $data2 | ConvertTo-Json -Depth 100 | Out-File $system_config
