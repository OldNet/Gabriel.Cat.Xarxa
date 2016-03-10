using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Threading;

namespace Gabriel.Cat.Xarxa
{
    public delegate HttpResponseMessage SendAsyncEventHandler(HttpRequestMessage request, CancellationToken cancellationToken);
   public class MensajeHttp : HttpMessageHandler
    {
        private SendAsyncEventHandler sendAsyncMetodo;

        public MensajeHttp(SendAsyncEventHandler sendAsyncMetodo)
        {
            this.SendAsyncMetodo = sendAsyncMetodo;
        }

        public SendAsyncEventHandler SendAsyncMetodo
        {
            get
            {
                return sendAsyncMetodo;
            }

            set
            {
                if (value == null)
                    throw new NullReferenceException();
                sendAsyncMetodo = value;
            }
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return new Task<HttpResponseMessage>(() => SendAsyncMetodo(request, cancellationToken));
        }
    }
   public class ClienteHttp
    {
        System.Net.Http.HttpClient client;
        MensajeHttp mensaje;
        public ClienteHttp(SendAsyncEventHandler metodoRespuestaHttp)
        {
            Mensaje = new MensajeHttp(metodoRespuestaHttp);
            Client = new HttpClient(Mensaje);
          
        }

        public HttpClient Client
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

        public MensajeHttp Mensaje
        {
            get
            {
                return mensaje;
            }

            set
            {
                if (value == null)
                    throw new NullReferenceException();
                mensaje = value;
            }
        }
    }
}
