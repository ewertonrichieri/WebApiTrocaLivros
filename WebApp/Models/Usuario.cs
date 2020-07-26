using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

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

        [Display(Name = "Estado")]
        public string Estado { get; set; }
    }
}