using System;
using System.Collections.Generic;
using System.Text;

namespace WMAExcel
{

    // The data is populated based ot the columns in Trial Orders rows
    // 
    // For example, the following columns:
    //  S1_L1_O1_Position	
    //  S1_L1_O1_Type
    //  S1_L1_O1_Obj	
    //  S1_L1_O1_Colour	
    //  S1_L1_O1_Size_deg
    // Populate the data
    public class ExcelTrialObject
    {
        public enum ShowCondition
        {
            None,
            Correct,
            Incorrect,
            Ommission
        }


        public string Event; // S1
        public int Level; // L1 -> 1
        public int Index; // O1 -> 1
        
        // Can be "Center" or "x_of_y" 
        public string Position;
        // can be "Text_Font_1", "Shape", "Picture", "Video"
        public string Type;
        // This is the actual Identifier of the object.
        // for TextFont it would be the actual text
        // for the Shape it would be an id of the shape
        // for the Picture or Video it would be a file name
        public string Object;
        // the id of the color. Can be "n/a" for Pictures.
        public string Colour;
        // the size of the object.
        public string Size;

        // It's used only for FB columns.
        // Where each value is given in a "comma separated" value
        public ShowCondition Condition = ShowCondition.None;
    }
}
