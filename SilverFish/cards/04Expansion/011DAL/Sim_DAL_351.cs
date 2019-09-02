using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
	class Sim_DAL_351 : SimTemplate //* 远古祝福
	{
        //双生法术：使你的随从获得+1/+1。

        public override void onCardPlay(Playfield p, bool ownplay, Minion target, int choice)
		{
           if (p.ownMinions.Count < 3) p.evaluatePenality +=30;
		   p.allMinionOfASideGetBuffed(ownplay, 1, 1);
		   p.drawACard(CardDB.cardIDEnum.DAL_351ts, ownplay);
		}
	}
}