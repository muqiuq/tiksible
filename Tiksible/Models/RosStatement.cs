using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tiksible.Models
{
    public enum RosCommandType
    {
        ADD = 1,
        SET = 2
    }

    public class RosStatement
    {
        public SortedDictionary<string, string> Properties { get; set; }
        public RosCommandType Command { get; set; }
        public string Path { get; set; }
        public string Condition { get; set; }

        public RosStatement(RosCommandType command, string path, IDictionary<string, string> properties = null, string condition = null)
        {
            Properties = properties  == null ? new SortedDictionary<string, string>() : new SortedDictionary<string, string>(properties);
            Command = command;
            Path = path;
            Condition = condition;
        }

        public string Export()
        {
            var buffer = new StringBuilder($"/{Path} {Command.ToString().ToLower()}");
            if (!string.IsNullOrEmpty(Condition))
            {
                buffer.Append($" [ {Condition} ]");
            }

            foreach (var prop in Properties)
            {
                if (prop.Value == null)
                {
                    buffer.Append($" {prop.Key}");
                }
                else if (prop.Value.Contains(" ") || prop.Value.Contains("="))
                {
                    buffer.Append($" {prop.Key}=\"{prop.Value}\"");
                }
                else
                {
                    buffer.Append($" {prop.Key}={prop.Value}");
                }
            }

            return buffer.ToString();
        }

        public override string ToString() => Export();

        public override int GetHashCode() => Export().GetHashCode();

        public override bool Equals(object obj) => obj is RosStatement statement && Export() == statement.Export();
    }
}
