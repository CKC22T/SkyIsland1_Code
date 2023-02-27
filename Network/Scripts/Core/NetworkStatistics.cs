using System;

namespace Network
{
    public class NetworkStatistics
    {
        // Total data sent/received amount
        public ulong TotalSentBytes => mTotalSentBytes;
        private ulong mTotalSentBytes;
        public ulong TotalReceivedBytes => mTotalReceivedBytes;
        private ulong mTotalReceivedBytes;
        public ulong TotalBytes => mTotalSentBytes + mTotalReceivedBytes;

        // Current bps
        private ulong mCurrentSent_bps;
        private ulong mCurrentReceived_bps;
        public ulong CurrentSent_bps
        {
            get
            {
                check_bpsResetTime();
                return mCurrentSent_bps;
            }
        }
        public ulong CurrentRecieved_bps
        {
            get
            {
                check_bpsResetTime();
                return mCurrentReceived_bps;
            }
        }
        public ulong Current_bps => CurrentSent_bps + CurrentRecieved_bps;

        // Last bps
        private ulong mLastSent_bps;
        private ulong mLastReceived_bps;
        public ulong LastSent_bps => mLastSent_bps;
        public ulong LastReceived_bps => mLastReceived_bps;
        public ulong LastTotal_bps => mLastSent_bps + mLastReceived_bps;

        // Measure time
        private int mLastSecond = 0;

        private object mLocker = new object();

        public NetworkStatistics()
        {
            mTotalSentBytes = 0;
            mTotalSentBytes = 0;

            mLastSecond = DateTime.Now.Second;
        }

        public void AddTotalSendBytes(int sendBytesCounts)
        {
            lock (mLocker)
            {
                check_bpsResetTime();

                if (sendBytesCounts < 0)
                {
                    return;
                }

                mTotalSentBytes += (ulong)sendBytesCounts;
                mCurrentSent_bps += (ulong)sendBytesCounts;
            }
        }

        public void AddTotalReceiveBytes(int receiveBytesCounts)
        {
            lock (mLocker)
            {
                check_bpsResetTime();

                if (receiveBytesCounts < 0)
                {
                    return;
                }

                mTotalReceivedBytes += (ulong)receiveBytesCounts;
                mCurrentReceived_bps += (ulong)receiveBytesCounts;
            }
        }

        private void check_bpsResetTime()
        {
            int currentSecond = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;

            if (mLastSecond < currentSecond)
            {
                mLastSecond = currentSecond;

                mLastSent_bps = mCurrentSent_bps;
                mLastReceived_bps = mCurrentReceived_bps;

                mCurrentSent_bps = 0;
                mCurrentReceived_bps = 0;
            }
        }

        public void Reset()
        {
            lock (mLocker)
            {
                mTotalSentBytes = 0;
                mCurrentSent_bps = 0;
                mTotalReceivedBytes = 0;
                mCurrentReceived_bps = 0;
            }
        }

        public override string ToString()
        {
            return $"Total [Sent : {mTotalSentBytes}][Received : {mTotalReceivedBytes}]\n" + 
                $"bps [Sent : {mCurrentSent_bps}][Received : {mCurrentReceived_bps}]";
        }
    }
}
