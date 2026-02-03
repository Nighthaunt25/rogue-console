using System;

namespace DonjonMortel.Core
{
    public class Player : Entity
    {
        public int Level { get; set; }
        public int Xp { get; set; }
        public int XpNextLvl { get; set; }
        public int Potions { get; set; }
        public string ClassType { get; set; }

        public Player(int x, int y, string classType, int hp, int atk, ConsoleColor color)
            : base(x, y, hp, atk, '@', color, "Héros")
        {
            ClassType = classType;
            Level = 1;
            Xp = 0;
            XpNextLvl = 15;
            Potions = 1;
        }

        public void GainXp(int amount)
        {
            Xp += amount;
            if (Xp >= XpNextLvl)
            {
                Level++;
                Xp -= XpNextLvl;
                XpNextLvl = (int)(XpNextLvl * 1.7);
                MaxHP += 15;
                HP = MaxHP;
                Atk += 2;
            }
        }

        public string GetClassSymbol()
        {
            return ClassType switch
            {
                "Guerrier" => "⚔",
                "Archer" => "➶",
                "Mage" => "✦",
                _ => "@"
            };
        }
    }
}
