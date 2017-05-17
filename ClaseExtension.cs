﻿using System;
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
		public static HtmlElement FindElementById(this HtmlDocument doc,string contenido)
		{
			HtmlElement elementToFound=null;
			string aux;
			for(int i=0;i<doc.Body.All.Count&&elementToFound==null;i++)
			{
				try{
					aux=doc.Body.All[i].GetAttribute("id");
					if(aux==contenido)
						elementToFound=doc.Body.All[i];
					
				}
				catch{}
			}
			return elementToFound;
		}
		public static HtmlElement[] FiltraPorClase(this HtmlElementCollection elementos,string clase)
		{
			List<HtmlElement> lstElementos=new List<HtmlElement>();
			string aContener;
			if(clase.Filtra((c)=>c==' ').Count>0)
				aContener="class=\""+clase+"\"";
			else aContener="class="+clase;
			for(int i=0;i<elementos.Count;i++)
				if(elementos[i].OuterHtml.Contains(aContener))
					lstElementos.Add(elementos[i]);
			return lstElementos.ToArray();
		}
		public static HtmlDocument DownloadUrl(this Uri uriWeb)
		{
			string textoWeb;
			WebClient wcFile=new WebClient();
			textoWeb=wcFile.DownloadString(uriWeb);
			return GetHtmlDocument(textoWeb);
		}

		private static System.Windows.Forms.HtmlDocument GetHtmlDocument(string html)
		{//sacado de http://stackoverflow.com/questions/4935446/string-to-htmldocument
			System.Windows.Forms.WebBrowser browser = new System.Windows.Forms.WebBrowser();
			browser.ScriptErrorsSuppressed = true;
			browser.DocumentText = html;
			browser.Document.OpenNew(true);
			browser.Document.Write(html);
			browser.Refresh();
			return browser.Document;
		}
	}
}
