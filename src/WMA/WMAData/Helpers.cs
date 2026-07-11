using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WMAData
{
    public static class Helpers
    {
        public static void FillShapes(StimuliData src, List<int> dst)
        {
            dst.Add(src.Shape);
            dst.Add(src.ShapePos1_NW);
            dst.Add(src.ShapePos2_NE);
            dst.Add(src.ShapePos3_SE);
            dst.Add(src.ShapePos4_SW);
            dst.Add(src.ShapePos5_Center);
            dst.Add(src.DistractShape);
        }
        public static List<int> GetShapes(this TrialOrder ord)
        {
            List<int> sh = new List<int>();
            FillShapes(ord.S1, sh);
            FillShapes(ord.S2, sh);
            FillShapes(ord.S3, sh);
            FillShapes(ord.S4, sh);
            sh.Add(ord.Feedback_shape);

            List<int> result = new List<int>();
            foreach (var s in sh)
                if (s != Consts.SHAPE_NONE)
                    result.Add(s);
            return result;
        }

        public static void GetPreloadImages(this Configuration cfg, List<TrialOrder> trials, List<string> images)
        {
            Dictionary<int, bool> imageCheck = new Dictionary<int, bool>();
            foreach (var vo in trials)
            {
                var shapesList = vo.GetShapes();
                foreach(var sh in shapesList)
                {
                    if ((sh >= Consts.SHAPE_BMP_MIN) && (sh <= Consts.SHAPE_BMP_MAX))
                    {
                        imageCheck[sh] = true;
                    }
                }
            }
            foreach (var nm in imageCheck.Keys)
                images.Add(nm.ToString());
        }

        public static void GetPreloadFonts(this Configuration cfg, List<string> fonts)
        {
            if (cfg == null) return;
            if (fonts == null) return;
            fonts.Add(cfg.font_1.name);
            //fonts.Add(cfg.font_2.name); // currently not used
            fonts.Add(cfg.Feedback_font.name);
        }

        public static void GetPreloadSounds(this Configuration cfg, List<TrialOrder> trials, List<string> sounds)
        {
            if (cfg == null) return;
            if (sounds == null) return;

            Dictionary<string, bool> snd = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            bool addFeedback = false;
            foreach(var o in trials)
            {
                if (o.S1.Sound != 0)
                    snd[o.S1.Sound.ToString()] = true;
                if (o.S2.Sound != 0)
                    snd[o.S2.Sound.ToString()] = true;
                if (o.S3.Sound != 0)
                    snd[o.S3.Sound.ToString()] = true;
                if (o.S4.Sound != 0)
                    snd[o.S4.Sound.ToString()] = true;
                addFeedback = addFeedback | (o.Feedback_sound != 0);
            }
            if (addFeedback)
            {
                snd["correct"] = true;
                snd["incorrect"] = true;
            }
            foreach(var k in snd.Keys)
            {
                sounds.Add($"{k}.wav");
            }
        }

        #region Async Experiment Notifier helpers
        public static Task StartTrial(this IAsyncExperimentNotifier nt)
        {
            return nt.StartTrial(CancellationToken.None);
        }

        public static Task MarkerS1(this IAsyncExperimentNotifier nt, int marker)
        {
            return nt.MarkerS1(marker, CancellationToken.None);
        }
        public static Task MarkerS2(this IAsyncExperimentNotifier nt, int marker)
        {
            return nt.MarkerS2(marker, CancellationToken.None);
        }
        public static Task MarkerS3(this IAsyncExperimentNotifier nt, int marker)
        {
            return nt.MarkerS3(marker, CancellationToken.None);
        }
        public static Task MarkerS4(this IAsyncExperimentNotifier nt, int marker)
        {
            return nt.MarkerS4(marker, CancellationToken.None);
        }
        public static Task Feedback(this IAsyncExperimentNotifier nt, bool isCorrect)
        {
            return nt.Feedback(isCorrect, CancellationToken.None);
        }

        #endregion
    }
}
