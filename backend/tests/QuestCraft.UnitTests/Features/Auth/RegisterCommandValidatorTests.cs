using QuestCraft.Application.Features.Auth.Commands.Register;

namespace QuestCraft.UnitTests.Features.Auth;

public class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _validator = new();

    private RegisterCommand MakeCommand(string email) =>
        new(Username: "validuser", FirstName: "Ali", LastName: "Test", Email: email, Password: "Passw0rd!");

    [Theory]
    [InlineData("ali@gmail.com")]
    [InlineData("ali123@gmail.com")]
    [InlineData("ali.test@gmail.com")]
    public void Validate_ValidEmail_HasNoEmailError(string email)
    {
        var result = _validator.Validate(MakeCommand(email));

        Assert.DoesNotContain(result.Errors, e => e.PropertyName == nameof(RegisterCommand.Email));
    }

    [Theory]
    [InlineData("ali@gmailcom")]
    [InlineData("ali@gmail.")]
    [InlineData("@gmail.com")]
    [InlineData("ali@")]
    [InlineData("ali..test@gmail.com")]
    [InlineData("ali @gmail.com")]
    public void Validate_InvalidEmail_HasEmailError(string email)
    {
        var result = _validator.Validate(MakeCommand(email));

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterCommand.Email));
    }
}
