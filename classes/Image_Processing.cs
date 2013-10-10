using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;


namespace Admo
{
    class Image_Processing
    {

        public static int[, ,] depth_array = new int[2, 640, 480]; //(480,640)
        public static int DepthSize = 50;
       
        public static int[] ProcessHands(short[] rawDepthData)
        {
            if (rawDepthData == null) throw new ArgumentNullException("rawDepthData");

            const int detectionRange = 100;
            
            int minDepthIndexX = 0;
            int minDepthIndexY = 0;

            int minDepth = 4000;

            //check for primary hand
            for (int depthIndex = 0; depthIndex < rawDepthData.Length; depthIndex++)
            {
                int depthX = (int)(depthIndex % 640);
                int depthY = (int)(depthIndex / 640);
                //get the player
                depth_array[1, depthX, depthY] = rawDepthData[depthIndex] & 7;
                
                if (depth_array[1, depthX, depthY] > 0)
                {
                    //gets the depth value
                    depth_array[0, depthX, depthY] = rawDepthData[depthIndex] >> 3;

                    if ((minDepth > depth_array[0, depthX, depthY]) && (depth_array[0, depthX, depthY] != -1))
                    {
                        //set the minimum depth and the index at which it occures
                        minDepth = depth_array[0, depthX, depthY];

                        //sets x and y coordinates of minimum depth point
                        minDepthIndexX = depthX;
                        minDepthIndexY = depthY;
                    }
                }
            }

            if (minDepth < 410)
            {
                minDepth = 400;
            }           

            int dx = DepthSize = 90000/minDepth;
            int cropIndexX = minDepthIndexX - dx / 2;
            int cropIndexY = minDepthIndexY - dx / 2;

            if (cropIndexX < 0)
            {
                cropIndexX = 0;
            }
            else if (cropIndexX > (640-dx))
            {
                cropIndexX = 640 - dx;
            }

            if (cropIndexY < 0)
            {
                cropIndexY = 0;
            }
            else if (cropIndexY > (480 - dx))
            {
                cropIndexY = 480 - dx;
            }

            
            int totalX = 0;
            int totalY = 0;
            int totalCount = 0;
            int totalDepth = 0;


            for (int ty = cropIndexY; ty < (cropIndexY+dx); ty++)
            {
                for (int tx = cropIndexX; tx < (cropIndexX + dx); tx++)
                {
                    if (((depth_array[0, tx, ty] - minDepth) < detectionRange) && (depth_array[1, tx, ty] > 0))
                    {
                        totalX = totalX + tx;
                        totalY = totalY + ty;
                        totalDepth = totalDepth + depth_array[0, tx, ty];
                        totalCount++;
                    }
                }
            }

            
            int minDepthIndexX2 = totalX / totalCount;
            int minDepthIndexY2 = totalY / totalCount;
            int minDepth2 = totalDepth / totalCount;

            int[] send = { minDepthIndexX2, minDepthIndexY2, minDepth2, minDepthIndexX, minDepthIndexY, minDepth};
            
            return send;
        }

        
    }
}
