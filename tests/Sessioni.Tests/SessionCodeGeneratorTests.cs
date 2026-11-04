using System.Text.RegularExpressions;
using Sessioni.Services;
using Xunit;

public class SessionCodeGeneratorTests
{
    [Fact]
    public void Generate_Returns_8_Uppercase_Alphanumeric()
    {
        var gen = new SessionCodeGenerator();
        var code = gen.Generate();
        Assert.Matches(new Regex("^[A-Z0-9]{8}$"), code);
    }

    [Fact]
    public void Generate_Is_Reasonably_Unique()
    {
        var gen = new SessionCodeGenerator();
        var set = new HashSet<string>();
        for (var i = 0; i < 1000; i++) set.Add(gen.Generate());
        Assert.True(set.Count > 990); // collisioni rarissime
    }
}
