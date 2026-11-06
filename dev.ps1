param(
    [ValidateSet('Restore','Build','Run','Bootstrap','Credenciais','CopiarTestes')][string]$Acao='Run',
    [ValidateSet('PokeVault','PokeVaultTests')][string]$Base='PokeVault',
    [ValidateRange(0,65535)][int]$Porta=0
)
$ErrorActionPreference='Stop'
Push-Location $PSScriptRoot
try {
    $env:DOTNET_CLI_HOME=Join-Path $PSScriptRoot '.local/dotnet'
    $env:NUGET_PACKAGES=Join-Path $PSScriptRoot '.local/packages'
    $env:DOTNET_CLI_TELEMETRY_OPTOUT='1'
    $env:ASPNETCORE_ENVIRONMENT='Development'
    $env:POKEVAULT_DATABASE=$Base
    if ($Porta -eq 0) { $Porta=if($Base -eq 'PokeVaultTests'){5188}else{5187} }
    if ($Acao -in @('Restore','Build')) {
        $roamingOriginal=$env:APPDATA
        $localOriginal=$env:LOCALAPPDATA
        try {
            $env:APPDATA=Join-Path $PSScriptRoot '.local/appdata'
            $env:LOCALAPPDATA=Join-Path $PSScriptRoot '.local/localappdata'
            New-Item -ItemType Directory -Force $env:APPDATA,$env:LOCALAPPDATA | Out-Null
            if($Acao -eq 'Restore') { & dotnet restore PokeVault.sln --configfile NuGet.Config }
            else { & dotnet build PokeVault.sln --no-restore }
            if($LASTEXITCODE -ne 0) { throw "$Acao falhou; consulte a saída." }
        } finally { $env:APPDATA=$roamingOriginal; $env:LOCALAPPDATA=$localOriginal }
    } elseif ($Acao -eq 'CopiarTestes') {
        $origem=Join-Path $PSScriptRoot 'src/PokeVault.Web/bin/Debug/net10.0'
        $destino=Join-Path $PSScriptRoot '.local/testapp'
        if (!(Test-Path (Join-Path $origem 'PokeVault.Web.dll'))) { throw 'Execute Build primeiro.' }
        New-Item -ItemType Directory -Force $destino | Out-Null
        Get-ChildItem -LiteralPath $origem | Copy-Item -Destination $destino -Recurse -Force
        Write-Output 'Snapshot de binários copiado para .local/testapp. Pare o processo de testes antes de atualizar esse snapshot.'
    } else {
        $argumentos=@('run','--project','src/PokeVault.Web','--no-build','--no-launch-profile','--')
        if($Acao -eq 'Bootstrap') { $argumentos+='--bootstrap' }
        elseif($Acao -eq 'Credenciais') { $argumentos+='--credentials' }
        else { $argumentos+=@('--urls',"http://localhost:$Porta") }
        if ($Base -eq 'PokeVaultTests') {
            $dll=Join-Path $PSScriptRoot '.local/testapp/PokeVault.Web.dll'
            if (!(Test-Path $dll)) { throw 'Execute CopiarTestes primeiro.' }
            $argumentos=@($dll,'--contentRoot',(Join-Path $PSScriptRoot 'src/PokeVault.Web'))
            if($Acao -eq 'Bootstrap'){$argumentos+='--bootstrap'}
            elseif($Acao -eq 'Credenciais'){$argumentos+='--credentials'}
            else{$argumentos+=@('--urls',"http://localhost:$Porta")}
        }
        & dotnet @argumentos
        if($LASTEXITCODE -ne 0) { throw "$Acao falhou; consulte a saída." }
    }
} finally { Pop-Location }

