using System;
using System.Collections.Generic;
using System.Linq;
using DonjonMortel.Core;

namespace DonjonMortel.Graphics
{
    public class Renderer
    {
        private const int WIDTH = 55;
        private const int HEIGHT = 15;

        public static void DrawMap(char[,] map, bool[,] explored, Player player,
            List<Entity> enemies, List<Item> items, List<BreakableWall> walls,
            List<Trap> traps, int exitX, int exitY, int floor)
        {
            Console.SetCursorPosition(0, 0);
            int vision = 7;

            // Dessiner le cadre supérieur avec titre
            DrawTopBorder(floor);

            for (int y = 0; y < HEIGHT; y++)
            {
                // Bordure gauche
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("║");

                for (int x = 0; x < WIDTH; x++)
                {
                    double dist = Math.Sqrt(Math.Pow(x - player.X, 2) + Math.Pow(y - player.Y, 2));

                    if (dist < vision)
                    {
                        explored[x, y] = true;
                        DrawVisibleTile(x, y, map, player, enemies, items, walls, traps, exitX, exitY, dist);
                    }
                    else if (explored[x, y])
                    {
                        DrawExploredTile(x, y, map, items, walls, traps, exitX, exitY);
                    }
                    else
                    {
                        Console.Write(" ");
                    }
                }

                // Bordure droite
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine("║");
            }

            // Dessiner le cadre inférieur
            DrawBottomBorder();
            Console.ResetColor();
        }

        private static void DrawTopBorder(int floor)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write("╔");
            Console.Write(new string('═', WIDTH));
            Console.WriteLine("╗");
            
            Console.Write("║");
            Console.ForegroundColor = ConsoleColor.Yellow;
            string title = $" DONJON MORTEL - ÉTAGE {floor}/15 ";
            int padding = (WIDTH - title.Length) / 2;
            Console.Write(new string(' ', padding));
            Console.Write(title);
            Console.Write(new string(' ', WIDTH - padding - title.Length));
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("║");
            
            Console.Write("╠");
            Console.Write(new string('═', WIDTH));
            Console.WriteLine("╣");
        }

        private static void DrawBottomBorder()
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write("╚");
            Console.Write(new string('═', WIDTH));
            Console.WriteLine("╝");
        }

        private static void DrawVisibleTile(int x, int y, char[,] map, Player player,
            List<Entity> enemies, List<Item> items, List<BreakableWall> walls,
            List<Trap> traps, int exitX, int exitY, double dist)
        {
            // Joueur
            if (x == player.X && y == player.Y)
            {
                Console.ForegroundColor = player.Color;
                Console.Write("@");
                return;
            }

            // Ennemis
            var enemy = enemies.FirstOrDefault(e => e.X == x && e.Y == y);
            if (enemy != null)
            {
                Console.ForegroundColor = enemy.Color;
                Console.Write(enemy.Symbol);
                return;
            }

            // Murs cassables
            var wall = walls.FirstOrDefault(w => w.X == x && w.Y == y);
            if (wall != null)
            {
                Console.ForegroundColor = wall.Color;
                Console.Write(wall.Symbol);
                return;
            }

            // Objets
            var item = items.FirstOrDefault(i => i.X == x && i.Y == y);
            if (item != null)
            {
                Console.ForegroundColor = item.Color;
                Console.Write(item.Symbol);
                return;
            }

            // Pièges
            var trap = traps.FirstOrDefault(t => t.X == x && t.Y == y);
            if (trap != null && (trap.IsRevealed || dist <= 1.5))
            {
                trap.IsRevealed = true;
                Console.ForegroundColor = trap.Color;
                Console.Write(trap.Symbol);
                return;
            }

            // Sortie
            if (x == exitX && y == exitY)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("▼");
                return;
            }

            // Terrain
            if (map[x, y] == '█')
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("█");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("·");
            }
        }

        private static void DrawExploredTile(int x, int y, char[,] map,
            List<Item> items, List<BreakableWall> walls, List<Trap> traps,
            int exitX, int exitY)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;

            if (x == exitX && y == exitY)
                Console.Write("▼");
            else if (items.Any(i => i.X == x && i.Y == y))
                Console.Write(items.First(i => i.X == x && i.Y == y).Symbol);
            else if (walls.Any(w => w.X == x && w.Y == y))
                Console.Write("▓");
            else if (traps.Any(t => t.X == x && t.Y == y && t.IsRevealed))
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.Write("^");
            }
            else if (map[x, y] == '█')
            {
                Console.ForegroundColor = ConsoleColor.Black;
                Console.Write("█");
            }
            else
                Console.Write("·");
        }

        public static void DrawUI(Player player, List<string> logs, int floor)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write("╔");
            Console.Write(new string('═', WIDTH));
            Console.WriteLine("╗");

            // Barre de vie avec graphique
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write("║ ");
            
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("❤ ");
            
            DrawHealthBar(player.HP, player.MaxHP);
            
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($" {player.HP}/{player.MaxHP}");
            
            int spacesNeeded = WIDTH - 4 - 30 - $" {player.HP}/{player.MaxHP}".Length;
            Console.Write(new string(' ', spacesNeeded));
            
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(" ║");

            // Stats du joueur
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write("║ ");
            
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"⚔ ATK:{player.Atk,3} ");
            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"⚗ Potions:{player.Potions,2} ");
            
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"★ Niv:{player.Level,2} ");
            
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write($"✦ XP:{player.Xp}/{player.XpNextLvl}");
            
            int remainingSpace = WIDTH - 2 - $"⚔ ATK:{player.Atk,3} ⚗ Potions:{player.Potions,2} ★ Niv:{player.Level,2} ✦ XP:{player.Xp}/{player.XpNextLvl}".Length;
            Console.Write(new string(' ', remainingSpace));
            
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(" ║");

            // Séparateur
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write("╠");
            Console.Write(new string('═', WIDTH));
            Console.WriteLine("╣");

            // Logs
            var lastLogs = logs.Skip(Math.Max(0, logs.Count - 3)).ToList();
            for (int i = 0; i < 3; i++)
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("║ ");
                
                if (i < lastLogs.Count)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    string log = lastLogs[i];
                    if (log.Length > WIDTH - 3)
                        log = log.Substring(0, WIDTH - 6) + "...";
                    Console.Write(log.PadRight(WIDTH - 2));
                }
                else
                {
                    Console.Write(new string(' ', WIDTH - 2));
                }
                
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine(" ║");
            }

            // Bordure inférieure
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write("╚");
            Console.Write(new string('═', WIDTH));
            Console.WriteLine("╝");

            // Contrôles
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(" [↑↓←→] Déplacer | [H] Potion | [ESPACE] Capacité | [B] Bestiaire");
            Console.ResetColor();
        }

        private static void DrawHealthBar(int currentHP, int maxHP)
        {
            int barLength = 30;
            double percentage = (double)currentHP / maxHP;
            int filledLength = (int)(percentage * barLength);

            for (int i = 0; i < barLength; i++)
            {
                if (i < filledLength)
                {
                    if (percentage > 0.6)
                        Console.ForegroundColor = ConsoleColor.Green;
                    else if (percentage > 0.3)
                        Console.ForegroundColor = ConsoleColor.Yellow;
                    else
                        Console.ForegroundColor = ConsoleColor.Red;
                    
                    Console.Write("█");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write("░");
                }
            }
        }

        public static void DrawBestiary()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("╔═══════════════════════════════════════════════════════╗");
            Console.Write("║");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("           BESTIAIRE DU DONJON MORTEL              ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("║");
            Console.WriteLine("╠═══════════════════════════════════════════════════════╣");
            
            Console.Write("║ ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("[g] GOBELIN");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("   : PV 15+ │ ATK 4+  │ Rapide         ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(" ║");
            
            Console.Write("║ ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("[O] ORQUE");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("     : PV 35+ │ ATK 7+  │ Résistant      ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(" ║");
            
            Console.Write("║ ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("[s] SPECTRE");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("   : PV 25+ │ ATK 10+ │ Dangereux      ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(" ║");
            
            Console.Write("║ ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write("[K] CHEVALIER");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" : PV 50+ │ ATK 10+ │ Étage 6+       ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(" ║");
            
            Console.Write("║ ");
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Write("[D] DRAGON");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("    : PV 80+ │ ATK 15+ │ Étage 11+      ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(" ║");
            
            Console.Write("║ ");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write("[B] BOSS");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("      : Stats Colossales │ Tous les 5   ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(" ║");
            
            Console.WriteLine("╠═══════════════════════════════════════════════════════╣");
            Console.Write("║ ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Appuyez sur une touche pour retourner au jeu...");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("   ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(" ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════╝");
            Console.ResetColor();
        }
    }
}
