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
        public ClienteServidorHttpSeguro(HttpListenerContext client)
        {
            if (client == null)
                throw new NullReferenceException();
            this.client = client;
            this.conexiones = 1;
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
