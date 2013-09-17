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
        public static int depth_size = 50;
       
        public static int[] ProcessHands(short[] rawDepthData)
        {
            if (rawDepthData == null) throw new ArgumentNullException("rawDepthData");

            const int detectionRange = 100;
            
            int minDepthIndexX = 0;
            int minDepthIndexY = 0;
            
            int min_depth_index = 0;
            int minDepth = 4000;

            //check for primary hand
            for (int depthIndex = 0; depthIndex < rawDepthData.Length; depthIndex++)
            {
                int depth_x = (int)(depthIndex % 640);
                int depth_y = (int)(depthIndex / 640);
                //get the player
                depth_array[1, depth_x, depth_y] = rawDepthData[depthIndex] & 7;
                
                if (depth_array[1, depth_x, depth_y] > 0)
                {
                    //gets the depth value
                    depth_array[0, depth_x, depth_y] = rawDepthData[depthIndex] >> 3;

                    if ((minDepth > depth_array[0, depth_x, depth_y]) && (depth_array[0, depth_x, depth_y] != -1))
                    {
                        //set the minimum depth and the index at which it occures
                        minDepth = depth_array[0, depth_x, depth_y];
                        min_depth_index = depthIndex;

                        //sets x and y coordinates of minimum depth point
                        minDepthIndexX = depth_x;
                        minDepthIndexY = depth_y;
                    }
                }
            }

            if (minDepth < 410)
            {
                minDepth = 400;
            }           

            int dx = depth_size = 90000/minDepth;
            int crop_index_x = minDepthIndexX - dx / 2;
            int crop_index_y = minDepthIndexY - dx / 2;

            if (crop_index_x < 0)
            {
                crop_index_x = 0;
            }
            else if (crop_index_x > (640-dx))
            {
                crop_index_x = 640 - dx;
            }

            if (crop_index_y < 0)
            {
                crop_index_y = 0;
            }
            else if (crop_index_y > (480 - dx))
            {
                crop_index_y = 480 - dx;
            }

            
            int totalX = 0;
            int totalY = 0;
            int totalCount = 0;
            int totalDepth = 0;


            for (int ty = crop_index_y; ty < (crop_index_y+dx); ty++)
            {
                for (int tx = crop_index_x; tx < (crop_index_x + dx); tx++)
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
