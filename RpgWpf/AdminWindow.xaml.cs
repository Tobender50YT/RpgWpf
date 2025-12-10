using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
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

        /// <summary>
        /// Kürzel für den Spieler aus der Engine, um den Zugriff im Admin-Fenster zu vereinfachen.
        /// </summary>
        private Charakter Player => _engine.Player;

        // ============================
        //   Properties für Bindings
        // ============================

        /// <summary>
        /// Aktuelle Coin-Anzahl laut GameEngine.
        /// </summary>
        public int Coins
        {
            get;
            set { field = value; OnPropertyChanged(nameof(Coins)); }
        }

        /// <summary>
        /// Aktuelles Spieler-Level.
        /// </summary>
        public int Level
        {
            get;
            set { field = value; OnPropertyChanged(nameof(Level)); }
        }

        /// <summary>
        /// Aktuelle Erfahrungspunkte im aktuellen Level.
        /// </summary>
        public int CurrentExp
        {
            get;
            set { field = value; OnPropertyChanged(nameof(CurrentExp)); }
        }

        /// <summary>
        /// Benötigte Erfahrungspunkte bis zum nächsten Level.
        /// </summary>
        public int ExpToNextLevel
        {
            get;
            set { field = value; OnPropertyChanged(nameof(ExpToNextLevel)); }
        }

        /// <summary>
        /// Aktuelle Lebenspunkte des Spielers.
        /// </summary>
        public double HP
        {
            get;
            set { field = value; OnPropertyChanged(nameof(HP)); }
        }

        /// <summary>
        /// Maximal mögliche Lebenspunkte des Spielers.
        /// </summary>
        public double MaxHP
        {
            get;
            set { field = value; OnPropertyChanged(nameof(MaxHP)); }
        }

        /// <summary>
        /// Aktueller Angriffsschaden (inkl. aller Modifikatoren).
        /// </summary>
        public double AttackDamage
        {
            get;
            set { field = value; OnPropertyChanged(nameof(AttackDamage)); }
        }

        /// <summary>
        /// Multiplikator für Spezialangriffe.
        /// </summary>
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

        /// <summary>
        /// Erstellt ein neues Admin-Fenster für die angegebene GameEngine.
        /// </summary>
        /// <param name="engine">Aktive Spiel-Engine.</param>
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
            var p = Player;

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
            InventoryUsed = inv.UsedSize;
            InventoryMax = inv.MaxSize;
            InventoryItems = inv.Snapshot();
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

        /// <summary>
        /// Löst PropertyChanged für die angegebene Property aus.
        /// </summary>
        /// <param name="propertyName">Name der geänderten Property.</param>
        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // ============================
        //   Button-Handler – Player
        // ============================

        /// <summary>
        /// Setzt das Level des Spielers direkt auf den eingegebenen Wert.
        /// EXP wird dabei auf 0 zurückgesetzt.
        /// </summary>
        private void SetLevelButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(LevelInputBox.Text, out int level) || level <= 0)
            {
                MessageBox.Show(
                    "Bitte einen gültigen Level-Wert größer 0 eingeben.",
                    "Ungültiger Level",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            Player.SetLevelFromAdmin(level);

            SyncFromEngine();
        }

        /// <summary>
        /// Setzt die aktuellen EXP des Spielers innerhalb des aktuellen Levels.
        /// </summary>
        private void SetExpButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(ExpInputBox.Text, out int exp) || exp < 0)
            {
                MessageBox.Show(
                    "Bitte einen gültigen, nicht negativen EXP-Wert eingeben.",
                    "Ungültige EXP",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            int maxExp = Player.ExpToNextLevel;
            if (exp > maxExp)
            {
                exp = maxExp;
            }

            Player.SetCurrentExpFromAdmin(exp);

            SyncFromEngine();
        }

        /// <summary>
        /// Setzt die Coin-Anzahl direkt auf den eingegebenen Wert.
        /// Negative Eingaben werden abgewiesen.
        /// </summary>
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

        /// <summary>
        /// Addiert einen Betrag auf die aktuelle Coin-Anzahl.
        /// Negative Werte sind erlaubt, Coins werden jedoch nicht kleiner als 0.
        /// </summary>
        private void AddCoinsButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(CoinsAddInputBox.Text, out int delta))
            {
                MessageBox.Show(
                    "Bitte einen gültigen Zahlenwert für Coins (add) eingeben.",
                    "Ungültiger Wert",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            int newAmount = _engine.Coins + delta;
            if (newAmount < 0)
            {
                newAmount = 0;
            }

            _engine.SetCoinsAdmin(newAmount);
            SyncFromEngine();
        }

        /// <summary>
        /// Setzt die aktuellen HP des Spielers.
        /// </summary>
        private void SetHpButton_Click(object sender, RoutedEventArgs e)
        {
            if (!double.TryParse(HpSetInputBox.Text, out double value))
            {
                MessageBox.Show(
                    "Bitte einen gültigen Wert für HP (set) eingeben.",
                    "Ungültiger Wert",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            if (value < 0)
            {
                value = 0;
            }

            if (value > Player.MaxHP)
            {
                value = Player.MaxHP;
            }

            Player.SetHP(value);
            SyncFromEngine();
        }

        /// <summary>
        /// Addiert einen Wert auf die aktuellen HP des Spielers.
        /// </summary>
        private void AddHpButton_Click(object sender, RoutedEventArgs e)
        {
            if (!double.TryParse(HpAddInputBox.Text, out double delta))
            {
                MessageBox.Show(
                    "Bitte einen gültigen Wert für HP (add) eingeben.",
                    "Ungültiger Wert",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            double newHp = Player.HP + delta;

            if (newHp < 0)
            {
                newHp = 0;
            }

            if (newHp > Player.MaxHP)
            {
                newHp = Player.MaxHP;
            }

            Player.SetHP(newHp);
            SyncFromEngine();
        }

        /// <summary>
        /// Setzt die maximalen HP des Spielers auf den eingegebenen Wert.
        /// HP werden bei Bedarf auf die neue Obergrenze begrenzt.
        /// </summary>
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

            Player.SetMaxHP(value);

            if (Player.HP > Player.MaxHP)
            {
                Player.SetHP(Player.MaxHP);
            }

            SyncFromEngine();
        }

        /// <summary>
        /// Erhöht oder verringert die maximalen HP des Spielers um den eingegebenen Wert.
        /// </summary>
        private void AddMaxHpButton_Click(object sender, RoutedEventArgs e)
        {
            if (!double.TryParse(MaxHpAddInputBox.Text, out double delta))
            {
                MessageBox.Show(
                    "Bitte einen gültigen Wert für Max HP (add) eingeben.",
                    "Ungültiger Wert",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            double newMaxHp = Player.MaxHP + delta;
            if (newMaxHp <= 0)
            {
                MessageBox.Show(
                    "Max HP darf nicht kleiner oder gleich 0 werden.",
                    "Ungültiger Wert",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            Player.SetMaxHP(newMaxHp);

            if (Player.HP > Player.MaxHP)
            {
                Player.SetHP(Player.MaxHP);
            }

            SyncFromEngine();
        }

        /// <summary>
        /// Heilt den Spieler vollständig auf seine maximalen HP.
        /// </summary>
        private void HealPlayerButton_Click(object sender, RoutedEventArgs e)
        {
            Player.SetHP(Player.MaxHP);
            SyncFromEngine();
        }

        /// <summary>
        /// Setzt den Basis-Angriffsschaden des Spielers.
        /// </summary>
        private void SetAttackButton_Click(object sender, RoutedEventArgs e)
        {
            if (!double.TryParse(AttackSetInputBox.Text, out double value) || value < 0)
            {
                MessageBox.Show(
                    "Bitte einen gültigen, nicht negativen Wert für Attack (set) eingeben.",
                    "Ungültiger Wert",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            Player.SetBaseAttack(value);
            SyncFromEngine();
        }

        /// <summary>
        /// Addiert einen Wert auf den Basis-Angriffsschaden des Spielers.
        /// </summary>
        private void AddAttackButton_Click(object sender, RoutedEventArgs e)
        {
            if (!double.TryParse(AttackAddInputBox.Text, out double delta))
            {
                MessageBox.Show(
                    "Bitte einen gültigen Wert für Attack (add) eingeben.",
                    "Ungültiger Wert",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            double newAttack = Player.GetAttackDamage() + delta;
            if (newAttack < 0)
            {
                newAttack = 0;
            }

            Player.SetBaseAttack(newAttack);
            SyncFromEngine();
        }

        /// <summary>
        /// Setzt den Damage-Multiplikator für Spezialangriffe.
        /// </summary>
        private void SetDamageMultiplierButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(DamageMultiplierInputBox.Text, out int value) || value < 1)
            {
                MessageBox.Show(
                    "Bitte einen gültigen ganzzahligen Multiplikator (min. 1) eingeben.",
                    "Ungültiger Wert",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            Player.SetDamageMultiplierFromAdmin(value);
            SyncFromEngine();
        }

        /// <summary>
        /// Setzt die Chance auf einen Spezialangriff in Prozent.
        /// </summary>
        private void SetSpecialChanceButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(SpecialChanceInputBox.Text, out int percent))
            {
                MessageBox.Show(
                    "Bitte einen gültigen Prozentwert (0–100) für die Special chance eingeben.",
                    "Ungültiger Wert",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            if (percent < 0) percent = 0;
            if (percent > 100) percent = 100;

            Player.SetSpecialAttackChancePercentFromAdmin(percent);
            SyncFromEngine();
        }

        // ============================
        //   Button-Handler – Enemies
        // ============================

        /// <summary>
        /// Setzt die HP des ausgewählten Gegners auf dessen MaxHP.
        /// </summary>
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

        /// <summary>
        /// Setzt die HP des ausgewählten Gegners auf 0.
        /// </summary>
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

        /// <summary>
        /// Setzt die HP des ausgewählten Gegners auf den eingegebenen Wert (0..MaxHP).
        /// </summary>
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

        /// <summary>
        /// Löscht das komplette Inventar des Spielers.
        /// </summary>
        private void ClearInventoryButton_Click(object sender, RoutedEventArgs e)
        {
            var inv = Player.Inventar;
            var items = inv.Snapshot();

            foreach (var item in items)
            {
                inv.Remove(item);
            }

            SyncFromEngine();
        }

        /// <summary>
        /// Fügt einen Heal Potion der Stufe I ins Inventar ein.
        /// </summary>
        private void AddHealPotion1Button_Click(object sender, RoutedEventArgs e)
        {
            var inv = Player.Inventar;
            inv.Add(new HealPotion(ItemGroesse: 1));
            SyncFromEngine();
        }

        /// <summary>
        /// Fügt einen Heal Potion der Stufe II ins Inventar ein.
        /// </summary>
        private void AddHealPotion2Button_Click(object sender, RoutedEventArgs e)
        {
            var inv = Player.Inventar;
            inv.Add(new HealPotion(ItemGroesse: 2));
            SyncFromEngine();
        }

        /// <summary>
        /// Fügt einen Heal Potion der Stufe III ins Inventar ein.
        /// </summary>
        private void AddHealPotion3Button_Click(object sender, RoutedEventArgs e)
        {
            var inv = Player.Inventar;
            inv.Add(new HealPotion(ItemGroesse: 3));
            SyncFromEngine();
        }

        /// <summary>
        /// Fügt einen Poison Potion der Stufe I ins Inventar ein.
        /// </summary>
        private void AddPoisonPotion1Button_Click(object sender, RoutedEventArgs e)
        {
            var inv = Player.Inventar;
            inv.Add(new PoisonPotion(ItemGroesse: 1));
            SyncFromEngine();
        }

        /// <summary>
        /// Fügt einen Poison Potion der Stufe II ins Inventar ein.
        /// </summary>
        private void AddPoisonPotion2Button_Click(object sender, RoutedEventArgs e)
        {
            var inv = Player.Inventar;
            inv.Add(new PoisonPotion(ItemGroesse: 2));
            SyncFromEngine();
        }

        /// <summary>
        /// Fügt einen Poison Potion der Stufe III ins Inventar ein.
        /// </summary>
        private void AddPoisonPotion3Button_Click(object sender, RoutedEventArgs e)
        {
            var inv = Player.Inventar;
            inv.Add(new PoisonPotion(ItemGroesse: 3));
            SyncFromEngine();
        }

        /// <summary>
        /// Aktuell belegte Inventar-Slots.
        /// </summary>
        public int InventoryUsed
        {
            get;
            set { field = value; OnPropertyChanged(nameof(InventoryUsed)); }
        }

        /// <summary>
        /// Maximale Inventar-Slots.
        /// </summary>
        public int InventoryMax
        {
            get;
            set { field = value; OnPropertyChanged(nameof(InventoryMax)); }
        }

    }
}
