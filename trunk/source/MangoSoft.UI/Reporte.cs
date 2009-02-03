using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ControlProx
{
    public partial class Reporte : Form
    {
        public Reporte()
        {
            InitializeComponent();
        }

        private void Reporte_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'dataSet1.VEventos' table. You can move, or remove it, as needed.
            this.vEventosTableAdapter.Fill(this.dataSet1.VEventos);

            this.reportViewer1.RefreshReport();
        }
    }
}
