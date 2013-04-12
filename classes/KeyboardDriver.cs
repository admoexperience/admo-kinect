using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsInput;


namespace Admo
{
    class KeyboardDriver
    {

        public static void FullScreen()
        {
            //InputSimulator.SimulateKeyDown(VirtualKeyCode.MENU);
            InputSimulator.SimulateKeyPress(VirtualKeyCode.F11);
            
        }

        public static void Exit()
        {
                InputSimulator.SimulateKeyDown(VirtualKeyCode.MENU);
                InputSimulator.SimulateKeyPress(VirtualKeyCode.F4);
                InputSimulator.SimulateKeyUp(VirtualKeyCode.MENU);                        
        }

        public static void NinjaExit()
        {
            //for when user presses shared by mistake
            //use voice command "go back" to exit share menu
            InputSimulator.SimulateKeyDown(VirtualKeyCode.ESCAPE);

            /*
            System.Threading.Thread.Sleep(100);
            
            InputSimulator.SimulateKeyDown(VirtualKeyCode.MENU);
            InputSimulator.SimulateKeyPress(VirtualKeyCode.F4);
            InputSimulator.SimulateKeyUp(VirtualKeyCode.MENU);
            SocketServer.Send_Gesture("blank");
            */
        }

        public static void Home()
        {
            InputSimulator.SimulateKeyPress(VirtualKeyCode.LWIN);
        }


        public static void KeyboardInput(string character,Boolean shift)
        {
            if (shift == false)
            {

                if (character == "a")
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_A);
                else if (character == "b")
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_B);
                else if (character == "c")
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_C);
                else if (character == "d")
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_D);
                else if (character == "e")
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_E);
                else if (character == "f")
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_F);
                else if (character == "g")
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_G);
                else if (character == "h")
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_H);
                else if (character == "i")
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_I);
                else if (character == "j")
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_J);
                else if (character == "k")
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_K);
                else if (character == "l")
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_L);
                else if (character == "m")
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_M);
                else if (character == "n")
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_N);
                else if (character == "o")
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_O);
                else if (character == "p")
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_P);
                else if (character == "q")
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_Q);
                else if (character == "r")
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_R);
                else if (character == "s")
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_S);
                else if (character == "t")
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_T);
                else if (character == "u")
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_U);
                else if (character == "v")
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_V);
                else if (character == "w")
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_W);
                else if (character == "x")
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_X);
                else if (character == "y")
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_Y);
                else if (character == "z")
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_Z);

                
            }
            else if (shift == true)
            {
                InputSimulator.SimulateKeyDown(VirtualKeyCode.SHIFT);

                if (character == "A")
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_A);
                else if (character == "B")
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_B);
                else if (character == "C")
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_C);
                else if (character == "D")
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_D);
                else if (character == "E")
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_E);
                else if (character == "F")
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_F);
                else if (character == "G")
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_G);
                else if (character == "H")
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_H);
                else if (character == "I")
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_I);
                else if (character == "J")
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_J);
                else if (character == "K")
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_K);
                else if (character == "L")
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_L);
                else if (character == "M")
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_M);
                else if (character == "N")
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_N);
                else if (character == "O")
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_O);
                else if (character == "P")
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_P);
                else if (character == "Q")
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_Q);
                else if (character == "R")
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_R);
                else if (character == "S")
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_S);
                else if (character == "T")
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_T);
                else if (character == "U")
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_U);
                else if (character == "V")
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_V);
                else if (character == "W")
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_W);
                else if (character == "X")
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_X);
                else if (character == "Y")
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_Y);
                else if (character == "Z")
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_Z);

                InputSimulator.SimulateKeyUp(VirtualKeyCode.SHIFT);
            }
        }

    }
}
