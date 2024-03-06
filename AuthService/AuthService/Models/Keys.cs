using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Models;

public class Keys
{
    public RSA Rsa;
    public RsaSecurityKey RsaSecurityKey;
    public Keys(IWebHostEnvironment env)
    {
        Rsa = RSA.Create();
        var path = Path.Combine(env.ContentRootPath, "key");
        if (File.Exists(path))
        {
            Rsa.ImportRSAPrivateKey(File.ReadAllBytes(path), out _);
        }
        else
        {
            var privateKey = Rsa.ExportRSAPrivateKey();
            File.WriteAllBytes(path, privateKey);
        }

        RsaSecurityKey = new RsaSecurityKey(Rsa);
        RsaSecurityKey.KeyId = Guid.NewGuid().ToString();
    }
}