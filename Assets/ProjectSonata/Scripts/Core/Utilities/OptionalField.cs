using System;

namespace ProjectSonata.Core.Utilities
{
    [Serializable]
    public struct OptionalField<T>
    {
        public bool enabled;
        public T value;

        public OptionalField(T initialValue)
        {
            enabled = true;
            value = initialValue;
        }

        public T Value => enabled ? value : default;
    }
}