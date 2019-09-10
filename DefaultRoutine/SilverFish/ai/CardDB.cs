﻿using System.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using HearthDb;
using HearthDb.Enums;
using SilverFish.Helpers;
using Triton.Common.LogUtilities;
using SilverFish.Enums;
using CardType = SilverFish.Enums.CardType;

namespace HREngine.Bots
{
    public struct targett
    {
        public int target;
        public int targetEntity;

        public targett(int targ, int ent)
        {
            target = targ;
            targetEntity = ent;
        }
    }


    public partial class CardDB
    {
        public CardIdEnum cardIdstringToEnum(string s)
        {
            CardIdEnum CardEnum;
            if (Enum.TryParse<CardIdEnum>(s, false, out CardEnum)) return CardEnum;
            else
            {
                Logger.GetLoggerInstanceForType().ErrorFormat("[Unidentified card ID :" + s + "]");
                return CardIdEnum.None;
            }
        }

        public CardName cardNameStringToEnum(string s, CardIdEnum tempCardIdEnum)
        {
            if (Enum.TryParse(s, false, out CardName nameEnum))
            {
                return nameEnum;
            }
            else
            {
                nameEnum = GetSpecialCardNameEnumFromCardIdEnum(tempCardIdEnum);
                if (nameEnum == CardName.unknown)
                {
                    Logger.GetLoggerInstanceForType()
                        .ErrorFormat("[Unidentified card name :" + s + "]");
                }
            }

            return nameEnum;
        }

        public enum ErrorType2
        {
            INVALID = -1,
            NONE = 0,
            REQ_MINION_TARGET = 1,
            REQ_FRIENDLY_TARGET = 2,
            REQ_ENEMY_TARGET = 3,
            REQ_DAMAGED_TARGET = 4,
            REQ_MAX_SECRETS = 5,
            REQ_FROZEN_TARGET = 6,
            REQ_CHARGE_TARGET = 7,
            REQ_TARGET_MAX_ATTACK = 8,
            REQ_NONSELF_TARGET = 9,
            REQ_TARGET_WITH_RACE = 10,
            REQ_TARGET_TO_PLAY = 11,
            REQ_NUM_MINION_SLOTS = 12,
            REQ_WEAPON_EQUIPPED = 13,
            REQ_ENOUGH_MANA = 14,
            REQ_YOUR_TURN = 15,
            REQ_NONSTEALTH_ENEMY_TARGET = 16,
            REQ_HERO_TARGET = 17,
            REQ_SECRET_ZONE_CAP = 18,
            REQ_MINION_CAP_IF_TARGET_AVAILABLE = 19,
            REQ_MINION_CAP = 20,
            REQ_TARGET_ATTACKED_THIS_TURN = 21,
            REQ_TARGET_IF_AVAILABLE = 22,
            REQ_MINIMUM_ENEMY_MINIONS = 23,
            REQ_TARGET_FOR_COMBO = 24,
            REQ_NOT_EXHAUSTED_ACTIVATE = 25,
            REQ_UNIQUE_SECRET_OR_QUEST = 26,
            REQ_TARGET_TAUNTER = 27,
            REQ_CAN_BE_ATTACKED = 28,
            REQ_ACTION_PWR_IS_MASTER_PWR = 29,
            REQ_TARGET_MAGNET = 30,
            REQ_ATTACK_GREATER_THAN_0 = 31,
            REQ_ATTACKER_NOT_FROZEN = 32,
            REQ_HERO_OR_MINION_TARGET = 33,
            REQ_CAN_BE_TARGETED_BY_SPELLS = 34,
            REQ_SUBCARD_IS_PLAYABLE = 35,
            REQ_TARGET_FOR_NO_COMBO = 36,
            REQ_NOT_MINION_JUST_PLAYED = 37,
            REQ_NOT_EXHAUSTED_HERO_POWER = 38,
            REQ_CAN_BE_TARGETED_BY_OPPONENTS = 39,
            REQ_ATTACKER_CAN_ATTACK = 40,
            REQ_TARGET_MIN_ATTACK = 41,
            REQ_CAN_BE_TARGETED_BY_HERO_POWERS = 42,
            REQ_ENEMY_TARGET_NOT_IMMUNE = 43,
            REQ_ENTIRE_ENTOURAGE_NOT_IN_PLAY = 44,
            REQ_MINIMUM_TOTAL_MINIONS = 45,
            REQ_MUST_TARGET_TAUNTER = 46,
            REQ_UNDAMAGED_TARGET = 47,
            REQ_CAN_BE_TARGETED_BY_BATTLECRIES = 48,
            REQ_STEADY_SHOT = 49,
            REQ_MINION_OR_ENEMY_HERO = 50,
            REQ_TARGET_IF_AVAILABLE_AND_DRAGON_IN_HAND = 51,
            REQ_LEGENDARY_TARGET = 52,
            REQ_FRIENDLY_MINION_DIED_THIS_TURN = 53,
            REQ_FRIENDLY_MINION_DIED_THIS_GAME = 54,
            REQ_ENEMY_WEAPON_EQUIPPED = 55,
            REQ_TARGET_IF_AVAILABLE_AND_MINIMUM_FRIENDLY_MINIONS = 56,
            REQ_TARGET_WITH_BATTLECRY = 57,
            REQ_TARGET_WITH_DEATHRATTLE = 58,
            REQ_TARGET_IF_AVAILABLE_AND_MINIMUM_FRIENDLY_SECRETS = 59,
            REQ_SECRET_ZONE_CAP_FOR_NON_SECRET = 60,
            REQ_TARGET_EXACT_COST = 61,
            REQ_STEALTHED_TARGET = 62,
            REQ_MINION_SLOT_OR_MANA_CRYSTAL_SLOT = 63,
            REQ_MAX_QUESTS = 64,
            REQ_TARGET_IF_AVAILABE_AND_ELEMENTAL_PLAYED_LAST_TURN = 65,
            REQ_TARGET_NOT_VAMPIRE = 66,
            REQ_TARGET_NOT_DAMAGEABLE_ONLY_BY_WEAPONS = 67,
            REQ_NOT_DISABLED_HERO_POWER = 68,
            REQ_MUST_PLAY_OTHER_CARD_FIRST = 69,
            REQ_HAND_NOT_FULL = 70,
            REQ_DRAG_TO_PLAY = 71,
        }

        List<string> namelist = new List<string>();

        public List<Card> CardList { get; } = new List<Card>();

        Dictionary<CardIdEnum, Card> cardidToCardList = new Dictionary<CardIdEnum, Card>();
        List<string> allCardIDS = new List<string>();
        public Card unknownCard;
        public bool installedWrong = false;

        /// <summary>
        /// 紫罗兰学徒(Violet Apprentice)
        /// </summary>
        public Card teacherminion;

        /// <summary>
        /// 埃辛诺斯之焰(Flame of Azzinoth)
        /// </summary>
        public Card illidanminion;

        /// <summary>
        /// 麻风侏儒(Leper Gnome)
        /// </summary>
        public Card lepergnome;

        /// <summary>
        /// 石腭穴居人壮汉(Burly Rockjaw Trogg)
        /// </summary>
        public Card burlyrockjaw;

        private static CardDB instance;

        public static CardDB Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new CardDB();
                    //instance.enumCreator();// only call this to get latest cardids
                    // have to do it 2 times (or the kids inside the simcards will not have a simcard :D
                    foreach (Card c in instance.CardList)
                    {
                        c.CardSimulation = CardHelper.GetCardSimulation(c.cardIDenum);
                    }

                    var totalCardSimCount = instance.CardList.Count;
                    var implementedCardSimCount = instance.CardList.Count(x =>x.CardSimulationImplemented);
                    var percentage = implementedCardSimCount / (double)totalCardSimCount;
                    Helpfunctions.Instance.ErrorLog(
                        $"Card simulation implemented {percentage:P}, {implementedCardSimCount}/{totalCardSimCount}");

                    instance.SetAdditionalData();
                }
                return instance;
            }
        }

        private CardDB()
        {
            InitSpecialNames();
            string path = Settings.Instance.DataFolderPath;
            string cardDbPath = Path.Combine(path, "_carddb.txt");
            var lines = File.ReadAllLines(cardDbPath);
            Helpfunctions.Instance.InfoLog("read carddb.txt " + lines.Length + " lines");

            CardList.Clear();
            cardidToCardList.Clear();

            //placeholdercard
            Card plchldr = new Card {name = CardName.unknown, cost = 1000};
            namelist.Add("unknown");
            CardList.Add(plchldr);
            unknownCard = CardList[0];

            string name = "";
            var cards = Cards.All;
            foreach (var item in cards.Keys)
            {
                var card = new Card();
                allCardIDS.Add(item);
                card.cardIDenum = ConvertHelper.cardIdstringToEnum(item);
                var dbCard = cards[item];
                card.Health = dbCard.Health;
                card.Class = (int) dbCard.Class;
                card.Attack.Value = dbCard.Attack;
                card.race = (int) dbCard.Race;
                card.rarity = (int) dbCard.Rarity;
                card.cost = dbCard.Cost;
                card.type = (CardType) dbCard.Type;
                if (card.type == CardType.Token)
                {
                    card.isToken = true;
                }
                if (card.type == CardType.ENCHANTMENT)
                {
                    continue;
                }

                var trimmedCardName = TrimHelper.TrimEnglishName(dbCard.Name);
                if (!string.IsNullOrWhiteSpace(name))
                {
                    namelist.Add(trimmedCardName);
                }
                card.name = ConvertHelper.cardNamestringToEnum(trimmedCardName);

                card.poisonous = dbCard.Entity.GetTag(GameTag.POISONOUS) == 1;
                card.Enrage = dbCard.Entity.GetTag(GameTag.ENRAGED) == 1;
                card.Aura = dbCard.Entity.GetTag(GameTag.AURA) == 1;
                card.tank = dbCard.Entity.GetTag(GameTag.TAUNT) == 1;
                card.battlecry= dbCard.Entity.GetTag(GameTag.BATTLECRY) == 1;
                card.discover = dbCard.Entity.GetTag(GameTag.DISCOVER) == 1;
                card.windfury = dbCard.Entity.GetTag(GameTag.WINDFURY) == 1;
                card.deathrattle = dbCard.Entity.GetTag(GameTag.DEATHRATTLE) == 1;
                card.Reborn = dbCard.Entity.GetTag(GameTag.REBORN) == 1;
                card.Inspire = dbCard.Entity.GetTag(GameTag.INSPIRE) == 1;
                card.Durability = dbCard.Entity.GetTag(GameTag.DURABILITY);
                card.Elite = dbCard.Entity.GetTag(GameTag.ELITE) == 1;
                card.Combo = dbCard.Entity.GetTag(GameTag.COMBO) == 1;
                card.oneTurnEffect = dbCard.Entity.GetTag(GameTag.TAG_ONE_TURN_EFFECT) == 1;
                card.overload = dbCard.Entity.GetTag(GameTag.OVERLOAD);
                card.lifesteal = dbCard.Entity.GetTag(GameTag.LIFESTEAL) == 1;
                card.untouchable = dbCard.Entity.GetTag(GameTag.UNTOUCHABLE) == 1;
                card.Stealth = dbCard.Entity.GetTag(GameTag.STEALTH)==1;
                card.Secret = dbCard.Entity.GetTag(GameTag.SECRET) == 1;
                card.Quest = dbCard.Entity.GetTag(GameTag.QUEST) == 1;
                card.Freeze = dbCard.Entity.GetTag(GameTag.FREEZE) == 1;
                card.AdjacentBuff = dbCard.Entity.GetTag(GameTag.ADJACENT_BUFF) == 1;
                card.DivineShield = dbCard.Entity.GetTag(GameTag.DIVINE_SHIELD) == 1;
                card.Charge = dbCard.Entity.GetTag(GameTag.CHARGE) == 1;
                card.Silence = dbCard.Entity.GetTag(GameTag.SILENCE) == 1;
                card.Morph = dbCard.Entity.GetTag(GameTag.MORPH) == 1;
                card.Spellpower = dbCard.Entity.GetTag(GameTag.SPELLPOWER) > 0;
                card.spellpowervalue = dbCard.Entity.GetTag(GameTag.SPELLPOWER);
                if (dbCard.Text.ToLower().Contains("choose one"))
                {
                    card.choice = true;
                }

                dbCard.Entity.GetTag(GameTag.ADDITIONAL_PLAY_REQS_1);
                dbCard.Entity.GetTag(GameTag.ADDITIONAL_PLAY_REQS_2);

                if (card.name != CardName.unknown)
                {
                    CardList.Add(card);
                    if (!cardidToCardList.ContainsKey(card.cardIDenum))
                    {
                        cardidToCardList.Add(card.cardIDenum, card);
                    }
                    else
                    {
                        Logger.GetLoggerInstanceForType()
                            .ErrorFormat("[c.cardIDenum:" + card.cardIDenum + "] already exists in cardidToCardList");
                    }
                }
            }

            teacherminion = getCardDataFromID(CardIdEnum.NEW1_026t);
            illidanminion = getCardDataFromID(CardIdEnum.EX1_614t);
            lepergnome = getCardDataFromID(CardIdEnum.EX1_029);
            burlyrockjaw = getCardDataFromID(CardIdEnum.GVG_068);

            Helpfunctions.Instance.InfoLog("CardList:" + cardidToCardList.Count);

        }

        public Card getCardData(CardName cardname)
        {

            foreach (Card ca in CardList)
            {
                if (ca.name == cardname)
                {
                    return ca;
                }
            }

            return unknownCard;
        }

        public Card getCardDataFromID(CardIdEnum id)
        {
            return cardidToCardList.ContainsKey(id) ? cardidToCardList[id] : unknownCard;
        }

        private void enumCreator()
        {
            //call this, if carddb.txt was changed, to get latest public enum cardIDEnum
            LogHelper.WriteMainLog("public enum cardIDEnum");
            LogHelper.WriteMainLog("{");
            LogHelper.WriteMainLog("None,");
            foreach (string cardid in allCardIDS)
            {
                LogHelper.WriteMainLog(cardid + ",");
            }
            LogHelper.WriteMainLog("}");



            LogHelper.WriteMainLog("public cardIDEnum cardIdstringToEnum(string s)");
            LogHelper.WriteMainLog("{");
            foreach (string cardid in allCardIDS)
            {
                LogHelper.WriteMainLog("if(s==\"" + cardid + "\") return CardIdEnum." + cardid + ";");
            }
            LogHelper.WriteMainLog("return CardIdEnum.None;");
            LogHelper.WriteMainLog("}");

            List<string> namelist = new List<string>();

            foreach (string cardid in this.namelist)
            {
                if (namelist.Contains(cardid)) continue;
                namelist.Add(cardid);
            }


            LogHelper.WriteMainLog("public enum cardName");
            LogHelper.WriteMainLog("{");
            foreach (string cardid in namelist)
            {
                LogHelper.WriteMainLog(cardid + ",");
            }
            LogHelper.WriteMainLog("}");

            LogHelper.WriteMainLog("public cardName cardNamestringToEnum(string s)");
            LogHelper.WriteMainLog("{");
            foreach (string cardid in namelist)
            {
                LogHelper.WriteMainLog("if(s==\"" + cardid + "\") return CardName." + cardid + ";");
            }
            LogHelper.WriteMainLog("return CardName.unknown;");
            LogHelper.WriteMainLog("}");

            // simcard creator:

            LogHelper.WriteMainLog("public SimTemplate getSimCard(cardIDEnum id)");
            LogHelper.WriteMainLog("{");
            foreach (string cardid in allCardIDS)
            {
                LogHelper.WriteMainLog("if(id == CardIdEnum." + cardid + ") return new Sim_" + cardid + "();");
            }
            LogHelper.WriteMainLog("return new SimTemplate();");
            LogHelper.WriteMainLog("}");

        }

        private void SetAdditionalData()
        {
            PenaltyManager penaltyManager = PenaltyManager.Instance;

            var triggerType = typeof(CardTrigger);
            var triggerNameArray = Enum.GetNames(triggerType);

            foreach (Card card in CardList)
            {
                if (penaltyManager.cardDrawBattleCryDatabase.ContainsKey(card.name))
                {
                    card.isCarddraw = penaltyManager.cardDrawBattleCryDatabase[card.name];
                }

                if (penaltyManager.DamageTargetSpecialDatabase.ContainsKey(card.name))
                {
                    card.damagesTargetWithSpecial = true;
                }

                if (penaltyManager.DamageTargetDatabase.ContainsKey(card.name))
                {
                    card.damagesTarget = true;
                }

                if (penaltyManager.priorityTargets.ContainsKey(card.name))
                {
                    card.targetPriority = penaltyManager.priorityTargets[card.name];
                }

                if (penaltyManager.specialMinions.ContainsKey(card.name))
                {
                    card.isSpecialMinion = true;
                }
                
                card.Triggers = new List<CardTrigger>();
                var cardSimulationType = card.CardSimulation.GetType();
                foreach (var triggerName in triggerNameArray)
                {
                    try
                    {
                        var methods = cardSimulationType.GetMethods()
                            .Where(x => x.Name.Equals(triggerName, StringComparison.Ordinal));
                        foreach (var methodInfo in methods)
                        {
                            if (methodInfo.DeclaringType == cardSimulationType)
                            {
                                var trigger = (CardTrigger) Enum.Parse(triggerType, triggerName);
                                card.Triggers.Add(trigger);
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        Helpfunctions.Instance.ErrorLog(ex);
                    }
                }

                if (card.Triggers.Count > 10)
                {
                    Helpfunctions.Instance.ErrorLog($"{cardSimulationType}'s triggers count is {card.Triggers.Count}");
                    card.Triggers.Clear();
                }
            }
        }
    }

}