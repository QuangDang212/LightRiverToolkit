using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LightRiver.ServiceModel;

namespace App.Services
{
    public class GetPhotoParameter : HttpServiceParameter
    {
        public string PhotoId { get; set; }

        public bool IsThumbnail { get; set; }
    }
}
