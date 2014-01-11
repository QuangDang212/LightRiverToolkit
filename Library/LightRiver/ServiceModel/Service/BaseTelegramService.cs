using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LightRiver.Net;
using LightRiver.Net.Sockets;

namespace LightRiver.ServiceModel
{
    public abstract class BaseTelegramService<TResult, TParameter, TParser> : BaseNetService<TResult, TParameter>
        where TParser : BaseParser<TResult, Telegram>, new()
    {
        private ParserPool _parserPool = null;

        private ITelegramExchangeDispatcher _telegramExchangeDispatcher = null;

        private TParser _parser = new TParser();

        public BaseTelegramService(ITelegramExchangeDispatcher telegramExchangeDispatcher, ParserPool parserPool)
        {
            _telegramExchangeDispatcher = telegramExchangeDispatcher;
            _parserPool = parserPool;
        }

        public override Task<ParseResult<TResult>> InvokeAsync(TParameter parameter, int timeout = 60000)
        {
            return Task.Factory.StartNew<ParseResult<TResult>>(() => {
                var telegram = PackParameterToSend(parameter);

                var waitEvent = new ManualResetEvent(false);

                ParseResult<TResult> parseResult = null;

                _telegramExchangeDispatcher.Enqueue(telegram, (telegramReceive) => {
                    var parseWorkItem = new ParseWorkItem<TResult, Telegram>(telegramReceive, _parser);
                    parseWorkItem.ParseFinish += (sender, e) => {
                        // if not found waitEvent meaning timeout then
                        if (waitEvent == null)
                            return;

                        parseResult = e.ParseResult;
                        waitEvent.Set();
                    };

                    _parserPool.Enqueue(parseWorkItem);
                });

                waitEvent.WaitOne(TimeSpan.FromMilliseconds(timeout));
                waitEvent.Dispose();
                waitEvent = null;

                return parseResult;
            });
        }

        protected abstract Telegram PackParameterToSend(TParameter parameter);
    }
}
