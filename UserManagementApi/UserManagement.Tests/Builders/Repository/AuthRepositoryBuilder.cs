using Bogus;
using System;
using UserManagement.Domain.Entities;

namespace UserManagement.Tests.Builders.Repository
{
    public class AuthRepositoryBuilder
    {
        private readonly Faker<ApplicationUser> _faker;

        public AuthRepositoryBuilder()
        {
            _faker = new Faker<ApplicationUser>()
                .RuleFor(u => u.Id, f => Guid.NewGuid())
                .RuleFor(u => u.Email, f => f.Internet.Email())
                .RuleFor(u => u.Password, f => "Test12345#");
        }

        public ApplicationUser Build()
        {
            return _faker.Generate();
        }

        public List<ApplicationUser> BuildMany(int count)
        {
            return _faker.Generate(count);
        }
    }
}
