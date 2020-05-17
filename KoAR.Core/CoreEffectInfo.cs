using System;

namespace KoAR.Core
{
    public enum DamageType
    {
        Unknown,
        Bleeding,
        Fire,
        Ice,
        Lightning,
        Physical,
        Piercing,
        Poison,
        Primal,
        MasterCrafted
    }

    public class CoreEffectInfo
    {
        public string Code { get; set; }
        public DamageType DamageType { get; set; }
        public float Tier { get; set; }
        public string DisplayText { get; set; }

        public CoreEffectInfo Clone() => (CoreEffectInfo)MemberwiseClone();
        public bool Equals(EffectInfo other) => other?.Code.Equals(Code, StringComparison.OrdinalIgnoreCase) == true;
        public override bool Equals(object obj) => Equals(obj as EffectInfo);
        public override int GetHashCode() => Code?.GetHashCode() ?? 0;
    }
}
