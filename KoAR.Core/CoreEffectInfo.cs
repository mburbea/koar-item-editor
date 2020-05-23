using System;

namespace KoAR.Core
{
    public class CoreEffectInfo : IEffectInfo, IEquatable<CoreEffectInfo>
    {
        public static readonly CoreEffectInfo Empty = new CoreEffectInfo();
        private uint _code;

        public uint Code
        {
            get => _code;
            set
            {
                Amalur.CoreEffects.TryGetValue(value, out var definition);
                definition ??= Empty;
                DamageType = definition.DamageType;
                Tier = definition.Tier;
                _code = value;
            }
        }

        public DamageType DamageType { get; set; }
        public float Tier { get; set; }
        public string DisplayText => DamageType == DamageType.Unknown ? "Unknown" : $"{this.DamageType} ({this.Tier})";

        public CoreEffectInfo Clone() => (CoreEffectInfo)MemberwiseClone();
        public bool Equals(CoreEffectInfo other) => other?.Code == Code;
        public override bool Equals(object obj) => Equals(obj as CoreEffectInfo);
        public override int GetHashCode() => Code.GetHashCode();
    }
}
