using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class ResponseMessage
    {
        public string Msg { get; set; }
        public string Status { get; set; }
        public int Code { get; set; }
    }
}