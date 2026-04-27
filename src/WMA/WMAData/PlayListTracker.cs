using System;
using System.Collections.Generic;
using System.Text;

namespace WMAData
{
    public class PlayListTracker
    {
        public double lastMs;
        public PlayList list;

        public PlayListTracker(PlayList list)
        {
            this.list = list;
        }

        // deltaMs from the last time
        // returns the number of items that triggered or faded
        // if the same item triggers and fades, it counts for 1.
        public int Track(double deltaMs, 
            // the list of effective items
            List<PlayItem> effList,   
            // the list of items that triggered with the last delta
            List<PlayItem> trigList, 
            // the list of items that stopped being active within this delta
            List<PlayItem> fadeList,
            // only items that triggered AND faded would go into this list
            List<PlayItem> trigFadeList
            )
        {
            double curMs = lastMs + deltaMs;
            int cnt = 0;
            foreach(var itm in list.items)
            {
                if (itm == null) continue;
                if (itm.itemType == PlayItemType.None) continue;
                if (itm.startMs > curMs) continue;
                
                bool hasExp = (itm.durationMs >= 0); // negative amount means it's eternal!
                double endMs;
                if (!hasExp) endMs = double.MaxValue;
                else endMs = itm.startMs + itm.durationMs;

                bool isTriggered = (itm.startMs > lastMs) && (itm.startMs <= curMs);
                if ((isTriggered) && (trigList != null))
                    trigList.Add(itm);

                bool isFaded = (endMs <= curMs)&&(lastMs < endMs);
                if ((isFaded) && (fadeList != null))
                    fadeList.Add(itm);

                bool inEff = curMs <= endMs;
                if ((inEff) && (effList != null)) 
                    effList.Add(itm);
                
                if (isFaded || isTriggered)
                    cnt++;
                if (isFaded && isTriggered && (trigFadeList != null))
                    trigFadeList.Add(itm);
            }
            lastMs = curMs;
            return cnt;
        }
    }
}
