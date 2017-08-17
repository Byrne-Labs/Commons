using System;

namespace ByrneLabs.Commons.Domain
{
    public interface IEntity
    {
        Guid EntityId { get; set; }
    }
}
