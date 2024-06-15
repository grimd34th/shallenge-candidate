using System;
using System.Security.Cryptography;
using System.Text;

public class LowestHashFinder
{
    public static void Main()
    {
        string username = "GenericToon";
        FindLowestHash(username);
    }

    public static void FindLowestHash(string username)
    {
        int nonce = 0;
        string lowestHash = null;
        int lowestNonce = 0;
        
        while (true)
        {
            string data = $"{username}/{nonce}";
            string hashValue = CalculateSHA256Hash(data);

            if (lowestHash == null || string.Compare(hashValue, lowestHash, StringComparison.OrdinalIgnoreCase) < 0)
            {
                lowestHash = hashValue;
                lowestNonce = nonce;
            }

            nonce++;

            // You can adjust the range of nonce values or add a condition to stop based on a specific criteria
            if (nonce > System.Int32.MaxValue)
                break;
        }

        Console.WriteLine($"Lowest hash: {lowestHash}, achieved with {username}/{lowestNonce}");
    }

    public static string CalculateSHA256Hash(string input)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }
    }
}