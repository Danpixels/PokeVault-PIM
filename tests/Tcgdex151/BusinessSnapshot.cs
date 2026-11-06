using Microsoft.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
namespace PokeVault.Testes.Tcgdex;

public static class BusinessSnapshot
{
    // Campos da baseline local. Colunas/referencias TCGdex novas nao entram no hash comercial.
    static readonly (string Table,string Fields,string Key)[] Tables=[
        ("Perfil","PerfilId,Codigo","PerfilId"),
        ("Usuario","UsuarioId,PerfilId,Nome,Email,EmailNormalizado,SenhaHash,Ativo,AdministradorFixo,CriadoEmUtc","UsuarioId"),
        ("SaldoCliente","UsuarioId,Saldo","UsuarioId"),
        ("Colecao","ColecaoId,Codigo,Nome","ColecaoId"),
        ("Carta","CartaId,ColecaoId,Numero,Nome,Raridade,ImagemUrl,Conservacao,Preco,QuantidadeEstoque,Ativo","CartaId"),
        ("Carrinho","CarrinhoId,UsuarioId","CarrinhoId"),
        ("ItemCarrinho","CarrinhoId,CartaId,Quantidade","CarrinhoId,CartaId"),
        ("Compra","CompraId,ClienteId,ChaveOperacao,ResumoRequisicao,Status,Total,CriadaEmUtc,CanceladaEmUtc,CanceladaPorId,MotivoCancelamento","CompraId"),
        ("ItemCompra","CompraId,CartaId,Quantidade,PrecoUnitario,NomeCartaSnapshot,ConservacaoSnapshot","CompraId,CartaId"),
        ("MovimentacaoSaldo","MovimentacaoSaldoId,UsuarioId,CompraId,Tipo,ValorDelta,SaldoApos,ResponsavelId,Motivo,CriadaEmUtc","MovimentacaoSaldoId"),
        ("MovimentacaoEstoque","MovimentacaoEstoqueId,CartaId,CompraId,Tipo,QuantidadeDelta,EstoqueApos,ResponsavelId,Motivo,CriadaEmUtc","MovimentacaoEstoqueId"),
        ("AlteracaoCarta","AlteracaoCartaId,CartaId,ResponsavelId,Motivo,PrecoAnterior,PrecoNovo,EstoqueAnterior,EstoqueNovo,AtivoAnterior,AtivoNovo,CriadaEmUtc","AlteracaoCartaId")
    ];
    public static async Task<Dictionary<string,string>> Capture(SqlConnection connection)
    {
        if(connection.Database!="PokeVaultTests")throw new InvalidOperationException("Snapshot restrito a base de testes");
        var result=new Dictionary<string,string>();
        foreach(var t in Tables)
        {
            await using var cmd=new SqlCommand($"SELECT (SELECT {t.Fields} FROM dbo.[{t.Table}] ORDER BY {t.Key} FOR JSON PATH,INCLUDE_NULL_VALUES)",connection);
            var data=Convert.ToString(await cmd.ExecuteScalarAsync())??"[]";
            result[t.Table]=Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(data)));
        }
        return result; // Nao retorna JSON dos registros ou hashes de senha individuais.
    }
}
