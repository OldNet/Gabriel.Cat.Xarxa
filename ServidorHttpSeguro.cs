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
        public ServidorHttpSeguro(double tiempoRenovarIntentosIp, int maxIntentosCliente, params string[] prefixes)
            : base(prefixes)
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
        public ServidorHttpSeguro(double tiempoRenovarIntentosIp, params string[] prefixes)
            : this(tiempoRenovarIntentosIp, MAXINTENTOSPORDEFECTO, prefixes)
        { }
        public ServidorHttpSeguro(int maxIntentosCliente, params string[] prefixes)
            : this(TIEMPORENOVARINTENTOSIPPORDEFECTO, maxIntentosCliente, prefixes)
        { }
        public ServidorHttpSeguro(params string[] prefixes)
            : this(TIEMPORENOVARINTENTOSIPPORDEFECTO, prefixes)
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
            string ipCliente = conexionNueva.Request.RemoteEndPoint.Address.ToString();
            bool existe = clientes.ExisteClave(ipCliente);
            if (System.Diagnostics.Debugger.IsAttached || ShowDebbugMessages)
            {
                Console.WriteLine("Hay una nueva conexion de la ip {0}", ipCliente);
            }
            if (existe && !clientes[ipCliente].Bloqueado || !existe && !ClienteUsaProxyEtc(ipCliente))//valido aqui que no este bloqueado para no tener que comprobar su ip en vano :)
            {
                if (System.Diagnostics.Debugger.IsAttached || ShowDebbugMessages)
                {
                    Console.WriteLine("\tLa ip {0} no usa proxys ni nada por el estilo y es valida", ipCliente);
                }
                try
                {
                    if (tmpResetIntentos.Enabled)
                        smpResetIntentos.WaitOne();//hace que vayan uno a uno...quizas pierde rendimiento
                    if (!clientes.ExisteClave(ipCliente))
                    {
                        if (System.Diagnostics.Debugger.IsAttached || ShowDebbugMessages)
                        {
                            Console.WriteLine("\tLa ip {0} es nueva", ipCliente);
                        }
                        cliente = new ClienteServidorHttpSeguro(conexionNueva);
                        clientes.Añadir(cliente);
                    }
                    else
                    {
                        cliente = clientes[ipCliente];
                        cliente.Client = conexionNueva;//es una nueva conexion :)
                        cliente.AñadirConexion();
                        if (System.Diagnostics.Debugger.IsAttached || ShowDebbugMessages)
                        {
                            Console.WriteLine("\tLa ip {0} lleva {1} conexiones", ipCliente, cliente.Conexiones);
                        }
                    }

                    if (cliente.Conexiones >= maxIntentosCliente)//si supera el maximo de intentos por conexion
                    {
                        if (System.Diagnostics.Debugger.IsAttached || ShowDebbugMessages)
                        {
                            Console.WriteLine("\tLa ip {0} supera las conexiones de un cliente normal", ipCliente, cliente.Conexiones);
                        }
                        if (ClienteNoSeguro != null)
                            ClienteNoSeguro(cliente);
                    }
                    else if (ClienteSeguro != null)
                    {
                        if (System.Diagnostics.Debugger.IsAttached || ShowDebbugMessages)
                        {
                            Console.WriteLine("\tLa ip {0} es un cliente normal", ipCliente, cliente.Conexiones);
                        }
                        ClienteSeguro(cliente);
                    }

                    cliente.Client.Response.OutputStream.Flush();//envio y limpio la conexion
                    conexionNueva.Response.Close();//si la cierro para los que estan bloqueados luego pueden volver a enviar (al menos los relentizo) asi que solo la cierro para los que son validos :)
                }
                catch (Exception excepcion) { throw excepcion; }//si hay algun problema lanzo la excepcion
                finally
                {
                    if (tmpResetIntentos.Enabled)
                        smpResetIntentos.Release();
                }
            }
            else if (System.Diagnostics.Debugger.IsAttached || ShowDebbugMessages)
            {
                Console.WriteLine("\tLa ip {0} no es valida", ipCliente);
                if (!existe)
                {
                    clientes.Añadir(new ClienteServidorHttpSeguro(conexionNueva) { Bloqueado=true});//añado la conexion a la lista de bloqueados para evitar que se vuelva a comprobar :)
                }
            }


        }
        /// <summary>
        /// Valida la Ip que no use Proxy u otro medio no directo (para prevenir ataques masivos)
        /// </summary>
        /// <param name="conexionNueva"></param>
        /// <returns></returns>
        protected virtual bool ClienteUsaProxyEtc(string ipAComprobar)//lo hago virtual para que los que hereden puedan cambiar de sitio web :)
        {
            //esta web mira proxy,vpn,red TOR y bad ip detection
            const char USAPROXYETC = '1';
            string pathWebConIp = "http://check.getipintel.net/check.php?ip=" + ipAComprobar;
            if (System.Diagnostics.Debugger.IsAttached || ShowDebbugMessages)
            {
                Console.WriteLine("Se usara la web '{0}' para validar la ip", pathWebConIp);
            }
            System.Net.Http.HttpClient cliente = new System.Net.Http.HttpClient();
            Task<System.Net.Http.HttpResponseMessage> respuesta = cliente.GetAsync(pathWebConIp);
            Task<string> datosRespuesta;
            string respuestaString;
            respuesta.Wait();
            datosRespuesta = respuesta.Result.Content.ReadAsStringAsync();//metodo para mirarlo online :)
            datosRespuesta.Wait();
            respuestaString = datosRespuesta.Result;
            if (System.Diagnostics.Debugger.IsAttached || ShowDebbugMessages)
            {
                Console.WriteLine("La respuesta de la web '{0}' para la ip {1}", respuestaString, ipAComprobar);
            }
            return respuestaString.Contains(USAPROXYETC);
        }

        private void ResetIntentos(object sender, ElapsedEventArgs e)
        {
            smpResetIntentos.WaitOne();
            if (System.Diagnostics.Debugger.IsAttached || ShowDebbugMessages)
            {
                Console.WriteLine("Vacio la lista de clientes");
            }
            clientes.Vaciar();//como cada peticion es una conexion nueva,para no guardar muchos clientes que ya se les habra cambiado la ip pues  vacio la lista :)
            smpResetIntentos.Release();
        }
    }

}
