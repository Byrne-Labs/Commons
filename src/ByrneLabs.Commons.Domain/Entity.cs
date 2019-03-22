using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
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
        [SuppressMessage("ReSharper", "VirtualMemberNeverOverridden.Global", Justification = "False positive -- this should not be possible on a public API")]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            HasChanged = true;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
