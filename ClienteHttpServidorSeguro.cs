using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Gabriel.Cat.Xarxa
{
    public class ClienteServidorHttpSeguro : IComparable<ClienteServidorHttpSeguro>, IComparable, IClauUnicaPerObjecte
    {
        HttpListenerContext client;
        int conexiones;
        bool bloqueado;
        object tag;
        public SesionUsuario DatosSesionUsuario { get; private set; }
        public string IdConexionCliente { get; private set; }
        public ClienteServidorHttpSeguro(HttpListenerContext client)
        {
            if (client == null)
                throw new NullReferenceException();
            DatosSesionUsuario = new SesionUsuario();
            this.client = client;
            this.conexiones = 1;
            bloqueado = false;
            IdConexionCliente=client.Request.GetClientCertificate().SerialNumber;// sera diferente en cada pc??? y en cada navegador??? puede ser una manera de identificarlos no?? y tienen que usar porque es https asi que tienen que tenerlo :)
            //falta saber si es unico :)
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
                IdConexionCliente = client.Request.GetClientCertificate().SerialNumber;//actualizo el serial
            }
        }
        public string IdUnicoCliente
        {
            get {return DireccionIP + IdConexionCliente; }//la ip cambia...y el id tambien??pero cada x tiempo...como lo hago si cambia???
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
        public object Tag {
            get {
                return tag;
            }
            set {
                tag = value; 
            } 
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
            int compareTo;

            if (other != null) compareTo = Clau.CompareTo(other.Clau);
            else compareTo = -1;
            return compareTo;
        }

        public int CompareTo(object obj)
        {
            return CompareTo(obj as ClienteServidorHttpSeguro);

        }

        public IComparable Clau
        {
            get
            {
                return DireccionIP + IdConexionCliente;
            }
        }
    }
}
