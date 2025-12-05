using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace RpgWpf
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        // ============================
        //  Properties für die Bindings
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

        private int _hp;
        public int HP
        {
            get => _hp;
            set { _hp = value; OnPropertyChanged(nameof(HP)); }
        }

        private int _maxHP;
        public int MaxHP
        {
            get => _maxHP;
            set { _maxHP = value; OnPropertyChanged(nameof(MaxHP)); }
        }

        private int _attackDamage;
        public int AttackDamage
        {
            get => _attackDamage;
            set { _attackDamage = value; OnPropertyChanged(nameof(AttackDamage)); }
        }

        private double _damageMultiplier;
        public double DamageMultiplier
        {
            get => _damageMultiplier;
            set { _damageMultiplier = value; OnPropertyChanged(nameof(DamageMultiplier)); }
        }

        private int _specialAttackDamage;
        public int SpecialAttackDamage
        {
            get => _specialAttackDamage;
            set { _specialAttackDamage = value; OnPropertyChanged(nameof(SpecialAttackDamage)); }
        }

        private double _specialProbability;
        public double SpecialProbability
        {
            get => _specialProbability;
            set { _specialProbability = value; OnPropertyChanged(nameof(SpecialProbability)); }
        }

        // ============================
        //      Konstruktor
        // ============================
        public MainWindow()
        {
            InitializeComponent();

            // WICHTIG: DataContext setzen!
            DataContext = this;

            // Testwerte setzen, damit UI etwas zeigt
            PlayerTag = "Spieler123";
            Alter = 0;

            HP = 0;
            MaxHP = 0;

            AttackDamage = 0;
            DamageMultiplier = 0;
            SpecialAttackDamage = 0;
            SpecialProbability = 0;

            // GameLog-Testtext
            GameLog.Text = "Spiel wurde gestartet...\n" +
                           "Dies ist dein zukünftiges Konsolenfeld.\n\n" +
                           "Links oben siehst du die Charakterdaten.\n" +
                           "Unten findest du die Steuerungsbuttons.\n\n" +
                           "Alles funktioniert – jetzt können wir mit dem RPG starten!";
        }

        // ============================
        //   INotifyPropertyChanged
        // ============================
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }


        // ============================
        //   BUTTON HANDLER (leer)
        // ============================

        private void EnterButton_Click(object sender, RoutedEventArgs e)
        {
            GameLog.AppendText("> Eingabe: " + InputBox.Text + "\n");
            InputBox.Clear();
            GameLog.ScrollToEnd();
        }

        private void InventarButton_Click(object sender, RoutedEventArgs e)
        {
            GameLog.AppendText("[Inventar geöffnet]\n");
        }

        private void GoblinButton_Click(object sender, RoutedEventArgs e)
        {
            GameLog.AppendText("[Goblin angreifen]\n");
        }

        private void ElfeButton_Click(object sender, RoutedEventArgs e)
        {
            GameLog.AppendText("[Elfe angreifen]\n");
        }

        private void WerwolfButton_Click(object sender, RoutedEventArgs e)
        {
            GameLog.AppendText("[Werwolf angreifen]\n");
        }

        private void PotionButton_Click(object sender, RoutedEventArgs e)
        {
            GameLog.AppendText("[Potion verwenden]\n");
        }

        private void AdminButton_Click(object sender, RoutedEventArgs e)
        {
            GameLog.AppendText("[Admin-Menü aufrufen]\n");
        }

        private void BeendenButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
