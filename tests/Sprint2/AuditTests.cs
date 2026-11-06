using System.Data;
using Microsoft.Data.SqlClient;
partial class Suite
{
    private partial async Task Audit()
    {
        await Case("SQL-19/S2-auditoria-sem-delta","Somente preco/ativacao registra autor,motivo,antes/depois; no-op/conflito sem auditoria extra ou estoque0",async()=>{
            var a=await Card();await using var c=await Open();var old=await Version(c,a);var one=await StockVersion(c,a,40,2,old);var v=(byte[])one["Versao"]!;
            var two=await Proc(c,"dbo.usp_AjustarCartaVersionado",P("@ResponsavelId",admin),P("@CartaId",a),Money("@Preco",40),P("@NovoEstoque",2),P("@Ativo",false),P("@Motivo",tag),new SqlParameter("@VersaoEsperada",SqlDbType.Binary,8){Value=v});
            var nv=(byte[])two["Versao"]!;
            await Proc(c,"dbo.usp_AjustarCartaVersionado",P("@ResponsavelId",admin),P("@CartaId",a),Money("@Preco",40),P("@NovoEstoque",2),P("@Ativo",false),P("@Motivo",tag),new SqlParameter("@VersaoEsperada",SqlDbType.Binary,8){Value=nv});
            await Error(51012,async()=>{await StockVersion(c,a,30,2,old);});
            var price=await Count(c,"AlteracaoCarta","CartaId=@a AND ResponsavelId=@r AND Motivo=@m AND PrecoAnterior=30 AND PrecoNovo=40 AND EstoqueAnterior=2 AND EstoqueNovo=2 AND AtivoAnterior=1 AND AtivoNovo=1",P("@a",a),P("@r",admin),P("@m",tag));
            var activation=await Count(c,"AlteracaoCarta","CartaId=@a AND ResponsavelId=@r AND Motivo=@m AND PrecoAnterior=40 AND PrecoNovo=40 AND EstoqueAnterior=2 AND EstoqueNovo=2 AND AtivoAnterior=1 AND AtivoNovo=0",P("@a",a),P("@r",admin),P("@m",tag));
            var total=await Count(c,"AlteracaoCarta","CartaId=@a",P("@a",a));var moves=await Count(c,"MovimentacaoEstoque","CartaId=@a",P("@a",a));
            Require(price==1 && activation==1 && total==2 && moves==1,"Auditoria sem delta divergente");Require((await Version(c,a)).SequenceEqual(nv),"No-op/conflito alterou versao");return new{a,autor=admin,motivo=tag,preco=new[]{30,40},ativo=new[]{true,false},estoque=new[]{2,2},auditorias=total,movimentosEstoque=moves};
        });
        await Case("SQL-20/S2-rollback-auditoria","Falha apos INSERT AlteracaoCarta reverte preco/token/auditoria sem backdoor demo",async()=>{
            var a=await Card();await using var c=await Open();var old=await Version(c,a);var trigger="tr_test_"+tag+"_audit";
            await Exec(c,$"CREATE TRIGGER dbo.[{trigger}] ON dbo.AlteracaoCarta AFTER INSERT AS BEGIN IF DB_NAME()='PokeVaultTests' AND TRY_CONVERT(int,SESSION_CONTEXT(N'PV_TEST_FAIL'))=1 BEGIN EXEC sys.sp_set_session_context @key=N'PV_TEST_REACHED',@value=1; THROW 51999,'TESTE_FALHA_APOS_AUDITORIA',1; END END");
            try{
                await Exec(c,"EXEC sys.sp_set_session_context @key=N'PV_TEST_FAIL',@value=1;");await Error(51999,async()=>{await StockVersion(c,a,40,2,old);});var reached=Convert.ToInt32(await Scalar(c,"SELECT SESSION_CONTEXT(N'PV_TEST_REACHED')"));
                Require(reached==1 && await Count(c,"AlteracaoCarta","CartaId=@a",P("@a",a))==0 && (await Version(c,a)).SequenceEqual(old) && Convert.ToDecimal(await Scalar(c,"SELECT Preco FROM dbo.Carta WHERE CartaId=@a",P("@a",a)))==30,"Rollback de auditoria incompleto");
                return new{a,falha=51999,pontoAposEscrita=reached,precoRevertido=30,tokenRevertido=true,auditoriaResidual=0};
            }finally{await Exec(c,$"DROP TRIGGER IF EXISTS dbo.[{trigger}]; EXEC sys.sp_set_session_context @key=N'PV_TEST_FAIL',@value=NULL; EXEC sys.sp_set_session_context @key=N'PV_TEST_REACHED',@value=NULL;");}
        });
        await Case("SQL-21/S2-limpeza-final","Zero triggers temporarias apos todos os casos SQL",async()=>{
            await using var c=await Open();var n=Convert.ToInt32(await Scalar(c,"SELECT COUNT(*) FROM sys.triggers WHERE name LIKE 'tr_test_%'"));Require(n==0,"Objeto temporario remanescente");return new{triggersTesteRestantes=n};
        });
    }
    async Task HttpAudit(Session adm,Session att,int staff)
    {
        await Case("HTTP-17/S2-auditoria-sem-delta","Atendente altera so preco/ativacao; SQL confirma autor/motivo/valores sem movimento0",async()=>{
            var a=await Card();var row=await Get(att,$"/api/v1/gestao/ofertas/{a}");var one=await Send(att,HttpMethod.Patch,$"/api/v1/gestao/ofertas/{a}",new{preco=40m,novoEstoque=2,ativo=true,motivo=tag,versaoEsperada=S(row,"versao")});Status(one.code,200);
            var two=await Send(att,HttpMethod.Patch,$"/api/v1/gestao/ofertas/{a}",new{preco=40m,novoEstoque=2,ativo=false,motivo=tag,versaoEsperada=S(one.data,"versao")});Status(two.code,200);await using var c=await Open();
            var price=await Count(c,"AlteracaoCarta","CartaId=@a AND ResponsavelId=@r AND Motivo=@m AND PrecoAnterior=30 AND PrecoNovo=40 AND EstoqueAnterior=2 AND EstoqueNovo=2 AND AtivoAnterior=1 AND AtivoNovo=1",P("@a",a),P("@r",staff),P("@m",tag));
            var active=await Count(c,"AlteracaoCarta","CartaId=@a AND ResponsavelId=@r AND Motivo=@m AND PrecoAnterior=40 AND PrecoNovo=40 AND EstoqueAnterior=2 AND EstoqueNovo=2 AND AtivoAnterior=1 AND AtivoNovo=0",P("@a",a),P("@r",staff),P("@m",tag));
            Require(price==1 && active==1 && await Count(c,"MovimentacaoEstoque","CartaId=@a",P("@a",a))==1,"Auditoria API divergente");return new{a,atendente=staff,motivo=tag,precoAnterior=30,precoNovo=40,ativoAnterior=true,ativoNovo=false,estoqueAnterior=2,estoqueNovo=2,auditorias=2,movimentosEstoque=1};
        });
    }
}
