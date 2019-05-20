using System;
using System.Globalization;
using System.Reflection;

namespace Turner.Infrastructure.Crud.EntityFrameworkExtensions
{
    internal class ConstructorMirageInfo<T> : ConstructorInfo
    {
        private readonly ConstructorInfo _realInfo;
        private readonly ParameterInfo[] _fakeParams;

        public ConstructorMirageInfo(params ParameterMirage[] fakeParams)
        {
            _realInfo = typeof(T).GetConstructor(Type.EmptyTypes);
            _fakeParams = new ParameterInfo[fakeParams.Length];

            for (var i = 0; i < fakeParams.Length; i++)
            {
                var p = fakeParams[i];
                _fakeParams[i] = new ParameterMirageInfo(this, p.Type, p.Name, i);
            }
        }

        public override MethodAttributes Attributes => _realInfo.Attributes;

        public override RuntimeMethodHandle MethodHandle => _realInfo.MethodHandle;

        public override Type DeclaringType => _realInfo.DeclaringType;

        public override string Name => _realInfo.Name;

        public override Type ReflectedType => _realInfo.ReflectedType;

        public override object[] GetCustomAttributes(bool inherit)
            => _realInfo.GetCustomAttributes(inherit);

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
            => _realInfo.GetCustomAttributes(attributeType, inherit);

        public override MethodImplAttributes GetMethodImplementationFlags()
            => _realInfo.GetMethodImplementationFlags();

        public override ParameterInfo[] GetParameters()
            => _fakeParams;

        public override bool IsDefined(Type attributeType, bool inherit)
            => _realInfo.IsDefined(attributeType, inherit);

        public override object Invoke(BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
            => throw new NotImplementedException();

        public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
