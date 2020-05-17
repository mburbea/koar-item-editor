using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public bool Equals(EffectInfo other) => other?.Code.Equals(Code, StringComparison.OrdinalIgnoreCase) == true;
        public override bool Equals(object obj) => Equals(obj as EffectInfo);
        public override int GetHashCode() => Code?.GetHashCode() ?? 0;
    }
}
