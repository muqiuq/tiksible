using tik4net.Objects;
using Tiksible.Models;
using Tiksible.Services;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization.NodeDeserializers;

namespace Tiksible
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var testData1 = File.ReadAllText("TestData/01_parse_example.rsc");

            var statements = RosRscParser.Parse(testData1);

            foreach (var statement in statements)
            {
                Console.WriteLine(statement.Export());
            }
        }

        static void YamlTest()
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build();

            var exampleConfig = File.ReadAllText("example.yaml");

            var configTasks = deserializer.Deserialize<List<ConfigTask>>(exampleConfig);
            //var configTask = deserializer.Deserialize(exampleConfig);

            foreach (var configTask in configTasks)
            {
                var tikEntries = TikEntityHelper.GetTikEntities(configTask);
                Console.WriteLine(tikEntries);
            }
        }
    }
}
