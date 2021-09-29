using System.Collections.Generic;
using System.Linq;

namespace Chromia.Postchain.Client
{
    public class Operation
    {
        public string OpName;
        public List<GTXValue> Args;
        private object[] _rawArgs;

        public Operation(string opName, object[] args): this()
        {
            this.OpName = opName;
            this._rawArgs = args;
            
            foreach (var opArg in args)
            {
                Args.Add(Gtx.ArgToGTXValue(opArg));
            }
        }

        private Operation()
        {
            this.OpName = "";
            this.Args = new List<GTXValue>();
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || ! this.GetType().Equals(obj.GetType())) 
            {
                return false;
            }
            else { 
                Operation gtxOperation = (Operation) obj;
                
                return this.OpName.Equals(gtxOperation.OpName)
                    && ((this._rawArgs == null || gtxOperation._rawArgs == null) 
                        ? this._rawArgs == gtxOperation._rawArgs 
                        : this._rawArgs.SequenceEqual(gtxOperation._rawArgs));
            }   
        }

        public override int GetHashCode()
        {
            return OpName.GetHashCode();
        }

        public GTXValue ToGtxValue()
        {
            var gtxValue = new GTXValue();
            gtxValue.Choice = GTXValueChoice.Array;
            gtxValue.Array = new List<GTXValue>(){Gtx.ArgToGTXValue(this.OpName)};
            gtxValue.Array.AddRange(this.Args);

            return gtxValue;
        }

        public object[] Raw()
        {
            return new object[]{this.OpName, this._rawArgs};
        }

        public override string ToString()
        {
            return OpName + " (" + string.Join(", ", Args) + ")";
        }
    }
}