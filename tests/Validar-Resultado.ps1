[CmdletBinding()]
param([Parameter(Mandatory)][string]$Caminho,[int]$MinimoCenarios=45)
$ErrorActionPreference='Stop'
$dados=Get-Content -LiteralPath $Caminho -Raw | ConvertFrom-Json
$casos=@($dados.resultados)
if($dados.baseSql -ne 'PokeVaultTests'){throw 'Resultado nao identifica PokeVaultTests.'}
if($casos.Count -lt $MinimoCenarios){throw "Cobertura incompleta: $($casos.Count), minimo $MinimoCenarios."}
if(@($casos|Where-Object { !$_.id -or $_.status -ne 'aprovado' }).Count -gt 0){throw 'Ha cenarios reprovados/bloqueados/sem status; consultar JSON preservado.'}
if(@($casos.id|Select-Object -Unique).Count -ne $casos.Count){throw 'IDs repetidos no resultado.'}
[pscustomobject]@{aprovados=$casos.Count;baseSql=$dados.baseSql}
