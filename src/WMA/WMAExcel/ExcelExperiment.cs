using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using NPOI.SS.UserModel;
using WMAData;
using static WMAExcel.Utils;
using static WMAExcel.ExcelToWMA;
using System.IO;
using NPOI.Util;

namespace WMAExcel
{
    public class ExcelExperiment : IExperimentData
    {
        public ILogger log;
        public ExcelConfig cfg = null;
        public List<ExcelInputData> inputData = new List<ExcelInputData>();
        public Dictionary<int, ExcelInputData> inputDataLk = new Dictionary<int, ExcelInputData>();
        public int[] inputNums;
        int inputDataNum;
        public bool LoadFromFile(string fn)
        {
            IWorkbook workbook = null;
            using (FileStream fs = new FileStream(fn, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                workbook = WorkbookFactory.Create(fs);
            }
            if (workbook == null)
            {
                log.warn($"failed to load the workbook: {fn}");
                return false;
            }

            var cnt = workbook.NumberOfSheets;
            log.debug($"{workbook.GetType().Name}");

            cfg = null;
            for (int i = 0; i<cnt; i++)
            {
                var sh = workbook.GetSheetAt(i);
                
                log.debug($"{sh.SheetName} -> {IsConfigSheet(sh.SheetName)}");
                

                if ((cfg == null) &&(IsConfigSheet(sh.SheetName)))
                {
                    //log.debug("parsing config");
                    cfg = new ExcelConfig();
                    ParseConfig(sh, cfg);
                } 
                else if (IsInputDataSheet(sh.SheetName, out var inpIdx))
                {
                    //log.debug("parsing input data");
                    var data = new ExcelInputData();
                    data.Index = inpIdx;
                    ParseInputData(sh, data);
                    inputData.Add(data);
                    inputDataLk[data.Index] = data;
                }
            }

            List<int> indicies = new List<int>();
            foreach (var data in inputData)
                indicies.Add(data.Index);
            inputNums = indicies.ToArray();

            return ((cfg != null) || (inputData.Count > 0));
        }

        public string GetOverview()
        {
            return ConvertOverview(cfg.Overview);
        }

        public int[] GetInputDataListSync()
        {
            return inputNums;
        }

        public bool SelectInputdata(int inputNum)
        {
            if (!inputDataLk.ContainsKey(inputNum))
            {
                inputDataNum = -1;
                return false;
            }
            else
            {
                inputDataNum = inputNum;
                return true;
            }
        }

        public string GetKeyboardCsv()
        {
            // using per TrialOrder
            return "";
        }
        public void GetPreloadImages(List<string> names)
        {
            if (names == null)
                return;

            var existing = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            foreach (var name in names)
            {
                if (!string.IsNullOrWhiteSpace(name))
                    existing[name.Trim()] = true;
            }

            if (inputDataNum <= 0)
                return;

            if (!inputDataLk.TryGetValue(inputDataNum, out var inp))
                return;

            foreach (var trial in inp.Trials)
            {
                if (trial == null)
                    continue;

                trial.Populate();

                if (trial.objects == null)
                    continue;

                foreach (var obj in trial.objects)
                {
                    if (obj == null)
                        continue;

                    if (!string.Equals(obj.Type, "Picture", StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (string.IsNullOrWhiteSpace(obj.Object))
                        continue;

                    string imageName = obj.Object.Trim();
                    if (existing.ContainsKey(imageName))
                        continue;

                    existing[imageName] = true;
                    names.Add(imageName);
                }
            }
        }
        public void GetPreloadFonts(List<string> names)
        {
            if (names == null)
                return;

            var existing = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            foreach (var name in names)
            {
                if (!string.IsNullOrWhiteSpace(name))
                    existing[name.Trim()] = true;
            }

            if (cfg == null || cfg.Fonts == null)
                return;

            foreach (var font in cfg.Fonts.Values)
            {
                if (string.IsNullOrWhiteSpace(font.name))
                    continue;

                string fontName = font.name.Trim();
                if (existing.ContainsKey(fontName))
                    continue;

                existing[fontName] = true;
                names.Add(fontName);
            }
        }
        public void GetPreloadSounds(List<string> names)
        {
            if (names == null)
                return;

            var existing = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            foreach (var name in names)
            {
                if (!string.IsNullOrWhiteSpace(name))
                    existing[name.Trim()] = true;
            }

            if (inputDataNum <= 0)
                return;

            if (!inputDataLk.TryGetValue(inputDataNum, out var inp))
                return;

            foreach (var trial in inp.Trials)
            {
                if (trial == null)
                    continue;

                trial.Populate();

                if (trial.events == null)
                    continue;

                foreach (var ev in trial.events)
                {
                    if (ev == null)
                        continue;

                    if (string.IsNullOrWhiteSpace(ev.Sound))
                        continue;

                    foreach (string sound in CsvKeysToArray(ev.Sound))
                    {
                        if (string.IsNullOrWhiteSpace(sound))
                            continue;

                        string soundName = sound.Trim();
                        if (existing.ContainsKey(soundName))
                            continue;

                        existing[soundName] = true;
                        names.Add(soundName);
                    }
                }
            }
        }

        public bool SchedulePlaylist(TrialMonitor display, PlayList playList, out int trialCount)
        {
            trialCount = 0;
            if (inputDataNum <= 0) 
                return false;
            if (!inputDataLk.TryGetValue(inputDataNum, out var inp))
                return false;

            Scheduler sch = new Scheduler();
            sch.display = display;
            sch.dst = playList;
            sch.inp = inp;
            sch.cfg = cfg;

            var seqList = Utils.GetSequenceList(inp.Sequence_of_Events_of_a_trial);
            double timeOfs = 0;
            trialCount = inp.Trials.Count;

            foreach (var t in inp.Trials)
            {
                t.Populate();
                for (int i = 0; i < seqList.Count; i++)
                {
                    var nm = seqList[i];
                    string nx;
                    if (i < seqList.Count - 1)
                        nx = seqList[i + 1];
                    else
                        nx = "";
                    sch.ScheduleEvent(t, nm, ref timeOfs, nx);
                }
            }
            return true;
        }

        // The class is used just not simplify the pass of the internal variables
        private class Scheduler
        {
            public string fallbackFont = "arial.ttf";

            public double distanceCm = 57;
            internal TrialMonitor display;
            internal PlayList dst;
            internal ExcelInputData inp;
            internal ExcelConfig cfg;

            private double DegToCmSize(string deg)
            {
                return DegToCmSize(ToDouble(deg));
            }

            private double DegToCmSize(double deg)
            {
                return (deg * (Math.PI / 180.0)) * distanceCm;
            }

            public void ScheduleEvent(ExcelTrialRow trial, string evName, ref double timeOfs, string nextEvent)
            {
                if (!inp.Events.TryGetValue(evName, out var evInp))
                    return;


                trial.eventsLk.TryGetValue(evName, out var evTr);
                if (evTr == null)
                    evTr = ExcelTrialEvent.Empty;

                double duration = 0;
                if (evTr != null)
                    duration = ToDouble(evTr.Duration);

                bool wantResponse = evName.StWith("R");

                if (wantResponse && (duration == 0)) // response
                {
                    if (trial.eventsLk.TryGetValue(evInp.link_to_stimulus, out var evResp))
                    {
                        duration = ToDouble(evResp.Response_time);
                    }
                }

                dst.StartSection(evName, timeOfs, duration);
                if (wantResponse)
                {
                    var r = dst.ReadResponse(timeOfs, duration);
                    r.responseKeys = CsvKeysToArray(evInp.allowed_keys_to_respond);
                    r.correctKeys = CsvKeysToArray(evTr.Response);
                }

                // event sound
                if (!string.IsNullOrWhiteSpace(evTr.Sound))
                {
                    dst.AddSound(evTr.Sound, timeOfs);
                }


                List<int> lvlIdx = new List<int>();
                lvlIdx.AddRange(evInp.levels.Keys);
                lvlIdx.Sort();

                foreach (var lidx in lvlIdx)
                {
                    var lvl = evInp.levels[lidx];
                    int totalVtx = lvl.VertCount;
                    double radius = DegToCmSize(Convert.ToDouble(lvl.EccentriciyDeg));

                    for (int i = 1; i <= lvl.ObjCount; i++)
                    {

                        // Normally, it's 1 object in the array
                        // but for the feed-back it's an array of 3 objects, with diff condition
                        if (!trial.objectsLk.TryGetValue($"{evName}_{lvl.LevelName}_O{i}", out var objArr))
                            continue;


                        if (objArr.Length == 3)
                        {
                            // conditions
                            foreach(var cobj in objArr)
                            {
                                var st = AllocItem(cobj, duration, timeOfs);
                                if (st != null)
                                {
                                    st = PositionItem(st, cobj, totalVtx, radius);
                                    st.cond = ToCond(cobj.Condition);
                                }
                            }
                        } 
                        else if (objArr.Length > 0)
                        {
                            // stimuli
                            var st = AllocItem(objArr[0], duration, timeOfs);
                            PositionItem(st, objArr[0], totalVtx, radius);
                        }
                    }
                }

                timeOfs += duration;

                // ISI (fade)
                duration = ToDouble(evTr.ISI);
                if (duration > 0)
                {
                    dst.StartSection($"{evName}>{nextEvent}", timeOfs, duration);
                    timeOfs += duration;
                }
            }

            public PlayItem PositionItem(PlayItem item, ExcelTrialObject obj, int totalVtx, double radius)
            {
                if (item == null) return null;
                // position!
                if (IsCenterPos(obj.Position))
                {
                    item.SetPos(PlayItemPos.Center, 0.0f);
                }
                else
                {
                    var vpos = ParseVertexPos(obj.Position);
                    item.pos = PlayItemPos.OneOfCount;
                    item.posOther = vpos;
                    item.posCount = totalVtx;
                    item.posDistanceCm = radius;
                    // todo: rotation for the position angle?
                }
                return item;
            }
            public PlayItem AllocItem(ExcelTrialObject obj, double duration, double timeOfs)
            {
                PlayItem result = null;
                TryParseColor(obj.Colour, out var clr);
                if (obj.Type == "Shape")
                {
                    result = dst.AddByShape(
                        ToInt(obj.Object), 
                        DegToCmSize(obj.Size), 
                        DegToCmSize(obj.Size), 
                        clr,
                        timeOfs, duration);
                    
                } 
                else if (obj.Type.StWith("Text_Font_"))
                {
                    // try to get font!
                    TryGetNumber(obj.Type, "Text_Font_", out var f);

                    string font = fallbackFont;
                    if (cfg.Fonts.TryGetValue(f, out var fd))
                        font = fd.name;

                    // todo: font size! and font style
                    result = dst.AddText(obj.Object, font, clr, timeOfs, duration);
                }
                else if (obj.Type.StWith("Picture"))
                {
                    result = dst.AddImageByName(obj.Object, DegToCmSize(obj.Size), clr, timeOfs, duration);
                }


    

                return result;
            }
        }

    }
}
