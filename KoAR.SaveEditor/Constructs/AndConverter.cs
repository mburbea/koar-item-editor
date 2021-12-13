using System.Linq;

namespace KoAR.SaveEditor.Constructs;

public sealed class AndConverter : BooleanCombinationConverter
{
    public AndConverter() : base(Enumerable.All) { }
}
