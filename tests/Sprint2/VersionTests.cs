using System.Data;
using Microsoft.Data.SqlClient;

partial class Suite
{
    static async Task<byte[]> Version(SqlConnection c,int id,bool saldo=false) => (byte[])(await Scalar(c,$"SELECT Versao FROM dbo.{(saldo?"SaldoCliente":"Carta")} WHERE {(saldo?"UsuarioId":"CartaId")}=@id",P("@id",id)))!;
    async Task<Dictionary<string,object?>> BalanceVersion(SqlConnection c,int user,decimal saldo,byte[] version) => await Proc(c,"dbo.usp_DefinirSaldoVersionado",P("@AdministradorId",admin),P("@ClienteId",user),Money("@NovoSaldo",saldo),P("@Motivo",tag),new SqlParameter("@VersaoEsperada",SqlDbType.Binary,8){Value=version});
    async Task<Dictionary<string,object?>> StockVersion(SqlConnection c,int card,decimal price,int stock,byte[] version) => await Proc(c,"dbo.usp_AjustarCartaVersionado",P("@ResponsavelId",admin),P("@CartaId",card),Money("@Preco",price),P("@NovoEstoque",stock),P("@Ativo",true),P("@Motivo",tag),new SqlParameter("@VersaoEsperada",SqlDbType.Binary,8){Value=version});
    private partial async Task Versions()
    {
        await Case("SQL-13/S2-saldo","Definir100→40 delta-60; repetir40 mesma versao sem UPDATE/movimento",async()=>{
            var u=await User();await using var c=await Open();var old=await Version(c,u,true);var r=await BalanceVersion(c,u,40,old);var next=(byte[])r["Versao"]!;var count=await Count(c,"MovimentacaoSaldo","UsuarioId=@u",P("@u",u));var again=await BalanceVersion(c,u,40,next);
            Require(await Saldo(c,u)==40 && Convert.ToDecimal(r["Delta"])==-60 && Convert.ToDecimal(again["Delta"])==0 && next.SequenceEqual((byte[])again["Versao"]!) && count==await Count(c,"MovimentacaoSaldo","UsuarioId=@u",P("@u",u)),"Saldo/no-op divergente");return new {u,antes=100,depois=40,delta=-60,semDuplicacao=true};
        });
        await Case("SQL-14/S2-saldo-venda","Venda invalida formulario saldo;51012 preserva70",async()=>{
            var u=await User();var a=await Card();await using var c=await Open();var old=await Version(c,u,true);await Cart(c,u,a);await Buy(c,u,Guid.NewGuid(),(a,1,30m));
            await Error(51012,async()=>{await BalanceVersion(c,u,150,old);});Require(await Saldo(c,u)==70,"Sobrescreveu venda");return new {u,a,saldo=70,erro=51012};
        });
        await Case("SQL-15/S2-estoque-venda","Venda invalida formulario estoque;51012 preserva1",async()=>{
            var u=await User();var a=await Card();await using var c=await Open();var old=await Version(c,a);await Cart(c,u,a);await Buy(c,u,Guid.NewGuid(),(a,1,30m));
            await Error(51012,async()=>{await StockVersion(c,a,30,10,old);});Require(await Stock(c,a)==1,"Sobrescreveu estoque vendido");return new {u,a,estoque=1,erro=51012};
        });
        await Case("SQL-16/S2-ajustes-concorrentes","Mesmo rowversion em dois ajustes estoque: um51012",async()=>{
            var a=await Card();await using var c=await Open();var old=await Version(c,a);
            var codes=await Pair(async x=>{await StockVersion(x,a,30,5,old);},async x=>{await StockVersion(x,a,30,9,old);});var stock=await Stock(c,a);
            Require(codes.Count(x=>x==0)==1 && codes.Count(x=>x==51012)==1 && (stock==5 || stock==9),"Edicao concorrente perdeu ajuste");return new {a,codes,estoque=stock};
        });
        await Case("SQL-17/S2-runtime","EXECUTE AS usuario restrito permite SELECT/EXEC versionado e nega DML/legado",async()=>{
            await using var c=await Open();
            await Exec(c,"EXECUTE AS USER=N'PokeVaultRuntimeVerificacao';");
            try {
                var select=Convert.ToInt32(await Scalar(c,"SELECT HAS_PERMS_BY_NAME('dbo.Carta','OBJECT','SELECT')"));var exec=Convert.ToInt32(await Scalar(c,"SELECT HAS_PERMS_BY_NAME('dbo.usp_AjustarCartaVersionado','OBJECT','EXECUTE')"));
                var write=Convert.ToInt32(await Scalar(c,"SELECT HAS_PERMS_BY_NAME('dbo.Carta','OBJECT','UPDATE')"));var legacy=Convert.ToInt32(await Scalar(c,"SELECT HAS_PERMS_BY_NAME('dbo.usp_AjustarCarta','OBJECT','EXECUTE')"));
                Require(select==1 && exec==1 && write==0 && legacy==0,"Permissoes runtime divergentes");
                await Error(229,async()=>{await Exec(c,"UPDATE dbo.Carta SET Preco=Preco WHERE 1=0;");});
                return new {select,exec,write,legacy,nivel="principal de banco via impersonacao; nao conexao externa runtime"};
            } finally { await Exec(c,"REVERT;"); }
        });
        await Case("SQL-18/T04-limpeza","ADM fixo protegido51011 e zero triggers de falha remanescentes",async()=>{
            await using var c=await Open();var id=Convert.ToInt32(await Scalar(c,"SELECT UsuarioId FROM dbo.Usuario WHERE AdministradorFixo=1"));
            await Error(51011,async()=>{await Proc(c,"dbo.usp_DefinirUsuarioAtivo",P("@AdministradorId",admin),P("@UsuarioId",id),P("@Ativo",false));});
            var triggers=Convert.ToInt32(await Scalar(c,"SELECT COUNT(*) FROM sys.triggers WHERE name LIKE 'tr_test_%'"));Require(triggers==0,"Trigger de falha nao removida");return new{administradorFixo=id,erro=51011,triggersTesteRestantes=triggers};
        });
    }
}
