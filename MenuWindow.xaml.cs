#nullable enable
using System;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;

namespace MascotApp
{
    public partial class MenuWindow : Window
    {
        private MainWindow? _mainWindow;

        public MenuWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
        }

        private void PlayModeButton_Click(object sender, RoutedEventArgs e)
        {
            GameModesPanel.Visibility = GameModesPanel.Visibility == Visibility.Visible 
                ? Visibility.Collapsed 
                : Visibility.Visible;
            ActionPanel.Visibility = Visibility.Collapsed;
        }

        private void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            ActionPanel.Visibility = ActionPanel.Visibility == Visibility.Visible
                ? Visibility.Collapsed
                : Visibility.Visible;
            GameModesPanel.Visibility = Visibility.Collapsed;
        }

        private void JumpModeButton_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow?.EnterJumpMode();
            this.Close();
        }

        private void TagModeButton_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow?.TagModeButton_Click(sender, e); // MainWindowのメソッドを呼び出す
            this.Close();
        }

        private void ChaseModeButton_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow?.ChaseModeButton_Click(sender, e); // MainWindowのメソッドを呼び出す
            this.Close();
        }

        private void WalkModeButton_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow?.WalkModeButton_Click(sender, e); // MainWindowのメソッドを呼び出す
            this.Close();
        }

        private void MoleGameButton_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow?.MoleGameButton_Click(sender, e); // MainWindowのメソッドを呼び出す
            this.Close();
        }

        private void StretchButton_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow?.DoIdleAnimation(0);
            this.Close();
        }

        private void YawnButton_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow?.DoIdleAnimation(1);
            this.Close();
        }

        private void LookAroundButton_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow?.DoIdleAnimation(2);
            this.Close();
        }

        private void SitButton_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow?.DoIdleAnimation(3);
            this.Close();
        }

        private void LieDownButton_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow?.DoIdleAnimation(4);
            this.Close();
        }

        private void MonologueButton_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow?.DoMonologue();
            this.Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
