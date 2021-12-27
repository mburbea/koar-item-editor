namespace KoAR.Core;

using static EquipmentCategory;

public static class EquipmentCategoryMethods
{
    public static bool IsJewelry(this EquipmentCategory category) => category is Necklace or Ring;

    public static bool IsWeapon(this EquipmentCategory category) => category is Chakrams
        or Daggers
        or Faeblades
        or Greatsword
        or Hammer
        or Longbow
        or Longsword
        or Sceptre
        or Staff;

    public static bool IsArmor(this EquipmentCategory category) => category is Hat
        or Torso
        or Robes
        or Legs
        or Hands
        or Feet
        or Shield;
}
