using DataAccess.Entities;
using DataAccess.Interfaces;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Nest;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
                client.Delete<User>(entity);
            }
            catch (Exception)
            {
                return 500;
            }

            return 200;
        }
    }
}