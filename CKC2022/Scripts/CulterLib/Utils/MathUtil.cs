using System;

namespace CulterLib.Utils
{
    public static class MathUtil
    {
        #region Value
        private static Random m_Random = new Random((int)DateTime.UtcNow.ToBinary());
        #endregion

        #region Function
        /// <summary>
        /// 랜덤한 long값을 얻습니다.
        /// </summary>
        /// <returns></returns>
        public static long GetRandomLong()
        {
            long l = m_Random.Next();
            l = l << 32;
            l = l | (long)m_Random.Next();
            return l;
        }
        /// <summary>
        /// 랜덤한 int값을 얻습니다.
        /// UnityEngine의 Random을 사용하지 않기 때문에 변수 초기화 시점에서도 사용 가능합니다.
        /// </summary>
        /// <returns></returns>
        public static int GetRandomInt()
        {
            return m_Random.Next();
        }
        /// <summary>
        /// 랜덤한 byte값을 얻습니다.
        /// UnityEngine의 Random을 사용하지 않기 때문에 변수 초기화 시점에서도 사용 가능합니다.
        /// </summary>
        /// <returns></returns>
        public static byte[] GetRandomBytes(int _len)
        {
            byte[] b = new byte[_len];
            m_Random.NextBytes(b);
            return b;
        }
        /// <summary>
        /// 랜덤한 byte값을 얻습니다.
        /// </summary>
        /// <param name="_bytes"></param>
        /// <returns></returns>
        public static bool TryGetRandomBytes(ref byte[] _bytes)
        {
            try
            {
                m_Random.NextBytes(_bytes);
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion
    }
}