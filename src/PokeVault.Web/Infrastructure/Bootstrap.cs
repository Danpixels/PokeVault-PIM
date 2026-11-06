using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using PokeVault.Web.Application;
using PokeVault.Web.Domain;

namespace PokeVault.Web.Infrastructure;

public static class Bootstrap
{
    private sealed record Credencial(string Email, string Nome, string Perfil, string Senha);

    public static async Task Executar(IConfiguration config, IWebHostEnvironment environment, IServiceProvider services, bool mostrar)
    {
        if (!OperatingSystem.IsWindows()) throw new InvalidOperationException("O bootstrap local usa DPAPI do Windows.");
        var connectionString = config.GetConnectionString("Preparacao") ?? config.GetConnectionString("PokeVault")
            ?? throw new InvalidOperationException("Configure a conexão local de preparação.");
        var parsed = new SqlConnectionStringBuilder(connectionString);
        if (parsed.InitialCatalog is not ("PokeVault" or "PokeVaultTests")) throw new InvalidOperationException("Bootstrap restrito às bases exclusivas do projeto.");
        var origem = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(parsed.DataSource + "/" + parsed.InitialCatalog)))[..12];
        var directory = Path.Combine(environment.ContentRootPath, ".local");
        var path = Path.Combine(directory, $"credenciais-{origem}.dpapi");
        List<Credencial> credenciais;
        if (File.Exists(path))
        {
            var bytes = ProtectedData.Unprotect(await File.ReadAllBytesAsync(path), null, DataProtectionScope.CurrentUser);
            credenciais = JsonSerializer.Deserialize<List<Credencial>>(bytes)!;
            CryptographicOperations.ZeroMemory(bytes);
        }
        else
        {
            if (mostrar) throw new InvalidOperationException("Execute --bootstrap primeiro.");
            credenciais = [
                new("admin@pokevault.local", "Administração demo", "ADMINISTRADOR", GerarSenha()),
                new("cliente@pokevault.local", "Cliente demo", "CLIENTE", GerarSenha()),
                new("cliente2@pokevault.local", "Cliente demo 2", "CLIENTE", GerarSenha()),
                new("atendente@pokevault.local", "Atendimento demo", "ATENDENTE", GerarSenha())
            ];
            Directory.CreateDirectory(directory);
            var bytes = JsonSerializer.SerializeToUtf8Bytes(credenciais);
            // Persistir antes do SQL permite retomar uma preparação interrompida sem perder as senhas.
            await File.WriteAllBytesAsync(path, ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser));
            CryptographicOperations.ZeroMemory(bytes);
        }
        if (mostrar)
        {
            if (Console.IsOutputRedirected) throw new InvalidOperationException("Abra --credentials somente em um terminal interativo, sem redirecionar a saída.");
            foreach (var c in credenciais) Console.WriteLine($"{c.Perfil}: {c.Email}  |  {c.Senha}");
            return;
        }

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await using (var marker = new SqlCommand("SELECT COUNT(*) FROM sys.extended_properties WHERE class=0 AND name=N'PokeVaultProjeto' AND CONVERT(nvarchar(100),value)=N'PIM-2026-Sprint1'", connection))
            if (Convert.ToInt32(await marker.ExecuteScalarAsync()) != 1) throw new InvalidOperationException("Base sem marcador do projeto. Execute os scripts de Banco antes.");

        var preparacao = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["ConnectionStrings:PokeVault"] = connectionString }).Build();
        var repository = new SqlLojaRepository(preparacao);
        var hasher = services.GetRequiredService<IPasswordHasher<Usuario>>();
        var admin = credenciais.Single(c => c.Perfil == "ADMINISTRADOR");
        var modeloAdmin = new Usuario(0, admin.Nome, admin.Email, "", admin.Perfil, true);
        await using (var command = new SqlCommand("""
            SET XACT_ABORT ON;
            BEGIN TRY
                BEGIN TRANSACTION;
                DECLARE @lock int;
                EXEC @lock=sys.sp_getapplock @Resource=N'PokeVault.Bootstrap',@LockMode=N'Exclusive',@LockOwner=N'Transaction',@LockTimeout=10000;
                IF @lock < 0 THROW 51104,'BOOTSTRAP_OCUPADO',1;
                IF NOT EXISTS(SELECT 1 FROM dbo.Usuario WITH (UPDLOCK,HOLDLOCK) WHERE AdministradorFixo=1)
                    INSERT dbo.Usuario(PerfilId,Nome,Email,EmailNormalizado,SenhaHash,Ativo,AdministradorFixo)
                    SELECT PerfilId,@Nome,@Email,@Normalizado,@Hash,1,1 FROM dbo.Perfil WHERE Codigo=N'ADMINISTRADOR';
                COMMIT;
            END TRY
            BEGIN CATCH
                IF XACT_STATE()<>0 ROLLBACK;
                THROW;
            END CATCH;
            """, connection))
        {
            command.Parameters.Add("@Nome", SqlDbType.NVarChar, 120).Value = admin.Nome;
            command.Parameters.Add("@Email", SqlDbType.NVarChar, 254).Value = admin.Email;
            command.Parameters.Add("@Normalizado", SqlDbType.NVarChar, 254).Value = LojaService.NormalizarEmail(admin.Email);
            command.Parameters.Add("@Hash", SqlDbType.NVarChar, 512).Value = hasher.HashPassword(modeloAdmin, admin.Senha);
            await command.ExecuteNonQueryAsync();
        }
        var usuarioAdmin = await repository.UsuarioPorEmail(LojaService.NormalizarEmail(admin.Email), default);
        if (usuarioAdmin is null || usuarioAdmin.Perfil != "ADMINISTRADOR" || !usuarioAdmin.Ativo
            || hasher.VerifyHashedPassword(usuarioAdmin, usuarioAdmin.SenhaHash, admin.Senha) == PasswordVerificationResult.Failed)
            throw new InvalidOperationException("Administrador existente não corresponde à credencial local. Nenhuma senha existente foi substituída.");

        foreach (var c in credenciais.Where(c => c.Perfil != "ADMINISTRADOR"))
        {
            var existente = await repository.UsuarioPorEmail(LojaService.NormalizarEmail(c.Email), default);
            if (existente is not null)
            {
                if (existente.Perfil != c.Perfil || !existente.Ativo || hasher.VerifyHashedPassword(existente, existente.SenhaHash, c.Senha) == PasswordVerificationResult.Failed)
                    throw new InvalidOperationException("Conta de demonstração existente diverge do arquivo protegido. Não foi substituída.");
                continue; // Nunca redefinir saldo na reexecução.
            }
            var usuario = new Usuario(0, c.Nome, c.Email, "", c.Perfil, true);
            await repository.Provisionar(c.Email, c.Nome, hasher.HashPassword(usuario, c.Senha), c.Perfil, default);
            if (c.Perfil == "CLIENTE")
            {
                var criado = (await repository.UsuarioPorEmail(LojaService.NormalizarEmail(c.Email), default))!;
                await using var saldo = new SqlCommand("dbo.usp_DefinirSaldo", connection) { CommandType = CommandType.StoredProcedure };
                saldo.Parameters.Add("@AdministradorId", SqlDbType.Int).Value = usuarioAdmin.Id;
                saldo.Parameters.Add("@ClienteId", SqlDbType.Int).Value = criado.Id;
                var valor = saldo.Parameters.Add("@NovoSaldo", SqlDbType.Decimal); valor.Precision = 12; valor.Scale = 2; valor.Value = c.Email == "cliente2@pokevault.local" ? 10m : 100m;
                saldo.Parameters.Add("@Motivo", SqlDbType.NVarChar, 300).Value = "Saldo fictício inicial da demonstração local";
                await saldo.ExecuteNonQueryAsync();
            }
        }
        Console.WriteLine("Bootstrap concluído. Credenciais protegidas por DPAPI do usuário Windows atual em: " + path);
        Console.WriteLine("Para consultar em terminal interativo local, execute o mesmo projeto com --credentials. Senhas não são registradas no log.");
    }

    private static string GerarSenha() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(24));
}
