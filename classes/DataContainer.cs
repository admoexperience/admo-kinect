using System;


/**
 * DataContainer is used so we can wrap all the data sent to the client ie
 * {type: "kinectState", data: {......}}
 * 
 */
namespace Admo.classes
{
    class DataContainer
    {
        public String Type { get; set; }
        public Object Data { get; set; }
    }
}
