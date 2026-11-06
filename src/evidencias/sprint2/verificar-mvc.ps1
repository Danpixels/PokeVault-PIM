$ErrorActionPreference='Stop'
$base='http://localhost:5187'
$raiz=(Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
Add-Type -Path (Join-Path $raiz 'src/PokeVault.Web/bin/Debug/net10.0/System.Security.Cryptography.ProtectedData.dll')
# Este roteiro só usa a demonstração identificada; nunca o arquivo da base de testes.
$origem=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes('.\SQLEXPRESS/PokeVault'))).Substring(0,12)
$path=Join-Path $raiz "src/PokeVault.Web/.local/credenciais-$origem.dpapi"
$bytes=[Security.Cryptography.ProtectedData]::Unprotect([IO.File]::ReadAllBytes($path),$null,[Security.Cryptography.DataProtectionScope]::CurrentUser)
$creds=[Text.Encoding]::UTF8.GetString($bytes)|ConvertFrom-Json
[Security.Cryptography.CryptographicOperations]::ZeroMemory($bytes)
$checks=[Collections.Generic.List[object]]::new()
function Check($nome,$ok,$detalhe=''){ $checks.Add([pscustomobject]@{verificacao=$nome;aprovada=[bool]$ok;detalhe=$detalhe}); if(!$ok){throw "Falhou: $nome ($detalhe)"} }
function Request($ctx,$method,$rota,$data=$null,$expected=200){
    $params=@{Uri=($base+$rota);Method=$method;SkipHttpErrorCheck=$true}
    if($ctx){$params.WebSession=$ctx.s;$params.Headers=$ctx.h}
    if($null -ne $data){$params.ContentType='application/json';$params.Body=$data|ConvertTo-Json -Depth 10}
    $r=Invoke-WebRequest @params
    Check "$method $rota" ($r.StatusCode -eq $expected) "HTTP $($r.StatusCode), esperado $expected"
    if($r.Content){$str=if($r.Content -is [byte[]]){[Text.Encoding]::UTF8.GetString($r.Content)}else{$r.Content}; if($r.Headers['Content-Type'] -match 'json'){return $str|ConvertFrom-Json}; return $str}
}
function Login($email,$senha){
    $s=[Microsoft.PowerShell.Commands.WebRequestSession]::new();$t=Invoke-RestMethod "$base/api/v1/antiforgery" -WebSession $s
    $ctx=@{s=$s;h=@{'X-CSRF-TOKEN'=$t.token}}
    Request $ctx Post '/api/v1/sessao' @{email=$email;senha=$senha}|Out-Null
    $ctx.h=@{'X-CSRF-TOKEN'=(Invoke-RestMethod "$base/api/v1/antiforgery" -WebSession $s).token};return $ctx
}

function Form($ctx,$rota,$body,$expected=200){
    $r=Invoke-WebRequest ($base+$rota) -Method Post -WebSession $ctx.s -Headers $ctx.h -Body $body -SkipHttpErrorCheck
    Check "POST MVC $rota" ($r.StatusCode -eq $expected) "HTTP $($r.StatusCode), esperado $expected"
    return $r
}
try {
    $cred=$creds|Where-Object Perfil -eq 'ADMINISTRADOR';$admin=Login $cred.Email $cred.Senha
    $fixture=Get-Content (Join-Path $PSScriptRoot 'resultado-desenvolvimento.json') -Raw|ConvertFrom-Json
    $cliente=Request $admin Get "/api/v1/admin/clientes/$($fixture.clienteId)"
    Request $admin Get "/admin/clientes/$($fixture.clienteId)/saldo"|Out-Null
    Form $admin "/admin/clientes/$($fixture.clienteId)/saldo" @{NovoSaldo='1.23';Motivo='Verificação de formulário MVC S2';VersaoEsperada=$cliente.versao}|Out-Null
    $novo=Request $admin Get "/api/v1/admin/clientes/$($fixture.clienteId)"
    Check 'MVC saldo decimal correto' ($novo.saldo -eq 1.23)
    $antigo=Form $admin "/admin/clientes/$($fixture.clienteId)/saldo" @{NovoSaldo='4.00';Motivo='Confirmação desatualizada MVC';VersaoEsperada=$cliente.versao} 409
    Check 'MVC saldo antigo exige recarga e desabilita envio' ($antigo.Content.Contains('Recarregar saldo e versão') -and $antigo.Content.Contains('disabled'))
    Form $admin "/admin/clientes/$($fixture.clienteId)/saldo" @{NovoSaldo='0.00';Motivo='Encerramento verificação de formulário MVC S2';VersaoEsperada=$novo.versao}|Out-Null
    $stamp=[guid]::NewGuid().ToString('N').Substring(0,12);$senha=[Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(24));$email="s2-mvc-$stamp@pokevault.local"
    $model=@{Nome='Atendente sintético verificação MVC S2';Email=$email;SenhaInicial=$senha}
    $r=Form $admin '/admin/funcionarios/novo' $model
    Check 'MVC cadastro redireciona para listagem sem senha' (!$r.Content.Contains($senha) -and $r.Content.Contains($email))
    $r=Form $admin '/admin/funcionarios/novo' $model 409
    Check 'MVC erro de duplicidade limpa senha inicial' (!$r.Content.Contains($senha))
    $funcionarios=Request $admin Get '/api/v1/admin/funcionarios'
    $id=($funcionarios|Where-Object Email -eq $email).usuarioId
    Form $admin "/admin/funcionarios/$id/ativo" @{Ativo='false'}|Out-Null
    $co=Request $admin Get '/api/v1/gestao/ofertas/colecoes'
    $numero="M2-$stamp"
    Form $admin '/gestao/ofertas/nova' @{ColecaoId=$co[0].colecaoId;Numero=$numero;Nome='Carta sintética verificação MVC S2';Raridade='Demonstração';Conservacao='DAMAGED';Preco='1.25';QuantidadeInicial='0'}|Out-Null
    $cartas=Request $admin Get '/api/v1/gestao/ofertas'
    $carta=$cartas|Where-Object Numero -eq $numero
    Check 'MVC cadastro de carta decimal correto' ($carta.preco -eq 1.25 -and $carta.estoque -eq 0)
    Form $admin "/gestao/ofertas/$($carta.ofertaId)/ajustar" @{Preco='1.75';NovoEstoque='0';Ativo='false';Motivo='Encerramento da verificação MVC S2';VersaoEsperada=$carta.versao}|Out-Null
    $cartaFinal=Request $admin Get "/api/v1/gestao/ofertas/$($carta.ofertaId)"
    Check 'MVC ajuste de carta preserva identidade e inativa' (!$cartaFinal.ativo -and $cartaFinal.preco -eq 1.75 -and $cartaFinal.numero -eq $numero)
} finally {
    [pscustomobject]@{dataUtc=[datetime]::UtcNow.ToString('o');base='PokeVault';atendenteId=$id;ofertaId=$carta.ofertaId;checks=$checks}|ConvertTo-Json -Depth 8|Set-Content (Join-Path $PSScriptRoot 'resultado-mvc.json') -Encoding utf8
    $checks|Format-Table -AutoSize
}
