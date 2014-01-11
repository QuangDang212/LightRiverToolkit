using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightRiver.Net
{
    public class ParseWorkItem<TResult, TSource> : IParserWorkItem
    {
        public event EventHandler<ParseFinishEventArgs<TResult>> ParseFinish;
        private void OnParseFinish(ParseResult<TResult> parseResult)
        {
            SafeRaise.Raise<ParseFinishEventArgs<TResult>>(ParseFinish, this, new ParseFinishEventArgs<TResult>(parseResult));
        }

        private TSource _source;

        private BaseParser<TResult, TSource> _parser;

        public ParseWorkItem(TSource source, BaseParser<TResult, TSource> parser)
        {
            _source = source;
            _parser = parser;
        }

        public void Execute()
        {
            var parseResult = _parser.Parse(_source);
            OnParseFinish(parseResult);
        }
    }
}
