namespace RebornFrame.Core.Math
{
    public struct RebornQuaternion
    {
        public RebornFloat W { get; set; }
        public RebornFloat X { get; set; }
        public RebornFloat Y { get; set; }
        public RebornFloat Z { get; set; }

        public RebornQuaternion(RebornFloat x, RebornFloat y, RebornFloat z,RebornFloat w)
        {
            W = w;
            X = x;
            Y = y;
            Z = z;
        }

        public static RebornQuaternion Identity => new RebornQuaternion(1.RebornFloat(), 0.RebornFloat(), 0.RebornFloat(), 0.RebornFloat());

        public RebornQuaternion Conjugate()
        {
            return new RebornQuaternion(-X, -Y, -Z, W);
        }

        public RebornFloat Magnitude()
        {
            return RebornMath.Sqrt(W * W + X * X + Y * Y + Z * Z);
        }

        public RebornQuaternion Normalize()
        {
            RebornFloat magnitude = Magnitude();
            return new RebornQuaternion(X / magnitude, Y / magnitude, Z / magnitude,W / magnitude);
        }
        
        public static bool operator ==(RebornQuaternion q1, RebornQuaternion q2)
        {
            return q1.X == q2.X && q1.Y == q2.Y && q1.Z == q2.Z && q1.W == q2.W;
        }

        public static bool operator !=(RebornQuaternion q1, RebornQuaternion q2)
        {
            return !(q1 == q2);
        }

        public static RebornQuaternion operator *(RebornQuaternion q1, RebornQuaternion q2)
        {
            RebornFloat w = q1.W * q2.W - q1.X * q2.X - q1.Y * q2.Y - q1.Z * q2.Z;
            RebornFloat x = q1.W * q2.X + q1.X * q2.W + q1.Y * q2.Z - q1.Z * q2.Y;
            RebornFloat y = q1.W * q2.Y - q1.X * q2.Z + q1.Y * q2.W + q1.Z * q2.X;
            RebornFloat z = q1.W * q2.Z + q1.X * q2.Y - q1.Y * q2.X + q1.Z * q2.W;

            return new RebornQuaternion(x, y, z, w);
        }

        public static RebornV3 operator *(RebornV3 point, RebornQuaternion rotation)
        {
            RebornQuaternion p = new RebornQuaternion(point.x, point.y, point.z,0.RebornFloat());
            RebornQuaternion result = rotation * p * rotation.Conjugate();
            return new RebornV3(result.X, result.Y, result.Z);
        }

        // 从欧拉角（Yaw、Pitch、Roll）创建四元数
        public static RebornQuaternion FromEulerAngles(RebornV3 euler)
        {
            RebornFloat yaw = euler.x;
            RebornFloat pitch = euler.y;
            RebornFloat roll = euler.z;

            RebornFloat cy = RebornMath.Cos(yaw * 0.5f.RebornFloat());
            RebornFloat sy = RebornMath.Sin(yaw * 0.5f.RebornFloat());
            RebornFloat cp = RebornMath.Cos(pitch * 0.5f.RebornFloat());
            RebornFloat sp = RebornMath.Sin(pitch * 0.5f.RebornFloat());
            RebornFloat cr = RebornMath.Cos(roll * 0.5f.RebornFloat());
            RebornFloat sr = RebornMath.Sin(roll * 0.5f.RebornFloat());

            RebornFloat w = cy * cp * cr + sy * sp * sr;
            RebornFloat x = cy * cp * sr - sy * sp * cr;
            RebornFloat y = sy * cp * sr + cy * sp * cr;
            RebornFloat z = sy * cp * cr - cy * sp * sr;

            return new RebornQuaternion(x, y, z, w);
        }
    }

}