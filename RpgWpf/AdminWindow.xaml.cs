using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using RpgWpf.GameCore;
using RpgWpf.GameLogic;

namespace RpgWpf
{
    /// <summary>
    /// Admin-Fenster für Debug- und Testfunktionen.
    /// Ermöglicht das Anpassen von Spieler-, Gegner- und Inventarwerten.
    /// </summary>
    public partial class AdminWindow : Window, INotifyPropertyChanged
    {
        private readonly GameEngine _engine;

        // ============================
        //   Properties für Bindings
        // ============================

        public int Coins
        {
            get;
            set { field = value; OnPropertyChanged(nameof(Coins)); }
        }

        public int Level
        {
            get;
            set { field = value; OnPropertyChanged(nameof(Level)); }
        }

        public int CurrentExp
        {
            get;
            set { field = value; OnPropertyChanged(nameof(CurrentExp)); }
        }

        public int ExpToNextLevel
        {
            get;
            set { field = value; OnPropertyChanged(nameof(ExpToNextLevel)); }
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

        /// <summary>
        /// Liste aller Gegner für das Admin-Fenster.
        /// </summary>
        public List<Entity> Enemies
        {
            get;
            set { field = value; OnPropertyChanged(nameof(Enemies)); }
        }

        /// <summary>
        /// Aktuell ausgewählter Gegner.
        /// </summary>
        public Entity SelectedEnemy
        {
            get;
            set
            {
                field = value;
                OnPropertyChanged(nameof(SelectedEnemy));
                SyncSelectedEnemy();
            }
        }

        /// <summary>
        /// Aktuelle HP des ausgewählten Gegners.
        /// </summary>
        public double SelectedEnemyHP
        {
            get;
            set { field = value; OnPropertyChanged(nameof(SelectedEnemyHP)); }
        }

        /// <summary>
        /// MaxHP des ausgewählten Gegners.
        /// </summary>
        public double SelectedEnemyMaxHP
        {
            get;
            set { field = value; OnPropertyChanged(nameof(SelectedEnemyMaxHP)); }
        }

        /// <summary>
        /// Snapshot der Inventar-Items für die Anzeige.
        /// </summary>
        public List<IInventarItem> InventoryItems
        {
            get;
            set { field = value; OnPropertyChanged(nameof(InventoryItems)); }
        }

        // ============================
        //   Konstruktor
        // ============================

        public AdminWindow(GameEngine engine)
        {
            InitializeComponent();

            _engine = engine ?? throw new ArgumentNullException(nameof(engine));

            DataContext = this;

            SyncFromEngine();
        }

        // ============================
        //   Sync-Helfer
        // ============================

        /// <summary>
        /// Synchronisiert alle relevanten Werte aus der GameEngine in die Admin-Properties.
        /// </summary>
        private void SyncFromEngine()
        {
            var p = _engine.Player;

            Coins = _engine.Coins;
            Level = p.Level;
            CurrentExp = p.CurrentExp;
            ExpToNextLevel = p.ExpToNextLevel;

            HP = p.HP;
            MaxHP = p.MaxHP;
            AttackDamage = p.GetAttackDamage();
            DamageMultiplier = p.DamageMultiplier;

            Enemies = new List<Entity>(_engine.Enemies);

            if (SelectedEnemy == null && Enemies.Count > 0)
            {
                SelectedEnemy = Enemies[0];
            }
            else
            {
                SyncSelectedEnemy();
            }

            var inv = p.Inventar;
            InventoryItems = inv.Snapshot();

            AdminEnemiesList.Items.Refresh();
            AdminInventoryList.Items.Refresh();
        }

        /// <summary>
        /// Synchronisiert die Anzeige für den aktuell ausgewählten Gegner.
        /// </summary>
        private void SyncSelectedEnemy()
        {
            if (SelectedEnemy == null)
            {
                SelectedEnemyHP = 0;
                SelectedEnemyMaxHP = 0;
                return;
            }

            SelectedEnemyHP = SelectedEnemy.HP;
            SelectedEnemyMaxHP = SelectedEnemy.MaxHP;
        }

        // ============================
        //   INotifyPropertyChanged
        // ============================

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // ============================
        //   Button-Handler – Player
        // ============================

        private void SetCoinsButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(CoinsInputBox.Text, out int value) || value < 0)
            {
                MessageBox.Show(
                    "Bitte einen gültigen, nicht negativen Zahlenwert für Coins eingeben.",
                    "Ungültiger Wert",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            _engine.SetCoinsAdmin(value);
            SyncFromEngine();
        }

        private void SetMaxHpButton_Click(object sender, RoutedEventArgs e)
        {
            if (!double.TryParse(MaxHpInputBox.Text, out double value) || value <= 0)
            {
                MessageBox.Show(
                    "Bitte einen gültigen Wert für Max HP eingeben (größer 0).",
                    "Ungültiger Wert",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // MaxHP setzen
            _engine.Player.SetMaxHP(value);

            // HP begrenzen, falls sie über der neuen MaxHP liegen
            if (_engine.Player.HP > _engine.Player.MaxHP)
            {
                _engine.Player.SetHP(_engine.Player.MaxHP);
            }

            SyncFromEngine();
        }

        private void HealPlayerButton_Click(object sender, RoutedEventArgs e)
        {
            _engine.Player.SetHP(_engine.Player.MaxHP);
            SyncFromEngine();
        }

        // ============================
        //   Button-Handler – Enemies
        // ============================

        private void ResetEnemyHpButton_Click(object sender, RoutedEventArgs e)
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

            SelectedEnemy.SetHP(SelectedEnemy.MaxHP);
            SyncFromEngine();
        }

        private void KillEnemyButton_Click(object sender, RoutedEventArgs e)
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

            SelectedEnemy.SetHP(0);
            SyncFromEngine();
        }

        private void SetEnemyHpButton_Click(object sender, RoutedEventArgs e)
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

            if (!double.TryParse(EnemyHpInputBox.Text, out double value))
            {
                MessageBox.Show(
                    "Bitte einen gültigen Wert für die HP eingeben.",
                    "Ungültiger Wert",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            if (value < 0) value = 0;
            if (value > SelectedEnemy.MaxHP) value = SelectedEnemy.MaxHP;

            SelectedEnemy.SetHP(value);
            SyncFromEngine();
        }

        // ============================
        //   Button-Handler – Inventory
        // ============================

        private void ClearInventoryButton_Click(object sender, RoutedEventArgs e)
        {
            var inv = _engine.Player.Inventar;
            var items = inv.Snapshot();

            foreach (var item in items)
            {
                inv.Remove(item);
            }

            SyncFromEngine();
        }

        private void AddHealPotion1Button_Click(object sender, RoutedEventArgs e)
        {
            var inv = _engine.Player.Inventar;
            inv.Add(new HealPotion(ItemGroesse: 1));
            SyncFromEngine();
        }

        private void AddHealPotion2Button_Click(object sender, RoutedEventArgs e)
        {
            var inv = _engine.Player.Inventar;
            inv.Add(new HealPotion(ItemGroesse: 2));
            SyncFromEngine();
        }

        private void AddPoisonPotion1Button_Click(object sender, RoutedEventArgs e)
        {
            var inv = _engine.Player.Inventar;
            inv.Add(new PoisonPotion(ItemGroesse: 1));
            SyncFromEngine();
        }

        private void AddPoisonPotion2Button_Click(object sender, RoutedEventArgs e)
        {
            var inv = _engine.Player.Inventar;
            inv.Add(new PoisonPotion(ItemGroesse: 2));
            SyncFromEngine();
        }
    }
}
