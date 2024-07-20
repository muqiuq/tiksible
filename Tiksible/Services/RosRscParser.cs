using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tiksible.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Tiksible.Services
{
    internal enum ParserState
    {
        NONE = 0,
        COMMENT = 1,
        PATH = 2,
        STATEMENT = 3,
        PROPERTY = 4,
        PROPERTY_VALUE_START = 5,
        PROPERTY_VALUE_START_NEWLINE = 6,
        PROPERTY_VALUE_QUOTED = 7,
        PROPERTY_VALUE_QUOTED_ESCAPE = 8,
        PROPERTY_VALUE_QUOTED_ESCAPE_NEWLINE = 9,
        PROPERTY_VALUE_UNQUOTED = 10,
        STATEMENT_CONDITION = 11,
        NOW_END_STATEMENT = 100
    }

    public class RosRscParser
    {
        public RosRscParser() { }

        public static RosStatementList Parse(string infile)
        {
            var statements = new RosStatementList();
            var buffer = "";
            int head = 0;
            var path = "";
            var c = ParserState.NONE;
            var propertyKey = "";
            var propertyValue = "";
            int escapeStart = -1;
            var commandType = "";
            bool nextlineIncluded = false;
            var condition = "";
            var properties = new Dictionary<string, string>();
            infile += "\n";
            infile = infile.Replace("\r", "");

            while (head < infile.Length)
            {
                var ch = infile[head];
                switch (c)
                {
                    case ParserState.NONE when ch == '#':
                        c = ParserState.COMMENT;
                        break;
                    case ParserState.NONE when ch == '/':
                        c = ParserState.PATH;
                        break;
                    case ParserState.COMMENT when ch == '\n':
                        buffer = "";
                        c = ParserState.NONE;
                        break;
                    case ParserState.PATH when ch == '\n':
                        path = buffer;
                        buffer = "";
                        c = ParserState.NONE;
                        break;
                    case ParserState.PATH when Regex.IsMatch(buffer, @"(?:\s|\\n)(add|set)(?:\s|\\n)$"):
                        c = ParserState.STATEMENT;
                        var words = buffer.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                        commandType = words.Last();
                        path = string.Join(" ", words, 0, words.Length - 1);
                        buffer = "";
                        head -= 1;
                        break;
                    case ParserState.NONE when buffer == "add" || buffer == "set":
                        c = ParserState.STATEMENT;
                        commandType = buffer.Trim();
                        buffer = "";
                        break;
                    case ParserState.STATEMENT when ch == '[':
                        c = ParserState.STATEMENT_CONDITION;
                        buffer = "";
                        break;
                    case ParserState.STATEMENT_CONDITION when ch == ']':
                        c = ParserState.STATEMENT;
                        condition = buffer.Trim();
                        buffer = "";
                        break;
                    case ParserState.STATEMENT when ch != ' ' && ch != '\n' && ch != '\\':
                        c = ParserState.PROPERTY;
                        buffer += ch;
                        break;
                    case ParserState.PROPERTY when ch == ' ' && !string.IsNullOrWhiteSpace(buffer):
                        c = ParserState.STATEMENT;
                        propertyKey = buffer.Trim();
                        if(!properties.ContainsKey(propertyKey)) properties.Add(propertyKey, null);
                        buffer = "";
                        break;
                    case ParserState.PROPERTY when ch == '=':
                        c = ParserState.PROPERTY_VALUE_START;
                        propertyKey = buffer.Trim();
                        buffer = "";
                        break;
                    case ParserState.PROPERTY_VALUE_START when ch != '\\':
                        c = ch == '"' ? ParserState.PROPERTY_VALUE_QUOTED : ParserState.PROPERTY_VALUE_UNQUOTED;
                        if (c == ParserState.PROPERTY_VALUE_UNQUOTED)
                            buffer += ch;
                        break;
                    case ParserState.PROPERTY_VALUE_START when ch == '\\':
                        c = ParserState.PROPERTY_VALUE_START_NEWLINE;
                        break;
                    case ParserState.PROPERTY_VALUE_START_NEWLINE when ch != '\n' && ch != ' ' && ch != '"':
                        c = ParserState.PROPERTY_VALUE_START;
                        if (ch != '"')
                            buffer += ch;
                        break;
                    case ParserState.PROPERTY_VALUE_START_NEWLINE when ch != '\n' && ch != ' ' && ch == '"':
                        c = ch == '"' ? ParserState.PROPERTY_VALUE_QUOTED : ParserState.PROPERTY_VALUE_UNQUOTED;
                        if (c == ParserState.PROPERTY_VALUE_UNQUOTED)
                            buffer += ch;
                        break;
                    case ParserState.PROPERTY_VALUE_QUOTED when ch == '\\':
                        c = ParserState.PROPERTY_VALUE_QUOTED_ESCAPE;
                        break;
                    case ParserState.PROPERTY_VALUE_QUOTED_ESCAPE:
                        if (ch != '\n')
                        {
                            buffer += "\\" + ch;
                            c = ParserState.PROPERTY_VALUE_QUOTED;
                        }
                        else
                        {
                            c = ParserState.PROPERTY_VALUE_QUOTED_ESCAPE_NEWLINE;
                        }
                        break;
                    case ParserState.PROPERTY_VALUE_QUOTED_ESCAPE_NEWLINE:
                        if (ch != ' ')
                        {
                            c = ParserState.PROPERTY_VALUE_QUOTED;
                            head--;
                        }
                        break;
                    case ParserState.PROPERTY_VALUE_UNQUOTED when ch == ' ' || ch == '\n':
                    case ParserState.PROPERTY_VALUE_QUOTED when ch == '"':
                        propertyValue = buffer.Trim();
                        buffer = "";
                        if (!properties.ContainsKey(propertyKey)) properties.Add(propertyKey, null);
                        properties[propertyKey] = propertyValue;
                        c = (c == ParserState.PROPERTY_VALUE_UNQUOTED && ch == '\n') ? ParserState.NOW_END_STATEMENT : ParserState.STATEMENT;
                        break;
                    case ParserState.STATEMENT when ch == '\\':
                        nextlineIncluded = true;
                        break;
                    case ParserState.STATEMENT when ch == '\n' && !nextlineIncluded:
                        c = ParserState.NOW_END_STATEMENT;
                        break;
                    case ParserState.STATEMENT when ch == '\n' && nextlineIncluded:
                        nextlineIncluded = false;
                        buffer = "";
                        break;
                    default:
                        buffer += ch;
                        break;
                }

                if (c == ParserState.NOW_END_STATEMENT)
                {
                    path = Regex.Replace(path.Replace("/", " ").Trim(), @"\s+", " ");
                    var statement =
                        new RosStatement((RosCommandType)Enum.Parse(typeof(RosCommandType), commandType.ToUpper()),
                            path, properties);
                    properties = new Dictionary<string, string>();
                    if (!string.IsNullOrEmpty(condition))
                    {
                        statement.Condition = condition;
                    }
                    condition = "";
                    statements.Add(statement);
                    c = ParserState.NONE;
                }

                head++;
            }

            return statements;
        }
    }
}
