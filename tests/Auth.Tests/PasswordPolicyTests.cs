using Auth.Services;
using Xunit;

public class PasswordPolicyTests
{
    private readonly PasswordPolicy _policy = new();

    [Theory]
    [InlineData("Abcdef1!", true)]
    [InlineData("short1!A", true)]
    [InlineData("abcdefg1!", false)]
    [InlineData("ABCDEFG1!", false)]
    [InlineData("Abcdefgh!", false)]
    [InlineData("Abcdefg1", false)]
    [InlineData("Ab1!", false)]
    public void IsValid_Checks_Complexity(string pwd, bool expected)
    {
        Assert.Equal(expected, _policy.IsValid(pwd));
    }

    [Fact]
    public void Hash_Then_Verify_Roundtrips()
    {
        var hash = _policy.Hash("Abcdef1!");
        Assert.NotEqual("Abcdef1!", hash);
        Assert.True(_policy.Verify("Abcdef1!", hash));
        Assert.False(_policy.Verify("Wrong1!A", hash));
    }
}
