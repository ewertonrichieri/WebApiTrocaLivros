using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class Response
    {
        public string TypeAccount { get; set; }
        public string ID { get; set; }
        public string Msg { get; set; }
        public string Status { get; set; }
        public int Code { get; set; }
        public string Email { get; set; }
        public string Endereco { get; set; }
        public string LatitudeLongitude { get; set; }
    }
}