using System;
using System.Collections.Generic;
using System.Text;
using static WMAExcel.Utils;

namespace WMAExcel
{
    public class ExcelTrialRow
    {
        public Dictionary<string, string> data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public List<ExcelTrialObject> objects = null;
        // it keeps an array because of the "conditions"!!!
        public Dictionary<string, ExcelTrialObject[]> objectsLk = null;
        public List<ExcelTrialEvent> events = null;
        public Dictionary<string, ExcelTrialEvent> eventsLk = null;


        // Experimental_Condition is recorder as conditions[0]
        // Condition_1 is recorded as conditions[1]
        // Condition_2 is recorded as conditions[2], etc
        public Dictionary<int, string> conditions = null;

        public void Populate()
        {
            objects = new List<ExcelTrialObject>();
            events = new List<ExcelTrialEvent>();
            conditions = new Dictionary<int, string>();

            objectsLk = new Dictionary<string, ExcelTrialObject[]>(StringComparer.OrdinalIgnoreCase);
            var objectOrder = new List<string>();
            eventsLk = new Dictionary<string, ExcelTrialEvent>(StringComparer.OrdinalIgnoreCase);
            var eventOrder = new List<string>();

            foreach (var item in data)
            {
                string columnName = NormalizeColumnName(item.Key);
                if (string.IsNullOrEmpty(columnName))
                    continue;

                if (TryPopulateCondition(columnName, item.Value))
                    continue;

                string[] parts = columnName.Split(new char[] { '_' }, StringSplitOptions.None);
                if (TryPopulateObject(parts, item.Value, objectsLk, objectOrder))
                    continue;

                TryPopulateEvent(parts, item.Value, eventsLk, eventOrder);
            }

            foreach (string key in objectOrder)
            {
                foreach (ExcelTrialObject obj in objectsLk[key])
                {
                    if (HasObjectData(obj))
                        objects.Add(obj);
                }
            }

            foreach (string key in eventOrder)
            {
                ExcelTrialEvent ev = eventsLk[key];
                if (HasEventData(ev))
                    events.Add(ev);
            }
        }

        private bool TryPopulateCondition(string columnName, string value)
        {
            if (string.Equals(columnName, "Experimental_condition", StringComparison.OrdinalIgnoreCase))
            {
                conditions[0] = value;
                return true;
            }

            const string prefix = "Condition_";
            if (!columnName.StWith(prefix))
                return false;

            if (TryGetNumber(columnName, prefix, out var index) && (index > 0))
                conditions[index] = value;

            return true;
        }

        private static bool TryPopulateObject(
            string[] parts,
            string value,
            Dictionary<string, ExcelTrialObject[]> groups,
            List<string> order)
        {
            if ((parts.Length < 4)
                || !Utils.IsSpecialNum(parts[0], out _, out _)
                || !TryParseIndexedPart(parts[1], 'L', out var level)
                || !TryParseIndexedPart(parts[2], 'O', out var index))
                return false;

            string field = string.Join("_", parts, 3, parts.Length - 3);
            if (!IsObjectField(field))
                return false;

            string eventName = parts[0];
            string key = eventName + "_L" + level + "_O" + index;
            if (!groups.TryGetValue(key, out var group))
            {
                bool isFeedback = eventName.StartsWith("FB", StringComparison.OrdinalIgnoreCase);
                int count = isFeedback ? 3 : 1;
                group = new ExcelTrialObject[count];
                for (int i = 0; i < count; i++)
                {
                    group[i] = new ExcelTrialObject
                    {
                        Event = eventName,
                        Level = level,
                        Index = index,
                        Condition = isFeedback ? FeedbackCondition(i) : ExcelTrialObject.ShowCondition.None
                    };
                }
                groups[key] = group;
                order.Add(key);
            }

            string[] values = group.Length == 1
                ? new string[] { value }
                : SplitConditionalValues(value);

            for (int i = 0; i < group.Length; i++)
            {
                string fieldValue = values.Length == 1
                    ? values[0]
                    : (i < values.Length ? values[i] : "");
                SetObjectField(group[i], field, fieldValue);
            }

            return true;
        }

        private static bool TryPopulateEvent(
            string[] parts,
            string value,
            Dictionary<string, ExcelTrialEvent> groups,
            List<string> order)
        {
            if ((parts.Length < 2) || !Utils.IsSpecialNum(parts[0], out _, out _))
                return false;

            string field = string.Join("_", parts, 1, parts.Length - 1);
            // if (!IsEventField(field))
            //     return false;

            string eventName = parts[0];
            if (!groups.TryGetValue(eventName, out var ev))
            {
                ev = new ExcelTrialEvent { Event = eventName };
                groups[eventName] = ev;
                order.Add(eventName);
            }

            if (field.Equals("Marker", StringComparison.OrdinalIgnoreCase))
                ev.Marker = value;
            else if (field.Equals("sound", StringComparison.OrdinalIgnoreCase))
                ev.Sound = value;
            else if (field.Equals("Duration", StringComparison.OrdinalIgnoreCase))
                ev.Duration = value;
            else if (field.Equals("data_logging", StringComparison.OrdinalIgnoreCase))
                ev.Data_Logging = value;
            else if (field.Equals("ISI", StringComparison.OrdinalIgnoreCase))
                ev.ISI = value;
            else if (field.Equals("Response_time", StringComparison.OrdinalIgnoreCase))
                ev.Response_time = value;
            else if (field.Equals("Response", StringComparison.OrdinalIgnoreCase)
                || field.Equals("Reponse", StringComparison.OrdinalIgnoreCase))
                ev.Response = value;


            return true;
        }

        private static string NormalizeColumnName(string columnName)
        {
            if (string.IsNullOrWhiteSpace(columnName))
                return "";

            int annotation = columnName.IndexOf('(');
            if (annotation >= 0)
                columnName = columnName.Substring(0, annotation);

            return columnName.Trim();
        }

        private static bool TryParseIndexedPart(string value, char prefix, out int index)
        {
            index = 0;
            return !string.IsNullOrEmpty(value)
                && (char.ToUpperInvariant(value[0]) == char.ToUpperInvariant(prefix))
                && int.TryParse(value.Substring(1), out index);
        }

        private static bool IsObjectField(string field)
        {
            return field.Equals("Position", StringComparison.OrdinalIgnoreCase)
                || field.Equals("Type", StringComparison.OrdinalIgnoreCase)
                || field.Equals("Obj", StringComparison.OrdinalIgnoreCase)
                || field.Equals("Colour", StringComparison.OrdinalIgnoreCase)
                || field.Equals("Size_deg", StringComparison.OrdinalIgnoreCase);
        }

        private static void SetObjectField(ExcelTrialObject obj, string field, string value)
        {
            if (field.Equals("Position", StringComparison.OrdinalIgnoreCase))
                obj.Position = value;
            else if (field.Equals("Type", StringComparison.OrdinalIgnoreCase))
                obj.Type = value;
            else if (field.Equals("Obj", StringComparison.OrdinalIgnoreCase))
                obj.Object = value;
            else if (field.Equals("Colour", StringComparison.OrdinalIgnoreCase))
                obj.Colour = value;
            else if (field.Equals("Size_deg", StringComparison.OrdinalIgnoreCase))
                obj.Size = value;
        }

        private static bool IsEventField(string field)
        {
            return field.Equals("Marker", StringComparison.OrdinalIgnoreCase)
                || field.Equals("sound", StringComparison.OrdinalIgnoreCase)
                || field.Equals("Duration", StringComparison.OrdinalIgnoreCase)
                || field.Equals("data_logging", StringComparison.OrdinalIgnoreCase)
                || field.Equals("ISI", StringComparison.OrdinalIgnoreCase);
        }

        private static string[] SplitConditionalValues(string value)
        {
            if (value == null)
                return new string[] { "" };

            string[] values = value.Split(new char[] { ',' }, StringSplitOptions.None);
            for (int i = 0; i < values.Length; i++)
                values[i] = values[i].Trim();
            return values;
        }

        private static ExcelTrialObject.ShowCondition FeedbackCondition(int index)
        {
            switch (index)
            {
                case 0: return ExcelTrialObject.ShowCondition.Correct;
                case 1: return ExcelTrialObject.ShowCondition.Incorrect;
                default: return ExcelTrialObject.ShowCondition.Ommission;
            }
        }

        private static bool HasObjectData(ExcelTrialObject obj)
        {
            return !string.IsNullOrEmpty(obj.Position)
                || !string.IsNullOrEmpty(obj.Type)
                || !string.IsNullOrEmpty(obj.Object)
                || !string.IsNullOrEmpty(obj.Colour)
                || !string.IsNullOrEmpty(obj.Size);
        }

        private static bool HasEventData(ExcelTrialEvent ev)
        {
            return !string.IsNullOrEmpty(ev.Marker)
                || !string.IsNullOrEmpty(ev.Sound)
                || !string.IsNullOrEmpty(ev.Duration)
                || !string.IsNullOrEmpty(ev.Data_Logging)
                || !string.IsNullOrEmpty(ev.ISI);
        }

        public string St(string fullName, string def = "")
        {
            if (!data.TryGetValue(fullName, out var result))
                return def;
            return result;
        }

    }
}
