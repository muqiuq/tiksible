using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Tiksible.Models;
using Tiksible.Services.RosRscStatementCleaners;

namespace Tiksible.Services
{
    public class RosRscStatementCleaner
    {

        public static void RemoveNotRequiredParameters(RosStatementList rosStatementList)
        {
            var statementCleaners = GetStatementCleaners();
            foreach (var statement in rosStatementList)
            {
                var relevantStatementCleaners = statementCleaners.Where(s => s.Match(statement.Path)).ToList();
                relevantStatementCleaners.ForEach(rs => rs.Clean(statement));
            }
        }

        public static List<IStatementCleaner> GetStatementCleaners()
        {
            var statementCleaners = new List<IStatementCleaner>();

            // Get all types in the RosRscStatementCleaners namespace
            var typesInNamespace = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.Namespace == typeof(IStatementCleaner).Namespace)
                .Where(t => typeof(IStatementCleaner).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            // Instantiate each type and add it to the list
            foreach (var type in typesInNamespace)
            {
                if (Activator.CreateInstance(type) is IStatementCleaner cleaner)
                {
                    statementCleaners.Add(cleaner);
                }
            }

            return statementCleaners;
        }

    }
}
