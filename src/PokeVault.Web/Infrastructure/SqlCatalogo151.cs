using System.Data;
using Microsoft.Data.SqlClient;
using PokeVault.Web.Domain;
using PokeVault.Web.Models;

namespace PokeVault.Web.Infrastructure;

public sealed partial class SqlLojaRepository
{
    public Task<TcgPagina> Referencias151(CancellationToken ct) => Executar(
        RefSelect + " FROM dbo.vw_BibliotecaTCGdex151 ORDER BY Numero",CommandType.Text,_=>{},async r=>
        {
            List<TcgReferencia> itens=[];
            while(await r.ReadAsync(ct)) itens.Add(LerReferencia(r));
            return new TcgPagina(itens,1,itens.Count);
        },ct);
    public Task<IReadOnlyList<Oferta>> Ofertas151(CancellationToken ct) => Executar<IReadOnlyList<Oferta>>(
        """
        SELECT c.CartaId,c.Nome,co.Nome,c.Numero,c.Conservacao,c.Preco,c.QuantidadeEstoque,r.CardId,r.Idioma,c.Idioma,c.Acabamento
        FROM dbo.Carta c JOIN dbo.Colecao co ON co.ColecaoId=c.ColecaoId
        LEFT JOIN dbo.VinculoCartaTCGdex v ON v.CartaId=c.CartaId
        LEFT JOIN dbo.ReferenciaTCGdex r ON r.ReferenciaTCGdexId=v.ReferenciaTCGdexId
        WHERE co.Codigo=N'MEW-151' AND c.Ativo=1
        ORDER BY c.Numero,c.Conservacao,c.Idioma,c.Acabamento,c.CartaId
        """,CommandType.Text,_=>{},async r=>
        {
            List<Oferta> lista=[];
            while(await r.ReadAsync(ct))
            {
                var referencia=r.IsDBNull(7)?null:r.GetString(7);
                lista.Add(new(r.GetInt32(0),r.GetString(1),r.GetString(2),r.GetString(3),r.GetString(4),r.GetDecimal(5),r.GetInt32(6),
                    images?.Resolver(referencia,r.IsDBNull(8)?null:r.GetString(8)).Url,referencia,r.GetString(9),r.GetString(10)));
            }
            return lista;
        },ct);
    public Task<IReadOnlyList<OfertaOperacao>> OfertasOperacao151(CancellationToken ct) => Executar<IReadOnlyList<OfertaOperacao>>(
        """
        SELECT c.CartaId,r.CardId,c.Conservacao,c.Idioma,c.Acabamento,c.Preco,c.QuantidadeEstoque,c.Ativo,c.Versao
        FROM dbo.Carta c JOIN dbo.VinculoCartaTCGdex v ON v.CartaId=c.CartaId
        JOIN dbo.ReferenciaTCGdex r ON r.ReferenciaTCGdexId=v.ReferenciaTCGdexId
        JOIN dbo.Colecao co ON co.ColecaoId=c.ColecaoId WHERE co.Codigo=N'MEW-151' AND r.SetId=N'sv03.5'
        ORDER BY c.CartaId
        """,CommandType.Text,_=>{},async r=>
        {
            List<OfertaOperacao> lista=[];
            while(await r.ReadAsync(ct)) lista.Add(new(r.GetInt32(0),r.GetString(1),r.GetString(2),r.GetString(3),r.GetString(4),r.GetDecimal(5),r.GetInt32(6),r.GetBoolean(7),Convert.ToBase64String((byte[])r.GetValue(8))));
            return lista;
        },ct);
    public Task<int> CriarVariante151(int ator,int referencia,SalvarOferta151Model model,CancellationToken ct) => Executar(
        "dbo.usp_CadastrarOfertaTCGdex151",CommandType.StoredProcedure,p=>
        {
            p.Add("@ResponsavelId",SqlDbType.Int).Value=ator;p.Add("@ReferenciaTCGdexId",SqlDbType.Int).Value=referencia;
            p.Add("@Conservacao",SqlDbType.NVarChar,30).Value=model.Conservacao;p.Add("@Idioma",SqlDbType.NVarChar,20).Value=model.Idioma;
            p.Add("@Acabamento",SqlDbType.NVarChar,20).Value=model.Acabamento;Monetario(p,"@Preco",model.Preco!.Value);
            p.Add("@QuantidadeInicial",SqlDbType.Int).Value=model.Unidades!.Value;
        },async r=>await r.ReadAsync(ct)?r.GetInt32(0):throw new InvalidOperationException("Oferta sem resultado."),ct);
    public async Task AdicionarItem(int cliente,int oferta,int quantidade,CancellationToken ct) =>
        _=await Executar("dbo.usp_AdicionarItemCarrinho",CommandType.StoredProcedure,p=>
        {
            p.Add("@ClienteId",SqlDbType.Int).Value=cliente;p.Add("@CartaId",SqlDbType.Int).Value=oferta;p.Add("@Quantidade",SqlDbType.Int).Value=quantidade;
        },async r=>await r.ReadAsync(ct),ct);
}
