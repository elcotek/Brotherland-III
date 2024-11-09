using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DOL.Database;
using DOL.GS.Spells;
using DOL.AI;
using DOL.GS.Effects;
using DOL.AI.Brain;

namespace DOL.GS
{

    public class CaledoniaAssistant : GameNPC
    {
        public override bool AddToWorld()
        {
            if (!(base.AddToWorld()))
                return false;

            Level = 100;
            Flags = 0;

            if (Realm == eRealm.None)
                Realm = eRealm.Albion;

            /*
            switch (Realm)
            {
                case eRealm.Albion:
                    {
                        Name = "Master Wizard";
                        Model = 61;
                        LoadEquipmentTemplateFromDatabase("wizard");

                    }
                    break;
                case eRealm.Hibernia:
                    {
                        Name = "Gate Keeper";
                        Model = 342;
                        LoadEquipmentTemplateFromDatabase("mentalist");
                    }
                    break;
                case eRealm.Midgard:
                    {
                        Name = "Master Runemaster";
                        Model = 153;
                        LoadEquipmentTemplateFromDatabase("runemaster");
                    }
                    break;

            }
            */

            return true;
        }
        public void CastEffect()
        {
            foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
            {
                player.Out.SendSpellCastAnimation(this, 4468, 50);
            }
        }
    }

    public class CaledoniaTeleporter : GameNPC
    {
        //Re-Port every 45 seconds.
        public const int ReportInterval = 45;
        public const string EmainID = "caledonia_necklace";
        public const string HomeID = "home_necklace";
        public const string DarknessFallsID = "df_necklace";

        private IList<CaledoniaAssistant> m_ofAssistants;
        public IList<CaledoniaAssistant> Assistants
        {
            get { return m_ofAssistants; }
            set { m_ofAssistants = value; }
        }

        private DBSpell m_buffSpell;
        private Spell m_portSpell;

        public Spell PortSpell
        {
            get
            {
                m_buffSpell = new DBSpell();
                m_buffSpell.ClientEffect = 4468;
                m_buffSpell.CastTime = 5;
                m_buffSpell.Icon = 4468;
                m_buffSpell.Duration = ReportInterval;
                m_buffSpell.Target = "Self";
                m_buffSpell.Type = "ArmorFactorBuff";
                m_buffSpell.Name = "TELEPORTER_EFFECT";
                m_portSpell = new Spell(m_buffSpell, 0);
                return m_portSpell;
            }
            set { m_portSpell = value; }
        }
        public void StartTeleporting()
        {
            CastSpell(PortSpell, SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));

            foreach (GameNPC assistant in GetNPCsInRadius(5000))
            {
                if (assistant is CaledoniaAssistant)
                {
                    (assistant as CaledoniaAssistant).CastEffect();
                }
            }
        }
        public override void OnAfterSpellCastSequence(ISpellHandler handler)
        {
            base.OnAfterSpellCastSequence(handler);

            InventoryItem necklace = null;

            foreach (GamePlayer player in GetPlayersInRadius(300))
            {
                GameLocation PortLocation = null;
                necklace = player.Inventory.GetItem(eInventorySlot.Neck);

                switch (player.Realm)
                {
                    case eRealm.Albion:
                        {
                            if (necklace != null)
                            {
                                switch (necklace.Id_nb)
                                {
                                    case EmainID: PortLocation = new GameLocation("Caledonia Alb", 166, 38085, 53514, 4154); break;
                                    case HomeID: PortLocation = new GameLocation("Home Alb", 1, 1, 1, 1); break;
                                    case DarknessFallsID: PortLocation = new GameLocation("DF Alb", 249, 30920, 27895, 22893); break;
                                }
                            }
                        }
                        break;
                    case eRealm.Midgard:
                        {
                            if (necklace != null)
                            {
                                switch (necklace.Id_nb)
                                {
                                    case EmainID: PortLocation = new GameLocation("Caledonia Mid", 166, 54723, 24190, 4320); break;
                                    case HomeID: PortLocation = new GameLocation("Home Mid", 1, 1, 1, 1); break;
                                    case DarknessFallsID: PortLocation = new GameLocation("DF Mid", 249, 18581, 18743, 22892); break;
                                }
                            }
                        }
                        break;
                    case eRealm.Hibernia:
                        {
                            if (necklace != null)
                            {
                                switch (necklace.Id_nb)
                                {
                                    case EmainID: PortLocation = new GameLocation("Caledonia Hib", 166, 18238, 17683, 4320); break;
                                    case HomeID: PortLocation = new GameLocation("Home Hib", 1, 1, 1, 1); break;
                                    case DarknessFallsID: PortLocation = new GameLocation("DF Hib", 249, 46385, 40298, 21357); break;
                                }
                            }
                        }
                        break;
                }

                //Move the player to the designated port location.
                if (PortLocation != null)
                {
                    //Remove the Necklace.
                    player.Inventory.RemoveItem(necklace);
                    player.MoveTo(PortLocation);
                }
            }
        }
        public override bool AddToWorld()
        {
            if (!(base.AddToWorld()))
                return false;

            if (Realm == eRealm.None)
                Realm = eRealm.Albion;

            switch (Realm)
            {
                case eRealm.Albion:
                    {
                        Name = "Master Visor";
                        Model = 63;
                        LoadEquipmentTemplateFromDatabase("wizard");

                    }
                    break;
                case eRealm.Hibernia:
                    {
                        Name = "Glasny";
                        Model = 342;
                        LoadEquipmentTemplateFromDatabase("mentalist");
                    }
                    break;
                case eRealm.Midgard:
                    {
                        Name = "Stor Gothi";
                        Model = 153;
                        LoadEquipmentTemplateFromDatabase("runemaster");
                    }
                    break;
            }

            SetOwnBrain(new MainTeleporterBrain());

            return true;
        }
    }
    public class MainTeleporterBrain : StandardMobBrain
    {
        public override void Think()
        {
            CaledoniaTeleporter teleporter = Body as CaledoniaTeleporter;

            GameSpellEffect effect = null;

            foreach (GameSpellEffect activeEffect in teleporter.EffectList)
            {
                if (activeEffect.Name == "TELEPORTER_EFFECT")
                {
                    effect = activeEffect;
                }
            }

            if (effect != null || teleporter.IsCasting)
                return;

            teleporter.StartTeleporting();

            base.Think();
        }
    }
}
