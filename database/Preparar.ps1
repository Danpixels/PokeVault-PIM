param(
    [string]$Servidor = '.\SQLEXPRESS',
    [Parameter(Mandatory)][ValidateSet('PokeVault','PokeVaultTests')][string]$Banco,
    [ValidateSet('Inicializar','Atualizar')][string]$Modo = 'Atualizar'
)
$ErrorActionPreference = 'Stop'
$pastaEvidencias = Join-Path $PSScriptRoot 'evidencias/sprint2'
New-Item -ItemType Directory -Force -Path $pastaEvidencias | Out-Null
$registro = Join-Path $pastaEvidencias ('preparacao-' + $Banco + '-' + [DateTime]::UtcNow.ToString('yyyyMMdd-HHmmss-fff') + '.txt')
$opcoesSql = @('-S',$Servidor,'-E','-C','-I','-b','-W','-w','240','-f','65001','-l','5','-t','30')
function Invoke-PokeVaultSql {
    param([string[]]$Argumentos)
    & sqlcmd @opcoesSql @Argumentos 2>&1 | Tee-Object -FilePath $registro -Append
    $codigoSql = $LASTEXITCODE
    if ($codigoSql -ne 0) { throw "sqlcmd exit $codigoSql; preparação interrompida. Registro: $registro" }
}
# Primeira operacao sempre somente leitura; nunca preparar se a conexao nao foi identificada.
Invoke-PokeVaultSql -Argumentos @('-Q',"SET NOCOUNT ON; SELECT CONVERT(nvarchar(80),@@SERVERNAME) AS Servidor,CONVERT(nvarchar(30),SERVERPROPERTY('ProductVersion')) AS Versao; SELECT name FROM sys.databases WHERE name IN ('PokeVault','PokeVaultTests');")
$scriptsBanco = @()
if ($Modo -eq 'Inicializar') {
    $scriptsBanco += @('000_preparar_base.sql','001_schema.sql','002_tipos.sql','003_procedures_compra.sql','004_procedures_operacao.sql','005_seed.sql','006_permissoes.sql')
}
$scriptsBanco += @('008_sprint2_versoes.sql','009_sprint2_ajustes.sql','010_sprint2_runtime.sql','013_sprint2_auditoria_carta.sql',
    '019_tcgdex151_schema.sql','020_tcgdex151_importacao.sql','021_tcgdex151_ofertas.sql','024_fluxo_catalogo151.sql','026_contas_senhas.sql')
foreach ($scriptBanco in $scriptsBanco) {
    # Nao republicar 009 sobre a procedure complementada por 013 numa base ja atualizada.
    $versaoDoScript = switch ($scriptBanco) {
        '008_sprint2_versoes.sql' { 2 }
        '009_sprint2_ajustes.sql' { 3 }
        '013_sprint2_auditoria_carta.sql' { 4 }
        '019_tcgdex151_schema.sql' { 8 }
        '020_tcgdex151_importacao.sql' { 9 }
        '021_tcgdex151_ofertas.sql' { 10 }
        '024_fluxo_catalogo151.sql' { 11 }
        '026_contas_senhas.sql' { 12 }
        default { 0 }
    }
    if ($versaoDoScript -gt 0) {
        $consultaVersao = "SET NOCOUNT ON; IF NOT EXISTS (SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'PokeVaultProjeto' AND CONVERT(nvarchar(100),value)=N'PIM-2026-Sprint1') THROW 51101,'BASE_EXISTENTE_NAO_IDENTIFICADA',1; SELECT COUNT(*) FROM dbo.SchemaVersao WHERE Versao=$versaoDoScript;"
        $saidaVersao = @(& sqlcmd @opcoesSql -d $Banco -h -1 -Q $consultaVersao 2>&1)
        if ($LASTEXITCODE -ne 0) {
            $saidaVersao | Tee-Object -FilePath $registro -Append
            throw "Falha ao consultar versão em $Banco; preparação interrompida."
        }
        $textoVersao = ($saidaVersao | Out-String).Trim()
        if ($textoVersao -notin @('0','1')) { throw "Resultado inesperado da consulta de versão; não prosseguir." }
        if ($textoVersao -eq '1') {
            "IGNORADO=$scriptBanco VERSAO_JA_APLICADA=$versaoDoScript BANCO=$Banco" | Tee-Object -FilePath $registro -Append
            continue
        }
    }
    "SCRIPT=$scriptBanco BANCO=$Banco UTC=$([DateTime]::UtcNow.ToString('O'))" | Tee-Object -FilePath $registro -Append
    Invoke-PokeVaultSql -Argumentos @('-v',"DatabaseName=$Banco",'-i',(Join-Path $PSScriptRoot $scriptBanco))
}
foreach ($consultaBanco in @('007_verificar.sql','011_sprint2_verificar.sql','022_tcgdex151_verificar.sql')) {
    "VERIFICACAO=$consultaBanco BANCO=$Banco" | Tee-Object -FilePath $registro -Append
    Invoke-PokeVaultSql -Argumentos @('-v',"DatabaseName=$Banco",'-i',(Join-Path $PSScriptRoot $consultaBanco))
}
"PREPARACAO_CONCLUIDA BANCO=$Banco UTC=$([DateTime]::UtcNow.ToString('O'))" | Tee-Object -FilePath $registro -Append
