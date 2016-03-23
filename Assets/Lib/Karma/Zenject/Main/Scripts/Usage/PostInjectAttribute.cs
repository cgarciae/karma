using System;

namespace Zenject
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class PostInjectAttribute : Attribute
    {
    }
}

