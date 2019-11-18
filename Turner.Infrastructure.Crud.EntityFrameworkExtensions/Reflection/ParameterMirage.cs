using System;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Turner.Infrastructure.Crud.EntityFrameworkExtensions
{
    internal class ParameterMirage
    {
        public string Name { get; set; }

        public Type Type { get; set; }
    }

    internal class ParameterMirageInfo : ParameterInfo
    {
        public ParameterMirageInfo(ConstructorInfo ctor, Type pType, string pName, int pPos)
        {
            Member = ctor;
            Name = pName;
            ParameterType = pType;
            Position = pPos;
        }

        public override MemberInfo Member { get; }

        public override string Name { get; }

        public override Type ParameterType { get; }

        public override int Position { get; }
    }
}
