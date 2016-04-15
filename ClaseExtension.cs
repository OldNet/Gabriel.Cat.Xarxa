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
			try {
				response.ContentLength64 = dades.Length;
				response.OutputStream.Write(dades, 0, dades.Length);
			} catch {
			} // suppress any exceptions
            finally {
				// always close the stream
				response.OutputStream.Close();
			}

		}
		public static string PostData(this HttpListenerRequest request)
		{
			string postData;
			System.IO.Stream body;
			System.IO.StreamReader reader;
			if (!request.HasEntityBody) {
				postData = null;
			} else {
				body = request.InputStream; // here we have data
				reader = new System.IO.StreamReader(body, request.ContentEncoding);
				postData = reader.ReadToEnd();
				reader.Close();
				
			}
			return postData;
			
		}
		//hacer metodo que de el resultado anterior KeyValuPair<IdCampo,Valor>[] asi es mas comodo
	}
}
