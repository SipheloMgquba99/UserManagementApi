using Bogus;
using UserManagement.Application.Dtos;

namespace UserManagement.Tests.Builders
{
    public class LoginUserDtoBuilder
    {
        private readonly Faker<LoginUserDto> _faker;

        public LoginUserDtoBuilder()
        {
            _faker = new Faker<LoginUserDto>()
                .RuleFor(l => l.Email, f => f.Internet.Email())
                .RuleFor(l => l.Password, f => f.Internet.Password(8, true, null, "@1A"));
        }

        public LoginUserDto Build() => _faker.Generate();
        public LoginUserDtoBuilder WithEmail(string email)
        {
            _faker.RuleFor(l => l.Email, _ => email);
            return this;
        }
        public LoginUserDtoBuilder WithPassword(string password)
        {
            _faker.RuleFor(l => l.Password, _ => password);
            return this;
        }
    }
}
