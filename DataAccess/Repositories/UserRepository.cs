using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.Entities;
using DataAccess.Interfaces;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Nest;


namespace DataAccess.Repositories
{
    public class UserRepository : IUserRepository
    {
        private ElasticClient client;
        private ConnectionSettings settings = new ConnectionSettings(new Uri("http://164.68.106.245:9200")).DefaultIndex("users");

        public UserRepository()
        {
            client = new ElasticClient(settings);
        }

        public async Task<ActionResult> AddAsync(User entity)
        {
            UserValidator uv = new UserValidator();
            ValidationResult result = uv.Validate(entity);
            if (!result.IsValid)
            {
                return new ObjectResult(result.Errors) { StatusCode = 500 };
            }

            var settings = new ConnectionSettings(new Uri("http://164.68.106.245:9200")).DefaultIndex("users");
            var client = new ElasticClient(settings);

            try
            {
                var rs = await client.SearchAsync<User>(s => s
                    .Query(q => q
                        .MatchPhrase(mp => mp
                                    .Field("username").Query(entity.Username))));

                if (rs.Hits.Count > 0)
                {
                    return new StatusCodeResult(500);
                }

                return new StatusCodeResult(200);
            } 
            catch (Exception)
            {
                //_logger.LogError(exception, "Could not retrieve any data from ElasticSearch");
                return new StatusCodeResult(500);
            }

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
                //_logger.LogError(exception, "Could not retrieve any data from ElasticSearch");
                return new StatusCodeResult(500);
            }
        }

        public ActionResult<User> DeleteByQueryAsync(User entity)
        {

            var response = client.Search<User>(q => q
            .Query(rq => rq
                .MatchPhrase(m => m
                .Field(f => f.Username)
                .Query(entity.ToString()))
            ));

            var id = "";

            var UseristWithIds = response.Hits.Select(h =>
            {
                id = h.Id;
                return h.Source;
            }).ToList();

            try
            {
                client.Delete<User>(id);
            }
            catch (Exception)
            {
                throw; 
            }
           
            return new StatusCodeResult(200);

        }

        public async Task<IEnumerable<User>> GetAll()
        {
            try
            {
                List<User> users = new List<User>();
                var rs = await client.SearchAsync<User>(s => s
                    .Query(q => q
                        .MatchAll()));

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
                //_logger.LogError(exception, "Could not retrieve any data from ElasticSearch");
            }
        }

        public Task<User> GetByQueryAsync(User entity)
        {
            throw new NotImplementedException();
        }

        public Task<User> UpdateAsync(User entity)
        {
            throw new NotImplementedException();
        }

        Task<User> IGenericRepository<User>.AddAsync(User entity)
        {
            throw new NotImplementedException();
        }

        Task<User> IGenericRepository<User>.DeleteByQueryAsync(User entity)
        {
            throw new NotImplementedException();
        }
    }
}