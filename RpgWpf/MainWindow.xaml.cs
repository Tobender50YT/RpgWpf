using RpgWpf.GameCore;
using RpgWpf.GameLogic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;

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

        /// <summary>
        /// Timer für die automatische Lebenspunkte-Regeneration.
        /// </summary>
        private readonly DispatcherTimer _hpRegenTimer;

        // ============================
        //   Properties für Bindings
        // ============================

        // --- Charakter-Box (links) ---
        /// <summary>
        /// Kennzeichnet, ob der aktuell gespielte Charakter Admin-Rechte hat.
        /// Wird aus der GameEngine gespiegelt (steuert Sichtbarkeit des Admin-Buttons).
        /// </summary>
        public bool IsAdmin
        {
            get;
            set
            {
                field = value;
                OnPropertyChanged(nameof(IsAdmin));
                // Sichtbarkeit des Admin-Buttons direkt anpassen
                AdminMenuButton.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }

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
        /// Spezialangriffs-Wahrscheinlichkeit 0..1 (Formatierung via {0:P0} in XAML).
        /// </summary>
        public double SpecialProbability
        {
            get;
            set { field = value; OnPropertyChanged(nameof(SpecialProbability)); }
        }

        /// <summary>Aktuelles Spielerlevel.</summary>
        public int Level
        {
            get;
            set { field = value; OnPropertyChanged(nameof(Level)); }
        }

        /// <summary>Aktuelle Erfahrungspunkte im aktuellen Level.</summary>
        public int CurrentExp
        {
            get;
            set { field = value; OnPropertyChanged(nameof(CurrentExp)); }
        }

        /// <summary>Benötigte Erfahrungspunkte bis zum nächsten Level.</summary>
        public int ExpToNextLevel
        {
            get;
            set { field = value; OnPropertyChanged(nameof(ExpToNextLevel)); }
        }

        // --- Gegner-Box (Mitte) ---
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

        // --- Inventar-Anzeige (untere linke Box) ---
        public int InventoryUsed
        {
            get;
            set { field = value; OnPropertyChanged(nameof(InventoryUsed)); }
        }

        public int InventoryMax
        {
            get;
            set { field = value; OnPropertyChanged(nameof(InventoryMax)); }
        }

        // --- Shop-Box (rechts) ---
        /// <summary>Aktuelle Coin-Anzahl für die Shop-Anzeige.</summary>
        public int Coins
        {
            get;
            set { field = value; OnPropertyChanged(nameof(Coins)); }
        }

        // --- Gegnerliste (mittlere Zeile) ---
        /// <summary>Liste aller sichtbaren Gegner (für EnemiesList).</summary>
        public List<Entity> Enemies
        {
            get;
            private set { field = value; OnPropertyChanged(nameof(Enemies)); }
        }

        /// <summary>Aktuell im UI ausgewählter Gegner.</summary>
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
        /// <summary>Liste der Items im Inventar des Spielers.</summary>
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

            // GameEngine erzeugen und initialisieren
            _engine = new GameEngine(
                vorname: "Tobi",
                playerTag: "Test_Player1",
                alter: 21,
                inventarGroesse: 8
            );
            _engine.Player.IsAdmin = true;

            // Admin-Status in die UI spiegeln
            IsAdmin = _engine.Player.IsAdmin;

            // Gegnerliste aus der Engine übernehmen (inkl. Slime, Orc, Dragon etc.)
            Enemies = new List<Entity>(_engine.Enemies);
            SelectedEnemy = Enemies.Count > 0 ? Enemies[0] : null;

            // UI mit Startwerten synchronisieren
            SyncCharacterToUi();
            SyncInventoryToUi();
            SyncCurrentEnemyToUi();
            SyncMetaToUi();

            // Willkommensnachricht und Status der Engine in den Game-Log schreiben
            AppendLog("Willkommen in deinem WPF-RPG!");
            AppendLog(_engine.GetStatusText());

            // Auto-Heal Timer initialisieren (regeneriert alle 3 Sekunden 1 HP)
            _hpRegenTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)
            };
            _hpRegenTimer.Tick += HpRegenTimer_Tick;
            _hpRegenTimer.Start();
        }

        // ============================
        //   Hilfsmethoden (UI-Sync)
        // ============================
        /// <summary>
        /// Tick-Handler für die automatische Lebenspunkte-Regeneration.
        /// Erhöht alle 3 Sekunden die HP um 1, solange der Spieler lebt und noch nicht voll geheilt ist.
        /// </summary>
        private void HpRegenTimer_Tick(object? sender, EventArgs e)
        {
            var player = _engine.Player;
            if (player.HP <= 0) return;               // Kein Auto-Heal, wenn tot
            if (player.HP >= player.MaxHP) return;    // Kein Auto-Heal, wenn volle HP

            // 1 HP regenerieren (maximal bis zur vollen HP)
            double newHp = Math.Min(player.MaxHP, player.HP + 1);
            player.SetHP(newHp);

            // Charakter-Display aktualisieren
            SyncCharacterToUi();
        }

        /// <summary>
        /// Synchronisiert die grundlegenden Charakterdaten (inkl. Level/EXP) in die Bindings.
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
        /// Synchronisiert Meta-Daten (Coins) in die Bindings und aktualisiert die Shop-Buttons.
        /// </summary>
        private void SyncMetaToUi()
        {
            Coins = _engine.Coins;
            UpdateShopButtonsEnabledState();
        }

        /// <summary>
        /// Aktiviert/Deaktiviert Shop-Buttons abhängig von Coins (ausgegraut, wenn zu teuer).
        /// </summary>
        private void UpdateShopButtonsEnabledState()
        {
            int coins = _engine.Coins;
            // Preise laut GameEngine: Attack 20, MaxHealth 15, Heal I 12, Heal II 20, Poison I 10, Poison II 18
            AttackUpgradeButton.IsEnabled = coins >= 20;
            HealthUpgradeButton.IsEnabled = coins >= 15;
            HealPotion1Button.IsEnabled = coins >= 12;
            HealPotion2Button.IsEnabled = coins >= 20;
            PoisonPotion1Button.IsEnabled = coins >= 10;
            PoisonPotion2Button.IsEnabled = coins >= 18;
        }

        /// <summary>
        /// Synchronisiert die Enemy-Box auf Basis des aktuell ausgewählten Gegners.
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
        /// Aktualisiert die Inventar-Anzeige anhand des Inventars aus der Engine.
        /// </summary>
        private void SyncInventoryToUi()
        {
            var inv = _engine.Player.Inventar;
            InventoryUsed = inv.UsedSize;
            InventoryMax = inv.MaxSize;
            InventoryItems = inv.Snapshot();
        }

        /// <summary>
        /// Hängt Text an den GameLog-TextBox an (sichtbarer Spiele-Log).
        /// Fügt bei Bedarf automatisch einen Zeilenumbruch an und scrollt nach unten.
        /// </summary>
        private void AppendLog(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            GameLog.AppendText(text);
            if (!text.EndsWith(Environment.NewLine))
            {
                GameLog.AppendText(Environment.NewLine);
            }
            GameLog.ScrollToEnd();
        }

        /// <summary>
        /// Aktualisiert alle anzeigerelevanten UI-Elemente (Charakter, Gegner, Meta, Inventar).
        /// </summary>
        private void RefreshUI()
        {
            // Alle Datenfelder aus Engine in UI-Properties übertragen
            SyncCharacterToUi();
            SyncCurrentEnemyToUi();
            SyncMetaToUi();
            SyncInventoryToUi();
            // Listen manuell aktualisieren (Werte können sich geändert haben)
            EnemiesList.Items.Refresh();
            InventoryList.Items.Refresh();
        }

        // ============================
        //   INotifyPropertyChanged
        // ============================
        public event PropertyChangedEventHandler? PropertyChanged;
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
            AppendLog($"--- Kampf gegen {SelectedEnemy.Name} ---");
            AppendLog(kampfText);

            // UI nach dem Kampf aktualisieren
            RefreshUI();

            // Hinweis-Popup anzeigen, falls der Spieler gestorben ist
            if (kampfText.Contains("Du wurdest besiegt."))
            {
                MessageBox.Show(
                    "Der Charakter ist gestorben. Coins und Inventar wurden gelöscht.\n" +
                    "Ein Neustart erfolgt mit vollen HP.",
                    "Niederlage",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Verwendet das aktuell im Inventar ausgewählte Item.
        /// HealPotion wirkt auf den Charakter, PoisonPotion auf den ausgewählten Gegner.
        /// Bei Potions werden nur Fehler per Popup angezeigt, Erfolge nur im Log.
        /// </summary>
        private void UsePotionButton_Click(object sender, RoutedEventArgs e)
        {
            if (InventoryList.SelectedItem is not IInventarItem selectedItem)
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
            AppendLog($"[Item] {result}");

            // UI nach Item-Nutzung aktualisieren
            RefreshUI();

            // Bei bestimmten Fällen (fehlgeschlagen oder Nicht-Trank) Popup anzeigen
            bool isPotion = selectedItem is HealPotion || selectedItem is PoisonPotion;
            bool isErrorResult = result.Contains("kein") || result.Contains("nicht") ||
                                  result.Contains("konnte") || result.Contains("hatte keinen Effekt");
            if (!isPotion || isErrorResult)
            {
                MessageBox.Show(
                    result,
                    "Item verwendet",
                    MessageBoxButton.OK,
                    isErrorResult ? MessageBoxImage.Warning : MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Öffnet das Admin-Fenster (nur für Admin-Spieler sichtbar).
        /// Während das Admin-Fenster geöffnet ist, wird die HP-Regeneration pausiert.
        /// </summary>
        private void AdminMenuButton_Click(object sender, RoutedEventArgs e)
        {
            // Sicherheitscheck (sollte durch Sichtbarkeit bereits sichergestellt sein)
            if (!_engine.Player.IsAdmin)
            {
                MessageBox.Show(
                    "Der aktuelle Charakter verfügt nicht über Admin-Rechte.",
                    "Kein Admin",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // HP-Regeneration pausieren, solange Admin-Fenster offen ist
            _hpRegenTimer?.Stop();

            try
            {
                // Admin-Fenster öffnen (Modal)
                var adminWindow = new AdminWindow(_engine) { Owner = this };
                adminWindow.ShowDialog();
            }
            finally
            {
                // Nach Schließen des Admin-Fensters HP-Regeneration fortsetzen
                _hpRegenTimer?.Start();
            }

            // Haupt-UI nach Admin-Änderungen aktualisieren
            RefreshUI();
        }

        /// <summary>
        /// Beendet die Anwendung.
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
            if (success)
            {
                // Angriff und Coins haben sich geändert
                SyncCharacterToUi();
                SyncInventoryToUi();
                SyncMetaToUi();
            }
            else
            {
                MessageBox.Show(
                    message,
                    "Shop – Attack Upgrade",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Kauft ein Max-Health-Upgrade (+20 MaxHP) für 15 Coins.
        /// </summary>
        private void BuyHealthButton_Click(object sender, RoutedEventArgs e)
        {
            const int cost = 15;
            const double hpIncrease = 20.0;
            bool success = _engine.TryBuyMaxHealthUpgrade(cost, hpIncrease, out string message);

            AppendLog("[Shop] " + message);
            if (success)
            {
                // MaxHP und aktuelle HP haben sich geändert
                SyncCharacterToUi();
                SyncInventoryToUi();
                SyncMetaToUi();
            }
            else
            {
                MessageBox.Show(
                    message,
                    "Shop – Max Health Upgrade",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Kauft eine Heal Potion Stufe 1 (für 12 Coins) und fügt sie dem Inventar hinzu.
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
            else
            {
                MessageBox.Show(
                    message,
                    "Shop – Heal Potion I",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Kauft eine Heal Potion Stufe 2 (für 20 Coins) und fügt sie dem Inventar hinzu.
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
            else
            {
                MessageBox.Show(
                    message,
                    "Shop – Heal Potion II",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Kauft eine Poison Potion Stufe 1 (für 10 Coins) und fügt sie dem Inventar hinzu.
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
            else
            {
                MessageBox.Show(
                    message,
                    "Shop – Poison Potion I",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Kauft eine Poison Potion Stufe 2 (für 18 Coins) und fügt sie dem Inventar hinzu.
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
            else
            {
                MessageBox.Show(
                    message,
                    "Shop – Poison Potion II",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }
    }
}
