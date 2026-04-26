using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace WMAData
{
    public static class Helpers
    {
        public static void FillShapes(StageData src, List<int> dst)
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
                images.Add($"{nm}.bmp");
        }
    }
}
