using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
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
                MensagemResult messageError = new MensagemResult();

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

                MensagemResult messageError = new MensagemResult();

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
        public IHttpActionResult AtualizaDadosPessoaisPorId(string id, string nome = null, int idade = 0,
            string cidade = null, string email = null, string senha = null, string estado = null)
        {
            try
            {
                //teste
                //Usuario u = new Usuario();
                //u.Id = ObjectId("5f4052a8dd3db438e03b50e5");
                //u.Nome = "Ewerton Richieri Lopes";
                //u.Idade = 18;
                //u.Cidade = "Atlanta Bar";
                //u.Email = "contato.ewertonrichieri@gmail.com";
                //u.Senha = "dGVzdGU=";
                //u.Estado = null;
                //u.DataRegistro = DateTime.Now;


                var idJson = ObjectId.Parse(id);

                IMongoClient client = new MongoClient(conexaoMongo);
                IMongoDatabase database = client.GetDatabase("TrocaLivro");
                IMongoCollection<Usuario> collect = database.GetCollection<Usuario>("Usuario");

                MensagemResult message = new MensagemResult();

                //teste
                var teste = collect.Find(c => c.Email == email && c.Id != idJson).FirstOrDefault();


                Usuario usuario = collect.Find(c => c.Id == idJson).FirstOrDefault();

                if (usuario != null)
                {
                    if (usuario.Email != email && email != null)
                    {
                        bool emailValido = ValidaEmail(email);

                        if (!emailValido)
                        {
                            message.msg = "Email inválido";
                            message.status = StatusResponse.ERROR.ToString();
                            message.code = 103;

                            return Ok(message);
                        }

                        else if ((collect.Find(c => c.Email == email && c.Id != idJson).FirstOrDefault()) != null)
                        {
                            message.msg = "Este email já existe";
                            message.status = StatusResponse.ERROR.ToString();
                            message.code = 101;
                            return Ok(message);
                        }
                        else
                        {
                            usuario.Email = email;
                        }
                    }

                    //corrigir esta calida~]ap
                    //verifica nome existente
                    if (usuario.Nome != nome && nome != null)
                    {
                        if (!Regex.Match(nome.Trim(), "^[A - Z][a - zA - Z] * $").Success)
                        {
                            message.msg = "Nome inválido";
                            message.status = StatusResponse.ERROR.ToString();
                            message.code = 104;

                            return Ok(message);
                        }
                        else if ((collect.Find(c => c.Nome == nome && c.Id != idJson).FirstOrDefault()) != null)
                        {
                            message.msg = "Este nome já existe";
                            message.status = StatusResponse.ERROR.ToString();
                            message.code = 105;
                            return Ok(message);
                        }
                        else
                        {
                            usuario.Nome = nome;
                        }
                    }

                    if (idade > 0 && usuario.Idade != idade) usuario.Idade = idade;
                    if (cidade != null && usuario.Cidade != cidade) usuario.Cidade = cidade;
                    if (senha != null && usuario.Senha != senha) usuario.Senha = senha;
                    if (estado != null && usuario.Estado != estado) usuario.Estado = cidade;
                    usuario.DataAlteracao = DateTime.Now;

                    collect.ReplaceOne(c => c.Id == idJson, usuario);

                    message.msg = "Alteração realizada com sucesso";
                    message.status = StatusResponse.OK.ToString();
                    message.code = 200;

                    return Ok(message);
                }
                else
                {
                    message.msg = "Usuário não encontrado";
                    message.status = StatusResponse.ERROR.ToString();
                    message.code = 100;

                    return Ok(message);
                }
            }
            catch (Exception e)
            {
                return InternalServerError(new Exception(e.Message));
            }

        }

        public bool ValidaEmail(string email)
        {
            try
            {
                MailAddress m = new MailAddress(email);

                return true;
            }
            catch (FormatException)
            {
                return false;
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
