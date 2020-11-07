using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Cors;
using WebApp.Models;

namespace WebApp.Controllers
{
    [EnableCors("*", "*", "*")]
    [RoutePrefix("api/cn/livro")]
    public class LivroController : ApiController
    {
        private enum StatusResponse
        {
            OK,
            ERROR
        }

        [Authorize]
        [HttpPost]
        [Route("criarLivro")]
        public IHttpActionResult CriarLivro(Livro livroExterno)
        {
            try
            {
                ResponseMessage response = new ResponseMessage();

                if (livroExterno == null)
                {
                    response.Code = 110;
                    response.Msg = "Nenhuma informação foi preenchida";
                    response.Status = StatusResponse.ERROR.ToString();

                    return Ok(response);
                }

                if ((String.IsNullOrEmpty(livroExterno.Titulo)) || (String.IsNullOrEmpty(livroExterno.Autor)) || (String.IsNullOrEmpty(livroExterno.DescricaoLivro))
                   || (livroExterno.QtdTotalPaginas == 0))
                {
                    response.Code = 111;
                    response.Msg = "Campos {Titulo, Autor, DescricaoLivro e QtdTotalPaginas} são obrigatórios";
                    response.Status = StatusResponse.ERROR.ToString();

                    return Ok(response);
                }

                MongoDBModel dbModel = new MongoDBModel();
                IMongoCollection<Livro> collect = dbModel.GetLivros();

                livroExterno.DataRegistro = DateTime.Now;

                if (System.Security.Claims.ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value != null)
                {
                    string idUsuarioLivro = System.Security.Claims.ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value;

                    if (collect.Find(c => c.Titulo == livroExterno.Titulo && c.idUsuarioLivro == idUsuarioLivro).FirstOrDefault() != null)
                    {
                        response.Code = 112;
                        response.Msg = "Você já cadastrou um livro com este título";
                        response.Status = StatusResponse.ERROR.ToString();

                        return Ok(response);
                    }

                    livroExterno.idUsuarioLivro = idUsuarioLivro;
                }
                else
                {
                    response.Code = 113;
                    response.Msg = "Existe um erro com o ID do usuário logado, Se o erro persistir realize um novo cadastro";
                    response.Status = StatusResponse.ERROR.ToString();

                    return Ok(response);
                }


                if (!String.IsNullOrEmpty(System.Security.Claims.ClaimsPrincipal.Current.FindFirst(ClaimTypes.StreetAddress).Value))
                    livroExterno.EnderecoUsuarioLivro = System.Security.Claims.ClaimsPrincipal.Current.FindFirst(ClaimTypes.StreetAddress).Value;

                if (!String.IsNullOrEmpty(System.Security.Claims.ClaimsPrincipal.Current.FindFirst(ClaimTypes.MobilePhone).Value))
                    livroExterno.CelularUsuarioLivro = System.Security.Claims.ClaimsPrincipal.Current.FindFirst(ClaimTypes.MobilePhone).Value;

                if (!String.IsNullOrEmpty(System.Security.Claims.ClaimsPrincipal.Current.FindFirst(ClaimTypes.Locality).Value))
                {
                    string[] local = System.Security.Claims.ClaimsPrincipal.Current.FindFirst(ClaimTypes.Locality).Value.Split(';');
                    livroExterno.LatitudeUsuarioLivro = local[0];
                    livroExterno.LongitudeUsuarioLivro = local[1];
                }

                collect.InsertOne(livroExterno);
                response.Code = 200;
                response.Msg = "Livro Cadastrado com sucesso";
                response.Status = StatusResponse.OK.ToString();

                return Ok(response);
            }
            catch (Exception e)
            {
                InternalServerError(e);
                return Ok(e);
            }
        }

        [Authorize]
        [HttpGet]
        [Route("consultarLivrosUsuarioLogado")]
        public List<Livro> GetConsultarMeusLivros()
        {
            string idUsuarioLogado = string.Empty;

            if (System.Security.Claims.ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value != null)
            {
                idUsuarioLogado = System.Security.Claims.ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value;
            }

            MongoDBModel mongoDB = new MongoDBModel();
            List<Livro> meusLivros = mongoDB.GetConsultaLivroUsuarioLogado(idUsuarioLogado);
            return meusLivros;
        }

        [HttpGet]
        [Route("getLivroTitulo")]
        public List<Livro> GetLivroTitulo(JObject jsonData)
        {
            dynamic json = jsonData;
            string titulo = json.titulo;
            if (!String.IsNullOrEmpty(titulo))
            {
                MongoDBModel mongoDB = new MongoDBModel();
                List<Livro> livros = mongoDB.GetTituloLivro(titulo);
                return livros;
            }
            else
            {
                return null;
            }
        }

        [HttpGet]
        [Route("getLivroAutor")]
        public List<Livro> GetLivroAutor(JObject jsonData)
        {
            dynamic json = jsonData;
            string autor = json.autor;
            if (!String.IsNullOrEmpty(autor))
            {
                MongoDBModel mongoDB = new MongoDBModel();
                List<Livro> livros = mongoDB.GetAutorLivro(autor);
                return livros;
            }
            else
            {
                return null;
            }
        }
    }
}
