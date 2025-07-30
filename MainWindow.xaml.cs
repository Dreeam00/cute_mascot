#nullable enable
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Threading.Tasks;

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
            InitializeTimers();
            SetupEventHandlers();
            SetupButtonTexts();
            HideBubbles();
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
            SetMascotImage("mascot_tickle.png"); // なでなでされた表情
            idleAnimationTimer?.Stop(); // アイドルアニメーションを一時停止
            IncreaseMood(10); // クリックで気分上昇
            e.Handled = true;
        }

        private void CharacterImage_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDraggingMascot)
            {
                SetMascotImage("mascot_happy.png"); // ドラッグ中は嬉しい表情
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
                SetMascotImage("mascot.png"); // 元の画像に戻す
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
                    SetMascotImage("mascot_stretch_start.png");
                    await Task.Delay(500);
                    SetMascotImage("mascot_stretch_end.png");
                    await Task.Delay(500);
                    break;
                case 1: // あくび
                    SetMascotImage("mascot_yawn.png");
                    await Task.Delay(1000);
                    break;
                case 2: // きょろきょろ
                    SetMascotImage("mascot_look_left.png");
                    await Task.Delay(500);
                    SetMascotImage("mascot_look_right.png");
                    await Task.Delay(500);
                    SetMascotImage("mascot_look_up.png");
                    await Task.Delay(500);
                    break;
                case 3: // 座る
                    SetMascotImage("mascot_sit.png");
                    await Task.Delay(2000); // 少し長めに座る
                    break;
                case 4: // 寝転がる
                    SetMascotImage("mascot_lie_down.png");
                    await Task.Delay(3000); // 少し長めに寝転がる
                    break;
            }
            // 元の画像に戻す
            SetMascotImage("mascot.png");
        }

        private async void DoPettingEffect()
        {
            // なでなでエフェクト中はアイドルアニメーションを停止
            idleAnimationTimer?.Stop();

            // なでなでされた時の特別なアニメーションや表情
            SetMascotImage("mascot_happy.png"); // なでなでされた時の特別な表情
            ShowMascotBubble("なでなで、きもちいい〜！");
            IncreaseMood(20); // なでなでで気分上昇

            // 少しの間、なでなでされた状態を維持
            await Task.Delay(2000);

            HideBubbles();
            SetMascotImage("mascot.png"); // 元の画像に戻す
            idleAnimationTimer?.Start(); // アイドルアニメーションを再開
        }

        public async void DoMonologue()
        {
            // 会話バブルが表示中の場合は独り言を言わない
            if (UserBubble.Visibility == Visibility.Visible || MascotBubble.Visibility == Visibility.Visible)
            {
                return;
            }

            string[] monologues = new string[]
            {
                "おなかすいたなぁ…",
                "ねむねむ…",
                "きょうもいちにちがんばろうね！",
                "ねぇねぇ、あそぼ！",
                "Zzz...",
                "ふぅ…",
                "なんかいいことないかなぁ…",
                "ぽかぽか…",
                "うーん…",
                "はっ！",
                "…",
                "ぴょんぴょん！"
            };

            string monologue = monologues[random.Next(monologues.Length)];

            // 独り言の内容に応じて表情を切り替える
            if (monologue.Contains("おなかすいた"))
            {
                SetMascotImage("mascot_hungry.png");
            }
            else if (monologue.Contains("ねむねむ") || monologue.Contains("Zzz"))
            {
                SetMascotImage("mascot_sleepy.png");
            }
            else if (monologue.Contains("うーん") || monologue.Contains("なんかいいことないかなぁ"))
            {
                SetMascotImage("mascot_thoughtful.png");
            }
            else
            {
                SetMascotImage("mascot.png"); // デフォルトの表情
            }

            ShowMascotBubble(monologue);

            // 独り言の表示時間
            await Task.Delay(TimeSpan.FromSeconds(random.Next(3, 6)));
            HideBubbles();
            SetMascotImage("mascot.png"); // 独り言終了後、元の画像に戻す
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
                return "mascot_love.png"; // Love
            }
            else if (currentMood > 70)
            {
                return "mascot_happy.png"; // ごきげん
            }
            else if (currentMood < 10)
            {
                return "mascot_angry.png"; // Angry
            }
            else if (currentMood < 30)
            {
                return "mascot_sad.png"; // しょんぼり
            }
            else
            {
                return "mascot.png"; // 普通
            }
        }

        private void SetMascotImage(string imageName)
        {
            CharacterImage.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/{imageName}"));
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
        private void HandleConversation(string userText, string[] responses, string imageName)
        {
            ShowUserBubble(userText);
            string mascotResponse = Messages.GetRandomMessage(responses);
            ShowMascotBubble(mascotResponse);
            SetMascotImage(imageName);
            autoHideTimer?.Start();
        }

        private void GreetingButton_Click(object sender, RoutedEventArgs e)
        {
            HandleConversation(Messages.Prompts.Greeting, Messages.Responses.Greetings, "mascot_happy.png");
        }

        private void WeatherButton_Click(object sender, RoutedEventArgs e)
        {
            HandleConversation(Messages.Prompts.Weather, Messages.Responses.Weather, "mascot_thoughtful.png");
        }

        private void TimeButton_Click(object sender, RoutedEventArgs e)
        {
            ShowUserBubble(Messages.Prompts.Time);
            string mascotResponse = Messages.GetTimeMessage();
            ShowMascotBubble(mascotResponse);
            SetMascotImage("mascot_look_up.png");
            autoHideTimer?.Start();
        }

        private void JokeButton_Click(object sender, RoutedEventArgs e)
        {
            HandleConversation(Messages.Prompts.Joke, Messages.Responses.Jokes, "mascot_happy.png");
        }

        private void GoodbyeButton_Click(object sender, RoutedEventArgs e)
        {
            HandleConversation(Messages.Prompts.Goodbye, Messages.Responses.Goodbyes, "mascot_sad.png");
        }

        private void HowAreYouButton_Click(object sender, RoutedEventArgs e)
        {
            HandleConversation(Messages.Prompts.HowAreYou, Messages.Responses.HowAreYou, "mascot_happy.png");
        }

        private void ComplimentButton_Click(object sender, RoutedEventArgs e)
        {
            HandleConversation(Messages.Prompts.Compliment, Messages.Responses.Compliments, "mascot_love.png");
        }

        private void MotivationButton_Click(object sender, RoutedEventArgs e)
        {
            HandleConversation(Messages.Prompts.Motivation, Messages.Responses.Motivation, "mascot_happy.png");
        }

        private void AdviceButton_Click(object sender, RoutedEventArgs e)
        {
            HandleConversation(Messages.Prompts.Advice, Messages.Responses.Advice, "mascot_thoughtful.png");
        }

        private void StoryButton_Click(object sender, RoutedEventArgs e)
        {
            HandleConversation(Messages.Prompts.Story, Messages.Responses.Stories, "mascot_happy.png");
        }

        private void FoodButton_Click(object sender, RoutedEventArgs e)
        {
            HandleConversation(Messages.Prompts.Food, Messages.Responses.Food, "mascot_hungry.png");
        }

        private void MusicButton_Click(object sender, RoutedEventArgs e)
        {
            HandleConversation(Messages.Prompts.Music, Messages.Responses.Music, "mascot_happy.png");
        }

        private void StudyButton_Click(object sender, RoutedEventArgs e)
        {
            HandleConversation(Messages.Prompts.Study, Messages.Responses.Study, "mascot_thoughtful.png");
        }

        private void SleepButton_Click(object sender, RoutedEventArgs e)
        {
            HandleConversation(Messages.Prompts.Sleep, Messages.Responses.Sleep, "mascot_sleepy.png");
        }

        private void ThanksButton_Click(object sender, RoutedEventArgs e)
        {
            HandleConversation(Messages.Prompts.Thanks, Messages.Responses.Thanks, "mascot_love.png");
        }
    }
} 