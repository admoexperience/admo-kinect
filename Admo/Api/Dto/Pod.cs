using System;

namespace Admo.Api.Dto
{
    public struct PodApp
    {
        public String Name { set; get; }

        public String UpdatedAt { set; get; }

        public String PodChecksum { set; get; }

        public String PodUrl { set; get; }
    }
}