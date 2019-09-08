using HREngine.Bots;
using SilverFish.Enums;

namespace SilverFish.cards._04Expansion._002AT
{
	class Sim_AT_007 : SimTemplate //* Spellslinger
	{
		//Battlecry: Add a random spell card to each player's hand.
		
		public override void getBattlecryEffect(Playfield p, Minion own, Minion target, int choice)
		{
            p.drawACard(CardName.frostbolt, true, true);
            p.drawACard(CardName.frostbolt, false, true);
		}
	}
}