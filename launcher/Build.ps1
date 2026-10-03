$ErrorActionPreference = 'Stop'
$csc = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (!(Test-Path $csc)) { throw '.NET Framework 4.x Compiler fehlt.' }
& $csc /nologo /target:winexe /platform:x64 /optimize+ /r:System.Windows.Forms.dll /r:System.Drawing.dll /out:"$PSScriptRoot\WeddingWitchLauncher.exe" "$PSScriptRoot\Launcher.cs"
if ($LASTEXITCODE -ne 0) { throw 'Launcher-Build fehlgeschlagen.' }
$test = Start-Process "$PSScriptRoot\WeddingWitchLauncher.exe" -ArgumentList '--self-test' -WindowStyle Hidden -Wait -PassThru
if ($test.ExitCode -ne 0) { throw 'Launcher-Selbsttest fehlgeschlagen.' }
Write-Output 'LAUNCHER_BUILD_AND_SELF_TEST_OK'
