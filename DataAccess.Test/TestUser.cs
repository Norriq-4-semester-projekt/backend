using NUnit.Framework;
using DataAccess;
using System.Threading.Tasks;
using DataAccess.Entities;

namespace DataAccess.Test
{
    [TestFixture]
    class TestUser
    {
        [SetUp]
        public void Setup()
        {
            User user = new User("søren1234");
        }

        [Test]
        public void Test1()
        {
            Assert.Pass();
        }
    }
}