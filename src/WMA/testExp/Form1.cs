using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WMAData;
using WMAFiles;

namespace testExp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string fn = "InputData_1.txt";
            var args = Environment.GetCommandLineArgs();
            if ((args != null) && (args.Length > 1))
                fn = args[1];

            int t = Environment.TickCount;
            var list = InputDataHelper.LoadTrials(fn);
            t = Environment.TickCount - t;
            this.Text = $"count: {list.Count}; t = {t}";

            //foreach (var t in list)
            //{
            //
            //}
        }
    }
}
