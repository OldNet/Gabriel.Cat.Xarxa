/*
 * Creado por SharpDevelop.
 * Usuario: Pingu
 * Fecha: 19/11/2014
 * Hora: 17:55
 * 
 * Para cambiar esta plantilla use Herramientas | Opciones | Codificación | Editar Encabezados Estándar
 */
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace Gabriel.Cat
{
	public delegate void ObjecteRebutMissatgerObjectEventHandler(JsonObject objJSonRebut, MissatgerArg info);
	public delegate void IpIdEventHandler(string ipId, MissatgerArg.TipusServidor info);
	public delegate void ServidorCanviatEventHandler(bool esServidor,string ipServidor);
	/// <summary>
	/// Description of MissatgerObject.
	/// </summary>
	public class MissatgerObject
	{
		public enum ModeServidorEnum
		{
			servidor,
			client,
			auto
		}
		static SortedList<string,string> llistaMissatgers = new SortedList<string, string>();
		static Semaphore semaforMissatgers = new Semaphore(1, 1);
		string idMissatger;
		Informador informador ;
		IXifrat xifratUsuari;
		//sempreServidor,sempreClient,Auto
		ModeServidorEnum modeServidor;
		bool enviarLocalHost;
		bool enviarXarxaLocal;
		bool enviarXarxaEnLinea;
		IClientMissatgerObject clientLocalHost;
		IClientMissatgerObject clientXarxaLocal;
		IClientMissatgerObject clientXarxaEnLinea;
		ServidorLocal servidorLocalHost;
		ServidorXarxaLocal servidorLocal;
		ServidorXarxaEnLinea servidorEnLinea;
		NivellXifrat xifratComunicacio = NivellXifrat.mitg;
		int informadorPortIp;
		public event ObjecteRebutMissatgerObjectEventHandler ObjRebutLocalHost;
		public event ObjecteRebutMissatgerObjectEventHandler ObjRebutXarxaLocal;
		public event ObjecteRebutMissatgerObjectEventHandler ObjRebutXarxaEnLinea;
		public event ObjecteRebutMissatgerObjectEventHandler ObjRebut;
		
		public event IpIdEventHandler IpConnectada;
		public event IpIdEventHandler IpDesconnectada;
		
		public event ServidorCanviatEventHandler ServidorCanviat;
		public MissatgerObject(string id, bool PosarServidorLocalHost, bool PosarServidorLocal, bool PosarServidorEnLinea)
		{
			semaforMissatgers.WaitOne();
			try {
				llistaMissatgers.Add(id, id);
			} catch {
				throw new Exception("Ja Existeix un missatgerAmbAquestId: " + id);
			} finally {
				semaforMissatgers.Release();
			}
			informador=new Informador(this);
			informador.ComençaEscoltaAsinc();
			modeServidor = ModeServidorEnum.auto;
			IdMissatger = id;
			enviarLocalHost = PosarServidorLocalHost;
			enviarXarxaEnLinea = PosarServidorLocal;
			enviarXarxaLocal = PosarServidorEnLinea;
			servidorLocalHost = new ServidorLocal();
			servidorLocalHost.ObjRebut+=EntregaObjRebut;
			servidorLocal = new ServidorXarxaLocal();
			servidorLocal.ObjRebut+=EntregaObjRebut;
			servidorEnLinea = new ServidorXarxaEnLinea();
			servidorEnLinea.ObjRebut+=EntregaObjRebut;
			informadorPortIp=1992;
		}
		public MissatgerObject(string id)
			: this(id, true, true, true)
		{
		}



		/// <summary>
		/// S'utlitza per fer proves en localhost l'id es MissatgerProves.
		/// </summary>
		public MissatgerObject()
			: this("MissatgerProves", true, false, false)
		{
		}
		void EntregaObjRebut(string ipId, JsonObject objecte, MissatgerArg.TipusServidor tipus)
		{
			JsonAmbReferencia jsonAmbReferencia=new JsonAmbReferencia(objecte);
			if(ObjRebut!=null)
				ObjRebut(jsonAmbReferencia.Json,new MissatgerArg(tipus,ipId,jsonAmbReferencia.Referencia));
				         switch(tipus)
				         {
				         		case MissatgerArg.TipusServidor.XarxaEnLinea:if(ObjRebutXarxaEnLinea!=null)ObjRebutXarxaEnLinea(jsonAmbReferencia.Json,new MissatgerArg(tipus,ipId,jsonAmbReferencia.Referencia));break;
				         		case MissatgerArg.TipusServidor.XarxaLocal:if(ObjRebutXarxaLocal!=null)ObjRebutXarxaLocal(jsonAmbReferencia.Json,new MissatgerArg(tipus,ipId,jsonAmbReferencia.Referencia));break;
				         		case MissatgerArg.TipusServidor.LocalHost:if(ObjRebutLocalHost!=null)ObjRebutLocalHost(jsonAmbReferencia.Json,new MissatgerArg(tipus,ipId,jsonAmbReferencia.Referencia));break;
				         }
			}
		#region Propietats
		/// <summary>
		/// Si el valor es null no es xifra
		/// </summary>
		public IXifrat XifratUsuari {
			get {
				return xifratUsuari;
			}
			set {
				xifratUsuari = value;
			}
		}

		public bool EsServidorLocal {
			get {
				return servidorLocal.EsServidor;
			}
		}
		internal Informador Informador
		{get{return informador;}}

		/// <summary>
		/// baix:
		/// mitg:
		/// alt:
		/// moltAlt:
		/// </summary>
		public NivellXifrat XifratComunicacio {
			get {
				return xifratComunicacio;
			}
			set {
				xifratComunicacio = value;
			}
		}
		public string[] IpsClientsLocals
		{ get { return servidorLocal.Ids; } }
		public string[] IdsClientsEnLiena
		{ get { return servidorEnLinea.Ids; } }
		public string[] IdsClientsLocalHost
		{ get { return servidorLocalHost.Ids; } }

		public string IdMissatger {
			get {
				return idMissatger;
			}
			set {
				idMissatger = value;
			}
		}
		/// <summary>
		/// Si s'escull servidor o client de forma fixa llavors a l'hora de repartir els rols s'acturà en consecuencia.
		/// </summary>
		public ModeServidorEnum ModeServidor {
			get {
				return modeServidor;
			}
			set {
				if (!value.Equals(modeServidor)) {
					modeServidor = value;
					CanviModeServidor();
				}
			}
		}
		
		public IClientMissatgerObject ClientXarxaEnLinea {
			get {
				return clientXarxaEnLinea;
			}
			set {
				clientXarxaEnLinea = value;
			}
		}
		public IClientMissatgerObject ClientXarxaLocal {
			get {
				return clientXarxaLocal;
			}
			set {
				clientXarxaLocal = value;
			}
		}
		public IClientMissatgerObject ClientLocalHost {
			get {
				return clientLocalHost;
			}
			set {
				clientLocalHost = value;
			}
		}
		public bool EnviarXarxaLocal {
			get {
				return enviarXarxaLocal;
			}
			set {
				enviarXarxaLocal = value;
			}
		}
		public bool EnviarLocalHost {
			get {
				return enviarLocalHost;
			}
			set {
				enviarLocalHost = value;
			}
		}

		public bool EnviarXarxaEnLinea {
			get {
				return enviarXarxaEnLinea;
			}
			set {
				enviarXarxaEnLinea = value;
			}
		}
		#endregion
		
		#region Metodes
		#region EnviaATots
		//envia a tots
		public void Envia(IJson jSon, int idPerIdentificar)
		{

			Envia(jSon.ToJson(), idPerIdentificar);
		}

		public void Envia(JsonObject jSon, int idPerIdentificar)
		{
			if (EnviarLocalHost)
				EnviaLocalHost(jSon, idPerIdentificar);
			if (EnviarXarxaLocal)
				EnviaXarxaLocal(jSon, idPerIdentificar);
			if (EnviarXarxaEnLinea)
				EnviaXarxaEnLinea(jSon, idPerIdentificar);
			
		}
		public void Envia(byte[] arxiu, string nomAmbExtensio)
		{
			if (EnviarLocalHost)
				EnviaLocalHost(arxiu, nomAmbExtensio);
			if (EnviarXarxaLocal)
				EnviaXarxaLocal(arxiu, nomAmbExtensio);
			if (EnviarXarxaEnLinea)
				EnviaXarxaEnLinea(arxiu, nomAmbExtensio);
		}
		//envia LocalHost
		public void EnviaLocalHost(IJson jSon, int idPerIdentificar)
		{
			EnviaLocalHost(jSon.ToJson(), idPerIdentificar);
		}
		public void EnviaLocalHost(JsonObject jSon, int idPerIdentificar)
		{
			
			foreach (string id in IdsClientsLocalHost)
				EnviaLocalHost(id, jSon, idPerIdentificar);
			
		}
		public void EnviaLocalHost(byte[] arxiu, string nomAmbExtensio)
		{
			foreach (string id in IdsClientsLocalHost)
				EnviaLocalHost(id, arxiu, nomAmbExtensio);
		}
		//envia XarxaLocal
		public void EnviaXarxaLocal(IJson jSon, int idPerIdentificar)
		{
			EnviaXarxaLocal(jSon.ToJson(), idPerIdentificar);
		}
		public void EnviaXarxaLocal(JsonObject jSon, int idPerIdentificar)
		{
			foreach (string id in IdsClientsLocalHost)
				EnviaXarxaLocal(id, jSon, idPerIdentificar);
		}
		public void EnviaXarxaLocal(byte[] arxiu, string nomAmbExtensio)
		{
			foreach (string id in IdsClientsLocalHost)
				EnviaXarxaLocal(id, arxiu, nomAmbExtensio);
		}
		//envia Xarxa en Linea
		public void EnviaXarxaEnLinea(IJson jSon, int idPerIdentificar)
		{
			EnviaXarxaEnLinea(jSon.ToJson(), idPerIdentificar);
		}
		public void EnviaXarxaEnLinea(JsonObject jSon, int idPerIdentificar)
		{
			foreach (string id in IdsClientsLocalHost)
				EnviaXarxaEnLinea(id, jSon, idPerIdentificar);
		}
		public void EnviaXarxaEnLinea(byte[] arxiu, string nomAmbExtensio)
		{
			foreach (string id in IdsClientsLocalHost)
				EnviaXarxaEnLinea(id, arxiu, nomAmbExtensio);
		}
		#endregion
		#region Metodes PerUsuari
		//envia a tots
		public void Envia(string idClient, IJson jSon, int idPerIdentificar)
		{

			Envia(idClient, jSon.ToJson(), idPerIdentificar);
		}

		public void Envia(string idClient, JsonObject jSon, int idPerIdentificar)
		{
			switch (TipusID(idClient)) {
				case MissatgerArg.TipusServidor.LocalHost:
					if (EnviarLocalHost)
						EnviaLocalHost(idClient, jSon, idPerIdentificar);
					break;
				case MissatgerArg.TipusServidor.XarxaLocal:
					if (EnviarXarxaLocal)
						EnviaXarxaLocal(idClient, jSon, idPerIdentificar);
					break;
				case MissatgerArg.TipusServidor.XarxaEnLinea:
					if (EnviarXarxaEnLinea)
						EnviaXarxaEnLinea(idClient, jSon, idPerIdentificar);
					break;
			}
			
		}
		public void Envia(string idClient, byte[] arxiu, string nomAmbExtensio)
		{
			switch (TipusID(idClient)) {
				case MissatgerArg.TipusServidor.LocalHost:
					if (EnviarLocalHost)
						EnviaLocalHost(idClient, arxiu, nomAmbExtensio);
					break;
				case MissatgerArg.TipusServidor.XarxaLocal:
					if (EnviarXarxaLocal)
						EnviaXarxaLocal(idClient, arxiu, nomAmbExtensio);
					break;
				case MissatgerArg.TipusServidor.XarxaEnLinea:
					
					if (EnviarXarxaEnLinea)
						EnviaXarxaEnLinea(idClient, arxiu, nomAmbExtensio);
					break;
			}
		}
		//envia LocalHost
		public void EnviaLocalHost(string idClient, IJson jSon, int idPerIdentificar)
		{
			EnviaLocalHost(idClient,jSon.ToJson(), idPerIdentificar);
		}
		public void EnviaLocalHost(string idClient, JsonObject jSon, int idPerIdentificar)
		{
			servidorLocalHost.Envia(idClient,jSon,idPerIdentificar);
		}
		public void EnviaLocalHost(string idClient, byte[] arxiu, string nomAmbExtensio)
		{
			
		}
		//envia XarxaLocal
		public void EnviaXarxaLocal(string idClient, IJson jSon, int idPerIdentificar)
		{
			EnviaXarxaLocal(idClient, jSon.ToJson(), idPerIdentificar);
		}
		public void EnviaXarxaLocal(string idClient, JsonObject jSon, int idPerIdentificar)
		{
			servidorLocal.Envia(idClient,jSon,idPerIdentificar);
		}
		public void EnviaXarxaLocal(string idClient, byte[] arxiu, string nomAmbExtensio)
		{
		}
		//envia Xarxa en Linea
		public void EnviaXarxaEnLinea(string idClient, IJson jSon, int idPerIdentificar)
		{
			EnviaXarxaEnLinea(idClient, jSon.ToJson(), idPerIdentificar);
		}
		public void EnviaXarxaEnLinea(string idClient, JsonObject jSon, int idPerIdentificar)
		{
           servidorEnLinea.Envia(idClient,jSon,idPerIdentificar);
		}
		public void EnviaXarxaEnLinea(string idClient, byte[] arxiu, string nomAmbExtensio)
		{
		}
		
		private MissatgerArg.TipusServidor TipusID(string idClient)
		{
			MissatgerArg.TipusServidor tipus = MissatgerArg.TipusServidor.XarxaLocal;
			if (idClient.Contains("LocalHost"))
				tipus = MissatgerArg.TipusServidor.LocalHost;
			else if (idClient.Contains("OnLine"))
				tipus = MissatgerArg.TipusServidor.XarxaEnLinea;
			else
				System.Net.IPAddress.Parse(idClient);//si no es una ip tira un excepcion!!!
			return tipus;
		}



		#endregion
		#endregion
		#region Event
		internal void IpIdEvent(string ipId, bool esConnecta, MissatgerArg.TipusServidor tipus)
		{
			if (esConnecta) {
				if (IpConnectada != null)
					IpConnectada(ipId, tipus);
			} else {
				if (IpDesconnectada != null)
					IpDesconnectada(ipId, tipus);
			}
		}
		
		#endregion
		public int PorEscoltaXarxaLocal
		{
			get{return servidorLocal.PortEscolta;}
			set{servidorLocal.PortEscolta=value;}
		}

		public  int InformadorIpsPort {
			get{ return informadorPortIp; }
			set{ informadorPortIp = value; }
		}

		#region Metodes de clase
		public static string GetLocalIpv4Ethernet()
		{
			return GetLocalIPv4(NetworkInterfaceType.Ethernet);
		}
		public static string GetLocalIpv4WireLess()
		{
			return GetLocalIPv4(NetworkInterfaceType.Wireless80211);
		}
		public static string GetLocalIPv4(NetworkInterfaceType _type)
		{
			string output = "";
			foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces()) {
				if (item.NetworkInterfaceType == _type && item.OperationalStatus == OperationalStatus.Up) {
					foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses) {
						if (ip.Address.AddressFamily == AddressFamily.InterNetwork) {
							output = ip.Address.ToString();
						}
					}
				}
			}
			return output;
		}
		#endregion

	   void CanviModeServidor()
		{
		//canvia el mode del servidor...
		}
	}
	public class MissatgerArg:EventArgs
	{
		public enum TipusServidor
		{
			LocalHost,
			XarxaLocal,
			XarxaEnLinea

		}
		TipusServidor tipus;
		string idRemitent;
		int referencia;
		public MissatgerArg(TipusServidor tipus, string idRemitent)
		{
			Tipus = tipus;
			IdRemitent = idRemitent;
		}

		public MissatgerArg(MissatgerArg.TipusServidor tipus, string ipId, int referencia):this(tipus,ipId)
		{
			Referencia=referencia;

		}

		public TipusServidor Tipus {
			get {
				return tipus;
			}
			private set{ tipus = value; }
		}

		public int Referencia {
			get {
				return referencia;
			}
			private set {
				referencia = value;
			}
		}
		public string IdRemitent {
			get {
				return idRemitent;
			}
			private set{ idRemitent = value; }
		}
	}
internal class JsonAmbReferencia:IJson
	{
		JsonObject json;
		int referencia;
		public JsonAmbReferencia(){}
		public JsonAmbReferencia(JsonObject json,int referencia)
		{
			Json=json;
			Referencia=referencia;
		}
		public JsonAmbReferencia(JsonObject json)
		{
			if(json==null)
				throw new Exception("Json incorrecte");
			JsonAmbReferencia jsonAmbReferencia=JsonConvert.DeserializeObject<JsonAmbReferencia>(json);
			Json=jsonAmbReferencia.Json;
			Referencia=jsonAmbReferencia.Referencia;
			
		}

		#region IJson implementation


		public JsonObject ToJson()
		{
			return (JsonObject)JsonConvert.SerializeObject(this);
		}


		#endregion

		public JsonObject Json {
			get {
				return json;
			}
			set {
				json = value;
			}
		}

		public int Referencia {
			get {
				return referencia;
			}
			set {
				referencia = value;
			}
		}
	}
}
