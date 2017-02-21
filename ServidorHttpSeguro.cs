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

        public  ServicioValidacionIP ServicioValidacionIP { get; set; }
        LlistaOrdenada<ClienteServidorHttpSeguro> clientes;
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
            clientes = new LlistaOrdenada<ClienteServidorHttpSeguro>();
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
            string serialClinete = conexionNueva.Request.GetClientCertificate().SerialNumber;
            string idUnicoCliente = ipCliente + serialClinete;
            bool existe = clientes.ContainsKey(ipCliente+serialClinete);
            if (System.Diagnostics.Debugger.IsAttached || ShowDebbugMessages)
            {
                Console.WriteLine("Hay una nueva conexion de la ip {0} y serial {1} ",ipCliente,serialClinete);
            }
            if (existe && !clientes[idUnicoCliente].Bloqueado || !existe && (ServicioValidacionIP == null ||  ServicioValidacionIP.ValidaIp(ipCliente)))//valido aqui que no este bloqueado para no tener que comprobar su ip en vano :)
            {
                if (System.Diagnostics.Debugger.IsAttached || ShowDebbugMessages)
                {
                    Console.WriteLine("\tLa ip {0} no usa proxys ni nada por el estilo y es valida", ipCliente);
                }
                try
                {
                    if (tmpResetIntentos.Enabled)
                        smpResetIntentos.WaitOne();//hace que vayan uno a uno...quizas pierde rendimiento
                    if (!clientes.ContainsKey(idUnicoCliente))
                    {
                        if (System.Diagnostics.Debugger.IsAttached || ShowDebbugMessages)
                        {
                            Console.WriteLine("\tLa ip {0} es nueva", ipCliente);
                        }
                        cliente = new ClienteServidorHttpSeguro(conexionNueva);
                        clientes.Add(cliente);
                    }
                    else
                    {
                        cliente = clientes[idUnicoCliente];
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
                            Console.WriteLine("\tLa ip {0} supera las conexiones de un cliente normal", ipCliente);
                        }
                        cliente.Bloqueado = true;//lo bloqueo antes porque luego se puede quitar el ban
                        if (ClienteNoSeguro != null)
                            ClienteNoSeguro(cliente);
                       
                    }
                    else if (ClienteSeguro != null)
                    {
                        if (System.Diagnostics.Debugger.IsAttached || ShowDebbugMessages)
                        {
                            Console.WriteLine("\tLa ip {0} es un cliente normal", ipCliente);
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
            else 
            {
                if (System.Diagnostics.Debugger.IsAttached || ShowDebbugMessages)
                {
                    Console.WriteLine("\tLa ip {0} no es valida", ipCliente);
                }
                if (!existe)
                {
                    clientes.Add(new ClienteServidorHttpSeguro(conexionNueva) { Bloqueado=true});//añado la conexion a la lista de bloqueados para evitar que se vuelva a comprobar :)
                    if (ClienteNoSeguro != null)//aviso para se sepa :)
                        ClienteNoSeguro(clientes[idUnicoCliente]);
                }
            }


        }


        private void ResetIntentos(object sender, ElapsedEventArgs e)
        {
            ClienteServidorHttpSeguro cliente;
            smpResetIntentos.WaitOne();

            for (int i = 0; i < clientes.Count; i++)
            {
                cliente = clientes.GetValueAt(i);
                if (!cliente.Bloqueado || cliente.Conexiones >= maxIntentosCliente)//los que se han excedido de intentos los desbaneo los otros se quedan por presunto ataque hacker ;)
                    this.clientes.Remove(clientes[i]);//los elimino porque la clave ip se va renovando asi que habrian al final muchas ips no usadas nunca...y supondria un problema :)
            }
            smpResetIntentos.Release();
        }
    }

}
