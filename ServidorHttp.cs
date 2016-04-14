using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gabriel.Cat.Xarxa
{
    public delegate void ServidorHttpEventHanlder(ServidorHttp servidor, HttpListenerContext conexion);
    public class ServidorHttp
    {
        public static bool ShowDebbugMessages = true;//asi se pueden ver sin debugear :)

        System.Net.HttpListener listener;
        Tiket<Object> tiketListener;
        

        public event ServidorHttpEventHanlder NuevaConexion;

        public ServidorHttp()
        {
            if (!HttpListener.IsSupported)//lo pongo por seguridad :D
                throw new NotSupportedException("Needs Windows XP SP2, Server 2003 or later.");
            listener = new System.Net.HttpListener();
            IdServidor = DateTime.Now.Ticks;

        }
        public ServidorHttp(params string[] prefixes):this()
        {
            if (prefixes != null)
                for (int i = 0; i < prefixes.Length; i++)
                    Prefixes.Add(prefixes[i]);
        }
        public long IdServidor { get; private set; }
        public HttpListenerPrefixCollection Prefixes
        {
            get { return listener.Prefixes; }
        }
        public void Start()
        {
            if (NuevaConexion == null)
                throw new Exception("No hay quien coja las nuevas conexiones");

            //si no hay prefix habra una excepcion
            if (!listener.IsListening)
            {

                listener.Start(); 
                tiketListener = new Tiket<object>((o) =>
                {
                    while (true)
                    {
                        NuevaConexion(this, listener.GetContext()); 
           
                    }
                }, null);
                tiketListener.AñadirPool();
                if (System.Diagnostics.Debugger.IsAttached || ShowDebbugMessages)
                {
                    Console.WriteLine("El servidor id='{0}' esta encendido", IdServidor);
                }
            }
        }
        public void Stop()
        {
            if (tiketListener != null)
            {

                listener.Stop();
                listener.Close();
                tiketListener.AbortaTrabajo();
                tiketListener = null;
                if (System.Diagnostics.Debugger.IsAttached || ShowDebbugMessages)
                {
                    Console.WriteLine("El servidor id='{0}' esta parado", IdServidor);
                }
            }
        }

        
    }


}
