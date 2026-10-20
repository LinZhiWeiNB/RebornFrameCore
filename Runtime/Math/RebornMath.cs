using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace RebornFrame.Core
{
    public class RebornMath
    {
        public static long PRECISION = 1000; //RebornMath整体精度，精度越大，可用范围越小。

        private static int _RandomSeed = 0;
        
        private static Random _random;

        public static RebornFloat PI = 3.1415926f.RebornFloat(); 
        
        public static int RandomSeed {
            get
            {
                return _RandomSeed;
            }
            set
            {
                _RandomSeed = value;
                _random = new Random(RandomSeed);
            }
        }


        public static RebornFloat Cos(RebornFloat value)
        {
            RebornFloat result = 1.RebornFloat(); // 初始值为1，对应于x^0/0!
            RebornFloat term = 1.RebornFloat();   // 初始化第一项

            for (int i = 1; i <= 10; i++) // 这里计算前10项，你可以根据需要增加项数
            {
                term *= -value * value / (2.RebornFloat() * i * (2.RebornFloat() * i - 1.RebornFloat())); // 计算下一项
                result += term; // 将下一项加到结果中
            }

            return result;
        }
        
        public static RebornFloat Sin(RebornFloat value)
        {
            RebornFloat result = value; // 初始值为x，对应于x^1/1!
            RebornFloat term = value;   // 初始化第一项

            for (int i = 1; i <= 10; i++) // 这里计算前10项，你可以根据需要增加项数
            {
                term *= -value * value / ((2.RebornFloat() * i + 1.RebornFloat()) * (2.RebornFloat() * i)); // 计算下一项
                result += term; // 将下一项加到结果中
            }

            return result;
        }
            
        public static RebornFloat Abs(RebornFloat value)
        {
            if (value < 0)
            {
                return -value;
            }
            else
                return value;
        }

        public static RebornFloat SinFromCos(RebornFloat cos)
        {
            RebornFloat vlaue = cos * cos / 1.RebornFloat();
            return Sqrt(1.RebornFloat() - vlaue);
        }

        public static RebornFloat Max(RebornFloat v1,RebornFloat v2)
        {
            return v1>v2?v1:v2;
        }
        
        public static RebornFloat Min(RebornFloat v1,RebornFloat v2)
        {
            return v1<v2?v1:v2;
        }
        
        /// <summary>
        /// 位移
        /// </summary>
        /// <param name="origin">当前位置</param>
        /// <param name="target">目标位置</param>
        /// <param name="speed">每秒最大位移距离（x1000）</param>
        /// <param name="deltaMillionSeconds">用时</param>
        /// <returns></returns>
        public static RebornV3 MoveToWards(RebornV3 origin, RebornV3 target, RebornFloat speed, RebornFloat deltaTime)
        {
            RebornV3 dir = target - origin;

            RebornV3 dirNorm = NormLize(dir);

            if (dirNorm.x == 0 && dirNorm.y == 0 && dirNorm.z == 0)
                return target;
        
            dir = dirNorm * speed * deltaTime;

            RebornV3 outOrigin = origin + dir;

            //判断是否超过

            dir = target - outOrigin;
            if (dir.x * dirNorm.x < 0 || dir.y * dirNorm.y < 0 || dir.z * dirNorm.z< 0)
                return target;
            return outOrigin;
        }

        public static RebornFloat Clamp(RebornFloat value, RebornFloat min, RebornFloat max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }

        public static RebornFloat Sqrt(RebornFloat x)
        {
            long xLong = x.Value * PRECISION;
            return new RebornFloat(_Sqrt(xLong));
        }
        
        static long _Sqrt(long x)
        {
            long temp = 0;
            int v_bit = 31;
            long n = 0;
            long b = 1L << v_bit;

            if (x <= 1)
                return x;

            do
            {
                temp = ((n << 1) + b) << (v_bit--);
                if (x >= temp)
                {
                    n += b;
                    x -= temp;
                }
            } while ((b >>= 1) != 0);

            return n;
        }
        
        public static RebornFloat Dot(RebornV3 value1,RebornV3 value2)
        {
            return value1.x*value2.x + value1.y*value2.y  + value1.z*value2.z ;
        }

        public static RebornV3 Cross(RebornV3 value1,RebornV3 value2)
        {
            var result = _Cross(value1, value2);
            if (result == RebornV3.zero)
                result = _Cross(value1.normalized, value2.normalized);
            return result;
        }
        
        static RebornV3 _Cross(RebornV3 value1,RebornV3 value2)
        {
            return new RebornV3(value1.y*value2.z - value1.z*value2.y,
                value1.z*value2.x - value1.x*value2.z,
                value1.x*value2.y - value1.y*value2.x);
        }
        public static int FloorToInt(RebornFloat value)
        {
            RebornFloat v = value;
            long longValue = v.Value;
            if (longValue % RebornMath.PRECISION > 0)
            {
                return (int)(longValue / RebornMath.PRECISION) + 1;
            }
            else
            {
                return (int)(longValue / RebornMath.PRECISION);
            }
        }
        /// <summary>
        /// 获取随机数 [min,max)
        /// </summary>
        /// <param name="min">包含最低值</param>
        /// <param name="max">不包含最大值</param>
        /// <returns></returns>
        public static int NextRandom(int min, int max)
        {
            var nextRandom = _random.Next(min, max);
#if LOGIC_LOG
            Debug.Log(nextRandom.ToString());
#endif
            return nextRandom;
        }
        
        
        public static RebornFloat AngleCos(RebornV3 v1, RebornV3 v2)
        {

            RebornFloat v1SqrtM = SqrMagnitude(v1);
            RebornFloat v2SqrtM = SqrMagnitude(v2);
            RebornFloat num = Sqrt(v1SqrtM * v2SqrtM);

            RebornFloat dotValue = Dot(v1, v2);
            RebornFloat divide = dotValue / num;
            RebornFloat clamp = Clamp(divide, -1.RebornFloat(), 1.RebornFloat());
            return clamp;
        }
        
        public static RebornFloat Magnitude(RebornV3 value)
        {
            return Sqrt(SqrMagnitude(value));
        }
        
        public static RebornFloat Length(RebornV3 value)
        {
            return Magnitude(value);
        }
        
        public static RebornV3 NormLize(RebornV3 value)
        {
            RebornFloat length = Length(value);
            if (length == 0)
                return new RebornV3(0, 0, 0);
            else
                return value / length;
        }
        
        public static RebornFloat SqrMagnitude(RebornV3 value)
        {
            return value.x*value.x + value.y*value.y  + value.z*value.z ;
        }
        
        public static RebornFloat Distance(RebornV3 v1, RebornV3 v2)
        {
            RebornV3 dir = v1 - v2;
            return Length(dir);
        }
    }

    public struct RebornV3
    {

        public RebornFloat x;

        public RebornFloat y;

        public RebornFloat z;

        public Vector3 vector3 => new Vector3(x.Float, y.Float, z.Float);
        
        public RebornV3(RebornFloat _x, RebornFloat _y, RebornFloat _z)
        {
            x = _x;
            y = _y;
            z =_z;
        }

        public RebornV3(int x, int y, int z)
        {
            this.x = x.RebornFloat();
            this.y = y.RebornFloat();
            this.z = z.RebornFloat();
        }

        #region 快捷数值

        public static RebornV3 left
        {
            get => new RebornV3(-1, 0, 0);
        }

        public static RebornV3 right
        {
            get => new RebornV3(1, 0, 0);
        }

        public static RebornV3 up
        {
            get => new RebornV3(0, 1, 0);
        }

        public static RebornV3 down
        {
            get => new RebornV3(0, -1, 0);
        }

        public static RebornV3 forward
        {
            get => new RebornV3(0, 0, 1);
        }

        public static RebornV3 back
        {
            get => new RebornV3(0, 0, -1);
        }

        public static RebornV3 zero
        {
            get => new RebornV3(0, 0, 0);
        }

        public static RebornV3 one
        {
            get => new RebornV3(1, 1, 1);
        }

        #endregion

        public static RebornV3 operator + (RebornV3 v1, RebornV3 v2)
        {
            return new RebornV3(v1.x+v2.x,v1.y+v2.y,v1.z+v2.z);
        }
        public static RebornV3 operator - (RebornV3 v1, RebornV3 v2)
        {
            return new RebornV3(v1.x-v2.x,v1.y-v2.y,v1.z-v2.z);
        }
        public static RebornV3 operator *(RebornV3 v1, RebornFloat v2)
        {
            return new RebornV3(v1.x*v2,v1.y*v2,v1.z*v2);
        }
        public static RebornV3 operator *(RebornFloat v2, RebornV3 v1)
        {
            return new RebornV3(v1.x*v2,v1.y*v2,v1.z*v2);
        }
        public static RebornV3 operator /(RebornV3 v1, RebornFloat v2)
        {
            return new RebornV3(v1.x/v2,v1.y/v2,v1.z/v2);
        }

        public static RebornV3 operator -(RebornV3 v1)
        {
            return new RebornV3(-v1.x,-v1.y,-v1.z);
        }
        
        public static bool operator ==(RebornV3 v1, RebornV3 v2)
        {
            return v1.x == v2.x && v1.y == v2.y  && v1.z == v2.z ;
        }
        public static bool operator !=(RebornV3 v1, RebornV3 v2)
        {
            return v1.x != v2.x || v1.y != v2.y  || v1.z != v2.z ;
        }
        
        public override string ToString()
        {
            return String.Format("RebornV3({0},{1},{2})", x, y, z);
        }
        
        public static RebornFloat AngleCos(RebornV3 v1, RebornV3 v2)
        {
            return RebornMath.AngleCos(v1, v2);
        }

        public static RebornFloat Distance(RebornV3 v1, RebornV3 v2)
        {
            return RebornMath.Distance(v1, v2);
        }
        
        public RebornFloat magnitude => RebornMath.Magnitude(this);
        public RebornFloat length => RebornMath.Length(this);
        public RebornFloat sqrMagnitude => RebornMath.SqrMagnitude(this);
        public RebornV3 normalized {
            get
            {
                // if ((x == 0 || x >= RebornMath.PRECISION || x <= -RebornMath.PRECISION) &&
                //     (y == 0 || y >= RebornMath.PRECISION || y <= -RebornMath.PRECISION) &&
                //     (z == 0 || z >= RebornMath.PRECISION || z <= -RebornMath.PRECISION))
                //     return RebornMath.NormLize(this);
                // var newValue = this * new RebornFloat(RebornMath.PRECISION * RebornMath.PRECISION);
                return RebornMath.NormLize(this);

            }
        }
        public RebornFloat cosRightAngle => RebornMath.AngleCos(this, right);
        public RebornFloat cosLeftAngle => RebornMath.AngleCos(this, left);
    }


    public struct RebornFloat
    {
        public long Value;

        public short miniValue => (short)(Value / 10);

        public float Float
        {
            get => ((float)Value / RebornMath.PRECISION);
        }

        public RebornFloat(int value)
        {
            Value = (long)value * RebornMath.PRECISION;
        }
        public RebornFloat(float value)
        {
            Value = (long)(value * RebornMath.PRECISION);
        }

        /// <summary>
        /// 数学库内部使用
        /// </summary>
        /// <param name="value"></param>
        public RebornFloat(long value)
        {
            Value = value;
        }


        #region 操作符


        public static RebornFloat operator /(RebornFloat v1, RebornFloat v2)
        {
            if (v2 == 0)
            {
                Debug.LogError("被除数不能为0！");
                return v1;
            }
            return new RebornFloat(v1.Value * RebornMath.PRECISION /  v2.Value);
        }

        public static RebornFloat operator *(RebornFloat v1, RebornFloat v2)
        {
            return new RebornFloat(v1.Value * v2.Value/ RebornMath.PRECISION);
        }
        
        public static RebornFloat operator *(int v1, RebornFloat v2)
        {
            return new RebornFloat(v1 * v2.Value);
        }
        
        public static RebornFloat operator *(RebornFloat v1, int v2)
        {
            return new RebornFloat(v1.Value * v2);
        }


        public static RebornFloat operator +(RebornFloat v1, RebornFloat v2)
        {
            return new RebornFloat(v1.Value + v2.Value);
        }


        public static RebornFloat operator -(RebornFloat v1, RebornFloat v2)
        {
            return new RebornFloat(v1.Value - v2.Value);
        }
        
        public static RebornFloat operator -(RebornFloat v1)
        {
            return new RebornFloat(-v1.Value);
        }
        public static bool operator >(RebornFloat v1, RebornFloat v2)
        {
            return v1.Value > v2.Value;
        }
        public static bool operator <(RebornFloat v1, RebornFloat v2)
        {
            return v1.Value < v2.Value;
        }

        public static bool operator >=(RebornFloat v1, RebornFloat v2)
        {
            return v1.Value >= v2.Value;
        }
        public static bool operator <=(RebornFloat v1, RebornFloat v2)
        {
            return v1.Value <= v2.Value;
        }

        public static bool operator ==(RebornFloat v1, RebornFloat v2)
        {
            return v1.Value == v2.Value;
        }

        public static bool operator !=(RebornFloat v1, RebornFloat v2)
        {
            return v1.Value != v2.Value;
        }

        public static bool operator >(RebornFloat v1, int v2)
        {
            return v1 > v2.RebornFloat();
        }
        public static bool operator <(RebornFloat v1, int v2)
        {
            return v1 < v2.RebornFloat();
        }

        public static bool operator >=(RebornFloat v1, int v2)
        {
            return v1 >= v2.RebornFloat();
        }
        public static bool operator <=(RebornFloat v1, int v2)
        {
            return v1 <= v2.RebornFloat();
        }

        public static bool operator ==(RebornFloat v1, int v2)
        {
            return v1 == v2.RebornFloat();
        }

        public static bool operator !=(RebornFloat v1, int v2)
        {
            return v1 != v2.RebornFloat();
        }

        public static bool operator >(RebornFloat v1, float v2)
        {
            return v1 > v2.RebornFloat();
        }
        public static bool operator <(RebornFloat v1, float v2)
        {
            return v1 < v2.RebornFloat();
        }

        public static bool operator >=(RebornFloat v1, float v2)
        {
            return v1 >= v2.RebornFloat();
        }
        public static bool operator <=(RebornFloat v1, float v2)
        {
            return v1 <= v2.RebornFloat();
        }

        #endregion


        #region 常量
        
        public static RebornFloat zero => new RebornFloat(0);
        public static RebornFloat one => new RebornFloat(1);
        #endregion
        
        public override string ToString()
        {
            return Float.ToString();
        }

    }


    public static class AdditionMethod
    {
        public static RebornFloat RebornFloat(this float v)
        {
            return new RebornFloat(v);
        }
        public static RebornFloat RebornFloat(this int v)
        {
            return new RebornFloat(v);
        }
        
        public static RebornFloat FromMiniValue(this short v)
        {
            RebornFloat f = new RebornFloat((long)(v * 10));
            return f;
        }
    }
}