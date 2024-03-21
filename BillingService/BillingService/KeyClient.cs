using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace BillingService;

public class KeyClient
{
    private HttpClient _httpClient;
    private string jwkUrl = "https://localhost:7018/key";

    public KeyClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<SecurityKey> GetSigningKeyAsync()
    {
        var response = await _httpClient.GetAsync(jwkUrl);

        var bytes = Convert.FromBase64String(await response.Content.ReadAsStringAsync());

        var rsa = RSA.Create();
        rsa.ImportRSAPublicKey(bytes, out _);
        return new RsaSecurityKey(rsa);
    }
}

