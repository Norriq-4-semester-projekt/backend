using DataAccess.Entities;
using DataAccess.Interfaces;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public class UserRepository : IUserRepository
    {
        public async Task<ActionResult> Login(User entity)
        {
            var response = await ElasticConnection.Instance.Client.SearchAsync<User>(s => s
                .Query(q => q
                    .MatchPhrase(mp => mp
                        .Field("username").Query(entity.Username))));

            if (response.Hits.Count > 0)
            {
                User user = response.Hits.Select(h => h.Source).FirstOrDefault<User>();
                if (PasswordHelper.ComparePass(entity.Password, user.Password, user.Salt))
                {
                    return new ObjectResult(user) { StatusCode = 200 };
                }

                return new ObjectResult("Incorrect username or password") { StatusCode = 500 };
            }
            return new ObjectResult("Incorrect username or password") { StatusCode = 500 };
        }

        //Metode til at tilføje en ny User. Bruges i forbindelse med "Register"
        public async Task<ActionResult> AddAsync(User entity)
        {
            UserValidator rs = new UserValidator();
            ValidationResult result = await rs.ValidateAsync(entity);
            if (!result.IsValid)
            {
                return new ObjectResult(result.Errors) { StatusCode = 500 };
            }

            var response = await ElasticConnection.Instance.Client.SearchAsync<User>(s => s
                .Query(q => q
                    .MatchPhrase(mp => mp
                        .Field("username").Query(entity.Username))));

            if (response.Hits.Count > 0)
            {
                return new ObjectResult("User already exists") { StatusCode = 500 };
            }

            entity.Salt = PasswordHelper.GenerateSalt();
            entity.Password = PasswordHelper.ComputeHash(entity.Password, entity.Salt);
            await ElasticConnection.Instance.Client.IndexDocumentAsync<User>(entity);

            return new StatusCodeResult(200);
        }

        public async Task<IEnumerable<User>> GetAll()
        {
            List<User> users = new List<User>();
            var rs = await ElasticConnection.Instance.Client.SearchAsync<User>(s => s
                .Query(q => q
                    .MatchAll()));

            if (rs.Hits.Count > 0)
            {
                users.AddRange(rs.Hits.Select(hit => hit.Source));
            }
            return users;
        }

        public async Task<ActionResult> UpdateByQueryAsync(User currentUser, User newUser)
        {
            var response = await ElasticConnection.Instance.Client.UpdateByQueryAsync<User>(q => q
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
                return new ObjectResult("User didnt exist") { StatusCode = 500 };
            }

            return new StatusCodeResult(200);
        }

        public async Task<ActionResult> DeleteByQueryAsync(User entity)
        {
            var response = await ElasticConnection.Instance.Client.DeleteByQueryAsync<User>(q => q
                .Query(rq => rq
                    .MatchPhrase(m => m
                        .Field("username")
                        .Query(entity.Username))
                ));
            if (response.Deleted == 0)
            {
                return new ObjectResult("User didnt exist") { StatusCode = 500 };
            }

            return new StatusCodeResult(200);
        }
    }
}