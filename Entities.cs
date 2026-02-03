using System;

namespace DonjonMortel.Core
{
    // Classe de base pour tous les objets du jeu
    public class GameObject
    {
        public int X { get; set; }
        public int Y { get; set; }
        public char Symbol { get; set; }
        public ConsoleColor Color { get; set; }

        public GameObject(int x, int y, char symbol, ConsoleColor color)
        {
            X = x;
            Y = y;
            Symbol = symbol;
            Color = color;
        }
    }

    // Entité vivante (joueur, ennemis)
    public class Entity : GameObject
    {
        public int HP { get; set; }
        public int MaxHP { get; set; }
        public int Atk { get; set; }
        public string Name { get; set; }

        public Entity(int x, int y, int hp, int atk, char symbol, ConsoleColor color, string name)
            : base(x, y, symbol, color)
        {
            HP = hp;
            MaxHP = hp;
            Atk = atk;
            Name = name;
        }
    }

    // Objet ramassable
    public class Item : GameObject
    {
        public string Type { get; set; }
        public int Value { get; set; }

        public Item(int x, int y, string type, int val, char sym, ConsoleColor col)
            : base(x, y, sym, col)
        {
            Type = type;
            Value = val;
        }
    }

    // Mur destructible
    public class BreakableWall : GameObject
    {
        public int HP { get; set; }

        public BreakableWall(int x, int y) : base(x, y, '▓', ConsoleColor.DarkGray)
        {
            HP = 3;
        }
    }

    // Piège
    public class Trap : GameObject
    {
        public bool IsRevealed { get; set; }

        public Trap(int x, int y) : base(x, y, '^', ConsoleColor.Red)
        {
            IsRevealed = false;
        }
    }
}
