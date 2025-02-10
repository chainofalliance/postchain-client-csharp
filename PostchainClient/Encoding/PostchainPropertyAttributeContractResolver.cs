using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Chromia.Encoding
{
    internal class PostchainPropertyContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            var customAttr = member.GetCustomAttribute<PostchainPropertyAttribute>();
            if (customAttr != null)
            {
                property.PropertyName = customAttr.Name;
                property.Writable = true;
            }

            return property;
        }
    }
}