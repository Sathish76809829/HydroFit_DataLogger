#region Usings
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;
using System.Windows;
#endregion Usings

#region OPCEngine Namespace
namespace Elpis.Windows.OPC.Server
{
    /// <summary>
    /// Shows how to serialize WPF's DependencyProperties mostly automatically using reflection.
    /// 
    /// Note that it is necessary to provide a deserialization constructor in every derived class!
    /// This is a bitter pill, but the next step - providing a SerializationSurrogate to hide all
    /// this from the programmer of the derived class - does not work it seems.
    /// </summary>
    [Serializable]
    public class SerializableDependencyObject : DependencyObject, ISerializable
    {
        #region Constructor  
        public SerializableDependencyObject()
        {

        }
        public SerializableDependencyObject(SerializationInfo info, StreamingContext context)
        {
            Type thisType = GetType();
            Debug.Print("SerializableDependencyObject deserialization constructor invoked.");
            Debug.Print("Deserializing object of type: {0}", thisType.Name);

            // De-Serialize Fields
            List<FieldInfo> fieldInformation = ReflectionHelper.GetSerializableFieldInformation(thisType);

            foreach (FieldInfo fieldInformationRover in fieldInformation)
            {
                Debug.Print("\tDeserializing field '{0}'", fieldInformationRover.Name);
                fieldInformationRover.SetValue(this, info.GetValue(fieldInformationRover.Name, fieldInformationRover.FieldType));
            }

            // De-Serialize DependencyProperties
            PropertyDescriptorCollection descriptors = TypeDescriptor.GetProperties(
                thisType,
                new Attribute[] 
                { 
                    new PropertyFilterAttribute(PropertyFilterOptions.SetValues | 
                                                PropertyFilterOptions.UnsetValues | 
                                                PropertyFilterOptions.Valid) 
                });

            foreach (PropertyDescriptor propertyDescriptor in descriptors)
            {
                if (!DependencyPropertyHelper.IsSerializableDependencyProperty(this, propertyDescriptor))
                {
                    continue;
                }

                Debug.Print("\tDeserializing dependency property '{0}'", propertyDescriptor.Name);
                DependencyProperty dp = DependencyPropertyHelper.FindDependencyProperty(this, propertyDescriptor.Name);

                if (null != dp)
                {
                    SetValue(dp, info.GetValue(propertyDescriptor.Name, propertyDescriptor.PropertyType));
                }
                else
                {
                    // DependencyPropertyKey dpKey = DependencyPropertyHelper.FindReadOnlyDependencyProperty(this, propertyDescriptor.Name);
                    // if (null == dpKey)
                    {
                        throw new SerializationException(String.Format("Failed to deserialize property '{0}' on object of type '{1}'. Property could not be found. Version Conflict?", propertyDescriptor.Name, thisType));
                    }
                }
            }
        }
        #endregion Constructor

        #region ISerializable Members
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            PropertyDescriptorCollection descriptors = TypeDescriptor.GetProperties(GetType(),
                    new Attribute[] { new PropertyFilterAttribute(PropertyFilterOptions.SetValues | 
                                                                  PropertyFilterOptions.UnsetValues | 
                                                                  PropertyFilterOptions.Valid ) } );

            List<FieldInfo> fieldInformation = ReflectionHelper.GetSerializableFieldInformation(GetType());

            Debug.Print("SerializableDependencyObject.GetObjectData() invoked.");
            Debug.Print("Serializing object of type: {0}", this.GetType().Name);
            foreach (FieldInfo fiRover in fieldInformation)
            {
                Debug.Print("\tSerializing member '{0}'", fiRover.Name);

                info.AddValue(fiRover.Name, fiRover.GetValue(this));
            }

            foreach (PropertyDescriptor propertyDescriptor in descriptors)
            {
                if (!DependencyPropertyHelper.IsSerializableDependencyProperty(this, propertyDescriptor))
                    continue;

                Debug.Print("\tSerializing dependency property '{0}'", propertyDescriptor.Name);
                info.AddValue(propertyDescriptor.Name, propertyDescriptor.GetValue(this));
            }
        }
        #endregion
    }
}
#endregion OPCEngine Namespace
