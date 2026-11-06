[CmdletBinding()]
param([ValidatePattern('^[A-Za-z0-9_-]+$')][string]$Rodada=('execucao-'+[DateTime]::UtcNow.ToString('yyyyMMdd-HHmmss')),[string]$PastaEvidencias='docs/evidencias/sprint2',[string]$ProdutoSha256Esperado='',[ValidateSet(4,10,11,12)][int]$SchemaMaximoEsperado=4,[switch]$Fluxo151,[switch]$Contas)
$ErrorActionPreference='Stop'
$raizTestes=(Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$pastaEvidencia=Join-Path $raizTestes (Join-Path $PastaEvidencias $Rodada)
if(Test-Path -LiteralPath $pastaEvidencia){throw 'Rodada ja existe; nao sobrescrever evidencia.'}
$produtoOrigem=Join-Path $raizTestes 'src/PokeVault.Web/bin/Debug/net10.0'
if(!(Test-Path (Join-Path $produtoOrigem 'PokeVault.Web.dll'))){throw 'Aguardar build da aplicacao por Desenvolvimento.'}
if($ProdutoSha256Esperado -and (Get-FileHash (Join-Path $produtoOrigem 'PokeVault.Web.dll') -Algorithm SHA256).Hash -ne $ProdutoSha256Esperado){throw 'Binario difere da versao final liberada.'}
$tcpTeste=[Net.Sockets.TcpClient]::new()
try {$portaOcupada=$tcpTeste.ConnectAsync('localhost',5188).Wait(500); if($portaOcupada -and $tcpTeste.Connected){throw 'Porta5188 ocupada; nao interromper processo existente.'}} catch {if($_.Exception.Message -like '*Porta5188*'){throw}} finally {$tcpTeste.Dispose()}
$pastaRuntime=Join-Path $PSScriptRoot ".runtime/$Rodada"
New-Item -ItemType Directory -Force $pastaRuntime,$pastaEvidencia|Out-Null
Get-ChildItem -LiteralPath $produtoOrigem|Copy-Item -Destination $pastaRuntime -Recurse -Force
Copy-Item -LiteralPath (Join-Path $raizTestes 'src/PokeVault.Web/wwwroot') -Destination $pastaRuntime -Recurse -Force
$nomesAmbiente=@('ASPNETCORE_ENVIRONMENT','POKEVAULT_DATABASE','ConnectionStrings__PokeVault','DOTNET_CLI_HOME','NUGET_PACKAGES','PV_TEST_HTTP','PV_TEST_FOCUS','PV_TEST_UI_FIXTURE','PV_TEST_REVISAO_HTML','PV_TEST_SCHEMA_MAX','APPDATA','LOCALAPPDATA')
$ambienteAnterior=@{};foreach($nomeAmbiente in $nomesAmbiente){$ambienteAnterior[$nomeAmbiente]=[Environment]::GetEnvironmentVariable($nomeAmbiente,'Process')}
$processoTestes=$null
Push-Location $raizTestes
try {
    $env:DOTNET_CLI_HOME=Join-Path $raizTestes '.local/dotnet'
    $env:NUGET_PACKAGES=Join-Path $raizTestes '.local/packages'
    $env:APPDATA=Join-Path $raizTestes '.local/appdata';$env:LOCALAPPDATA=Join-Path $raizTestes '.local/localappdata'
    $env:ASPNETCORE_ENVIRONMENT='Development';$env:POKEVAULT_DATABASE='PokeVaultTests'
    $env:ConnectionStrings__PokeVault='Server=.\SQLEXPRESS;Database=PokeVaultTests;Integrated Security=true;Encrypt=true;TrustServerCertificate=true;Connect Timeout=5'
    $env:PV_TEST_SCHEMA_MAX=[string]$SchemaMaximoEsperado
    $env:PV_TEST_HTTP='1';$env:PV_TEST_FOCUS='';$env:PV_TEST_UI_FIXTURE='';$env:PV_TEST_REVISAO_HTML=''
    if($Fluxo151){$env:PV_TEST_FOCUS='FLUXO151'}
    if($Contas){$env:PV_TEST_FOCUS='CONTAS'}
    & dotnet restore tests/Sprint2/PokeVault.Testes.csproj --configfile NuGet.Config --ignore-failed-sources
    if($LASTEXITCODE -ne 0){throw 'Restore do harness falhou.'}
    & dotnet build tests/Sprint2/PokeVault.Testes.csproj --no-restore
    if($LASTEXITCODE -ne 0){throw 'Build do harness falhou.'}
    $dllRuntime=Join-Path $pastaRuntime 'PokeVault.Web.dll'
    $processoTestes=Start-Process -FilePath (Get-Command dotnet).Source -ArgumentList @(('"'+$dllRuntime+'"'),'--contentRoot',('"'+$pastaRuntime+'"'),'--urls','http://localhost:5188') -PassThru -WindowStyle Hidden -RedirectStandardOutput (Join-Path $pastaRuntime 'servidor-out.log') -RedirectStandardError (Join-Path $pastaRuntime 'servidor-err.log')
    $pronto=$false
    for($tentativa=0;$tentativa -lt 60;$tentativa++){
        try{if((Invoke-WebRequest http://localhost:5188/health -TimeoutSec 1).StatusCode -eq 200){$pronto=$true;break}}catch{}
        if($processoTestes.HasExited){throw 'Processo de testes encerrou; consultar logs no runtime.'}
        Start-Sleep -Milliseconds 250
    }
    if(!$pronto){throw 'Processo5188 nao respondeu a health no prazo.'}
    $dllHarness=Join-Path $raizTestes 'tests/Sprint2/bin/Debug/net10.0/PokeVault.Testes.dll'
    @{dataUtc=[DateTime]::UtcNow.ToString('o');base='PokeVaultTests';porta=5188;wrapperSHA256=(Get-FileHash (Join-Path $PSScriptRoot 'Executar-Sprint2.ps1') -Algorithm SHA256).Hash;validadorSHA256=(Get-FileHash (Join-Path $PSScriptRoot 'Validar-Resultado.ps1') -Algorithm SHA256).Hash;produtoSHA256=(Get-FileHash $dllRuntime -Algorithm SHA256).Hash;harnessSHA256=(Get-FileHash $dllHarness -Algorithm SHA256).Hash}|ConvertTo-Json|Set-Content (Join-Path $pastaEvidencia 'manifesto.json') -Encoding utf8
    $resultadoCaminho=Join-Path $pastaEvidencia 'resultados.json'
    & dotnet $dllHarness $resultadoCaminho
    if($LASTEXITCODE -ne 0){throw 'Harness encerrou com erro.'}
    & (Join-Path $PSScriptRoot 'Validar-Resultado.ps1') -Caminho $resultadoCaminho -MinimoCenarios $(if($Contas){10}elseif($Fluxo151){10}else{45})
} finally {
    if($processoTestes -and !$processoTestes.HasExited){Stop-Process -Id $processoTestes.Id; $processoTestes.WaitForExit()}
    @{dataUtc=[DateTime]::UtcNow.ToString('o');porta=5188;pidProcesso=if($processoTestes){$processoTestes.Id}else{$null};encerrado=if($processoTestes){$processoTestes.HasExited}else{$null}}|ConvertTo-Json|Set-Content (Join-Path $pastaEvidencia 'encerramento.json') -Encoding utf8
    foreach($nomeAmbiente in $nomesAmbiente){[Environment]::SetEnvironmentVariable($nomeAmbiente,$ambienteAnterior[$nomeAmbiente],'Process')}
    Pop-Location
}





