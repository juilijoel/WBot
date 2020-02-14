using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using WBot.DataAccess.Services;

namespace WBot
{
    class Program
    {
        private static IConfigurationRoot _config;
        private static ITelegramBotClient _botClient;
        private static User _botUser;
        private static WBotService _service;

        static void Main(string[] args)
        {
            //Set config file
#if DEBUG
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings_debug.json");
#else
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
#endif

            _config = builder.Build();

            //Init bot client
            _botClient = new TelegramBotClient(_config["accessToken"]);
            _botUser = _botClient.GetMeAsync().Result;

            //Init db service
            _service = new WBotService();

            Console.WriteLine(
              $"Hello, World! I am user {_botUser.Id} and my name is {_botUser.FirstName}."
            );

            _botClient.OnMessage += Bot_OnMessage;
            _botClient.StartReceiving();
            Thread.Sleep(int.MaxValue);
        }

        private static async void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            try
            {
                //check if message has no text
                if(String.IsNullOrEmpty(e.Message.Text))
                {
                    return;
                }

                //check if message contains an URL
                var url = GetUrlFromMessage(e.Message);
                if(url == null)
                {
                    return;
                }

                var count = await _service.InsertLink(url, e.Message);

                //URL is posted for the first time
                if(count == 1)
                {
                    return;
                }

                await _botClient.SendTextMessageAsync(e.Message.Chat, $"W, postattu {count} kertaa", replyToMessageId: e.Message.MessageId);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static string GetUrlFromMessage(Message message)
        {
            //Split words
            var splitted = message.Text.Split(new char[] {' ','\n'});
            foreach (var word in splitted)
            {
                if (word.IsUrlValid())
                {
                    return word.FormatUrl();
                }
            }

            return null;
        }
    }
}
