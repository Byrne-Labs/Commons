using System;
using System.ComponentModel;

namespace ByrneLabs.Commons.Domain
{
    public interface IEntity : INotifyPropertyChanged, ICloneable
    {
        Guid? EntityId { get; set; }
    }
}
