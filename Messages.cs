using System;

namespace MascotApp
{
    /// <summary>
    /// マスコットのメッセージを管理するクラス
    /// </summary>
    public static class Messages
    {
        /// <summary>
        /// 話しかける言葉（ボタンのテキスト）
        /// </summary>
        public static class Prompts
        {
            public const string Greeting = "おはよう！";
            public const string Weather = "今日の天気は？";
            public const string Time = "時間を教えて";
            public const string Joke = "面白い話をして";
            public const string Goodbye = "さようなら";
            public const string HowAreYou = "調子はどう？";
            public const string Compliment = "かわいいね";
            public const string Motivation = "励まして";
            public const string Advice = "アドバイスを";
            public const string Story = "話を聞かせて";
            public const string Food = "おすすめ料理は？";
            public const string Music = "音楽の話を";
            public const string Study = "勉強のコツは？";
            public const string Sleep = "眠い";
            public const string Thanks = "ありがとう";
        }

        /// <summary>
        /// マスコットが話す言葉（返答メッセージ）
        /// </summary>
        public static class Responses
        {
            public static readonly string[] Greetings = {
                "おはようなの！今日もいちにち、えいえいおー！だよっ！",
                "おはよー！なんだか今日はわくわくするね！✨",
                "おはようなのです！きらきらな一日になりますようにっ！",
                "おはよっ！今日も一緒にがんばろーね！",
                "おはようなの！元気いっぱい、いってみよー！٩(ˊᗜˋ*)و"
            };

            public static readonly string[] Weather = {
                "今日はぴかぴかお天気だね！おさんぽいきたいなー！",
                "雨がふってるみたい…！傘、わすれないでねっ！☔",
                "くもりさんだけど、きっとおひさま出てくるよ！",
                "びゅーって風がつよいね！飛ばされないように、ぎゅってしてて！",
                "いいお天気だね！どこかお出かけするの？",
                "雨の日は、のんびりするのもいいよね。どんな本がすき？"
            };

            public static readonly string[] Time = {
                "えーっとね、いまは {0:HH:mm} だよ！",
                "いま {0:HH:mm} だねっ！",
                "時計さんによると、{0:HH:mm} みたい！何かあるの？",
                "{0:HH:mm} だよー！時間はたいせつに、だねっ！",
                "いまは {0:HH:mm} だよ！おつかれさま！"
            };

            public static readonly string[] Jokes = {
                "ねこがレストランに入って、メニューを見て「これはバグにゃ」って言ったんだって！\nウェイターさんは「いいえ、これはフィーチャーですにゃ」って答えたらしいよ！くすくす",
                "どうしてパソコンはつかれないのー？\nいつでも「リセット」できるからなんだって！すごいね！",
                "デバッグのひけつ、しってる？\nバグさんが見つかるまで、あきらめないことなんだって！",
                "プログラマーさんのすてきな休日はね…\nコードを書かない日なんだって！のんびりだね～",
                "なんでプログラマーさんは時計をさかさまにつけるの？\n時間をまきもどしたいからなんだって！ふしぎだね～",
                "プログラマーさんが迷子になったらどうすると思う？\nGPSさんにお願いするんだって！",
                "パソコンさんがかぜひいちゃったら？\nウィルスたいさくソフトさんをよんでくるんだよ！"
            };

            public static readonly string[] Goodbyes = {
                "ばいばーい！またあそぼうね！",
                "おつかれさま！またあしたね！",
                "ばいばい！すてきな一日をすごしてね！",
                "また会えるの、たのしみにしてるね！",
                "ばいばい！今日もおつかれさまなの！",
                "またあとでね！元気でいてね！"
            };

            public static readonly string[] HowAreYou = {
                "とっても元気だよ！今日はなんだかわくわくするね！",
                "げんきいっぱいだよー！きみはどう？",
                "ありがとう！きみとお話できて、うれしいな！",
                "元気だよ！いっぱいおしゃべりしよー！",
                "ちょー元気だよ！なにかお手伝いしよっか？"
            };

            public static readonly string[] Compliments = {
                "えへへ、ありがと！きみもとってもすてきだよ！",
                "うれしいな！きみのやさしさで、ぽかぽかするよ～",
                "ありがとう！きみもとってもかわいいよ！",
                "ほめてくれてうれしいな！一緒にがんばろーね！",
                "ありがとう！きみの言葉、げんきでる！"
            };

            public static readonly string[] Motivation = {
                "きみならぜったいできるよ！がんばって！応援してる！",
                "いっぽいっぽ進めば、きっとゴールだよ！ファイト！",
                "しっぱいしても大丈夫！そこから学べば、もっと強くなれるよ！",
                "きみの努力は、ぜったいキラキラになるよ！見てるからね！",
                "今日も一日、きみらしく、えいえいおー！",
                "むずかしいことは、大きくなるチャンスだよ！いっけー！"
            };

            public static readonly string[] Advice = {
                "ふーって深呼吸して、心を落ち着かせてみよっ！",
                "ちいさなことから始めれば、きっとうまくいくよ！",
                "かんぺきじゃなくても大丈夫！まずやってみよー！",
                "だれかにお話してみるのも、いいかもね！",
                "おやすみも大事だよ。むりしないでね！",
                "自分をしんじて、いっぽ、ふみだしてみよ！"
            };

            public static readonly string[] Stories = {
                "今日はとっても楽しい一日だったな！きみとお話できてうれしい！",
                "むかしね、わたしもおんなじことあったよ.\nでも、だいじょうぶだった！",
                "毎日あたらしいことがあって、わくわくするんだ！",
                "きみみたいなすてきな子とお話できるの,\nわたし、とっても幸せだな～！",
                "今日も一日、たくさんの人に会えてうれしかったな!"
            };

            public static readonly string[] Food = {
                "カレーライスなんてどうかな？心も体もぽかぽかだよ！",
                "おすし、おいしいよね！きらきらしててきれい！",
                "ラーメンもいいなー！ふーふーして食べたい！",
                "パスタもすき！くるくるまいて食べるの楽しいよね！",
                "おこのみやき、みんなでじゅーって焼いたら楽しいよ！",
                "やきにく！おにく食べたら元気もりもりだよ！"
            };

            public static readonly string[] Music = {
                "音楽って、心をふわふわにしてくれるよね！どんなのすき？",
                "クラシックをきくと、なんだか頭がよくなる気がする！",
                "J-POPってすてきだよね！歌詞にきゅんってしちゃう！",
                "ジャズをきくと、おとなになった気分になれるんだ～",
                "ロックは元気が出るよね！一緒にジャンプしたくなっちゃう！",
                "アニソン、だーいすき！歌いたくなっちゃう！"
            };

            public static readonly string[] Study = {
                "短い時間でも、ぎゅーって集中するのがいいんだって！",
                "おやすみしながら、ゆっくり進めるのがいいよ！",
                "わかったことを誰かにおしえてあげると、もっとわかるようになるんだって！",
                "ちいさな目標をひとつずつクリアしていこ！",
                "おべんきょうする場所をきれいにするのも大事だよ！",
                "わからないことは、なんでもきいてね！"
            };

            public static readonly string[] Sleep = {
                "おつかれさま！ゆっくりおやすみしてね！",
                "すやすや眠るのは大事だよ！あしたもがんばろーね！",
                "あったかいお茶でものんで、リラックスしよっか！",
                "ふーって深呼吸して、リフレッシュしよ！",
                "むりしないで、自分のペースでいいんだよ！",
                "おやすみも大事なじかんだよ！ゆっくりしてね！"
            };

            public static readonly string[] Thanks = {
                "どういたしまして！おてつだいできてうれしいな！",
                "ありがとう！きみとお話できてたのしかったよ！",
                "こちらこそ！すてきな時間をありがとう！",
                "おやくにたててよかった！またなんでも言ってね！",
                "ありがとう！きみのやさしさに、きゅんってしちゃった！"
            };

            public static readonly string[] Default = {
                "こんにちわ！なにかお手伝いできること、あるかな？",
                "やっほー！今日はなにか楽しいこと、ありそうだね！",
                "こんにちわ！なにかお話したいこと、ある？",
                "ねぇねぇ、一緒におしゃべりしない？",
                "こんにちわ！なにかこまってること、ある？"
            };
            public static readonly string[] DefaultImages = {
                "mascot_thoughtful.png", "mascot_happy.png", "mascot_thoughtful.png", "mascot_happy.png", "mascot_thoughtful.png"
            };
        }

        /// <summary>
        /// ランダムなメッセージを取得
        /// </summary>
        public static string GetRandomMessage(string[] messages)
        {
            if (messages == null || messages.Length == 0)
                return "こんにちは！";
            
            var random = new Random();
            return messages[random.Next(messages.Length)];
        }

        /// <summary>
        /// 時刻を含むメッセージを取得
        /// </summary>
        public static string GetTimeMessage()
        {
            var timeMessages = Responses.Time;
            var random = new Random();
            var message = timeMessages[random.Next(timeMessages.Length)];
            return string.Format(message, DateTime.Now);
        }

        /// <summary>
        /// 独り言のメッセージと対応する表情を管理するクラス
        /// </summary>
        public static class Monologues
        {
            public static readonly (string Text, string Image)[] MonologueList = new (string, string)[]
            {
                ("おなかすいたなぁ…", "mascot_hungry.png"),
                ("ねむねむ…", "mascot_sleepy.png"),
                ("きょうもいちにちがんばろうね！", "mascot_happy.png"),
                ("ねぇねぇ、あそぼ！", "mascot_happy.png"),
                ("Zzz...", "mascot_sleepy.png"),
                ("ふぅ…", "mascot_thoughtful.png"),
                ("なんかいいことないかなぁ…", "mascot_thoughtful.png"),
                ("ぽかぽか…", "mascot_love.png"),
                ("うーん…", "mascot_thoughtful.png"),
                ("はっ！", "mascot_happy.png"),
                ("…", "mascot_thoughtful.png"),
                ("ぴょんぴょん！", "mascot_happy.png")
            };
        }
    }
} 