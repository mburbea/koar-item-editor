using System;

namespace KoAR.Core
{
    public class EffectInfo : IEffectInfo, IEquatable<EffectInfo>
    {
        private string _code;
        public string Code
        {
            get => _code;
            set
            {
                Amalur.DedupedEffects.TryGetValue(value ??= "", out var definition);
                DisplayText = definition?.DisplayText ?? "Unknown";
                _code = value;
            }
        }

        public string DisplayText { get; set; }

        public EffectInfo Clone() => (EffectInfo)MemberwiseClone();

        public bool Equals(EffectInfo other) => other?.Code.Equals(Code, StringComparison.OrdinalIgnoreCase) == true;
        public override bool Equals(object obj) => Equals(obj as EffectInfo);
        public override int GetHashCode() => Code?.GetHashCode() ?? 0;
    }
}
