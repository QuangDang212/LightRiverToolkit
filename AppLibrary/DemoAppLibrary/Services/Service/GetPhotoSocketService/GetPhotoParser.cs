using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LightRiver.Net;
using LightRiver.Net.Sockets;

namespace App.Services
{
    public class GetPhotoParser : BaseParser<PhotoData, Telegram>
    {
        public override ParseResult<PhotoData> Parse(Telegram source)
        {
            ParseResult<PhotoData> result = new ParseResult<PhotoData>(new PhotoData());
            return result;
        }
    }
}
