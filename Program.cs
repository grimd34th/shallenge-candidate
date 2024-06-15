using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

public class LowestHashFinderOptimized
{
    private static int MaxConcurrency = Environment.ProcessorCount; // Adjust based on system capabilities
    private const int TotalNonces = System.Int32.MaxValue; // Total number of nonce values to check
    private static readonly object lockObject = new();

    public static int Main(string[] args)
    {
        if (args.Length == 0)
        {
             System.Console.WriteLine("Please enter a username");
            return 1;
        } else {
        string username = args[0];
        FindLowestHashParallel(username);
        return 0;
        }
    }

    public static void FindLowestHashParallel(string username)
    {
        string lowestHash = string.Empty;
        int lowestNonce = 0;

        // Calculate number of nonces per partition
        int noncesPerPartition = TotalNonces / MaxConcurrency;
        
        // Partition nonces into smaller segments
        List<Task> tasks = [];

        for (int partition = 0; partition < MaxConcurrency; partition++)
        {
            int startNonce = partition * noncesPerPartition;
            int endNonce = Math.Min(startNonce + noncesPerPartition, TotalNonces);

            tasks.Add(Task.Run(() =>
            {
                string localLowestHash = string.Empty;
                int localLowestNonce = 0;

                for (int nonce = startNonce; nonce < endNonce; nonce++)
                {
                    string data = $"{username}/{nonce}";
                    string hashValue = CalculateSHA256Hash(data);

                    lock (lockObject) // Ensure thread-safe access to shared variables
                    {
                        if (localLowestHash == string.Empty || string.Compare(hashValue, localLowestHash, StringComparison.OrdinalIgnoreCase) < 0)
                        {
                            localLowestHash = hashValue;
                            localLowestNonce = nonce;
                        }
                    }
                }

                // Merge local results into global result
                lock (lockObject)
                {
                    if (lowestHash == string.Empty || (localLowestHash != null && string.Compare(localLowestHash, lowestHash, StringComparison.OrdinalIgnoreCase) < 0))
                    {
                        lowestHash = localLowestHash;
                        lowestNonce = localLowestNonce;
                    }
                }
            }));
        }

        // Wait for all tasks to complete
        Task.WaitAll([.. tasks]);

        Console.WriteLine($"Lowest hash: {lowestHash}, achieved with {username}/{lowestNonce}");
    }

    public static string CalculateSHA256Hash(string input)
    {
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        StringBuilder builder = new();
        for (int i = 0; i < bytes.Length; i++)
        {
            builder.Append(bytes[i].ToString("x2"));
        }
        return builder.ToString();
    }
}