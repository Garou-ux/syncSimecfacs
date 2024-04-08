using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using Dapper;
using System.ComponentModel;

namespace simecFacs
{
    public partial class Form1 : Form
    {
        private string connectionStringMySQL; // Cadena de conexión a MySQL

        public Form1(string connectionStringMySQL)
        {
            InitializeComponent();
            this.connectionStringMySQL = connectionStringMySQL;
        }

        static int ObtenerUltimoIdMySQL(string cadenaConexion, string query)
        {
            int ultimoId = 0;

            using (MySqlConnection conexion = new MySqlConnection(cadenaConexion))
            {
                using (MySqlCommand comando = new MySqlCommand(query, conexion))
                {
                    try
                    {
                        conexion.Open();
                        object resultado = comando.ExecuteScalar();
                        if (resultado != DBNull.Value)
                        {
                            ultimoId = Convert.ToInt32(resultado);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al obtener el último ID: {ex.Message}");
                    }
                }
            }

            return ultimoId;
        }

        public void syncFacs()
        {
            FoxQuerysToDataset instancia = new FoxQuerysToDataset();
            //string queryFoxPro = "select Cidfoldig, Ciddocto, Cfolio, Carchdidis from MGW10045 where Crazon != '' AND Cidfoldig > 3038";
            string ultimoIdQuery = "select max(c_id_docto) from simecFacs.factura_detalles order by id desc;";
            int ultimoId = ObtenerUltimoIdMySQL(connectionStringMySQL, ultimoIdQuery);

            // Construir la consulta final
            string queryFoxPro = $"SELECT Cidfoldig, Ciddocto, Cfolio, Carchdidis FROM MGW10045 WHERE Crazon != '' AND Cidfoldig > {ultimoId}";
            //string queryFoxPro = "select Cidfoldig, Ciddocto, Cfolio, Carchdidis from MGW10045 where Crazon != ''";
            DataSet dsFoxPro = instancia.ObtenerDataSetDesdeFoxPro(queryFoxPro);

            List<DatosDTO> resultados = new List<DatosDTO>();

            foreach (DataRow row in dsFoxPro.Tables[0].Rows)
            {
                resultados.Add(new DatosDTO
                {
                    Cidfoldig = row["Cidfoldig"].ToString(),
                    Ciddocto = row["Ciddocto"].ToString(),
                    Cfolio = row["Cfolio"].ToString(),
                    Carchdidis = row["Carchdidis"].ToString()
                });
            }

            // creamos los archivos relacionados a las facturas
            IRepository<DatosDTO> repository = new MySQLRepository<DatosDTO>(connectionStringMySQL);
            repository.Insert(resultados);

            //insertamos las facturas
            //string queryFacturas = "select Ciddocum01, Ciddocum02, Cidconce01, Cfolio, Cfechave01, Cidclien01, Cneto, Ctotal from MGW10008 WHERE Ciddocum01 > 3126";
            string ultimoIdQuery = "select max(c_id_doc_path) from simecFacs.facturas order by id desc;";
            int ultimoId = ObtenerUltimoIdMySQL(connectionStringMySQL, ultimoIdQuery);
            string queryFacturas = "select Ciddocum01, Ciddocum02, Cidconce01, Cfolio, Cfechave01, Cidclien01, Cneto, Ctotal from MGW10008";
            DataSet dxFacturas = instancia.ObtenerDataSetDesdeFoxPro(queryFacturas);
            if (dxFacturas != null)
            {
                List<Factura> resFacturas = new List<Factura>();

                foreach (DataRow row in dxFacturas.Tables[0].Rows)
                {
                    resFacturas.Add(new Factura
                    {
                        Ciddocum01 = row["Ciddocum01"].ToString(),
                        Ciddocum02 = row["Ciddocum02"].ToString(),
                        Cidconce01 = row["Cidconce01"].ToString(),
                        Cfolio = row["Cfolio"].ToString(),
                        Cfechave01 = row["Cfechave01"].ToString(),
                        Cidclien01 = row["Cidclien01"].ToString(),
                        Cneto = decimal.Parse(row["Cneto"].ToString()),
                        Ctotal = decimal.Parse(row["Ctotal"].ToString())
                    });
                }

                IRepository<Factura> repository1 = new MySQLRepository<Factura>(connectionStringMySQL);
                repository1.InsertFactura(resFacturas);
            }

            //InsertFacturaDetalles
            //string queryFacturaDetalles = "select Ciddocum01, Cidprodu01 from MGW10010 WHERE Ciddocum01 > 3126";
            string queryFacturaDetalles = "select Ciddocum01, Cidprodu01 from MGW10010";
            DataSet dxFacturaDetalles = instancia.ObtenerDataSetDesdeFoxPro(queryFacturaDetalles);
            if ( dxFacturaDetalles != null )
            {
                List<FacturaDetalle> resFacturaDetalles = new List<FacturaDetalle>();

                foreach (DataRow row in dxFacturaDetalles.Tables[0].Rows)
                {
                    resFacturaDetalles.Add(new FacturaDetalle
                    {
                        Ciddocum01 = row["Ciddocum01"].ToString(),
                        Cidprodu01 = row["Cidprodu01"].ToString()
                    });
                }

                IRepository<FacturaDetalle> repository2 = new MySQLRepository<FacturaDetalle>(connectionStringMySQL);
                repository2.InsertFacturaDetalles(resFacturaDetalles);
            }

            //insertClientes
            //string queryClientes = "select Cidclien01, Ccodigoc01, Crazonso01, Crfc, Cfechaalta from MGW10002 WHERE Cidclien01 > 49";
            string queryClientes = "select Cidclien01, Ccodigoc01, Crazonso01, Crfc, Cfechaalta from MGW10002";
            DataSet dxClientes = instancia.ObtenerDataSetDesdeFoxPro(queryClientes);
            if (dxClientes != null)
            {
                List<Cliente> resClientes = new List<Cliente>();

                foreach (DataRow row in dxClientes.Tables[0].Rows)
                {
                    resClientes.Add(new Cliente
                    {
                        Cidclien01 = row["Cidclien01"].ToString(),
                        Ccodigoc01 = row["Ccodigoc01"].ToString(),
                        Crazonso01 = row["Crazonso01"].ToString(),
                        Crfc = row["Crfc"].ToString(),
                        Cfechaalta = row["Cfechaalta"].ToString(),
                    });
                }

                IRepository<Cliente> repository3 = new MySQLRepository<Cliente>(connectionStringMySQL);
                repository3.InsertClientes(resClientes);
            }

            //insertProduicts
            //string queryProductos = "select Cidprodu01, Ccodigop01, Cnombrep01, Ctipopro01, Cclavesat, Cfechaal01 from MGW10005 WHERE Cidprodu01 > 806";
            string queryProductos = "select Cidprodu01, Ccodigop01, Cnombrep01, Ctipopro01, Cclavesat, Cfechaal01 from MGW10005";
            DataSet dxProductos = instancia.ObtenerDataSetDesdeFoxPro(queryProductos);
            if (dxProductos != null)
            {
                List<Producto> resProductos = new List<Producto>();

                foreach (DataRow row in dxProductos.Tables[0].Rows)
                {
                    resProductos.Add(new Producto
                    {
                        Cidprodu01 = row["Cidprodu01"].ToString(),
                        Ccodigop01 = row["Ccodigop01"].ToString(),
                        Cnombrep01 = row["Cnombrep01"].ToString(),
                        Ctipopro01 = row["Ctipopro01"].ToString(),
                        Cclavesat = row["Cclavesat"].ToString(),
                        Cfechaal01 = row["Cfechaal01"].ToString()
                    });
                }

                IRepository<Producto> repository4 = new MySQLRepository<Producto>(connectionStringMySQL);
                repository4.InsertProductos(resProductos);
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {

            btnFiltrar.Enabled = false;
            MessageBox.Show("Se esta sincronizando la informacion, espere un momento");

            backgroundWorker1.RunWorkerAsync();
            syncFacs();
            
            MessageBox.Show("Sincronizacion terminada");
            btnFiltrar.Enabled = true;
        }
    }

    public class DatosDTO
    {
        // Cidfoldig, Ciddocto, Cfolio, Carchdidis
        public string Cidfoldig { get; set; }
        public string Ciddocto { get; set; }
        public string Cfolio { get; set; }
        public string Carchdidis { get; set; }
    }

    public class Factura
    {
        //c_id_doc_path, c_id_tipodoc, c_id_conce, c_folio, c_fecha, c_id_cliente, c_neto, c_total
        public string Ciddocum01 { get; set; }
        public string Ciddocum02 { get; set; }
        public string Cidconce01 { get; set; }
        public string Cfolio { get; set; }
        public string Cfechave01 { get; set; }
        public string Cidclien01 { get; set; }
        public decimal Cneto { get; set; }
        public decimal Ctotal { get; set; }
    }

    public class FacturaDetalle
    {
        public string Ciddocum01 { get; set; }
        public string Cidprodu01 { get; set; }
    }

    public class Cliente
    {
        public string Cidclien01 { get; set; }
        public string Ccodigoc01 { get; set; }
        public string Crazonso01 { get; set; }
        public string Crfc { get; set; }
        public string Cfechaalta { get; set; }
    }

    public class Producto
    {
        public string Cidprodu01 { get; set; }
        public string Ccodigop01 { get; set; }
        public string Cnombrep01 { get; set; }
        public string Ctipopro01 { get; set; }
        public string Cclavesat { get; set; }
        public string Cfechaal01 { get; set; }
    }

    public class FoxQuerysToDataset
    {
        // Esta función recibe el query y devuelve un DataSet
        public DataSet ObtenerDataSetDesdeFoxPro(string query)
        {
            // Cadena de conexión a tu base de datos FoxPro
            string cadenaFoxPro = "Provider=vfpoledb; Data Source = C:\\; Extended Properties = dBase IV";

            // Crear la conexión a la base de datos
            using (OleDbConnection conFoxPro = new OleDbConnection(cadenaFoxPro))
            {
                try
                {
                    // Abrir la conexión
                    conFoxPro.Open();

                    // Crear el adaptador de datos con el query proporcionado
                    using (OleDbDataAdapter adapterFoxPro = new OleDbDataAdapter(query, conFoxPro))
                    {
                        // Crear un nuevo DataSet
                        DataSet dsFoxPro = new DataSet();

                        // Llenar el DataSet con los datos obtenidos del query
                        adapterFoxPro.Fill(dsFoxPro);

                        // Devolver el DataSet lleno
                        return dsFoxPro;
                    }
                }
                catch (Exception ex)
                {
                    // Manejar cualquier excepción que pueda ocurrir
                    Console.WriteLine("Error al obtener datos desde FoxPro: " + ex.Message);
                    return null; // Devolver null en caso de error
                }
            }
        }
    }

    public interface IRepository<T>
    {
        void Insert(IEnumerable<T> entities);

        void InsertFactura(IEnumerable<T> entities);
        void InsertFacturaDetalles(IEnumerable<T> entities);
        void InsertClientes(IEnumerable<T> entities);

        void InsertProductos(IEnumerable<T> entities);
    }

    public class MySQLRepository<T> : IRepository<T>
    {
        private string connectionString;

        public MySQLRepository(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public void Insert(IEnumerable<T> entities)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {

                    foreach (var entity in entities)
                    {
                        // Aquí realizas la lógica para insertar cada entidad en la base de datos
                        // por ejemplo, utilizando un ORM como Entity Framework o Dapper
                        // Ejemplo con Dapper:
                        connection.Execute("INSERT INTO simecFacs.factura_archivos ( c_id_foldig, c_id_docto, c_folio, c_archivo_path) VALUES (@Cidfoldig, @Ciddocto, @Cfolio, @Carchdidis)", entity, transaction);

                    }
                    transaction.Commit();
                }
            }
        }

        public void InsertFactura(IEnumerable<T> entities)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using ( var transaction = connection.BeginTransaction() )
                {
                    foreach ( var entity in entities)
                    {
                        connection.Execute("INSERT INTO simecFacs.facturas ( c_id_doc_path, c_id_tipodoc, c_id_conce, c_folio, c_fecha, c_id_cliente, c_neto, c_total) VALUES " +
                            "(@Ciddocum01, @Ciddocum02, @Cidconce01, @Cfolio, @Cfechave01, @Cidclien01, @Cneto, @Ctotal)", entity, transaction);
                    }
                    transaction.Commit();
                }
            }
        }

        public void InsertFacturaDetalles(IEnumerable<T> entities)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    foreach (var entity in entities)
                    {
                        connection.Execute("INSERT INTO simecFacs.factura_detalles (c_id_docto, c_idprod) VALUES (@Ciddocum01, @Cidprodu01)", entity, transaction);
                    }
                    transaction.Commit();
                }
            }
        }

        public void InsertClientes(IEnumerable<T> entities)
        {
            //c_id, c_codigo, c_razon, c_rfc, c_fecha_alta
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    foreach (var entity in entities)
                    {
                        connection.Execute("INSERT INTO simecFacs.clientes (c_id, c_codigo, c_razon, c_rfc, c_fecha_alta) VALUES (@Cidclien01, @Ccodigoc01, @Crazonso01, @Crfc, @Cfechaalta)", entity, transaction);
                    }
                    transaction.Commit();
                }
            }
        }

        public void InsertProductos(IEnumerable<T> entities)
        {
            //id_prod, c_codigo, c_nombre, c_tipoprod, c_clave_sat, c_fecha_alta
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    foreach (var entity in entities)
                    {
                        connection.Execute("INSERT INTO simecFacs.productos (id_prod, c_codigo, c_nombre, c_tipoprod, c_clave_sat, c_fecha_alta) VALUES (@Cidprodu01, @Ccodigop01, @Cnombrep01, @Ctipopro01, @Cclavesat, @Cfechaal01)", entity, transaction);
                    }
                    transaction.Commit();
                }
            }
        }
    }
}
