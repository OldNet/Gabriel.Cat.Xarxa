using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gabriel.Cat;
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
		//hacer metodo que de el resultado anterior en un diccionario<idCampo,valor>
		public static LlistaOrdenada<string,string> PostDataDiccionary(this HttpListenerRequest request)
		{
			
			string postData=request.PostData();
			LlistaOrdenada<string,string> diccionari=new LlistaOrdenada<string, string>();
			//poso les dades
			return diccionari;
		}
		
		public static bool Exist(this Uri uriFile)
		{
			WebRequest wrFile=WebRequest.Create(uriFile);
			bool exist;
			try{
				exist=System.IO.File.Exists(uriFile.ToString());
				
			}catch{
				exist=false;
			}
			
			if(!exist)
			{
				try{
					wrFile.GetResponse();
					exist=true;
				}catch{
					exist=false;
				}
			}
			return exist;
		}
		public static HtmlDocument DownloadUrl(this Uri uriWeb)
		{
			WebBrowser	wbFile=new WebBrowser();
			bool acabadoDeDescargar=false;
			wbFile.Navigated+=(s,e)=>{
				acabadoDeDescargar=true;
			};
			wbFile.Navigate(uriWeb);
			while(!acabadoDeDescargar)
				System.Threading.Thread.Sleep(150);
			return wbFile.Document;
		}
	}
}
