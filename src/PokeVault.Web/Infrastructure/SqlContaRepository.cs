using System.Data;
using Microsoft.Data.SqlClient;

namespace PokeVault.Web.Infrastructure;

public sealed partial class SqlLojaRepository
{
    public Task<int> RegistrarCliente(string nome,string email,string normalizado,string hash,CancellationToken ct) => Executar(
        "dbo.usp_RegistrarCliente",CommandType.StoredProcedure,p=>
        {
            p.Add("@Nome",SqlDbType.NVarChar,120).Value=nome;p.Add("@Email",SqlDbType.NVarChar,254).Value=email;
            p.Add("@EmailNormalizado",SqlDbType.NVarChar,254).Value=normalizado;p.Add("@SenhaHash",SqlDbType.NVarChar,512).Value=hash;
        },async r=>await r.ReadAsync(ct)?r.GetInt32(0):throw new InvalidOperationException("Cadastro sem resultado."),ct);

    public async Task AlterarSenha(int usuarioId,string hashEsperado,string novoHash,CancellationToken ct) =>
        _=await Executar("dbo.usp_AlterarSenhaConta",CommandType.StoredProcedure,p=>
        {
            p.Add("@UsuarioId",SqlDbType.Int).Value=usuarioId;p.Add("@HashEsperado",SqlDbType.NVarChar,512).Value=hashEsperado;
            p.Add("@NovoHash",SqlDbType.NVarChar,512).Value=novoHash;
        },async r=>await r.ReadAsync(ct),ct);
}
