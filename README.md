# Admo Kinect

Admo's core gesture engine processes and simplifies how apps view gesture data.

This simplification allows us to build apps very rapidly only focusing on data that is useful to an experience.

## Phases  

Admo's platform revolves around 3 simple phases. This simplification is core to making our applications  intuitive easy to understand and use.

* **Phase 1** is active when no one is in view.
* **Phase 2** is active is when something or someone is in view. The Kinect is detecting something in the depth field, but can’t detect a skeletal structure. The only input received from the Kinect in this phase is the user’s head coordinate.
* **Phase 3** is active if someone is in view and the skeletal structure of the person is detected. 

### Coordinates

Our platform works on a simplified coordinate system, allowing gesture applications to be built using standard HD 1920 x 1080. We take the kinect’s 640x480 and double it. This direct mapping allows app developers to build apps using standard CSS using their normal design flow.

### Kinect Data

     KinectState = {
      phase:3,
      head:{
        x:300, y:135, z:1400
      },
      leftHand:{
        x:250, y:300, z:1400
      },
      rightHand: {
        x:350, y:300, z:1400
      }
      .....
    };

## Building

      Scripts\build.bat 1.1.%BUILD_NUMBER%.1 %GIT_BRANCH%

### Contributors

https://github.com/admoexperience/admo-kinect/contributors