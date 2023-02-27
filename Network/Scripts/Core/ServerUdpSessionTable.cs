using System.Collections.Generic;
using System.Net;

namespace Network
{
    /// <summary>Thread safe한 UDP Session 관리 Table입니다.</summary>
    public class ServerUdpSessionTable
    {
        public int Count
        {
            get
            {
                lock (mLock)
                {
                    return mEndPointBySessionID.Count;
                }
            }
        }
        public bool IsEmpty
        {
            get
            {
                lock (mLock)
                {
                    return mEndPointBySessionID.Count == 0;
                }
            }
        }

        private readonly Dictionary<int, EndPoint> mEndPointBySessionID = new();
        private readonly Dictionary<string, int> mSessionIdByEndPointString = new();

        public Dictionary<int, EndPoint>.KeyCollection SessionIDs
        {
            get
            {
                lock (mLock)
                {
                    return mEndPointBySessionID.Keys;
                }
            }
        }
        public Dictionary<int, EndPoint>.ValueCollection SessionEndPoints
        {
            get
            {
                lock (mLock)
                {
                    return mEndPointBySessionID.Values;
                }
            }
        }

        private readonly object mLock = new object();

        public void Add(int sessionID, EndPoint sessionEndPoint)
        {
            lock (mLock)
            {
                mEndPointBySessionID.Add(sessionID, sessionEndPoint);
                mSessionIdByEndPointString.Add(sessionEndPoint.ToString(), sessionID);
            }
        }

        public bool TryRemove(int sessionID)
        {
            lock (mLock)
            {
                if (mEndPointBySessionID.ContainsKey(sessionID))
                {
                    foreach (string endPointString in mSessionIdByEndPointString.Keys)
                    {
                        if (mSessionIdByEndPointString[endPointString] == sessionID)
                        {
                            mEndPointBySessionID.Remove(sessionID);
                            mSessionIdByEndPointString.Remove(endPointString);
                            return true;
                        }
                    }
                }

                return false;
            }
        }

        public bool HasSession(int sessionID)
        {
            lock (mLock)
            {
                return mEndPointBySessionID.ContainsKey(sessionID);
            }
        }

        public void Clear()
        {
            lock (mLock)
            {
                mEndPointBySessionID.Clear();
                mSessionIdByEndPointString.Clear();
            }
        }

        public bool TryFindEndPointByID(int sessionID, out EndPoint endPoint)
        {
            lock (mLock)
            {
                if (mEndPointBySessionID.TryGetValue(sessionID, out var findedEndPoint))
                {
                    endPoint = findedEndPoint;
                    return true;
                }
                else
                {
                    endPoint = null;
                    return false;
                }
            }
        }

        public bool TryFindSessionIdByEndPoint(EndPoint endpoint, out int sessionID)
        {
            lock (mLock)
            {
                string endPointString = endpoint.ToString();

                if (TryFindSessionIdByEndPointString(endPointString, out int findedSessionID))
                {
                    sessionID = findedSessionID;
                    return true;
                }
                else
                {
                    sessionID = -1;
                    return false;
                }
            }
        }

        public bool TryFindSessionIdByEndPointString(string endpointString, out int sessionID)
        {
            lock (mLock)
            {
                if (mSessionIdByEndPointString.TryGetValue(endpointString, out var findedID))
                {
                    sessionID = findedID;
                    return true;
                }
                else
                {
                    sessionID = -1;
                    return false;
                }
            }
        }
    }
}
