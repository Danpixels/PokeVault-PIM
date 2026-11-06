param(
    [switch]$SemNavegador,
    [switch]$SemDialogo,
    [ValidateRange(1024,65535)][int]$Porta=5187
)
$ErrorActionPreference='Stop'
$endereco="http://localhost:$Porta"
$processoIniciado=$null
function Ler-Prontidao {
    $resposta=$null
    try {
        $pedido=[Net.HttpWebRequest]::Create("$endereco/health/ready")
        $pedido.Timeout=3000
        $pedido.ReadWriteTimeout=3000
        try {$resposta=$pedido.GetResponse()} catch [Net.WebException] {$resposta=$_.Exception.Response}
        if($null -eq $resposta){return $null}
        $leitor=New-Object IO.StreamReader($resposta.GetResponseStream())
        try {$json=$leitor.ReadToEnd()|ConvertFrom-Json} finally {$leitor.Dispose()}
        return [pscustomobject]@{codigo=[int]$resposta.StatusCode;dados=$json}
    } catch {return $null} finally {if($null -ne $resposta){$resposta.Dispose()}}
}
function Porta-Ocupada {
    $cliente=New-Object Net.Sockets.TcpClient
    try {$cliente.Connect('localhost',$Porta);return $true} catch {return $false} finally {$cliente.Dispose()}
}
try {
    Set-Location $PSScriptRoot
    $estado=Ler-Prontidao
    $reutilizado=$false
    if($estado -and $estado.dados.aplicacao -eq 'PokeVault') {
        if($estado.codigo -ne 200){throw 'O PokéVault está aberto, mas o SQL Server ou o banco local não está pronto. Verifique se o SQL Server está ativo e se a base está no schema12. Nenhum dado foi reinicializado.'}
        $reutilizado=$true
    } else {
        if(Porta-Ocupada){throw "A porta $Porta já está ocupada por outro processo ou versão. Feche essa versão antes de abrir o PokéVault; nenhum processo alheio foi encerrado."}
        $dotnet=Get-Command dotnet -ErrorAction SilentlyContinue
        if(!$dotnet){throw 'O .NET não foi encontrado. Este projeto requer o SDK .NET10 já instalado; o abridor não instala ferramentas.'}
        $dll=Join-Path $PSScriptRoot 'src/PokeVault.Web/bin/Debug/net10.0/PokeVault.Web.dll'
        $pastaLog=Join-Path $PSScriptRoot '.local/launcher'
        New-Item -ItemType Directory -Force $pastaLog | Out-Null
        if(!(Test-Path $dll)) {
            & (Join-Path $PSScriptRoot 'dev.ps1') Restore *> (Join-Path $pastaLog 'restore.txt')
            & (Join-Path $PSScriptRoot 'dev.ps1') Build *> (Join-Path $pastaLog 'build.txt')
        }
        $env:ASPNETCORE_ENVIRONMENT='Development'
        $env:POKEVAULT_DATABASE='PokeVault'
        $stamp=[datetime]::UtcNow.ToString('yyyyMMdd-HHmmss-fff')
        $argumentos='"'+$dll+'" --contentRoot "'+(Join-Path $PSScriptRoot 'src/PokeVault.Web')+'" --urls '+$endereco
        $processoIniciado=Start-Process -FilePath $dotnet.Source -ArgumentList $argumentos -WorkingDirectory $PSScriptRoot -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $pastaLog "$stamp-saida.txt") -RedirectStandardError (Join-Path $pastaLog "$stamp-erro.txt")
        $limite=[datetime]::UtcNow.AddSeconds(35)
        do {
            $estado=Ler-Prontidao
            if($estado -and $estado.dados.aplicacao -eq 'PokeVault') {break}
            if($processoIniciado.HasExited){throw 'O servidor local não conseguiu iniciar. Consulte .local/launcher e o README. Nenhuma instalação ou preparação do banco foi executada.'}
            Start-Sleep -Milliseconds 400
        } while([datetime]::UtcNow -lt $limite)
        if(!$estado -or $estado.codigo -ne 200 -or $estado.dados.aplicacao -ne 'PokeVault') {throw 'O SQL Server ou o banco local não está disponível no schema12. Verifique o SQL Server e a configuração local indicada no README. O abridor não cria nem reseta o banco.'}
    }
    if(!$SemNavegador){Start-Process $endereco}
    $registro=[pscustomobject]@{dataUtc=[datetime]::UtcNow.ToString('o');status='pronto';endereco=$endereco;reutilizado=$reutilizado;processoIniciado=if($processoIniciado){$processoIniciado.Id}else{$null};navegadorSolicitado=(!$SemNavegador)}
    $diretorioLog=Join-Path $PSScriptRoot '.local/launcher'
    New-Item -ItemType Directory -Force $diretorioLog | Out-Null
    $registro|ConvertTo-Json -Compress|Add-Content (Join-Path $diretorioLog 'aberturas.jsonl') -Encoding utf8
    $registro
} catch {
    if($processoIniciado -and !$processoIniciado.HasExited){Stop-Process -Id $processoIniciado.Id -ErrorAction SilentlyContinue}
    $mensagem=$_.Exception.Message
    if(!$SemDialogo){Add-Type -AssemblyName PresentationFramework;[System.Windows.MessageBox]::Show($mensagem,'PokéVault — abertura local','OK','Warning')|Out-Null}
    Write-Output $mensagem
    exit 1
}
