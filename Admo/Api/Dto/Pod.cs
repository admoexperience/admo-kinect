using System;

namespace Admo.Api.Dto
{
    public class PodApp : BaseApiResult
    {
        public string Name { set; get; }

        public string UpdatedAt { set; get; }

        public string PodChecksum { set; get; }

        public string PodUrl { set; get; }

        public string PodName { get; set; }
    }
}