using System;

namespace RMS.Service.Abstractions.PlugIns
{
    /// <summary>
    /// Host info of the plugIn
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class HostInfoAttribute : Attribute
    {
        public HostInfoAttribute(string name, string id)
        {
            Name = name;
            Id = Guid.Parse(id);
        }

        public string Name { get; }

        public Guid Id { get; }
    }
}
