using System;

namespace KoAR.Core
{
    public class CoreEffectInfo : IEquatable<CoreEffectInfo>
    {
        public string Code { get; set; }
        public DamageType DamageType { get; set; }
        public float Tier { get; set; }
        public string DisplayText { get; set; }

        public CoreEffectInfo Clone() => (CoreEffectInfo)MemberwiseClone();
        public bool Equals(CoreEffectInfo other) => other?.Code.Equals(Code, StringComparison.OrdinalIgnoreCase) == true;
        public override bool Equals(object obj) => Equals(obj as CoreEffectInfo);
        public override int GetHashCode() => Code?.GetHashCode() ?? 0;
    }
}
