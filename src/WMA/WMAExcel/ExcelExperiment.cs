using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Extensions.Logging;
using NPOI;
using NPOI.OOXML;
using NPOI.SS.UserModel;
using WMAData;
using static WMAExcel.Utils;

namespace WMAExcel
{
    public class ExcelExperiment : IExperimentData
    {
        public ILogger log;
        public ExcelConfig cfg = null;
        public List<ExcelInputData> inputData = new List<ExcelInputData>();
        public int[] inputNums;
        public bool LoadFromFile(string fn)
        {
            IWorkbook workbook = WorkbookFactory.Create(fn);
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
            return cfg.Overview;
        }

        public int[] GetInputDataListSync()
        {
            return inputNums;
        }


        public bool SchedulePlaylist(int inputDataNum, TrialMonitor display, PlayList playList)
        {
            return false;
        }

        public static void ParseConfig(ISheet source, ExcelConfig dst, ILogger log = null)
        {
            bool inOverview = false;
            StringBuilder over = null;
            SheetReader rdr = new SheetReader(source);
            while (rdr.ReadNext())
            {
                log.debug($"name:  {rdr.Name}");
                log.debug($"value: {rdr.Value}");
                var v = rdr.Value;
                if (!string.IsNullOrEmpty(rdr.Name)&&!string.IsNullOrEmpty(v) && inOverview)
                {
                    inOverview = false;
                }

                if (inOverview && string.IsNullOrEmpty(v))
                {
                    over.AppendLine(rdr.Name);
                    continue;
                }

                if (rdr.Name.StWith("Message_"))
                {
                    if (TryGetNumber(rdr.Name, "message_", out var n))
                    {
                        if ((!string.IsNullOrEmpty(v))
                            && (string.Compare(v, "not in use", true) != 0))
                        {
                            dst.Messages[n] = v;
                        }
                    }
                }
                else if (rdr.Name.StWith("Shapes_text_colour_"))
                {
                    if (TryGetNumber(rdr.Name, "Shapes_text_colour_", out var n))
                    {
                        if (!string.IsNullOrEmpty(v))
                        {
                            dst.Shapes_text_colour[n] = v;
                        }
                    }
                }
                else if (rdr.Name.StWith("LUFA_USB_CDC_Interrupt_"))
                    ParseLufa(rdr.Name, v, dst);
                else if (rdr.Name.StWith("Font_"))
                    ParseFont(rdr.Name, v, dst);
                else if (rdr.Name.StWith("Experiment"))
                    dst.Experiment = v;
                else if (rdr.Name.StWith("Reference_Number"))
                    dst.Reference_Number = v;
                else if (rdr.Name.StWith("Version"))
                    dst.Version = v;
                else if (rdr.Name.StWith("Release_date_"))
                    dst.Release_date = v;
                else if (rdr.Name.StWith("Author"))
                    dst.Author = v;
                else if (rdr.Name.StWith("Member_user_name"))
                    dst.Member_user_name = v;
                else if (rdr.Name.StWith("Age_range"))
                    dst.Age_range = v;
                else if (rdr.Name.StWith("Device"))
                    dst.Device = v;
                else if (rdr.Name.StWith("Study_Reference_Number"))
                    dst.Study_Reference_Number = v;
                else if (rdr.Name.StWith("Study"))
                    dst.Study = v;
                else if (rdr.Name.StWith("Minimum_training_accuracy"))
                    dst.Minimum_training_accuracy = v;
                else if (rdr.Name.StWith("N_trials_before_pause_training"))
                    dst.N_trials_before_pause_training = v;
                else if (rdr.Name.StWith("Instructions_ODD_participants"))
                    dst.Instructions_ODD_participants = v;
                else if (rdr.Name.StWith("Instructions_EVEN_participants"))
                    dst.Instructions_EVEN_participants = v;
                else if (rdr.Name.StWith("Audio_Instructions_ODD_participants"))
                    dst.Audio_Instructions_ODD_participants = v;
                else if (rdr.Name.StWith("Audio_Instructions_EVEN_participants"))
                    dst.Audio_Instructions_EVEN_participants = v;
                else if (rdr.Name.StWith("RT_constant_error_ms"))
                    dst.RT_constant_error_ms = v;
                else if (rdr.Name.StWith("Pause_background_shape_colour"))
                    dst.Pause_background_shape_colour = v;
                else if (rdr.Name.StWith("Run_background_shape_colour"))
                    dst.Run_background_shape_colour = v;
                else if (rdr.Name.StWith("overview"))
                {
                    if (over == null)
                        over = new StringBuilder();
                    over.AppendLine(v);
                    inOverview = true;
                }
            }
            if (over != null)
                dst.Overview = over.ToString();
            else
                dst.Overview = "";
        }

        public static void ParseInputData(ISheet source, ExcelInputData dst, ILogger log = null)
        {
            SheetReader rdr = new SheetReader(source);
            while (rdr.ReadNext())
            {
                log.debug($"name:  {rdr.Name}");
                log.debug($"value: {rdr.Value}");
                var v = rdr.Value;

                if (rdr.Name.StWith("// Start trial sequence"))
                {
                    ParseTrialSequence(source, rdr.row, dst);
                    break;
                }

                if (rdr.Name.EndsWith("_marker", StringComparison.OrdinalIgnoreCase))
                {
                    ParseTrialSequence(source, rdr.row - 1, dst);
                    break;
                }

                if (rdr.Name.StWith("Background_Type"))
                    dst.Background_Type = v;
                else if (rdr.Name.StWith("Background_Object"))
                    dst.Background_Object = v;
                else if (rdr.Name.StWith("Background_diameter_deg"))
                    dst.Background_diameter_deg = v;
                else if (rdr.Name.StWith("Background_sound"))
                    dst.Background_sound = v;
                else if (rdr.Name.StWith("Number_of_events_on_a_trial"))
                    dst.Number_of_events_on_a_trial = v;
                else if (rdr.Name.StWith("Sequence_of_Events_of_a_trial"))
                    dst.Sequence_of_Events_of_a_trial = v;
                else
                    ParseInputEvent(rdr.Name, v, dst);
            }
        }

        private static void ParseTrialSequence(ISheet source, int headerRowIndex, ExcelInputData dst)
        {
            IRow headerRow = source.GetRow(headerRowIndex);
            if (headerRow == null)
                return;

            List<int> columns = new List<int>();
            List<string> names = new List<string>();
            for (int c = headerRow.FirstCellNum; c < headerRow.LastCellNum; c++)
            {
                if (c < 0)
                    continue;

                var name = headerRow.GetCell(c).ValueAsStr().Trim();
                if (string.IsNullOrEmpty(name))
                    continue;

                columns.Add(c);
                names.Add(name);
                dst.TrialDataNames.Add(name);
            }

            for (int r = headerRowIndex + 1; r <= source.LastRowNum; r++)
            {
                IRow row = source.GetRow(r);
                if (row == null)
                    continue;

                ExcelTrialRow trial = new ExcelTrialRow();
                bool hasValue = false;
                for (int i = 0; i < columns.Count; i++)
                {
                    int c = columns[i];
                    string name = names[i];
                    string value = row.GetCell(c).ValueAsStr().Trim();

                    if (!string.IsNullOrEmpty(value))
                        hasValue = true;

                    trial.data[name] = value;
                }

                if (hasValue)
                    dst.Trials.Add(trial);
            }
        }

        private static void ParseInputEvent(string name, string value, ExcelInputData dst)
        {
            if (!ParseName(name,
                out var stimName,
                out var stimType,
                out var stimNum,
                out var levelName,
                out var levelNum,
                out var suffix))
                return;

            if (string.IsNullOrEmpty(stimType))
                return;

            var ev = ForceInputEvent(dst, stimName, stimType, stimNum);
            if (!string.IsNullOrEmpty(levelName))
            {
                ParseInputEventLevel(ev, levelName, levelNum, suffix, value);
                return;
            }

            if (suffix.StWith("Label"))
                ev.label = value;
            else if (suffix.StWith("link_to_stimulus"))
                ev.link_to_stimulus = value;
            else if (suffix.StWith("link_to_response"))
                ev.link_to_response = value;
            else if (suffix.StWith("allowed_keys_to_respond"))
                ev.allowed_keys_to_respond = value;
            else if (suffix.StWith("Number_of_Layers"))
                ev.number_of_layers = value;
        }

        private static ExcelInputEvent ForceInputEvent(
            ExcelInputData dst,
            string stimName,
            string stimType,
            int stimNum)
        {
            if (!dst.Events.TryGetValue(stimName, out var ev))
            {
                ev = new ExcelInputEvent
                {
                    Name = stimName,
                    Type = stimType,
                    Num = stimNum
                };
                dst.Events[stimName] = ev;
            }

            return ev;
        }

        private static void ParseInputEventLevel(
            ExcelInputEvent ev,
            string levelName,
            int levelNum,
            string suffix,
            string value)
        {
            if (!ev.levels.TryGetValue(levelNum, out var level))
            {
                level = new ExcelInputEvent.Level
                {
                    LevelName = levelName,
                    LevelNum = levelNum
                };
                ev.levels[levelNum] = level;
            }

            if (suffix.StWith("No_of_vertices_of_the_virtual_circle"))
            {
                if (int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var vertCount))
                    level.VertCount = vertCount;
            }
            else if (suffix.StWith("Eccentricity_of_the_virtual_circle_deg"))
            {
                if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var eccentricityDeg))
                    level.EccentriciyDeg = eccentricityDeg;
            }
            else if (suffix.StWith("No_of_Objects"))
            {
                if (int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var objCount))
                    level.ObjCount = objCount;
            }
        }

        private static void ParseLufa(string name, string value, ExcelConfig dst)
        {
            if (!TryGetNumber(name, "LUFA_USB_CDC_Interrupt_", out var n))
                return;

            if (!dst.LUFA_USB_CDC_Interrupt.TryGetValue(n, out var lufa))
            {
                lufa = new ExcelLufa();
                dst.LUFA_USB_CDC_Interrupt[n] = lufa;
            }

            lufa.index = n;

            if (name.StWith($"LUFA_USB_CDC_Interrupt_{n}_mode"))
                lufa.mode = value;
            else if (name.StWith($"LUFA_USB_CDC_Interrupt_{n}_dead_time_ms"))
                lufa.dead_ms = value;
        }

        private static void ParseFont(string name, string value, ExcelConfig dst)
        {
            if (!TryGetNumber(name, "Font_", out var n))
                return;

            if (!dst.Fonts.TryGetValue(n, out var font))
                font = new FontData();

            if (name.StWith($"Font_{n}_size"))
            {
                if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var size))
                    font.size = size;
            }
            else if (name.StWith($"Font_{n}_style"))
            {
                font.style = ParseFontStyle(value);
            }
            else if (name.StWith($"Font_{n}"))
            {
                font.name = value;
            }

            dst.Fonts[n] = font;
        }

        private static string ParseFontStyle(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "";

            value = value.Trim();
            int i = 0;
            while ((i < value.Length) && char.IsLetter(value[i]))
                i++;

            if (i == 0)
                return "";

            return value.Substring(0, i);
        }
    }
}
