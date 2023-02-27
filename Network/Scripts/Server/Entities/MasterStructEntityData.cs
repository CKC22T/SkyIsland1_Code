using System.Collections.Generic;
using UnityEngine;

namespace Network.Server
{
    public class MasterStructEntityData : MasterEntityData
    {
        [SerializeField] private List<BaseLocationEventTrigger> OnActionDieEventList;

        public void Start()
        {
            mOnCreated += () =>
            {
                mDestroyEvent += callEvents;
            };
        }

        private void callEvents(int entityID)
        {
            if (OnActionDieEventList == null)
                return;

            foreach (var e in OnActionDieEventList)
            {
                e.TriggeredEvent(null);
            }
        }
    }
}
