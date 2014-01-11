using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LightRiver.Net;
using LightRiver.Net.Sockets;
using LightRiver.ServiceModel;

namespace App.Services
{
    public class GetPhotoSocketService : BaseTelegramService<PhotoData, GetPhotoParameter, GetPhotoParser>
    {
        public GetPhotoSocketService(ITelegramExchangeDispatcher telegramExchangeDispatcher, ParserPool parserPool)
            : base(telegramExchangeDispatcher, parserPool)
        {
        }

        // this method is only for demo. not meaning what you have to do.
        protected override Telegram PackParameterToSend(GetPhotoParameter parameter)
        {
            const string key = "ID={0};IsTumbnail={1}";
            var completeKey = string.Format(key, parameter.PhotoId, (parameter.IsThumbnail ? "Y" : "N"));
            var completeKeyBytes = Encoding.UTF8.GetBytes(completeKey);
            return new Telegram(0, completeKeyBytes);
        }
    }
}
