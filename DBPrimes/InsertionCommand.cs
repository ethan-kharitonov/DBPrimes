using System.Collections.Generic;
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
}
