#nullable enable
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MascotApp
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer? autoHideTimer;
        private DispatcherTimer? idleAnimationTimer;
        private DispatcherTimer? monologueTimer;
        private DispatcherTimer? moodTimer;
        private MascotJumpWindow? mascotJumpWindow;
        private Random random = new Random();
        private bool isDraggingMascot = false;
        private string currentMascot = "Lumina"; // デフォルトのマスコット

        // なでなでエフェクト用
        private double lastMouseX = -1;
        private int pettingSequenceCount = 0; // 0:初期状態, 1:右, 2:右左, 3:右左右, 4:右左右左
        private const int PETTING_THRESHOLD = 10; // マウス移動の閾値

        // 気分システム用
        private int currentMood = 50; // 0-100で気分を表現 (50が普通)
        private const int MOOD_MAX = 100;
        private const int MOOD_MIN = 0;

        public MainWindow()
        {
            InitializeComponent();
            LoadMascotSettings();
            Messages.LoadMessages(currentMascot);
            InitializeTimers();
            SetupEventHandlers();
            SetupButtonTexts();
            HideBubbles();
        }

        private void LoadMascotSettings()
        {
            try
            {
                string settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
                if (File.Exists(settingsPath))
                {
                    string json = File.ReadAllText(settingsPath);
                    dynamic? settings = JsonConvert.DeserializeObject(json);
                    if (settings != null && settings.current_mascot != null)
                    {
                        currentMascot = settings.current_mascot?.ToString() ?? "Mascot";
                    }
                    // キャラクター名の先頭を大文字に変換
                    if (!string.IsNullOrEmpty(currentMascot))
                    {
                        currentMascot = char.ToUpper(currentMascot[0]) + currentMascot.Substring(1);
                    }
                }
            }
            catch (Exception ex)
            {
                // エラー処理
                MessageBox.Show($"設定ファイルの読み込みに失敗しました: {ex.Message}");
            }
        }

        private void InitializeTimers()
        {
            autoHideTimer = new DispatcherTimer();
            autoHideTimer.Interval = TimeSpan.FromSeconds(10); // メッセージ表示時間を10秒に延長
            autoHideTimer.Tick += (s, e) =>
            {
                HideBubbles();
                autoHideTimer.Stop();
            };

            idleAnimationTimer = new DispatcherTimer();
            idleAnimationTimer.Interval = TimeSpan.FromSeconds(random.Next(10, 20)); // 10秒から20秒の間でランダム
            idleAnimationTimer.Tick += (s, e) =>
            {
                DoIdleAnimation();
                idleAnimationTimer.Interval = TimeSpan.FromSeconds(random.Next(10, 20)); // 次のインターバルを再設定
            };
            idleAnimationTimer.Start();

            monologueTimer = new DispatcherTimer();
            monologueTimer.Interval = TimeSpan.FromSeconds(random.Next(20, 60)); // 20秒から60秒の間でランダム
            monologueTimer.Tick += (s, e) =>
            {
                DoMonologue();
                monologueTimer.Interval = TimeSpan.FromSeconds(random.Next(20, 60)); // 次のインターバルを再設定
            };
            monologueTimer.Start();

            moodTimer = new DispatcherTimer();
            moodTimer.Interval = TimeSpan.FromSeconds(30); // 30秒ごとに気分をチェック
            moodTimer.Tick += (s, e) =>
            {
                DecreaseMood(5); // 30秒ごとに気分を5減少
            };
            moodTimer.Start();
        }

        private void SetupEventHandlers()
        {
            // 右クリックでコンテキストメニュー表示
            this.MouseRightButtonDown += (s, e) =>
            {
                ContextMenuPopup.IsOpen = true;
                e.Handled = true;
            };
            // ウィンドウ外クリックでコンテキストメニューを閉じる
            this.MouseLeftButtonDown += (s, e) =>
            {
                if (ContextMenuPopup.IsOpen)
                {
                    ContextMenuPopup.IsOpen = false;
                    e.Handled = true;
                }
            };

            // マスコットのドラッグとクリックイベント
            CharacterImage.MouseLeftButtonDown += CharacterImage_MouseLeftButtonDown;
            CharacterImage.MouseMove += CharacterImage_MouseMove;
            CharacterImage.MouseLeftButtonUp += CharacterImage_MouseLeftButtonUp;
        }

        private void OpenMenuButton_Click(object sender, RoutedEventArgs e)
        {
            MenuWindow menuWindow = new MenuWindow(this);
            menuWindow.Owner = this;
            menuWindow.Left = this.Left + this.Width;
            menuWindow.Top = this.Top;
            menuWindow.Show();
            ContextMenuPopup.IsOpen = false; // メニューを開いたらコンテキストメニューを閉じる
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // ウィンドウをドラッグで移動
            if (e.OriginalSource == this || e.OriginalSource == CharacterImage)
            {
                this.DragMove();
            }
        }

        private void CharacterImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isDraggingMascot = true;
            CharacterImage.CaptureMouse();
            AnimateCharacter(); // クリック時のアニメーション
            SetMascotImage("tickle"); // なでなでされた表情
            idleAnimationTimer?.Stop(); // アイドルアニメーションを一時停止
            IncreaseMood(10); // クリックで気分上昇
            e.Handled = true;
        }

        private void CharacterImage_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDraggingMascot)
            {
                SetMascotImage("happy"); // ドラッグ中は嬉しい表情
                // ウィンドウをマウスの位置に移動
                this.Left = e.GetPosition(this.Parent as UIElement).X - (CharacterImage.ActualWidth / 2);
                this.Top = e.GetPosition(this.Parent as UIElement).Y - (CharacterImage.ActualHeight / 2);
            }

            // なでなでエフェクトの検出
            if (e.LeftButton == MouseButtonState.Pressed && !isDraggingMascot) // 左クリック中でドラッグではない場合
            {
                double currentMouseX = e.GetPosition(CharacterImage).X;

                if (lastMouseX == -1)
                {
                    lastMouseX = currentMouseX;
                    return;
                }

                double deltaX = currentMouseX - lastMouseX;

                if (Math.Abs(deltaX) > PETTING_THRESHOLD)
                {
                    if (pettingSequenceCount == 0 && deltaX > 0) // 右に移動 (R)
                    {
                        pettingSequenceCount = 1;
                    }
                    else if (pettingSequenceCount == 1 && deltaX < 0) // 左に移動 (RL)
                    {
                        pettingSequenceCount = 2;
                    }
                    else if (pettingSequenceCount == 2 && deltaX > 0) // 右に移動 (RLR)
                    {
                        pettingSequenceCount = 3;
                    }
                    else if (pettingSequenceCount == 3 && deltaX < 0) // 左に移動 (RLRL)
                    {
                        pettingSequenceCount = 4;
                        DoPettingEffect(); // なでなでエフェクト発動！
                        pettingSequenceCount = 0; // リセット
                    }
                    else
                    {
                        pettingSequenceCount = 0; // パターンが崩れたらリセット
                    }
                    lastMouseX = currentMouseX;
                }
            }
            else
            {
                // マウスボタンが離れたらリセット
                pettingSequenceCount = 0;
                lastMouseX = -1;
            }
        }

        private void CharacterImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isDraggingMascot)
            {
                isDraggingMascot = false;
                CharacterImage.ReleaseMouseCapture();
                SetMascotImage("default"); // 元の画像に戻す
                idleAnimationTimer?.Start(); // アイドルアニメーションを再開
            }
        }

        private void SetupButtonTexts()
        {
            // 1行目のボタン
            GreetingButton.Content = Messages.Prompts.Greeting;
            WeatherButton.Content = Messages.Prompts.Weather;
            TimeButton.Content = Messages.Prompts.Time;
            JokeButton.Content = Messages.Prompts.Joke;
            GoodbyeButton.Content = Messages.Prompts.Goodbye;
            
            // 2行目のボタン
            HowAreYouButton.Content = Messages.Prompts.HowAreYou;
            ComplimentButton.Content = Messages.Prompts.Compliment;
            MotivationButton.Content = Messages.Prompts.Motivation;
            AdviceButton.Content = Messages.Prompts.Advice;
            StoryButton.Content = Messages.Prompts.Story;
            
            // 3行目のボタン
            FoodButton.Content = Messages.Prompts.Food;
            MusicButton.Content = Messages.Prompts.Music;
            StudyButton.Content = Messages.Prompts.Study;
            SleepButton.Content = Messages.Prompts.Sleep;
            ThanksButton.Content = Messages.Prompts.Thanks;
        }

        private void ShowUserBubble(string userText)
        {
            UserText.Text = userText;
            UserBubble.Visibility = Visibility.Visible;
            var animation = new DoubleAnimation { From = 0, To = 1, Duration = TimeSpan.FromMilliseconds(200) };
            UserBubble.BeginAnimation(OpacityProperty, animation);
        }

        private void ShowMascotBubble(string mascotText)
        {
            MascotText.Text = mascotText;
            MascotBubble.Visibility = Visibility.Visible;
            var animation = new DoubleAnimation { From = 0, To = 1, Duration = TimeSpan.FromMilliseconds(200) };
            MascotBubble.BeginAnimation(OpacityProperty, animation);
        }

        private void HideBubbles()
        {
            UserBubble.Visibility = Visibility.Collapsed;
            MascotBubble.Visibility = Visibility.Collapsed;
            SetMascotImage(GetMascotImageBasedOnMood()); // 吹き出しが消えたら気分に応じた表情に戻す
        }

        private void AnimateCharacter()
        {
            var scaleAnimation = new DoubleAnimation
            {
                From = 1.0,
                To = 1.1,
                Duration = TimeSpan.FromMilliseconds(100),
                AutoReverse = true
            };
            CharacterScale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
            CharacterScale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
        }

        public async void DoIdleAnimation(int? animationType = null)
        {
            // アイドルアニメーション中は会話バブルを非表示にする
            HideBubbles();

            int type = animationType ?? random.Next(0, 5); // 引数がなければランダム

            switch (type)
            {
                case 0: // 伸び
                    SetMascotImage("stretch_start");
                    await Task.Delay(500);
                    SetMascotImage("stretch_end");
                    await Task.Delay(500);
                    break;
                case 1: // あくび
                    SetMascotImage("yawn");
                    await Task.Delay(1000);
                    break;
                case 2: // きょろきょろ
                    SetMascotImage("look_left");
                    await Task.Delay(500);
                    SetMascotImage("look_right");
                    await Task.Delay(500);
                    SetMascotImage("look_up");
                    await Task.Delay(500);
                    break;
                case 3: // 座る
                    SetMascotImage("sit");
                    await Task.Delay(2000); // 少し長めに座る
                    break;
                case 4: // 寝転がる
                    SetMascotImage("lie_down");
                    await Task.Delay(3000); // 少し長めに寝転がる
                    break;
            }
            // 元の画像に戻す
            SetMascotImage("default");
        }

        private async void DoPettingEffect()
        {
            // なでなでエフェクト中はアイドルアニメーションを停止
            idleAnimationTimer?.Stop();

            // なでなでされた時の特別なアニメーションや表情
            SetMascotImage("happy"); // なでなでされた時の特別な表情
            ShowMascotBubble("なでなで、きもちいい〜！");
            IncreaseMood(20); // なでなでで気分上昇

            // 少しの間、なでなでされた状態を維持
            await Task.Delay(2000);

            HideBubbles();
            SetMascotImage("default"); // 元の画像に戻す
            idleAnimationTimer?.Start(); // アイドルアニメーションを再開
        }

        public async void DoMonologue()
        {
            // 会話バブルが表示中の場合は独り言を言わない
            if (UserBubble.Visibility == Visibility.Visible || MascotBubble.Visibility == Visibility.Visible)
            {
                return;
            }

            var monologueEntry = Messages.GetRandomMonologue();
            string monologue = monologueEntry.Text;
            string imageState = monologueEntry.Image;

            SetMascotImage(imageState);

            ShowMascotBubble(monologue);

            // 独り言の表示時間
            await Task.Delay(TimeSpan.FromSeconds(random.Next(3, 6)));
            HideBubbles();
            SetMascotImage("default"); // 独り言終了後、元の画像に戻す
        }

        private void IncreaseMood(int amount)
        {
            currentMood = Math.Min(MOOD_MAX, currentMood + amount);
            UpdateMascotMood();
        }

        private void DecreaseMood(int amount)
        {
            currentMood = Math.Max(MOOD_MIN, currentMood - amount);
            UpdateMascotMood();
        }

        private void UpdateMascotMood()
        {
            // 気分に応じてアイドルアニメーションの頻度などを調整することも可能
            // 例: idleAnimationTimer.Interval = TimeSpan.FromSeconds(Math.Max(2, 10 - currentMood / 10));
        }

        private string GetMascotImageBasedOnMood()
        {
            if (currentMood > 90)
            {
                return "love"; // Love
            }
            else if (currentMood > 70)
            {
                return "happy"; // ごきげん
            }
            else if (currentMood < 10)
            {
                return "angry"; // Angry
            }
            else if (currentMood < 30)
            {
                return "sad"; // しょんぼり
            }
            else
            {
                return "default"; // 普通
            }
        }

        private void SetMascotImage(string state)
        {
            string imageName = ""; // 初期化
            string mascotPrefix = ""; // 初期化

            // マスコットごとのファイル名プレフィックスを決定
            if (currentMascot == "Lumina")
            {
                mascotPrefix = "Lumina";
            }
            else if (currentMascot == "Planet")
            {
                mascotPrefix = "planet"; // Planetはplanet_xxx.png
            }
            else // Mascot (デフォルト)
            {
                mascotPrefix = "mascot"; // Mascotはmascot_xxx.png
            }

            if (state == "default")
            {
                // デフォルト状態の場合、画像名はプレフィックスのみ (例: mascot.png, lumina.png, planet.png)
                imageName = $"{mascotPrefix}.png";
            }
            else
            {
                // その他の状態の場合、プレフィックスと状態を組み合わせる
                imageName = $"{mascotPrefix}_{state}.png";
            }

            string uri = $"pack://application:,,,/mascot_image_priset/{currentMascot}/{imageName}";

            try
            {
                CharacterImage.Source = new BitmapImage(new Uri(uri));
            }
            catch
            {
                // デフォルト画像を試す
                try
                {
                    string fallbackUri = $"pack://application:,,,/mascot_image_priset/{currentMascot}/{mascotPrefix}.png";
                    CharacterImage.Source = new BitmapImage(new Uri(fallbackUri));
                }
                catch
                {
                    CharacterImage.Source = null;
                }
            }
        }

        

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // MenuWindowから呼び出されるメソッド
        public void EnterJumpMode()
        {
            // メインウィンドウを隠す
            this.Hide();
            mascotJumpWindow = new MascotJumpWindow();
            mascotJumpWindow.OnExitJumpMode += () =>
            {
                this.Show();
            };
            mascotJumpWindow.Show();
        }

        public void TagModeButton_Click(object sender, RoutedEventArgs e)
        {
            // メインウィンドウを隠す
            this.Hide();
            // ユーザーが「追いかけられる」モードなので、マスコットは追いかける (isChaser = true)
            mascotJumpWindow = new MascotJumpWindow(true, true);
            mascotJumpWindow.OnExitJumpMode += () =>
            {
                this.Show();
            };
            mascotJumpWindow.Show();
        }

        public void ChaseModeButton_Click(object sender, RoutedEventArgs e)
        {
            // メインウィンドウを隠す
            this.Hide();
            // ユーザーが「追いかける」モードなので、マスコットは逃げる (isChaser = false)
            mascotJumpWindow = new MascotJumpWindow(true, false);
            mascotJumpWindow.OnExitJumpMode += () =>
            {
                this.Show();
            };
            mascotJumpWindow.Show();
        }

        public void WalkModeButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            WalkWindow walkWindow = new WalkWindow();
            walkWindow.OnExitWalkMode += () =>
            {
                this.Show();
            };
            walkWindow.Show();
        }

        public void MoleGameButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            MoleGameWindow moleGameWindow = new MoleGameWindow();
            moleGameWindow.OnExitMoleGameMode += () =>
            {
                this.Show();
            };
            moleGameWindow.Show();
        }

        // アクションボタンのイベントハンドラ
        private void HandleConversation(string userText, string responseCategory, string imageState)
        {
            ShowUserBubble(userText);
            string mascotResponse = Messages.GetRandomMessage(responseCategory);
            ShowMascotBubble(mascotResponse);
            SetMascotImage(imageState);
            autoHideTimer?.Start();
        }

        private void GreetingButton_Click(object sender, RoutedEventArgs e)
        {
            HandleConversation(Messages.Prompts.Greeting, "Greetings", "happy");
        }

        private void WeatherButton_Click(object sender, RoutedEventArgs e)
        {
            HandleConversation(Messages.Prompts.Weather, "Weather", "thoughtful");
        }

        private void TimeButton_Click(object sender, RoutedEventArgs e)
        {
            ShowUserBubble(Messages.Prompts.Time);
            string mascotResponse = Messages.GetTimeMessage();
            ShowMascotBubble(mascotResponse);
            SetMascotImage("look_up");
            autoHideTimer?.Start();
        }

        private void JokeButton_Click(object sender, RoutedEventArgs e)
        {
            HandleConversation(Messages.Prompts.Joke, "Jokes", "happy");
        }

        private void GoodbyeButton_Click(object sender, RoutedEventArgs e)
        {
            HandleConversation(Messages.Prompts.Goodbye, "Goodbyes", "sad");
        }

        private void HowAreYouButton_Click(object sender, RoutedEventArgs e)
        {
            HandleConversation(Messages.Prompts.HowAreYou, "HowAreYou", "happy");
        }

        private void ComplimentButton_Click(object sender, RoutedEventArgs e)
        {
            HandleConversation(Messages.Prompts.Compliment, "Compliments", "love");
        }

        private void MotivationButton_Click(object sender, RoutedEventArgs e)
        {
            HandleConversation(Messages.Prompts.Motivation, "Motivation", "happy");
        }

        private void AdviceButton_Click(object sender, RoutedEventArgs e)
        {
            HandleConversation(Messages.Prompts.Advice, "Advice", "thoughtful");
        }

        private void StoryButton_Click(object sender, RoutedEventArgs e)
        {
            HandleConversation(Messages.Prompts.Story, "Stories", "happy");
        }

        private void FoodButton_Click(object sender, RoutedEventArgs e)
        {
            HandleConversation(Messages.Prompts.Food, "Food", "hungry");
        }

        private void MusicButton_Click(object sender, RoutedEventArgs e)
        {
            HandleConversation(Messages.Prompts.Music, "Music", "happy");
        }

        private void StudyButton_Click(object sender, RoutedEventArgs e)
        {
            HandleConversation(Messages.Prompts.Study, "Study", "thoughtful");
        }

        private void SleepButton_Click(object sender, RoutedEventArgs e)
        {
            HandleConversation(Messages.Prompts.Sleep, "Sleep", "sleepy");
        }

        private void ThanksButton_Click(object sender, RoutedEventArgs e)
        {
            HandleConversation(Messages.Prompts.Thanks, "Thanks", "love");
        }
    }
}