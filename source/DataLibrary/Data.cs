using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;

// http://www.csharp-station.com/Tutorials/AdoDotNet/Lesson03.aspx

namespace DataLibrary
{
    public class Data
    {
        private string _connectionString;

        public string ConnectionString
        {
            get { return _connectionString; }
            set { _connectionString = value; }
        }

        private SqlConnection GetConnection()
        {
            SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();
            return connection;
        }

        private string _errorString;

        public string ErrorString
        {
            get { return _errorString; }
        }

        private bool _modoDummy;

        public bool ModoDummy
        {
            get { return _modoDummy; }
            set { _modoDummy = value; }
        }

        public bool NewPersonTag(int NroTag, string DescTag)
        {
            return NewTag(NroTag, DescTag, "P");
        }
        
        public bool NewPlaceTag(int NroTag, string DescTag)
        {
            return NewTag(NroTag, DescTag, "L");
        }

        public bool NewIncidenceTag(int NroTag, string DescTag)
        {
            return NewTag(NroTag, DescTag, "I");
        }

        public bool NewUnknownTag(int NroTag, string DescTag)
        {
            return NewTag(NroTag, DescTag, "D");
        }

        public bool NewTag(int NroTag, string DescTag, string Tipo)
        {
            _errorString = "";
            SqlConnection cn = new SqlConnection(_connectionString);
            string sql = @"INSERT INTO Tags(NroTag,DescTag,Tipo,SnHabilitado) VALUES(@NroTag, @DescTag, @Tipo,'S')";
            SqlCommand cmd = new SqlCommand(sql, cn);

            SqlParameter param1 = new SqlParameter("@NroTag", NroTag);
            SqlParameter param2 = new SqlParameter("@DescTag", DescTag);
            SqlParameter param3 = new SqlParameter("@Tipo", Tipo);
            cmd.Parameters.Add(param1);
            cmd.Parameters.Add(param2);
            cmd.Parameters.Add(param3);

            cn.Open();
            try
            {
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (SqlException ex)
            {
                string errMessage = "";
                for (Exception tempException = ex; tempException != null; tempException = tempException.InnerException)
                {
                    errMessage += tempException.Message + Environment.NewLine + Environment.NewLine;
                }
                _errorString = errMessage;
                return false;
            }
            finally
            {
                cn.Close();
                if (cn != null)
                    cn.Dispose();
            }
        }

        public bool NewEvent(int NroTagPersona, int NroTag, DateTime Fecha)
        {
            _errorString = "";
            SqlConnection cn = new SqlConnection(_connectionString);
            string sql = @"INSERT INTO Eventos(NroTagPersona,NroTag,FecEvento) VALUES(@NroTagPersona, @NroTag, @FecEvento)";
            SqlCommand cmd = new SqlCommand(sql, cn);

            SqlParameter param0 = new SqlParameter("@NroTagPersona", NroTagPersona);
            SqlParameter param1 = new SqlParameter("@NroTag", NroTag);
            SqlParameter param2 = new SqlParameter("@FecEvento", Fecha);
            cmd.Parameters.Add(param0);
            cmd.Parameters.Add(param1);
            cmd.Parameters.Add(param2);

            cn.Open();
            try
            {
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (SqlException ex)
            {
                string errMessage = "";
                for (Exception tempException = ex; tempException != null; tempException = tempException.InnerException)
                {
                    errMessage += tempException.Message + Environment.NewLine + Environment.NewLine;
                }
                _errorString = errMessage;
                return false;
            }
            finally
            {
                cn.Close();
                if (cn != null)
                    cn.Dispose();
            }
        }

        public DataTable GetEvents()
        {
            SqlConnection cn = new SqlConnection(_connectionString);
            _errorString = "";

            cn.Open();
            try
            {
                DataSet ds = new DataSet();

                string sql = @"SELECT * FROM EVENTOS";
                SqlCommand com = new SqlCommand(sql, cn);

                SqlDataAdapter da = new SqlDataAdapter(sql, cn);

                da.Fill(ds);

                return ds.Tables[0];
            }
            catch (SqlException ex)
            {
                string errMessage = "";
                for (Exception tempException = ex; tempException != null; tempException = tempException.InnerException)
                {
                    errMessage += tempException.Message + Environment.NewLine + Environment.NewLine;
                }
                _errorString = errMessage;
                return null;
            }
            finally
            {
                cn.Close();
                if (cn != null)
                    cn.Dispose();
            }
        }

        public DataTable GetTags()
        {
            SqlConnection cn = new SqlConnection(_connectionString);
            _errorString = "";

            cn.Open();
            try
            {
                DataSet ds = new DataSet();

                string sql = @"SELECT * FROM Tags";
                SqlCommand com = new SqlCommand(sql, cn);

                SqlDataAdapter da = new SqlDataAdapter(sql, cn);

                da.Fill(ds);

                return ds.Tables[0];
            }
            catch (SqlException ex)
            {
                string errMessage = "";
                for (Exception tempException = ex; tempException != null; tempException = tempException.InnerException)
                {
                    errMessage += tempException.Message + Environment.NewLine + Environment.NewLine;
                }
                _errorString = errMessage;
                return null;
            }
            finally
            {
                cn.Close();
                if (cn != null)
                    cn.Dispose();
            }
        }

        public bool ExistsTag(int NroTag)
        {
            _errorString = "";
            SqlConnection cn = new SqlConnection(_connectionString);
            string sql = @"SELECT COUNT(1) FROM Tags WHERE NroTag = @NroTag";
            SqlCommand cmd = new SqlCommand(sql, cn);

            SqlParameter param1 = new SqlParameter("@NroTag", NroTag);
            cmd.Parameters.Add(param1);

            cn.Open();
            try
            {
                int count = (int)cmd.ExecuteScalar();
                return (count > 0);
            }
            catch (SqlException ex)
            {
                string errMessage = "";
                for (Exception tempException = ex; tempException != null; tempException = tempException.InnerException)
                {
                    errMessage += tempException.Message + Environment.NewLine + Environment.NewLine;
                }
                _errorString = errMessage;
                return false;
            }
            finally
            {
                cn.Close();
                if (cn != null)
                    cn.Dispose();
            }
        }

        public bool GenerateTipoTags()
        {
            _errorString = "";
            SqlConnection cn = new SqlConnection(_connectionString);
            string sql = @"INSERT INTO TipoTag(Tipo,DescTipo) VALUES(@Tipo, @DescTipo)";
            SqlCommand cmd = new SqlCommand(sql, cn);

            SqlParameter param1 = new SqlParameter("@Tipo", null);
            SqlParameter param2 = new SqlParameter("@DescTipo", null);
            cmd.Parameters.Add(param1);
            cmd.Parameters.Add(param2);

            cn.Open();
            try
            {
                param1.Value = "P";
                param2.Value = "Persona";
                cmd.ExecuteNonQuery();

                param1.Value = "L";
                param2.Value = "Lugar";
                cmd.ExecuteNonQuery();

                param1.Value = "I";
                param2.Value = "Incidencia";
                cmd.ExecuteNonQuery();

                param1.Value = "D";
                param2.Value = "Desconocido";
                cmd.ExecuteNonQuery();

                return true;
            }
            catch (SqlException ex)
            {
                string errMessage = "";
                for (Exception tempException = ex; tempException != null; tempException = tempException.InnerException)
                {
                    errMessage += tempException.Message + Environment.NewLine + Environment.NewLine;
                }
                _errorString = errMessage;
                return false;
            }
            finally
            {
                cn.Close();
                if (cn != null)
                    cn.Dispose();
            }
        }

        public Evento GetLastEvent()
        {
            /*
            _errorString = "";
            SqlConnection cn = new SqlConnection(_connectionString);
            string sql = @"SELECT NroTag FROM Eventos WHERE FecEvento = (SELECT MAX(Fec)";
            SqlCommand cmd = new SqlCommand(sql, cn);

            SqlParameter param1 = new SqlParameter("@NroTag", NroTag);
            cmd.Parameters.Add(param1);

            cn.Open();
            try
            {
                int count = (int)cmd.ExecuteScalar();
                return (count > 0);
            }
            catch (SqlException ex)
            {
                string errMessage = "";
                for (Exception tempException = ex; tempException != null; tempException = tempException.InnerException)
                {
                    errMessage += tempException.Message + Environment.NewLine + Environment.NewLine;
                }
                _errorString = errMessage;
                return false;
            }
            finally
            {
                cn.Close();
                if (cn != null)
                    cn.Dispose();
            }
            */
            return null;
        }

        public bool IsPersonTag(int NroTag)
        {
            _errorString = "";
            SqlConnection cn = new SqlConnection(_connectionString);
            string sql = @"SELECT count(*) FROM Tags WHERE Tipo = 'P' and NroTag = @NroTag";
            SqlCommand cmd = new SqlCommand(sql, cn);

            SqlParameter param1 = new SqlParameter("@NroTag", NroTag);
            cmd.Parameters.Add(param1);

            cn.Open();
            try
            {
                int count = (int)cmd.ExecuteScalar();
                return (count > 0);
            }
            catch (SqlException ex)
            {
                string errMessage = "";
                for (Exception tempException = ex; tempException != null; tempException = tempException.InnerException)
                {
                    errMessage += tempException.Message + Environment.NewLine + Environment.NewLine;
                }
                _errorString = errMessage;
                return false;
            }
            finally
            {
                cn.Close();
                if (cn != null)
                    cn.Dispose();
            }
        }

        public bool DeleteEventos()
        {
            _errorString = "";
            SqlConnection cn = new SqlConnection(_connectionString);
            string sql = @"DELETE From Eventos";
            SqlCommand cmd = new SqlCommand(sql, cn);

            cn.Open();
            try
            {
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (SqlException ex)
            {
                string errMessage = "";
                for (Exception tempException = ex; tempException != null; tempException = tempException.InnerException)
                {
                    errMessage += tempException.Message + Environment.NewLine + Environment.NewLine;
                }
                _errorString = errMessage;
                return false;
            }
            finally
            {
                cn.Close();
                if (cn != null)
                    cn.Dispose();
            }
        }

    }
}

