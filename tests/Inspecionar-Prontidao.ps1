[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$raizProjeto = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$alvos = @('docs/DECISOES.md','docs/diario-sprints.md','docs/contrato-sprint1.md','src/PokeVault.Web/PokeVault.Web.csproj','src/PokeVault.Web/Program.cs')
$arquivos = foreach ($relativo in $alvos) {
    $absoluto = Join-Path $raizProjeto $relativo
    $existe = Test-Path -LiteralPath $absoluto -PathType Leaf
    [pscustomobject]@{
        Caminho = $relativo
        Existe = $existe
        SHA256 = if ($existe) { (Get-FileHash -LiteralPath $absoluto -Algorithm SHA256).Hash } else { $null }
    }
}
$sql = @(if (Test-Path -LiteralPath (Join-Path $raizProjeto 'database')) {
    Get-ChildItem -LiteralPath (Join-Path $raizProjeto 'database') -Filter '*.sql' -File -Recurse | ForEach-Object {
        [pscustomobject]@{Caminho=$_.FullName.Substring($raizProjeto.Length + 1);SHA256=(Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash}
    }
})
[pscustomobject]@{
    Tipo = 'Inspecao estatica de arquivos; nao e teste funcional'
    RelogioHostUtc = [DateTime]::UtcNow.ToString('o')
    Raiz = $raizProjeto
    Arquivos = @($arquivos)
    ScriptsSql = $sql
    Limite = 'Nao conecta SQL, nao inicia app, nao faz restore/build, nao verifica rotas nem executa transacoes. Presenca/hash nao significa contrato aprovado ou funcionamento.'
} | ConvertTo-Json -Depth 6
