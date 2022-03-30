using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsForms_WTDS
{
    public partial class Welcome : Form
    {
        public Welcome()
        {
            InitializeComponent();
        }
      
        private void label1_Click(object sender, EventArgs e)
        {
            timer1.Stop();
            this.Close();
           
        }

        private void Welcome_Load(object sender, EventArgs e)
        {
           timer1.Start();
        }
       

        private void Welcome_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Dispose();
        }
    }
}
