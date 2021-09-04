using System;
using Telegram.Bot;
using Telegram.Bot.Args;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace TelegramBot
{
    class Program
    {
        static TelegramBotClient bot;
        static Quiz QwObject;
        static Dictionary<long, QuastionState> QuastionStates;

        static void Main(string[] args)
        {
            var token = "1927626701:AAG8vLTJjQh0rgUb6Mz4povXw2WZSYADQoE";
            bot = new TelegramBotClient(token);
            QwObject = new Quiz();
            QuastionStates = new Dictionary<long, QuastionState>();
            bot.OnMessage += Bot_OnMessage;
            bot.StartReceiving();
            Console.ReadLine();
        }

        static float score = 0;
        static float all = 0;
        static int end = 0;

        private static void Bot_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            var chatId = e.Message.Chat.Id;
            var massage = e.Message.Text;
            var userId = e.Message.From.FirstName;
           
            if (massage == "/start")
            {
                NewRound(chatId);
                end = 0;
            }

            if (massage == "/exit")
            {
                float g = score / all;
                bot.SendTextMessageAsync(chatId, "Процент правильныйх ответов: " + (g * 100) + " %. "+ userId + " вы молодец!!!");
                bot.SendTextMessageAsync(chatId, "Подсказка:\n  /start - что бы начать новую игру");

                all = 0;
                score = 0;
                end = 1;


            } else

            if (end == 0) 
            { 
                if (QuastionStates.TryGetValue(chatId, out var quastionState))
                {

                    if (massage.Trim().ToLowerInvariant() == quastionState.Item.Answer.Trim().ToLowerInvariant())
                    {
                        var WinMassage = $"Правильно - это {quastionState.Item.Answer}";
                        bot.SendTextMessageAsync(chatId, WinMassage);
                        NewRound(chatId);
                        quastionState.Win = true;
                        all++;
                        score++;
                    }
                    else
                    {
                        quastionState.Opened++;
                        if (quastionState.Opened >= 4)
                        {
                            var lose = $"Не верно. Ответ - {quastionState.Item.Answer}";
                            bot.SendTextMessageAsync(chatId, lose);
                            NewRound(chatId);
                            all++;
                        }
                        else
                        {
                            bot.SendTextMessageAsync(chatId, $"{quastionState.Item.Quaston}\nПодсказка:\n{quastionState.AnswerHint}");
                        }
                    }

                }
            }

        }

        public static void NewRound(long chatId)
        {
            var quastion = QwObject.Give();

            QuastionStates[chatId] = new QuastionState
            {
                Item = quastion
            };
        }
    }


    class Quiz
    {
        List<QuastionItem> qw;
        Random random;
        int count;
        public Quiz(string t = "rrr.txt")
        {
            random = new Random();
            var ObFile = File.ReadAllLines("rrr.txt");
            qw = ObFile.Select(s => s.Split('|')).Select(s => new QuastionItem {Quaston = s[0], Answer = s[1] }).ToList();
            
        }

        public QuastionItem Give()
        {
           
            if (count < 1)
            {
                count = qw.Count;
            }

            var index = random.Next(count - 1);
            var question = qw[index];
            qw.RemoveAt(index);
            qw.Add(question);
            count--;
            return question;
        }
    }

    class QuastionItem
    {
        public string Quaston { get; set; }
        public string Answer { get; set; }
    }

    class QuastionState
    {
        public QuastionItem Item{ get; set;}
        public int Opened { get; set; }
        public bool isEnd => Opened >= Item.Answer.Length;
        public bool Win { get; set; }
        public string AnswerHint => Item.Answer.Substring(0, Opened).PadRight(Item.Answer.Length, '_');
    }
}
