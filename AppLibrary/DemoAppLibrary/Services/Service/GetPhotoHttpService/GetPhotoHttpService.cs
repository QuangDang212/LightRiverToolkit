using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LightRiver.Net;
using LightRiver.ServiceModel;

namespace App.Services
{
    public class GetPhotoHttpService : BaseHttpService<PhotoData, GetPhotoParameter, JsonParser<PhotoData>>
    {
        public GetPhotoHttpService(string apiUrl)
            : base(apiUrl)
        {
        }
    }
}
