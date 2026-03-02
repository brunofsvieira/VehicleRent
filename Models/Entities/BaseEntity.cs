using System;

namespace VehicleRent.Models.Entities
{
    /// <summary>
    /// Provides a base class for entities that require a unique identifier and automatic tracking of creation and
    /// modification timestamps.
    /// </summary>
    /// <remarks>This abstract class is intended to be inherited by domain entities that need consistent
    /// management of identity and audit information. The identifier and timestamps are set automatically and updated as
    /// appropriate, simplifying entity lifecycle tracking across the application.</remarks>
    public abstract class BaseEntity
    {
        public long Id { get; private set; }
        public DateTime InsertDate { get; private set; }
        public DateTime UpdateDate { get; private set; }

        protected BaseEntity()
        {
            InsertDate = DateTime.UtcNow;
            UpdateDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the entity's last modified date to the current UTC time.
        /// </summary>
        /// <remarks>Call this method to mark the entity as updated. This is typically used before saving
        /// changes to ensure the update timestamp reflects the latest modification.</remarks>
        public void TouchUpdate()
        {
            UpdateDate = DateTime.UtcNow;
        }
    }
}
