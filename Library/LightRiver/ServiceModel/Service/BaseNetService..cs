using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LightRiver.Net;

namespace LightRiver.ServiceModel
{
    public abstract class BaseNetService<TResult, TParameter>
    {
        public abstract Task<ParseResult<TResult>> InvokeAsync(TParameter parameter, int timeout = 60000);
    }
}
