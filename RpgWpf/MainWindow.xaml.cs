using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RpgWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private GameEngine _engine;
        public MainWindow()
        {
            InitializeComponent();

            _engine = new GameEngine();

            // Intro-Text ins GameLog
            GameLog.Text = _engine.GetIntroText();
        }
        private void EnterButton_Click(object sender, RoutedEventArgs e)
        {
            var command = InputBox.Text;

            if (string.IsNullOrWhiteSpace(command))
                return;

            // Befehl an Engine schicken
            string response = _engine.ProcessCommand(command);

            // In die "Konsole" schreiben
            GameLog.AppendText($"> {command}\n");
            GameLog.AppendText(response + "\n\n");

            // Scroll nach unten
            GameLog.ScrollToEnd();

            // Eingabefeld leeren
            InputBox.Clear();
            InputBox.Focus();
        }
    }
}