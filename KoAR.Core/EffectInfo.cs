using System;

namespace KoAR.Core
{
    public class EffectInfo : IEquatable<EffectInfo>
    {
        public string Code { get; set; }

        public string DisplayText { get; set; }

        public EffectInfo Clone() => (EffectInfo)MemberwiseClone();

        public bool Equals(EffectInfo other) => other?.Code.Equals(Code, StringComparison.OrdinalIgnoreCase) == true;
        public override bool Equals(object obj) => Equals(obj as EffectInfo);
        public override int GetHashCode() => Code?.GetHashCode() ?? 0;
    }
}
