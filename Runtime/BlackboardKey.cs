using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BehaviourTreeBuilder
{
    [Serializable]
    public abstract class BlackboardKey : ISerializationCallbackReceiver
    {
        [TableColumnWidth(130, false)]
        public string name;
        [HideInTables] public string typeName;
        public Type underlyingType;
        
        public BlackboardKey(Type underlyingType)
        {
            this.underlyingType = underlyingType;
            typeName = this.underlyingType.FullName;
        }

        public void OnBeforeSerialize()
        {
            typeName = underlyingType.AssemblyQualifiedName;
        }

        public void OnAfterDeserialize()
        {
            underlyingType = Type.GetType(typeName);
        }

        public abstract void CopyFrom(BlackboardKey key);
        public abstract bool Equals(BlackboardKey key);

        public static BlackboardKey CreateKey(Type type)
        {
            return Activator.CreateInstance(type) as BlackboardKey;
        }
    }

    [Serializable]
    public abstract class BlackboardKey<T> : BlackboardKey
    {
        public T value;

        public BlackboardKey() : base(typeof(T))
        {
        }

        public override string ToString()
        {
            return $"{name} : {value}";
        }

        public override void CopyFrom(BlackboardKey key)
        {
            if (key.underlyingType == underlyingType)
            {
                var other = key as BlackboardKey<T>;
                value = other.value;
            }
        }

        public override bool Equals(BlackboardKey key)
        {
            if (key.underlyingType == underlyingType)
            {
                var other = key as BlackboardKey<T>;
                return value.Equals(other.value);
            }

            return false;
        }
    }
}