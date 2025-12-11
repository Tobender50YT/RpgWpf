using RpgWpf.GameCore;
using RpgWpf.GameLogic;
using System.ComponentModel;
using System.Windows;

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
        /// Vereinfachter Zugriff auf den Spielercharakter der Engine.
        /// </summary>
        private Charakter Player => _engine.Player;

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

        /// <summary>Liste aller Gegner (für ComboBox im Admin-Fenster).</summary>
        public List<Entity> Enemies
        {
            get;
            set { field = value; OnPropertyChanged(nameof(Enemies)); }
        }

        /// <summary>Aktuell ausgewählter Gegner (in der ComboBox).</summary>
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

        public double SelectedEnemyHP
        {
            get;
            set { field = value; OnPropertyChanged(nameof(SelectedEnemyHP)); }
        }

        public double SelectedEnemyMaxHP
        {
            get;
            set { field = value; OnPropertyChanged(nameof(SelectedEnemyMaxHP)); }
        }

        /// <summary>Aktuelle Liste aller Inventar-Items (Snapshot für Anzeige).</summary>
        public List<IInventarItem> InventoryItems
        {
            get;
            set { field = value; OnPropertyChanged(nameof(InventoryItems)); }
        }

        /// <summary>Aktuell belegte Inventar-Slots.</summary>
        public int InventoryUsed
        {
            get;
            set { field = value; OnPropertyChanged(nameof(InventoryUsed)); }
        }

        /// <summary>Maximale Anzahl an Inventar-Slots.</summary>
        public int InventoryMax
        {
            get;
            set { field = value; OnPropertyChanged(nameof(InventoryMax)); }
        }

        // ============================
        //   Konstruktor
        // ============================
        /// <summary>
        /// Erstellt ein Admin-Fenster für die angegebene GameEngine.
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
            // Ersten Gegner auswählen, falls noch keiner ausgewählt ist
            if (SelectedEnemy == null && Enemies.Count > 0)
            {
                SelectedEnemy = Enemies[0];
            }
            else
            {
                SyncSelectedEnemy();
            }
            // Inventar-Daten synchronisieren
            var inv = p.Inventar;
            InventoryUsed = inv.UsedSize;
            InventoryMax = inv.MaxSize;
            InventoryItems = inv.Snapshot();
        }

        /// <summary>
        /// Aktualisiert die Anzeige für den aktuell ausgewählten Gegner (HP-Werte).
        /// </summary>
        private void SyncSelectedEnemy()
        {
            if (SelectedEnemy == null)
            {
                SelectedEnemyHP = 0;
                SelectedEnemyMaxHP = 0;
            }
            else
            {
                SelectedEnemyHP = SelectedEnemy.HP;
                SelectedEnemyMaxHP = SelectedEnemy.MaxHP;
            }
        }

        // ============================
        //   INotifyPropertyChanged
        // ============================
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // ============================
        //   Button-Handler – Spieler
        // ============================
        /// <summary>
        /// Setzt das Level des Spielers auf den eingegebenen Wert (EXP wird auf 0 gesetzt).
        /// </summary>
        private void SetLevelButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(LevelInputBox.Text, out int level) || level <= 0)
            {
                MessageBox.Show(
                    "Bitte einen gültigen Level-Wert > 0 eingeben.",
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
                    "Bitte einen gültigen (nicht negativen) EXP-Wert eingeben.",
                    "Ungültige EXP",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }
            int maxExp = Player.ExpToNextLevel;
            if (exp > maxExp) exp = maxExp;
            Player.SetCurrentExpFromAdmin(exp);
            SyncFromEngine();
        }

        /// <summary>
        /// Setzt die Coin-Anzahl direkt auf den eingegebenen Wert (nicht negativ).
        /// </summary>
        private void SetCoinsButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(CoinsInputBox.Text, out int value) || value < 0)
            {
                MessageBox.Show(
                    "Bitte einen gültigen (nicht negativen) Coin-Wert eingeben.",
                    "Ungültiger Wert",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }
            _engine.SetCoinsAdmin(value);
            SyncFromEngine();
        }

        /// <summary>
        /// Addiert einen Wert auf die aktuelle Coin-Anzahl (Negativ möglich, Minimum 0).
        /// </summary>
        private void AddCoinsButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(CoinsAddInputBox.Text, out int delta))
            {
                MessageBox.Show(
                    "Bitte einen gültigen Zahlenwert für Coins (Add) eingeben.",
                    "Ungültiger Wert",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }
            int newAmount = _engine.Coins + delta;
            if (newAmount < 0) newAmount = 0;
            _engine.SetCoinsAdmin(newAmount);
            SyncFromEngine();
        }

        /// <summary>
        /// Setzt die aktuellen HP des Spielers (begrenzt 0..MaxHP).
        /// </summary>
        private void SetHpButton_Click(object sender, RoutedEventArgs e)
        {
            if (!double.TryParse(HpSetInputBox.Text, out double value))
            {
                MessageBox.Show(
                    "Bitte einen gültigen Wert für HP (Set) eingeben.",
                    "Ungültiger Wert",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }
            if (value < 0) value = 0;
            if (value > Player.MaxHP) value = Player.MaxHP;
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
                    "Bitte einen gültigen Wert für HP (Add) eingeben.",
                    "Ungültiger Wert",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }
            double newHp = Player.HP + delta;
            if (newHp < 0) newHp = 0;
            if (newHp > Player.MaxHP) newHp = Player.MaxHP;
            Player.SetHP(newHp);
            SyncFromEngine();
        }

        /// <summary>
        /// Setzt die maximalen HP des Spielers auf den eingegebenen Wert (> 0).
        /// Falls aktuelle HP größer sind, werden sie auf die neue MaxHP begrenzt.
        /// </summary>
        private void SetMaxHpButton_Click(object sender, RoutedEventArgs e)
        {
            if (!double.TryParse(MaxHpInputBox.Text, out double value) || value <= 0)
            {
                MessageBox.Show(
                    "Bitte einen gültigen Wert für Max HP (> 0) eingeben.",
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
        /// Erhöht/Verringert die maximalen HP des Spielers um den eingegebenen Wert.
        /// </summary>
        private void AddMaxHpButton_Click(object sender, RoutedEventArgs e)
        {
            if (!double.TryParse(MaxHpAddInputBox.Text, out double delta))
            {
                MessageBox.Show(
                    "Bitte einen gültigen Wert für Max HP (Add) eingeben.",
                    "Ungültiger Wert",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }
            double newMaxHp = Player.MaxHP + delta;
            if (newMaxHp <= 0)
            {
                MessageBox.Show(
                    "Max HP darf nicht ≤ 0 werden.",
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
                    "Bitte einen gültigen (nicht negativen) Wert für Attack (Set) eingeben.",
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
                    "Bitte einen gültigen Wert für Attack (Add) eingeben.",
                    "Ungültiger Wert",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }
            double newAttack = Player.GetAttackDamage() + delta;
            if (newAttack < 0) newAttack = 0;
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
        /// Setzt die Chance auf einen Spezialangriff (in Prozent).
        /// </summary>
        private void SetSpecialChanceButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(SpecialChanceInputBox.Text, out int percent))
            {
                MessageBox.Show(
                    "Bitte einen gültigen Prozentwert (0–100) für die Special Chance eingeben.",
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
        //   Button-Handler – Gegner
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
        //   Button-Handler – Inventar
        // ============================
        /// <summary>
        /// Löscht das gesamte Inventar des Spielers.
        /// </summary>
        private void ClearInventoryButton_Click(object sender, RoutedEventArgs e)
        {
            var inv = Player.Inventar;
            foreach (var item in inv.Snapshot())
            {
                inv.Remove(item);
            }
            SyncFromEngine();
        }

        /// <summary>
        /// Fügt einen Heiltrank der Stufe I ins Inventar ein.
        /// </summary>
        private void AddHealPotion1Button_Click(object sender, RoutedEventArgs e)
        {
            Player.Inventar.Add(new HealPotion(ItemGroesse: 1));
            SyncFromEngine();
        }

        /// <summary>
        /// Fügt einen Heiltrank der Stufe II ins Inventar ein.
        /// </summary>
        private void AddHealPotion2Button_Click(object sender, RoutedEventArgs e)
        {
            Player.Inventar.Add(new HealPotion(ItemGroesse: 2));
            SyncFromEngine();
        }

        /// <summary>
        /// Fügt einen Heiltrank der Stufe III ins Inventar ein.
        /// </summary>
        private void AddHealPotion3Button_Click(object sender, RoutedEventArgs e)
        {
            Player.Inventar.Add(new HealPotion(ItemGroesse: 3));
            SyncFromEngine();
        }

        /// <summary>
        /// Fügt einen Gifttrank der Stufe I ins Inventar ein.
        /// </summary>
        private void AddPoisonPotion1Button_Click(object sender, RoutedEventArgs e)
        {
            Player.Inventar.Add(new PoisonPotion(ItemGroesse: 1));
            SyncFromEngine();
        }

        /// <summary>
        /// Fügt einen Gifttrank der Stufe II ins Inventar ein.
        /// </summary>
        private void AddPoisonPotion2Button_Click(object sender, RoutedEventArgs e)
        {
            Player.Inventar.Add(new PoisonPotion(ItemGroesse: 2));
            SyncFromEngine();
        }

        /// <summary>
        /// Fügt einen Gifttrank der Stufe III ins Inventar ein.
        /// </summary>
        private void AddPoisonPotion3Button_Click(object sender, RoutedEventArgs e)
        {
            Player.Inventar.Add(new PoisonPotion(ItemGroesse: 3));
            SyncFromEngine();
        }

        /// <summary>
        /// Setzt den gesamten Spielstand des Spielers zurück.
        /// Zeigt vorher eine Warnung, da diese Aktion nicht rückgängig gemacht werden kann.
        /// </summary>
        private void ResetPlayerButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Der komplette Spielstand des Spielers (Level, EXP, Coins, Inventar, Kampfwerte) wird zurückgesetzt.\n\nFortfahren?",
                "Spieler zurücksetzen – Warnung",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;
            _engine.ResetPlayerProgress();
            SyncFromEngine();
        }
    }
}
