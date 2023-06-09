using System.Security.Cryptography;
using System.Text;

namespace Notatez.Models.Helpers;

// Encryption with encoding, brute-force attack prevension, obfuscation
// Encrypt any data such as confidential info like password or even ssn
public static class EncryptionHelper
{
    private static readonly string EncryptionKey = "i:v$O,8<d#}^V$*C}Q~KaAp0F-/lwgZOWgE07V/uM\"w(NPVjc,Ush/[s&:be+)q";
    private static readonly int IterationCount = 100000;
    private static readonly int KeySizeInBytes = 256 / 8;
    private static readonly int BlockSizeInBytes = 128 / 8;

    public static string EncryptData(string data)
    {
        byte[] clearBytes = Encoding.Unicode.GetBytes(data);
        byte[] encryptedBytes;

        using (Aes encryptor = Aes.Create())
        {
            using (PasswordDeriveBytes pdb = new PasswordDeriveBytes(EncryptionKey, null))
            {
                pdb.IterationCount = IterationCount;

                encryptor.KeySize = KeySizeInBytes * 8;
                encryptor.BlockSize = BlockSizeInBytes * 8;

                encryptor.Key = pdb.GetBytes(KeySizeInBytes);
                encryptor.IV = pdb.GetBytes(BlockSizeInBytes);
            }

            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(clearBytes, 0, clearBytes.Length);
                    cs.Close();
                }

                encryptedBytes = ms.ToArray();
            }
        }

        // Additional encryption technique: Base64 encoding
        string encryptedData = Convert.ToBase64String(encryptedBytes);

        return encryptedData;
    }

    // Decrypt, only use when necessary, other wise use MatchData
    public static string DecryptData(string encryptedData)
    {
        byte[] encryptedBytes = Convert.FromBase64String(encryptedData);
        byte[] clearBytes;

        using (Aes encryptor = Aes.Create())
        {
            using (PasswordDeriveBytes pdb = new PasswordDeriveBytes(EncryptionKey, null))
            {
                pdb.IterationCount = IterationCount;

                encryptor.KeySize = KeySizeInBytes * 8;
                encryptor.BlockSize = BlockSizeInBytes * 8;

                encryptor.Key = pdb.GetBytes(KeySizeInBytes);
                encryptor.IV = pdb.GetBytes(BlockSizeInBytes);
            }

            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(encryptedBytes, 0, encryptedBytes.Length);
                    cs.Close();
                }

                clearBytes = ms.ToArray();
            }
        }

        string decryptedData = Encoding.Unicode.GetString(clearBytes);

        return decryptedData;
    }

    // To be more secure, rather than returning the decrypted data
    // this returns true if the inputted data and the encrypted data is a matc
    public static bool MatchData(string inputData, string encryptedData)
    {
        byte[] encryptedBytes = Convert.FromBase64String(encryptedData);
        byte[] clearBytes;

        using (Aes encryptor = Aes.Create())
        {
            using (PasswordDeriveBytes pdb = new PasswordDeriveBytes(EncryptionKey, null))
            {
                pdb.IterationCount = IterationCount;

                encryptor.KeySize = KeySizeInBytes * 8;
                encryptor.BlockSize = BlockSizeInBytes * 8;

                encryptor.Key = pdb.GetBytes(KeySizeInBytes);
                encryptor.IV = pdb.GetBytes(BlockSizeInBytes);
            }

            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(encryptedBytes, 0, encryptedBytes.Length);
                    cs.Close();
                }

                clearBytes = ms.ToArray();
            }
        }

        string decryptedData = Encoding.Unicode.GetString(clearBytes);

        return inputData == decryptedData;
    }
}
