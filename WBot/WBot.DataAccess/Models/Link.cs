using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace WBot.DataAccess.Models
{
    public class Link
    {
        public int Id { get; set; }
        [Url]
        public long ChatId { get; set; }
        public long FirstPostMessageId { get; set; }
        public string LinkUrl { get; set; }
        public int PasteCount { get; set; }
    }
}
