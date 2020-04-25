using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        private static int _shutUpTreshold;
        private static int _shutUpCounter;

        private static List<string> _shutUpCheckList;
        private static List<string> _shutUpResponseList;

        static void Main(string[] args)
        {
            //Set config file
#if DEBUG
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings_debug.json");
#else
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
#endif

            _config = builder.Build();

            _shutUpCounter = 0;
            _shutUpTreshold = int.Parse(_config["shutUpThreshold"]);
            _shutUpCheckList = _config.GetSection("shutUpCheckList").GetChildren().Select(fm => fm.Value.ToString()).ToList();
            _shutUpResponseList = _config.GetSection("shutUpResponseList").GetChildren().Select(fm => fm.Value.ToString()).ToList();

            //Init bot client
            _botClient = new TelegramBotClient(_config["accessToken"]);
            _botUser = _botClient.GetMeAsync().Result;

            //Init db service
            _service = new WBotService();

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
                    //check if we want to response to shut up or similar
                    if(_shutUpCounter > 0 && shutUpCheck(e.Message.Text))
                    {
                        //1/4 chance to respond
                        Random r = new Random();
                        if(r.Next(3) == 0)
                        {
                            await _botClient.SendTextMessageAsync(e.Message.Chat, _shutUpResponseList.PickRandom(), replyToMessageId: e.Message.MessageId);
                            _shutUpCounter = 0;
                            return;
                        }

                        _shutUpCounter -= 1;
                    }
                    return;
                }

                var count = await _service.InsertLink(url, e.Message);

                //URL is posted for the first time
                if(count == 1)
                {
                    return;
                }

                await _botClient.SendTextMessageAsync(e.Message.Chat, $"W, postattu {count} kertaa", replyToMessageId: e.Message.MessageId);
                _shutUpCounter = _shutUpTreshold;
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
                    var formatted = word.FormatUrl();

                    //re-check
                    if (formatted.IsUrlValid())
                    {
                        return formatted;
                    }
                }
            }

            return null;
        }

        private static bool shutUpCheck(string messageTxt)
        {
            var txt = messageTxt.ToLower().Replace(" ", "");

            foreach(var s in _shutUpCheckList)
            {
                if (txt.Contains(s))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
