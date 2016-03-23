using System;

namespace Zenject
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class InjectOptionalAttribute : Attribute
    {
        public InjectOptionalAttribute(string identifier)
        {
            Identifier = identifier;
        }

        public InjectOptionalAttribute()
        {
        }

        public string Identifier
        {
            get;
            private set;
        }
    }
}

