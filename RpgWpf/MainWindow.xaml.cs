using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using RpgWpf.GameCore;
using RpgWpf.GameLogic;

namespace RpgWpf
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        // ============================
        //   Engine / Spiellogik
        // ============================
        private GameEngine _engine;

        // ============================
        //   Properties für Bindings
        // ============================

        // --- Charakter-Daten (linke Box) ---

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
        /// 0..1 für {0:P0} in XAML (z. B. 0.2 = 20 %).
        /// </summary>
        public double SpecialProbability
        {
            get;
            set { field = value; OnPropertyChanged(nameof(SpecialProbability)); }
        }

        // --- Gegnerdaten (mittlere Box) ---

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

        // --- Shop-Anzeige (rechte Box) ---

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

        // --- Gegnerliste (Enemies-Übersicht) ---

        /// <summary>
        /// Liste aller sichtbaren Gegner für die Enemies-Übersicht.
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
        /// Sichtbare Liste der Inventar-Items des Spielers.
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

            // DataContext für Bindings
            DataContext = this;

            // Engine mit Beispielwerten erzeugen
            _engine = new GameEngine(
                vorname: "Tobi",
                playerTag: "Tobender50",
                alter: 20,
                inventarGroesse: 24
            );

            // Gegnerliste aus der Engine übernehmen (inkl. Slime, Orc, Dragon)
            Enemies = new List<Entity>(_engine.Enemies);

            // Standard-Gegner auswählen (erster Eintrag in der Liste)
            SelectedEnemy = Enemies.Count > 0 ? Enemies[0] : null;


            // UI initial synchronisieren
            SyncCharacterToUi();
            SyncInventoryToUi();
            SyncCurrentEnemyToUi();
            SyncMetaToUi();   // Coins + Defense aus der Engine übernehmen

        }

        // ============================
        //   Hilfsmethoden (UI Sync)
        // ============================

        /// <summary>
        /// Synchronisiert die grundlegenden Charakterdaten in die gebundenen Properties.
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
        }

        /// <summary>
        /// Synchronisiert Coins und Defense aus der GameEngine in die gebundenen Properties.
        /// </summary>
        private void SyncMetaToUi()
        {
            Coins = _engine.Coins;
            Defense = _engine.Defense;
        }


        /// <summary>
        /// Synchronisiert die Gegner-Anzeige (mittlere Box) anhand des aktuell ausgewählten Gegners.
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
        /// Aktualisiert die Inventar-Anzeige des Spielers.
        /// </summary>
        private void SyncInventoryToUi()
        {
            var inv = _engine.Player.Inventar;
            var items = inv.Snapshot(); // Erwartet eine Liste von IInventarItem

            InventoryItems = items;
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

            string kampfText;

            if (SelectedEnemy == _engine.Goblin)
            {
                kampfText = _engine.AttackGoblin();
            }
            else if (SelectedEnemy == _engine.Elfe)
            {
                kampfText = _engine.AttackElfe();
            }
            else if (SelectedEnemy == _engine.Werwolf)
            {
                kampfText = _engine.AttackWerwolf();
            }
            else
            {
                // Generische Behandlung für weitere Gegnertypen (Slime, Orc, Dragon, ...)
                kampfText = _engine.Attack(SelectedEnemy);
            }

            // UI nach dem Kampf aktualisieren
            SyncCharacterToUi();
            SyncCurrentEnemyToUi();
            SyncMetaToUi();        // Coins / Defense
            SyncInventoryToUi();   // Inventar (inkl. möglicher Drops)

            // Listen neu zeichnen
            EnemiesList.Items.Refresh();
            InventoryList.Items.Refresh();

            // kampfText kann später im Kampflog angezeigt werden
            _ = kampfText;
        }


        /// <summary>
        /// Verwendet das im Inventar ausgewählte Item.
        /// Die konkrete Logik für Heal-/Poison-Potions wird später ergänzt.
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

            // Zielgegner für PoisonPotion ist der aktuell ausgewählte Gegner.
            // Für HealPotion wird dieses Ziel in der Engine ignoriert und stattdessen der Spieler geheilt.
            var target = SelectedEnemy;

            string result = _engine.UseInventoryItem(selectedItem, target);

            // UI nach der Item-Verwendung aktualisieren
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
        /// Öffnet später ein separates Admin-Menü.
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
