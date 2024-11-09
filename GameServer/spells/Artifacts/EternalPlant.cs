using System;
using System.Collections;
using System.Collections.Generic;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;

namespace DOL.GS.Spells
{
    /// <summary>
    /// 
    /// </summary>
    [SpellHandlerAttribute("EternalPlant")]
    public class EternalPlantSpellHandler : RemoveSpellEffectHandler
    {
        // constructor
        public EternalPlantSpellHandler(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line)
        {
            // RR4: now it's a list
            m_spellTypesToRemove = new List<string>();
            m_spellTypesToRemove.Add("MaddeningScalars");
            m_spellTypesToRemove.Add("Morph");
            m_spellTypesToRemove.Add("ScarabProc");
            m_spellTypesToRemove.Add("ShadesOfMist");
            m_spellTypesToRemove.Add("StarsProc2");
            m_spellTypesToRemove.Add("AlvarusMorph");
            m_spellTypesToRemove.Add("DreamGroupMorph");
            m_spellTypesToRemove.Add("DreamMorph");
            m_spellTypesToRemove.Add("ScarabProc");
       }
    }
}