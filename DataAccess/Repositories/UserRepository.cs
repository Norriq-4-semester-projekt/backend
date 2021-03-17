﻿using DataAccess.Entities;
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
    public class UserRepository : IUserRepository
    {
        private ElasticClient client;
        private ConnectionSettings settings = new ConnectionSettings(new Uri("http://164.68.106.245:9200")).DefaultIndex("users");

        public UserRepository()
        {
            settings.BasicAuthentication("elastic", "changeme"); // ElasticSearch Username and Password
            settings.ThrowExceptions(alwaysThrow: true); // I like exceptions
            settings.PrettyJson(); // Good for DEBUG
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
                    else
                    {
                        throw new Exception("Incorrect username or password");
                    }
                }
                else
                {
                    throw new Exception("Incorrect username or password");
                    //return new ObjectResult("Incorrect username or password") { StatusCode = 500 };
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        //Metode til at tilføje en ny User. Bruges i forbindelse med "Register"
        public async Task<ActionResult> AddAsync(User entity)
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

        //public async Task<IEnumerable<User>> GetAll(List<User> users)
        //{
        //    try
        //    {
        //        //List<User> users = new List<User>();
        //        var rs = await client.SearchAsync<User>(s => s
        //            .Query(q => q
        //                .MatchAll()));

        //        if (rs.Hits.Count > 0)
        //        {
        //            foreach (var hit in rs.Hits)

        //            {
        //                User u = hit.Source;
        //                users.Add(u);
        //            }
        //        }
        //        return users;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        public async Task<int> GetByQueryAsync(User entity)
        {
            try
            {
                var rs = await client.SearchAsync<User>(s => s
                    .Query(q => q
                        .MatchPhrase(mp => mp
                                    .Field("username").Query(entity.Username))));
            }
            catch (Exception)
            {
                return 500;
            }
            return 200;
        }

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
                if (response.Updated == 0)
                {
                    return 500;
                }
            }
            catch (Exception)
            {
                return 500;
            }
            return 200;
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
                if (response.Deleted == 0)
                {
                    return 500;
                }

                //client.Delete<User>(entity);
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

        async Task<int> IGenericRepository<User>.GetAll(List<User> users)
        {
            try
            {
                //List<User> users = new List<User>();
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
                return 200;
            }
            catch (Exception)
            {
                return 500;
            }
        }
    }
}