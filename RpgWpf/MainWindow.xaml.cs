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

        /// <summary>
        /// Aktuelle Coin-Anzahl für die Shop-Anzeige.
        /// </summary>
        public int Coins
        {
            get;
            set { field = value; OnPropertyChanged(nameof(Coins)); }
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
                inventarGroesse: 20
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
        /// Synchronisiert Meta-Daten (Coins) in die Bindings.
        /// </summary>
        private void SyncMetaToUi()
        {
            Coins = _engine.Coins;
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
        /// Aktualisiert die Inventar-Anzeige auf Basis des Inventars aus der Engine.
        /// </summary>
        private void SyncInventoryToUi()
        {
            var inv = _engine.Player.Inventar;
            var items = inv.Snapshot();
            InventoryItems = items;
        }

        /// <summary>
        /// Hängt Text an das GameLog-Textfeld an (sichtbarer Game-Log in der UI).
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
        //   Button-Handler (Kampf / Admin)
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
                    "Der Charakter ist gestorben. Coins und Inventar wurden gelöscht.\nDer Neustart erfolgt mit vollen HP.",
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

        // ============================
        //   Shop-Button-Handler
        // ============================

        /// <summary>
        /// Kauft ein Attack-Upgrade (+2 Damage) für 20 Coins.
        /// </summary>
        private void BuyAttackButton_Click(object sender, RoutedEventArgs e)
        {
            const int cost = 20;
            const double increase = 2.0;

            bool success = _engine.TryBuyAttackUpgrade(cost, increase, out string message);

            AppendLog("[Shop] " + message);

            // Bei Erfolg Character-Werte und Coins aktualisieren
            if (success)
            {
                SyncCharacterToUi();
                SyncMetaToUi();
            }

            MessageBox.Show(
                message,
                "Shop – Attack upgrade",
                MessageBoxButton.OK,
                success ? MessageBoxImage.Information : MessageBoxImage.Warning);
        }

        /// <summary>
        /// Kauft ein Max-Health-Upgrade (+20 MaxHP) für 15 Coins.
        /// MaxHP und aktuelle HP werden in der Engine angepasst.
        /// </summary>
        private void BuyHealthButton_Click(object sender, RoutedEventArgs e)
        {
            const int cost = 15;
            const double hpIncrease = 20.0; // Aufwertung der MaxHP

            bool success = _engine.TryBuyMaxHealthUpgrade(cost, hpIncrease, out string message);

            AppendLog("[Shop] " + message);

            if (success)
            {
                // MaxHP und aktuelle HP haben sich geändert → Character-UI aktualisieren
                SyncCharacterToUi();
                SyncMetaToUi();
            }

            MessageBox.Show(
                message,
                "Shop – Max health upgrade",
                MessageBoxButton.OK,
                success ? MessageBoxImage.Information : MessageBoxImage.Warning);
        }

        /// <summary>
        /// Kauft eine HealPotion Stufe 1 und fügt sie dem Inventar hinzu.
        /// </summary>
        private void BuyHealPotion1Button_Click(object sender, RoutedEventArgs e)
        {
            bool success = _engine.TryBuyHealPotion(1, out string message);

            AppendLog("[Shop] " + message);

            if (success)
            {
                SyncMetaToUi();
                SyncInventoryToUi();
                InventoryList.Items.Refresh();
            }

            MessageBox.Show(
                message,
                "Shop – Heal Potion I",
                MessageBoxButton.OK,
                success ? MessageBoxImage.Information : MessageBoxImage.Warning);
        }

        /// <summary>
        /// Kauft eine HealPotion Stufe 2 und fügt sie dem Inventar hinzu.
        /// </summary>
        private void BuyHealPotion2Button_Click(object sender, RoutedEventArgs e)
        {
            bool success = _engine.TryBuyHealPotion(2, out string message);

            AppendLog("[Shop] " + message);

            if (success)
            {
                SyncMetaToUi();
                SyncInventoryToUi();
                InventoryList.Items.Refresh();
            }

            MessageBox.Show(
                message,
                "Shop – Heal Potion II",
                MessageBoxButton.OK,
                success ? MessageBoxImage.Information : MessageBoxImage.Warning);
        }

        /// <summary>
        /// Kauft eine PoisonPotion Stufe 1 und fügt sie dem Inventar hinzu.
        /// </summary>
        private void BuyPoisonPotion1Button_Click(object sender, RoutedEventArgs e)
        {
            bool success = _engine.TryBuyPoisonPotion(1, out string message);

            AppendLog("[Shop] " + message);

            if (success)
            {
                SyncMetaToUi();
                SyncInventoryToUi();
                InventoryList.Items.Refresh();
            }

            MessageBox.Show(
                message,
                "Shop – Poison Potion I",
                MessageBoxButton.OK,
                success ? MessageBoxImage.Information : MessageBoxImage.Warning);
        }

        /// <summary>
        /// Kauft eine PoisonPotion Stufe 2 und fügt sie dem Inventar hinzu.
        /// </summary>
        private void BuyPoisonPotion2Button_Click(object sender, RoutedEventArgs e)
        {
            bool success = _engine.TryBuyPoisonPotion(2, out string message);

            AppendLog("[Shop] " + message);

            if (success)
            {
                SyncMetaToUi();
                SyncInventoryToUi();
                InventoryList.Items.Refresh();
            }

            MessageBox.Show(
                message,
                "Shop – Poison Potion II",
                MessageBoxButton.OK,
                success ? MessageBoxImage.Information : MessageBoxImage.Warning);
        }
    }
}
