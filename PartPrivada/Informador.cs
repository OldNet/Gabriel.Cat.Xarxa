/*
 * Creado por SharpDevelop.
 * Usuario: Pingu
 * Fecha: 19/11/2014
 * Hora: 18:05
 * 
 * Para cambiar esta plantilla use Herramientas | Opciones | Codificación | Editar Encabezados Estándar
 */
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Gabriel.Cat.Extension;
namespace Gabriel.Cat
{
	/// <summary>
	/// Description of Informador. escolta peticions i envia peticions per saber ips i ip del servidor
	/// </summary>
	public	class Informador:IReceptor
	{

		MissatgerObject client;
		string ipServidor;
		const int TAMANYPETICIO = 2 * 1024 * 1024;
		const int TEMPSRESPOSTA = 3 * 60 * 1000;
		LlistaOrdenada<AddressFamily,Missatger> missatgers;
		LlistaOrdenada<AddressFamily,Missatger> missatgersBroadCast;
		Llista<string> ips;
		const int FILESBRODADCAST = 5;
		Thread fil;
		enum Peticions
		{
			IdMissatger,
			Ip,
			FiConnexio
		}
		public Informador(MissatgerObject client)
		{
			this.client = client;
			ipServidor = ObetenirIpServidor();
			missatgers = new LlistaOrdenada<AddressFamily, Missatger>();
			missatgersBroadCast = new LlistaOrdenada<AddressFamily, Missatger>();
			fil = new Thread(() => ComençaEscolta());
			fil.Start();
		}



		string ObetenirIpServidor()
		{
			return MissatgerObject.GetLocalIpv4Ethernet();
		}

		//		private string ObetenirIpServidor()
		//		{
		//			string ip = MissatgerObject.GetLocalIpv4Ethernet();
		//			if (ip == "")
		//				ip = MissatgerObject.GetLocalIpv4WireLess();
		//			return ip;
		//		}
		
		public void ComençaEscolta()
		{
			TcpClient socketClient;
			Thread filClient;
			IPAddress ipAdres = IPAddress.Parse(ipServidor);
			var socketServidor = new TcpListener(ipAdres, PortEscoltaPeticioIP);
			
			for (;;)
				try {
				socketClient = socketServidor.AcceptTcpClient();//espera a rebre una connexió
				Console.WriteLine("El servidor " + ToString() + " ha rebut una connexió");//per fer proves
				//tracta el client
				filClient = new Thread(() => TractaClient(socketClient));
				filClient.Start();



			} catch {
			}
		}
		public void ComençaEscoltaAsinc()
		{
			Thread fil = new Thread(() => ComençaEscolta());
			fil.Start();
		}
		void TractaClient(TcpClient socketClient)
		{
			Missatger missatger;
			if (missatgers.ContainsKey(socketClient.Client.AddressFamily)) {
				missatgers[socketClient.Client.AddressFamily].Client = socketClient;
				missatger = missatgers[socketClient.Client.AddressFamily];
			} else {
				missatger = new Missatger(socketClient, this);
				missatgers.Add(socketClient.Client.AddressFamily, missatger);
			}
			missatger.ConexioPerduda += TreuMissatgerLlista;
		}
		public string[] BroadCast()
		{
			
			ips = new Llista<string>();
			Llista<string> ipsPerComprovar = new Llista<string>();
			string[,] llistes;
			Thread[] fils = new Thread[FILESBRODADCAST];
			//obtengo las ips
			string[] ip = ObetenirIpServidor().Split('.');
			string rang = ip[0] + '.' + ip[1] + '.' + ip[2];
			for (int i = 2; i < 255; i++)
				ipsPerComprovar.Add(rang + '.' + i);
			ipsPerComprovar.Remove(ObetenirIpServidor());
			llistes = ipsPerComprovar.ToMatriu(FILESBRODADCAST);
			for (int i = 0; i < FILESBRODADCAST; i++) {
				fils[i] = new Thread(() => ComprovaIps(llistes, i));
				fils[i].Start();
			}
			for (int i = 0; i < fils.Length; i++)
				fils[i].Join();
			while (missatgersBroadCast.Count > 0)
				Thread.Sleep(100);//mientras hayan mensajeros por dar su mensaje espera...

			return ips.ToTaula();
		}

		void ComprovaIps(string[,] llistes, int y)
		{
			Llista<string> ips = new Llista<string>();
			TcpClient client;
			Missatger missatger;
			for (int x = 0; x < llistes.GetLength(0) && llistes[x, y] != null; x++)
				ips.Add(llistes[x, y]);
			foreach (string ip in ips)//els pcs que tinguin el port obert estaran disponibles
				try {
				client = new TcpClient();
				client.Connect(ip, PortEscoltaPeticioIP);
				missatger = new Missatger(client, this);
				missatger.ConexioPerduda += TreuMissatgerBroadCastLlista;
				missatger.Envia((StringJson)this.client.IdMissatger, (int)Peticions.IdMissatger);
				missatgersBroadCast.Add(client.Client.AddressFamily, missatger);
			} catch {
			}
			
			
		}

		void TreuMissatgerBroadCastLlista(Missatger missatger)
		{
			missatgersBroadCast.Remove(missatger.AdreçaDelClient);
		}
		void TreuMissatgerLlista(Missatger missatger)
		{
			missatgers.Remove(missatger.AdreçaDelClient);
		}
		#region IReceptor implementation
		public void Reb(Missatger missatger, JsonObject obj, int referencia)
		{
			Peticions peticio = (Peticions)referencia;
			StringJson dades = new StringJson(obj);
			
			switch (peticio) {
				case Peticions.IdMissatger:
					if (!dades.Missatge.Equals(client.IdMissatger)) {
						missatger.Envia((StringJson)"", (int)Peticions.FiConnexio);
						
					} else
						missatger.Envia((StringJson)ObetenirIpServidor(), (int)Peticions.Ip);
					break;

				case Peticions.Ip:
					ips.Add(dades.Missatge);
					missatger.Envia((StringJson)"", (int)Peticions.FiConnexio);//debo finalizar la conexion con el...pero cuando lo haga yo tambien...
					break;
				case Peticions.FiConnexio:
					missatger.Finaliza();
					break;
			}
		}

		public void Reb(Missatger missatger, byte[] dades)
		{
			throw new NotImplementedException();
		}

		#endregion



		string MD5Hash(string text)
		{
			MD5 md5 = new MD5CryptoServiceProvider();

			//compute hash from the bytes of text
			md5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(text));

			//get hash result after compute it
			byte[] result = md5.Hash;

			StringBuilder strBuilder = new StringBuilder();
			for (int i = 0; i < result.Length; i++) {
				//change it into 2 hexadecimal digits
				//for each byte
				strBuilder.Append(result[i].ToString("x2"));
			}

			return strBuilder.ToString();
		}
		
		public int PortEscoltaPeticioIP {
			get {
				return client.InformadorIpsPort;
			}

		}


	}
}
