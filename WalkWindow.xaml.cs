#nullable enable
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Threading.Tasks;
using System.Drawing; // スクリーンショット用
using System.Drawing.Imaging; // スクリーンショット用
using System.IO; // メモリストリーム用
using System.Windows.Media; // ImageBrush用
using Newtonsoft.Json.Linq; // 追加

namespace MascotApp
{
    public partial class WalkWindow : Window
    {
        private DispatcherTimer? walkTimer;
        private double mascotX;
        private double mascotY;
        private double mascotVX;
        private double mascotVY;
        private double mascotWidth = 100;
        private double mascotHeight = 100;
        private double screenW;
        private double screenH;
        private Random random = new Random();
        private int actionCounter = 0;
        private string currentMascotName = "Mascot"; // 追加

        public event Action? OnExitWalkMode;

        public WalkWindow()
        {
            InitializeComponent();
            Loaded += WalkWindow_Loaded;
            LoadCurrentMascotName(); // 追加
            SetMascotImageSource(); // 追加
        }

        private void LoadCurrentMascotName() // 追加
        {
            try
            {
                string settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
                if (File.Exists(settingsPath))
                {
                    string json = File.ReadAllText(settingsPath);
                    JObject settings = JObject.Parse(json);
                    currentMascotName = settings["current_mascot"]?.ToString() ?? "Mascot";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"設定ファイルの読み込みに失敗しました: {ex.Message}");
                currentMascotName = "Mascot"; // エラー時はデフォルトに設定
            }
        }

        private void SetMascotImageSource() // 追加
        {
            string imagePath = $"/mascot_image_priset/{currentMascotName}/{currentMascotName}.png";
            MascotImage.Source = new BitmapImage(new Uri(imagePath, UriKind.RelativeOrAbsolute));
        }

        private void WalkWindow_Loaded(object sender, RoutedEventArgs e)
        {
            screenW = SystemParameters.PrimaryScreenWidth;
            screenH = SystemParameters.PrimaryScreenHeight;

            // ウィンドウを画面全体に広げ、背景をスクリーンショットに設定
            this.Width = screenW;
            this.Height = screenH;
            this.Left = 0;
            this.Top = 0;
            SetDesktopScreenshotAsBackground();

            // 初期位置をランダムに設定
            mascotX = random.NextDouble() * (screenW - mascotWidth);
            mascotY = random.NextDouble() * (screenH - mascotHeight);
            UpdateMascotPosition();

            // 初期速度をランダムに設定
            mascotVX = (random.NextDouble() - 0.5) * 5; // -2.5 から 2.5
            mascotVY = (random.NextDouble() - 0.5) * 5; // -2.5 から 2.5

            walkTimer = new DispatcherTimer();
            walkTimer.Interval = TimeSpan.FromMilliseconds(50);
            walkTimer.Tick += WalkTimer_Tick;
            walkTimer.Start();
        }

        private void SetDesktopScreenshotAsBackground()
        {
            try
            {
                // プライマリモニタのスクリーンショットを撮る
                using (Bitmap bmp = new Bitmap((int)screenW, (int)screenH))
                {
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        g.CopyFromScreen(0, 0, 0, 0, bmp.Size);
                    }

                    // BitmapをBitmapImageに変換して背景に設定
                    using (MemoryStream ms = new MemoryStream())
                    {
                        bmp.Save(ms, ImageFormat.Png);
                        ms.Position = 0;
                        BitmapImage bi = new BitmapImage();
                        bi.BeginInit();
                        bi.StreamSource = ms;
                        bi.CacheOption = BitmapCacheOption.OnLoad;
                        bi.EndInit();
                        bi.Freeze(); // UIスレッド以外からもアクセスできるようにフリーズ

                        ImageBrush imageBrush = new ImageBrush(bi);
                        GameCanvas.Background = imageBrush;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"デスクトップの取得に失敗しました: {ex.Message}", "エラー");
                // エラー時はデフォルトの背景色に戻す
                GameCanvas.Background = new SolidColorBrush(System.Windows.Media.Colors.SkyBlue);
            }
        }

        private void WalkTimer_Tick(object? sender, EventArgs e)
        {
            // マウス座標取得 (WPFのグローバル座標)
            System.Windows.Point mousePos = Mouse.GetPosition(this);
            System.Windows.Point screenMousePos = PointToScreen(mousePos);
            double mouseX = screenMousePos.X;
            double mouseY = screenMousePos.Y;

            // キャラ中心座標
            double mascotCenterX = mascotX + mascotWidth / 2;
            double mascotCenterY = mascotY + mascotHeight / 2;

            // マウスへのベクトル
            double dx = mouseX - mascotCenterX;
            double dy = mouseY - mascotCenterY;
            double dist = Math.Sqrt(dx * dx + dy * dy);

            // マウスに近づく速度
            double followSpeed = 5.0; // 追従速度
            if (dist > 10) // マウスに近づきすぎないように閾値を設ける
            {
                double angle = Math.Atan2(dy, dx);
                mascotVX = Math.Cos(angle) * followSpeed;
                mascotVY = Math.Sin(angle) * followSpeed;
            }
            else
            {
                // マウスに十分近い場合は停止または減速
                mascotVX *= 0.8; // 減速
                mascotVY *= 0.8; // 減速
            }

            mascotX += mascotVX;
            mascotY += mascotVY;

            // 画面端での反射 (追従モードではあまり意味がないかもしれませんが、念のため残します)
            if (mascotX < 0) { mascotX = 0; mascotVX = Math.Abs(mascotVX); }
            if (mascotX > screenW - mascotWidth) { mascotX = screenW - mascotWidth; mascotVX = -Math.Abs(mascotVX); }
            if (mascotY < 0) { mascotY = 0; mascotVY = Math.Abs(mascotVY); }
            if (mascotY > screenH - mascotHeight) { mascotY = screenH - mascotHeight; mascotVY = -Math.Abs(mascotVY); }

            // 一定時間ごとにランダムな行動 (どじな行動は残します)
            actionCounter++;
            if (actionCounter > 100) // 約5秒ごと
            {
                actionCounter = 0;
                if (random.Next(0, 10) < 3) // 30%の確率でどじな行動
                {
                    DoClumsyAction();
                }
            }

            UpdateMascotPosition();
        }

        private async void DoClumsyAction()
        {
            walkTimer!.Stop();
            MessageText.Text = "あっ、押しちゃった！";
            MessageText.Visibility = Visibility.Visible;

            // 少しアイコンをクリックするようなアニメーション
            var originalX = mascotX;
            var originalY = mascotY;
            // スクリーンショット上のランダムな位置に移動する
            var targetX = random.NextDouble() * (screenW - mascotWidth);
            var targetY = random.NextDouble() * (screenH - mascotHeight);

            // アイコンに近づく
            for (int i = 0; i < 10; i++)
            {
                mascotX += (targetX - originalX) / 10;
                mascotY += (targetY - originalY) / 10;
                UpdateMascotPosition();
                await Task.Delay(20);
            }

            // クリックするような動き
            MascotImage.RenderTransform = new ScaleTransform(0.9, 0.9, 0.5, 0.5);
            await Task.Delay(100);
            MascotImage.RenderTransform = new ScaleTransform(1.0, 1.0, 0.5, 0.5);

            await Task.Delay(1500); // メッセージ表示
            MessageText.Visibility = Visibility.Collapsed;
            walkTimer.Start();
        }

        private void UpdateMascotPosition()
        {
            Canvas.SetLeft(MascotImage, mascotX);
            Canvas.SetTop(MascotImage, mascotY);
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            walkTimer?.Stop();
            this.Close();
            OnExitWalkMode?.Invoke();
        }
    }
}
