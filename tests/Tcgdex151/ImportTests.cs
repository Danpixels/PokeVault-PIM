using System.Text.Json;
using System.Text.RegularExpressions;
using System.Net;
using PokeVault.Testes.Tcgdex;
partial class TcgSuite
{
    const string Api="/api/v1/gestao/tcgdex151";
    async Task<JsonElement> Import(Session session,bool force=false)
    {
        Status((await Send(session,HttpMethod.Post,Api+"/atualizar",new{forcarRede=force})).code,202);
        var deadline=DateTime.UtcNow.AddMinutes(15);int last=-1;
        while(DateTime.UtcNow<deadline){var state=await Get(session,Api+"/status");var done=I(state,"processados");if(done!=last){last=done;Console.WriteLine("Importacao real/processo: "+done+"/207; importados="+I(state,"importados")+"; ausentes="+I(state,"ausentes"));await File.AppendAllTextAsync(Path.Combine(Path.GetDirectoryName(Path.GetFullPath(output))!,"progresso.jsonl"),JsonSerializer.Serialize(new{utc=DateTime.UtcNow,status=state})+Environment.NewLine);}
            if(!state.GetProperty("emExecucao").GetBoolean()){await Task.Delay(250);return state;}await Task.Delay(1000);}
        throw new InvalidOperationException("Importacao ainda ativa apos15min; nao declarar concluida");
    }
    async Task ImportTests()
    {
        using var adm=new Session();using var att=new Session();using var cli=new Session();using var anon=new Session();await Login(adm,adminEmail,adminSecret);var su=await HttpUser(adm,"funcionarios");var cu=await HttpUser(adm,"clientes");await Login(att,su.email,su.secret);await Login(cli,cu.email,cu.secret);await Token(anon);
        if(Environment.GetEnvironmentVariable("PV_TCG_PHASE")=="offline"){await OfflineTests(adm,cli,cu.id);return;}
        await Case("LIVE-01/perfis-csrf-alvo","Cliente403/anonimo401/CSRF400; alvo arbitrario rejeitado400 sem importacao",async()=>{
            await using var c=await Open();var n=await Count(c,"ImportacaoTCGdex151","1=1");var b=new{forcarRede=false};Status((await Send(cli,HttpMethod.Post,Api+"/atualizar",b)).code,403);Status((await Send(anon,HttpMethod.Post,Api+"/atualizar",b)).code,401);Status((await Send(att,HttpMethod.Post,Api+"/atualizar",b,false)).code,400);Status((await Send(adm,HttpMethod.Post,Api+"/atualizar",new{forcarRede=false,setId="sv01"})).code,400);Require(await Count(c,"ImportacaoTCGdex151","1=1")==n,"Negacao iniciou importacao");return new{cliente=403,anonimo=401,csrf=400,alvoExtra=400};
        });
        await Case("LIVE-02/importacao-real-preservacao","Importacao real207 detalhes; zero mudanca nos12 hashes comerciais",async()=>{
            await using var c=await Open();var before=await BusinessSnapshot.Capture(c);var state=await Import(adm,Environment.GetEnvironmentVariable("PV_TCG_FORCE")=="1");var after=await BusinessSnapshot.Capture(c);var logical=await Count(c,"vw_BibliotecaTCGdex151","1=1");var details=await Count(c,"vw_BibliotecaTCGdex151","Detalhada=1");
            Require(before.All(x=>after[x.Key]==x.Value),"Importacao alterou dados comerciais");Require(logical==207&&details==207&&I(state,"importados")==207&&I(state,"ausentes")==0,"Importacao parcial: "+state.ToString());
            await File.WriteAllTextAsync(Path.Combine(Path.GetDirectoryName(Path.GetFullPath(output))!,"preservacao-importacao.json"),JsonSerializer.Serialize(new{antes=before,depois=after,iguais=true},new JsonSerializerOptions{WriteIndented=true}));return new{status=state,referenciasLogicas=logical,detalhadas=details,traducoes=await Count(c,"ReferenciaTCGdex","1=1"),tabelasComerciaisIguais=before.Count};
        });
        await Case("LIVE-03/repeticao-atendente","Reimportacao por atendente preserva207 refs/sem duplicar dados comerciais",async()=>{
            await using var c=await Open();var n=await Count(c,"ReferenciaTCGdex","1=1");var before=await BusinessSnapshot.Capture(c);var state=await Import(att);Require(await Count(c,"ReferenciaTCGdex","1=1")==n&&await Count(c,"vw_BibliotecaTCGdex151","1=1")==207&&I(state,"importados")==207,"Repeticao duplicou/perdeu refs");var after=await BusinessSnapshot.Capture(c);Require(before.All(x=>after[x.Key]==x.Value),"Repeticao alterou comercio");return new{status=state,linhasAntes=n,linhasDepois=await Count(c,"ReferenciaTCGdex","1=1")};
        });
        await Case("LIVE-04/biblioteca-precos-imagens","207 cards em9paginas; imagens locais HTTP e metadados de preco/origem/data medidos",async()=>{
            var refs=new List<JsonElement>();for(int page=1;page<=9;page++){var data=await Get(att,Api+"?pagina="+page);refs.AddRange(data.GetProperty("itens").EnumerateArray().Select(x=>x.Clone()));}Require(refs.Count==207&&refs.Select(x=>S(x,"id")).Distinct().Count()==207,"Paginas nao somam207");
            int images=0,missing=0,eur=0,usd=0,noPrices=0;foreach(var row in refs){if(row.TryGetProperty("imagemLocal",out var im)&&im.ValueKind==JsonValueKind.String)images++;else missing++;using var price=JsonDocument.Parse(S(row,"precosJson"));if(price.RootElement.TryGetProperty("cardmarket",out var cm)&&cm.ValueKind==JsonValueKind.Object)eur++;if(price.RootElement.TryGetProperty("tcgplayer",out var tp)&&tp.ValueKind==JsonValueKind.Object)usd++;if(price.RootElement.ValueKind!=JsonValueKind.Object||!price.RootElement.EnumerateObject().Any())noPrices++;Require(S(row,"fonteUrl").StartsWith("https://api.tcgdex.net/v2/"),"Origem incorreta");}
            var detail=await Get(att,Api+"/sv03.5-025");var img=detail.GetProperty("imagemLocal");if(img.ValueKind==JsonValueKind.String){var r=await Send(anon,HttpMethod.Get,img.GetString()!);Status(r.code,200);}var html=await Send(att,HttpMethod.Get,"/gestao/tcgdex151/sv03.5-025");Status(html.code,200);Require(html.text.Contains("EUR")&&html.text.Contains("USD")&&html.text.Contains("UTC"),"Preco/origem/data nao apresentados");return new{referencias=refs.Count,imagensLocais=images,imagensAusentes=missing,cardmarket=eur,tcgplayer=usd,semPricing=noPrices,idiomas=refs.GroupBy(x=>S(x,"idioma")).ToDictionary(x=>x.Key,x=>x.Count()),detalheHtml=200,limite="HTTP/HTML; nao visual"};
        });
        await Case("LIVE-05/fallback-imagem","Ausencia temporaria da imagem na copia de teste nao quebra detalhe; arquivo restaurado",async()=>{
            var root=Path.GetFullPath(Environment.GetEnvironmentVariable("PV_TCG_RUNTIME_ROOT")??throw new InvalidOperationException("Informar runtime de teste"));Require(root.StartsWith(Path.GetFullPath("tests/.runtime"),StringComparison.OrdinalIgnoreCase),"Runtime fora de tests");var backups=new List<(string source,string backup)>();
            try{foreach(var lang in new[]{"pt","en"}){var file=Path.GetFullPath(Path.Combine(root,"wwwroot/cache/tcgdex151/PokeVaultTests/"+lang+"/sv03.5-025.webp"));Require(file.StartsWith(root,StringComparison.OrdinalIgnoreCase),"Arquivo fora do runtime");if(File.Exists(file)){var backup=file+".hold-"+tag;File.Move(file,backup);backups.Add((file,backup));}}
                var html=await Send(att,HttpMethod.Get,"/gestao/tcgdex151/sv03.5-025");Status(html.code,200);Require(WebUtility.HtmlDecode(html.text).Contains("Imagem não disponível no cache local"),"Fallback ausente");return new{http=200,arquivosTemporariamenteRenomeados=backups.Count,fallback=true};
            }finally{foreach(var b in backups)File.Move(b.backup,b.source);}
        });
        await SqlTests();
        await Case("LIVE-06/prefill-comercio-explicito","Metadados locais; camposBRL/estoque/conservacao vazios; cria somente oferta explicitada",async()=>{
            var choice=await UnusedReference();var html=await Send(att,HttpMethod.Get,"/gestao/tcgdex151/"+choice.card+"/oferta");Status(html.code,200);Require(html.text.Contains("name=\"Preco\"")&&html.text.Contains("name=\"QuantidadeInicial\"")&&html.text.Contains("value=\"\""),"Campos comerciais nao exigidos");await using var c=await Open();var count=await Count(c,"Carta","1=1");var missingBody=await Send(att,HttpMethod.Post,Api+"/"+choice.card+"/ofertas",new{conservacao="MINT"});Status(missingBody.code,400);Require(await Count(c,"Carta","1=1")==count,"Ausencia comercial criou oferta");var r=await Send(att,HttpMethod.Post,Api+"/"+choice.card+"/ofertas",new{conservacao="MINT",preco=12.34m,quantidadeInicial=2});Status(r.code,201);var id=I(r.data,"ofertaId");Require(await Count(c,"Carta","1=1")==count+1&&await Stock(c,id)==2&&Convert.ToDecimal(await Scalar(c,"SELECT Preco FROM dbo.Carta WHERE CartaId=@id",P("@id",id)))==12.34m,"Oferta local/preco incorreto");return new{cardId=choice.card,oferta=id,precoBRL=12.34m,estoque=2,criadas=1,conservacao="MINT"};
        });
        await IdentityTests();
    }
    async Task<(string card,int reference)> UnusedReference(){await using var c=await Open();await using var cmd=new Microsoft.Data.SqlClient.SqlCommand("SELECT TOP(1) r.CardId,r.ReferenciaTCGdexId FROM dbo.vw_BibliotecaTCGdex151 r WHERE r.Detalhada=1 AND NOT EXISTS(SELECT 1 FROM dbo.Carta c WHERE c.ColecaoId=@co AND TRY_CONVERT(int,c.Numero)=TRY_CONVERT(int,r.Numero) AND c.Conservacao=N'MINT') ORDER BY r.Numero",c);cmd.Parameters.AddWithValue("@co",collection);await using var reader=await cmd.ExecuteReaderAsync();Require(await reader.ReadAsync(),"Sem referencia livre de fixture");return(reader.GetString(0),reader.GetInt32(1));}
    async Task OfflineTests(Session adm,Session cli,int user)
    {
        await Case("OFF-01/biblioteca-cache-sem-api","RedeTCG desabilitada: biblioteca/detalhe/imagem e reimportacao do cache funcionam",async()=>{
            var state=await Import(adm);Require(state.GetProperty("modoOffline").GetBoolean()&&I(state,"importados")==207,"Cache offline incompleto");var list=await Get(adm,Api);Require(I(list,"total")==207,"Biblioteca offline ausente");var card=await Get(adm,Api+"/sv03.5-025");using var anon=new Session();var image=card.GetProperty("imagemLocal");if(image.ValueKind==JsonValueKind.String)Status((await Send(anon,HttpMethod.Get,image.GetString()!)).code,200);return new{status=state,total=207,imagem= image.ValueKind==JsonValueKind.String?"local200":"fallback",modo="TcgDex:RedeHabilitada=false; nao desligou rede do SO"};
        });
        await Case("OFF-02/compra-selecao-estorno","OfflineTCG: oferta referenciada local30 + remanescente; compra/replay/estorno funcionam",async()=>{
            var current=await Get(adm,$"/api/v1/admin/clientes/{user}");await SetBalance(adm,user,100,S(current,"versao"));var choice=await UnusedReference();var created=await Send(adm,HttpMethod.Post,Api+"/"+choice.card+"/ofertas",new{conservacao="MINT",preco=30m,quantidadeInicial=2});Status(created.code,201);var card=I(created.data,"ofertaId");var remaining=await Card(20,3);await HCart(cli,card);await HCart(cli,remaining);var key=Guid.NewGuid();var payload=Purchase(key,(card,30m));var buy=await Send(cli,HttpMethod.Post,"/api/v1/compras",payload);Status(buy.code,200);Require(D(buy.data,"total")==30&&D(buy.data,"saldoAtual")==70,"Compra offline valores errados");var cart=await Get(cli,"/api/v1/carrinho");Require(cart.GetProperty("itens").GetArrayLength()==1&&I(cart.GetProperty("itens")[0],"ofertaId")==remaining,"Remanescente perdido");var repeat=await Send(cli,HttpMethod.Post,"/api/v1/compras",payload);Status(repeat.code,200);Require(repeat.data.GetProperty("repetida").GetBoolean(),"Replay nao idempotente");var id=I(buy.data,"compraId");var cancel=await Send(adm,HttpMethod.Post,$"/api/v1/compras/{id}/cancelamento",new{motivo="TesteofflineTCG"});Status(cancel.code,200);Require(D(cancel.data,"saldoAtual")==100,"Estorno offline divergente");return new{card,remaining,compra=id,key,total=30,saldoFinal=100,redeTCG=false};
        });
    }
}

