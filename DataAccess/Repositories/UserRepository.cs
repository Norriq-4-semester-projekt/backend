using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Helpers;
using DataAccess.Entities;
using DataAccess.Interfaces;
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
        public Task<User> AddAsync(User entity)
        {
            throw new NotImplementedException();
        }

        public Task<User> DeleteByQueryAsync(User entity)
        {
            throw new NotImplementedException();
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

    }
}
