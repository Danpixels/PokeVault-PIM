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
$r=Invoke-WebRequest "$base/api/v1/gestao/tcgdex151/atualizar" -Method Post -WebSession $s -Headers $h -ContentType 'application/json' -Body '{"forcarRede":false}' -SkipHttpErrorCheck
[pscustomobject]@{dataUtc=[datetime]::UtcNow.ToString('o');http=[int]$r.StatusCode;acao='Importar207referencias reais sv03.5 sem ofertas automaticamente'}|ConvertTo-Json|Tee-Object (Join-Path $PSScriptRoot 'importacao-disparo.json')
if($r.StatusCode -ne 202){throw 'Importação não aceita'}
$r2=Invoke-WebRequest "$base/api/v1/gestao/tcgdex151/atualizar" -Method Post -WebSession $s -Headers $h -ContentType 'application/json' -Body '{"forcarRede":false}' -SkipHttpErrorCheck
[pscustomobject]@{segundaChamadaHttp=[int]$r2.StatusCode;esperado=409}|ConvertTo-Json|Set-Content (Join-Path $PSScriptRoot 'reentrancia.json') -Encoding utf8
$limite=[datetime]::UtcNow.AddMinutes(8)
do {
 $estado=Invoke-RestMethod "$base/api/v1/gestao/tcgdex151/status" -WebSession $s
 $estado|ConvertTo-Json -Compress|Add-Content (Join-Path $PSScriptRoot 'progresso.jsonl') -Encoding utf8
 Write-Output ("Processados={0}; importados={1}; imagens={2}; ausentes={3}; executando={4}" -f $estado.processados,$estado.importados,$estado.imagensLocais,$estado.ausentes,$estado.emExecucao)
 if(!$estado.emExecucao){break}
 Start-Sleep -Seconds 5
}while([datetime]::UtcNow -lt $limite)
$estado|ConvertTo-Json|Tee-Object (Join-Path $PSScriptRoot 'importacao-resultado.json')
