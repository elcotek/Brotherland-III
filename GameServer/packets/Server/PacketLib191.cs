/*
* DAWN OF LIGHT - The first free open source DAoC server emulator
*
* This program is free software; you can redistribute it and/or
* modify it under the terms of the GNU General Public License
* as published by the Free Software Foundation; either version 2
* of the License, or (at your option) any later version.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this program; if not, write to the Free Software
* Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
*
*/
#define NOENCRYPTION
using DOL.GS.Effects;
using DOL.GS.Spells;
using log4net;
using System.Reflection;

namespace DOL.GS.PacketHandler
{
    [PacketLib(191, GameClient.eClientVersion.Version191)]
    public class PacketLib191 : PacketLib190
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        protected override void WriteGroupMemberUpdate(GSTCPPacketOut pak, bool updateIcons, GameLiving living)
        {
            pak.WriteByte((byte)(living.GroupIndex + 1)); // From 1 to 8
            bool sameRegion = living.CurrentRegion == m_gameClient.Player.CurrentRegion;
            GamePlayer player = null;

            if (sameRegion)
            {

                player = living as GamePlayer;

                if (player != null)
                    pak.WriteByte(player.CharacterClass.HealthPercentGroupWindow);
                else
                    pak.WriteByte(living.HealthPercent);

                pak.WriteByte(living.ManaPercent);
                pak.WriteByte(living.EndurancePercent); // new in 1.69

                byte playerStatus = 0;
                if (!living.IsAlive)
                    playerStatus |= 0x01;
                if (living.IsMezzed)
                    playerStatus |= 0x02;
                if (living.IsDiseased)
                    playerStatus |= 0x04;
                if (SpellHelper.FindEffectOnTarget(living, "DamageOverTime") != null)
                    playerStatus |= 0x08;
                if (living is GamePlayer)
                {
                    if ((living as GamePlayer).Client.ClientState == GameClient.eClientState.Linkdead)
                        playerStatus |= 0x10;
                }
                if (!sameRegion)
                    playerStatus |= 0x20;
                if (living.DebuffCategory[(int)eProperty.SpellRange] != 0 || living.DebuffCategory[(int)eProperty.ArcheryRange] != 0)
                    playerStatus |= 0x40;

                pak.WriteByte(playerStatus);
                // 0x00 = Normal , 0x01 = Dead , 0x02 = Mezzed , 0x04 = Diseased ,
                // 0x08 = Poisoned , 0x10 = Link Dead , 0x20 = In Another Region, 0x40 - NS

                if (updateIcons)
                {
                    pak.WriteByte((byte)(0x80 | living.GroupIndex));
                    lock (living.EffectList)
                    {
                        byte i = 0;
                        foreach (IGameEffect effect in living.EffectList)
                            if (effect is GameSpellEffect)
                                i++;
                        pak.WriteByte(i);
                        foreach (IGameEffect effect in living.EffectList)
                            if (effect is GameSpellEffect)
                            {
                                pak.WriteByte(0);
                                pak.WriteShort(effect.Icon);
                            }
                    }
                }
                WriteGroupMemberMapUpdate(pak, living);
            }
            else
            {
                pak.WriteInt(0x20);
                if (updateIcons)
                {
                    pak.WriteByte((byte)(0x80 | living.GroupIndex));
                    pak.WriteByte(0);
                }
            }
        }

        public override void SendConcentrationList()
        {
            if (m_gameClient.Player == null)
            {
                return;
            }

            using (GSTCPPacketOut pak = new GSTCPPacketOut(GetPacketCode(eServerPackets.ConcentrationList)))
            {
                lock (m_gameClient.Player.ConcentrationEffects)
                {
                    pak.WriteByte((byte)(m_gameClient.Player.ConcentrationEffects.Count));
                    pak.WriteByte(0); // unknown
                    pak.WriteByte(0); // unknown
                    pak.WriteByte(0); // unknown

                    for (int i = 0; i < m_gameClient.Player.ConcentrationEffects.Count; i++)
                    {
                        IConcentrationEffect effect = m_gameClient.Player.ConcentrationEffects[i];
                        pak.WriteByte((byte)i);
                        pak.WriteByte(0); // unknown
                        pak.WriteByte(effect.Concentration);
                        pak.WriteShort(effect.Icon);
                        if (effect.Name.Length > 14)
                        {
                            pak.WritePascalString(effect.Name.Substring(1, 11) + "...");
                        }
                        else
                        {
                            pak.WritePascalString(effect.Name);
                        }

                        if (/*effect.Concentration > 0 && */effect.OwnerName.Length > 14)
                        {
                            pak.WritePascalString(effect.OwnerName.Substring(1, 11) + "...");
                        }
                        else
                        {
                            pak.WritePascalString(effect.OwnerName);
                        }
                    }
                }
                SendTCP(pak);
                SendStatusUpdate(); // send status update for convinience, mostly the conc has changed
            }
        }

        /// <summary>
        /// Constructs a new PacketLib for Version 1.91 clients
        /// </summary>
        /// <param name="client">the gameclient this lib is associated with</param>
        public PacketLib191(GameClient client)
            : base(client)
        {
        }
    }
}
