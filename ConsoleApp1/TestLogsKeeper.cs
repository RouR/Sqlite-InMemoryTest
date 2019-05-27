using System.Collections.Generic;
using System.Linq;

namespace Test
{
    public class TestLogsKeeper : IWriter
    {
        private readonly List<string> _logs = new List<string>(200);

        public void WriteLine(string str)
        {
            _logs.Add(str);
        }

        public List<string> GetLogs() => _logs;
        public List<string> GetSQL() => _logs.Where(x => x.Contains("Executing DbCommand")).ToList();
        public string GetSQLAll() => string.Join("\n\n",GetSQL());
    }
}