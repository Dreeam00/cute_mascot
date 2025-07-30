#nullable enable
using System;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Threading.Tasks;

namespace MascotApp
{
    public partial class MascotJumpWindow : Window
    {
        #pragma warning disable CS8618 // null 非許容のフィールドは、コンストラクターの終了時に null 以外の値が入っていなければなりません。
        private DispatcherTimer? jumpTimer = null;
        #pragma warning restore CS8618
        private double mascotX;
        private double mascotY;
        private double mascotVX;
        private double mascotVY;
        private double jumpVelocity = -14;
        private double gravity = 0.8;
        private double mascotWidth = 120;
        private double mascotHeight = 120;
        private double screenW;
        private double screenH;
        private bool tagMode = false;
        private bool isChaser = false; // 鬼ごっこで追いかける側か
        private Random random = new Random();

        public event Action? OnExitJumpMode;

        public MascotJumpWindow(bool tagMode = false, bool isChaser = false)
        {
            InitializeComponent();
            this.tagMode = tagMode;
            this.isChaser = isChaser;
            Loaded += MascotJumpWindow_Loaded;
            MascotImage.MouseLeftButtonDown += MascotImage_MouseLeftButtonDown;
        }

        private void MascotJumpWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // 画面サイズ取得
            screenW = SystemParameters.PrimaryScreenWidth;
            screenH = SystemParameters.PrimaryScreenHeight;
            // 初期位置
            mascotX = screenW / 2 - mascotWidth / 2;
            mascotY = screenH - mascotHeight - 40; // タスクバー上
            mascotVX = tagMode ? 0 : 8;
            mascotVY = tagMode ? 0 : jumpVelocity;
            UpdateMascotPosition();

            jumpTimer = new DispatcherTimer();
            jumpTimer.Interval = TimeSpan.FromMilliseconds(16);
            jumpTimer.Tick += tagMode ? TagTimer_Tick : JumpTimer_Tick;
            jumpTimer.Start();
        }

        // 通常ジャンプモード
        private void JumpTimer_Tick(object? sender, EventArgs e)
        {
            mascotX += mascotVX;
            mascotY += mascotVY;
            mascotVY += gravity;

            // 左右の画面端で反射
            if (mascotX < 0)
            {
                mascotX = 0;
                mascotVX = -mascotVX;
            }
            if (mascotX > screenW - mascotWidth)
            {
                mascotX = screenW - mascotWidth;
                mascotVX = -mascotVX;
            }
            // 下でバウンド
            double groundY = screenH - mascotHeight - 40;
            if (mascotY > groundY)
            {
                mascotY = groundY;
                mascotVY = jumpVelocity * (0.8 + 0.2 * random.NextDouble());
            }
            // 上に行きすぎないように
            if (mascotY < 0)
            {
                mascotY = 0;
                mascotVY = Math.Abs(mascotVY);
            }
            UpdateMascotPosition();
        }

        // 鬼ごっこモード
        private void TagTimer_Tick(object? sender, EventArgs e)
        {
            Point mousePos = Mouse.GetPosition(this);
            Point screenMousePos = PointToScreen(mousePos);
            double mouseX = screenMousePos.X;
            double mouseY = screenMousePos.Y;

            double mascotCenterX = mascotX + mascotWidth / 2;
            double mascotCenterY = mascotY + mascotHeight / 2;

            if (isChaser) // isChaser: マスコットが追いかける (ユーザーが逃げる)
            {
                double dx = mouseX - mascotCenterX;
                double dy = mouseY - mascotCenterY;
                double dist = Math.Sqrt(dx * dx + dy * dy);
                double angle = Math.Atan2(dy, dx);

                // マウスを追いかける
                double speed = 8;
                mascotVX = Math.Cos(angle) * speed;
                mascotVY = Math.Sin(angle) * speed;

                // 捕獲判定 (マスコットがマウスを捕まえたらユーザーの負け)
                double catchRange = 25;
                if (dist < catchRange)
                {
                    jumpTimer!.Stop();
                    System.Windows.MessageBox.Show("捕まった！", "鬼ごっこ", MessageBoxButton.OK, MessageBoxImage.Warning);
                    this.Close();
                    OnExitJumpMode?.Invoke();
                    return;
                }
            }
            else // !isChaser: マスコットが逃げる (ユーザーが追いかける)
            {
                double dx = mascotCenterX - mouseX;
                double dy = mascotCenterY - mouseY;
                double dist = Math.Sqrt(dx * dx + dy * dy);

                // 一定距離以内なら逃げる
                double escapeRange = 200;
                if (dist < escapeRange)
                {
                    double angle = Math.Atan2(dy, dx);
                    // 距離が近いほど速く逃げる
                    double speed = 18 * (1.0 - dist / escapeRange) + 5;
                    mascotVX = Math.Cos(angle) * speed + (random.NextDouble() - 0.5) * 4;
                    mascotVY = Math.Sin(angle) * speed + (random.NextDouble() - 0.5) * 4;
                }
                else
                {
                    // 範囲外ならゆっくり減速
                    mascotVX *= 0.92;
                    mascotVY *= 0.92;
                }
            }

            mascotX += mascotVX;
            mascotY += mascotVY;

            // 画面端の処理 (跳ね返りを少し自然に)
            if (mascotX < 0) { mascotX = 0; mascotVX = Math.Abs(mascotVX) * 0.8; }
            if (mascotX > screenW - mascotWidth) { mascotX = screenW - mascotWidth; mascotVX = -Math.Abs(mascotVX) * 0.8; }
            if (mascotY < 0) { mascotY = 0; mascotVY = Math.Abs(mascotVY) * 0.8; }
            double groundY = screenH - mascotHeight - 40;
            if (mascotY > groundY) { mascotY = groundY; mascotVY = -Math.Abs(mascotVY) * 0.5; }

            UpdateMascotPosition();
        }

        private void MascotImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // マスコットが「逃げる」モードの時だけ、クリックで捕獲できる
            if (tagMode && !isChaser)
            {
                jumpTimer!.Stop();
                MascotImage.IsEnabled = false;
                // 捕まえたアニメーション
                var anim = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(500));
                MascotImage.BeginAnimation(OpacityProperty, anim);
                Task.Delay(600).ContinueWith(_ =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        System.Windows.MessageBox.Show("捕まえた！あなたの勝ち！", "鬼ごっこ", MessageBoxButton.OK, MessageBoxImage.Information);
                        this.Close();
                        OnExitJumpMode?.Invoke();
                    });
                });
            }
        }

        private void UpdateMascotPosition()
        {
            this.Left = mascotX;
            this.Top = mascotY;
        }

        private void ExitJumpModeButton_Click(object sender, RoutedEventArgs e)
        {
            jumpTimer!.Stop();
            this.Close();
            OnExitJumpMode?.Invoke();
        }
    }
}
 