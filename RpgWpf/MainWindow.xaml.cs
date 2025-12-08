using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using RpgWpf.GameCore;
using RpgWpf.GameLogic;

namespace RpgWpf
{
    /// <summary>
    /// Hauptfenster des WPF-RPGs. Kapselt die Bindings zur GameEngine und die UI-Interaktionen.
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        // ============================
        //   Engine / Spiellogik
        // ============================
        private readonly GameEngine _engine;

        // ============================
        //   Properties für Bindings
        // ============================

        // --- Character-Box (links) ---

        public string PlayerTag
        {
            get;
            set { field = value; OnPropertyChanged(nameof(PlayerTag)); }
        }

        public int Alter
        {
            get;
            set { field = value; OnPropertyChanged(nameof(Alter)); }
        }

        public double HP
        {
            get;
            set { field = value; OnPropertyChanged(nameof(HP)); }
        }

        public double MaxHP
        {
            get;
            set { field = value; OnPropertyChanged(nameof(MaxHP)); }
        }

        public double AttackDamage
        {
            get;
            set { field = value; OnPropertyChanged(nameof(AttackDamage)); }
        }

        public int DamageMultiplier
        {
            get;
            set { field = value; OnPropertyChanged(nameof(DamageMultiplier)); }
        }

        public double SpecialAttackDamage
        {
            get;
            set { field = value; OnPropertyChanged(nameof(SpecialAttackDamage)); }
        }

        /// <summary>
        /// Spezialangriffs-Wahrscheinlichkeit 0..1 (für {0:P0} in XAML).
        /// </summary>
        public double SpecialProbability
        {
            get;
            set { field = value; OnPropertyChanged(nameof(SpecialProbability)); }
        }

        /// <summary> Aktuelles Spielerlevel. </summary>
        public int Level
        {
            get;
            set { field = value; OnPropertyChanged(nameof(Level)); }
        }

        /// <summary> Aktuelle Erfahrungspunkte im aktuellen Level. </summary>
        public int CurrentExp
        {
            get;
            set { field = value; OnPropertyChanged(nameof(CurrentExp)); }
        }

        /// <summary> Benötigte Erfahrungspunkte bis zum nächsten Level. </summary>
        public int ExpToNextLevel
        {
            get;
            set { field = value; OnPropertyChanged(nameof(ExpToNextLevel)); }
        }

        // --- Enemy-Box (Mitte) ---

        public string CurrentEnemyName
        {
            get;
            set { field = value; OnPropertyChanged(nameof(CurrentEnemyName)); }
        }

        public double CurrentEnemyHP
        {
            get;
            set { field = value; OnPropertyChanged(nameof(CurrentEnemyHP)); }
        }

        public double CurrentEnemyMaxHP
        {
            get;
            set { field = value; OnPropertyChanged(nameof(CurrentEnemyMaxHP)); }
        }

        public double CurrentEnemyAttack
        {
            get;
            set { field = value; OnPropertyChanged(nameof(CurrentEnemyAttack)); }
        }

        // --- Shop-Box (rechts) ---

        public int Coins
        {
            get;
            set { field = value; OnPropertyChanged(nameof(Coins)); }
        }

        public double Defense
        {
            get;
            set { field = value; OnPropertyChanged(nameof(Defense)); }
        }

        // --- Enemies-Übersicht (mittlere Zeile) ---

        /// <summary>
        /// Liste aller sichtbaren Gegner für EnemiesList.
        /// </summary>
        public List<Entity> Enemies
        {
            get;
            private set { field = value; OnPropertyChanged(nameof(Enemies)); }
        }

        /// <summary>
        /// Aktuell im UI ausgewählter Gegner.
        /// </summary>
        public Entity SelectedEnemy
        {
            get;
            set
            {
                field = value;
                OnPropertyChanged(nameof(SelectedEnemy));
                SyncCurrentEnemyToUi();
            }
        }

        // --- Inventar-Anzeige (untere linke Box) ---

        /// <summary>
        /// Liste der Items im Inventar des Spielers.
        /// </summary>
        public List<IInventarItem> InventoryItems
        {
            get;
            set { field = value; OnPropertyChanged(nameof(InventoryItems)); }
        }

        // ============================
        //   Konstruktor
        // ============================

        public MainWindow()
        {
            InitializeComponent();

            // DataContext für Bindings setzen
            DataContext = this;

            // Engine mit Beispielwerten erzeugen
            _engine = new GameEngine(
                vorname: "Tobi",
                playerTag: "Tobender50",
                alter: 20,
                inventarGroesse: 24
            );

            // Gegnerliste aus der Engine übernehmen (inklusive Slime, Orc, Dragon usw.)
            Enemies = new List<Entity>(_engine.Enemies);

            // Standard-Gegner auswählen (erster Eintrag in der Liste, falls vorhanden)
            SelectedEnemy = Enemies.Count > 0 ? Enemies[0] : null;

            // UI initial synchronisieren
            SyncCharacterToUi();
            SyncInventoryToUi();
            SyncCurrentEnemyToUi();
            SyncMetaToUi();

            // Startmeldung in den Game-Log schreiben
            AppendLog("Willkommen in deinem WPF-RPG!");
            AppendLog(string.Empty);
            AppendLog(_engine.GetStatusText());
        }

        // ============================
        //   Hilfsmethoden (UI-Sync)
        // ============================

        /// <summary>
        /// Synchronisiert grundlegende Charakterdaten (inkl. Level/EXP) in die Bindings.
        /// </summary>
        private void SyncCharacterToUi()
        {
            var p = _engine.Player;

            PlayerTag = p.PlayerTag;
            Alter = p.Alter;
            HP = p.HP;
            MaxHP = p.MaxHP;
            AttackDamage = p.GetAttackDamage();
            DamageMultiplier = p.DamageMultiplier;
            SpecialAttackDamage = p.GetSpecialAttackDamage();
            SpecialProbability = p.SpecialAttackChancePercent / 100.0;

            Level = p.Level;
            CurrentExp = p.CurrentExp;
            ExpToNextLevel = p.ExpToNextLevel;
        }

        /// <summary>
        /// Synchronisiert Coins und Defense in die Bindings.
        /// </summary>
        private void SyncMetaToUi()
        {
            Coins = _engine.Coins;
            Defense = _engine.Defense;
        }

        /// <summary>
        /// Synchronisiert die Enemy-Box anhand des aktuell ausgewählten Gegners.
        /// </summary>
        private void SyncCurrentEnemyToUi()
        {
            var enemy = SelectedEnemy;

            if (enemy == null)
            {
                CurrentEnemyName = string.Empty;
                CurrentEnemyHP = 0;
                CurrentEnemyMaxHP = 0;
                CurrentEnemyAttack = 0;
                return;
            }

            CurrentEnemyName = enemy.Name;
            CurrentEnemyHP = enemy.HP;
            CurrentEnemyMaxHP = enemy.MaxHP;
            CurrentEnemyAttack = enemy.GetAttackDamage();
        }

        /// <summary>
        /// Aktualisiert die Inventar-Anzeige auf Basis des Inventars der Engine.
        /// </summary>
        private void SyncInventoryToUi()
        {
            var inv = _engine.Player.Inventar;
            var items = inv.Snapshot(); // erwartet eine Liste von IInventarItem
            InventoryItems = items;
        }

        /// <summary>
        /// Hängt Text an das GameLog-Textfeld an (für den später sichtbaren Game-Log).
        /// </summary>
        private void AppendLog(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;

            GameLog.AppendText(text);
            if (!text.EndsWith(Environment.NewLine))
            {
                GameLog.AppendText(Environment.NewLine);
            }

            GameLog.ScrollToEnd();
        }

        // ============================
        //   INotifyPropertyChanged
        // ============================

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // ============================
        //   Button-Handler
        // ============================

        /// <summary>
        /// Führt einen Angriff auf den aktuell ausgewählten Gegner aus.
        /// </summary>
        private void AttackButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedEnemy == null)
            {
                MessageBox.Show(
                    "Es ist kein Gegner ausgewählt.",
                    "Kein Gegner",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            string kampfText = _engine.Attack(SelectedEnemy);

            // Kampfprotokoll in den Game-Log schreiben
            AppendLog(string.Empty);
            AppendLog($"--- Kampf gegen {SelectedEnemy.Name} ---");
            AppendLog(kampfText);

            // UI nach dem Kampf aktualisieren
            SyncCharacterToUi();
            SyncCurrentEnemyToUi();
            SyncMetaToUi();
            SyncInventoryToUi();

            // Listen neu zeichnen
            EnemiesList.Items.Refresh();
            InventoryList.Items.Refresh();

            // Niederlage-Popup anzeigen, falls der Spieler gestorben ist
            if (kampfText.Contains("Du wurdest besiegt."))
            {
                MessageBox.Show(
                    "Du bist gestorben. Deine Coins und dein Inventar wurden gelöscht.\nDu startest mit vollen HP neu.",
                    "Niederlage",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Verwendet das aktuell im Inventar ausgewählte Item.
        /// HealPotion wirkt auf den Charakter, PoisonPotion auf den ausgewählten Gegner.
        /// </summary>
        private void UsePotionButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = InventoryList.SelectedItem as IInventarItem;

            if (selectedItem == null)
            {
                MessageBox.Show(
                    "Es ist kein Item im Inventar ausgewählt.",
                    "Kein Item",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            var target = SelectedEnemy;
            string result = _engine.UseInventoryItem(selectedItem, target);

            // Log-Eintrag und UI-Update
            AppendLog($"[Item] {result}");

            SyncCharacterToUi();
            SyncCurrentEnemyToUi();
            SyncMetaToUi();
            SyncInventoryToUi();

            EnemiesList.Items.Refresh();
            InventoryList.Items.Refresh();

            MessageBox.Show(
                result,
                "Item verwendet",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        /// <summary>
        /// Öffnet später ein separates Admin-Menü (derzeit nur Platzhalter).
        /// </summary>
        private void AdminMenuButton_Click(object sender, RoutedEventArgs e)
        {
            // Vorläufig nur Platzhalter – später eigenes Admin-Fenster oder Dialog
            MessageBox.Show(
                "Das Admin-Menü ist noch nicht implementiert.",
                "Admin-Menü",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        /// <summary>
        /// Beendet die Anwendung. Später kann hier eine Bestätigungsabfrage ergänzt werden.
        /// </summary>
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
