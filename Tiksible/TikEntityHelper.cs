using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tik4net.Objects;
using Tiksible.Models;

namespace Tiksible
{
    public class TikEntityHelper
    {

        public static List<object> GetTikEntities(ConfigTask configTask)
        {
            var tikEntries = new List<object>();
            foreach (var configEntries in configTask.Configs)
            {
                foreach (var configEntry in configEntries)
                {
                    var tikSearchKey = configEntry.Key;
                    var tikEntityType = Helper.GetTikAssembly()
                        .GetTypes()
                        .FirstOrDefault(tt => tt.GetCustomAttributes(false).Any(att =>
                            att is TikEntityAttribute && ((TikEntityAttribute)att).EntityPath == tikSearchKey));
                    if (tikEntityType == null)
                    {
                        Console.WriteLine($"EntityPath not found {tikSearchKey}");
                        return null!;
                    }

                    var tikEntity = Activator.CreateInstance(tikEntityType);

                    foreach (var configParam in configEntry.Value)
                    {
                        var fieldType = tikEntityType.GetProperties().FirstOrDefault(tp => tp.GetCustomAttributes(false)
                            .Any(att => att is TikPropertyAttribute &&
                                        ((TikPropertyAttribute)att).FieldName == configParam.Key));
                        if (fieldType == null)
                        {
                            Console.WriteLine($"Field not found {configParam.Key}");
                            return null!;
                        }
                        fieldType.SetValue(tikEntity, configParam.Value.Trim());
                    }
                    tikEntries.Add(tikEntity);
                }
            }

            return tikEntries;
        }

    }
}
