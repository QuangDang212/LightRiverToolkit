using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace App.Services
{
    public class PhotoData
    {
        [JsonProperty("PhotoUrl")]
        public string PhotoUrl { get; set; }
    }
}
