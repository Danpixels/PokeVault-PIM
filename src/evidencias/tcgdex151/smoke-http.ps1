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

$checks=[Collections.Generic.List[object]]::new()
function Check($nome,$ok){$checks.Add([pscustomobject]@{verificacao=$nome;aprovada=[bool]$ok});if(!$ok){throw "Falhou: $nome"}}
try{
 $ready=Invoke-RestMethod "$base/health/ready";Check 'Schema10 pronto' ($ready.schemaRequerido -eq 10)
 $lista=Invoke-RestMethod "$base/api/v1/gestao/tcgdex151" -WebSession $s;Check 'Biblioteca207 e página24' ($lista.total -eq 207 -and $lista.itens.Count -eq 24)
 $filtro=Invoke-RestMethod "$base/api/v1/gestao/tcgdex151?nome=025&raridade=Comum" -WebSession $s;Check 'Filtro número/raridade' ($filtro.itens.Count -eq 1 -and $filtro.itens[0].id -eq 'sv03.5-025')
 $ref=Invoke-RestMethod "$base/api/v1/gestao/tcgdex151/sv03.5-025" -WebSession $s;Check 'Referência detalhada pt com imagem local' ($ref.detalhada -and $ref.idioma -eq 'pt' -and $ref.imagemLocal.StartsWith('/cache/'))
 $imagem=Invoke-WebRequest ($base+$ref.imagemLocal);Check 'Imagem cache HTTP200 webp' ($imagem.StatusCode -eq 200 -and $imagem.Headers['Content-Type'] -match 'image/webp')
 $html=Invoke-WebRequest "$base/gestao/tcgdex151/sv03.5-025" -WebSession $s;Check 'Detalhe mostra Cardmarket/EUR TCGplayer/USD e variante real' ($html.StatusCode -eq 200 -and $html.Content.Contains('Cardmarket') -and $html.Content.Contains('EUR') -and $html.Content.Contains('TCGplayer') -and $html.Content.Contains('USD') -and $html.Content.Contains('reverse-holofoil'))
 $texto=[Net.WebUtility]::HtmlDecode($html.Content);Check 'Datas, ausente e sem conversão BRL explícitos' ($texto.Contains('Não informado') -and $texto.Contains('UTC') -and $texto.Contains('Sem convers'))
 $oferta=Invoke-WebRequest "$base/gestao/tcgdex151/sv03.5-025/oferta" -WebSession $s;Check 'Formulário oferta exige preçoBRL e estoque sem autopreencher preço' ($oferta.StatusCode -eq 200 -and $oferta.Content.Contains('name="Preco"') -and $oferta.Content.Contains('name="QuantidadeInicial"') -and $oferta.Content.Contains('name="Conservacao"'))
 $publico=Invoke-RestMethod "$base/api/v1/ofertas";Check 'Três ofertas originais têm imagens locais vinculadas' (@($publico.itens|Where-Object {$_.imagemUrl -and $_.referenciaTcgDexId -in @('sv03.5-025','sv03.5-004')}).Count -eq 3)
 $anonimo=Invoke-WebRequest "$base/api/v1/gestao/tcgdex151" -SkipHttpErrorCheck;Check 'Biblioteca gestão protegida401' ($anonimo.StatusCode -eq 401)
 $sem=[Microsoft.PowerShell.Commands.WebRequestSession]::new();$sem.Cookies=$s.Cookies
 $csrf=Invoke-WebRequest "$base/api/v1/gestao/tcgdex151/atualizar" -Method Post -WebSession $sem -ContentType 'application/json' -Body '{"forcarRede":false}' -SkipHttpErrorCheck;Check 'Importação semCSRF recusada400' ($csrf.StatusCode -eq 400)
 $invalid=Invoke-WebRequest "$base/api/v1/gestao/tcgdex151/swsh3-001" -WebSession $s -SkipHttpErrorCheck;Check 'Set externo fora de escopo400' ($invalid.StatusCode -eq 400)
}finally{
 [pscustomobject]@{dataUtc=[datetime]::UtcNow.ToString('o');checks=$checks}|ConvertTo-Json -Depth 5|Set-Content (Join-Path $PSScriptRoot 'smoke-http-resultado.json') -Encoding utf8
 $checks|Format-Table -AutoSize
}

