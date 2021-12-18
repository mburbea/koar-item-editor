using System;
using System.Collections.Generic;

namespace KoAR.Core;

public sealed partial class ItemDefinition
{
    public record WireFormat(EquipmentCategory Category, uint TypeId, byte Level, string? Name, string? OriginalName, string InternalName, float MaxDurability, Rarity Rarity,
        string SocketTypes, Element Element, ArmorType ArmorType, uint Prefix, uint Suffix,
        uint[] ItemBuffs, uint[] PlayerBuffs, bool IsMerchant, bool AffixableName, bool HasVariants, string ChaosTier)
    {
        public static explicit operator ItemDefinition(WireFormat wf) =>
            new(wf.Category, wf.TypeId, wf.Level, wf.Name ?? wf.OriginalName ?? wf.InternalName, wf.InternalName, wf.MaxDurability, wf.Rarity,
                wf.SocketTypes, wf.Element, wf.ArmorType, Amalur.Buffs.GetValueOrDefault(wf.Prefix), Amalur.Buffs.GetValueOrDefault(wf.Suffix),
                Array.ConvertAll(wf.ItemBuffs, b => Amalur.GetBuff(b)), Array.ConvertAll(wf.PlayerBuffs, b => Amalur.GetBuff(b)), wf.IsMerchant, wf.AffixableName, wf.HasVariants, wf.ChaosTier);
    }
}
