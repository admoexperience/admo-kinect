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

        public static float[] GetCoordinates(Skeleton first)
        {
            JointCollection mySkelJoints = first.Joints;
            var handX = mySkelJoints[JointType.HandLeft].Position.X;
            //get joint coordinates
            var coordinates = new float[24];
            var leftHand = first.Joints[JointType.HandLeft];
            var rightHand = first.Joints[JointType.HandRight];
            var spineCenter = first.Joints[JointType.Spine];
            var head = first.Joints[JointType.Head];
            var shoulderLeft = first.Joints[JointType.ShoulderLeft];
            var shoulderRight = first.Joints[JointType.ShoulderRight];
            var hipCenter = first.Joints[JointType.HipCenter];
            var shoulderCenter = first.Joints[JointType.ShoulderCenter];
            var leftElbow = first.Joints[JointType.ElbowLeft];
            var rightElbow = first.Joints[JointType.ElbowRight];

            //set joint coordinates into array
            coordinates[0] = leftHand.Position.X;
            coordinates[1] = leftHand.Position.Y;
            coordinates[2] = rightHand.Position.X;
            coordinates[3] = rightHand.Position.Y;
            coordinates[4] = 0;// spine_center.Position.X;
            coordinates[5] = 0;// spine_center.Position.Y;
            coordinates[6] = head.Position.X;
            coordinates[7] = head.Position.Y;
            coordinates[8] = shoulderLeft.Position.X;
            coordinates[9] = shoulderLeft.Position.Y;
            coordinates[10] = shoulderRight.Position.X;
            coordinates[11] = shoulderRight.Position.Y;
            coordinates[12] = 0;// hip_center.Position.X;
            coordinates[13] = 0;// hip_center.Position.Z;
            coordinates[14] = leftHand.Position.Z;
            coordinates[15] = rightHand.Position.Z;
            coordinates[16] = shoulderCenter.Position.X;
            coordinates[17] = shoulderCenter.Position.Y;
            coordinates[18] = spineCenter.Position.Z;
            coordinates[19] = head.Position.Z;
            coordinates[20] = leftElbow.Position.X;
            coordinates[21] = leftElbow.Position.Y;
            coordinates[22] = rightElbow.Position.X;
            coordinates[23] = rightElbow.Position.Y;

            Application_Handler.SkeletalCoordinates = coordinates;

            return coordinates;
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
