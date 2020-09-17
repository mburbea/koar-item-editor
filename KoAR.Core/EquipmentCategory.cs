﻿namespace KoAR.Core
{
    using static EquipmentCategory;
    public enum EquipmentCategory
    {
        Hat = 1,
        Torso,
        Robes,
        Legs,
        Hands,
        Feet,
        Shield,
        Chakrams,
        Daggers,
        Faeblades,
        Greatsword,
        Hammer,
        Longbow,
        Longsword,
        Sceptre,
        Staff,
        Necklace,
        Ring
    }

    public static class EquipmentCategoryMethods
    {
        public static bool IsJewelry(this EquipmentCategory category) 
            => category == Necklace 
            || category == Ring;

        public static bool IsWeapon(this EquipmentCategory category) 
            => category == Chakrams
            || category == Daggers
            || category == Faeblades
            || category == Greatsword
            || category == Hammer
            || category == Longbow
            || category == Longsword
            || category == Sceptre
            || category == Staff;

        public static bool IsArmor(this EquipmentCategory category)
            => category == Hat
            || category == Torso
            || category == Robes
            || category == Legs
            || category == Hands
            || category == Feet
            || category == Shield;
    }
}
