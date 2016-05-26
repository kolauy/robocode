using System;
using Robocode;

namespace C2C
{
    public class Enemy
    {
        public string Name { get; private set; }
        public double Distance { get; private set; }
        public double Bearing { get; private set; }
        public double Heading { get; private set; }
        public double Velocity { get; private set; }
        public double X { get; private set; }
        public double Y { get; private set; }

        public Enemy(string name)
        {
            Name = name == null ? string.Empty : name;
        }

        public void UpdateState(ScannedRobotEvent evnt, double myx, double myy)
        {
            Distance = evnt.Distance;
            Bearing = evnt.Bearing;
            Heading = evnt.Heading;
            Velocity = evnt.Velocity;
            X = myx + evnt.Distance * Math.Cos(evnt.BearingRadians);
            Y = myx + evnt.Distance * Math.Sin(evnt.BearingRadians);
        }
    }
}