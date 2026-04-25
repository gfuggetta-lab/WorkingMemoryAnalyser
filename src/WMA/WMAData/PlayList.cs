using System;
using System.Collections.Generic;
using System.Text;

namespace WMAData
{
    public class PlayList
    {
        public List<PlayItem> items = new List<PlayItem>();
        int scheduleCounter;

        public PlayItem Add(double ofsMs)
        {
            var result = new PlayItem { startMs = ofsMs, sameTimeOrder = scheduleCounter};
            items.Add(result);
            scheduleCounter++;
            return result;
        }
    }
}
