using System;
using System.Collections.Generic;
using System.Linq;
using ModestTree;

namespace Zenject
{
    [System.Diagnostics.DebuggerStepThrough]
    public class SingletonId : IEquatable<SingletonId>
    {
        public readonly Type Type;
        public readonly string Identifier;

        public SingletonId(string identifier, Type type)
        {
            Identifier = identifier;
            Type = type;
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                hash = hash * 29 + this.Type.GetHashCode();
                hash = hash * 29 + (this.Identifier == null ? 0 : this.Identifier.GetHashCode());
                return hash;
            }
        }

        public override bool Equals(object other)
        {
            if (other is SingletonId)
            {
                SingletonId otherId = (SingletonId)other;
                return otherId == this;
            }
            else
            {
                return false;
            }
        }

        public bool Equals(SingletonId that)
        {
            return this == that;
        }

        public static bool operator ==(SingletonId left, SingletonId right)
        {
            return left.Type == right.Type && object.Equals(left.Identifier, right.Identifier);
        }

        public static bool operator !=(SingletonId left, SingletonId right)
        {
            return !left.Equals(right);
        }
    }
}
