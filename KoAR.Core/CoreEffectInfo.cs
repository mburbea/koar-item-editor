using System;

namespace KoAR.Core
{
    public class CoreEffectInfo : IEffectInfo, IEquatable<CoreEffectInfo>
    {
        public uint Code { get; set; }
        public DamageType DamageType { get; set; }
        public float Tier { get; set; }
        public string DisplayText => DamageType == DamageType.Unknown ? "Unknown" : $"{this.DamageType} (Tier: {this.Tier})";

        public CoreEffectInfo Clone() => (CoreEffectInfo)MemberwiseClone();
        public bool Equals(CoreEffectInfo? other) => other?.Code == Code;
        public override bool Equals(object obj) => Equals(obj as CoreEffectInfo);
        public override int GetHashCode() => Code.GetHashCode();
    }
}
