using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Gabriel.Cat.Xarxa
{
    public delegate void ClienteSeguroEventHandler(ClienteServidorHttpSeguro cliente);
    public class ServidorHttpSeguro : ServidorHttp
    {
        public static bool StartTemporizadorResetIntenentosPorDefecto = true;
        const int MAXINTENTOSPORDEFECTO = 1000;
        const double TIEMPORENOVARINTENTOSIPPORDEFECTO = 4 * 60 * 60 * 1000;//cada 4 horas
        ListaUnica<ClienteServidorHttpSeguro> clientes;
        System.Timers.Timer tmpResetIntentos;
        System.Threading.Semaphore smpResetIntentos;
        int maxIntentosCliente;
       
        public event ClienteSeguroEventHandler ClienteNoSeguro;
        public event ClienteSeguroEventHandler ClienteSeguro;
        #region Constructores
        public ServidorHttpSeguro(double tiempoRenovarIntentosIp, int maxIntentosCliente, params string[] prefixes) : base(prefixes)
        {
            MaxIntentosCliente = maxIntentosCliente;
            clientes = new ListaUnica<ClienteServidorHttpSeguro>();
            tmpResetIntentos = new System.Timers.Timer();
            tmpResetIntentos.Elapsed += ResetIntentos;
            TiempoRenovarIntentosIp = tiempoRenovarIntentosIp;
            smpResetIntentos = new System.Threading.Semaphore(1, 1);
            NuevaConexion += ControlarAcceso;
            if (StartTemporizadorResetIntenentosPorDefecto)
                tmpResetIntentos.Start();


        }
        public ServidorHttpSeguro(double tiempoRenovarIntentosIp, params string[] prefixes) : this(tiempoRenovarIntentosIp, MAXINTENTOSPORDEFECTO, prefixes)
        { }
        public ServidorHttpSeguro(int maxIntentosCliente, params string[] prefixes) : this(TIEMPORENOVARINTENTOSIPPORDEFECTO, maxIntentosCliente, prefixes)
        { }
        public ServidorHttpSeguro(params string[] prefixes) : this(TIEMPORENOVARINTENTOSIPPORDEFECTO, prefixes)
        { }
        #endregion
        #region Propiedades
        public double TiempoRenovarIntentosIp
        {
            get { return tmpResetIntentos.Interval; }
            set { tmpResetIntentos.Interval = value; }
        }

        public int MaxIntentosCliente
        {
            get
            {
                return maxIntentosCliente;
            }

            set
            {
                maxIntentosCliente = value;
            }
        }
        #endregion
        public void StartTemporizadorRenovarIp()
        {
            if (!tmpResetIntentos.Enabled)
            {
                smpResetIntentos.WaitOne();
                tmpResetIntentos.Start();
                smpResetIntentos.Release();
            }
        }
        public void StopTemporizadorRenovarIp()
        {
            if (tmpResetIntentos.Enabled)
            {
                smpResetIntentos.WaitOne();
                tmpResetIntentos.Stop();
                smpResetIntentos.Release();
            }
        }
        private void ControlarAcceso(ServidorHttp servidor, HttpListenerContext conexionNueva)
        {
            ClienteServidorHttpSeguro cliente;
            IComparable keyClient = conexionNueva.Request.RemoteEndPoint.Address.ToString();
            bool existe = clientes.ExisteClave(keyClient);
            if (!ClienteUsaProxyEtc(conexionNueva))
            {
                if (!existe || existe && !clientes[keyClient].Bloqueado)
                {
                    try
                    {
                        if (tmpResetIntentos.Enabled)
                            smpResetIntentos.WaitOne();//hace que vayan uno a uno...quizas pierde rendimiento
                        if (!clientes.ExisteClave(keyClient))
                        {
                            cliente = new ClienteServidorHttpSeguro(conexionNueva);
                            clientes.Añadir(cliente);
                        }
                        else
                        {
                            cliente = clientes[keyClient];
                            cliente.Client = conexionNueva;//es una nueva conexion :)
                        }
                        cliente.AñadirConexion();
                        if (cliente.Conexiones >= maxIntentosCliente && ClienteNoSeguro != null)
                            ClienteNoSeguro(cliente);
                        else if (ClienteSeguro != null)
                            ClienteSeguro(cliente);
                        cliente.Client.Response.OutputStream.Flush();
                        conexionNueva.Response.Close();//si la cierro para los que estan bloqueados luego pueden volver a enviar (al menos los relentizo) asi que solo la cierro para los que son validos :)
                    }
                    catch (Exception excepcion) { throw excepcion; }//si hay algun problema lanzo la excepcion
                    finally
                    {
                        if (tmpResetIntentos.Enabled)
                            smpResetIntentos.Release();
                    }
                }
            }
              
        }

        private bool ClienteUsaProxyEtc(HttpListenerContext conexionNueva)
        {
            const char USAPROXYETC = '1';
            string pathWebConIp = "http://check.getipintel.net/check.php?ip=" + conexionNueva.Request.RemoteEndPoint.Address.ToString();
            Console.WriteLine(pathWebConIp);
            System.Net.Http.HttpClient cliente = new System.Net.Http.HttpClient();
            Task<System.Net.Http.HttpResponseMessage> respuesta= cliente.GetAsync(pathWebConIp);
            Task<string> datosRespuesta;
            string respuestaString;
            respuesta.Wait();
            datosRespuesta= respuesta.Result.Content.ReadAsStringAsync();//metodo para mirarlo online :)
            datosRespuesta.Wait();
            respuestaString= datosRespuesta.Result;
            Console.WriteLine(respuestaString);
            return respuestaString.Contains(USAPROXYETC);
        }

        private void ResetIntentos(object sender, ElapsedEventArgs e)
        {
            smpResetIntentos.WaitOne();
            clientes.Vaciar();//como cada peticion es una conexion nueva,para no guardar muchos clientes que ya se les habra cambiado la ip pues  vacio la lista :)
            smpResetIntentos.Release();
        }
    }
    public class ClienteServidorHttpSeguro : IComparable<ClienteServidorHttpSeguro>, IComparable, IClauUnicaPerObjecte
    {
        HttpListenerContext client;
        int conexiones;
        bool bloqueado;
        public ClienteServidorHttpSeguro(HttpListenerContext client)
        {
            if (client == null)
                throw new NullReferenceException();
            this.client = client;
            this.conexiones = 0;
            bloqueado = false;
        }

        public HttpListenerContext Client
        {
            get
            {
                return client;
            }
            set
            {
                if (value == null)
                    throw new NullReferenceException();
                client = value;
            }
        }

        public int Conexiones
        {
            get
            {
                return conexiones;
            }

            private set
            {
                conexiones = value;
            }
        }

        public bool Bloqueado
        {
            get
            {
                return bloqueado;
            }

            set
            {
                bloqueado = value;
            }
        }
        public string DireccionIP
        {
            get { return client.Request.RemoteEndPoint.Address.ToString(); }
        }
        public void ResetConexiones()
        {
            Conexiones = 0;
        }
        public void AñadirConexion()
        {
            Conexiones++;
        }

        public int CompareTo(ClienteServidorHttpSeguro other)
        {
            return Clau().CompareTo(other.Clau());
        }

        public int CompareTo(object obj)
        {
            int compareTo;
            if (obj is ClienteServidorHttpSeguro)
            {
                compareTo = CompareTo((ClienteServidorHttpSeguro)obj);
            }
            else
            {
                compareTo = -1;
            }
            return compareTo;

        }

        public IComparable Clau()
        {
            return DireccionIP;
        }
    }
}
