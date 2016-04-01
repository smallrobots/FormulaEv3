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

using MonoBrickFirmware.Sensors;
using MonoBrickFirmware.Movement;
using SmallRobots.Ev3ControlLib;
using System;
using MonoBrickFirmware.Display;
using MonoBrickFirmware.UserInput;
using System.Threading;

namespace SmallRobots.FormulaEv3
{
    /// <summary>
    /// Ev3 IR Remote Command
    /// </summary>
    public enum Direction
    {
        Stop = 0,
        Straight_Forward,
        Left_Forward,
        Right_Forward,
        Straight_Backward,
        Left_Backward,
        Right_Backward,
        Beacon_ON
    }

    public partial class FormulaEv3 : Robot
    {
        #region Fields
        /// <summary>
        /// Commanded steering angle
        /// </summary>
        public float commandedSteeringAngle;

        /// <summary>
        /// Commanded direction
        /// </summary>
        public Direction direction;

        /// <summary>
        /// The right back engine
        /// </summary>
        public Motor rightEngine;

        /// <summary>
        /// The left back engine
        /// </summary>
        public Motor leftEngine;

        /// <summary>
        /// The steering engine
        /// </summary>
        public Motor steeringEngine;

        /// <summary>
        /// The Ev3 IR Sensor
        /// </summary>
        public EV3IRSensor irSensor;

        /// <summary>
        /// The Ev3 Color Sensor
        /// </summary>
        public EV3ColorSensor colorSensor;
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public FormulaEv3(): base()
        {
            LcdConsole.Clear();
            LcdConsole.WriteLine("FEv3 Init");
            // Initialize the motors
            leftEngine = new Motor(MotorPort.OutB);
            rightEngine = new Motor(MotorPort.OutC);
            steeringEngine = new Motor(MotorPort.OutA);
            LcdConsole.WriteLine("Motors ok");

            // Initialize the sensors
            irSensor = new EV3IRSensor(SensorPort.In4, IRMode.Remote);
            colorSensor = new EV3ColorSensor(SensorPort.In3, ColorMode.Reflection);   // Just to show the red light like a true Formula 1
            LcdConsole.WriteLine("Sensors init ok");
            LcdConsole.WriteLine("FEv3 Init OK");

            // Initialize the tasks
            TaskScheduler.Add(new IRRemoteTask());
            LcdConsole.WriteLine("IRRemoteTask OK");

            TaskScheduler.Add(new LCDUpdateTask());
            LcdConsole.WriteLine("LCDUpdateTask OK");

            TaskScheduler.Add(new KeyboardTask());
            LcdConsole.WriteLine("KeyboardTask OK");

            TaskScheduler.Add(new DriveTask());
            LcdConsole.WriteLine("DriveTask OK");

            TaskScheduler.Add(new SteeringTask());
            LcdConsole.WriteLine("SteeringTask OK");
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Starts the robot
        /// </summary>
        public void Start()
        {
            // Welcome messages
            LcdConsole.Clear();
            LcdConsole.WriteLine("*****************************");
            LcdConsole.WriteLine("*                           *");
            LcdConsole.WriteLine("*      SmallRobots.it       *");
            LcdConsole.WriteLine("*                           *");
            LcdConsole.WriteLine("*     Formula Ev3  1.0      *");
            LcdConsole.WriteLine("*                           *");
            LcdConsole.WriteLine("*                           *");
            LcdConsole.WriteLine("*   Enter to start          *");
            LcdConsole.WriteLine("*   Escape to quit          *");
            LcdConsole.WriteLine("*                           *");
            LcdConsole.WriteLine("*****************************");

            // Busy wait for user
            bool enterButtonPressed = false;
            bool escapeButtonPressed = false;
            while (!(enterButtonPressed || escapeButtonPressed))
            {
                // Either the user presses the touch sensor, or presses the escape button
                // If users presses both, escape button will prevale
                enterButtonPressed = (Buttons.ButtonStates.Enter == Buttons.GetKeypress(new CancellationToken(true)));
                escapeButtonPressed = (Buttons.ButtonStates.Escape == Buttons.GetKeypress(new CancellationToken(true)));
            }

            if (escapeButtonPressed)
            {
                return;
            }

            if (enterButtonPressed)
            {
                LcdConsole.Clear();
                LcdConsole.WriteLine("*****************************");
                LcdConsole.WriteLine("*                           *");
                LcdConsole.WriteLine("*      SmallRobots.it       *");
                LcdConsole.WriteLine("*                           *");
                LcdConsole.WriteLine("*     Formula Ev3  1.0      *");
                LcdConsole.WriteLine("*                           *");
                LcdConsole.WriteLine("*                           *");
                LcdConsole.WriteLine("*        Starting....       *");
                LcdConsole.WriteLine("*                           *");
                LcdConsole.WriteLine("*                           *");
                LcdConsole.WriteLine("*****************************");

                // Reset tachos
                steeringEngine.ResetTacho();

                // Acually starts the robot
                TaskScheduler.Start();
            }
        }
        #endregion

        #region Private methods
        #endregion
    }

    /// <summary>
    /// Periodic Task that updates message on the LCD
    /// </summary>
    public class LCDUpdateTask : PeriodicTask
    {
        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public LCDUpdateTask () : base()
        {
            // Set the periodic Action
            Action = OnTimer;

            // Set the period
            Period = 100;

            // Erase the LCD
            LcdConsole.Clear();
        }
        #endregion

        #region Private Methods
        private void OnTimer(Robot robot)
        {
            switch (((FormulaEv3)robot).direction)
            {
                case Direction.Stop:
                    // LcdConsole.WriteLine("Stop!");
                    break;
                case Direction.Beacon_ON:
                    // LcdConsole.WriteLine("Beacon ON");
                    break;
                case Direction.Left_Backward:
                    // LcdConsole.WriteLine("Left Backward");
                    break;
                case Direction.Left_Forward:
                    // LcdConsole.WriteLine("Left Forward");
                    break;
                case Direction.Right_Backward:
                    // LcdConsole.WriteLine("Right Backward");
                    break;
                case Direction.Right_Forward:
                    // LcdConsole.WriteLine("Right Forward");
                    break;
                case Direction.Straight_Backward:
                    // LcdConsole.WriteLine("Straight Backward");
                    break;
                case Direction.Straight_Forward:
                    // LcdConsole.WriteLine("Straight Forward");
                    break;
            }
        }
        #endregion
    }

    /// <summary>
    /// Periodic Task that receives commands from the Ev3 IR Remote
    /// </summary>
    public class IRRemoteTask : PeriodicTask
    {
        #region Fields
        /// <summary>
        /// Last command received from the Ev3 IR Remote
        /// </summary>
        byte remoteCommand;

        bool beaconActivated; 
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public IRRemoteTask () : base()
        {
            // Fields initialization
            remoteCommand = 0;
            beaconActivated = false;

            // Set the action
            Action = OnTimer;

            // Set the period
            Period = 100;
        }
        #endregion

        #region Private methods
        private void OnTimer(Robot robot)
        {
            if (beaconActivated && ((FormulaEv3)robot).irSensor.ReadBeaconLocation().Distance > -100)
            {
                // Don't change Ev3 IR Sensor mode
                return;
            }
            else
            { 
                // Ev3 IR Sensor mode can be changed because it's not detected anymore
                beaconActivated = false;
            }

            remoteCommand = ((FormulaEv3)robot).irSensor.ReadRemoteCommand();
            switch (remoteCommand)
            {
                case 0:
                    ((FormulaEv3)robot).direction = Direction.Stop;
                    break;
                case 1:
                    ((FormulaEv3)robot).direction = Direction.Left_Forward;
                    break;
                case 3:
                    ((FormulaEv3)robot).direction = Direction.Right_Forward;
                    break;
                case 5:
                    ((FormulaEv3)robot).direction = Direction.Straight_Forward;
                    break;
                case 2:
                    ((FormulaEv3)robot).direction = Direction.Left_Backward;
                    break;
                case 4:
                    ((FormulaEv3)robot).direction = Direction.Right_Backward;
                    break;
                case 8:
                    ((FormulaEv3)robot).direction = Direction.Straight_Backward;
                    break;
                case 9:
                    ((FormulaEv3)robot).direction = Direction.Beacon_ON;
                    beaconActivated = true;
                    break;
                default:
                    ((FormulaEv3)robot).direction = Direction.Stop;
                    break;
            }
        }
        #endregion
    }

    /// <summary>
    /// Periodic Task that checks for keyboards
    /// </summary>
    public class KeyboardTask : PeriodicTask
    {
        #region Constrcuctors
        public KeyboardTask () : base()
        {
            // Set the Action
            Action = OnTimer;

            // Set the period
            Period = 500;
        }
        #endregion

        #region Private methods
        private void OnTimer(Robot robot)
        {
            if (Buttons.ButtonStates.Escape == Buttons.GetKeypress(new CancellationToken(true)))
            {
                ((FormulaEv3)robot).TaskScheduler.Stop();

                // Shutdown
                Buttons.LedPattern(0);
                ((FormulaEv3)robot).leftEngine.SetPower(0);
                ((FormulaEv3)robot).rightEngine.SetPower(0);
                ((FormulaEv3)robot).steeringEngine.SetPower(0);
            }
        }
        #endregion
    }

    /// <summary>
    /// Periodic Task that drives the Formula Ev3 racing car
    /// </summary>
    public class DriveTask : PeriodicTask
    {
        #region Fields
        Direction previousDirection;
        #endregion

        #region Constructors
        public DriveTask () : base ()
        {
            // Set the action
            Action = OnTimer;

            // Set the Period
            Period = 100;
        }
        #endregion

        #region Private methods
        private void OnTimer(Robot robot)
        {
            // Adjust the LED Pattern
            if (((FormulaEv3)robot).direction == Direction.Stop)
            {
                Buttons.LedPattern(3);
            }
            else if (((FormulaEv3)robot).direction == Direction.Beacon_ON)
            {
                Buttons.LedPattern(2);
            }
            else
            {
                Buttons.LedPattern(1);
            }

            // Move the Formula Ev3
            switch (((FormulaEv3)robot).direction)
            {
                case Direction.Beacon_ON :
                    // Read the beacon distance and location
                    BeaconLocation bl = ((FormulaEv3)robot).irSensor.ReadBeaconLocation();
                    LcdConsole.WriteLine("Loc: " + bl.Location.ToString());
                    LcdConsole.WriteLine("Dis: " + bl.Distance.ToString());
                    ((FormulaEv3)robot).commandedSteeringAngle = - 3.0f * bl.Location;
                    if (bl.Distance >= 10)
                    {
                        ((FormulaEv3)robot).leftEngine.SetPower(-50);
                        ((FormulaEv3)robot).rightEngine.SetPower(-50);
                    }
                    else
                    {
                        // Too close
                        ((FormulaEv3)robot).leftEngine.SetPower(0);
                        ((FormulaEv3)robot).rightEngine.SetPower(0);
                    }

                    break;
                case Direction.Straight_Forward :
                    if (previousDirection != Direction.Straight_Forward)
                    {
                        previousDirection = Direction.Straight_Forward;
                        ((FormulaEv3)robot).leftEngine.SetPower(-100);
                        ((FormulaEv3)robot).rightEngine.SetPower(-100);
                        ((FormulaEv3)robot).commandedSteeringAngle = 0;
                    }
                    break;
                case Direction.Straight_Backward:
                    if (previousDirection != Direction.Straight_Backward)
                    {
                        previousDirection = Direction.Straight_Backward;
                        ((FormulaEv3)robot).leftEngine.SetPower(100);
                        ((FormulaEv3)robot).rightEngine.SetPower(100);
                        ((FormulaEv3)robot).commandedSteeringAngle = 0;
                    }
                    break;
                case Direction.Left_Forward :
                    if (previousDirection != Direction.Left_Forward)
                    {
                        previousDirection = Direction.Left_Forward;
                        ((FormulaEv3)robot).leftEngine.SetPower(-80);
                        ((FormulaEv3)robot).rightEngine.SetPower(-100);
                        ((FormulaEv3)robot).commandedSteeringAngle = 40;
                    }
                    break;
                case Direction.Right_Forward:
                    if (previousDirection != Direction.Right_Forward)
                    {
                        previousDirection = Direction.Right_Forward;
                        ((FormulaEv3)robot).leftEngine.SetPower(-100);
                        ((FormulaEv3)robot).rightEngine.SetPower(-80);
                        ((FormulaEv3)robot).commandedSteeringAngle = -30;
                    }
                    break;
                case Direction.Left_Backward:
                    if (previousDirection != Direction.Left_Backward)
                    {
                        previousDirection = Direction.Left_Backward;
                        ((FormulaEv3)robot).leftEngine.SetPower(80);
                        ((FormulaEv3)robot).rightEngine.SetPower(100);
                        ((FormulaEv3)robot).commandedSteeringAngle = 40;
                    }
                    break;
                case Direction.Right_Backward:
                    if (previousDirection != Direction.Right_Backward)
                    {
                        previousDirection = Direction.Right_Backward;
                        ((FormulaEv3)robot).leftEngine.SetPower(100);
                        ((FormulaEv3)robot).rightEngine.SetPower(80);
                        ((FormulaEv3)robot).commandedSteeringAngle = -30;
                    }
                    break;
                default:
                    ((FormulaEv3)robot).leftEngine.SetPower(0);
                    ((FormulaEv3)robot).rightEngine.SetPower(0);
                    ((FormulaEv3)robot).commandedSteeringAngle = 0;
                    break;
            }
        }
        #endregion
    }

    /// <summary>
    /// Periodic Task that steers the front wheels
    /// </summary>
    public class SteeringTask : PIDController
    {
        #region Constructor
        public SteeringTask () : base(50)
        {
            // Initialize the PID Parameters
            MinPower = -50;
            MaxPower = 50;
            Kp = 2;
            Ki = 0.1f;
            Kd = 0;
            LowPassConstant = 0.9f;
        }
        #endregion

        #region Protected methods
        protected override void PIDAlgorithm(Robot robot)
        {
            ProcessVariableSignal = ((FormulaEv3)robot).steeringEngine.GetTachoCount();
            SetPoint = ((FormulaEv3)robot).commandedSteeringAngle;

            base.PIDAlgorithm(robot);

            ((FormulaEv3)robot).steeringEngine.SetPower(OutputSignal);

        }
        #endregion
    }

}
