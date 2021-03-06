﻿using Microsoft.Kinect;

namespace Admo.classes.lib
{
    public class KinectLib
    {
        public int LockedSkeletonId = 0;
        public Skeleton LockedSkeleton = null;

        public static readonly TransformSmoothParameters AvatarAppSmoothingParams = new TransformSmoothParameters
            {
                Smoothing = 0.5f,
                Correction = 0.0f,
                Prediction = 0.0f,
                JitterRadius = 0.02f,
                MaxDeviationRadius = 0.04f
            };

        public static readonly TransformSmoothParameters CursorAppSmoothingParams = new TransformSmoothParameters
            {
                Smoothing = 0.9f,
                Correction = 0.1f,
                Prediction = 0.1f,
                JitterRadius = 0.05f,
                MaxDeviationRadius = 0.05f
            };

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
                if (TheHacks.LockedSkeleton == true)
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
                                TheHacks.LockedSkeleton = false;
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
                                if ((skeleton.Position.Z > skeletons[i].Position.Z))
                                    //if not just use the closest skeleton
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
            if ((skeleton == null) && (TheHacks.LockedSkeleton == true))
            {
                TheHacks.LockedSkeleton = false;
            }

            return skeleton;
        }
        public TransformSmoothParameters GetTransformSmoothParameters(string type)
        {
            return type == "cursor" ? CursorAppSmoothingParams : AvatarAppSmoothingParams;
        }
    }
}