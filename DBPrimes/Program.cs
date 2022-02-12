using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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

            var primes = GetPrimes(2, end).ToList();

            var cmd = new InsertionCommand("INSERT INTO Factors VALUES");
            for (int i = start; i <= end; ++i)
            {
                foreach (int prime in primes.Where(p => p <= i))
                {
                    var m = GetMultiplicity(i, prime);
                    if (m != 0)
                    {
                        cmd.Add($"({i}, {prime}, {m})");
                    }
                }
            }

            foreach (var batch in cmd.GetBatches())
            {
                sqlCmd.CommandText = batch;
                sqlCmd.ExecuteNonQuery();
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
