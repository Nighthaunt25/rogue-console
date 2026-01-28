using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// --- CLASSES DE BASE ---
class GameObject {
    public int X, Y; public char Symbol; public ConsoleColor Color;
    public GameObject(int x, int y, char symbol, ConsoleColor color) { X = x; Y = y; Symbol = symbol; Color = color; }
}

class Entity : GameObject {
    public int HP, MaxHP, Atk; public string Name;
    public Entity(int x, int y, int hp, int atk, char symbol, ConsoleColor color, string name) 
        : base(x, y, symbol, color) { HP = hp; MaxHP = hp; Atk = atk; Name = name; }
}

class Item : GameObject {
    public string Type; public int Value;
    public Item(int x, int y, string type, int val, char sym, ConsoleColor col) 
        : base(x, y, sym, col) { Type = type; Value = val; }
}

class BreakableWall : GameObject {
    public int HP = 3;
    public BreakableWall(int x, int y) : base(x, y, '▓', ConsoleColor.DarkGray) { }
}

class Trap : GameObject {
    public bool IsRevealed = false;
    public Trap(int x, int y) : base(x, y, '^', ConsoleColor.Red) { }
}

class Player : Entity { 
    public int Level = 1, Xp = 0, XpNextLvl = 15, Potions = 1; public string ClassType;
    public Player(int x, int y, string classType, int hp, int atk, ConsoleColor color) : base(x, y, hp, atk, '@', color, "Héros") { ClassType = classType; }
    public void GainXp(int amount) {
        Xp += amount;
        if (Xp >= XpNextLvl) { Level++; Xp -= XpNextLvl; XpNextLvl = (int)(XpNextLvl * 1.7); MaxHP += 15; HP = MaxHP; Atk += 2; }
    }
}

class Program {
    static int width = 55, height = 15;
    static char[,] map = new char[width, height];
    static bool[,] explored = new bool[width, height];
    static Random rand = new Random();
    static Player player;
    static List<Entity> enemies = new List<Entity>();
    static List<Item> items = new List<Item>();
    static List<BreakableWall> breakableWalls = new List<BreakableWall>();
    static List<Trap> traps = new List<Trap>();
    static List<string> logs = new List<string>();
    static int floor = 1, exitX, exitY;
    static bool gameRunning = false;
    static string saveFile = "save.txt";

    static void Main() {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.CursorVisible = false;
        while (true) {
            if (!gameRunning) ShowMenu();
            else { 
                Draw(); 
                if (HandleInput()) { if (player.HP > 0) UpdateEnemies(); }
                if (player.HP <= 0) GameOver(); 
            }
        }
    }

    static void ShowMenu() {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("\n      === DONJON MORTEL : L'ARCHIDÉMON ===");
        if (File.Exists(saveFile)) {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n      [R] REPRENDRE (Étage " + File.ReadAllLines(saveFile)[1] + ")");
        }
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("\n      [1] GUERRIER | [2] ARCHER | [3] MAGE");
        var key = Console.ReadKey(true).Key;
        if (key == ConsoleKey.R && File.Exists(saveFile)) LoadGame();
        else {
            if (key == ConsoleKey.D1 || key == ConsoleKey.NumPad1) player = new Player(0, 0, "Guerrier", 110, 8, ConsoleColor.Cyan);
            else if (key == ConsoleKey.D2 || key == ConsoleKey.NumPad2) player = new Player(0, 0, "Archer", 75, 10, ConsoleColor.Green);
            else player = new Player(0, 0, "Mage", 50, 15, ConsoleColor.Blue);
            floor = 1;
        }
        Console.Clear(); gameRunning = true; GenerateLevel();
    }

    static void SaveGame() {
        string[] data = { player.ClassType, floor.ToString(), player.HP.ToString(), player.MaxHP.ToString(), player.Atk.ToString(), player.Potions.ToString(), player.Level.ToString(), player.Xp.ToString() };
        File.WriteAllLines(saveFile, data);
    }

    static void LoadGame() {
        string[] d = File.ReadAllLines(saveFile);
        string type = d[0];
        ConsoleColor col = type == "Guerrier" ? ConsoleColor.Cyan : (type == "Archer" ? ConsoleColor.Green : ConsoleColor.Blue);
        player = new Player(0, 0, type, int.Parse(d[3]), int.Parse(d[4]), col);
        floor = int.Parse(d[1]); player.HP = int.Parse(d[2]); player.Potions = int.Parse(d[5]);
        player.Level = int.Parse(d[6]); player.Xp = int.Parse(d[7]);
    }

    static void ShowBestiary() {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("\n  === BESTIAIRE DU DONJON ===");
        Console.ResetColor();
        Console.WriteLine("\n  [g] GOBELIN   : PV 15+ | ATK 4+  (Rapide)");
        Console.WriteLine("  [O] ORQUE     : PV 35+ | ATK 7+  (Résistant)");
        Console.WriteLine("  [s] SPECTRE   : PV 25+ | ATK 10+ (Dangereux)");
        Console.WriteLine("  [K] CHEVALIER : PV 50+ | ATK 10+ (Apparaît Niv 6)");
        Console.WriteLine("  [D] DRAGON    : PV 80+ | ATK 15+ (Apparaît Niv 11)");
        Console.WriteLine("  [B] BOSS      : Stats Colossales (Tous les 5 niv)");
        Console.WriteLine("\n  Appuyez sur n'importe quelle touche pour revenir...");
        Console.ReadKey(true);
        Console.Clear();
    }

    static void GenerateLevel() {
        SaveGame();
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++) {
                map[x, y] = (x == 0 || x == width - 1 || y == 0 || y == height - 1 || rand.Next(0, 100) < 10) ? '█' : '░';
                explored[x, y] = false;
            }
        enemies.Clear(); items.Clear(); breakableWalls.Clear(); traps.Clear(); logs.Clear();
        
        if (rand.Next(0, 100) < 40) {
            int sX = rand.Next(5, width - 10), sY = rand.Next(3, height - 7);
            for (int i = 0; i < 4; i++) for (int j = 0; j < 4; j++) map[sX + i, sY + j] = '░';
            for (int i = -1; i < 5; i++) { map[sX+i,sY-1]='█'; map[sX+i,sY+4]='█'; map[sX-1,sY+i]='█'; map[sX+4,sY+i]='█'; }
            breakableWalls.Add(new BreakableWall(sX + 2, sY - 1));
            int treasureType = rand.Next(3);
            if (treasureType == 0) items.Add(new Item(sX+2, sY+2, "EpeeLegendaire", 15, '★', ConsoleColor.Yellow));
            else if (treasureType == 1) items.Add(new Item(sX+2, sY+2, "ArmureLegendaire", 50, '★', ConsoleColor.Cyan));
            else items.Add(new Item(sX+2, sY+2, "AnneauLegendaire", 8, '★', ConsoleColor.Magenta));
        }

        PlaceAtRandom(player);

        if (floor % 5 == 0) {
            string bName = (floor == 15) ? "L'ARCHIDÉMON FINAL" : (floor == 10 ? "LE SEIGNEUR DE GUERRE" : "LE ROI SQUELETTE");
            Entity boss = new Entity(0, 0, 150 + (floor * 15), 10 + floor, 'B', ConsoleColor.Magenta, bName);
            PlaceAtRandom(boss); enemies.Add(boss);
        } else {
            int numEnemies = 3 + (floor / 3);
            for (int i = 0; i < numEnemies; i++) {
                Entity e; int roll = rand.Next(100);
                if (floor > 10 && roll < 40) e = new Entity(0, 0, 80 + floor, 15 + floor, 'D', ConsoleColor.DarkRed, "Dragon");
                else if (floor > 5 && roll < 50) e = new Entity(0, 0, 50 + floor, 10 + floor, 'K', ConsoleColor.DarkYellow, "Chevalier");
                else {
                    int type = rand.Next(3);
                    e = type == 0 ? new Entity(0, 0, 15 + floor, 4 + floor, 'g', ConsoleColor.Green, "Gobelin") :
                        type == 1 ? new Entity(0, 0, 35 + floor, 7 + floor, 'O', ConsoleColor.Red, "Orque") :
                                   new Entity(0, 0, 25 + floor, 10 + floor, 's', ConsoleColor.White, "Spectre");
                }
                PlaceAtRandom(e); enemies.Add(e);
            }
        }
        for (int i = 0; i < 5; i++) { Trap t = new Trap(0,0); PlaceAtRandom(t); traps.Add(t); }
        items.Add(new Item(0, 0, "Heal", 45, 'P', ConsoleColor.Green)); PlaceAtRandom(items.Last());
        do { exitX = rand.Next(1, width - 1); exitY = rand.Next(1, height - 1); } 
        while (map[exitX, exitY] == '█' || breakableWalls.Any(w => w.X == exitX && w.Y == exitY));
        AddLog($"--- ÉTAGE {floor} / 15 (Tapez B pour le bestiaire) ---");
    }

    static void PlaceAtRandom(GameObject g) {
        do { g.X = rand.Next(1, width - 1); g.Y = rand.Next(1, height - 1); } 
        while (map[g.X, g.Y] == '█' || breakableWalls.Any(w => w.X == g.X && w.Y == g.Y) || (g.X == exitX && g.Y == exitY));
    }

    static void Draw() {
        Console.SetCursorPosition(0, 0);
        int vision = 7; 
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                double dist = Math.Sqrt(Math.Pow(x - player.X, 2) + Math.Pow(y - player.Y, 2));
                Entity e = enemies.FirstOrDefault(en => en.X == x && en.Y == y);
                Item it = items.FirstOrDefault(i => i.X == x && i.Y == y);
                BreakableWall bw = breakableWalls.FirstOrDefault(w => w.X == x && w.Y == y);
                Trap t = traps.FirstOrDefault(tr => tr.X == x && tr.Y == y);
                if (dist < vision) {
                    explored[x, y] = true;
                    if (t != null && dist <= 1.5) t.IsRevealed = true;
                    if (x == player.X && y == player.Y) { Console.ForegroundColor = player.Color; Console.Write("@"); }
                    else if (e != null) { Console.ForegroundColor = e.Color; Console.Write(e.Symbol); }
                    else if (bw != null) { Console.ForegroundColor = bw.Color; Console.Write(bw.Symbol); }
                    else if (it != null) { Console.ForegroundColor = it.Color; Console.Write(it.Symbol); }
                    else if (t != null && t.IsRevealed) { Console.ForegroundColor = t.Color; Console.Write(t.Symbol); }
                    else if (x == exitX && y == exitY) { Console.ForegroundColor = ConsoleColor.Yellow; Console.Write("▼"); }
                    else { Console.ForegroundColor = map[x, y] == '█' ? ConsoleColor.Gray : ConsoleColor.DarkGray; Console.Write(map[x, y]); }
                } 
                else if (explored[x, y]) {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    if (x == exitX && y == exitY) Console.Write("▼");
                    else if (it != null) Console.Write(it.Symbol);
                    else if (bw != null) Console.Write("▓");
                    else if (t != null && t.IsRevealed) { Console.ForegroundColor = ConsoleColor.DarkRed; Console.Write("^"); }
                    else if (map[x, y] == '█') { Console.ForegroundColor = ConsoleColor.Black; Console.Write("█"); }
                    else Console.Write("░");
                } 
                else Console.Write(" ");
            }
            Console.WriteLine();
        }
        Console.ResetColor();
        Console.WriteLine(($" HP: {player.HP}/{player.MaxHP} | ATK: {player.Atk} | POTIONS: {player.Potions} | ÉTAGE: {floor}/15").PadRight(Console.WindowWidth - 1));
        var lastLogs = logs.Skip(Math.Max(0, logs.Count - 3)).ToList();
        for (int i = 0; i < 3; i++) Console.WriteLine((i < lastLogs.Count ? "> " + lastLogs[i] : "> ").PadRight(Console.WindowWidth - 1));
    }

    static void AddLog(string m) { logs.Add(m); if(logs.Count > 10) logs.RemoveAt(0); }

    static bool HandleInput() {
        if (!Console.KeyAvailable) return false;
        var key = Console.ReadKey(true).Key;
        if (key == ConsoleKey.B) { ShowBestiary(); return false; }
        if (key == ConsoleKey.Spacebar) {
            int cost = player.ClassType == "Guerrier" ? 10 : (player.ClassType == "Archer" ? 15 : 20);
            if (player.HP <= cost) { AddLog("Énergie insuffisante !"); return false; }
            player.HP -= cost; 
            if (player.ClassType == "Guerrier") {
                foreach (var t in traps) if (Math.Sqrt(Math.Pow(t.X - player.X, 2) + Math.Pow(t.Y - player.Y, 2)) <= 6) t.IsRevealed = true;
                AddLog("CAPACITÉ : Radar (-" + cost + " HP)");
            } 
            else if (player.ClassType == "Archer") {
                for (int y = 0; y < height; y++) for (int x = 0; x < width; x++) 
                    if (Math.Sqrt(Math.Pow(x - player.X, 2) + Math.Pow(y - player.Y, 2)) <= 15) explored[x,y] = true;
                AddLog("CAPACITÉ : Vision étendue (-" + cost + " HP)");
            } 
            else if (player.ClassType == "Mage") { PlaceAtRandom(player); AddLog("CAPACITÉ : Téléportation (-" + cost + " HP)"); }
            return true; 
        }
        if (key == ConsoleKey.H && player.Potions > 0) { player.HP = Math.Min(player.HP + 45, player.MaxHP); player.Potions--; AddLog("SOIN : +45 HP !"); return true; }

        int nX = player.X, nY = player.Y;
        if (key == ConsoleKey.UpArrow) nY--; else if (key == ConsoleKey.DownArrow) nY++; 
        else if (key == ConsoleKey.LeftArrow) nX--; else if (key == ConsoleKey.RightArrow) nX++;
        else return false;

        Entity target = enemies.FirstOrDefault(e => e.X == nX && e.Y == nY);
        BreakableWall wall = breakableWalls.FirstOrDefault(w => w.X == nX && w.Y == nY);
        if (target != null) { 
            target.HP -= player.Atk; AddLog($"Combat : {target.Name} !"); 
            if (target.HP <= 0) { player.GainXp(30); enemies.Remove(target); if (floor == 15 && target.Symbol == 'B') Win(); } 
            return true; 
        }
        if (wall != null) { wall.HP--; AddLog("Le mur craque..."); if (wall.HP <= 0) { breakableWalls.Remove(wall); map[wall.X, wall.Y] = '░'; } return true; }
        if (map[nX, nY] != '█') {
            player.X = nX; player.Y = nY;
            Trap trap = traps.FirstOrDefault(tr => tr.X == player.X && tr.Y == player.Y);
            if (trap != null) { player.HP -= 15; AddLog("PIÈGE ! (-15 PV)"); traps.Remove(trap); }
            if (player.X == exitX && player.Y == exitY) {
                if (enemies.Count == 0) { floor++; GenerateLevel(); Console.Clear(); return false; }
                else AddLog("Le Boss bloque la sortie !");
            }
            var it = items.FirstOrDefault(i => i.X == player.X && i.Y == player.Y);
            if (it != null) { 
                if (it.Type == "Heal") { player.Potions++; AddLog("POTION trouvée !"); } 
                else if (it.Type == "EpeeLegendaire") { player.Atk += it.Value; AddLog("TRÉSOR : Épée de Puissance (+" + it.Value + " ATK) !"); }
                else if (it.Type == "ArmureLegendaire") { player.MaxHP += it.Value; player.HP += it.Value; AddLog("TRÉSOR : Armure de Mithril (+" + it.Value + " PV) !"); }
                else if (it.Type == "AnneauLegendaire") { player.Atk += it.Value; player.Potions += 2; AddLog("TRÉSOR : Anneau de Célérité (+8 ATK & 2 Potions) !"); }
                items.Remove(it); 
            }
            return true;
        }
        return false;
    }

    static void UpdateEnemies() {
        foreach (var e in enemies.ToList()) {
            int dist = Math.Abs(player.X - e.X) + Math.Abs(player.Y - e.Y);
            if (dist == 1) { player.HP -= e.Atk; AddLog($"{e.Name} attaque !"); }
            else if (dist < 8) {
                int dx = Math.Sign(player.X - e.X), dy = Math.Sign(player.Y - e.Y);
                if (map[e.X + dx, e.Y] != '█' && !enemies.Any(o => o.X == e.X + dx && o.Y == e.Y) && !breakableWalls.Any(w => w.X == e.X + dx && w.Y == e.Y)) e.X += dx;
                else if (map[e.X, e.Y + dy] != '█' && !enemies.Any(o => o.X == e.X && o.Y == e.Y + dy) && !breakableWalls.Any(w => w.X == e.X && w.Y == e.Y + dy)) e.Y += dy;
            }
        }
    }

    static void Win() { if (File.Exists(saveFile)) File.Delete(saveFile); gameRunning = false; Console.Clear(); Console.ForegroundColor = ConsoleColor.Yellow; Console.WriteLine("\n   FÉLICITATIONS ! L'ARCHIDÉMON EST VAINCU !"); Console.WriteLine("   VOUS AVEZ TERMINÉ LE DONJON !"); Console.ResetColor(); Console.ReadKey(); }
    static void GameOver() { if (File.Exists(saveFile)) File.Delete(saveFile); gameRunning = false; Console.Clear(); Console.WriteLine("\n   VOUS ÊTES MORT À L'ÉTAGE " + floor); Console.ReadKey(); }
}