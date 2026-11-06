$ErrorActionPreference='Stop'
$raiz=(Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
Add-Type -Path (Join-Path $raiz 'src/PokeVault.Web/bin/Debug/net10.0/System.Security.Cryptography.ProtectedData.dll')
$path=Join-Path $raiz 'src/PokeVault.Web/.local/credenciais-D4E6EF8BED95.dpapi'
$bytes=[Security.Cryptography.ProtectedData]::Unprotect([IO.File]::ReadAllBytes($path),$null,[Security.Cryptography.DataProtectionScope]::CurrentUser)
$creds=[Text.Encoding]::UTF8.GetString($bytes)|ConvertFrom-Json
[Security.Cryptography.CryptographicOperations]::ZeroMemory($bytes)
$admin=$creds|Where-Object Perfil -eq 'ADMINISTRADOR'|Select-Object -First 1
$base='http://localhost:5187';$s=[Microsoft.PowerShell.Commands.WebRequestSession]::new()
$t=Invoke-RestMethod "$base/api/v1/antiforgery" -WebSession $s
Invoke-RestMethod "$base/api/v1/sessao" -Method Post -WebSession $s -Headers @{'X-CSRF-TOKEN'=$t.token} -ContentType 'application/json' -Body (@{email=$admin.Email;senha=$admin.Senha}|ConvertTo-Json)|Out-Null
$t=Invoke-RestMethod "$base/api/v1/antiforgery" -WebSession $s
$h=@{'X-CSRF-TOKEN'=$t.token}

$before=Get-Content (Join-Path $PSScriptRoot 'importacao-inicial.json') -Raw|ConvertFrom-Json
$r=Invoke-WebRequest "$base/api/v1/gestao/tcgdex151/atualizar" -Method Post -WebSession $s -Headers $h -ContentType 'application/json' -Body '{"forcarRede":false}' -SkipHttpErrorCheck
if($r.StatusCode -ne 202){throw 'Retomada não aceita'}
do{Start-Sleep -Seconds 1;$estado=Invoke-RestMethod "$base/api/v1/gestao/tcgdex151/status" -WebSession $s}while($estado.emExecucao)
[pscustomobject]@{dataUtc=[datetime]::UtcNow.ToString('o');mesmaImportacaoId=($before.importacaoId -eq $estado.importacaoId);resultado=$estado}|ConvertTo-Json -Depth 5|Tee-Object (Join-Path $PSScriptRoot 'importacao-offline.json')

