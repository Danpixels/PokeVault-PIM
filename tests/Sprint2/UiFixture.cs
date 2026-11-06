using System.Text.Json;
partial class Suite
{
    async Task PrepararUi()
    {
        using var adm=new Session();await Login(adm,adminEmail,adminSecret);
        var client=await HttpUser(adm,"clientes");var staff=await HttpUser(adm,"funcionarios");
        var row=await Get(adm,$"/api/v1/admin/clientes/{client.id}");await SetBalance(adm,client.id,100,S(row,"versao"));
        var a=await Card();var b=await Card(20,3,"SLIGHTLY_PLAYED");
        var path=Path.GetFullPath(Environment.GetEnvironmentVariable("PV_TEST_UI_FIXTURE")!);
        var root=Path.GetFullPath("tests/.runtime")+Path.DirectorySeparatorChar;
        Require(path.StartsWith(root,StringComparison.OrdinalIgnoreCase)&&!File.Exists(path),"Arquivo temporario UI deve ser novo e ficar em tests/.runtime");
        await File.WriteAllTextAsync(path,JsonSerializer.Serialize(new {admin=new{email=adminEmail,senha=adminSecret},cliente=new{email=client.email,senha=client.secret},atendente=new{email=staff.email,senha=staff.secret}}));
        results.Add(new{id="UI-PREPARACAO",status="aprovado",esperado="Contas proprias para revisao visual; nenhum segredo na evidencia",observado=new{adminId=admin,clienteId=client.id,atendenteId=staff.id,clienteEmail=client.email,atendenteEmail=staff.email,filtroCatalogo=tag,cartas=new[]{a,b},saldoCliente=100,limite="Arquivo de credenciais temporario fora das evidencias; remover apos carregar em memoria"}});
    }
}
