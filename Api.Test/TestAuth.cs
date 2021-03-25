using NUnit.Framework;
using DataAccess;
using System.Threading.Tasks;
using DataAccess.Entities;
using DataAccess.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using Api.Controllers;
using Nest;

namespace Api.Test
{
    [TestFixture]
    class TestAuth
    {
        /*
        public class UserRepoMock : IUserRepository
        {
            private ElasticClient client;
            private ConnectionSettings settings = new ConnectionSettings(new Uri("http://164.68.106.245:9200")).DefaultIndex("users");
            public List<User> Users { get; set; } = new List<User>();
            public UserRepoMock()
            {
                settings.BasicAuthentication("elastic", "changeme"); // ElasticSearch Username and Password
                settings.ThrowExceptions(alwaysThrow: true); // I like exceptions
                settings.PrettyJson(); // Good for DEBUG
                client = new ElasticClient(settings);
            }

            public async Task<ActionResult> AddAsync(User entity)
            {
                Users.Add(entity);
                return new StatusCodeResult(200);
            }

            //Task<ActionResult> GetByQueryAsync(T entity);
            Task<IEnumerable<User>> GetAll()
            {
                //List<User> users = new List<User>();
                return Users;
            }

            Task<ActionResult> UpdateByQueryAsync(User entity, User u1);

            Task<ActionResult> DeleteByQueryAsync(User entity);
        }
        */

        [SetUp]
        public void Setup()
        {
            User user = new User("søren", "12345678");
        }

        [Test]
        public void RegisterNewUser()
        {
            /*
            AuthController controller = new AuthController
            {

            }
            Assert.Pass();
            */
        }

        [Test]
        public void TryRegisterExistingUser()
        {
            Assert.Pass();
        }

        [Test]
        public void UpdateExistingUser()
        {
            Assert.Pass();
        }

        [Test]
        public void UpdateNewUser()
        {
            Assert.Pass();
        }

        [Test]
        public void DeleteExistingUser()
        {
            Assert.Pass();
        }

        [Test]
        public void DeleteNewUser()
        {
            Assert.Pass();
        }
    }
}