using System;
using System.Collections.Generic;
using System.Linq;

namespace RpgWpf.GameCore
{
    /// <summary>
    /// Einfaches, größenbasiertes Inventar. Jedes Item hat eine Größe; die Summe darf MaxSize nicht überschreiten.
    /// </summary>
    public class Inventar
    {
        private readonly List<IInventarItem> _items = new List<IInventarItem>();

        public int MaxSize { get; }
        public int UsedSize { get; private set; }

        public Inventar(int groesse = 24)
        {
            MaxSize = Math.Max(1, groesse);
        }

        /// <summary>
        /// Versucht ein Item einzulagern. Erfolgreich nur, wenn genügend Platz vorhanden ist.
        /// </summary>
        public bool Add(IInventarItem item)
        {
            if (item == null) return false;
            if (UsedSize + item.InventarGroesse > MaxSize) return false;
            _items.Add(item);
            UsedSize += item.InventarGroesse; // BUGFIX: Platzverbrauch korrekt erhöhen
            return true;
        }

        /// <summary>
        /// Entfernt ein Item (per Referenz). Rückgabe true, wenn vorhanden & entfernt.
        /// </summary>
        public bool Remove(IInventarItem item)
        {
            if (item == null) return false;
            int idx = _items.IndexOf(item);
            if (idx < 0) return false;
            UsedSize -= _items[idx].InventarGroesse; // BUGFIX: Platz wieder freigeben
            _items.RemoveAt(idx);
            return true;
        }

        /// <summary> Entfernt Item an Index. </summary>
        public bool RemoveAt(int index)
        {
            if (index < 0 || index >= _items.Count) return false;
            UsedSize -= _items[index].InventarGroesse;
            _items.RemoveAt(index);
            return true;
        }

        /// <summary> Nur lesender Snapshot (kopierte Liste). </summary>
        public List<IInventarItem> Snapshot() => _items.ToList();

        /// <summary> Alle Items eines Typs (z. B. Potion). </summary>
        public IEnumerable<T> OfType<T>() => _items.OfType<T>();

        /// <summary> Konsolenausgabe (UI Helfer). In echter Architektur eher in UI Schicht. </summary>
        public void Display()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"Dein Inventar ({UsedSize}/{MaxSize}) :");
            Console.ForegroundColor = ConsoleColor.White;

            for (int i = 0; i < _items.Count; i++)
            {
                var item = _items[i];
                Console.Write($"[{i}] {item.ItemName} - {item.InventarGroesse}");
                if (i < _items.Count - 1)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write(" | ");
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
            Console.WriteLine();
        }
    }
}
