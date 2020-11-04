using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class Livro
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string Titulo { get; set; }
        public string Autor { get; set; }
        public string DescricaoLivro { get; set; }
        public int QtdTotalPaginas { get; set; }
        public string Idioma { get; set; }
        public long Isbn { get; set; }
        public string idUsuarioLivro { get; set; }
        public string EnderecoUsuarioLivro { get; set; }
        public string LatitudeUsuarioLivro { get; set; }
        public string LongitudeUsuarioLivro { get; set; }
        public DateTime DataEdicao { get; set; }
        public DateTime DataRegistro { get; set; }

    }
}