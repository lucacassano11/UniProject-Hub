using System.Security.Cryptography;

namespace Sessioni.Services;

public class SessionCodeGenerator : ISessionCodeGenerator
{
    private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public string Generate()
    {
        var chars = new char[8];
        for (var i = 0; i < 8; i++)
            chars[i] = Alphabet[RandomNumberGenerator.GetInt32(Alphabet.Length)];
        return new string(chars);
    }
}
