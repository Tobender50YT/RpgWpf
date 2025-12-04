using System;
using System.Collections.Generic;
using System.Text;

namespace RpgWpf
{
    public class GameEngine
    {
        // Einfacher Zustand zum Testen
        private int turn = 0;

        public string ProcessCommand(string command)
        {
            turn++;

            // Hier kommt später dein RPG-Code hin.
            // Für den Anfang einfach eine Test-Antwort:
            return $"[Turn {turn}] Du hast eingegeben: {command}";
        }

        public string GetIntroText()
        {
            return "Willkommen in deinem WPF-RPG!\nGib einen Befehl ein und drücke Enter.\n";
        }
    }
}
