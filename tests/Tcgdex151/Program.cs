using System.Data;
using System.Text.Json;
using System.Security.Cryptography;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Identity;
using PokeVault.Testes.Tcgdex;

var output=args.FirstOrDefault()??throw new ArgumentException("Informar novo arquivo de evidencia");
if(File.Exists(output))throw new InvalidOperationException("Nao sobrescrever evidencia");
await new TcgSuite(output).Run();
partial class TcgSuite(string output)
{
    const string Cs="Server=.\\SQLEXPRESS;Database=PokeVaultTests;Integrated Security=true;Encrypt=true;TrustServerCertificate=true;Connect Timeout=5";
    readonly string tag="tcg"+Guid.NewGuid().ToString("N")[..9];
    readonly List<object> results=[];
    int admin,collection;
    string adminEmail="",adminSecret="";
    public async Task Run()
    {
        try{
            await using var c=await Open();Require(c.Database=="PokeVaultTests","Base nao exclusiva");
            Require(Convert.ToInt32(await Scalar(c,"SELECT COUNT(*) FROM sys.extended_properties WHERE class=0 AND name=N'PokeVaultProjeto' AND CONVERT(nvarchar(100),value)=N'PIM-2026-Sprint1'"))==1,"Marcador invalido");
            Require(Convert.ToInt32(await Scalar(c,"SELECT MAX(Versao) FROM dbo.SchemaVersao"))==10&&Convert.ToInt32(await Scalar(c,"SELECT COUNT(*) FROM dbo.SchemaVersao WHERE Versao IN(5,6,7)"))==0,"Requer schemaTCG10 semWIP5-7");
            collection=Convert.ToInt32(await Scalar(c,"SELECT ColecaoId FROM dbo.Colecao WHERE Codigo=N'MEW-151'"));
            adminEmail=tag+"@tests.pokevault.local";adminSecret=Convert.ToBase64String(RandomNumberGenerator.GetBytes(24));var hash=new PasswordHasher<object>().HashPassword(new(),adminSecret);
            admin=Convert.ToInt32(await Scalar(c,"INSERT dbo.Usuario(PerfilId,Nome,Email,EmailNormalizado,SenhaHash,Ativo,AdministradorFixo) OUTPUT inserted.UsuarioId SELECT PerfilId,@nome,@email,UPPER(@email),@hash,1,0 FROM dbo.Perfil WHERE Codigo=N'ADMINISTRADOR'",P("@nome",tag),P("@email",adminEmail),P("@hash",hash)));
            var phase=Environment.GetEnvironmentVariable("PV_TCG_PHASE")??"online";if(phase=="online"){await ClientFixtures();await ImporterFixture();await CacheValidationFixtures();}if(phase=="identity")await IdentityTests();else if(phase=="pricing-ui"){using var session=new Session();await Login(session,adminEmail,adminSecret);await PriceHistoryTest(session);}else await ImportTests();
        }catch(Exception e){results.Add(new{id="INFRA",status="bloqueado",observado=Safe(e)});}
        finally{Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(output))!);await File.WriteAllTextAsync(output,JsonSerializer.Serialize(new{dataUtc=DateTime.UtcNow,baseSql="PokeVaultTests",rodada=tag,resultados=results},new JsonSerializerOptions{WriteIndented=true}));Console.WriteLine("Evidencia: "+output);}
    }
    static string Safe(Exception e)=>e is SqlException s?"SqlException "+s.Number:e.Message;
    static void Require(bool ok,string message){if(!ok)throw new InvalidOperationException(message);}
    async Task Case(string id,string expected,Func<Task<object>> action){var start=DateTime.UtcNow;try{var seen=await action();results.Add(new{id,status="aprovado",inicioUtc=start,fimUtc=DateTime.UtcNow,esperado=expected,observado=seen});Console.WriteLine(id+": aprovado");}catch(Exception e){results.Add(new{id,status="reprovado",inicioUtc=start,fimUtc=DateTime.UtcNow,esperado=expected,observado=Safe(e)});Console.WriteLine(id+": reprovado: "+Safe(e));}}
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
        await using var c=await Open(); var row=await Proc(c,"dbo.usp_CadastrarCarta",P("@ResponsavelId",admin),P("@ColecaoId",collection),P("@Numero",number??Guid.NewGuid().ToString("N")[..16]),P("@Nome",tag),P("@Raridade","Teste"),P("@Conservacao",conservation),Money("@Preco",price),P("@QuantidadeInicial",stock));
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

}






