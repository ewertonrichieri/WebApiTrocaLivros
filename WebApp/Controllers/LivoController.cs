using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;

namespace WebApp.Controllers
{
    [EnableCors("*", "*", "*")]
    [RoutePrefix("api/livro")]
    public class LivoController : ApiController
    {
        // GET: api/Livo
        [HttpGet]
        [Route("criarUsuario/{nome}/{idade:int}/{cidade}/{email}/{senha}/{estado=null}")]
        public IHttpActionResult PostCriarUsuario(string nome, int idade, string cidade, string email, string senha, string estado = "")
        {
            try
            {


                return Ok("");
            }
            catch (Exception ex)
            {

                return InternalServerError(ex);

            }
        }

        // GET: api/Livo/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Livo
        public void Post([FromBody] string value)
        {
        }

        // PUT: api/Livo/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/Livo/5
        public void Delete(int id)
        {
        }
    }
}
