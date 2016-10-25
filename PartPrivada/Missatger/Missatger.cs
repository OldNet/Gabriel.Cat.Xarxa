/*
 * Creado por SharpDevelop.
 * Usuario: Pingu
 * Fecha: 23/11/2014
 * Hora: 13:54
 * 
 * Para cambiar esta plantilla use Herramientas | Opciones | Codificación | Editar Encabezados Estándar
 */
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using Newtonsoft.Json;
namespace Gabriel.Cat
{
public	delegate void MissatgerSenseConnexioEventHandler(Missatger missatger);
	/// <summary>
	/// Clase que envia i reb els missatges i utilitza les cartes i els paquets :)
	/// </summary>
public	class Missatger:IComparable
	{
		TcpClient client;
		static int genID = 0;
		readonly int id;
		IReceptor receptor;
		Thread filPrincipal;
		int tempsEsperaRebut = 3 * 30 * 1000;
		int tempsEsperaEnviar = 3 * 30 * 1000;
		const int QUANTITATBYTES = 3 * 30 * 1024;
		IXifrat xifratClient;
		AddressFamily adreçaDelClient;
        
		int xifrat;
		int maximCaractersPerPart;
		
		//envia
		LlistaOrdenada<string,Carta> cartesPerEnviar;
		LlistaOrdenada<string,PartPaquet> partsNoEnviades;
		//reb
		LlistaOrdenada<string,Carta> cartesRebudes;
		//s'ha de netejar d'alguna forma...
		LlistaOrdenada<string,PartPaquet> partsRebudes;
		LlistaOrdenada<string,string> paquetsEntregats;
		//desso el id només
		LlistaOrdenada<string,Paquet> paquetsIncomplets;
		//vigilants
		LlistaOrdenada<string,Thread> vigilantsActius;
		int tempsEsperaVigilant = 1000 * 3;
		//event
		public event MissatgerSenseConnexioEventHandler ConexioPerduda;
		public Missatger(TcpClient client, IReceptor receptor)
		{
			this.client = client;
			adreçaDelClient = client.Client.AddressFamily;//a verue si conté l'adreça ip del client!!!
			this.receptor = receptor;
			id = genID++;
			Inicialitza();
			
		}
		/// <summary>
		/// Si la ip no es valida o el port, genera una excepció
		/// ArgumentNullException: El valor del parámetro hostname es null.
        ///ArgumentOutOfRangeException: El valor del parámetro port no está entre MinPort y MaxPort.
        ///SocketException: Se ha producido un error al obtener acceso al socket. Vea la sección Comentarios para obtener más información.
		/// </summary>
		/// <param name="ipClient"></param>
		/// <param name="port"></param>
		/// <param name="receptor"></param>
		public Missatger(string ipClient,int port,IReceptor receptor):this(new TcpClient(ipClient,port),receptor)
		{
			
		}
		public IXifrat XifratClient {
			get {
				return xifratClient;
			}
			set {
				xifratClient = value;
			}
		}
		public AddressFamily AdreçaDelClient {
			get {
				return adreçaDelClient;
			}
		}
		public TcpClient Client {
			get {
				return client;
			}
			set {
				if (!adreçaDelClient.Equals(value.Client.AddressFamily))
					throw new Exception("No pertany al mateix client!!!");
				client = value;
			}
		}

		public int MaximCaractersPerPart {

			get{ return maximCaractersPerPart; }
			set{ maximCaractersPerPart = value; }
		}

		public int Xifrat {
			get {
				return xifrat;
			}
			set {
				xifrat = value;
			}
		}
		void Inicialitza()
		{
			cartesPerEnviar = new LlistaOrdenada<string, Carta>();
			cartesRebudes = new LlistaOrdenada<string, Carta>();
			partsNoEnviades = new LlistaOrdenada<string, PartPaquet>();
			partsRebudes = new LlistaOrdenada<string, PartPaquet>();
			paquetsEntregats = new LlistaOrdenada<string, string>();
			vigilantsActius = new LlistaOrdenada<string, Thread>();
			paquetsIncomplets = new LlistaOrdenada<string, Paquet>();
		}
		public void Finaliza()
		{
			Inicialitza();
			try {
				filPrincipal.Abort();
				if(ConexioPerduda!=null)
					ConexioPerduda(this);
			} catch {
			}
		}
		public void ComençaAsinc()
		{
			filPrincipal =	new Thread(() => Comença());
			filPrincipal.Start();
		}

		public void Comença()
		{
			while (client.Connected)
				PossaObjecteAlSeuLloc(RebObj());
			if (filPrincipal.IsAlive)
				filPrincipal.Abort();
			if (ConexioPerduda != null)
				ConexioPerduda(this);
			
		}
		public void EliminaDadesPendentsUsuari()
		{			//esborro les dades dessades
			Inicialitza();
		}
		#region reb dades
		void PossaObjecteAlSeuLloc(JsonObject jsonObject)
		{
			object objRebut = null;
			try {
				objRebut = new CartaPaquet(jsonObject);
			} catch {
				
				try {
					objRebut = new CartaConfirmacio(jsonObject);
				} catch {
					try {
						objRebut = new PartPaquet(jsonObject);
					} catch {
						try {
							
							objRebut = new CartaReclamacio(jsonObject);
						} catch {
						}
					}
				}
				
			}
			if (objRebut is PartPaquet) {
				PartPaquet part = (PartPaquet)objRebut;
				if (!paquetsEntregats.ContainsKey(part.IdPaquet)) {
					if (!partsRebudes.ContainsKey(part.Id)) {
			
						partsRebudes.Add(part.Id, part);
			
						if (!paquetsIncomplets.ContainsKey(part.IdPaquet)) {
				
							paquetsIncomplets.Add(part.IdPaquet, new Paquet(part.TotalParts, part.IdPaquet));
						
							//vigilo que venga la carta del paquete
							Vigila(part.IdPaquet, true, true);
												
							//vigilo que se reciban todas partes del paquete
							for (int i = 0; i < part.NumPart; i++)
								Vigila(part.IdPaquet + ";" + i, false, true);//que pasa si ya ha llegado la parte...
						}
				
						paquetsIncomplets[part.IdPaquet][part.NumPart] = part;
			
						if (paquetsIncomplets[part.IdPaquet].PaquetSenser())
							EntregaPaquet(part.IdPaquet);
					}
				}
				//si hay un vigilante para la parte dejo de vigilar que llegue
				if (vigilantsActius.ContainsKey(part.Id + "falsetrue")) {
		
					vigilantsActius[part.Id + "falsetrue"].Abort();
					vigilantsActius.Remove(part.Id);
				}
				//envio que ha llegado
				Envia(new CartaConfirmacio(part.Id, CartaConfirmacio.Tipus.partPaquet, true));//envio l'id del paquet amb el numero de la part
				
			} else if (objRebut is CartaPaquet) {
				CartaPaquet carta = (CartaPaquet)objRebut;
				if (!paquetsEntregats.ContainsKey(carta.IdPaquet)) {
					if (!paquetsIncomplets.ContainsKey(carta.IdPaquet))
						paquetsIncomplets.Add(carta.IdPaquet, new Paquet(carta.NumParts, carta.IdPaquet));
					
					//vigilo que se reciban todas partes del paquete
					for (int i = 0; i < carta.NumParts; i++)
						Vigila(carta.IdPaquet + ";" + i, false, true);//que pasa si ya ha llegado la parte...
					if (paquetsIncomplets[carta.IdPaquet].PaquetSenser()) {
						EntregaPaquet(carta.IdPaquet);
					}
					
				}
				//si estic esperant l'arrivada de la cartaPaquet
				if (vigilantsActius.ContainsKey(carta.IdPaquet + "truetrue")) {
	
					vigilantsActius[carta.IdPaquet + "truetrue"].Abort();
					vigilantsActius.Remove(carta.IdPaquet + "truetrue");
				}
				//envia carta
				Envia(new CartaConfirmacio(carta.IdIntern, CartaConfirmacio.Tipus.cartaPaquet, true));//envio l'id intern de la carta...
				
			} else if (objRebut is CartaConfirmacio) {
				CartaConfirmacio carta = objRebut as CartaConfirmacio;
				string esCarta = "true";
				switch (carta.TipusId) {
					case CartaConfirmacio.Tipus.cartaPaquet:
					case CartaConfirmacio.Tipus.cartaPartPaquet:

						cartesPerEnviar.Remove(carta.IdRebut);
	
						break;
					case CartaConfirmacio.Tipus.partPaquet:
			
						partsNoEnviades.Remove(carta.IdRebut);
		
						esCarta = "false";
						break;
				}
				if (vigilantsActius.ContainsKey(carta.IdRebut + esCarta + false)) {
				
					vigilantsActius[carta.IdRebut + esCarta + false].Abort();
					vigilantsActius.Remove(carta.IdRebut + esCarta + false);
				}
				
			} else if (objRebut is CartaReclamacio) {
				CartaReclamacio carta = objRebut as CartaReclamacio;
				if (carta.EsCarta) {
					if (cartesPerEnviar.ContainsKey(carta.IdAReclamar))
						Envia(cartesPerEnviar[carta.IdAReclamar]);
				} else if (partsNoEnviades.ContainsKey(carta.IdAReclamar))
					Envia(partsNoEnviades[carta.IdAReclamar]);
			}
			
			
			
		}
		void Envia(Carta carta)
		{
			//envia la carta
			CartaReclamacio cartaR = carta as CartaReclamacio;
			CartaConfirmacio cartaC = carta as CartaConfirmacio;
			CartaPaquet cartaP = carta as CartaPaquet;
			if (cartaR != null)
				Envia(cartaR.ToJson());
			else if (cartaC != null)
				Envia(cartaC.ToJson());
			else if (cartaP != null) {
				
				cartesPerEnviar.Add(cartaP.IdPaquet, cartaP);
		
				Vigila(cartaP.IdPaquet, true, false);
				Envia(cartaP.ToJson());
			}
		}
		void Envia(PartPaquet part)
		{
			
			Vigila(part.Id, false, false);
			Envia(part.ToJson());
		}
		void Envia(JsonObject json)
		{
			//Codi envia
			if (client.Connected) {
				byte[] msg = System.Text.Encoding.UTF8.GetBytes(json);
				try {
					Thread.Sleep(100);
			
					NetworkStream stream = client.GetStream();
					stream.WriteTimeout = tempsEsperaEnviar;//temps per enviar la resposta
			
					stream.Write(msg, 0, msg.Length);
					stream.Flush();
				} catch { /*Console.WriteLine("Error al enviar el missatge: inici{" + System.Text.Encoding.UTF8.GetString(msg) + "}fi;"); */
				}
			}
		}
		void Vigila(string idAVigilar, bool esCarta, bool shaDeRebre)
		{
			Thread filVigilant = new Thread(() => IVigila(idAVigilar, esCarta, shaDeRebre));
			filVigilant.Name = idAVigilar + esCarta + shaDeRebre;
			if (!vigilantsActius.ContainsKey(filVigilant.Name)) {
		
				vigilantsActius.Add(filVigilant.Name, filVigilant);
	
				filVigilant.Start();
			}
		}
		void IVigila(string idAVigilar, bool esCarta, bool shaDeRebre)
		{
			Thread.Sleep(tempsEsperaVigilant);
			if (shaDeRebre) {
				if (esCarta) {
					if (!cartesRebudes.ContainsKey(idAVigilar)) {
						Envia(new CartaReclamacio(idAVigilar, esCarta));
						IVigila(idAVigilar, esCarta, shaDeRebre);
					}
				} else {
					if (!partsRebudes.ContainsKey(idAVigilar)) {
						Envia(new CartaReclamacio(idAVigilar, esCarta));
						IVigila(idAVigilar, esCarta, shaDeRebre);
					}
				}
			} else {
				if (esCarta) {
					if (cartesPerEnviar.ContainsKey(idAVigilar)) {
						Envia(cartesPerEnviar[idAVigilar].ToJson());
						IVigila(idAVigilar, esCarta, shaDeRebre);
						
					}
				} else {
					if (partsNoEnviades.ContainsKey(idAVigilar)) {
						Envia(partsNoEnviades[idAVigilar].ToJson());
						IVigila(idAVigilar, esCarta, shaDeRebre);
						
					}
				}
			}
			
			
			if (vigilantsActius.ContainsKey(Thread.CurrentThread.Name)) {

				vigilantsActius.Remove(Thread.CurrentThread.Name);
		
				Thread.CurrentThread.Abort();
			}
		}
		void EntregaPaquet(string idPaquet)
		{
			if (paquetsIncomplets.ContainsKey(idPaquet) && cartesRebudes.ContainsKey(idPaquet)) {//i si esta la carta del paquet!!
				Paquet paquetSenserAEntrear = paquetsIncomplets[idPaquet];
				JsonObject objPerEntregar = (JsonObject)Desxifra(paquetSenserAEntrear.Contingut);
				
	
				paquetsIncomplets.Remove(idPaquet);

		
				paquetsEntregats.Add(paquetSenserAEntrear.IdPaquet + "", paquetSenserAEntrear.IdPaquet + "");
			
				
				
				CartaPaquet cartaPaquet = cartesRebudes[idPaquet] as CartaPaquet;//carta rebuda;
			

				if (cartaPaquet.IdRerenciaDesti != -1)
					receptor.Reb(this, objPerEntregar, cartaPaquet.IdRerenciaDesti);
				else
					receptor.Reb(this, new ByteJson(objPerEntregar).Dades);
				

				paquetsEntregats.Add(idPaquet, idPaquet);
			
				foreach (PartPaquet part in paquetSenserAEntrear)
					partsRebudes.Remove(part.Id);

			}
		}

		JsonObject RebObj()
		{
			JsonObject json = null;
			byte[] bytesRebuts = new byte[QUANTITATBYTES];
			string rebut;
			if (client.Connected) {

				NetworkStream stream = client.GetStream();
		
				stream.ReadTimeout = tempsEsperaRebut;
				
				try {
					stream.Read(bytesRebuts, 0, QUANTITATBYTES);
					rebut = System.Text.ASCIIEncoding.UTF8.GetString(bytesRebuts);
					json = new JsonObject(Desxifra(rebut.Substring(0, rebut.IndexOf('\0'))));
				} catch {
				}
			}
			return json;
		}

		string Desxifra(string str)
		{
			if (xifratClient != null)
				str = xifratClient.DesXifra(str);
			return str;//si la connexió va xifrada aqui es desxifra!
		}
		#endregion
		#region envia dades

		public void Envia(JsonObject json, int id)
		{
			Paquet paquet = new Paquet(Xifra(json), maximCaractersPerPart, xifrat);
			CartaPaquet cartaPaquet = new CartaPaquet(xifrat, id, paquet.IdPaquet + "", paquet.NumParts);
			
		}
		public void Envia(IJson json, int id)
		{
			Envia(json.ToJson(), id);
		}
		public void Envia(byte[] dades)
		{
			Envia(new ByteJson(dades), -1);
		}
		public void Envia(string text, int id)
		{
			Envia((StringJson)text, id);
		}
		string Xifra(string str)
		{
			if (xifratClient != null)
				str = xifratClient.Xifra(str);
			return str;
		}

		#endregion
		#region IComparable implementation

		public int CompareTo(object obj)
		{
			Missatger other = obj as Missatger;
			int comparteTo = -1;
			if (other != null)
				comparteTo = id.CompareTo(other.id);
			return comparteTo;
		}

		#endregion
	}
	public interface IReceptor
	{
		void Reb(Missatger missatger, JsonObject obj, int referencia);
		void Reb(Missatger missatger, byte[] dades);

	}

}
