using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using NPOI;
using NPOI.OOXML;
using NPOI.SS.UserModel;
using WMAData;
using static WMAExcel.Utils;

namespace WMAExcel
{
    public class ExcelMain
    {
        public ExcelConfig cfg = null;
        public List<ExcelInputData> inputData = new List<ExcelInputData>();
        public bool LoadFromFile(string fn)
        {
            IWorkbook workbook = WorkbookFactory.Create(fn);
            var cnt = workbook.NumberOfSheets;
            Console.WriteLine($"{workbook.GetType().Name}");

            cfg = null;
            for (int i = 0; i<cnt; i++)
            {
                var sh = workbook.GetSheetAt(i);
                
                Console.WriteLine($"{sh.SheetName} -> {IsConfigSheet(sh.SheetName)}");
                

                if ((cfg == null) &&(IsConfigSheet(sh.SheetName)))
                {
                    Console.WriteLine("parsing config");
                    cfg = new ExcelConfig();
                    ParseConfig(sh, cfg);
                    //ParseSheet
                } 
                else if (IsInputDataSheet(sh.SheetName, out var inpIdx))
                {
                    // ParseInput data
                }
            }

            return ((cfg != null) || (inputData.Count > 0));
        }

        public static void ParseConfig(ISheet source, ExcelConfig dst)
        {
            bool inOverview = false;
            StringBuilder over = null;
            SheetReader rdr = new SheetReader(source);
            while (rdr.ReadNext())
            {
                Console.WriteLine($"name:  {rdr.Name}");
                Console.WriteLine($"value: {rdr.Value}");
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

                if (rdr.Name.StartsWith("Message_"))
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
                else if (rdr.Name.StartsWith("Shapes_text_colour_"))
                {
                    if (TryGetNumber(rdr.Name, "Shapes_text_colour_", out var n))
                    {
                        if (!string.IsNullOrEmpty(v))
                        {
                            dst.Shapes_text_colour[n] = v;
                        }
                    }
                }
                else if (rdr.Name.StartsWith("Font_"))
                    ParseFont(rdr.Name, v, dst);
                else if (rdr.Name.StartsWith("Experiment"))
                    dst.Experiment = v;
                else if (rdr.Name.StartsWith("Reference_Number"))
                    dst.Reference_Number = v;
                else if (rdr.Name.StartsWith("Version"))
                    dst.Version = v;
                else if (rdr.Name.StartsWith("Release_date_"))
                    dst.Release_date = v;
                else if (rdr.Name.StartsWith("Age_range"))
                    dst.Age_range = v;
                else if (rdr.Name.StartsWith("Device"))
                    dst.Device = v;
                else if (rdr.Name.StartsWith("Study_Reference_Number"))
                    dst.Study_Reference_Number = v;
                else if (rdr.Name.StartsWith("Study"))
                    dst.Study = v;
                else if (rdr.Name.StartsWith("Minimum_training_accuracy"))
                    dst.Minimum_training_accuracy = v;
                else if (rdr.Name.StartsWith("N_trials_before_pause_training"))
                    dst.N_trials_before_pause_training = v;
                else if (rdr.Name.StartsWith("Instructions_ODD_participants"))
                    dst.Instructions_ODD_participants = v;
                else if (rdr.Name.StartsWith("Instructions_EVEN_participants"))
                    dst.Instructions_EVEN_participants = v;
                else if (rdr.Name.StartsWith("Audio_Instructions_ODD_participants"))
                    dst.Audio_Instructions_ODD_participants = v;
                else if (rdr.Name.StartsWith("Audio_Instructions_EVEN_participants"))
                    dst.Audio_Instructions_EVEN_participants = v;
                else if (rdr.Name.StartsWith("RT_constant_error_ms"))
                    dst.RT_constant_error_ms = v;
                else if (rdr.Name.StartsWith("Pause_background_shape_colour"))
                    dst.Pause_background_shape_colour = v;
                else if (rdr.Name.StartsWith("Run_background_shape_colour"))
                    dst.Run_background_shape_colour = v;
                else if (rdr.Name.StartsWith("overview"))
                {
                    if (over == null)
                        over = new StringBuilder();
                    over.AppendLine(v);
                    inOverview = true;
                }
            }
        }

        private static void ParseFont(string name, string value, ExcelConfig dst)
        {
            if (!TryGetNumber(name, "Font_", out var n))
                return;

            if (!dst.Fonts.TryGetValue(n, out var font))
                font = new FontData();

            if (name.StartsWith($"Font_{n}_size"))
            {
                if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var size))
                    font.size = size;
            }
            else if (name.StartsWith($"Font_{n}_style"))
            {
                font.style = ParseFontStyle(value);
            }
            else if (name.StartsWith($"Font_{n}"))
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
