namespace RebornFrame.Core.Math
{
      public struct Matrix3x3
    {
        public RebornFloat A00;
        public RebornFloat A01;
        public RebornFloat A02;
        public RebornFloat A10;
        public RebornFloat A11;
        public RebornFloat A12;
        public RebornFloat A20;
        public RebornFloat A21;
        public RebornFloat A22;

        public Matrix3x3(RebornFloat a00, RebornFloat a01, RebornFloat a02, RebornFloat a10, RebornFloat a11, RebornFloat a12, RebornFloat a20, RebornFloat a21, RebornFloat a22)
        {
            A00 = a00;
            A01 = a01;
            A02 = a02;
            A10 = a10;
            A11 = a11;
            A12 = a12;
            A20 = a20;
            A21 = a21;
            A22 = a22;
        }
        public static Matrix3x3 One => new Matrix3x3(
            1.RebornFloat(), 0.RebornFloat(), 0.RebornFloat(),
            0.RebornFloat(), 1.RebornFloat(), 0.RebornFloat(), 
            0.RebornFloat(), 0.RebornFloat(), 1.RebornFloat()
        );


        #region 操作符

        public static RebornV3 operator *(Matrix3x3 m, RebornV3 v)
        {

            return new RebornV3(
                m.A00 * v.x + m.A01 * v.y+m.A02*v.z,
                m.A10 * v.x + m.A11 * v.y+m.A12*v.z,
                m.A20 * v.x + m.A21 * v.y+m.A22*v.z
                );
        }

        public static Matrix3x3 operator /(RebornFloat f, Matrix3x3 m)
        {
            return new Matrix3x3(
                m.A00 == 0? 0.RebornFloat() :f/m.A00, m.A10 == 0? 0.RebornFloat() :f/m.A10, m.A20 == 0? 0.RebornFloat() :f/m.A20,
                m.A01 == 0? 0.RebornFloat() :f/m.A01, m.A11 == 0? 0.RebornFloat() :f/m.A11, m.A21 == 0? 0.RebornFloat() :f/m.A21,
                m.A02 == 0? 0.RebornFloat() :f/m.A02, m.A12 == 0? 0.RebornFloat() :f/m.A12, m.A22 == 0? 0.RebornFloat() :f/m.A22
            );
        }
        public static Matrix3x3 operator *(Matrix3x3 m, Matrix3x3 v)
        {
            return new Matrix3x3(
                m.A00 * v.A00 + m.A01 * v.A10 + m.A02 * v.A20, m.A00 * v.A01 + m.A01 * v.A11 + m.A02 * v.A21, m.A00 * v.A02 + m.A01 * v.A12 + m.A02 * v.A22,
                m.A10 * v.A00 + m.A11 * v.A10 + m.A12 * v.A20, m.A10 * v.A01 + m.A11 * v.A11 + m.A12 * v.A21, m.A10 * v.A02 + m.A11 * v.A12 + m.A12 * v.A22,
                m.A20 * v.A00 + m.A21 * v.A10 + m.A22 * v.A20, m.A20 * v.A01 + m.A21 * v.A11 + m.A22 * v.A21, m.A20 * v.A02 + m.A21 * v.A12 + m.A22 * v.A22
            );
        }

        public static Matrix3x3 operator +(Matrix3x3 m, Matrix3x3 v)
        {
            return new Matrix3x3(
                m.A00 + v.A00, m.A01 + v.A01, m.A02 + v.A02,
                m.A10 + v.A10, m.A11 + v.A11, m.A12 + v.A12,
                m.A20 + v.A20, m.A21 + v.A21, m.A22 + v.A22
            );
        }   
        public static Matrix3x3 operator -(Matrix3x3 m, Matrix3x3 v)
        {
            return new Matrix3x3(
                m.A00 - v.A00, m.A01 - v.A01, m.A02 - v.A02,
                m.A10 - v.A10, m.A11 - v.A11, m.A12 - v.A12,
                m.A20 - v.A20, m.A21 - v.A21, m.A22 - v.A22
            );
        }  
        #endregion
        
        /// <summary>
        /// 转置
        /// </summary>
        public Matrix3x3 transpose
        {
            get => new Matrix3x3(
                A00, A10, A20,
                A01, A11, A21,
                A02, A12, A22
            );
        } 
        
        public static Matrix3x3 FromEulaAngel(RebornV3 eulaRotation)
        {
            RebornFloat ax = eulaRotation.x * RebornMath.PI / 180.RebornFloat();
            RebornFloat ay = eulaRotation.y * RebornMath.PI / 180.RebornFloat();
            RebornFloat az = eulaRotation.z * RebornMath.PI / 180.RebornFloat();
            RebornFloat cx = RebornMath.Cos(ax);
            RebornFloat sx = RebornMath.Sin(ax);
            RebornFloat cy = RebornMath.Cos(ay);
            RebornFloat sy = RebornMath.Sin(ay);
            RebornFloat cz = RebornMath.Cos(az);
            RebornFloat sz = RebornMath.Sin(az);
            return new Matrix3x3(
                cy * cz,          -cy*sz,           sy,
                sx*sy*cz+cx*sz,   -sx*sy*sz+cx*cz,  -sx*cy,
                -cx*sy*cz+sx*sz,  cx*sy*sz+sx*cz,   cx*cy
            );
        }
        
    }
}