using Microsoft.Data.SqlClient;

namespace PokeVault.Web.Infrastructure;

public sealed class LocalReadiness(IConfiguration config)
{
    public async Task<bool> Verificar(CancellationToken ct)
    {
        try
        {
            await using var connection = new SqlConnection(config.GetConnectionString("PokeVault"));
            await connection.OpenAsync(ct);
            await using var command = new SqlCommand("""
                SELECT CASE WHEN DB_NAME() IN (N'PokeVault',N'PokeVaultTests')
                  AND EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'PokeVaultProjeto' AND CONVERT(nvarchar(100),value)=N'PIM-2026-Sprint1')
                  AND (SELECT MAX(Versao) FROM dbo.SchemaVersao)=12
                  AND (SELECT COUNT(*) FROM dbo.SchemaVersao WHERE Versao IN(4,8,9,10,11,12))=6
                  THEN 1 ELSE 0 END
                """, connection) { CommandTimeout = 3 };
            return Convert.ToInt32(await command.ExecuteScalarAsync(ct)) == 1;
        }
        catch (SqlException) { return false; }
    }
}
