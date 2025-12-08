using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.DataProtection;

namespace ProyectoSeguridadInformatica.Services
{
    /// <summary>
    /// Utilidad para identificar de forma estable un dispositivo usando cookie y/o IP + User-Agent.
    /// </summary>
public static class DeviceIdentifier
{
    private const string CookieName = "DEVICE_ID";
    private static readonly IDataProtector _protector;

    static DeviceIdentifier()
    {
        var provider = DataProtectionProvider.Create("ProyectoSeguridadInformatica");
        _protector = provider.CreateProtector("DeviceIdentifier.v1");
    }

    public static string GetOrCreateDeviceId(HttpContext context, bool setCookieIfMissing = true)
    {
        if (context.Request.Cookies.TryGetValue(CookieName, out var protectedId))
        {
            try
            {
                return _protector.Unprotect(protectedId);
            }
            catch
            {
                // Si no se puede desencriptar, continuamos con fallback.
            }
        }

        if (!setCookieIfMissing)
        {
            return Fingerprint(context);
        }

        // Primera vez: generar ID criptográficamente seguro
        var newId = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var protectedPayload = _protector.Protect(newId);

        context.Response.Cookies.Append(CookieName, protectedPayload, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            IsEssential = true,
            Expires = DateTimeOffset.UtcNow.AddYears(1)
        });

        return newId;
    }

    private static string Fingerprint(HttpContext context)
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var ua = context.Request.Headers["User-Agent"].ToString();
        var data = $"{ip}|{ua}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(data));
        return $"fp-{Convert.ToHexString(hash)}";
    }
}
}


