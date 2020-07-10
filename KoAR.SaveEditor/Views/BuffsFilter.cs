using KoAR.Core;

namespace KoAR.SaveEditor.Views
{
    public enum BuffsFilter
    {
        Player = 0,
        Item,
        Prefix,
        Suffix
    }

    public static class BuffsFilterMethods
    {
        public static bool Matches(this BuffsFilter filter, Buff buff)
        {
            return filter switch
            {
                BuffsFilter.Prefix => (buff.BuffType & BuffTypes.Prefix) == BuffTypes.Prefix,
                BuffsFilter.Suffix => (buff.BuffType & BuffTypes.Suffix) == BuffTypes.Suffix,
                BuffsFilter.Item => buff.ApplyType == ApplyType.OnObject,
                _ => true
            };
        }
    }
}
