using System;
using System.ComponentModel;
using System.Text;
using System.Windows;
using RpgWpf.GameLogic;
using RpgWpf.GameCore;

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

        private string _playerTag;
        public string PlayerTag
        {
            get => _playerTag;
            set { _playerTag = value; OnPropertyChanged(nameof(PlayerTag)); }
        }

        private int _alter;
        public int Alter
        {
            get => _alter;
            set { _alter = value; OnPropertyChanged(nameof(Alter)); }
        }

        private double _hp;
        public double HP
        {
            get => _hp;
            set { _hp = value; OnPropertyChanged(nameof(HP)); }
        }

        private double _maxHP;
        public double MaxHP
        {
            get => _maxHP;
            set { _maxHP = value; OnPropertyChanged(nameof(MaxHP)); }
        }

        private double _attackDamage;
        public double AttackDamage
        {
            get => _attackDamage;
            set { _attackDamage = value; OnPropertyChanged(nameof(AttackDamage)); }
        }

        private int _damageMultiplier;
        public int DamageMultiplier
        {
            get => _damageMultiplier;
            set { _damageMultiplier = value; OnPropertyChanged(nameof(DamageMultiplier)); }
        }

        private double _specialAttackDamage;
        public double SpecialAttackDamage
        {
            get => _specialAttackDamage;
            set { _specialAttackDamage = value; OnPropertyChanged(nameof(SpecialAttackDamage)); }
        }

        /// <summary>
        /// 0..1 für {0:P0} in XAML (z.B. 0.2 = 20%).
        /// </summary>
        private double _specialProbability;
        public double SpecialProbability
        {
            get => _specialProbability;
            set { _specialProbability = value; OnPropertyChanged(nameof(SpecialProbability)); }
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
            // (später können wir hier Eingabedialog einbauen)
            _engine = new GameEngine(
                vorname: "Tobi",
                playerTag: "Tobender50",
                alter: 20,
                inventarGroesse: 24
            );

            // UI einmal initial mit echten Werten füllen
            SyncCharacterToUi();

            // GameLog füllen
            GameLog.Text = "Willkommen in deinem WPF-RPG!\n\n";
            GameLog.AppendText(_engine.GetStatusText() + "\n");
            GameLog.ScrollToEnd();
        }

        // ============================
        //   Hilfsmethoden (UI Sync)
        // ============================

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

        private void AppendLog(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;

            GameLog.AppendText(text);
            if (!text.EndsWith("\n"))
                GameLog.AppendText("\n");
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

        private void EnterButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(InputBox.Text))
            {
                AppendLog($"> {InputBox.Text}");
                InputBox.Clear();
            }
        }

        private void InventarButton_Click(object sender, RoutedEventArgs e)
        {
            var inv = _engine.Player.Inventar;
            var items = inv.Snapshot();

            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine($"Inventar ({inv.UsedSize}/{inv.MaxSize}):");

            if (items.Count == 0)
            {
                sb.AppendLine("  (leer)");
            }
            else
            {
                for (int i = 0; i < items.Count; i++)
                {
                    var item = items[i];
                    sb.AppendLine($"  [{i}] {item.ItemName} (Größe {item.InventarGroesse})");
                }
            }

            AppendLog(sb.ToString());
        }

        private void GoblinButton_Click(object sender, RoutedEventArgs e)
        {
            string log = _engine.AttackGoblin();
            AppendLog("\n--- Kampf gegen Goblin ---");
            AppendLog(log);
            SyncCharacterToUi();
        }

        private void ElfeButton_Click(object sender, RoutedEventArgs e)
        {
            string log = _engine.AttackElfe();
            AppendLog("\n--- Kampf gegen Elfe ---");
            AppendLog(log);
            SyncCharacterToUi();
        }

        private void WerwolfButton_Click(object sender, RoutedEventArgs e)
        {
            string log = _engine.AttackWerwolf();
            AppendLog("\n--- Kampf gegen Werwolf ---");
            AppendLog(log);
            SyncCharacterToUi();
        }

        private void PotionButton_Click(object sender, RoutedEventArgs e)
        {
            // Vorläufig nur Platzhalter – echte Potion-Logik bauen wir später ein.
            AppendLog("\n[Potion verwenden] – Logik folgt später.");
        }

        private void AdminButton_Click(object sender, RoutedEventArgs e)
        {
            // Vorläufig Platzhalter – später eigenes Admin-Fenster.
            AppendLog("\n[Admin-Menü] – noch nicht implementiert.");
        }

        private void BeendenButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
