using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace DBPrimes
{
    class InsertionCommand
    {
        private const int BATCH_SIZE = 1000;
        readonly string command;
        readonly List<string> items = new();

        public InsertionCommand(string command)
        {
            this.command = command;
        }

        public void Add(string item)
        {
            items.Add(item);
        }

        public IEnumerable<string> GetBatches()
        {
            var sb = new StringBuilder();

            for (int i = 0; i < items.Count; i++)
            {
                string item = items[i];
                if (i % BATCH_SIZE == 0)
                {
                    sb.AppendLine(command);
                }

                sb.Append(item);
                if ((i + 1) % BATCH_SIZE != 0 && i != items.Count - 1)
                {
                    sb.AppendLine(",");
                }
                else
                {
                    yield return sb.ToString();
                    sb.Clear();
                }
            }
        }
    }

    class Program
    {
        const string SQL_GET_START_END = @"
DECLARE @Start INT = (SELECT ISNULL(MAX([Value]), 0) + 1 FROM Factors)
DECLARE @End INT = (SELECT MAX([Value]) FROM Numbers)
DECLARE @MaxPrime INT = (SELECT ISNULL(MAX([Value]), 0) FROM Primes)
SELECT @Start, @End, @MaxPrime
";
        static void Main(string[] args)
        {
            using var sqlConn = new SqlConnection("server=localhost;database=numbers;trusted_connection=true");
            using var sqlCmd = new SqlCommand(SQL_GET_START_END, sqlConn);
            sqlConn.Open();

            int start, end, maxPrime;
            using (var reader = sqlCmd.ExecuteReader())
            {
                reader.Read();
                start = reader.GetInt32(0);
                end = reader.GetInt32(1);
                maxPrime = reader.GetInt32(2);
            }

            var commands = new List<InsertionCommand>
            {
                new InsertionCommand("INSERT INTO Primes VALUES")
            };
            var primes = GetPrimes(2, end).ToList();
            foreach (int prime in primes.Where(p => p > maxPrime))
            {
                commands.Last().Add($"({prime})");
            }

            commands.Add(new InsertionCommand("INSERT INTO Factors VALUES"));
            for (int i = start; i <= end; ++i)
            {
                foreach (int prime in primes.Where(p => p <= i))
                {
                    var m = GetMultiplicity(i, prime);
                    if (m != 0)
                    {
                        commands.Last().Add($"({i}, {prime}, {m})");
                    }
                }
            }

            foreach (InsertionCommand command in commands)
            {
                foreach (var batch in command.GetBatches())
                {
                    sqlCmd.CommandText = batch;
                    sqlCmd.ExecuteNonQuery();
                }
            }
        }


        private static int GetMultiplicity(int n, int p)
        {
            int count = 0;
            while (n % p == 0)
            {
                n /= p;
                ++count;
            }

            return count;
        }

        private static IEnumerable<int> GetPrimes(int start, int end)
        {
            for (int i = start; i <= end; ++i)
            {
                if (IsPrime(i))
                {
                    yield return i;
                }
            }
        }

        private static bool IsPrime(int n)
        {
            if (n % 2 == 0)
            {
                return n == 2;
            }
            var r = (int)Math.Sqrt(n);
            for (int i = 3; i <= r; i += 2)
            {
                if (n % i == 0)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
