using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using WebApp.Models;

namespace WebApp.Controllers
{
    [EnableCors("*", "*", "*")]
    [RoutePrefix("api/livros")]
    [Authorize]
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
                Response messageError = new Response();

                if ((collect.Find(c => c.Nome == nome).FirstOrDefault()) != null)
                {
                    messageError.Status = StatusResponse.ERROR.ToString();
                    messageError.Msg = "Nome de usuário já cadastrado";
                    messageError.Code = 409;

                    return Ok(messageError);
                }

                if ((collect.Find(c => c.Email == email).FirstOrDefault()) != null)
                {
                    messageError.Status = StatusResponse.ERROR.ToString();
                    messageError.Msg = "Email já esta cadastrado";
                    messageError.Code = 412;

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

                messageError.Msg = "Usuário cadastrado com sucesso";
                messageError.Status = StatusResponse.OK.ToString();
                messageError.Code = 200;

                collect.InsertOne(usuario);

                return Ok(messageError);
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }

        public static Response AutenticarUsuario(string nome, string senha)
        {
            Response messageError = new Response();

            try
            {
                string conexaoMongo = "mongodb+srv://Livros:Livros@trocalivrostcc.nsbyt.gcp.mongodb.net/Livros?retryWrites=true&w=majority";

                var result = string.Empty;
                IMongoClient client = new MongoClient(conexaoMongo);
                IMongoDatabase database = client.GetDatabase("TrocaLivro");
                List<Usuario> collect = database.GetCollection<Usuario>("Usuario").AsQueryable().ToList();

                if (collect.Any(user => user.Nome == nome))
                {
                    var user = collect.Find(c => c.Nome == nome && c.Senha == Base64Encode(senha));

                    if (user != null)
                    {
                        messageError.Status = StatusResponse.OK.ToString();
                        messageError.Code = 200;
                        messageError.Msg = "Usuário autenticado com sucesso";
                        messageError.ID= user.Id.ToString();
                        messageError.TypeAccount = user.TypeAccount != null ? user.TypeAccount : "User";
                    }
                    else
                    {
                        messageError.Status = StatusResponse.ERROR.ToString();
                        messageError.Code = 406;
                        messageError.Msg = "Usuário ou Senha inválida";
                    }
                }
                else
                {
                    messageError.Status = StatusResponse.ERROR.ToString();
                    messageError.Code = 407;
                    messageError.Msg = "Usuário não encontrado";
                }

                return messageError;

            }
            catch (Exception e)
            {
            }

            return messageError;
        }

        [Authorize(Roles = "Administrador")]
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
                //falta arrumar datetime.Now

                //u.Id = ObjectId("5f4052a8dd3db438e03b50e5");
                Response message = new Response();

                if (nome == null && idade == 0 && cidade == null && email == null && senha == null && estado == null)
                {
                    message.Msg = "Nenhuma alteração foi realizada";
                    message.Status = StatusResponse.ERROR.ToString();
                    message.Code = 107;

                    return Ok(message);
                }

                IMongoClient client = new MongoClient(conexaoMongo);
                IMongoDatabase database = client.GetDatabase("TrocaLivro");
                IMongoCollection<Usuario> collect = database.GetCollection<Usuario>("Usuario");

                var idJson = ObjectId.Parse(id);

                Usuario usuario = collect.Find(c => c.Id == idJson).FirstOrDefault();

                if (usuario != null)
                {
                    if (usuario.Email != email && email != null)
                    {
                        bool emailValido = ValidaEmail(email);

                        if (!emailValido)
                        {
                            message.Msg = "Email inválido";
                            message.Status = StatusResponse.ERROR.ToString();
                            message.Code = 103;

                            return Ok(message);
                        }

                        else if ((collect.Find(c => c.Email == email && c.Id != idJson).FirstOrDefault()) != null)
                        {
                            message.Msg = "Este email já existe";
                            message.Status = StatusResponse.ERROR.ToString();
                            message.Code = 101;
                            return Ok(message);
                        }
                        else
                        {
                            usuario.Email = email;
                        }
                    }

                    //verifica nome existente
                    if (usuario.Nome != nome && nome != null)
                    {
                        if (!Regex.IsMatch(nome, @"^[a-zA-Z ]+$"))
                        {
                            message.Msg = "Nome inválido";
                            message.Status = StatusResponse.ERROR.ToString();
                            message.Code = 104;

                            return Ok(message);
                        }
                        else
                        {
                            usuario.Nome = nome.Trim().Replace("  ", " ");
                        }
                    }

                    if (idade > 0 && usuario.Idade != idade) usuario.Idade = idade;
                    if (cidade != null && usuario.Cidade != cidade) usuario.Cidade = cidade;
                    if (senha != null && usuario.Senha != senha) usuario.Senha = senha;
                    if (estado != null && usuario.Estado != estado) usuario.Estado = estado;
                    DateTime dt = DateTime.Now;
                    usuario.DataAlteracao = dt;

                    collect.ReplaceOne(c => c.Id == idJson, usuario);

                    message.Msg = "Alteração realizada com sucesso";
                    message.Status = StatusResponse.OK.ToString();
                    message.Code = 200;

                    return Ok(message);
                }
                else
                {
                    message.Msg = "Usuário não encontrado";
                    message.Status = StatusResponse.ERROR.ToString();
                    message.Code = 100;

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
