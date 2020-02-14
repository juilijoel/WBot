using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telegram.Bot.Types;
using System.Threading.Tasks;

namespace WBot.DataAccess.Services
{
    public class WBotService
    {
        //returns count of previous posts if link is already found in db
        public async Task<int> InsertLink(string linkUrl, Message message)
        {
            using (var ctx = new WBotContext())
            {
                var existing = ctx.Links.FirstOrDefault(l => l.LinkUrl == linkUrl && l.ChatId == message.Chat.Id);

                if (existing == null)
                {
                    ctx.Links.Add(new Models.Link()
                    {
                        LinkUrl = linkUrl,
                        ChatId = message.Chat.Id,
                        FirstPostMessageId = message.MessageId,
                        FirstPostTimeStamp = message.Date,
                        PasteCount = 1
                    });

                    await ctx.SaveChangesAsync();
                    return 1;
                }

                else
                {
                    existing.PasteCount++;
                    await ctx.SaveChangesAsync();
                    return existing.PasteCount;
                }
            }
        }
    }
}
