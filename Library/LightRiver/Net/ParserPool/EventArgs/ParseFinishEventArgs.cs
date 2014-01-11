using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightRiver.Net
{
    public class ParseFinishEventArgs<T> : EventArgs
    {
        public ParseResult<T> ParseResult { get; private set; }

        public ParseFinishEventArgs(ParseResult<T> parseResult)
        {
            ParseResult = parseResult;
        }
    }
}
