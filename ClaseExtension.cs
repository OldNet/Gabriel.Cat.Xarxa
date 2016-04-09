using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Gabriel.Cat.Extension
{
   public static class ClaseExtensionXarxa
    {
        public static void Send(this HttpListenerResponse response, string dades)
        {
          response.Send(Encoding.UTF8.GetBytes(dades));
        }
        public static void Send(this HttpListenerResponse response, byte[] dades)
        {
            if (!response.OutputStream.CanWrite)
                throw new Exception("can not write on output stream!");
            try
            {
                response.ContentLength64 = dades.Length;
                response.OutputStream.Write(dades, 0, dades.Length);
            }
            catch { } // suppress any exceptions
            finally
            {
                // always close the stream
                response.OutputStream.Close();
            }

        }

        public static string GetHttpValueArgument(this Uri url, string nameArg)
        {
            string[] campos = url.ToString().Split('?');
            string argumentoConValor = "=";
            for (int i = 1; i < campos.Length&&argumentoConValor=="="; i++)
            {
                if (campos[i].Contains(nameArg + "="))
                    argumentoConValor = campos[i];
            }
            return argumentoConValor.Split('=')[1];
        }
    }
}
