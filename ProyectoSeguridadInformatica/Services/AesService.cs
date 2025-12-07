using System.Security.Cryptography;
using System.Text;

namespace ProyectoSeguridadInformatica.Services
{
    /// <summary>
    /// Servicio de cifrado simétrico basado en AES con contraseña de usuario.
    /// Deriva la clave con PBKDF2 (Rfc2898DeriveBytes) y empaqueta Salt+IV+cipher en Base64.
    /// </summary>
    public class AesService
    {
        private const int KeySizeBytes = 32;   // 256 bits
        private const int IvSizeBytes = 16;    // 128 bits (bloque AES)
        private const int SaltSizeBytes = 16;  // 128 bits de salt
        private const int Iterations = 600_000; // PBKDF2 iterations

        public string Encrypt(string plainText, string password)
        {
            if (string.IsNullOrEmpty(plainText))
                throw new ArgumentException("El texto plano no puede estar vacío.", nameof(plainText));

            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("La clave no puede estar vacía.", nameof(password));

            var salt = RandomNumberGenerator.GetBytes(SaltSizeBytes);

            using var aes = Aes.Create();
            aes.KeySize = KeySizeBytes * 8;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.GenerateIV();

            using var derive = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
            aes.Key = derive.GetBytes(KeySizeBytes);

            var plainBytes = Encoding.UTF8.GetBytes(plainText);

            using var encryptor = aes.CreateEncryptor();
            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            {
                cs.Write(plainBytes, 0, plainBytes.Length);
                cs.FlushFinalBlock();
            }

            var cipherBytes = ms.ToArray();

            // [salt][IV][cipherText]
            var result = new byte[SaltSizeBytes + IvSizeBytes + cipherBytes.Length];
            Buffer.BlockCopy(salt, 0, result, 0, SaltSizeBytes);
            Buffer.BlockCopy(aes.IV, 0, result, SaltSizeBytes, IvSizeBytes);
            Buffer.BlockCopy(cipherBytes, 0, result, SaltSizeBytes + IvSizeBytes, cipherBytes.Length);

            return Convert.ToBase64String(result);
        }

        public string Decrypt(string cipherTextBase64, string password)
        {
            if (string.IsNullOrEmpty(cipherTextBase64))
                throw new ArgumentException("El texto cifrado no puede estar vacío.", nameof(cipherTextBase64));

            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("La clave no puede estar vacía.", nameof(password));

            var allBytes = Convert.FromBase64String(cipherTextBase64);
            if (allBytes.Length < SaltSizeBytes + IvSizeBytes)
                throw new FormatException("El texto cifrado no tiene el formato esperado.");

            var salt = new byte[SaltSizeBytes];
            var iv = new byte[IvSizeBytes];
            var cipherBytes = new byte[allBytes.Length - SaltSizeBytes - IvSizeBytes];

            Buffer.BlockCopy(allBytes, 0, salt, 0, SaltSizeBytes);
            Buffer.BlockCopy(allBytes, SaltSizeBytes, iv, 0, IvSizeBytes);
            Buffer.BlockCopy(allBytes, SaltSizeBytes + IvSizeBytes, cipherBytes, 0, cipherBytes.Length);

            using var aes = Aes.Create();
            aes.KeySize = KeySizeBytes * 8;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.IV = iv;

            using var derive = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
            aes.Key = derive.GetBytes(KeySizeBytes);

            using var decryptor = aes.CreateDecryptor();
            using var ms = new MemoryStream(cipherBytes);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs, Encoding.UTF8);

            return sr.ReadToEnd();
        }
    }
}


