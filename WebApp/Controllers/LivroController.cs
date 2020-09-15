using MongoDB.Bson;
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
        private enum StatusResponse
        {
            OK,
            ERROR
        }

        string conexaoMongo = "mongodb+srv://Livros:Livros@trocalivrostcc.nsbyt.gcp.mongodb.net/Livros?retryWrites=true&w=majority";

        [HttpPost]
        [Route("criarUsuario/{nome}/{idade:int}/{cidade}/{email}/{senha}/{estado=null}")]
        public IHttpActionResult CadastrarUsuario(string nome, int idade, string cidade, string email, string senha, string estado = null)
        {
            try
            {
                IMongoClient client = new MongoClient(conexaoMongo);
                IMongoDatabase database = client.GetDatabase("TrocaLivro");
                IMongoCollection<Usuario> collect = database.GetCollection<Usuario>("Usuario");
                MessageError messageError = new MessageError();

                if ((collect.Find(c => c.Nome == nome).FirstOrDefault()) != null)
                {
                    messageError.status = StatusResponse.ERROR.ToString();
                    messageError.msg = "Nome de usuário já cadastrado";
                    messageError.code = 409;

                    return Ok(messageError);
                }

                if ((collect.Find(c => c.Email == email).FirstOrDefault()) != null)
                {
                    messageError.status = StatusResponse.ERROR.ToString();
                    messageError.msg = "Email já esta cadastrado";
                    messageError.code = 412;

                    return Ok(messageError);
                }

                Usuario usuario = new Usuario();

                usuario.Nome = nome;
                usuario.Idade = idade;
                usuario.Cidade = cidade;
                usuario.Email = email;
                usuario.Estado = estado;
                usuario.DataRegistro = DateTime.Now;
                usuario.DataAlteracao = DateTime.Now;
                usuario.Senha = Base64Encode(senha);
                if (string.IsNullOrEmpty(estado)) { usuario.Estado = estado; }

                messageError.msg = "Usuário cadastrado com sucesso";
                messageError.status = StatusResponse.OK.ToString();
                messageError.code = 200;

                collect.InsertOne(usuario);

                return Ok(messageError);
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
                //falta codificar a senha em base 64 para fazer a comparação
                var result = string.Empty;
                IMongoClient client = new MongoClient(conexaoMongo);
                IMongoDatabase database = client.GetDatabase("TrocaLivro");
                List<Usuario> collect = database.GetCollection<Usuario>("Usuario").AsQueryable().ToList();

                MessageError messageError = new MessageError();

                if (collect.Any(user => user.Nome == nome))
                {
                    if (collect.Any(user => user.Nome == nome && user.Email == email))
                    {
                        if (collect.Any(user => user.Nome == nome && user.Email == email && user.Senha == Base64Encode(senha)))
                        {
                            var user = collect.Find(c => c.Nome == nome && c.Email == email);
                            messageError.status = StatusResponse.OK.ToString();
                            messageError.code = 200;
                            messageError.msg = "Usuário autenticado com sucesso";
                            messageError.id = user.Id.ToString();
                        }
                        else
                        {
                            messageError.status = StatusResponse.ERROR.ToString();
                            messageError.code = 406;
                            messageError.msg = "Senha incorreta";
                        }
                    }
                    else
                    {
                        messageError.status = StatusResponse.ERROR.ToString();
                        messageError.code = 405;
                        messageError.msg = "Email incorreto";
                    }
                }
                else
                {
                    messageError.status = StatusResponse.ERROR.ToString();
                    messageError.code = 407;
                    messageError.msg = "Usuário não encontrado";
                }

                return Ok(messageError);
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }

        [HttpGet]
        [Route("listarUsuarios")]
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

        [HttpPut]
        [Route("atualizarDadosPessoais/{id}")]
        public IHttpActionResult AtualizarDadosPessoais(string id)
        {

            try
            {
                //teste
                Usuario u = new Usuario();
                //u.Id = ObjectId("5f4052a8dd3db438e03b50e5");
                u.Nome = "Ewerton Richieri Lopes";
                u.Idade = 18;
                u.Cidade = "Atlanta Bar";
                u.Email = "contato.ewertonrichieri@gmail.com";
                u.Senha = "dGVzdGU=";
                u.Estado = null;
                u.DataRegistro = DateTime.Now;
               

                var idJson = ObjectId.Parse(id);

                IMongoClient client = new MongoClient(conexaoMongo);
                IMongoDatabase database = client.GetDatabase("TrocaLivro");
                IMongoCollection<Usuario> collect = database.GetCollection<Usuario>("Usuario");

                var teste2 = collect.Find(c =>  c.Id == idJson).FirstOrDefault();
                teste2.Cidade = "Natal BR";

                collect.ReplaceOne(c => c.Nome == u.Nome, teste2);

            }
            catch (Exception e)
            {
                return InternalServerError(new Exception(e.Message));
            }
            return Ok("");
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
