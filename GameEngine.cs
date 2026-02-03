using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using DonjonMortel.Core;
using DonjonMortel.Graphics;

namespace DonjonMortel.Game
{
    public class GameEngine
    {
        private const int WIDTH = 55;
        private const int HEIGHT = 15;
        private const string SAVE_FILE = "save.txt";
        private const string HIGHSCORE_FILE = "highscore.txt";

        private char[,] map;
        private bool[,] explored;
        private Random rand;
        private Player player;
        private List<Entity> enemies;
        private List<Item> items;
        private List<BreakableWall> breakableWalls;
        private List<Trap> traps;
        private List<string> logs;
        private int floor;
        private int exitX, exitY;
        private bool gameRunning;

        public GameEngine()
        {
            rand = new Random();
            map = new char[WIDTH, HEIGHT];
            explored = new bool[WIDTH, HEIGHT];
            enemies = new List<Entity>();
            items = new List<Item>();
            breakableWalls = new List<BreakableWall>();
            traps = new List<Trap>();
            logs = new List<string>();
            gameRunning = false;
        }

        public void Run()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.CursorVisible = false;

            while (true)
            {
                if (!gameRunning)
                {
                    ShowMenu();
                }
                else
                {
                    Draw();
                    if (HandleInput())
                    {
                        if (player.HP > 0)
                            UpdateEnemies();
                    }
                    if (player.HP <= 0)
                        GameOver();
                }
            }
        }

        private void ShowMenu()
        {
            bool hasSave = File.Exists(SAVE_FILE);
            string saveInfo = "";
            
            if (hasSave)
            {
                var lines = File.ReadAllLines(SAVE_FILE);
                saveInfo = $"(Étage {lines[1]})";
            }

            MenuRenderer.ShowMainMenu(hasSave, saveInfo);
            
            var key = Console.ReadKey(true).Key;
            
            if (key == ConsoleKey.R && hasSave)
            {
                LoadGame();
            }
            else if (key == ConsoleKey.D1 || key == ConsoleKey.NumPad1)
            {
                player = new Player(0, 0, "Guerrier", 110, 8, ConsoleColor.Cyan);
                floor = 1;
                gameRunning = true;
                MenuRenderer.ShowLoadingScreen(floor);
                GenerateLevel();
            }
            else if (key == ConsoleKey.D2 || key == ConsoleKey.NumPad2)
            {
                player = new Player(0, 0, "Archer", 75, 10, ConsoleColor.Green);
                floor = 1;
                gameRunning = true;
                MenuRenderer.ShowLoadingScreen(floor);
                GenerateLevel();
            }
            else if (key == ConsoleKey.D3 || key == ConsoleKey.NumPad3)
            {
                player = new Player(0, 0, "Mage", 50, 15, ConsoleColor.Blue);
                floor = 1;
                gameRunning = true;
                MenuRenderer.ShowLoadingScreen(floor);
                GenerateLevel();
            }
            
            Console.Clear();
        }

        private void GenerateLevel()
        {
            SaveGame();
            
            // Initialiser la carte
            for (int y = 0; y < HEIGHT; y++)
            {
                for (int x = 0; x < WIDTH; x++)
                {
                    map[x, y] = (x == 0 || x == WIDTH - 1 || y == 0 || y == HEIGHT - 1 || rand.Next(0, 100) < 10) ? '█' : '░';
                    explored[x, y] = false;
                }
            }

            enemies.Clear();
            items.Clear();
            breakableWalls.Clear();
            traps.Clear();
            logs.Clear();

            // Salle secrète avec trésor
            if (rand.Next(0, 100) < 40)
            {
                int sX = rand.Next(5, WIDTH - 10);
                int sY = rand.Next(3, HEIGHT - 7);
                
                for (int i = 0; i < 4; i++)
                    for (int j = 0; j < 4; j++)
                        map[sX + i, sY + j] = '░';
                
                for (int i = -1; i < 5; i++)
                {
                    map[sX + i, sY - 1] = '█';
                    map[sX + i, sY + 4] = '█';
                    map[sX - 1, sY + i] = '█';
                    map[sX + 4, sY + i] = '█';
                }
                
                breakableWalls.Add(new BreakableWall(sX + 2, sY - 1));
                
                int treasureType = rand.Next(3);
                if (treasureType == 0)
                    items.Add(new Item(sX + 2, sY + 2, "EpeeLegendaire", 15, '†', ConsoleColor.Yellow));
                else if (treasureType == 1)
                    items.Add(new Item(sX + 2, sY + 2, "ArmureLegendaire", 50, '♦', ConsoleColor.Cyan));
                else
                    items.Add(new Item(sX + 2, sY + 2, "AnneauLegendaire", 8, '°', ConsoleColor.Magenta));
            }

            // Placer le joueur
            PlaceAtRandom(player);

            // Générer les ennemis
            if (floor % 5 == 0)
            {
                Entity boss = new Entity(0, 0, 100 + floor * 10, 15 + floor, 'B', ConsoleColor.DarkRed, "BOSS");
                PlaceAtRandom(boss);
                enemies.Add(boss);
            }
            else
            {
                int enemyCount = 3 + floor / 2;
                for (int i = 0; i < enemyCount; i++)
                {
                    Entity e;
                    int roll = rand.Next(100);
                    
                    if (floor > 10 && roll < 40)
                        e = new Entity(0, 0, 80 + floor, 15 + floor, 'D', ConsoleColor.DarkRed, "Dragon");
                    else if (floor > 5 && roll < 50)
                        e = new Entity(0, 0, 50 + floor, 10 + floor, 'K', ConsoleColor.DarkYellow, "Chevalier");
                    else
                    {
                        int type = rand.Next(3);
                        e = type == 0 ? new Entity(0, 0, 15 + floor, 4 + floor, 'g', ConsoleColor.Green, "Gobelin") :
                            type == 1 ? new Entity(0, 0, 35 + floor, 7 + floor, 'O', ConsoleColor.Red, "Orque") :
                                       new Entity(0, 0, 25 + floor, 10 + floor, 's', ConsoleColor.White, "Spectre");
                    }
                    
                    PlaceAtRandom(e);
                    enemies.Add(e);
                }
            }

            // Pièges
            for (int i = 0; i < 5; i++)
            {
                Trap t = new Trap(0, 0);
                PlaceAtRandom(t);
                traps.Add(t);
            }

            // Potion
            items.Add(new Item(0, 0, "Heal", 45, '⚗', ConsoleColor.Green));
            PlaceAtRandom(items.Last());

            // Sortie
            do
            {
                exitX = rand.Next(1, WIDTH - 1);
                exitY = rand.Next(1, HEIGHT - 1);
            }
            while (map[exitX, exitY] == '█' || breakableWalls.Any(w => w.X == exitX && w.Y == exitY));

            AddLog($"╔═══ ÉTAGE {floor} / 15 ═══╗");
            if (floor % 5 == 0)
            {
                AddLog("⚠ ATTENTION : BOSS À PROXIMITÉ ⚠");
            }
        }

        private void PlaceAtRandom(GameObject g)
        {
            do
            {
                g.X = rand.Next(1, WIDTH - 1);
                g.Y = rand.Next(1, HEIGHT - 1);
            }
            while (map[g.X, g.Y] == '█' || 
                   breakableWalls.Any(w => w.X == g.X && w.Y == g.Y) || 
                   (g.X == exitX && g.Y == exitY));
        }

        private void Draw()
        {
            Renderer.DrawMap(map, explored, player, enemies, items, breakableWalls, traps, exitX, exitY, floor);
            Renderer.DrawUI(player, logs, floor);
        }

        private bool HandleInput()
        {
            if (!Console.KeyAvailable)
                return false;

            var key = Console.ReadKey(true).Key;

            if (key == ConsoleKey.B)
            {
                Renderer.DrawBestiary();
                Console.ReadKey(true);
                Console.Clear();
                return false;
            }

            if (key == ConsoleKey.Spacebar)
            {
                int cost = player.ClassType == "Guerrier" ? 10 : (player.ClassType == "Archer" ? 15 : 20);
                
                if (player.HP <= cost)
                {
                    AddLog("⚠ Énergie insuffisante !");
                    return false;
                }

                player.HP -= cost;

                if (player.ClassType == "Guerrier")
                {
                    foreach (var t in traps)
                        if (Math.Sqrt(Math.Pow(t.X - player.X, 2) + Math.Pow(t.Y - player.Y, 2)) <= 6)
                            t.IsRevealed = true;
                    AddLog($"⚔ RADAR ACTIVÉ (-{cost} HP)");
                }
                else if (player.ClassType == "Archer")
                {
                    for (int y = 0; y < HEIGHT; y++)
                        for (int x = 0; x < WIDTH; x++)
                            if (Math.Sqrt(Math.Pow(x - player.X, 2) + Math.Pow(y - player.Y, 2)) <= 15)
                                explored[x, y] = true;
                    AddLog($"➶ VISION ÉTENDUE (-{cost} HP)");
                }
                else if (player.ClassType == "Mage")
                {
                    PlaceAtRandom(player);
                    AddLog($"✦ TÉLÉPORTATION (-{cost} HP)");
                }

                return true;
            }

            if (key == ConsoleKey.H && player.Potions > 0)
            {
                player.HP = Math.Min(player.HP + 45, player.MaxHP);
                player.Potions--;
                AddLog("❤ SOIN : +45 HP !");
                return true;
            }

            int nX = player.X, nY = player.Y;
            
            if (key == ConsoleKey.UpArrow) nY--;
            else if (key == ConsoleKey.DownArrow) nY++;
            else if (key == ConsoleKey.LeftArrow) nX--;
            else if (key == ConsoleKey.RightArrow) nX++;
            else return false;

            Entity target = enemies.FirstOrDefault(e => e.X == nX && e.Y == nY);
            BreakableWall wall = breakableWalls.FirstOrDefault(w => w.X == nX && w.Y == nY);

            if (target != null)
            {
                target.HP -= player.Atk;
                AddLog($"⚔ Combat : {target.Name} ({target.HP} PV)");
                
                if (target.HP <= 0)
                {
                    player.GainXp(30);
                    enemies.Remove(target);
                    AddLog($"★ {target.Name} vaincu ! +30 XP");
                    
                    if (floor == 15 && target.Symbol == 'B')
                        Win();
                }
                return true;
            }

            if (wall != null)
            {
                wall.HP--;
                AddLog($"⛏ Le mur craque... ({wall.HP} PV)");
                
                if (wall.HP <= 0)
                {
                    breakableWalls.Remove(wall);
                    map[wall.X, wall.Y] = '░';
                    AddLog("✓ Mur détruit !");
                }
                return true;
            }

            if (map[nX, nY] != '█')
            {
                player.X = nX;
                player.Y = nY;

                Trap trap = traps.FirstOrDefault(tr => tr.X == player.X && tr.Y == player.Y);
                if (trap != null)
                {
                    player.HP -= 15;
                    AddLog("⚠ PIÈGE ! (-15 PV)");
                    traps.Remove(trap);
                }

                if (player.X == exitX && player.Y == exitY)
                {
                    if (enemies.Count == 0)
                    {
                        floor++;
                        MenuRenderer.ShowLoadingScreen(floor);
                        GenerateLevel();
                        Console.Clear();
                        return false;
                    }
                    else
                    {
                        AddLog("⚠ Le Boss bloque la sortie !");
                    }
                }

                var it = items.FirstOrDefault(i => i.X == player.X && i.Y == player.Y);
                if (it != null)
                {
                    if (it.Type == "Heal")
                    {
                        player.Potions++;
                        AddLog("⚗ POTION trouvée !");
                    }
                    else if (it.Type == "EpeeLegendaire")
                    {
                        player.Atk += it.Value;
                        AddLog($"† ÉPÉE LÉGENDAIRE ! +{it.Value} ATK");
                    }
                    else if (it.Type == "ArmureLegendaire")
                    {
                        player.MaxHP += it.Value;
                        player.HP += it.Value;
                        AddLog($"♦ ARMURE LÉGENDAIRE ! +{it.Value} PV MAX");
                    }
                    else if (it.Type == "AnneauLegendaire")
                    {
                        player.Atk += it.Value;
                        player.Potions += 2;
                        AddLog($"° ANNEAU LÉGENDAIRE ! +{it.Value} ATK & +2 Potions");
                    }
                    
                    items.Remove(it);
                }

                return true;
            }

            return false;
        }

        private void UpdateEnemies()
        {
            foreach (var e in enemies.ToList())
            {
                int dist = Math.Abs(player.X - e.X) + Math.Abs(player.Y - e.Y);
                
                if (dist == 1)
                {
                    player.HP -= e.Atk;
                    AddLog($"⚔ {e.Name} attaque ! (-{e.Atk} PV)");
                }
                else if (dist < 8)
                {
                    int dx = Math.Sign(player.X - e.X);
                    int dy = Math.Sign(player.Y - e.Y);
                    
                    if (map[e.X + dx, e.Y] != '█' && 
                        !enemies.Any(o => o.X == e.X + dx && o.Y == e.Y) && 
                        !breakableWalls.Any(w => w.X == e.X + dx && w.Y == e.Y))
                    {
                        e.X += dx;
                    }
                    else if (map[e.X, e.Y + dy] != '█' && 
                             !enemies.Any(o => o.X == e.X && o.Y == e.Y + dy) && 
                             !breakableWalls.Any(w => w.X == e.X && w.Y == e.Y + dy))
                    {
                        e.Y += dy;
                    }
                }
            }
        }

        private void AddLog(string message)
        {
            logs.Add(message);
            if (logs.Count > 10)
                logs.RemoveAt(0);
        }

        private void SaveGame()
        {
            string[] data = {
                player.ClassType,
                floor.ToString(),
                player.HP.ToString(),
                player.MaxHP.ToString(),
                player.Atk.ToString(),
                player.Potions.ToString(),
                player.Level.ToString(),
                player.Xp.ToString()
            };
            File.WriteAllLines(SAVE_FILE, data);
        }

        private void LoadGame()
        {
            string[] d = File.ReadAllLines(SAVE_FILE);
            string type = d[0];
            ConsoleColor col = type == "Guerrier" ? ConsoleColor.Cyan : 
                              (type == "Archer" ? ConsoleColor.Green : ConsoleColor.Blue);
            
            player = new Player(0, 0, type, int.Parse(d[3]), int.Parse(d[4]), col);
            floor = int.Parse(d[1]);
            player.HP = int.Parse(d[2]);
            player.Potions = int.Parse(d[5]);
            player.Level = int.Parse(d[6]);
            player.Xp = int.Parse(d[7]);
            
            gameRunning = true;
            MenuRenderer.ShowLoadingScreen(floor);
            GenerateLevel();
        }

        private void GameOver()
        {
            int score = floor * 100 + player.Level * 50;
            
            // Sauvegarder le meilleur score
            int highscore = 0;
            if (File.Exists(HIGHSCORE_FILE))
            {
                highscore = int.Parse(File.ReadAllText(HIGHSCORE_FILE));
            }
            
            if (score > highscore)
            {
                File.WriteAllText(HIGHSCORE_FILE, score.ToString());
            }
            
            if (File.Exists(SAVE_FILE))
                File.Delete(SAVE_FILE);
            
            gameRunning = false;
            MenuRenderer.ShowGameOver(floor, score);
            Console.ReadKey();
        }

        private void Win()
        {
            if (File.Exists(SAVE_FILE))
                File.Delete(SAVE_FILE);
            
            gameRunning = false;
            MenuRenderer.ShowVictory();
            Console.ReadKey();
        }
    }
}
