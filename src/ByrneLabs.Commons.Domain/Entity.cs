using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace ByrneLabs.Commons.Domain
{
    [PublicAPI]
    public abstract class Entity : HandyObject<Entity>, IEntity
    {
        protected Entity()
        {
            NeverPersisted = true;
        }

        public Guid? EntityId { get; set; }

        public bool HasChanged { get; set; }

        public bool NeverPersisted { get; set; }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            HasChanged = true;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
