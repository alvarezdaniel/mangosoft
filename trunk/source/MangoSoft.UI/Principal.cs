using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using ControlProx.Properties;
using CommLibrary;
using DataLibrary;
using ControlProx;

namespace SerialPortTerminal
{
    public partial class formPrincipal : DevExpress.XtraEditors.XtraForm
    {
        private Communications comm;
        private Data data;
        
        public formPrincipal()
        {
            InitializeComponent();
        }

        private void formPrincipal_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'dataSetTipoTag.TipoTag' table. You can move, or remove it, as needed.
            this.tipoTagTableAdapter.Fill(this.dataSetTipoTag.TipoTag);
            // TODO: This line of code loads data into the 'dataSetTags.Tags' table. You can move, or remove it, as needed.
            this.tagsTableAdapter.Fill(this.dataSetTags.Tags);
            // TODO: This line of code loads data into the 'BaseDataSet2.Eventos' table. You can move, or remove it, as needed.
            //this.EventosTableAdapter.Fill(this.BaseDataSet2.Eventos);
            // TODO: This line of code loads data into the 'baseDataSet1.Tags' table. You can move, or remove it, as needed.
            //this.tagsTableAdapter.Fill(this.baseDataSet1.Tags);

            comm = new Communications();
            comm.Config(Settings.Default.BaudRate, Settings.Default.DataBits, Settings.Default.StopBits,
                Settings.Default.Parity, Settings.Default.PortName, Settings.Default.Timeout);
            comm.ModoDummy = Settings.Default.ModoDummy;

            data = new Data();
            data.ConnectionString = Settings.Default.BaseConnectionString;
            data.ModoDummy = Settings.Default.ModoDummy;
            data.GenerateTipoTags();

            button2.Visible = Settings.Default.ModoDummy;
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            frmTerminal f = new frmTerminal();
            f.Show();

        }

        private void simpleButton7_Click(object sender, EventArgs e)
        {
            if (gridControl1.MainView != layoutView1)
                gridControl1.MainView = layoutView1;
            else
                gridControl1.MainView = gridView1;
        }

        private void simpleButton8_Click(object sender, EventArgs e)
        {
            //gridControl1.Refresh();
            tagsTableAdapter.Update(dataSetTags.Tags);
        }

        private void textEdit3_EditValueChanged(object sender, EventArgs e)
        {

        }

        private TextBox GetInnerTextBox(DevExpress.XtraEditors.TextEdit editor)
        {
            if (editor != null)
                foreach (Control control in editor.Controls)
                    if (control is TextBox)
                        return (TextBox)control;
            return null;
        }        
        
        private void simpleButton2_Click(object sender, EventArgs e)
        {
            // Después obtiene la cantidad total de eventos
            string line;
            comm.GetEventCount();
            if (comm.CantEventos == 0)
            {
                line = string.Format("No hay eventos para descargar\r\n");
                GetInnerTextBox(memoEdit1).AppendText(line);
                return;
            }

            line = string.Format("Bajando {0} eventos...\r\n", comm.CantEventos);
            GetInnerTextBox(memoEdit1).AppendText(line);

            // Arma la tabla de Eventos a procesar luego
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            DataColumn dc;
            dc = new DataColumn("NroTag", Type.GetType("System.Int32"));
            dt.Columns.Add(dc);
            dc = new DataColumn("FecEvento", Type.GetType("System.DateTime"));
            dt.Columns.Add(dc);
            ds.Tables.Add(dt);

            // divide el pedido de eventos en partes, según la cantidad de eventos y eventos x banco
            int eventsToDownload = comm.CantEventos - comm.CantEventosInternos;
            int eventIndex = 1;

            progressBarControl1.Properties.Minimum = 0;
            progressBarControl1.Properties.Maximum = eventsToDownload + 1;
            progressBarControl1.Position = 0;
            int banco = 0;
            int cantidad = 0;
            int eventoInicial = 0;
            while (eventsToDownload > 0)
            {
                banco = (eventIndex - 1) / comm.CantEventosBanco;
                cantidad = Math.Min(eventsToDownload, 10);

                List<Evento> lista = comm.GetEventList(banco, eventoInicial, cantidad);
                if (lista != null)
                {
                    foreach (Evento ev in lista)
                    {
                        DataRow dr = dt.NewRow();
                        dr["NroTag"] = ev.NroTag;
                        dr["FecEvento"] = ev.FecEvento;
                        dt.Rows.Add(dr);

                        string n = ev.NroTag.ToString();
                        while (n.Length < 6)
                            n = "0" + n;

                        line = string.Format("{0} - {1}\r\n", n, ev.FecEvento);
                        GetInnerTextBox(memoEdit1).AppendText(line);
                    }
                    eventIndex += cantidad;
                    eventsToDownload -= cantidad;
                    eventoInicial += 64;
                    if (eventoInicial >= 255)
                        eventoInicial = 0;

                    progressBarControl1.Position = eventIndex;
                    Application.DoEvents();

                    //System.Threading.Thread.Sleep(300);
                }
            }

            if (comm.CantEventosInternos > 0)
            {
                List<Evento> lista = comm.GetEventList(128, 0, comm.CantEventosInternos);
                if (lista != null)
                {
                    foreach (Evento ev in lista)
                    {
                        DataRow dr = dt.NewRow();
                        dr["NroTag"] = ev.NroTag;
                        dr["FecEvento"] = ev.FecEvento;
                        dt.Rows.Add(dr);

                        string n = ev.NroTag.ToString();
                        while (n.Length < 6)
                            n = "0" + n;

                        line = string.Format("{0} - {1}\r\n", n, ev.FecEvento);
                        GetInnerTextBox(memoEdit1).AppendText(line);
                    }
                }
            }

            line = string.Format("Almacenando {0} eventos...\r\n", dt.Rows.Count);
            GetInnerTextBox(memoEdit1).AppendText(line);

            progressBarControl1.Properties.Minimum = 0;
            progressBarControl1.Properties.Maximum = dt.Rows.Count + 1;
            progressBarControl1.Position = 0;
            int i = 0;
            int NroTagPersona = 0;
            foreach (DataRow dr in dt.Rows)
            {
                int NroTag = (int)dr["NroTag"];
                DateTime FecEvento = (DateTime)dr["FecEvento"];

                //if (!data.ExistsTag(NroTag))
                //    data.NewUnknownTag(NroTag, "Tag " + NroTag);

                // Chequeo si tag es persona
                if (data.IsPersonTag(NroTag))
                {
                    NroTagPersona = NroTag;
                }
                else
                {
                    data.NewEvent(NroTagPersona, NroTag, FecEvento);
                }
                
                i++;
                progressBarControl1.Position = i;
                Application.DoEvents();
                //System.Threading.Thread.Sleep(300);
            }

            progressBarControl1.Position = 0;
            line = string.Format("Descarga exitosa\r\n");
            GetInnerTextBox(memoEdit1).AppendText(line);

            if (checkEdit2.Checked)
            {
                line = string.Format("Borrando eventos\r\n");
                GetInnerTextBox(memoEdit1).AppendText(line);
                comm.BorrarEventos();
            }
        }

        private void simpleButton9_Click(object sender, EventArgs e)
        {
            Reporte r = new Reporte();
            r.ShowDialog();
        }

        private void pictureBox1_DoubleClick(object sender, EventArgs e)
        {
            frmTerminal f = new frmTerminal();
            f.Show();
        }

        private void simpleButton3_Click(object sender, EventArgs e)
        {
            comm.GetEventCount();
            textEdit1.Text = comm.CantEventos.ToString();

            textEdit2.Text = "CTRLPROX1";

            DateTime d = comm.Date;
            dateEdit1.DateTime = d;
            timeEdit1.Time = d;

            textEdit3.Text = comm.Version;

            checkEdit1.Checked = comm.Repeticion;
        }

        private void simpleButton4_Click(object sender, EventArgs e)
        {
            DateTime d = dateEdit1.DateTime;
            int dia = d.Day;
            int mes = d.Month;
            int anio = d.Year;
            DateTime d2 = timeEdit1.Time;
            int hora = d2.Hour;
            int minuto = d2.Minute;
            int segundo = d2.Second;

            DateTime d3 = new DateTime(anio, mes, dia, hora, minuto, segundo);
            comm.SetFecha(d3);

            System.Threading.Thread.Sleep(500);

            comm.SetRepeticion(checkEdit1.Checked);

            MessageBox.Show("Configuración guardada");
        }

        private void simpleButton5_Click(object sender, EventArgs e)
        {
            if (comm.BorrarEventos())
                MessageBox.Show("Eventos borrados");
        }

        private void simpleButton6_Click(object sender, EventArgs e)
        {
            if (comm.RecuperarEventos())
                MessageBox.Show("Recupero de eventos exitoso");
        }

        private void simpleButton10_Click(object sender, EventArgs e)
        {
            //if (MessageBox.Show("Está seguro?", "Pregunta", "Confirmación", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly, "") == DialogResult.Yes)
                data.DeleteEventos();
        }
    }
}