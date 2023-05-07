#region Usings
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
#endregion Usings

#region OPCEngine Namespace
namespace Elpis.Windows.OPC.Server
{
    /// <summary>
    /// Using reflection class assemble all members each type, serialize each type
    /// </summary>
    public static class ReflectionHelper
    {
        private static Hashtable serializationLists = new Hashtable();

        public static List<FieldInfo> GetSerializableFieldInformation(Type _type)
        {
            if (serializationLists.ContainsKey(_type))
            {
                return serializationLists[_type] as List <FieldInfo >;
            }

            if (_type.IsSubclassOf(typeof(DependencyObject)))
            {
                List <FieldInfo > fieldInformation = new List<FieldInfo > ();
                Type typeRover = _type;

                while (typeRover != null && typeRover.BaseType != typeof(DependencyObject))
                {
                    // Retrieve all instance fields. 
                    // This will present us with quite a lot of stuff, such as backings for 
                    // auto-properties, events, etc.
                    FieldInfo[] fields = typeRover.GetFields
                        (BindingFlags.Instance | BindingFlags.Public |
                                        BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

                    foreach (FieldInfo fiRover in fields)
                    {
                        if (fiRover.IsNotSerialized)
                            continue;

                        // Make sure we don't serialize events here, 
                        // because that will drag along half of 
                        // your forms, views, presenters or whatever is used for user interaction....
                        if (fiRover.FieldType.IsSubclassOf(typeof(MulticastDelegate)) ||
                            fiRover.FieldType.IsSubclassOf(typeof(Delegate)))
                            continue;

                        fieldInformation.Add(fiRover);
                    }

                    typeRover = typeRover.BaseType;
                }

                serializationLists.Add(_type, fieldInformation);
                return fieldInformation;
            }

            return null;
        }
    }
}
#endregion OPCEngine Namespace
