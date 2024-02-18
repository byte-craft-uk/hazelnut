using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Hazelnut.Entities
{
    /// <summary>
    /// Represents an M# Entity.
    /// </summary>
    public interface IEntity : IComparable
    {
        /// <summary>
        /// Determines whether this object has just been instantiated as a new object, or represent an already persisted instance.
        /// </summary>
        bool IsNew { get; }

        /// <summary>
        /// Gets the id of this entity.
        /// </summary>        
        object GetId();

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        IEntity Clone();

    }

    /// <summary>
    /// A persistent object in the application.
    /// </summary>
    public interface IEntity<T> : IEntity
    {
        /// <summary>
        /// Gets the ID.
        /// </summary>
        T ID { get; set; }
    }

    internal interface IOriginalIdHolder { void SetOriginalId(); }
}