using System;
using System.Collections.Generic;
using System.Text;

namespace DataLibrary
{
    public class Evento
    {
        private int _nroTag;

        public int NroTag
        {
            get { return _nroTag; }
            set { _nroTag = value; }
        }
        private DateTime _fecEvento;

        public DateTime FecEvento
        {
            get { return _fecEvento; }
            set { _fecEvento = value; }
        }

        public void ParseFromByteArray(byte[] array)
        {
            // evento, paquete de 6 bytes
            // b1: MSB Tag
            // b2: LSB Tag
            // b3: 2 bits MSB mes, 6 bits minutos
            // b4: 2 bits LSB mes, 5 bits hora, 1 bit MSB dia
            // b5: 4 bits LSB dia, 4 bits MSB segundos
            // b6: 2 bits LSB segundos, 6 bits año

            _nroTag = array[0] * 256 + array[1];

            int dia, mes, anio, horas, minutos, segundos;

            mes = ((array[2] >> 6) << 2)  + (array[3] >> 6);
            minutos = array[2] & 0x3F;
            horas = (array[3] >> 1) & 0x1F;
            dia = ((array[3] & 0x01) << 4) + ((array[4] >> 4) & 0x0F);
            segundos = ((array[4] & 0x0F) << 2) + ((array[5] >> 6) & 0x3);
            anio = (array[5] & 0x3F) + 2000;

            try
            {
                _fecEvento = new DateTime(anio, mes, dia, horas, minutos, segundos);
            }
            catch
            {
                _fecEvento = DateTime.MaxValue;
            }
        }
    }
}
