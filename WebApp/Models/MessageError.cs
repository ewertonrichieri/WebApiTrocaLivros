using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class MessageError
    {
       
        public string id { get; set; }
        public string msg { get; set; }
        public string status { get; set; }
        public int code { get; set; }

    }
}