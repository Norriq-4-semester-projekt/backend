using DataAccess.Entities;
using DataAccess.Interfaces;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    //User repository - indeholder implementationer af metoderne fra IUserRepository og IGenericRepository
    public class UserRepository : IUserRepository
    {
        private ElasticClient client;
        private ConnectionSettings settings = new ConnectionSettings(new Uri("http://164.68.106.245:9200")).DefaultIndex("users");

        public UserRepository()
        {
            settings.BasicAuthentication("elastic", "changeme");
            client = new ElasticClient(settings);
        }

        public async Task<ActionResult> Login(User entity)
        {
            try
            {
                var rs = await client.SearchAsync<User>(s => s
                    .Query(q => q
                        .MatchPhrase(mp => mp
                                    .Field("username").Query(entity.Username))));

                if (rs.Hits.Count > 0)
                {
                    User u = rs.Hits.Select(h => h.Source).FirstOrDefault<User>();
                    if (PasswordHelper.ComparePass(entity.Password, u.PasswordHash, u.Salt))
                    {
                        return new ObjectResult(u) { StatusCode = 200 };
                    }
                }
                return new StatusCodeResult(500);
            }
            catch (Exception)
            {
                //_logger.LogError(exception, "Could not retrieve any data from ElasticSearch");
                return new StatusCodeResult(500);
            }
        }

        public async Task<ActionResult> Register(User entity)
        {
            UserValidator uv = new UserValidator();
            ValidationResult result = uv.Validate(entity);
            if (!result.IsValid)
            {
                return new ObjectResult(result.Errors) { StatusCode = 500 };
            }

            try
            {
                var rs = await client.SearchAsync<User>(s => s
                    .Query(q => q
                        .MatchPhrase(mp => mp
                                    .Field("username").Query(entity.Username))));

                if (rs.Hits.Count > 0)
                {
                    return new ObjectResult("User already exists") { StatusCode = 500 };
                }
            }
            catch (Exception)
            {
                //_logger.LogError(exception, "Could not retrieve any data from ElasticSearch");
                return new StatusCodeResult(500);
            }
            entity.Salt = PasswordHelper.GenerateSalt();
            entity.PasswordHash = PasswordHelper.ComputeHash(entity.Password, entity.Salt);

            try
            {
                client.IndexDocument<User>(entity);

                return new StatusCodeResult(200);
            }
            catch (Exception)
            {
                //_logger.LogError(exception, "Could not retrieve any data from ElasticSearch");
                return new StatusCodeResult(500);
            }
        }

        //Metode til at tilføje en ny User. Bruges i forbindelse med "Register"
        public async Task<ActionResult> AddAsync(User entity)
        {
            //Validerer om password og username opfylder kravene om længde
            UserValidator uv = new UserValidator();
            ValidationResult result = uv.Validate(entity);
            if (!result.IsValid)
            {
                return new ObjectResult(result.Errors) { StatusCode = 500 };
            }

            try
            {
                //Query til Elastic som finder User ud fra Username
                var rs = await client.SearchAsync<User>(s => s
                    .Query(q => q
                        .MatchPhrase(mp => mp
                                    .Field("username").Query(entity.Username))));

                if (rs.Hits.Count > 0)
                {
                    return new StatusCodeResult(500);
                }
            }
            catch (Exception)
            {
                return new StatusCodeResult(500);
            }

            //Hasher passwordet på den nye User
            User u = new User(entity.Username);
            u.Salt = PasswordHelper.GenerateSalt();
            u.PasswordHash = PasswordHelper.ComputeHash(entity.Password, u.Salt);
            try
            {
                await client.IndexDocumentAsync<User>(u);
                return new StatusCodeResult(200);
            }
            catch (Exception)
            {
                return new StatusCodeResult(500);
            }
        }

        //Henter alle Users fra Elastic
        public async Task<IEnumerable<User>> GetAll()
        {
            try
            {
                List<User> users = new List<User>();
                var rs = await client.SearchAsync<User>(s => s
                    .Query(q => q
                        .MatchAll()));

                //Tilføjer hver User til users listen
                if (rs.Hits.Count > 0)
                {
                    foreach (var hit in rs.Hits)

                    {
                        User u = hit.Source;
                        users.Add(u);
                    }
                }

                return users;
            }
            catch (Exception)
            {
                throw;
            }
        }

        //Burde returnere én User
        public Task<User> GetByQueryAsync(User entity)
        {
            throw new NotImplementedException();
        }

        //Opdaterer brugernavnet på en User
        [HttpPost]
        public async Task<int> UpdateByQueryAsync(User currentUser, User newUser)
        {
            try
            {
                var response = await client.UpdateByQueryAsync<User>(q => q
               .Query(rq => rq
                   .MatchPhrase(m => m
                   .Field("username")
                   .Query(currentUser.Username)))
               .Script(s => s
                    .Source("ctx._source.username = params.username")
                    .Params(p => p
                        .Add("username", newUser.Username))
                    ));
            }
            catch (Exception)
            {
                return 500;
            }
            return 200;
        }

        Task<User> IGenericRepository<User>.AddAsync(User entity)
        {
            throw new NotImplementedException();
        }

        //Sletter en User fra Elastic ud fra brugernavn.
        public async Task<int> DeleteByQueryAsync(User entity)
        {
            //var id = response.Hits.Select(h => h.Id).FirstOrDefault<string>();

            try
            {
                var response = await client.DeleteByQueryAsync<User>(q => q
               .Query(rq => rq
                   .MatchPhrase(m => m
                   .Field("username")
                   .Query(entity.Username))
               ));

                //client.Delete<User>(entity);
            }
            catch (Exception)
            {
                return 500;
            }

            return 200;
        }
    }
}