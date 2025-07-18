using Bogus;
using UserManagement.Application.Dtos;

namespace UserManagement.Tests.Builders
{
    public class RegisterUserDtoBuilder
    {
        private readonly Faker<RegisterUserDto> _faker;

        public RegisterUserDtoBuilder()
        {
            _faker = new Faker<RegisterUserDto>()
                .RuleFor(r => r.FirstName, f => f.Name.FirstName())
                .RuleFor(r => r.LastName, f => f.Name.LastName())
                .RuleFor(r => r.Email, f => f.Internet.Email())
                .RuleFor(r => r.Password, f => f.Internet.Password(8, true, null, "@1A"))
                .RuleFor(r => r.ConfirmPassword, (f, r) => r.Password);
        }

        public RegisterUserDto Build() => _faker.Generate();

        public RegisterUserDtoBuilder WithEmail(string email)
        {
            _faker.RuleFor(r => r.Email, _ => email);
            return this;
        }

        public RegisterUserDtoBuilder WithPassword(string password)
        {
            _faker.RuleFor(r => r.Password, _ => password)
                  .RuleFor(r => r.ConfirmPassword, _ => password);
            return this;
        }

        public RegisterUserDtoBuilder WithFirstName(string firstName)
        {
            _faker.RuleFor(r => r.FirstName, _ => firstName);
            return this;
        }

        public RegisterUserDtoBuilder WithLastName(string lastName)
        {
            _faker.RuleFor(r => r.LastName, _ => lastName);
            return this;
        }
    }
}
