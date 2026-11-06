using System.Data;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;

var output = args.FirstOrDefault() ?? throw new ArgumentException("Informar caminho de evidencia nova.");
if (File.Exists(output)) throw new InvalidOperationException("Nao sobrescrever evidencia.");
var suite = new Suite(output);
await suite.Run();

partial class Suite(string output)
{
    const string Cs = "Server=.\\SQLEXPRESS;Database=PokeVaultTests;Integrated Security=true;Encrypt=true;TrustServerCertificate=true;Connect Timeout=5";
    readonly string tag = "cw" + Guid.NewGuid().ToString("N")[..10];
    readonly List<object> results = [];
    int admin, collection;
    string adminEmail="",adminSecret="";
    static readonly JsonSerializerOptions Json = new() { WriteIndented = true };
    public async Task Run()
    {
        try
        {
            await using var c = await Open();
            var marked = await Scalar(c, "SELECT COUNT(*) FROM sys.extended_properties WHERE class=0 AND name=N'PokeVaultProjeto' AND CONVERT(nvarchar(100),value)=N'PIM-2026-Sprint1'");
            Require(Convert.ToInt32(marked) == 1 && c.Database == "PokeVaultTests", "Base exclusiva/marcador invalidos");
            Require(Convert.ToInt32(await Scalar(c,"SELECT COUNT(*) FROM dbo.SchemaVersao WHERE Versao IN(1,2,3,4,5,6,7)"))==7,"Requer schema1-7 e liberacao de Banco antes de fixture mutante");
            collection = Convert.ToInt32(await Scalar(c, "SELECT ColecaoId FROM dbo.Colecao WHERE Codigo=N'MEW-151'"));
            var pass = Convert.ToBase64String(RandomNumberGenerator.GetBytes(24));
            adminSecret=pass;adminEmail=tag+"@tests.pokevault.local";
            var hash = new PasswordHasher<object>().HashPassword(new(), pass);
            admin = Convert.ToInt32(await Scalar(c, "INSERT dbo.Usuario(PerfilId,Nome,Email,EmailNormalizado,SenhaHash,Ativo,AdministradorFixo) OUTPUT inserted.UsuarioId SELECT PerfilId,@nome,@email,UPPER(@email),@hash,1,CASE WHEN EXISTS(SELECT 1 FROM dbo.Usuario WHERE AdministradorFixo=1) THEN 0 ELSE 1 END FROM dbo.Perfil WHERE Codigo=N'ADMINISTRADOR'", P("@nome", tag), P("@email", tag+"@tests.pokevault.local"), P("@hash", hash)));
            await Core();
            await Complemento();
        }
        catch (Exception e) { results.Add(new { id="INFRA", status="bloqueado", observado=Safe(e) }); }
        finally
        {
            var evidence = new { dataUtc=DateTime.UtcNow, fuso="America/Sao_Paulo", baseSql="PokeVaultTests", instancia=".\\SQLEXPRESS", rodada=tag, executor="Codex com apoio de IA", resultados=results };
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(output))!);
            await File.WriteAllTextAsync(output, JsonSerializer.Serialize(evidence, Json));
            Console.WriteLine($"Evidencia salva: {output}; registros={results.Count}");
        }
    }
    static string Safe(Exception e) => e is SqlException s ? $"SqlException {s.Number}" : e.Message;
    static void Require(bool ok,string message) { if(!ok) throw new InvalidOperationException(message); }
    async Task Case(string id,string expected,Func<Task<object>> run)
    {
        try { var seen=await run(); results.Add(new { id,status="aprovado",esperado=expected,observado=seen }); Console.WriteLine(id+": aprovado"); }
        catch(Exception e) { results.Add(new { id,status="reprovado",esperado=expected,observado=Safe(e) }); Console.WriteLine(id+": reprovado: "+Safe(e)); }
    }
    static SqlParameter P(string n, object v) => new(n,v);
    static SqlParameter Money(string n, decimal v) => new(n,SqlDbType.Decimal) { Precision=12,Scale=2,Value=v };
    static async Task<SqlConnection> Open() { var c=new SqlConnection(Cs); await c.OpenAsync(); return c; }
    static async Task<object?> Scalar(SqlConnection c,string sql,params SqlParameter[] ps)
    { await using var cmd=new SqlCommand(sql,c); cmd.Parameters.AddRange(ps); return await cmd.ExecuteScalarAsync(); }
    static async Task Exec(SqlConnection c,string sql,params SqlParameter[] ps) { await using var cmd=new SqlCommand(sql,c); cmd.Parameters.AddRange(ps); await cmd.ExecuteNonQueryAsync(); }
    static async Task<Dictionary<string,object?>> Proc(SqlConnection c,string name,params SqlParameter[] ps)
    {
        await using var cmd=new SqlCommand(name,c) { CommandType=CommandType.StoredProcedure,CommandTimeout=20 }; cmd.Parameters.AddRange(ps);
        await using var r=await cmd.ExecuteReaderAsync(); var row=new Dictionary<string,object?>();
        if(await r.ReadAsync()) for(int i=0;i<r.FieldCount;i++) row[r.GetName(i)]=r.IsDBNull(i)?null:r.GetValue(i);
        return row;
    }
    async Task<int> User(decimal saldo=100m,string perfil="CLIENTE")
    {
        await using var c=await Open(); var email=tag+"-"+Guid.NewGuid().ToString("N")[..8]+"@tests.pokevault.local";
        var hash=new PasswordHasher<object>().HashPassword(new(),Convert.ToBase64String(RandomNumberGenerator.GetBytes(24)));
        var row=await Proc(c,"dbo.usp_CriarUsuario",P("@AdministradorId",admin),P("@Nome",tag),P("@Email",email),P("@EmailNormalizado",email.ToUpperInvariant()),P("@SenhaHash",hash),P("@PerfilCodigo",perfil));
        var id=Convert.ToInt32(row["UsuarioId"]);
        if(perfil=="CLIENTE") await Balance(c,id,saldo);
        return id;
    }
    async Task Balance(SqlConnection c,int id,decimal value) => _=await Proc(c,"dbo.usp_DefinirSaldo",P("@AdministradorId",admin),P("@ClienteId",id),Money("@NovoSaldo",value),P("@Motivo",tag));
    async Task<int> Card(decimal price=30m,int stock=2,string conservation="NEAR_MINT",string? number=null)
    {
        await using var c=await Open(); var row=await Proc(c,"dbo.usp_CadastrarCarta",P("@ResponsavelId",admin),P("@ColecaoId",collection),P("@Numero",number??Guid.NewGuid().ToString("N")[..16]),P("@Nome",tag),P("@Raridade","Common"),P("@Conservacao",conservation),Money("@Preco",price),P("@QuantidadeInicial",stock));
        return Convert.ToInt32(row["CartaId"]);
    }
    static async Task Cart(SqlConnection c,int user,int card,int qty=1) => _=await Proc(c,"dbo.usp_DefinirItemCarrinho",P("@ClienteId",user),P("@CartaId",card),P("@Quantidade",qty));
    static async Task<Dictionary<string,object?>> Buy(SqlConnection c,int user,Guid key,params (int id,int qty,decimal price)[] items)
    {
        var t=new DataTable(); t.Columns.Add("CartaId",typeof(int));t.Columns.Add("Quantidade",typeof(int));t.Columns.Add("PrecoConfirmado",typeof(decimal));
        foreach(var i in items)t.Rows.Add(i.id,i.qty,i.price);
        return await Proc(c,"dbo.usp_ConcluirCompra",P("@ClienteId",user),P("@ChaveOperacao",key),new SqlParameter("@Itens",SqlDbType.Structured){TypeName="dbo.ItemCompraSelecionadoType",Value=t});
    }
    async Task<Dictionary<string,object?>> Cancel(SqlConnection c,int id,string? motivo=null) => await Proc(c,"dbo.usp_CancelarCompra",P("@AdministradorId",admin),P("@CompraId",id),P("@Motivo",motivo??tag));
    static async Task Error(int number,Func<Task> action) { try { await action(); } catch(SqlException s) when(s.Number==number) { return; } throw new InvalidOperationException("Esperado erro SQL "+number); }
    static async Task<decimal> Saldo(SqlConnection c,int user) => Convert.ToDecimal(await Scalar(c,"SELECT Saldo FROM dbo.SaldoCliente WHERE UsuarioId=@id",P("@id",user)));
    static async Task<int> Stock(SqlConnection c,int card) => Convert.ToInt32(await Scalar(c,"SELECT QuantidadeEstoque FROM dbo.Carta WHERE CartaId=@id",P("@id",card)));
    static async Task<int> Count(SqlConnection c,string table,string condition,params SqlParameter[] ps) => Convert.ToInt32(await Scalar(c,$"SELECT COUNT(*) FROM dbo.{table} WHERE {condition}",ps));
    static async Task<int[]> Pair(Func<SqlConnection,Task> a,Func<SqlConnection,Task> b)
    {
        await using var ca=await Open();await using var cb=await Open();
        var start=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        async Task<int> Run(SqlConnection c,Func<SqlConnection,Task> job) { await start.Task;try {await job(c);return 0;}catch(SqlException e){return e.Number;} }
        var ta=Run(ca,a);var tb=Run(cb,b);start.SetResult();return await Task.WhenAll(ta,tb);
    }
    async Task Core()
    {
        await Case("SQL-01/T10,T08","Compra NM30; SP e B remanescentes; saldo70",async()=>{
            var u=await User();var number=Guid.NewGuid().ToString("N")[..16];var a=await Card(number:number);var sp=await Card(20,3,"SLIGHTLY_PLAYED",number);var b=await Card(15.50m,1);
            await using var c=await Open();foreach(var id in new[]{a,sp,b})await Cart(c,u,id);
            var r=await Buy(c,u,Guid.NewGuid(),(a,1,30m));
            Require(await Saldo(c,u)==70 && await Stock(c,a)==1 && await Stock(c,sp)==3 && await Stock(c,b)==1,"Saldo/estoque divergente");
            Require(await Count(c,"ItemCarrinho","CarrinhoId=(SELECT CarrinhoId FROM dbo.Carrinho WHERE UsuarioId=@u)",P("@u",u))==2,"Carrinho remanescente divergente");
            return new {u,a,sp,b,compra=r};
        });
        await Case("SQL-02/T13,T14,T15","Recusas 51008/51009 sem debito/compra/limpeza parcial",async()=>{
            var u=await User(10);var a=await Card();var b=await Card(15.5m,1);await using var c=await Open();await Cart(c,u,a);await Cart(c,u,b,2);
            await Error(51008,async()=>{await Buy(c,u,Guid.NewGuid(),(a,1,30m),(b,2,15.5m));});
            await Error(51009,async()=>{await Buy(c,u,Guid.NewGuid(),(a,1,30m));});
            Require(await Saldo(c,u)==10 && await Stock(c,a)==2 && await Stock(c,b)==1 && await Count(c,"Compra","ClienteId=@u",P("@u",u))==0,"Recusa teve efeitos");
            Require(await Count(c,"ItemCarrinho","CarrinhoId=(SELECT CarrinhoId FROM dbo.Carrinho WHERE UsuarioId=@u)",P("@u",u))==2,"Carrinho alterado");return new {u,a,b,erros=new[]{51008,51009}};
        });
        await Case("SQL-03/T27","Preco30→40 recusa51007, depois novo aceite40",async()=>{
            var u=await User();var a=await Card();await using var c=await Open();await Cart(c,u,a);await Adjust(c,a,40,2);
            await Error(51007,async()=>{await Buy(c,u,Guid.NewGuid(),(a,1,30m));});Require(await Saldo(c,u)==100 && await Stock(c,a)==2,"Preco recusado teve efeitos");
            var r=await Buy(c,u,Guid.NewGuid(),(a,1,40m));Require(await Saldo(c,u)==60,"Novo aceite diverge");return new {u,a,compra=r};
        });
        await Case("SQL-04/T28,T29,T19","UUID retorna mesma compra apos limpeza/cancelamento; divergente51004",async()=>{
            var u=await User();var a=await Card();await using var c=await Open();await Cart(c,u,a);var key=Guid.NewGuid();var one=await Buy(c,u,key,(a,1,30m));var two=await Buy(c,u,key,(a,1,30m));var id=Convert.ToInt32(one["CompraId"]);
            Require(Convert.ToInt32(two["CompraId"])==id && (bool)two["Repetida"]!,"Reenvio diferente");await Error(51004,async()=>{await Buy(c,u,key,(a,1,31m));});
            await Cancel(c,id,"original");await Cancel(c,id,"outro");var after=await Buy(c,u,key,(a,1,30m));
            Require((string)after["Status"]! =="CANCELADO" && await Saldo(c,u)==100 && await Stock(c,a)==2,"Reenvio reativou/duplicou");
            Require(await Count(c,"MovimentacaoSaldo","CompraId=@id AND Tipo='ESTORNO'",P("@id",id))==1,"Estorno duplicado");return new {u,a,key,compra=id,reenvio=after};
        });
        await Case("SQL-05/T21","Ultima unidade duas conexoes; uma aprova outra51008",async()=>{
            var u=await User();var v=await User();var a=await Card(15.5m,1);await using var c=await Open();await Cart(c,u,a);await Cart(c,v,a);
            var codes=await Pair(async x=>{await Buy(x,u,Guid.NewGuid(),(a,1,15.5m));},async x=>{await Buy(x,v,Guid.NewGuid(),(a,1,15.5m));});
            Require(codes.Count(x=>x==0)==1 && codes.Count(x=>x==51008)==1 && await Stock(c,a)==0 && await Saldo(c,u)+await Saldo(c,v)==184.5m,"Concorrencia ultima unidade divergente");return new {u,v,a,codes,saldos=new[]{await Saldo(c,u),await Saldo(c,v)}};
        });
        await Case("SQL-06/T22","Saldo30 disputado por duas cartas30; um51009",async()=>{
            var u=await User(30);var a=await Card();var b=await Card();await using var c=await Open();await Cart(c,u,a);await Cart(c,u,b);
            var codes=await Pair(async x=>{await Buy(x,u,Guid.NewGuid(),(a,1,30m));},async x=>{await Buy(x,u,Guid.NewGuid(),(b,1,30m));});
            Require(codes.Count(x=>x==0)==1 && codes.Count(x=>x==51009)==1 && await Saldo(c,u)==0 && await Stock(c,a)+await Stock(c,b)==3,"Concorrencia saldo divergente");return new {u,a,b,codes,saldo=await Saldo(c,u)};
        });
        await Case("SQL-07/T30","Mesmo UUID simultaneo cria uma compra e um debito",async()=>{
            var u=await User();var a=await Card();await using var c=await Open();await Cart(c,u,a);var key=Guid.NewGuid();
            var codes=await Pair(async x=>{await Buy(x,u,key,(a,1,30m));},async x=>{await Buy(x,u,key,(a,1,30m));});
            Require(codes.All(x=>x==0) && await Saldo(c,u)==70 && await Stock(c,a)==1 && await Count(c,"Compra","ClienteId=@u",P("@u",u))==1,"UUID concorrente duplicado");return new {u,a,key,codes};
        });
        await Case("SQL-08/T20","Cancelamento simultaneo um estorno efetivo",async()=>{
            var u=await User();var a=await Card();await using var c=await Open();await Cart(c,u,a);var r=await Buy(c,u,Guid.NewGuid(),(a,1,30m));var id=Convert.ToInt32(r["CompraId"]);
            var codes=await Pair(async x=>{await Cancel(x,id);},async x=>{await Cancel(x,id);});
            Require(codes.All(x=>x==0) && await Saldo(c,u)==100 && await Stock(c,a)==2 && await Count(c,"MovimentacaoSaldo","CompraId=@id AND Tipo='ESTORNO'",P("@id",id))==1 && await Count(c,"MovimentacaoEstoque","CompraId=@id AND Tipo='ESTORNO'",P("@id",id))==1,"Estorno concorrente duplicado");return new {u,a,compra=id,codes};
        });
        await Case("SQL-09/T25,T35","Estorno30 original apos preco40 e saldo definido40 resulta70",async()=>{
            var u=await User();var a=await Card();await using var c=await Open();await Cart(c,u,a);var r=await Buy(c,u,Guid.NewGuid(),(a,1,30m));var id=Convert.ToInt32(r["CompraId"]);
            await Adjust(c,a,40,1);await Balance(c,u,40);await Cancel(c,id);
            Require(await Saldo(c,u)==70 && await Stock(c,a)==2,"Estorno usou preco/saldo errado");return new {u,a,compra=id,saldo=await Saldo(c,u)};
        });
        await Case("SQL-10/T16","Quantidade parcial da linha recusada51005",async()=>{
            var u=await User();var a=await Card();await using var c=await Open();await Cart(c,u,a,2);await Error(51005,async()=>{await Buy(c,u,Guid.NewGuid(),(a,1,30m));});Require(await Saldo(c,u)==100 && await Stock(c,a)==2,"Recusa parcial alterou estado");return new {u,a,erro=51005};
        });
        await RollbackTests();
    }
    // Atualizacoes de fixture usam o contrato disponivel, sem duplicar DML financeiro.
    static async Task AddVersion(SqlConnection c,List<SqlParameter> ps,string proc,int id,bool saldo=false)
    {
        var has=Convert.ToInt32(await Scalar(c,"SELECT COUNT(*) FROM sys.parameters WHERE object_id=OBJECT_ID(@p) AND name IN ('@VersaoEsperada','@VersaoSaldoEsperada','@VersaoEstoqueEsperada')",P("@p",proc)));
        if(has==0)return;
        var name=Convert.ToString(await Scalar(c,"SELECT TOP(1) name FROM sys.parameters WHERE object_id=OBJECT_ID(@p) AND name LIKE '%Versao%Esperada%'",P("@p",proc)))!;
        var table=saldo?"SaldoCliente":"Carta";var key=saldo?"UsuarioId":"CartaId";
        var column=Convert.ToString(await Scalar(c,$"SELECT TOP(1) name FROM sys.columns WHERE object_id=OBJECT_ID('dbo.{table}') AND system_type_id=189"))!;
        if(string.IsNullOrEmpty(column))throw new InvalidOperationException("Versao esperada sem rowversion publicado");
        var val=await Scalar(c,$"SELECT [{column}] FROM dbo.{table} WHERE {key}=@id",P("@id",id));
        ps.Add(new SqlParameter(name,SqlDbType.Binary,8){Value=val});
    }
    async Task Adjust(SqlConnection c,int id,decimal price,int stock)
    {
        var ps=new List<SqlParameter>{P("@ResponsavelId",admin),P("@CartaId",id),Money("@Preco",price),P("@NovoEstoque",stock),P("@Ativo",true),P("@Motivo",tag)};
        await AddVersion(c,ps,"dbo.usp_AjustarCarta",id);await Proc(c,"dbo.usp_AjustarCarta",ps.ToArray());
    }
    async Task RollbackTests()
    {
        await Case("SQL-11/T23-compra","Falha51999 apos movimentos/baixas; rollback integral sem transacao externa",async()=>await Rollback(false));
        await Case("SQL-12/T23-estorno","Falha51999 apos credito/reposicao; rollback integral e posterior sucesso",async()=>await Rollback(true));
    }
    async Task<object> Rollback(bool cancel)
    {
        var u=await User();var a=await Card();await using var c=await Open();await Cart(c,u,a);var key=Guid.NewGuid();int id=0;
        if(cancel)id=Convert.ToInt32((await Buy(c,u,key,(a,1,30m)))["CompraId"]);
        string trigger="tr_test_"+tag+(cancel?"_cancel":"_buy");
        await Exec(c,$"CREATE TRIGGER dbo.[{trigger}] ON dbo.MovimentacaoEstoque AFTER INSERT AS BEGIN SET NOCOUNT ON; IF DB_NAME()=N'PokeVaultTests' AND TRY_CONVERT(int,SESSION_CONTEXT(N'PV_TEST_FAIL'))=1 AND EXISTS(SELECT 1 FROM inserted WHERE Tipo=N'{(cancel?"ESTORNO":"COMPRA")}') BEGIN EXEC sys.sp_set_session_context @key=N'PV_TEST_REACHED',@value=1; THROW 51999,'TESTE_FALHA_APOS_ESCRITA',1; END END");
        try
        {
            await Exec(c,"EXEC sys.sp_set_session_context @key=N'PV_TEST_FAIL',@value=1;");
            await Error(51999,async()=>{if(cancel)await Cancel(c,id);else await Buy(c,u,key,(a,1,30m));});
            var reached=Convert.ToInt32(await Scalar(c,"SELECT SESSION_CONTEXT(N'PV_TEST_REACHED')"));Require(reached==1,"Nao comprovou ponto apos escrita");
            Require(await Saldo(c,u)==(cancel?70:100) && await Stock(c,a)==(cancel?1:2),"Rollback saldo/estoque incompleto");
            Require(await Count(c,"MovimentacaoEstoque","CompraId IN(SELECT CompraId FROM dbo.Compra WHERE ClienteId=@u) AND Tipo=@tipo",P("@u",u),P("@tipo",cancel?"ESTORNO":"COMPRA"))==0,"Rollback movimentos incompleto");
            Require(await Count(c,"MovimentacaoSaldo","UsuarioId=@u AND Tipo=@tipo",P("@u",u),P("@tipo",cancel?"ESTORNO":"COMPRA"))==0,"Rollback movimentos de saldo incompleto");
            if(cancel) Require((string)(await Scalar(c,"SELECT Status FROM dbo.Compra WHERE CompraId=@id",P("@id",id)))! =="CONCLUIDO","Status nao revertido");
            else Require(await Count(c,"Compra","ClienteId=@u",P("@u",u))==0 && await Count(c,"ItemCarrinho","CartaId=@a AND CarrinhoId=(SELECT CarrinhoId FROM dbo.Carrinho WHERE UsuarioId=@u)",P("@a",a),P("@u",u))==1,"Compra/carrinho nao revertidos");
            await Exec(c,"EXEC sys.sp_set_session_context @key=N'PV_TEST_FAIL',@value=NULL;");
            if(cancel)await Cancel(c,id);else await Buy(c,u,key,(a,1,30m));
            return new {u,a,compra=id,falha=51999,pontoAposEscrita=reached,transacaoExterna=false,recuperacao="sucesso apos retirar flag"};
        }
        finally { await Exec(c,$"DROP TRIGGER IF EXISTS dbo.[{trigger}]; EXEC sys.sp_set_session_context @key=N'PV_TEST_FAIL',@value=NULL; EXEC sys.sp_set_session_context @key=N'PV_TEST_REACHED',@value=NULL;"); }
    }
    private partial Task Complemento();


}


