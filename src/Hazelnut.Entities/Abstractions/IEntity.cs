using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Hazelnut.Entities
{
    /// <summary>
    /// Represents an M# Entity.
    /// </summary>
    public interface IEntity
    {

        /// <summary>
        /// Gets the id of this entity.
        /// </summary>        
        object GetId();

    }

    /// <summary>
    /// A persistent object in the application.
    /// </summary>
    public interface IEntity<T> : IEntity
    {
        /// <summary>
        /// Gets the ID.
        /// </summary>
        T Id { get; set; }
    }

    internal interface IOriginalIdHolder { void SetOriginalId(); }
}