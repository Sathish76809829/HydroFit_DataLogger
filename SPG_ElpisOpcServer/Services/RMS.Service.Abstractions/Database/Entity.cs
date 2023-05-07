using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace RMS.Service.Abstractions.Database
{
    /// <summary>
    /// Used for initializing Entity
    /// </summary>
    public class Entity
    {
        public Entity(Type type)
        {
            Type = type;
        }

        public Type Type { get; }

        public Action<EntityTypeBuilder> BuildAction { get; set; }
    }
}
