using System;
using System.Data;
using System.Text;
using System.Drawing;
using System.IO.Ports;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;

using ControlProx.Properties;
using CommLibrary;
using DataLibrary;

namespace SerialPortTerminal
{
    public partial class frmTerminal : Form
    {
        #region Local Variables
        private Communications comm;
        private Data data;
        //private SerialPort comportEmu;
        #endregion

        #region Constructor
        public frmTerminal()
        {
            // Build the form
            InitializeComponent();

            // Restore the users settings
            InitializeControlValues();
        }
        #endregion

        /// <summary> Populate the form's controls with default settings. </summary>
        private void InitializeControlValues()
        {
        }

        private void frmTerminal_Shown(object sender, EventArgs e)
        {
            comm = new Communications();
            comm.Config(Settings.Default.BaudRate, Settings.Default.DataBits, Settings.Default.StopBits,
                Settings.Default.Parity, Settings.Default.PortName, Settings.Default.Timeout);
            comm.ModoDummy = Settings.Default.ModoDummy;

            data = new Data();
            data.ConnectionString = Settings.Default.BaseConnectionString;
            data.ModoDummy = Settings.Default.ModoDummy;

            /*
            comportEmu = new SerialPort();
            comportEmu.BaudRate = Settings.Default.BaudRate;
            comportEmu.DataBits = Settings.Default.DataBits;
            comportEmu.StopBits = Settings.Default.StopBits;
            comportEmu.Parity = Settings.Default.Parity;
            comportEmu.PortName = "COM4";
            comportEmu.Open();
            comportEmu.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);
            */
        }
        
        private void frmTerminal_FormClosing(object sender, FormClosingEventArgs e)
        {
            /*
            comportEmu.Close();
            */
        }

        /*
        private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string data = comportEmu.ReadExisting();

            textBoxRX.Text = data;
            textBoxRXHexa.Text = StringToHexString(data);
        }
        */

        private void btnSendMango_Click(object sender, EventArgs e)
        {
            //comportEmu.Write(textBoxTX.Text + comportEmu.NewLine);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            switch (comm.CommunicationStatus)
            {
                case StatusMango.NoCommunication:
                    MessageBox.Show("No hay comunicación con la base");
                    break;
                case StatusMango.OkBase:
                    MessageBox.Show("Hay comunicación con la base pero no está el mango");
                    break;
                case StatusMango.OkBaseYMango:
                    MessageBox.Show("La comunicación con el mango está ok");
                    break;
                default:
                    break;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (comm.GetEventCount())
            {
                MessageBox.Show("Cantidad de eventos: " + comm.CantEventos + ", eventos x banco = " + comm.CantEventosBanco);
            }
            else
            {
                MessageBox.Show("No hay comunicación con la base");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            MessageBox.Show(comm.Version);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            List<Evento> lista = comm.GetEventList(int.Parse(textBox1.Text), int.Parse(textBox2.Text), int.Parse(textBox7.Text));
            if (lista != null)
            {
                MessageBox.Show("Se obtuvieron " + lista.Count + " eventos");
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            DateTime d = comm.Date;
            MessageBox.Show(d.ToShortDateString() + " - " + d.ToShortTimeString());
        }

        private void button6_Click(object sender, EventArgs e)
        {
            MessageBox.Show(comm.Repeticion.ToString());
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (comm.SetFecha(DateTime.Now))
                MessageBox.Show("SetFecha ok");
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (comm.SetRepeticion(checkBox1.Checked))
                MessageBox.Show("SetRepeticion ok");
        }

        private void button9_Click(object sender, EventArgs e)
        {
            if (comm.BorrarEventos())
                MessageBox.Show("Eventos Borrados");
        }

        private void button10_Click(object sender, EventArgs e)
        {
            if (comm.RecuperarEventos())
                MessageBox.Show("Eventos Recuperados");
        }

        private void button11_Click(object sender, EventArgs e)
        {
            if (data.NewPersonTag(int.Parse(textBox3.Text), textBox4.Text))
                MessageBox.Show("Tag Guardado");
            else
                MessageBox.Show(data.ErrorString);
        }

        private void button15_Click(object sender, EventArgs e)
        {
            for (int i = 0; i <= numericUpDown1.Value - 1; i++)
            {
                data.NewPersonTag(i, "Persona " + i);
            }
            MessageBox.Show(numericUpDown1.Value + " tags guardados");
        }

        private void button12_Click(object sender, EventArgs e)
        {
            if (data.NewEvent(0, int.Parse(textBox5.Text), DateTime.Parse(textBox6.Text)))
                MessageBox.Show("Evento Guardado");
            else
                MessageBox.Show(data.ErrorString);
        }

        private void button13_Click(object sender, EventArgs e)
        {
            dataGridView1.DataSource = data.GetEvents();
        }

        private void button14_Click(object sender, EventArgs e)
        {
            // Después obtiene la cantidad total de eventos
            comm.GetEventCount();

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
            int eventsToDownload = comm.CantEventos;
            int eventIndex = 1;

            progressBar1.Minimum = 0;
            progressBar1.Maximum = eventsToDownload + 1;
            progressBar1.Value = 0;
            while (eventsToDownload > 0)
            {
                int banco = (eventIndex - 1) / comm.CantEventosBanco;
                int cantidad = Math.Min(eventsToDownload, 40);
                int eventoInicial = 0;
                
                List<Evento> lista = comm.GetEventList(banco, eventoInicial, cantidad);
                if (lista != null)
                {
                    foreach (Evento ev in lista)
                    {
                        DataRow dr = dt.NewRow();
                        dr["NroTag"] = ev.NroTag;
                        dr["FecEvento"] = ev.FecEvento;
                        dt.Rows.Add(dr);
                    }
                    eventIndex += cantidad;
                    eventsToDownload -= cantidad;
                    progressBar1.Value = eventIndex;
                    Application.DoEvents();
                    
                    //System.Threading.Thread.Sleep(300);
                }
            }

            progressBar1.Minimum = 0;
            progressBar1.Maximum = dt.Rows.Count+1;
            progressBar1.Value = 0;
            int i = 0;
            foreach (DataRow dr in dt.Rows)
            {
                int NroTag = (int)dr["NroTag"];
                DateTime FecEvento = (DateTime)dr["FecEvento"];
                
                if (!data.ExistsTag(NroTag))
                    data.NewUnknownTag(NroTag, "Tag " + NroTag);

                //data.NewEvent(NroTag, FecEvento);
                i++;
                progressBar1.Value = i;
                Application.DoEvents();
                //System.Threading.Thread.Sleep(300);
            }

            dataGridView1.DataSource = dt;
        }

        private void button16_Click(object sender, EventArgs e)
        {
            dataGridView1.DataSource = data.GetTags();
        }

        private void button17_Click(object sender, EventArgs e)
        {
            data.NewPersonTag(60271, "Guardia 1");
            data.NewPersonTag(1340, "Guardia 2");

            data.NewPlaceTag(40456, "Lugar 1");
            data.NewPlaceTag(60785, "Lugar 2");
            data.NewPlaceTag(40970, "Lugar 3");
            data.NewPlaceTag(21155, "Lugar 4");
            data.NewPlaceTag(47061, "Lugar 5");
            data.NewPlaceTag(27246, "Lugar 6");

            MessageBox.Show("OK");
        }
    }
}