using DataAccess.Entities;
using NUnit.Framework;

namespace DataAccess.Test
{
    [TestFixture]
    internal class TestUser
    {
        [SetUp]
        public void Setup()
        {
            User user = new User("s�ren1234");
        }

        [Test]
        public void Test1()
        {
            Assert.Pass();
        }
    }
}