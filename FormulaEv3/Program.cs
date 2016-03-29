//////////////////////////////////////////////////////////////////////////////////////////////////
// Formula EV3                                                                                  //
// Version 1.0                                                                                  //
//                                                                                              //
// Happily shared under the MIT License (MIT)                                                   //
//                                                                                              //
// Copyright(c) 2016 SmallRobots.it                                                             //
//                                                                                              //
// Permission is hereby granted, free of charge, to any person obtaining                        //
//a copy of this software and associated documentation files (the "Software"),                  //
// to deal in the Software without restriction, including without limitation the rights         //
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies             //
// of the Software, and to permit persons to whom the Software is furnished to do so,           //      
// subject to the following conditions:                                                         //
//                                                                                              //
// The above copyright notice and this permission notice shall be included in all               //
// copies or substantial portions of the Software.                                              //
//                                                                                              //
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,          //
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR     //
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE           //
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,          //
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE        //
// OR OTHER DEALINGS IN THE SOFTWARE.                                                           //
//                                                                                              //
// Visit http://wwww.smallrobots.it for tutorials and videos                                    //
//                                                                                              //
// Credits                                                                                      //
// The Formula Ev3 is built with Lego Mindstorms Ev3 retail set                                 //
// Building instructions can be found on                                                        //
// "The Lego Mindstorms Ev3 Discovery Book" of Laurens Valk                                     //
//////////////////////////////////////////////////////////////////////////////////////////////////

using System.Threading;
using SmallRobots.Ev3ControlLib.Menu;
using MonoBrickFirmware.Display;
using MonoBrickFirmware.Movement;
using MonoBrickFirmware.Display.Menus;

namespace SmallRobots.FormulaEv3
{
    class Program
    {
        #region Static Fields
        private static int previousSteeringValue = 0;
        #endregion

        static public MenuContainer container;

        public static void Main(string[] args)
        {
            Menu menu = new Menu("Formula Ev3");
            container = new MenuContainer(menu);
            menu.AddItem(new ItemWithNumericInput("Calibrate Steering", 0, CalibrateSteering, -30, 30));
            menu.AddItem(new MainMenuItem("Start", Start_OnEnterPressed));
            menu.AddItem(new MainMenuItem("Quit", Quit_OnEnterPressed));

            container.Show();
        }

        public static void TerminateMenu()
        {
            container.Terminate();
        }

        public static void CalibrateSteering(int newValue)
        {
            sbyte maxSpeed = 10;
            sbyte speed = 0;
            Motor Motor = new Motor(MotorPort.OutA);

            if (newValue > previousSteeringValue)
            {
                speed = (sbyte)-maxSpeed;
            }
            else
            {
                speed = (sbyte)maxSpeed;
            }
            previousSteeringValue = newValue;

            Motor.SpeedProfileTime(speed, 100, 100, 100, true);
        }

        public static void Start_OnEnterPressed()
        {
            container.SuspendButtonEvents();
            FormulaEv3 FormulaEv3 = new FormulaEv3();
            FormulaEv3.Start();
            container.ResumeButtonEvents();
        }

        public static void Quit_OnEnterPressed()
        {
            LcdConsole.Clear();
            LcdConsole.WriteLine("Terminating");
            // Wait a bit
            Thread.Sleep(1000);
            TerminateMenu();
        }

    }
}
