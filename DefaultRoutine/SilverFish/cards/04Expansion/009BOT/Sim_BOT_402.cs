using System;
using System.Collections.Generic;
using System.Text;
using SilverFish.Enums;

namespace HREngine.Bots
{
	class Sim_BOT_402 : SimTemplate //* 奥秘图纸
	{
		//发现一个奥秘

        public override void onCardPlay(Playfield p, bool ownplay, Minion target, int choice)
        {
            p.drawACard(CardName.frostbolt, ownplay, true);
        }
    }
}