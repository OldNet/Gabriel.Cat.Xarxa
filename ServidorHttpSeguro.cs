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
        public ServidorHttpSeguro(double tiempoRenovarIntentosIp, int maxIntentosCliente)
        {
            clientes = new ListaUnica<ClienteServidorHttpSeguro>();
            tmpResetIntentos = new System.Timers.Timer();
            tmpResetIntentos.Elapsed += ResetIntentos;
            TiempoRenovarIntentosIp = tiempoRenovarIntentosIp;
            smpResetIntentos = new System.Threading.Semaphore(1, 1);
            NuevaConexion += ControlarAcceso;
            if(StartTemporizadorResetIntenentosPorDefecto)
               tmpResetIntentos.Start();

        }
        public ServidorHttpSeguro(double tiempoRenovarIntentosIp) : this(tiempoRenovarIntentosIp, MAXINTENTOSPORDEFECTO)
        { }
        public ServidorHttpSeguro(int maxIntentosCliente) : this(TIEMPORENOVARINTENTOSIPPORDEFECTO, maxIntentosCliente)
        { }
        public ServidorHttpSeguro() : this(TIEMPORENOVARINTENTOSIPPORDEFECTO)
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
            if(!tmpResetIntentos.Enabled)
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
            IComparable keyClient = conexionNueva.Request.LocalEndPoint.Address.ToString();
            bool existe = clientes.ExisteClave(keyClient);
            if (!existe||existe && !clientes[keyClient].Bloqueado)
            {
                try
                {
                    if(tmpResetIntentos.Enabled)
                        smpResetIntentos.WaitOne();//hace que vayan uno a uno...quizas pierde rendimiento
                    if (!clientes.ExisteClave(keyClient))
                    {
                        cliente = new ClienteServidorHttpSeguro(conexionNueva);
                        clientes.Añadir(cliente);
                    }
                    else
                    {
                        cliente = clientes[keyClient];
                    }
                    cliente.AñadadirConexion();
                    if (cliente.Conexiones > maxIntentosCliente && ClienteNoSeguro != null)
                        ClienteNoSeguro(cliente);
                    else if (ClienteSeguro != null)
                        ClienteSeguro(cliente);
                }
                catch (Exception excepcion){ throw excepcion; }//si hay algun problema lanzo la excepcion
                finally
                {
                    if (tmpResetIntentos.Enabled)
                        smpResetIntentos.Release();
                }
            }
        }

        private void ResetIntentos(object sender, ElapsedEventArgs e)
        {
            smpResetIntentos.WaitOne();
            for (int i = 0; i < clientes.Count; i++)
                clientes[i].ResetConexiones();
            smpResetIntentos.Release();
        }
    }
    public struct ClienteServidorHttpSeguro : IComparable<ClienteServidorHttpSeguro>, IComparable, IClauUnicaPerObjecte
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
            get { return client.Request.LocalEndPoint.Address.ToString(); }
        }
        public void ResetConexiones()
        {
            Conexiones = 0;
        }
        public void AñadadirConexion()
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
