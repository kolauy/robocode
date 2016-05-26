using Robocode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C2C
{
    public class Tailer : AdvancedRobot
    {
        private readonly Algorithm _algorithm;

        public Tailer()
        {
            _algorithm = new Algorithm(this);
        }

        public override void OnScannedRobot(ScannedRobotEvent evnt)
        {
            _algorithm.SetScannedRobot(evnt);
        }

        public override void OnRobotDeath(RobotDeathEvent evnt)
        {
            _algorithm.RemoveDeadRobot(evnt.Name);
        }

        public override void Run()
        {
            IsAdjustGunForRobotTurn = true;
            IsAdjustRadarForGunTurn = true;
            IsAdjustRadarForRobotTurn = true;

            while(true)
            {
                _algorithm.Step();
                Execute();
            }
        }
    }
}
