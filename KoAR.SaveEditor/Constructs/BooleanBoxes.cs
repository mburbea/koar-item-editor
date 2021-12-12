namespace KoAR.SaveEditor.Constructs;

public static class BooleanBoxes
{
    public static readonly object False = false;

    public static readonly object True = true;

    public static object GetBox(bool value) => value ? BooleanBoxes.True : BooleanBoxes.False;
}
