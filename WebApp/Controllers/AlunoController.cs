using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using WebApp.Models;

namespace WebApp.Controllers
{
    [EnableCors("*", "*", "*")]
    [RoutePrefix("api/livro")]

    public class AlunoController : ApiController
    {
        //string conexaoMongo = ConfigurationManager.ConnectionStrings["conexaoMongoDB"].ConnectionString;


        //string conexaoMongo = "mongodb+srv://Livros:livros@cluster0.n5wca.azure.mongodb.net/Livros?retryWrites=true&w=majority";
        //string conexaoMongo = "mongodb+srv://Livros:livros@cluster0.n5wca.gcp.mongodb.net/Livros?retryWrites=true";

        string connectionMongo = "mongodb+srv://Caronas:caronas2020@caronas.n5wca.gcp.mongodb.net/Caronas?retryWrites=true&w=majority";



        // GET: api/Aluno
        [HttpGet]
        [Route("recuperar/{nome}/{sobrenome=andrade}")]
        public IHttpActionResult Get( string nome = null)
        {
            IMongoClient client = new MongoClient(connectionMongo);
            IMongoDatabase database = client.GetDatabase("Livros");
            IMongoCollection<Pais> col = database.GetCollection<Pais>("pais");



            Pais p = new Pais();
            p.PaisCodigo = nome;

            col.InsertOne(p);


            try
            {

            }
            catch (Exception ex) {
                return InternalServerError(ex);
            }
            return Ok("teste");
        }

        // GET: api/Aluno/5
        public string Get(int id)
        {
            try
            {

            }
            catch (Exception e) {
                throw new Exception(e.Message);
            }
            return "value";
        }

        // POST: api/Aluno
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Aluno/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Aluno/5
        public void Delete(int id)
        {
        }
    }
}
