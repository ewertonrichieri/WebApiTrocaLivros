using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

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