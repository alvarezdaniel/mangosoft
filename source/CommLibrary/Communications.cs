using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using DataLibrary;
using System.Data;

namespace CommLibrary
{
    public class Communications
    {
        private SerialPort comPort = new SerialPort();

        private int _baudRate = 9600;
        private int _dataBits = 8;
        private StopBits _stopBits = StopBits.One;
        private Parity _parity = Parity.None;
        private string _portName = "COM1";
        private int _timeout = 2000;
        private int _cantEventos = -1;

        public int CantEventos
        {
            get { return _cantEventos; }
            set { _cantEventos = value; }
        }
        private int _cantEventosBanco = -1;

        public int CantEventosBanco
        {
            get { return _cantEventosBanco; }
            set { _cantEventosBanco = value; }
        }

        private int _cantEventosInternos;

        public int CantEventosInternos
        {
            get { return _cantEventosInternos; }
            set { _cantEventosInternos = value; }
        }
        
        private string _version = "";
        private DateTime _fecha;

        private bool _modoDummy;

        public bool ModoDummy
        {
            get { return _modoDummy; }
            set { _modoDummy = value; }
        }


        public void Config(int BaudRate, int DataBits, StopBits StopBits, Parity Parity, string PortName, int Timeout)
        {
            _baudRate = BaudRate;
            _dataBits = DataBits;
            _stopBits = StopBits;
            _parity = Parity;
            _portName = PortName;
            _timeout = Timeout;
        }
        
        private void Open()
        {
            if (comPort.IsOpen)
                return;

            // Set the port's settings
            comPort.BaudRate = _baudRate;
            comPort.DataBits = _dataBits;
            comPort.StopBits = _stopBits;
            comPort.Parity = _parity;
            comPort.PortName = _portName;
            comPort.ReadTimeout = _timeout;

            // Open the port
            comPort.Open();
        }

        private void Close()
        {
            if (comPort.IsOpen)
                comPort.Close();
        }

        public StatusMango CommunicationStatus
        {
            get
            {
                if (comPort.IsOpen)
                    comPort.Close();

                try
                {
                    Open();

                    // si base está pero no mango: "nesta"
                    // si base y mango están: "esta"
                    comPort.Write("chk" + comPort.NewLine);
                    try
                    {
                        string s = comPort.ReadLine();

                        if (s.Equals("nta"))
                            return StatusMango.OkBase;
                        else if (s.Equals("sta"))
                            return StatusMango.OkBaseYMango;
                        else
                            return StatusMango.NoCommunication;
                    }
                    catch
                    {
                        return StatusMango.NoCommunication;
                    }
                }
                finally
                {
                    Close();
                }
            }
        }

        public bool GetEventCount()
        {
            if (_modoDummy)
            {
                _cantEventos = 108;
                _cantEventosBanco = 30;
                return true;
            }
            
            if (comPort.IsOpen)
                comPort.Close();

            try
            {
                Open();

                // devuelve "NNN1-N2-N3" 
                // NNN1=cantidad de eventos, N2:cantidad de eventos x banco, N3:cant. eventos internos
                comPort.Write("gec" + comPort.NewLine);
                try
                {
                    string s = comPort.ReadLine();

                    string[] array = s.Split("-".ToCharArray());

                    _cantEventos = int.Parse(array[0], System.Globalization.NumberStyles.HexNumber);
                    _cantEventosBanco = int.Parse(array[1], System.Globalization.NumberStyles.HexNumber);
                    _cantEventosInternos = int.Parse(array[2], System.Globalization.NumberStyles.HexNumber);

                    return true;
                }
                catch
                {
                    return false;
                }
            }
            finally
            {
                Close();
            }
        }

        public string Version
        {
            get
            {
                if (comPort.IsOpen)
                    comPort.Close();

                try
                {
                    Open();

                    // devuelve string de versión, modelo, fecha, etc 
                    comPort.Write("gve" + comPort.NewLine);
                    try
                    {
                        string s = comPort.ReadLine();
                        _version = s;
                        return s;
                    }
                    catch
                    {
                        return string.Empty;
                    }
                }
                finally
                {
                    Close();
                }
            }
        }

        public List<Evento> GetEventList(int banco, int eventIndex, int cantidad)
        {
            /*
            List<Evento> lis = new List<Evento>();
            for (int i = 0; i <= cantidad - 1; i++)
            {
                Evento ev = new Evento();
                ev.NroTag = i;
                ev.FecEvento = DateTime.Now;
                lis.Add(ev);
            }
            //System.Threading.Thread.Sleep(500);
            return lis;
            */
            
            if (comPort.IsOpen)
                comPort.Close();

            try
            {
                Open();

                // geteven-X1-X2-X3  (X1: banco, X2: evento inicial, X3: cantidad 1-10)
                // devuelve string binario en hexa + crc
                // evento, paquete de 6 bytes
                // b1: MSB Tag
                // b2: LSB Tag
                // b3: 2 bits MSB mes, 6 bits minutos
                // b4: 2 bits LSB mes, 5 bits hora, 1 bit MSB dia
                // b5: 4 bits LSB dia, 4 bits MSB hora
                // b6: 2 bits LSB segundos, 6 bits año
                // crc xor al final
                string cmd = "gev" +
                    HexUtils.HexaPadded(banco) +
                    HexUtils.HexaPadded(eventIndex) +
                    HexUtils.HexaPadded(cantidad) + comPort.NewLine;
                comPort.Write(cmd);
                try
                {
                    string s;
                    if (_modoDummy)
                    {
                        /*
                        5232 10/02/05  04:07:55
                        14 70 07 88 AD C5
                     
                        1555 12/10/08 12:07:43
                        06 13 87 98 CA C8
                     
                        3248 13/12/08 11:25:36
                        0C B0 D9 16 D9 08
                     
                        10345 25/10/08 16:52:05
                        28 69 B4 A1 91 48
                     
                        24352 28/11/08 17:26:18
                        5F 20 9A E3 C6 88
                     
                        2328 02/05/08 21:32:25
                        09 18 60 6A 26 48
                     
                        3435 01/04/09 03:28:51
                        0D B6 5C 06 1C C9
                     
                        38888 31/03/09 03:32:58
                        97 E8 20 C7 FE 89
                        */
                        
                        string[] eventos = new string[8] {"14700788ADC5", "06138798CAC8",
                           "0CB0D916D908", "2869B4A19148", "5F209AE3C688",
                           "0918606A2648", "0D6B5C061CC9", "97E820C7FE89" };

                        Random r = new Random(DateTime.Now.Millisecond);
                        StringBuilder sb = new StringBuilder();
                        for (int i=0; i<=cantidad-1; i++)
                        {
                            int index = r.Next(7);
                            sb.Append(eventos[index]);
                        }
                        sb.Append("00");
                        s = sb.ToString();
                    }
                    else
                    {
                        s = comPort.ReadLine();
                    }
                    
                    // Convierte string hexa a binario
                    int discarded = 0;
                    byte[] array = HexEncoding.GetBytes(s, out discarded);

                    // Chequea crc

                    if (!_modoDummy)
                    {
                        byte crc1 = array[array.Length - 1];
                        byte crc2 = 0;
                        for (int i = 0; i <= array.Length - 2; i++)
                            crc2 = (byte)(crc2 ^ array[i]);

                        if (crc1 != crc2)
                            throw new Exception("Failed CRC check");
                    }

                    // Extrae lista de eventos
                    List<Evento> lista = new List<Evento>();
                    for (int i = 0; i <= cantidad - 1; i++)
                    {
                        byte[] eventArray = new byte[6];

                        eventArray[0] = array[(i * 6)];
                        eventArray[1] = array[(i * 6) + 1];
                        eventArray[2] = array[(i * 6) + 2];
                        eventArray[3] = array[(i * 6) + 3];
                        eventArray[4] = array[(i * 6) + 4];
                        eventArray[5] = array[(i * 6) + 5];

                        Evento ev = new Evento();
                        ev.ParseFromByteArray(eventArray);
                        lista.Add(ev);
                    }

                    // Devuelve la lista
                    return lista;
                }
                catch
                {
                    return null;
                }
            }
            finally
            {
                Close();
            }
        }

        public DateTime Date
        {
            get
            {
                if (comPort.IsOpen)
                    comPort.Close();

                try
                {
                    Open();

                    // hhmmssddMMyy
                    comPort.Write("gfe" + comPort.NewLine);
                    try
                    {
                        string s = comPort.ReadLine();

                        int hora = int.Parse(s.Substring(0, 2));
                        int minuto = int.Parse(s.Substring(2, 2));
                        int segundo = int.Parse(s.Substring(4, 2));
                        
                        int dia = int.Parse(s.Substring(6, 2));
                        int mes = int.Parse(s.Substring(8, 2));
                        int anio = int.Parse(s.Substring(10, 2));

                        DateTime d = new DateTime(anio + 2000, mes, dia, hora, minuto, segundo);
                        _fecha = d;

                        return _fecha;
                    }
                    catch
                    {
                        return DateTime.MinValue;
                    }
                }
                finally
                {
                    Close();
                }
            }
        }

        public bool Repeticion
        {
            get
            {
                if (comPort.IsOpen)
                    comPort.Close();

                try
                {
                    Open();

                    // y/n
                    comPort.Write("gre" + comPort.NewLine);
                    try
                    {
                        string s = comPort.ReadLine();
                        return s.Equals("y");
                    }
                    catch
                    {
                        return false;
                    }
                }
                finally
                {
                    Close();
                }
            }
        }

        public bool SetFecha(DateTime fecha)
        {
            if (comPort.IsOpen)
                comPort.Close();

            try
            {
                Open();

                string hh = fecha.Hour.ToString();
                if (hh.Length == 1) hh = "0" + hh;
                string mm = fecha.Minute.ToString();
                if (mm.Length == 1) mm = "0" + mm;
                string ss = fecha.Second.ToString();
                if (ss.Length == 1) ss = "0" + ss;

                string dd = fecha.Day.ToString();
                if (dd.Length == 1) dd = "0" + dd;
                string MM = fecha.Month.ToString();
                if (MM.Length == 1) MM = "0" + MM;
                string yy = (fecha.Year-2000).ToString();
                if (yy.Length == 1) yy = "0" + yy;

                // hhmmssddMMyy
                // responde ack
                comPort.Write("sfe" + hh + mm + ss + dd + MM + yy + comPort.NewLine);
                try
                {
                    string s = comPort.ReadLine();
                    return s.Equals("ack");
                }
                catch
                {
                    return false;
                }
            }
            finally
            {
                Close();
            }
        }

        public bool SetRepeticion(bool repeticion)
        {
            if (comPort.IsOpen)
                comPort.Close();

            try
            {
                Open();

                // y/n
                // responde ack
                comPort.Write("sre" + (repeticion ? "y" : "n") + comPort.NewLine);
                try
                {
                    string s = comPort.ReadLine();
                    return s.Equals("ack");
                }
                catch
                {
                    return false;
                }
            }
            finally
            {
                Close();
            }
        }

        public bool BorrarEventos()
        {
            if (comPort.IsOpen)
                comPort.Close();

            try
            {
                Open();

                // responde ack
                comPort.Write("bev" + comPort.NewLine);
                try
                {
                    string s = comPort.ReadLine();
                    return s.Equals("ack");
                }
                catch
                {
                    return false;
                }
            }
            finally
            {
                Close();
            }
        }

        public bool RecuperarEventos()
        {
            if (comPort.IsOpen)
                comPort.Close();

            try
            {
                Open();

                // responde ack
                comPort.Write("rev" + comPort.NewLine);
                try
                {
                    string s = comPort.ReadLine();
                    return s.Equals("ack");
                }
                catch
                {
                    return false;
                }
            }
            finally
            {
                Close();
            }
        }
    }
}
