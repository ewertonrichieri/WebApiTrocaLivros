using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class Pais
    {
        [BsonId()]
        public ObjectId id { get; set; }

        [Required]
        [Display(Name ="Nome")]
        public string PaisCodigo { get; set; }
    }
}