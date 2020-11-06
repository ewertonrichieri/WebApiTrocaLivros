using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class MongoDBModel
    {
        //string conexaoMongo = "mongodb+srv://Livros:livros@cluster0.n5wca.azure.mongodb.net/Livros?retryWrites=true&w=majority";
        private string conexaoMongo = "mongodb+srv://Livros:Livros@trocalivrostcc.nsbyt.gcp.mongodb.net/Livros?retryWrites=true&w=majority";
        private string dt = "TrocaLivro";

        public IMongoCollection<Usuario> GetUsuarios()
        {
            IMongoClient client = new MongoClient(conexaoMongo);
            IMongoDatabase database = client.GetDatabase(dt);
            IMongoCollection<Usuario> usuarios = database.GetCollection<Usuario>("Usuario");

            return usuarios;
        }

        public Usuario GetUsuarioId(string id)
        {
            IMongoClient client = new MongoClient(conexaoMongo);
            IMongoDatabase database = client.GetDatabase(dt);
            Usuario usuario = database.GetCollection<Usuario>("Usuario").AsQueryable().Where(c=> c.Id == ObjectId.Parse(id)).FirstOrDefault();

            return usuario;
        }

        public IMongoCollection<Livro> GetLivros()
        {
            IMongoClient client = new MongoClient(conexaoMongo);
            IMongoDatabase database = client.GetDatabase(dt);
            IMongoCollection<Livro> livros = database.GetCollection<Livro>("Livro");

            return livros;
        }

        public List<Livro> GetTituloLivro(string titulo)
        {
            IMongoClient client = new MongoClient(conexaoMongo);
            IMongoDatabase database = client.GetDatabase(dt);
            List<Livro> livros = database.GetCollection<Livro>("Livro").AsQueryable().Where(c=> c.Titulo == titulo).ToList();

            return livros;
        }

        public List<Livro> GetConsultaLivroUsuarioLogado(string idUsuarioLogado) {

            IMongoClient client = new MongoClient(conexaoMongo);
            IMongoDatabase database = client.GetDatabase(dt);
            List<Livro> livros = database.GetCollection<Livro>("Livro").AsQueryable().Where(c => c.idUsuarioLivro == idUsuarioLogado).ToList();

            return livros;
        }

    }
}