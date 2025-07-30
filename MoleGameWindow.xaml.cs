#nullable enable
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Threading.Tasks;

namespace MascotApp
{
    public partial class MoleGameWindow : Window
    {
        private DispatcherTimer? gameTimer;
        private DispatcherTimer? moleTimer;
        private int score = 0;
        private int timeLeft = 30;
        private Random random = new Random();

        public event Action? OnExitMoleGameMode;

        public MoleGameWindow()
        {
            InitializeComponent();
            Loaded += MoleGameWindow_Loaded;
        }

        private void MoleGameWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // ゲームタイマー設定
            gameTimer = new DispatcherTimer();
            gameTimer.Interval = TimeSpan.FromSeconds(1);
            gameTimer.Tick += GameTimer_Tick;
            gameTimer.Start();

            // モグラ出現タイマー設定
            moleTimer = new DispatcherTimer();
            moleTimer.Interval = TimeSpan.FromMilliseconds(random.Next(500, 1500));
            moleTimer.Tick += MoleTimer_Tick;
            moleTimer.Start();

            UpdateScoreAndTime();
        }

        private void GameTimer_Tick(object? sender, EventArgs e)
        {
            timeLeft--;
            UpdateScoreAndTime();

            if (timeLeft <= 0)
            {
                EndGame();
            }
        }

        private void MoleTimer_Tick(object? sender, EventArgs e)
        {
            // モグラを非表示にしてから新しい位置に表示
            MascotMole.Visibility = Visibility.Collapsed;

            double x = random.NextDouble() * (GameCanvas.ActualWidth - MascotMole.Width);
            double y = random.NextDouble() * (GameCanvas.ActualHeight - MascotMole.Height);

            Canvas.SetLeft(MascotMole, x);
            Canvas.SetTop(MascotMole, y);
            MascotMole.Visibility = Visibility.Visible;

            // 次の出現までの時間をランダムに設定
            moleTimer!.Interval = TimeSpan.FromMilliseconds(random.Next(500, 1500));
        }

        private void MascotMole_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            score++;
            UpdateScoreAndTime();
            MascotMole.Visibility = Visibility.Collapsed; // 叩いたら消える
        }

        private void UpdateScoreAndTime()
        {
            ScoreText.Text = $"スコア: {score}";
            TimeText.Text = $"時間: {timeLeft}";
        }

        private void EndGame()
        {
            gameTimer?.Stop();
            moleTimer?.Stop();
            MascotMole.Visibility = Visibility.Collapsed;

            System.Windows.MessageBox.Show($"ゲーム終了！あなたのスコアは {score} です！", "もぐらたたき", MessageBoxButton.OK, MessageBoxImage.Information);
            this.Close();
            OnExitMoleGameMode?.Invoke();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            gameTimer?.Stop();
            moleTimer?.Stop();
            this.Close();
            OnExitMoleGameMode?.Invoke();
        }
    }
}
