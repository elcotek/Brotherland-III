

namespace DOL.GS
{
    public class HoundsPet : GamePet
    {
        public HoundsPet(INpcTemplate npcTemplate) : base(npcTemplate) { }

        public override void OnAttackedByEnemy(AttackData ad) { /* do nothing */ }



        public static int HealthRatio = ServerProperties.Properties.THEURG_PET_CONLEVEL;

        public override int MaxHealth
        {
            get
            {
                return Level * HealthRatio;
            }
        }
        /// <summary>
        /// not each summoned pet 'll fire ambiant sentences
        /// let's say 10%
        /// </summary>
        protected override void BuildAmbientTexts()
        {
            base.BuildAmbientTexts();
            if (ambientTexts.Count > 0)
                foreach (var at in ambientTexts)
                    at.Chance /= 10;
        }
    }
}