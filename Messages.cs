#nullable enable
using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace MascotApp
{
    public static class Messages
    {
        private static JObject allMessages = new JObject();
        private static string currentMascot = "Mascot";

        public static void LoadMessages(string mascotName)
        {
            try
            {
                string messagesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "messages.json");
                if (File.Exists(messagesPath))
                {
                    string json = File.ReadAllText(messagesPath);
                    allMessages = JObject.Parse(json);
                }
                currentMascot = mascotName;
            }
            catch (Exception ex)
            {
                allMessages = new JObject(); // Initialize as empty on error
                Console.WriteLine($"メッセージファイルの読み込みに失敗しました: {ex.Message}");
            }
        }

        private static string[] GetMessages(string category)
        {
            if (allMessages[currentMascot] is JObject mascotMessages && mascotMessages[category] is JArray messagesArray)
            {
                return messagesArray.ToObject<string[]>() ?? new[] { "..." };
            }
            return new[] { "..." };
        }

        public static (string Text, string Image) GetRandomMonologue()
        {
            if (allMessages[currentMascot]?["Monologues"] is JArray monologues)
            {
                var random = new Random();
                var monologue = monologues[random.Next(monologues.Count)];
                return (monologue["Text"]?.ToString() ?? "", monologue["Image"]?.ToString() ?? "default");
            }
            return ("", "default");
        }

        public static string GetRandomMessage(string category)
        {
            string[] messages = GetMessages(category);
            if (messages == null || messages.Length == 0)
                return "...";

            var random = new Random();
            return messages[random.Next(messages.Length)];
        }

        public static string GetTimeMessage()
        {
            string[] timeMessages = GetMessages("Time");
            var random = new Random();
            if (timeMessages.Length == 0)
                return "";
            var message = timeMessages[random.Next(timeMessages.Length)];
            return string.Format(message, DateTime.Now);
        }

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
    }
} 