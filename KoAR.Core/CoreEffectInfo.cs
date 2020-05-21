using System;

namespace KoAR.Core
{
    public class CoreEffectInfo : IEquatable<CoreEffectInfo>
    {
        private static readonly CoreEffectInfo Empty = new CoreEffectInfo();
        private string _code;

        public string Code
        {
            get => _code;
            set
            {
                Amalur.CoreEffects.TryGetValue(value ??= "", out var definition);
                definition ??= Empty;
                DamageType = definition.DamageType;
                Tier = definition.Tier;
                DisplayText = definition.DisplayText;
                _code = value;
            }
        }

        public DamageType DamageType { get; set; }
        public float Tier { get; set; }
        public string DisplayText { get; set; }

        public CoreEffectInfo Clone() => (CoreEffectInfo)MemberwiseClone();
        public bool Equals(CoreEffectInfo other) => other?.Code.Equals(Code, StringComparison.OrdinalIgnoreCase) == true;
        public override bool Equals(object obj) => Equals(obj as CoreEffectInfo);
        public override int GetHashCode() => Code?.GetHashCode() ?? 0;
    }
}
