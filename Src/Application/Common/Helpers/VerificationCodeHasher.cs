using System.Security.Cryptography;
using System.Text;

namespace Application.Common.Helpers;

public static class VerificationCodeHasher
{
    public static string Hash(string code)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(code));
        return Convert.ToHexString(bytes);
    }
}
