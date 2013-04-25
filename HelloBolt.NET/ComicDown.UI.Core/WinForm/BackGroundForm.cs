using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ComicDown.UI.Core.Bolt;

namespace ComicDown.UI.Core
{
    public partial class BackGroundForm : Form
    {
        private int c = 0;
        public event Action TimerTick;
        public BackGroundForm()
        {
            InitializeComponent();
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (TimerTick != null) {
                TimerTick();
            }
        }
    }
}
