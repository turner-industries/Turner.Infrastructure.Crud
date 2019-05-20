using System;
using System.Globalization;
using System.Reflection;

namespace Turner.Infrastructure.Crud.EntityFrameworkExtensions
{
    internal class PropertyMirageInfo<T> : PropertyInfo
    {
        private readonly PropertyInfo _realInfo;

        public PropertyMirageInfo(PropertyInfo realInfo)
        {
            _realInfo = realInfo;
        }

        public override PropertyAttributes Attributes => _realInfo.Attributes;

        public override bool CanRead => _realInfo.CanRead;

        public override bool CanWrite => _realInfo.CanWrite;

        public override Type PropertyType => _realInfo.PropertyType;

        public override Type DeclaringType => typeof(T);

        public override string Name => _realInfo.Name;

        public override Type ReflectedType => _realInfo.ReflectedType;

        public override MethodInfo[] GetAccessors(bool nonPublic)
        {
            return _realInfo.GetAccessors(nonPublic);
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return _realInfo.GetCustomAttributes(inherit);
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return _realInfo.GetCustomAttributes(attributeType, inherit);
        }

        public override ParameterInfo[] GetIndexParameters()
        {
            return _realInfo.GetIndexParameters();
        }

        public override MethodInfo GetGetMethod(bool nonPublic)
        {
            return _realInfo.GetGetMethod(nonPublic);
        }

        public override MethodInfo GetSetMethod(bool nonPublic)
        {
            return _realInfo.GetSetMethod(nonPublic);
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return _realInfo.IsDefined(attributeType, inherit);
        }

        public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
            => throw new NotImplementedException();

        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
