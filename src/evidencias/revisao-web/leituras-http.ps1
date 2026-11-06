$ErrorActionPreference='Stop'
$raiz=(Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
$base='http://localhost:5187'
$checks=[Collections.Generic.List[object]]::new()
function Get-Check($rota,$sessao=$null,$esperado=200){
 $args=@{Uri=($base+$rota);SkipHttpErrorCheck=$true};if($sessao){$args.WebSession=$sessao}
 $r=Invoke-WebRequest @args
 $checks.Add([pscustomobject]@{rota=$rota;status=[int]$r.StatusCode;esperado=$esperado;aprovado=($r.StatusCode -eq $esperado)})
 if($r.StatusCode -ne $esperado){throw "Resposta inesperada em $rota"};return $r
}
try {
 foreach($rota in @('/health/ready','/','/entrar','/api/v1/ofertas','/?nome=zz_revisao_sem_resultado_20261103')){Get-Check $rota|Out-Null}
 Get-Check '/api/v1/carrinho' $null 401|Out-Null
 Get-Check '/api/v1/ofertas?pagina=0' $null 400|Out-Null
 Add-Type -Path (Join-Path $raiz 'src/PokeVault.Web/bin/Debug/net10.0/System.Security.Cryptography.ProtectedData.dll')
 $arquivo=Join-Path $raiz 'src/PokeVault.Web/.local/credenciais-D4E6EF8BED95.dpapi'
 $bytes=[Security.Cryptography.ProtectedData]::Unprotect([IO.File]::ReadAllBytes($arquivo),$null,[Security.Cryptography.DataProtectionScope]::CurrentUser)
 $credenciais=[Text.Encoding]::UTF8.GetString($bytes)|ConvertFrom-Json
 [Security.Cryptography.CryptographicOperations]::ZeroMemory($bytes)
 foreach($perfil in @('ADMINISTRADOR','ATENDENTE','CLIENTE')){
  $c=$credenciais|Where-Object Perfil -eq $perfil|Select-Object -First 1
  $s=[Microsoft.PowerShell.Commands.WebRequestSession]::new()
  $token=Invoke-RestMethod "$base/api/v1/antiforgery" -WebSession $s
  $r=Invoke-WebRequest "$base/api/v1/sessao" -Method Post -WebSession $s -Headers @{'X-CSRF-TOKEN'=$token.token} -ContentType 'application/json' -Body (@{email=$c.Email;senha=$c.Senha}|ConvertTo-Json) -SkipHttpErrorCheck
  $checks.Add([pscustomobject]@{rota='Login de leitura '+$perfil;status=[int]$r.StatusCode;esperado=200;aprovado=($r.StatusCode -eq 200)})
  if($r.StatusCode -ne 200){throw "Conta de demonstração não autenticou; nenhuma senha foi alterada: $perfil"}
  $rotas=switch($perfil){'ADMINISTRADOR'{@('/admin/clientes','/admin/clientes/novo','/admin/funcionarios','/admin/funcionarios/novo','/gestao/ofertas','/gestao/ofertas/nova','/compras')};'ATENDENTE'{@('/gestao/ofertas','/gestao/ofertas/nova')};'CLIENTE'{@('/carrinho','/compras')}}
  foreach($rota in $rotas){Get-Check $rota $s|Out-Null}
 }
} finally {
 [pscustomobject]@{dataUtc=[datetime]::UtcNow.ToString('o');escopo='Leituras e login, sem mudanças em contas, saldo, estoque, carrinho ou compras';base='PokeVault';checks=$checks}|ConvertTo-Json -Depth 5|Set-Content (Join-Path $PSScriptRoot 'leituras-http-resultado.json') -Encoding utf8
 $checks|Format-Table -AutoSize
}
