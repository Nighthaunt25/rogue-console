using System;
using DonjonMortel.Game;

namespace DonjonMortel
{
    class Program
    {
        static void Main()
        {
            try
            {
                Console.OutputEncoding = System.Text.Encoding.UTF8;
                Console.CursorVisible = false;
                
                // Tenter d'agrandir la fenêtre console
                try
                {
                    Console.SetWindowSize(Math.Min(80, Console.LargestWindowWidth), 
                                        Math.Min(35, Console.LargestWindowHeight));
                }
                catch
                {
                    // Si ça échoue, continuer quand même
                }

                GameEngine game = new GameEngine();
                game.Run();
            }
            catch (Exception ex)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Une erreur est survenue :");
                Console.WriteLine(ex.Message);
                Console.ResetColor();
                Console.WriteLine("\nAppuyez sur une touche pour quitter...");
                Console.ReadKey();
            }
        }
    }
}
