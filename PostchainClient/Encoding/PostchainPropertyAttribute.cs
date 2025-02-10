using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace Chromia.Encoding
{
    /// <summary>
    /// Attribute to mark a property or field as a Postchain attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class PostchainPropertyAttribute : Attribute
    {
        /// <summary>
        /// The name of the attribute.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The order of the attribute.
        /// </summary>
        public int Order { get; }

        /// <summary>
        /// Constructor for the PostchainAttribute.
        /// </summary>
        /// <param name="name">The name of the attribute.</param>
        public PostchainPropertyAttribute(string name)
        {
            Name = name;
            Order = 0;
        }

        /// <summary>
        /// Constructor for the PostchainAttribute.
        /// </summary>
        /// <param name="name">The name of the attribute.</param>
        /// <param name="order">The order of the attribute.</param>
        public PostchainPropertyAttribute(string name, int order)
        {
            Name = name;
            Order = order;
        }

        /// <summary>
        /// Converts the PostchainAttribute to a JsonPropertyAttribute.
        /// </summary>
        /// <returns>The JsonPropertyAttribute.</returns>
        internal JsonPropertyAttribute ToJsonProperty()
        {
            return new JsonPropertyAttribute(Name);
        }
    }


    /// <summary>
    /// Attribute to mark a property or field as a Postchain attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class PostchainSerializableAttribute : Attribute
    {
        /// <summary>
        /// Constructor for the PostchainSerializableAttribute.
        /// </summary>
        public PostchainSerializableAttribute()
        {
        }
    }

    internal static class PostchainPropertyExtension
    {
        public static IEnumerable<object> GetPostchainProperties(this object obj)
        {
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var type = obj.GetType();

            // Get all members in declaration order
            var allMembers = type.GetFields(flags)
                .Cast<MemberInfo>()
                .Concat(type.GetProperties(flags))
                .Where(m => m.GetCustomAttribute<PostchainPropertyAttribute>() != null)
                .OrderBy(m => m.GetCustomAttribute<PostchainPropertyAttribute>().Order)
                .Select(m => m switch
                {
                    PropertyInfo property => property.GetValue(obj),
                    FieldInfo field => field.GetValue(obj),
                    _ => throw new ArgumentException($"Member {m.Name} is neither a field nor a property")
                });

            return allMembers;
        }
    }
}