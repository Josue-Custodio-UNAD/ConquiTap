using System.Security.Cryptography;

namespace ConquiTap.Helpers;

public static class PasswordHelper
{
    private const int SaltSize   = 16;
    private const int HashSize   = 32;
    private const int Iterations = 100_000;

    /// <summary>Genera un hash seguro de la contraseña usando PBKDF2/SHA-256.</summary>
    public static string HashPassword(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
        byte[] hash = pbkdf2.GetBytes(HashSize);

        byte[] combined = new byte[SaltSize + HashSize];
        Buffer.BlockCopy(salt, 0, combined, 0, SaltSize);
        Buffer.BlockCopy(hash, 0, combined, SaltSize, HashSize);
        return Convert.ToBase64String(combined);
    }

    /// <summary>Verifica si una contraseña coincide con su hash almacenado.</summary>
    public static bool VerifyPassword(string password, string storedHash)
    {
        try
        {
            byte[] combined = Convert.FromBase64String(storedHash);
            if (combined.Length != SaltSize + HashSize) return false;

            byte[] salt       = new byte[SaltSize];
            byte[] storedBytes = new byte[HashSize];
            Buffer.BlockCopy(combined, 0,        salt,        0, SaltSize);
            Buffer.BlockCopy(combined, SaltSize, storedBytes, 0, HashSize);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
            byte[] computedHash = pbkdf2.GetBytes(HashSize);

            return CryptographicOperations.FixedTimeEquals(storedBytes, computedHash);
        }
        catch { return false; }
    }

    /// <summary>Valida que la contraseña tenga mínimo 6 caracteres.</summary>
    public static bool IsPasswordValid(string password, out string message)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            message = "La contraseña no puede estar vacía.";
            return false;
        }
        if (password.Length < 6)
        {
            message = "La contraseña debe tener al menos 6 caracteres.";
            return false;
        }
        message = string.Empty;
        return true;
    }
}
