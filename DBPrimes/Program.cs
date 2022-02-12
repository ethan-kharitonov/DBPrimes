using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;

namespace DBPrimes
{
    class Program
    {
        static void Main(string[] args)
        {
            int end = Convert.ToInt32(args[0]);

            const string SQL_GET_START = "SELECT ISNULL(MAX(Value), 0) + 1 FROM Factors";

            using var sqlConn = new SqlConnection("server=localhost;database=numbers;trusted_connection=true");
            using var sqlCmd = new SqlCommand(SQL_GET_START, sqlConn);
            sqlConn.Open();
            var start = (int)sqlCmd.ExecuteScalar();

            if (end <= start)
            {
                const string SQL_DELETE_RECORDS = "DELETE FROM Factors WHERE [Value] > @end";
                sqlCmd.CommandText = SQL_DELETE_RECORDS;
                sqlCmd.Parameters.Add(new SqlParameter("@end", end));
                sqlCmd.ExecuteNonQuery();
                return;
            }

            var primeFactors = new Dictionary<int, Dictionary<int, int>>();
            const string SQL_GET_FACTORS = "SELECT [Value], Prime, Degree FROM Factors";
            sqlCmd.CommandText = SQL_GET_FACTORS;

            int value;
            using (var reader = sqlCmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    value = reader.GetInt32(0);
                    if (!primeFactors.TryGetValue(value, out var factors))
                    {
                        primeFactors[value] = factors = new();
                    }

                    factors.Add(reader.GetInt32(1), reader.GetInt32(2));
                }
            }

            var primes = GetPrimes(2, end).ToList();

            for (int i = start; i <= end; ++i)
            {
                var found = primes.BinarySearch(i);
                if (found >= 0)
                {
                    primeFactors.Add(i, new Dictionary<int, int>
                    {
                        { i, 1}
                    });
                    continue;
                }

                foreach (int prime in primes)
                {
                    if (i % prime == 0)
                    {
                        var f = new Dictionary<int, int>(primeFactors[i / prime]);
                        f.TryGetValue(prime, out var degree);
                        f[prime] = degree + 1;
                        primeFactors.Add(i, f);
                        break;
                    }
                }
            }

            var cmd = new InsertionCommand("INSERT INTO Factors VALUES");
            foreach (var (num, f) in primeFactors.Where(e => e.Key >= start).OrderBy(e => e.Key))
            {
                foreach (var (prime, degree) in f.OrderBy(e => e.Key))
                {
                    cmd.Add($"({num}, {prime}, {degree})");
                }
            }

            sqlCmd.CommandText = cmd.Query;
            sqlCmd.CommandTimeout = 600;

            var sw = Stopwatch.StartNew();
            sqlCmd.ExecuteNonQuery();
            Console.WriteLine(sw.Elapsed);
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
