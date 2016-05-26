using System;
using Robocode;
using Robocode.Util;
using System.Drawing;
using System.Numerics;

namespace C2C
{
    internal class Algorithm
    {
        private AdvancedRobot _self;
        private Enemy _target;

        private const double ShootDistance = 400;

        public Algorithm(AdvancedRobot robot)
        {
            _self = robot;
        }

        internal void SetScannedRobot(ScannedRobotEvent evnt)
        {
            if (_target == null)
                _target = new Enemy(evnt.Name);
            if (!_target.Name.Equals(evnt.Name, StringComparison.OrdinalIgnoreCase))
                return;
            _target.UpdateState(evnt, _self.X, _self.Y);

            var offset = Rectify(_target.Bearing + _self.Heading - _self.RadarHeading) * 1.5;
            _self.SetTurnRadarRight(offset);

            var power = GetGunPower();


            offset = Rectify(_target.Bearing + _self.Heading - _self.GunHeading);
//            offset = AdjustGunOffset(offset, power);
            _self.SetTurnGunRight(offset);

            _self.SetFire(power);

//            var x = random.Next(_self.SentryBorderSize, (int)_self.BattleFieldWidth - _self.SentryBorderSize);
//            var y = random.Next(_self.SentryBorderSize, (int)_self.BattleFieldHeight - _self.SentryBorderSize);
//            var x = random.NextDouble() * (_self.BattleFieldWidth - _self.SentryBorderSize) + _self.SentryBorderSize / 2;
//            var y = random.NextDouble() * (_self.BattleFieldHeight - _self.SentryBorderSize) + _self.SentryBorderSize / 2;
//            var distance = Math.Sqrt((x - _self.X) * (x - _self.X) + (y - _self.Y) * (y - _self.Y));
//            double angle = Math.Atan((x - _self.X) / (y - _self.Y)) * 180 / Math.PI - _self.Heading;

            var distance = random.Next(20, 40);
            double angle = GetMoveAngle(distance);
            //var tolerance = _self.SentryBorderSize;
            //if (_target.Distance < tolerance &&
            //    (_self.X > tolerance && _self.X < _self.BattleFieldWidth - tolerance) &&
            //    (_self.Y > tolerance && _self.Y < _self.BattleFieldHeight - tolerance))
            //    angle = Rectify(angle + 90);
            if (_self.TurnRemaining < 1)
                _self.SetTurnRight(Rectify(angle));
            if (_self.DistanceRemaining < 10)
                _self.SetAhead(distance);
        }
        Random random = new Random(DateTime.UtcNow.Millisecond);

        public bool LocationInCenterZone(double x, double y)
        {
            if (x < _self.SentryBorderSize || x > _self.BattleFieldWidth - _self.SentryBorderSize)
                return false;
            if (y < _self.SentryBorderSize || y > _self.BattleFieldHeight - _self.SentryBorderSize)
                return false;
            return true;
        }

        public double GetDistance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt((x1-x2)*(x1-x2)+(y1-y2)*(y1-y2));
        }

        private double GetMoveAngle(double distance)
        {
            const int candidates = 10;
            //var angles = new double[candidates];

            //for (int i = 0; i < candidates; ++i)
            //{
            //    angles[i] = Rectify(random.Next(-45, 45) + _target.Bearing);
            //    if (_target.Distance < _self.Width * 3 && LocationInCenterZone(_self.X, _self.Y))
            //        angles[i] += 180;
            //}
            //int choose = 0;
            //double farest = 0.0;
            //for(int i = 1; i < candidates; ++i)
            //{
            //    var target_x = _self.X + distance * Math.Cos(angles[i] * Math.PI / 180);
            //    var target_y = _self.Y + distance * Math.Sin(angles[i] * Math.PI / 180);
            //    var d_t = (target_x - _target.X) * (target_x - _target.X) + (target_y - _target.Y) * (target_y - _target.Y);
            //    if (d_t > farest)
            //        continue;
            //    farest = d_t;
            //    choose = i;
            //}
            //return angles[choose];

            var xs = new double[candidates];
            var ys = new double[candidates];
            for(int i = 0; i < candidates; ++i)
            {
                xs[i] = random.NextDouble() * (_self.BattleFieldWidth - 4 * _self.Width) + 2 * _self.Width;
                ys[i] = random.NextDouble() * (_self.BattleFieldHeight - 4 * _self.Width) + 2 * _self.Width;
            }
            int choose = random.Next(0, candidates - 1);
            //double farest = 0;
            //for (int i = 0; i < candidates; ++i)
            //{
            //    var f = GetDistance(xs[i], ys[i], _self.X, _self.Y) + GetDistance(xs[i], ys[i], _target.X, _target.Y);
            //    if (f < farest)
            //        continue;
            //    farest = f;
            //    choose = i;
            //}
            var angle = Math.Atan((xs[choose] - _self.X) / (ys[choose] - _self.Y)) * 180 / Math.PI;
            return angle - _self.Heading;
        }

        internal void RemoveDeadRobot(string name)
        {
            if (_target == null || _target.Name != name)
                return;
            _target = null;
        }

        internal void Step()
        {
            if (_target != null)
                return;
            _self.SetTurnRadarRight(20);
        }

        private double GetGunPower()
        {
            var max = Rules.MAX_BULLET_POWER;
            var min = Rules.MIN_BULLET_POWER;
            var max_d = 400;
            var min_d = 10;

            var result = max - (max - min) / (max_d - min_d) * (_target.Distance - min_d);
            if (result > max)
                result = max;
            if (result < min)
                result = min;
            return result;
        }

        private double AdjustGunOffset(double offset, double power)
        {
            var speed = Rules.GetBulletSpeed(power);
            var move = _target.Distance / speed * _target.Velocity;

            var v1 = new Complex(_target.Distance * Math.Cos(_target.Bearing),
                _target.Distance * Math.Sin(_target.Bearing));
            var v2 = new Complex(move * Math.Cos(_target.Heading),
                move * Math.Sin(_target.Heading));
            var v3 = v1 + v2;

            if (v3.Real == 0)
                return offset + 90 - _target.Bearing;
            return Math.Atan(v3.Imaginary / v3.Real) + offset;
        }

        private double Rectify(double angle)
        {
            return Utils.NormalRelativeAngleDegrees(angle);
            if (angle < -180)
                angle += 360;
            if (angle > 180)
                angle -= 360;
            return angle;
        }
    }
}