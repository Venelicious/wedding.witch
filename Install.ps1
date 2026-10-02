param([string]$GameDir, [string]$ArchipelagoDir, [switch]$InstallWorld)
$ErrorActionPreference = 'Stop'
if (Get-Process 'Wedding Witch' -ErrorAction SilentlyContinue) { throw 'Wedding Witch zuerst schließen. Fortschritt und Server bleiben erhalten.' }
if (!$GameDir) {
 $steam = (Get-ItemProperty 'HKCU:\Software\Valve\Steam' -ErrorAction SilentlyContinue).SteamPath
 $libraries = @($steam, 'C:\Program Files (x86)\Steam')
 if ($steam -and (Test-Path "$steam\steamapps\libraryfolders.vdf")) {
  $vdf = Get-Content "$steam\steamapps\libraryfolders.vdf" -Raw
  foreach ($m in [regex]::Matches($vdf, '"path"\s+"([^"]+)"')) { $libraries += $m.Groups[1].Value.Replace('\\','\') }
 }
 $GameDir = $libraries | Where-Object { $_ } | ForEach-Object { Join-Path $_ 'steamapps\common\Wedding Witch' } | Where-Object { Test-Path "$_\Wedding Witch.exe" } | Select-Object -First 1
 if (!$GameDir) { throw 'Spiel nicht gefunden. Install.ps1 -GameDir "D:\SteamLibrary\steamapps\common\Wedding Witch" ausführen.' }
}
if (!(Test-Path "$GameDir\Wedding Witch.exe")) { throw 'Kein Wedding-Witch-Spielordner.' }
foreach ($file in @('WeddingWitchLauncher.exe','plugins\WeddingWitchArchipelago.dll','plugins\Archipelago.MultiClient.Net.dll','plugins\Newtonsoft.Json.dll')) {
 if (!(Test-Path (Join-Path $PSScriptRoot $file))) { throw "Paket unvollständig: $file" }
}
$loader = Join-Path $GameDir 'BepInEx\core\BepInEx.dll'
if (Test-Path $loader) {
 $major = [Reflection.AssemblyName]::GetAssemblyName($loader).Version.Major
 if ($major -ne 5) { throw 'Vorhandenes BepInEx ist nicht Version 5. Keine Dateien verändert.' }
}
$backup = Join-Path $GameDir ('BepInEx\WeddingWitch-backups\'+(Get-Date -Format 'yyyyMMdd-HHmmss-fff'))
New-Item -ItemType Directory $backup -Force | Out-Null
foreach ($name in @('doorstop_config.ini','winhttp.dll','WeddingWitchLauncher.exe')) {
 if (Test-Path "$GameDir\$name") { Copy-Item "$GameDir\$name" "$backup\$name" }
}
if (Test-Path "$GameDir\BepInEx\WeddingWitchArchipelago") { Copy-Item "$GameDir\BepInEx\WeddingWitchArchipelago" "$backup\AP-profile" -Recurse }
if (!(Test-Path $loader)) {
 $zip = Join-Path $env:TEMP ('ww-bepinex-'+[guid]::NewGuid()+'.zip')
 $extract = $zip+'.dir'
 try {
  [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
  Invoke-WebRequest 'https://github.com/BepInEx/BepInEx/releases/download/v5.4.23.5/BepInEx_win_x64_5.4.23.5.zip' -OutFile $zip -UseBasicParsing
  if ((Get-FileHash $zip -Algorithm SHA256).Hash.ToLowerInvariant() -ne '82f9878551030f54657792c0740d9d51a09500eeae1fba21106b0c441e6732c4') { throw 'BepInEx-Prüfsumme stimmt nicht.' }
  Expand-Archive $zip $extract
  Get-ChildItem $extract | Copy-Item -Destination $GameDir -Recurse -Force
 } finally {
  if (Test-Path $zip) { Remove-Item $zip }
  if (Test-Path $extract) { Remove-Item $extract -Recurse }
 }
}
$doorstop = Join-Path $GameDir 'doorstop_config.ini'
if (!(Test-Path $doorstop)) { throw 'doorstop_config.ini fehlt. BepInEx 5 x64 vollständig installieren.' }
$text = Get-Content $doorstop -Raw
$text = [regex]::Replace($text, '(?im)^\s*enabled\s*=.*$', 'enabled = true')
$text = [regex]::Replace($text, '(?im)^\s*ignore_disable_switch\s*=.*$', 'ignore_disable_switch = false')
[IO.File]::WriteAllText($doorstop, $text, [Text.UTF8Encoding]::new($false))
$plugins = Join-Path $GameDir 'BepInEx\plugins'
if (Test-Path $plugins) {
 foreach ($file in Get-ChildItem $plugins -Filter '*WeddingWitch*.dll' -Recurse) {
  $relative = $file.FullName.Substring($plugins.Length).TrimStart('\')
  $target = Join-Path "$backup\plugins" $relative
  New-Item -ItemType Directory (Split-Path $target) -Force | Out-Null
  Move-Item $file.FullName $target
 }
}
$dest = Join-Path $plugins 'WeddingWitchCustom'
New-Item -ItemType Directory $dest -Force | Out-Null
Copy-Item "$PSScriptRoot\plugins\*.dll" $dest -Force
Copy-Item "$PSScriptRoot\WeddingWitchLauncher.exe" "$GameDir\WeddingWitchLauncher.exe" -Force
if ($InstallWorld -or $ArchipelagoDir) {
 if (!$ArchipelagoDir) { $ArchipelagoDir = 'C:\ProgramData\Archipelago' }
 if (!(Test-Path $ArchipelagoDir)) { throw 'Archipelago-Ordner fehlt. Mod installiert; APWorld kann separat kopiert werden.' }
 $worldDir = Join-Path $ArchipelagoDir 'custom_worlds'
 New-Item -ItemType Directory $worldDir -Force | Out-Null
 if (Test-Path "$worldDir\wedding_witch.apworld") { Copy-Item "$worldDir\wedding_witch.apworld" "$backup\previous-wedding_witch.apworld" }
 Copy-Item "$PSScriptRoot\wedding_witch.apworld" $worldDir -Force
}
Write-Host 'Wedding Witch Archipelago 0.3.5 installiert.'
Write-Host "Launcher: $GameDir\WeddingWitchLauncher.exe"
Write-Host "Backup: $backup"
Write-Host 'Original oder Archipelago im Launcher auswählen. AP-Verbindung im Spiel mit F8.'
