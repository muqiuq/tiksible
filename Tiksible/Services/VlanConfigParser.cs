using Tiksible.Models.CfgEntities;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Tiksible.Services
{
    public static class VlanConfigParser
    {
        public static VlanHostConfigCfgEntity? Parse(HostCfgEntity host)
        {
            if (host.Params == null || !host.Params.ContainsKey("vlan"))
                return null;

            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            var yaml = serializer.Serialize(host.Params["vlan"]);
            return deserializer.Deserialize<VlanHostConfigCfgEntity>(yaml);
        }
    }
}
