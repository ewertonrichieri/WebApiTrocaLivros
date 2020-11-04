using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Web.Http;
using System.Web.Http.Cors;
using WebApp.Models;

// using SendGrid's C# Library


namespace WebApp.Controllers
{
    [EnableCors("*", "*", "*")]
    [RoutePrefix("api/cn")]
    public class UsuarioController : ApiController
    {
        private enum StatusResponse
        {
            OK,
            ERROR
        }

        string conexaoMongo = "mongodb+srv://Livros:Livros@trocalivrostcc.nsbyt.gcp.mongodb.net/Livros?retryWrites=true&w=majority";

        [HttpPost]
        [Route("criarUsuario")]
        public IHttpActionResult CriarUsuario(Usuario userExterno)
        {
            try
            {
                ResponseMessage messageError = new ResponseMessage();

                if (userExterno == null)
                {
                    messageError.Code = 100;
                    messageError.Msg = "Campos {Nome, Idade, Cidade, Email, Senha, Estado e Endereço} são obrigatórios";
                    messageError.Status = StatusResponse.ERROR.ToString();

                    return Ok(messageError);
                }

                if ((String.IsNullOrEmpty(userExterno.Nome)) || (userExterno.Idade == 0) || (String.IsNullOrEmpty(userExterno.Cidade))
                    || (String.IsNullOrEmpty(userExterno.Endereco)) || (String.IsNullOrEmpty(userExterno.Email))
                    || (String.IsNullOrEmpty(userExterno.Senha)) || (String.IsNullOrEmpty(userExterno.Estado)))
                {
                    messageError.Code = 100;
                    messageError.Msg = "Campos {Nome, Idade, Cidade, Email, Senha, Estado e Endereço} são obrigatórios";
                    messageError.Status = StatusResponse.ERROR.ToString();

                    return Ok(messageError);
                }

                if (String.IsNullOrEmpty(userExterno.Estado) || userExterno.Estado.Length > 2)
                {
                    messageError.Code = 101;
                    messageError.Msg = "Campo 'Estado' só pode ter 2 caracteres";
                    messageError.Status = StatusResponse.ERROR.ToString();

                    return Ok(messageError);
                }

                if (!ValidaEmail(userExterno.Email))
                {
                    messageError.Code = 102;
                    messageError.Msg = "Email inválido";
                    messageError.Status = StatusResponse.ERROR.ToString();

                    return Ok(messageError);
                }


                if (!ValidaNomeCidadeUsuario(userExterno.Nome))
                {
                    messageError.Code = 103;
                    messageError.Msg = "Parâmetro 'Nome' inválido";
                    messageError.Status = StatusResponse.ERROR.ToString();

                    return Ok(messageError);
                }

                if (!ValidaNomeCidadeUsuario(userExterno.Cidade))
                {
                    messageError.Code = 104;
                    messageError.Msg = "Parâmetro 'Cidade' inválido";
                    messageError.Status = StatusResponse.ERROR.ToString();

                    return Ok(messageError);
                }

                userExterno.Nome = userExterno.Nome.Contains("  ") ? RetiraEspacoBrancoDesnecessario(userExterno.Nome) : userExterno.Nome.Trim();

                IMongoClient client = new MongoClient(conexaoMongo);
                IMongoDatabase database = client.GetDatabase("TrocaLivro");
                IMongoCollection<Usuario> collect = database.GetCollection<Usuario>("Usuario");

                if ((collect.Find(c => c.Nome == userExterno.Nome).FirstOrDefault()) != null)
                {
                    messageError.Status = StatusResponse.ERROR.ToString();
                    messageError.Msg = "Já existe um usuário cadastrado com este nome";
                    messageError.Code = 403;

                    return Ok(messageError);
                }

                if ((collect.Find(c => c.Email == userExterno.Email).FirstOrDefault()) != null)
                {
                    messageError.Status = StatusResponse.ERROR.ToString();
                    messageError.Msg = "Email já esta cadastrado";
                    messageError.Code = 404;

                    return Ok(messageError);
                }

                Usuario user = new Usuario();

                user.Nome = userExterno.Nome;
                user.Idade = userExterno.Idade;
                user.Cidade = userExterno.Cidade;
                user.Email = userExterno.Email;
                user.Estado = userExterno.Estado;
                user.DataRegistro = DateTime.Now;
                user.DataAlteracao = DateTime.Now;
                user.Senha = Base64Encode(userExterno.Senha);
                if (!string.IsNullOrEmpty(userExterno.Estado)) { user.Estado = userExterno.Estado; }
                if (!string.IsNullOrEmpty(userExterno.Latitude)) { user.Latitude = userExterno.Latitude; }
                if (!string.IsNullOrEmpty(userExterno.Longitude)) { user.Longitude = userExterno.Longitude; }
                if (!string.IsNullOrEmpty(userExterno.Endereco)) { user.Endereco = userExterno.Endereco; }

                messageError.Msg = "Usuário cadastrado com sucesso";
                messageError.Status = StatusResponse.OK.ToString();
                messageError.Code = 200;

                collect.InsertOne(user);

                return Ok(messageError);
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }

        [Authorize]
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
                        //Se for bloqueada por alguma denuncia
                        bool resultadoContaBloqueada = true;

                        if (!String.IsNullOrEmpty(user.DataDesbloqueioConta))
                        {
                            DateTime dataAtual = DateTime.Now.Date;
                            try
                            {
                                DateTime dataContaAcessoLiberada = DateTime.ParseExact(user.DataDesbloqueioConta, "dd/MM/yyyy", null);
                                resultadoContaBloqueada = DateTime.Compare(dataContaAcessoLiberada, dataAtual) == 1 ? false : true;
                            }
                            catch (Exception e)
                            {
                                messageError.Msg = "Data de bloqueio '" + user.DataDesbloqueioConta + "' esta no formato incorreto, favor corrigir";
                                return messageError;
                            }
                        }

                        if (!resultadoContaBloqueada)
                        {
                            messageError.Status = StatusResponse.ERROR.ToString();
                            messageError.Code = 405;
                            messageError.Msg = "Conta bloqueada até a data " + user.DataDesbloqueioConta;
                        }
                        else
                        {
                            messageError.Status = StatusResponse.OK.ToString();
                            messageError.Code = 200;
                            messageError.Msg = "Usuário autenticado com sucesso";
                            messageError.ID = user.Id.ToString();
                            messageError.Email = user.Email;
                            messageError.TypeAccount = user.TypeAccount != null ? user.TypeAccount : "User";
                            messageError.Endereco = user.Endereco != null ? user.Endereco : string.Empty;
                            messageError.LatitudeLongitude = user.Latitude != null ? user.Latitude +";"+ user.Longitude : string.Empty;
                        }
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
                    messageError.Code = 406;
                    messageError.Msg = "Usuário ou Senha inválida";
                }

                return messageError;

            }
            catch (Exception e)
            {
                messageError.Msg = e.Message;
            }

            return messageError;
        }

        [Authorize(Roles = "Administrador")]
        [HttpGet]
        [Route("listarUsuarios")]
        public IHttpActionResult GetListaUsuarios()
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

        [Authorize]
        [HttpPut]
        [Route("atualizarDadosPessoais")]
        public IHttpActionResult AtualizaDadosPessoais(Usuario userExterno)
        {
            try
            {
                //u.Id = ObjectId("5f4052a8dd3db438e03b50e5");
                ResponseMessage messageError = new ResponseMessage();

                if (userExterno == null)
                {
                    messageError.Msg = "Nenhuma alteração foi realizada";
                    messageError.Status = StatusResponse.ERROR.ToString();
                    messageError.Code = 107;

                    return Ok(messageError);
                }

                if (String.IsNullOrEmpty(userExterno.Nome) && userExterno.Idade == 0 && String.IsNullOrEmpty(userExterno.Cidade) 
                    && String.IsNullOrEmpty(userExterno.Endereco) && String.IsNullOrEmpty(userExterno.Email) 
                    && String.IsNullOrEmpty(userExterno.Senha) && String.IsNullOrEmpty(userExterno.Estado))
                {
                    messageError.Msg = "Nenhuma alteração foi realizada";
                    messageError.Status = StatusResponse.ERROR.ToString();
                    messageError.Code = 107;

                    return Ok(messageError);
                }

                if (userExterno.Estado.Length > 2)
                {
                    messageError.Code = 101;
                    messageError.Msg = "Campo 'Estado' só pode ter 2 caracteres";
                    messageError.Status = StatusResponse.ERROR.ToString();

                    return Ok(messageError);
                }

                if (!ValidaEmail(userExterno.Email))
                {
                    messageError.Code = 102;
                    messageError.Msg = "Email inválido";
                    messageError.Status = StatusResponse.ERROR.ToString();

                    return Ok(messageError);
                }

                if (!ValidaNomeCidadeUsuario(userExterno.Nome))
                {
                    messageError.Code = 103;
                    messageError.Msg = "Parâmetro 'Nome' inválido";
                    messageError.Status = StatusResponse.ERROR.ToString();

                    return Ok(messageError);
                }

                if (!ValidaNomeCidadeUsuario(userExterno.Cidade))
                {
                    messageError.Code = 104;
                    messageError.Msg = "Parâmetro 'Cidade' inválido";
                    messageError.Status = StatusResponse.ERROR.ToString();

                    return Ok(messageError);
                }

                //valida o nome se tem espaços desnecessario
                userExterno.Nome = userExterno.Nome.Contains("  ") ? RetiraEspacoBrancoDesnecessario(userExterno.Nome) : userExterno.Nome.Trim();

                IMongoClient client = new MongoClient(conexaoMongo);
                IMongoDatabase database = client.GetDatabase("TrocaLivro");
                IMongoCollection<Usuario> collect = database.GetCollection<Usuario>("Usuario");

                string idUser = string.Empty;

                if (System.Security.Claims.ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value != null)
                {
                    idUser = System.Security.Claims.ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value;
                }

                var idJson = ObjectId.Parse(idUser);

                Usuario usuario = collect.Find(c => c.Id == idJson).FirstOrDefault();

                if (usuario != null)
                {
                    if (usuario.Email != userExterno.Email && userExterno.Email != null)
                    {
                        bool emailValido = ValidaEmail(userExterno.Email);

                        if (!emailValido)
                        {
                            messageError.Msg = "Email inválido";
                            messageError.Status = StatusResponse.ERROR.ToString();
                            messageError.Code = 102;

                            return Ok(messageError);
                        }

                        else if ((collect.Find(c => c.Email == userExterno.Email && c.Id != idJson).FirstOrDefault()) != null)
                        {
                            messageError.Msg = "Este email já existe";
                            messageError.Status = StatusResponse.ERROR.ToString();
                            messageError.Code = 404;
                            return Ok(messageError);
                        }
                        else
                        {
                            usuario.Email = userExterno.Email;
                        }
                    }

                    //verifica nome existente
                    if (usuario.Nome != userExterno.Nome && (!String.IsNullOrEmpty(userExterno.Nome)))
                    {
                        if (!ValidaNomeCidadeUsuario(userExterno.Nome))
                        {
                            messageError.Msg = "Parâmetro 'Nome' Inválido";
                            messageError.Status = StatusResponse.ERROR.ToString();
                            messageError.Code = 103;

                            return Ok(messageError);
                        }
                        else
                        {
                            usuario.Nome = userExterno.Nome;
                        }
                    }

                    if (userExterno.Idade > 0 && usuario.Idade != userExterno.Idade) usuario.Idade = userExterno.Idade;
                    if (userExterno.Cidade != null && usuario.Cidade != userExterno.Cidade) usuario.Cidade = userExterno.Cidade;
                    if (userExterno.Senha != null && usuario.Senha != userExterno.Senha) usuario.Senha = userExterno.Senha;
                    if (userExterno.Estado != null && usuario.Estado != userExterno.Estado) usuario.Estado = userExterno.Estado;
                    if (!String.IsNullOrEmpty(userExterno.Endereco) && usuario.Endereco != userExterno.Endereco) usuario.Endereco = userExterno.Endereco;

                    if ((!String.IsNullOrEmpty(userExterno.Latitude)) && usuario.Latitude != userExterno.Latitude) usuario.Latitude = userExterno.Latitude;
                    if ((!String.IsNullOrEmpty(userExterno.Longitude)) && usuario.Longitude != userExterno.Longitude) usuario.Longitude = userExterno.Longitude;
                    DateTime dt = DateTime.Now;
                    usuario.DataAlteracao = dt;

                    collect.ReplaceOne(c => c.Id == idJson, usuario);

                    messageError.Msg = "Alteração realizada com sucesso";
                    messageError.Status = StatusResponse.OK.ToString();
                    messageError.Code = 200;

                    return Ok(messageError);
                }
                else
                {
                    messageError.Msg = "Usuário não encontrado";
                    messageError.Status = StatusResponse.ERROR.ToString();
                    messageError.Code = 407;

                    return Ok(messageError);
                }
            }
            catch (Exception e)
            {
                return InternalServerError(new Exception(e.Message));
            }
        }



        [Route("enviarTeste")]
        public IHttpActionResult PostEMAIL([FromBody] string value)
        {
            try
            {
                string emailBook = "bookstationapp@gmail.com";
                string mensagemUser = "botaGofofoffoofoff";
                string emailUser = "tested";

                MailMessage message = new MailMessage();
                SmtpClient smtp = new SmtpClient();
                message.From = new MailAddress(emailBook);
                message.To.Add(new MailAddress(emailBook));
                message.Subject = "Denuncias Cult Network";
                message.IsBodyHtml = true;
                message.Body = mensagemUser + " ENVIADO POR: " + emailUser;
                smtp.Port = 587;
                smtp.Host = "smtp.gmail.com";
                smtp.EnableSsl = true;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential(emailBook, "tcclivros");
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.Send(message);

                return Ok("Email enviado com suceso");
            }
            catch (Exception e)
            {
                InternalServerError(e);
            }
            return NotFound();
        }

        [HttpPost]
        [Route("enviarEmailDenuncia/{mensagemUser}/emailUser")]
        //public IHttpActionResult PostEnviarEmailDenuncia(JObject jsonData)
        public IHttpActionResult PostEnviarEmailDenuncia(string mensagemUser, string emailUser)
        {
            try
            {


                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress("bookstationapp@gmail.com");
                    mail.To.Add("somebody@domain.com");
                    mail.Subject = "Hello World";
                    mail.Body = "<h1>Hello</h1>";
                    mail.IsBodyHtml = true;
                    //mail.Attachments.Add(new Attachment("C:\\file.zip"));

                    using (SmtpClient smtps = new SmtpClient("smtp.gmail.com", 587))
                    {
                        smtps.Credentials = new NetworkCredential("email@gmail.com", "tcclivros");
                        smtps.EnableSsl = true;
                        smtps.Send(mail);
                    }
                }










                string emailBook = "bookstationapp@gmail.com";

                //dynamic json = jsonData;
                //json = jsonData;

                ResponseMessage response = new ResponseMessage();

                //if (jsonData == null)
                //{
                //    response.Code = 410;
                //    response.Status = StatusResponse.ERROR.ToString();
                //    response.Msg = "Campo 'mensagem' obrigatório";

                //    return Ok(response);
                //}

                //string mensagemUser = Convert.ToString(json.mensagem);
                //string emailUser = Convert.ToString(json.emailUser);

                if (String.IsNullOrEmpty(mensagemUser))
                {
                    response.Code = 410;
                    response.Status = StatusResponse.ERROR.ToString();
                    response.Msg = "Campo 'mensagem' obrigatório";

                    return Ok(response);
                }

                if (String.IsNullOrEmpty(emailUser))
                {

                    if (String.IsNullOrEmpty(System.Security.Claims.ClaimsPrincipal.Current.FindFirst(ClaimTypes.Email).Value))
                    {
                        emailUser = System.Security.Claims.ClaimsPrincipal.Current.FindFirst(ClaimTypes.Email).Value;
                    }

                    else
                    {
                        emailUser = "Anonimo";
                    }

                }

                else
                {
                    if (!ValidaEmail(emailUser))
                    {
                        response.Code = 102;
                        response.Status = StatusResponse.ERROR.ToString();
                        response.Msg = "Campo 'Email' inválido";

                        return Ok(response);
                    }
                }

                MailMessage message = new MailMessage();
                SmtpClient smtp = new SmtpClient();
                message.From = new MailAddress(emailBook);
                message.To.Add(new MailAddress(emailBook));
                message.Subject = "Denuncias Cult Network";
                message.IsBodyHtml = true;
                message.Body = mensagemUser + " ENVIADO POR: " + emailUser;
                smtp.Port = 587;
                smtp.Host = "smtp.gmail.com";
                smtp.EnableSsl = true;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential(emailBook, "tcclivros");
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.Send(message);

                return Ok("Email enviado com suceso");

            }
            catch (Exception e)
            {
                return InternalServerError(e);
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

        public bool ValidaNomeCidadeUsuario(string nome)
        {
            try
            {
                if (Regex.IsMatch(nome, @"[^A-Za-z0-9\ ãéç]"))
                {   //nome invalido
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public string RetiraEspacoBrancoDesnecessario(string nome)
        {
            //retira espaços desnecessario
            string nomeComEspacoDesnecessario = nome.Trim();

            while (nomeComEspacoDesnecessario.Contains("  "))
            {
                nomeComEspacoDesnecessario = nomeComEspacoDesnecessario.Replace("  ", " ");
            }

            return nome = nomeComEspacoDesnecessario;

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
