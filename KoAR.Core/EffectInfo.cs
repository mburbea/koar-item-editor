using System;

namespace KoAR.Core
{
    public class EffectInfo : IEffectInfo, IEquatable<EffectInfo>
    {
        public uint Code { get; set; }
        public string DisplayText { get; set; }

        public EffectInfo Clone() => (EffectInfo)MemberwiseClone();
        public bool Equals(EffectInfo other) => other?.Code == Code;
        public override bool Equals(object obj) => Equals(obj as EffectInfo);
        public override int GetHashCode() => Code.GetHashCode();
    }
}
