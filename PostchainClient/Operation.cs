using Chromia.Encoding;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Chromia
{
    /// <summary>
    /// Contains information about a Chromia operation.
    /// </summary>
    public class Operation
    {
        /// <summary>
        /// The name of the operation.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Ordered list of parameters.
        /// </summary>
        public readonly List<object> Parameters;

        private static readonly Random _random = new Random();
        private static readonly string _nopName = "nop";

        /// <summary>
        /// Creates a new parameterless operation.
        /// </summary>
        /// <param name="name">The name of the operation.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public Operation(string name)
            : this(name, new List<object>()) { }

        /// <summary>
        /// Creates a new operation.
        /// </summary>
        /// <param name="name">The name of the operation.</param>
        /// <param name="gtv">The parameters of the operation in gtv serializable format.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public Operation(string name, IGtvSerializable gtv)
            : this(name, JObject.FromObject(gtv).Values().Select(v => v.ToObject<object>()).ToList()) { }

        /// <summary>
        /// Creates a new operation.
        /// </summary>
        /// <param name="name">The name of the operation.</param>
        /// <param name="args">The parameters of the operation.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public Operation(string name, params object[] args)
            : this(name, args.ToList()) { }

        /// <summary>
        /// Creates a new operation.
        /// </summary>
        /// <param name="name">The name of the operation.</param>
        /// <param name="args">The parameters of the operation.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public Operation(string name, List<object> args)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            else if (args == null)
                throw new ArgumentNullException(nameof(args));
            else if (name.Length == 0)
                throw new ArgumentOutOfRangeException(nameof(name), "cannot be empty");

            Name = name;
            Parameters = args;
        }

        /// <summary>
        /// Adds a parameter to the operation and returns the object.
        /// </summary>
        /// <param name="parameter">The new parameter.</param>
        /// <returns>This object.</returns>
        public Operation AddParameter(object parameter)
        {
            Parameters.Add(parameter);
            return this;
        }

        /// <summary>
        /// Encodes the operation to an ASN1 buffer.
        /// </summary>
        /// <returns>The ASN1 bytes in a <see cref="Buffer"/>.</returns>
        /// <exception cref="ChromiaException"></exception>
        public Buffer Encode()
        {
            return Gtv.Encode(GetBody());
        }

        internal object[] GetBody()
        {
            return new object[]
            {
                Name,
                Parameters.ToArray()
            };
        }

        /// <summary>
        /// Decodes an object to an operation.
        /// </summary>
        /// <param name="obj">The object array containing the name and parameters.</param>
        /// <returns>The decoded <see cref="Operation"/>.</returns>
        /// <exception cref="ChromiaException"></exception>
        public static Operation Decode(object[] obj)
        {
            try
            {
                return new Operation(obj[0] as string, (obj[1] as object[]).ToList());
            }
            catch (Exception)
            {
                throw new ChromiaException("malformed operation encoding");
            }
        }

        /// <summary>
        /// Decodes an ASN1 buffer to an operation.
        /// </summary>
        /// <param name="buffer">The ASN1 bytes.</param>
        /// <returns>The decoded <see cref="Operation"/>.</returns>
        /// <exception cref="ChromiaException"></exception>
        public static Operation Decode(Buffer buffer)
        {
            return Decode(Gtv.Decode(buffer) as object[]);
        }

        /// <summary>
        /// A "no-operation" operation used to create unique transaction hashes.
        /// </summary>
        /// <returns>The "no-operation" operation.</returns>
        public static Operation Nop()
        {
            return new Operation(_nopName, _random.Next(int.MinValue, int.MaxValue));
        }

        private string ParametersToString()
        {
            return ParametersToString(Parameters.ToArray());
        }

        private static string ParametersToString(object[] obj)
        {

            var str = "";
            foreach (var param in obj)
            {
                if (param is object[])
                    str += $"[{ParametersToString(param as object[])}]  ";
                else if (param == null)
                    str += "<null>, ";
                else
                    str += $"{param}, ";
            }

            if (str.Length == 0)
                return str;
            else
                return str[..^2];
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if ((obj == null) || !GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                var b = (Operation)obj;
                if (Name == _nopName)
                    return b.Name == _nopName;
                else 
                    return Name == b.Name 
                        && (Parameters == null ? b.Parameters == null 
                            : ParametersToString() == b.ParametersToString());
            }
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            if (Name == _nopName)
                return Name.GetHashCode();
            else
                return $"{Name.GetHashCode()}{Parameters.GetHashCode()}".GetHashCode();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Operation \"{Name}\": {ParametersToString()}";
        }
    }
}
