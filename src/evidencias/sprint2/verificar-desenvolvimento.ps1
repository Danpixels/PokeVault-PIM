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
$novoCliente=$null;$novoAtendente=$null;$novaCarta=$null
try {
    $adminCred=$creds|Where-Object Perfil -eq 'ADMINISTRADOR'
    $admin=Login $adminCred.Email $adminCred.Senha
    foreach($rota in @('/admin/clientes','/admin/clientes/novo','/admin/funcionarios','/admin/funcionarios/novo','/gestao/ofertas','/gestao/ofertas/nova')){Request $admin Get $rota|Out-Null}
    $lista=Request $admin Get '/api/v1/admin/clientes'
    Check 'Listagem sem propriedades de segredo' (!(($lista|ConvertTo-Json -Depth 8) -match 'senha|hash'))
    $stamp=[guid]::NewGuid().ToString('N').Substring(0,12)
    $senha=[Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(24))
    $clienteModel=@{nome='Verificação desenvolvimento S2 cliente';email="s2-cliente-$stamp@pokevault.local";senhaInicial=$senha}
    $novoCliente=Request $admin Post '/api/v1/admin/clientes' $clienteModel 201
    $cliente=Request $admin Get "/api/v1/admin/clientes/$($novoCliente.usuarioId)"
    Check 'Cliente novo tem saldo zero e versão de 8 bytes' ($cliente.saldo -eq 0 -and [Convert]::FromBase64String($cliente.versao).Length -eq 8)
    $sessaoCliente=Login $clienteModel.email $senha
    $carrinho=Request $sessaoCliente Get '/api/v1/carrinho'
    Check 'Cliente novo tem carrinho vazio' ($carrinho.itens.Count -eq 0 -and $carrinho.saldo -eq 0)
    $clienteModel.email=$clienteModel.email.ToUpperInvariant()
    Request $admin Post '/api/v1/admin/clientes' $clienteModel 409|Out-Null
    $sobreposto=@{nome='Não criar';email="nao-$stamp@pokevault.local";senhaInicial=$senha;perfil='ADMINISTRADOR'}
    Request $admin Post '/api/v1/admin/clientes' $sobreposto 400|Out-Null
    Request $sessaoCliente Get '/api/v1/admin/clientes' $null 403|Out-Null
    $saldo=@{novoSaldo=5.00;motivo='Verificação S2 saldo absoluto';versaoEsperada=$cliente.versao}
    $saldoAtual=Request $admin Put "/api/v1/admin/clientes/$($cliente.usuarioId)/saldo" $saldo
    Check 'Saldo absoluto definido' ($saldoAtual.saldo -eq 5)
    $conflito=Request $admin Put "/api/v1/admin/clientes/$($cliente.usuarioId)/saldo" $saldo 409
    Check 'Saldo antigo retorna conflito de versão' ($conflito.codigo -eq 'VERSAO_DESATUALIZADA')
    $saldo.novoSaldo=5.001;$saldo.versaoEsperada=$saldoAtual.versao
    Request $admin Put "/api/v1/admin/clientes/$($cliente.usuarioId)/saldo" $saldo 400|Out-Null
    $saldo.novoSaldo=0;$saldo.motivo='Encerramento da verificação S2 de saldo'
    Request $admin Put "/api/v1/admin/clientes/$($cliente.usuarioId)/saldo" $saldo|Out-Null
    Request $admin Patch "/api/v1/admin/funcionarios/$($cliente.usuarioId)" @{ativo=$false} 403|Out-Null
    $identidade=Request $admin Get '/api/v1/sessao'
    Request $admin Patch "/api/v1/admin/funcionarios/$($identidade.id)" @{ativo=$false} 403|Out-Null
    $funcionarioModel=@{nome='Verificação desenvolvimento S2 atendente';email="s2-atendente-$stamp@pokevault.local";senhaInicial=$senha}
    $novoAtendente=Request $admin Post '/api/v1/admin/funcionarios' $funcionarioModel 201
    $atendente=Login $funcionarioModel.email $senha
    Request $atendente Get '/api/v1/admin/clientes' $null 403|Out-Null
    $colecoes=Request $atendente Get '/api/v1/gestao/ofertas/colecoes'
    $cartaModel=@{colecaoId=$colecoes[0].colecaoId;numero="S2-$stamp";nome='Carta de verificação S2';raridade='Demonstração';conservacao='DAMAGED';preco=12.34;quantidadeInicial=2}
    $novaCarta=Request $atendente Post '/api/v1/gestao/ofertas' $cartaModel 201
    Request $atendente Post '/api/v1/gestao/ofertas' $cartaModel 409|Out-Null
    $carta=Request $atendente Get "/api/v1/gestao/ofertas/$($novaCarta.ofertaId)"
    $antes=$carta.versao
    $ajuste=@{preco=12.34;novoEstoque=2;ativo=$true;motivo='Verificação S2 sem alteração';versaoEsperada=$carta.versao}
    $igual=Request $atendente Patch "/api/v1/gestao/ofertas/$($carta.ofertaId)" $ajuste
    Check 'Ajuste sem alteração preserva versão' ($igual.versao -eq $antes)
    $ajuste.preco=13.00;$ajuste.novoEstoque=3
    $nova=Request $atendente Patch "/api/v1/gestao/ofertas/$($carta.ofertaId)" $ajuste
    Check 'Ajuste de preço/estoque atualiza versão' ($nova.estoque -eq 3 -and $nova.preco -eq 13 -and $nova.versao -ne $antes)
    Request $atendente Patch "/api/v1/gestao/ofertas/$($carta.ofertaId)" $ajuste 409|Out-Null
    $form=@{Preco='12.34';NovoEstoque='2';Ativo='true';Motivo='Formulário antigo';VersaoEsperada=$antes}
    $r=Invoke-WebRequest "$base/gestao/ofertas/$($carta.ofertaId)/ajustar" -Method Post -WebSession $atendente.s -Headers $atendente.h -Body $form -SkipHttpErrorCheck
    Check 'MVC desatualizado exige recarga' ($r.StatusCode -eq 409 -and $r.Content.Contains('Recarregar carta e versão') -and $r.Content.Contains('disabled'))
    $ajuste.versaoEsperada=$nova.versao;$ajuste.ativo=$false;$ajuste.motivo='Encerramento da verificação S2: oferta inativa'
    $inativa=Request $atendente Patch "/api/v1/gestao/ofertas/$($carta.ofertaId)" $ajuste
    $publico=Request $null Get '/api/v1/ofertas?nome=Carta%20de%20verifica'
    Check 'Oferta inativa fora do catálogo público' ($publico.itens.Count -eq 0)
    $gestao=Request $atendente Get '/api/v1/gestao/ofertas'
    Check 'Oferta inativa na gestão' (@($gestao|Where-Object ofertaId -eq $carta.ofertaId).Count -eq 1)
    Request $sessaoCliente Get '/api/v1/gestao/ofertas' $null 403|Out-Null
    $semCsrf=[Microsoft.PowerShell.Commands.WebRequestSession]::new();$semCsrf.Cookies=$admin.s.Cookies
    $r=Invoke-WebRequest "$base/api/v1/admin/funcionarios/$($novoAtendente.usuarioId)" -Method Patch -WebSession $semCsrf -ContentType 'application/json' -Body '{"ativo":false}' -SkipHttpErrorCheck
    Check 'Mutação administrativa sem CSRF recusada' ($r.StatusCode -eq 400)
    Request $admin Patch "/api/v1/admin/funcionarios/$($novoAtendente.usuarioId)" @{ativo=$false}|Out-Null
    Request $atendente Get '/api/v1/gestao/ofertas' $null 401|Out-Null
    Request $admin Patch "/api/v1/admin/funcionarios/$($novoAtendente.usuarioId)" @{ativo=$true}|Out-Null
    $reativado=Login $funcionarioModel.email $senha
    Request $reativado Get '/api/v1/gestao/ofertas'|Out-Null
    Request $admin Patch "/api/v1/admin/funcionarios/$($novoAtendente.usuarioId)" @{ativo=$false}|Out-Null
    Check 'Reativação permitiu novo login; conta sintética terminou inativa' $true
} finally {
    [pscustomobject]@{dataUtc=[datetime]::UtcNow.ToString('o');base='PokeVault';url=$base;clienteId=$novoCliente.usuarioId;atendenteId=$novoAtendente.usuarioId;ofertaId=$novaCarta.ofertaId;checks=$checks}|ConvertTo-Json -Depth 8|Set-Content (Join-Path $PSScriptRoot 'resultado-desenvolvimento.json') -Encoding utf8
    $checks|Format-Table -AutoSize
}

