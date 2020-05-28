using System;

namespace KoAR.Core
{
    public class EffectInfo : IEffectInfo, IEquatable<EffectInfo>
    {
        public EffectInfo(uint code, string displayText) => (Code, DisplayText) = (code, displayText);
        
        public uint Code { get; }
        public string DisplayText { get; }
        public EffectInfo Clone() => (EffectInfo)MemberwiseClone();
        public bool Equals(EffectInfo? other) => other?.Code == Code;
        public override bool Equals(object obj) => Equals(obj as EffectInfo);
        public override int GetHashCode() => Code.GetHashCode();
    }
}
