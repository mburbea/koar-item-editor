using System;

namespace KoAR.Core
{
    public class EffectInfo
    {
        public string Code { get; set; }

        public string DisplayText { get; set; }

        public EffectInfo Clone() => (EffectInfo)MemberwiseClone();
    }
}
