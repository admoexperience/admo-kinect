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

            int detection_range = 100;
            
            int min_depth_index_x = 0;
            int min_depth_index_y = 0;
            
            int min_depth_index = 0;
            int min_depth = 4000;            
            int depth_x = 0;
            int depth_y = 0;              

            //check for primary hand
            for (int depthIndex = 0; depthIndex < rawDepthData.Length; depthIndex++)
            {
                depth_x = (int)(depthIndex % 640);
                depth_y = (int)(depthIndex / 640);
                //get the player
                depth_array[1, depth_x, depth_y] = rawDepthData[depthIndex] & 7;
                
                if (depth_array[1, depth_x, depth_y] > 0)
                {
                    //gets the depth value
                    depth_array[0, depth_x, depth_y] = rawDepthData[depthIndex] >> 3;

                    if ((min_depth > depth_array[0, depth_x, depth_y]) && (depth_array[0, depth_x, depth_y] != -1))
                    {
                        //set the minimum depth and the index at which it occures
                        min_depth = depth_array[0, depth_x, depth_y];
                        min_depth_index = depthIndex;

                        //sets x and y coordinates of minimum depth point
                        min_depth_index_x = depth_x;
                        min_depth_index_y = depth_y;
                    }
                }
            }

            if (min_depth < 410)
            {
                min_depth = 400;
            }           

            int dx = depth_size = 90000/min_depth;
            int crop_index_x = min_depth_index_x - dx / 2;
            int crop_index_y = min_depth_index_y - dx / 2;

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

            
            int total_x = 0;
            int total_y = 0;
            int total_count = 0;
            int total_depth = 0;


            for (int ty = crop_index_y; ty < (crop_index_y+dx); ty++)
            {
                for (int tx = crop_index_x; tx < (crop_index_x + dx); tx++)
                {
                    if (((depth_array[0, tx, ty] - min_depth) < detection_range) && (depth_array[1, tx, ty] > 0))
                    {
                        total_x = total_x + tx;
                        total_y = total_y + ty;
                        total_depth = total_depth + depth_array[0, tx, ty];
                        total_count++;
                    }
                }
            }

            
            int min_depth_index_x2 = total_x / total_count;
            int min_depth_index_y2 = total_y / total_count;
            int min_depth2 = total_depth / total_count;

            int[] send = { min_depth_index_x2, min_depth_index_y2, min_depth2, min_depth_index_x, min_depth_index_y, min_depth};
            
            return send;
        }

        
    }
}
