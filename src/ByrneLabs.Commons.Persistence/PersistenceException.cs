using System;
using System.Collections.Generic;
using ByrneLabs.Commons.Domain;

namespace ByrneLabs.Commons.Persistence
{
    public class PersistenceException : Exception
    {
        public PersistenceException() : this(null, null, new List<Entity>())
        {
        }

        public PersistenceException(string message) : this(message, null, new List<Entity>())
        {
        }

        public PersistenceException(string message, Exception innerException) : this(message, innerException, new List<Entity>())
        {
        }

        public PersistenceException(string message, IEnumerable<Entity> affectedEntities) : this(message, null, affectedEntities)
        {
        }

        public PersistenceException(string message, Exception innerException, IEnumerable<Entity> affectedEntities) : base(message, innerException)
        {
            AffectedEntities = affectedEntities;
        }

        public IEnumerable<Entity> AffectedEntities { get; }
    }
}
