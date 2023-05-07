#region Usings
using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
#endregion Usings

#region OPCEngine Namespace
namespace Elpis.Windows.OPC.Server
{
    /// <summary>
    /// Helper class for dependency properties serialization.
    /// </summary>
    public class DependencyPropertyHelper
    {
        public static DependencyProperty FindDependencyProperty(object _object, string _propertyName)
        {
            return FindDependencyPropertyInternal(_object, _propertyName, true) as DependencyProperty;
        }

        public static DependencyProperty FindSerializableDependencyProperty(object _object, string _propertyName)
        {
            return FindDependencyPropertyInternal(_object, _propertyName, false) as DependencyProperty;
        }

        public static DependencyPropertyKey FindReadOnlyDependencyProperty(object _object, string _propertyName)
        {
            return FindDependencyPropertyInternal(_object, _propertyName, true) as DependencyPropertyKey;
        }

        public static bool IsSerializableDependencyProperty(DependencyObject _obj, PropertyDescriptor _descriptor)
        {
            if (null != _descriptor)
            {
                // Quick exit if possible
                if (_descriptor.IsReadOnly)
                {
                    return false;
                }

                DependencyProperty dp = DependencyPropertyHelper.FindSerializableDependencyProperty(_obj, _descriptor.Name);

                if (dp != null)
                {
                    return true;
                }

                return false;               
            }
            else
            {
                throw new ArgumentNullException("_descriptor");
            }
        }
        public static object FindDependencyPropertyInternal(object _object, string _propertyName, bool _acceptNotSerialized)
        {
            if (null != _object && !String.IsNullOrEmpty(_propertyName))
            {
                Type typeAttachedTo = _object.GetType();

                string propertyPropertyName = _propertyName + "Property";
                FieldInfo fieldInfo = null;

                while (null == fieldInfo && typeAttachedTo != typeof(DependencyObject))
                {
                    fieldInfo = typeAttachedTo.GetField(propertyPropertyName);

                    if (null != fieldInfo && (fieldInfo.Attributes & FieldAttributes.NotSerialized) ==
                                  FieldAttributes.NotSerialized)
                    {
                        // bail out: We found the property, but it's marked as NonSerialized, 
                        // so we're done.
                        return null;
                    }

                    typeAttachedTo = typeAttachedTo.BaseType;
                }

                if (null != fieldInfo)
                {
                    return fieldInfo.GetValue(null);
                }
            }

            return null;
        }
        
    }
}
#endregion OPCEngine Namespace
