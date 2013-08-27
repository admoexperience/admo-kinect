using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;

namespace Admo.classes.lib
{
    public class KinectLib
    {

        public int LockedSkeletonId = 0;
        public Skeleton LockedSkeleton = null;

        public void StopKinectSensor(KinectSensor sensor)
        {
            if (sensor == null) return;
            if (!sensor.IsRunning) return;
            //stop sensor 
            sensor.Stop();

            //stop audio if not null
            if (sensor.AudioSource != null)
            {
                sensor.AudioSource.Stop();
            }
        }

        public Skeleton GetPrimarySkeleton(Skeleton[] skeletons)
        {
            Skeleton skeleton = null;

            if (skeletons != null)
            {

                //check if any skeletons has been locked on in t-1        
                if (Application_Handler.locked_skeleton == true)
                {
                    for (int i = 0; i < skeletons.Length; i++)
                    {
                        if (skeletons[i].TrackingState == SkeletonTrackingState.Tracked)
                        {
                            if (skeletons[i].TrackingId == LockedSkeletonId)
                            {
                                //locked skeletal is still in range
                                skeleton = LockedSkeleton;
                                break;
                            }
                            else
                            {
                                Application_Handler.locked_skeleton = false;

                            }
                        }
                    }

                }
                else
                {
                    //Find the closest skeleton        
                    for (int i = 0; i < skeletons.Length; i++)
                    {
                        if (skeletons[i].TrackingState == SkeletonTrackingState.Tracked)
                        {
                            if (skeleton == null)
                            {
                                skeleton = skeletons[i];
                            }
                            else
                            {
                                if ((skeleton.Position.Z > skeletons[i].Position.Z))//if not just use the closest skeleton
                                {
                                    skeleton = skeletons[i];
                                }
                            }
                        }
                    }
                }

            }

            //if there is no other users in die fov and the locked skeleton moves out of the field of view
            //stop stopwatch 
            if ((skeleton == null) && (Application_Handler.locked_skeleton == true))
            {
                Application_Handler.locked_skeleton = false;
            }

            return skeleton;
        }
    }
}
