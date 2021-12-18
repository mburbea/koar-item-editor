using System.Linq;

namespace KoAR.SaveEditor.Constructs;

public sealed class OrConverter : BooleanCombinationConverter
{
    public OrConverter() : base(Enumerable.Any) { }
}
