using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    public class Usuario
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [Display(Name = "Nome")]
        public string Nome { get; set; }

        [Required]
        [Display(Name = "Idade")]
        public int Idade { get; set; }

        [Required]
        [Display(Name = "Cidade")]
        public string Cidade { get; set; }

        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Senha")]
        public string Senha { get; set; }

        [BsonIgnoreIfNull]
        [Display(Name = "Estado")]
        public string Estado { get; set; }

        [Display(Name = "Celular")]
        public string Celular { get; set; }

        public string TypeAccount { get; set; }

        [BsonIgnoreIfNull]
        [Display(Name = "Latitude")]
        public string Latitude { get; set; }

        [BsonIgnoreIfNull]
        [Display(Name = "Longitude")]
        public string Longitude { get; set; }

        [Display(Name = "Endereco")]
        public string Endereco { get; set; }

        [Display(Name = "DataRegistro")]
        public DateTime DataRegistro { get; set; }

        [BsonIgnoreIfNull]
        [Display(Name = "DataAlteracao")]
        public DateTime DataAlteracao { get; set; }

        [BsonIgnoreIfNull]
        [Display(Name = "DataDesbloqueioConta")]
        public string DataDesbloqueioConta { get; set; }
    }
}