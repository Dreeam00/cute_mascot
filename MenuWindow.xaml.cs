#nullable enable
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Linq;

namespace MascotApp
{
    public partial class MenuWindow : Window
    {
        private MainWindow? _mainWindow;

        public MenuWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            LoadMascotSelection();
        }

        private void LoadMascotSelection()
        {
            try
            {
                string settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
                if (File.Exists(settingsPath))
                {
                    string json = File.ReadAllText(settingsPath);
                    dynamic? settings = JsonConvert.DeserializeObject(json);
                    string currentMascot = settings?.current_mascot?.ToString() ?? "Mascot";
                    if (settings != null && settings.current_mascot != null)
                    {
                        currentMascot = settings.current_mascot.ToString();
                    }

                    // イベントハンドラを一時的に解除して、不要な再起動を防ぐ
                    if (MascotSelectionComboBox != null)
                    {
                        MascotSelectionComboBox.SelectionChanged -= MascotSelectionComboBox_SelectionChanged;

                        foreach (ComboBoxItem item in MascotSelectionComboBox.Items.OfType<ComboBoxItem>())
                        {
                            if (item.Content?.ToString() == currentMascot)
                            {
                                item.IsSelected = true;
                                break;
                            }
                        }

                        MascotSelectionComboBox.SelectionChanged += MascotSelectionComboBox_SelectionChanged;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"設定の読み込み中にエラーが発生しました: {ex.Message}");
            }
        }

        private void MascotSelectionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MascotSelectionComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string selectedMascot = selectedItem.Content?.ToString() ?? string.Empty;
                try
                {
                    string settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
                    string json = File.ReadAllText(settingsPath);
                    dynamic? settings = JsonConvert.DeserializeObject(json);
                    if (settings == null)
                    {
                        settings = new System.Dynamic.ExpandoObject();
                    }
                    // Ensure settings is not null before using it
                    dynamic nonNullableSettings = settings!; // Use null-forgiving operator as we know it's not null here
                    nonNullableSettings.current_mascot = selectedMascot;
                    string newJson = JsonConvert.SerializeObject(settings, Formatting.Indented);
                    File.WriteAllText(settingsPath, newJson);

                    // アプリケーションを再起動して変更を適用
                    System.Windows.Forms.Application.Restart();
                    Application.Current.Shutdown();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"設定の保存中にエラーが発生しました: {ex.Message}");
                }
            }
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
