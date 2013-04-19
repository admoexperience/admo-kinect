using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;
using Fleck;
using System.IO;
using System.Threading;
//using System.Windows.Forms.Integration;
using System.Threading.Tasks;
using SocketIOClient.Messages;
using SocketIOClient;
using SocketIOClient.Eventing;
using Newtonsoft.Json.Linq;

namespace Admo
{
    public class SocketServer
    {
        public static Client socket;
        public static bool server_running = false;

        //Websocket client setup for communication between Node server and .NET
        public static void Start_SocketIOClient(String server)
		{

			Console.WriteLine("Starting SocketIOClient Example........");

            socket = new Client(server); // url to the nodejs / socket.io instance

			socket.Opened += SocketOpened;
			socket.Message += SocketMessage;
			socket.SocketConnectionClosed += SocketConnectionClosed;
			socket.Error += SocketError;    
                  

			// make the socket.io connection
			socket.Connect();
            var logger = socket.Connect("/life"); // connect to the logger ns                
            
            socket.On("connect", (fn) =>
            {
                Console.WriteLine("********" + fn.RawMessage);
            });

            /*            
           //http://stackoverflow.com/questions/12207600/socketio4net-client-subscribing-to-a-channel
            //don't do the motherfucking .On("eventname, function) shit if you want to receive a message event         
           logger.On("connect", (log) =>
           {
           });
           */

        }

        public static void Close_SocketServer(String temp)
        {
            socket.Close();
            if(temp!="close server")
                Start_SocketIOClient(temp);            
        }
        

        public static void SocketError(object sender, SocketIOClient.ErrorEventArgs e) { }

        public static void SocketConnectionClosed(object sender, EventArgs e) { }

        public static bool user_start = false;
        public static String[] selections = new String[100];
        public static int selection_count = 0;

        //read message at socket
        public static void SocketMessage(object sender, MessageEventArgs e)
		{
            //getting app name from node server
            String tempMessage = e.Message.RawMessage;
            try
            {
                int first = tempMessage.IndexOf("name");
                first = first + 9;
                int last = tempMessage.LastIndexOf("\"");
                string str2 = tempMessage.Substring(first, last - first-2);
                //Console.WriteLine(str2);
                
                if (str2 == "alive")
                {
                    //receive "alive" ping message from browser
                    double current_time = Convert.ToDouble(DateTime.Now.Ticks) / 10000;
                    LifeCycle.browser_time  = current_time;
                    Send_App("host-" + MainWindow.pc_name);
                }
                else if (str2 == "reloaded") {
                    //Start up stage 5 is "allowing camera access"
                    //This needs to be done in a different thread so set the var
                    //so next cycle will accept camera access
                    LifeCycle.startup_stage5 = false;
                }
            }
            catch (Exception et)
            {
            }
		}

        public static void SocketOpened(object sender, EventArgs e)
		{
		}
       


        public static String set_gesture = "";

        //set gesture to be send
        public static void Set_Gesture(String name)
        {
            set_gesture = name;
        }

        //Send gesture to node server
        public static void Send_Gesture(String stick)
        {
            String name = stick + set_gesture;
            if (server_running == true)
                socket.Emit("gesture", new Gesture(name));
                        
            set_gesture = "";
        }

        //Send gesture to node server
        public static void Send_App(String name)
        {
            Thread.Sleep(1000);
            if (server_running == true)
                socket.Emit("gesture", new Gesture(name));

            //Console.WriteLine(name);
        }



        public class Gesture
        {
            public string gesture;

            public Gesture(string name)
            {
                gesture = name;
            }
        }


    }
}
