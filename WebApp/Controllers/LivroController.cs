using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;
using WebApp.Models;

namespace WebApp.Controllers
{
    [EnableCors("*", "*", "*")]
    [RoutePrefix("api/livros")]
    public class LivroController : ApiController
    {

        string conexaoMongo = "mongodb+srv://Livros:livros@caronas.n5wca.gcp.mongodb.net/Livros?retryWrites=true";

        [HttpPost]
        [Route("criarUsuario/{nome}/{idade:int}/{cidade}/{email}/{senha}/{estado=null}")]
        public IHttpActionResult CadastrarUsuario(string nome, int idade, string cidade, string email, string senha, string estado = null)
        {
            try
            {
                IMongoClient client = new MongoClient(conexaoMongo);
                IMongoDatabase database = client.GetDatabase("TrocaLivro");
                IMongoCollection<Usuario> collect = database.GetCollection<Usuario>("Usuario");

                if (!string.IsNullOrEmpty(collect.Find(c => c.Nome == nome).FirstOrDefault().ToString()))
                {

                    return Ok("Erro, Nome de usuário já cadastrado");
                }

                if (!string.IsNullOrEmpty(collect.Find(c => c.Email == email).FirstOrDefault().ToString()))
                {
                    return Ok("Erro, Email já esta cadastrado");
                }

                Usuario usuario = new Usuario();

                usuario.Nome = nome;
                usuario.Idade = idade;
                usuario.Cidade = cidade;
                usuario.Email = email;
                usuario.DataRegistro = DateTime.Now;
                usuario.Senha = Base64Encode(senha);
                if (string.IsNullOrEmpty(estado)) { usuario.Estado = estado; }

                collect.InsertOne(usuario);
                return Ok("Ok, Usuário cadastrado com sucesso");
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }

        [HttpGet]
        [Route("autenticarUsuario/{nome}/{email}/{senha}")]
        public IHttpActionResult AuthenticarUsuario(string nome, string email, string senha)
        {
            try
            {
                var result = string.Empty;
                IMongoClient client = new MongoClient(conexaoMongo);
                IMongoDatabase database = client.GetDatabase("TrocaLivro");
                List<Usuario> collect = database.GetCollection<Usuario>("Usuario").AsQueryable().ToList();

                if (collect.Any(user => user.Nome == nome))
                {
                    if (collect.Any(user => user.Nome == nome && user.Email == email))
                    {
                        if (collect.Any(user => user.Nome == nome && user.Email == email && user.Senha == senha))
                        {
                            result = "Usuário autenticado com sucesso";
                        }
                        else
                        {
                            result = "Senha incorreta";
                        }
                    }
                    else
                    {
                        result = "Email incorreto";
                    }
                }
                else
                {
                    result = "Usuário não encontrado";
                }

                return Ok(result);
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }

        public IHttpActionResult GetLista()
        {
            try
            {
                IMongoClient client = new MongoClient(conexaoMongo);
                IMongoDatabase database = client.GetDatabase("TrocaLivro");
                var collect = database.GetCollection<Usuario>("Usuario").AsQueryable();

                List<Usuario> lista = collect.ToList();

                return Ok(lista);
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }


        public static string Base64Encode(string text)
        {
            try
            {
                var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(text);
                return System.Convert.ToBase64String(plainTextBytes);
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

    }
}
