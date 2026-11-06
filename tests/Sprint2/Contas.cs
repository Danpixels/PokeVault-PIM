using System.Net;
using Microsoft.AspNetCore.Identity;
partial class Suite
{
    async Task Contas()
    {
        using var anon=new Session();using var conta=new Session();using var antiga=new Session();using var adm=new Session();
        await Token(anon);await Token(conta);await Login(adm,adminEmail,adminSecret);
        var email=tag+"-cadastro@tests.pokevault.local";
        var inicial=Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(18));
        var nova=Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(18));
        await using var c=await Open();int id=0;
        await Case("CONTA-01","Cadastro público com email/senha/olho; Minha conta exige login",async()=>{
            var html=await Send(anon,HttpMethod.Get,"/criar-conta");Status(html.code,200);Require(html.text.Contains("name=\"Email\"")&&html.text.Contains("name=\"Senha\"")&&html.text.Contains("data-password-toggle")&&!html.text.Contains("name=\"Perfil\""),"Formulário divergente");
            Status((await Send(anon,HttpMethod.Get,"/conta")).code,302);return new{cadastro=200,contaProtegida=302};
        });
        await Case("CONTA-02","Cadastro sem CSRF e entradas inválidas não criam usuários nem exibem senha",async()=>{
            Status((await Form(anon,"/criar-conta",new(){["Email"]=email,["Senha"]=inicial},false)).code,400);
            var ruim=await Form(anon,"/criar-conta",new(){["Email"]="email-invalido",["Senha"]="abc"});Status(ruim.code,200);
            Require(!ruim.html.Contains("value=\"abc\"")&&await Count(c,"Usuario","EmailNormalizado=@e",P("@e",email.ToUpperInvariant()))==0,"Entrada inválida persistida/exposta");return new{csrf=400,validacao=200,usuariosCriados=0};
        });
        await Case("CONTA-03","Cadastro sempre CLIENTE com saldo0/carrinho vazio; injeção de perfil/ID/saldo ignorada",async()=>{
            var criado=await Form(conta,"/criar-conta",new(){["Email"]=email,["Senha"]=inicial,["Perfil"]="ADMINISTRADOR",["PerfilCodigo"]="ATENDENTE",["UsuarioId"]=admin.ToString(),["Saldo"]="999",["Ativo"]="false"});Status(criado.code,302);
            var identidade=await Get(conta,"/api/v1/sessao");id=I(identidade,"id");Require(S(identidade,"perfil")=="CLIENTE"&&id!=admin,"Escalada de privilégio");
            var cart=await Get(conta,"/api/v1/carrinho");Require(D(cart,"saldo")==0&&cart.GetProperty("itens").GetArrayLength()==0,"Cliente não iniciou vazio");
            var hash=Convert.ToString(await Scalar(c,"SELECT SenhaHash FROM dbo.Usuario WHERE UsuarioId=@u",P("@u",id)))!;
            Require(hash!=inicial&&new PasswordHasher<object>().VerifyHashedPassword(new(),hash,inicial)!=PasswordVerificationResult.Failed,"Senha sem hash válido");
            Require(!criado.html.Contains(inicial),"Senha exposta");return new{id,perfil="CLIENTE",saldo=0,carrinho=0,hashVerificado=true};
        });
        if(id==0)throw new InvalidOperationException("Cadastro bloqueou cenários dependentes");
        await Case("CONTA-04","Email normalizado duplicado não cria segunda conta",async()=>{
            var dup=await Form(anon,"/criar-conta",new(){["Email"]=email.ToUpperInvariant(),["Senha"]=inicial});Status(dup.code,409);
            Require(await Count(c,"Usuario","EmailNormalizado=@e",P("@e",email.ToUpperInvariant()))==1,"Duplicou email");Require(!dup.html.Contains(inicial),"Senha retornada no erro");return new{duplicidade=409,contas=1};
        });
        await Token(conta);await Login(antiga,email,inicial);
        Dictionary<string,string> Troca(string atual,string senha,string confirmacao)=>new(){["SenhaAtual"]=atual,["NovaSenha"]=senha,["Confirmacao"]=confirmacao};
        await Case("CONTA-05","Senha atual errada, confirmação divergente e senha curta não alteram hash",async()=>{
            var antes=Convert.ToString(await Scalar(c,"SELECT SenhaHash FROM dbo.Usuario WHERE UsuarioId=@u",P("@u",id)));
            Status((await Form(conta,"/conta/senha",Troca("invalida",nova,nova))).code,400);
            var divergente=await Form(conta,"/conta/senha",Troca(inicial,nova,"diferente"));Status(divergente.code,200);
            Status((await Form(conta,"/conta/senha",Troca(inicial,"abc","abc"))).code,200);
            Require(!divergente.html.Contains(inicial)&&!divergente.html.Contains(nova),"Senhas retornadas na validação");
            Require(antes==Convert.ToString(await Scalar(c,"SELECT SenhaHash FROM dbo.Usuario WHERE UsuarioId=@u",P("@u",id))),"Erro alterou senha");return new{senhaErrada=400,confirmacaoRecusada=true,senhaCurtaRecusada=true,hashPreservado=true};
        });
        await Case("CONTA-06","Troca sem CSRF recusada; formulário Minha conta com três controles de olho",async()=>{
            Status((await Form(conta,"/conta/senha",Troca(inicial,nova,nova),false)).code,400);
            var html=await Send(conta,HttpMethod.Get,"/conta");Status(html.code,200);Require(System.Text.RegularExpressions.Regex.Matches(html.text,"data-password-toggle=").Count==3,"Faltam controles de senha");return new{csrf=400,campos=3};
        });
        await Case("CONTA-07","Troca própria ignora ID de outro usuário e preserva papel/saldo; outras sessões perdem acesso",async()=>{
            var hashAdmin=Convert.ToString(await Scalar(c,"SELECT SenhaHash FROM dbo.Usuario WHERE UsuarioId=@u",P("@u",admin)));
            var campos=Troca(inicial,nova,nova);campos["UsuarioId"]=admin.ToString();campos["Perfil"]="ADMINISTRADOR";
            Status((await Form(conta,"/conta/senha",campos)).code,302);
            Status((await Send(antiga,HttpMethod.Get,"/api/v1/sessao")).code,401);
            Require(S(await Get(conta,"/api/v1/sessao"),"perfil")=="CLIENTE"&&D(await Get(conta,"/api/v1/carrinho"),"saldo")==0,"Troca mudou papel/saldo");
            Require(hashAdmin==Convert.ToString(await Scalar(c,"SELECT SenhaHash FROM dbo.Usuario WHERE UsuarioId=@u",P("@u",admin))),"ID injetado mudou outra conta");return new{troca=302,outraSessao=401,contaAlheiaPreservada=true};
        });
        await Case("CONTA-08","Login antigo falha e senha nova autentica após sair",async()=>{
            Status((await Send(conta,HttpMethod.Delete,"/api/v1/sessao")).code,204);await Token(conta);
            Status((await Send(conta,HttpMethod.Post,"/api/v1/sessao",new{email,senha=inicial})).code,401);
            await Login(conta,email,nova);Require(I(await Get(conta,"/api/v1/sessao"),"id")==id,"Senha nova não autentica a mesma conta");return new{antiga=401,nova=200,id};
        });
        await Case("CONTA-09","Administrador e atendente também alteram a própria senha",async()=>{
            var senhaAdm=Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(18));
            Status((await Form(adm,"/conta/senha",Troca(adminSecret,senhaAdm,senhaAdm))).code,302);Require(S(await Get(adm,"/api/v1/sessao"),"perfil")=="ADMINISTRADOR","Perdeu perfil admin");
            var staff=await HttpUser(adm,"funcionarios");using var atendente=new Session();await Login(atendente,staff.email,staff.secret);
            var senhaStaff=Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(18));
            Status((await Form(atendente,"/conta/senha",Troca(staff.secret,senhaStaff,senhaStaff))).code,302);Require(S(await Get(atendente,"/api/v1/sessao"),"perfil")=="ATENDENTE","Perdeu perfil atendente");return new{admin=302,atendente=302,perfisPreservados=true};
        });
        await Case("CONTA-10","Hash esperado impede sobrescrita concorrente de senha",async()=>{
            var hash=Convert.ToString(await Scalar(c,"SELECT SenhaHash FROM dbo.Usuario WHERE UsuarioId=@u",P("@u",id)))!;
            var hasher=new PasswordHasher<object>();var ha=hasher.HashPassword(new(),"concorrente-"+Guid.NewGuid());var hb=hasher.HashPassword(new(),"concorrente-"+Guid.NewGuid());
            var codes=await Pair(async cx=>{await Proc(cx,"dbo.usp_AlterarSenhaConta",P("@UsuarioId",id),P("@HashEsperado",hash),P("@NovoHash",ha));},async cx=>{await Proc(cx,"dbo.usp_AlterarSenhaConta",P("@UsuarioId",id),P("@HashEsperado",hash),P("@NovoHash",hb));});
            Require(codes.Count(x=>x==0)==1&&codes.Count(x=>x==51012)==1,"Sobrescrita de senha concorrente");return new{sucessos=1,conflitos=1};
        });
    }
}
