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
    public partial class Form_main : Form
    {
        Welcome wel = new Welcome();
        Form1 form1 = new Form1();
        public Form_main()
        {
            InitializeComponent();                   
            wel.StartPosition = FormStartPosition.CenterScreen;
            wel.ShowDialog();
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
           
            form1.ShowDialog();
        }
    }
}
