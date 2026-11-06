using System.ComponentModel.DataAnnotations;
using PokeVault.Web.Domain;
using PokeVault.Web.Infrastructure;
using PokeVault.Web.Models;

namespace PokeVault.Web.Application;

public sealed class Catalogo151Service(SqlLojaRepository repository, TcgDexService biblioteca, GestaoService gestao, LojaService loja)
{
    public async Task<TcgPagina> Biblioteca(int ator,CancellationToken ct)
    {
        await biblioteca.Autorizar(ator,ct);
        return await repository.Referencias151(ct);
    }
    public async Task<IReadOnlyList<CartaVitrine>> Vitrine(CancellationToken ct)
    {
        var ofertas = await repository.Ofertas151(ct);
        return ofertas.Where(o => o.Estoque > 0).GroupBy(o => o.Numero)
            .Select(g => new CartaVitrine(g.First().Nome,g.First().Colecao,g.Key,g.Sum(o => o.Estoque),g.Min(o => o.Preco),
                g.Select(o => o.Preco).Distinct().Count()>1,g.Select(o=>o.ImagemUrl).FirstOrDefault(u=>u is not null),g.First().OfertaId))
            .OrderBy(c => c.Numero).ToArray();
    }
    public async Task<CartaLoja> Carta(int ofertaId,CancellationToken ct)
    {
        var ofertas=await repository.Ofertas151(ct);
        var principal=ofertas.FirstOrDefault(o=>o.OfertaId==ofertaId) ?? throw new RegraException("CARTA_INDISPONIVEL","Carta não encontrada no catálogo.",404);
        return new(ofertas.Where(o=>o.Numero==principal.Numero).ToArray());
    }
    public async Task<IReadOnlyList<OfertaOperacao>> Operacao(int ator,CancellationToken ct)
    {
        await biblioteca.Autorizar(ator,ct);
        return await repository.OfertasOperacao151(ct);
    }
    public async Task Salvar(int ator,string referenciaId,SalvarOferta151Model model,CancellationToken ct)
    {
        var referencia=await biblioteca.Obter(ator,referenciaId,ct);
        if (!referencia.Detalhada || !Validator.TryValidateObject(model,new ValidationContext(model),null,true)
            || !Conservacoes.Todas.Contains(model.Conservacao) || decimal.Round(model.Preco!.Value,2)!=model.Preco.Value)
            throw new RegraException("DADOS_INVALIDOS","Revise estado, unidades e preço em reais com até duas casas decimais.",400);
        if(model.OfertaId is int id)
        {
            var atual=(await repository.OfertasOperacao151(ct)).FirstOrDefault(o=>o.OfertaId==id && o.ReferenciaId==referenciaId);
            if(atual is null || atual.Conservacao!=model.Conservacao || atual.Idioma!=model.Idioma || atual.Acabamento!=model.Acabamento)
                throw new RegraException("VARIANTE_INVALIDA","A variante mudou ou não corresponde a esta carta. Recarregue o catálogo.",409);
            await gestao.AjustarCarta(ator,id,new AjusteCartaModel { Preco=model.Preco,NovoEstoque=model.Unidades,Ativo=true,
                VersaoEsperada=model.VersaoEsperada ?? "",Motivo="Ajuste de preço/estoque pelo catálogo 151" },ct);
        }
        else
        {
            if(!Variantes.Idiomas.Contains(model.Idioma) || !Variantes.Acabamentos.Contains(model.Acabamento))
                throw new RegraException("VARIANTE_INVALIDA","Selecione idioma e acabamento válidos.",400);
            await repository.CriarVariante151(ator,referencia.ReferenciaId,model,ct);
        }
    }
    public async Task Adicionar(int cliente,int ofertaId,int quantidade,CancellationToken ct)
    {
        await loja.Carrinho(cliente,ct); // Revalida perfil/conta ativa.
        if(ofertaId<=0 || quantidade is <1 or >10000) throw new RegraException("QUANTIDADE_INVALIDA","Informe entre 1 e 10.000 unidades.",400);
        await repository.AdicionarItem(cliente,ofertaId,quantidade,ct);
    }
}
