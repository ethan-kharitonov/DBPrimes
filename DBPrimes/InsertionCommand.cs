using System.Collections.Generic;
using System.Text;

namespace DBPrimes
{
    class InsertionCommand
    {
        private const int BATCH_SIZE = 1000;
        readonly string command;
        readonly StringBuilder sb = new();
        int count = 0;
        string delim = "";

        public InsertionCommand(string command)
        {
            this.command = command;
        }

        public string Query => sb.ToString();

        public void Add(string item)
        {
            if (count == 0)
            {
                sb.AppendLine();
                sb.Append(command);
                delim = "";
            }
            sb.AppendLine(delim);
            sb.Append(item);
            delim = ",";
            ++count;
            if (count == BATCH_SIZE)
            {
                count = 0;
            }
        }
    }
}
