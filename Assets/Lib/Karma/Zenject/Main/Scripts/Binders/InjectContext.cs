using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ModestTree;

namespace Zenject
{
    public class InjectContext
    {
        // The type of the object which is having its members injected
        // NOTE: This is null for root calls to Resolve<> or Instantiate<>
        public readonly Type ObjectType;

        // Parent context that triggered the creation of ObjectType
        // This can be used for very complex conditions using parent info such as identifiers, types, etc.
        // Note that ParentContext.MemberType is not necessarily the same as ObjectType,
        // since the ObjectType could be a derived type from ParentContext.MemberType
        public readonly InjectContext ParentContext;

        // The instance which is having its members injected
        // Note that this is null when injecting into the constructor
        public readonly object ObjectInstance;

        // Identifier - most of the time this is null
        // It will match 'foo' in this example:
        //      ... In an installer somewhere:
        //          Container.Bind<Foo>("foo").ToSingle();
        //      ...
        //      ... In a constructor:
        //          public Foo([Inject("foo") Foo foo)
        public readonly string Identifier;

        // ConcreteIdentifier - most of the time this is null
        // It will match 'foo' in this example:
        //      ... In an installer somewhere:
        //          Container.Bind<Foo>().ToSingle("foo");
        //          Container.Bind<ITickable>().ToSingle<Foo>("foo");
        //      ...
        // This allows you to create When() conditionals like this:
        //      ...
        //          Container.BindInstance("some text").When(c => c.ConcreteIdentifier == "foo");
        public readonly string ConcreteIdentifier;

        // The constructor parameter name, or field name, or property name
        public readonly string MemberName;

        // The type of the constructor parameter, field or property
        public readonly Type MemberType;

        // When optional, null is a valid value to be returned
        public readonly bool Optional;

        // When optional, this is used to provide the value
        public readonly object FallBackValue;

        // The container used for this injection
        public readonly DiContainer Container;

        // Convenience member for use in DiContainer
        internal readonly BindingId BindingId;

        public InjectContext(
            DiContainer container, Type memberType, string identifier, bool optional,
            Type objectType, object objectInstance, string memberName, InjectContext parentContext, string concreteIdentifier, object fallBackValue)
        {
            ObjectType = objectType;
            ObjectInstance = objectInstance;
            Identifier = identifier;
            ConcreteIdentifier = concreteIdentifier;
            MemberName = memberName;
            MemberType = memberType;
            Optional = optional;
            FallBackValue = fallBackValue;
            Container = container;
            BindingId = new BindingId(memberType, identifier);
            ParentContext = parentContext;
        }

        public InjectContext(
            DiContainer container, Type memberType, string identifier, bool optional,
            Type objectType, object objectInstance, string memberName, InjectContext parentContext)
            : this(container, memberType, identifier, optional, objectType, objectInstance, memberName, parentContext, null, null)
        {
        }

        public InjectContext(
            DiContainer container, Type memberType, string identifier, bool optional, Type objectType, object objectInstance)
            : this(container, memberType, identifier, optional, objectType, objectInstance, "", null)
        {
        }

        public InjectContext(
            DiContainer container, Type memberType, string identifier, bool optional)
            : this(container, memberType, identifier, optional, null, null)
        {
        }

        public InjectContext(
            DiContainer container, Type memberType, string identifier)
            : this(container, memberType, identifier, false)
        {
        }

        public InjectContext(
            DiContainer container, Type memberType)
            : this(container, memberType, null, false)
        {
        }

        public IEnumerable<InjectContext> ParentContexts
        {
            get
            {
                if (ParentContext == null)
                {
                    yield break;
                }

                yield return ParentContext;

                foreach (var context in ParentContext.ParentContexts)
                {
                    yield return context;
                }
            }
        }

        public IEnumerable<InjectContext> ParentContextsAndSelf
        {
            get
            {
                yield return this;

                foreach (var context in ParentContexts)
                {
                    yield return context;
                }
            }
        }

        // This will return the types of all the objects that are being injected
        // So if you have class Foo which has constructor parameter of type IBar,
        // and IBar resolves to Bar, this will be equal to (Bar, Foo)
        public IEnumerable<Type> AllObjectTypes
        {
            get
            {
                foreach (var context in ParentContextsAndSelf)
                {
                    if (context.ObjectType != null)
                    {
                        yield return context.ObjectType;
                    }
                }
            }
        }

        public string GetObjectGraphString()
        {
            var result = new StringBuilder();

            foreach (var context in ParentContextsAndSelf.Reverse())
            {
                if (context.ObjectType == null)
                {
                    continue;
                }

                result.AppendLine(context.ObjectType.Name());
            }

            return result.ToString();
        }

        public InjectContext ChangeMemberType(Type newMemberType)
        {
            return new InjectContext(
                Container, newMemberType, Identifier, Optional, ObjectType, ObjectInstance, MemberName, ParentContext, ConcreteIdentifier, FallBackValue);
        }

        public InjectContext ChangeConcreteIdentifier(string concreteIdentifier)
        {
            return new InjectContext(
                Container, MemberType, Identifier, Optional, ObjectType, ObjectInstance, MemberName, ParentContext, concreteIdentifier, FallBackValue);
        }
    }
}
