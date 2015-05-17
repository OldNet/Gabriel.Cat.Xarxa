/*
 * Creado por SharpDevelop.
 * Usuario: Pingu
 * Fecha: 21/11/2014
 * Hora: 16:27
 * 
 * Para cambiar esta plantilla use Herramientas | Opciones | Codificación | Editar Encabezados Estándar
 */
using System;
using System.Drawing;
using System.Net.Sockets;
using System.Threading;

namespace Gabriel.Cat
{
	/// <summary>
	/// Description of ServidorXarxaLocal.
	/// </summary>
	class ServidorXarxaLocal:Servidor,IReceptor
	{
		string ipServidor;
		bool esServidor;
		int portEscolta;
		LlistaOrdenada<AddressFamily,Missatger> missatgersClientsConectats;
		LlistaOrdenada<AddressFamily,Missatger> missatgersServidorsConectats;
		Thread filPrincipal;
		enum Peticions
		{
			QueEts,EsServidor,EsClient,Objecte,FinalitzaSessio
		}
		public ServidorXarxaLocal()
		{
			missatgersClientsConectats=new LlistaOrdenada<AddressFamily, Gabriel.Cat.Missatger>();
			missatgersServidorsConectats=new LlistaOrdenada<AddressFamily, Gabriel.Cat.Missatger>();
			portEscolta=1992;
			ipServidor = ObetenirIpServidor();
		}

		public string IpServidor {
			get {
				return ipServidor;
			}
			protected set{ ipServidor = value; }
			
		}

		public bool EsServidor {
			get;
			set;
		}

		public int PortEscolta {
			get{return portEscolta;}
			set{portEscolta=value;}
			
		}

		#region implemented abstract members of Servidor


		public override void Envia(string ipÌd, JsonObject json)
		{
			throw new NotImplementedException();
		}


		#endregion

		protected virtual string ObetenirIpServidor()
		{
			string ip = MissatgerObject.GetLocalIpv4Ethernet();
			if (ip == "")
				ip = MissatgerObject.GetLocalIpv4WireLess();
			return ip;
		}
		public void ComençaAsinc()
		{
			filPrincipal=new Thread(()=>Comença());
			filPrincipal.Start();
		}
		public void Comença()
		{
			DecideixServidor();//tengo que saber como montarlo....
			for(;;)
			{
				while(EsServidor);//hace de servidor
				while(!EsServidor);//hace de cliente
			}
		}

		void DecideixServidor()
		{
			ipServidor=null;
			string[] ips=Missatger.Informador.BroadCast();
			Missatger[] missatgers=new Gabriel.Cat.Missatger[ips.Length];
			for(int i=0;i<missatgers.Length;i++){
				missatgers[i]=new Gabriel.Cat.Missatger(ips[i],PortEscolta,this);
				missatgers[i].Envia("",(int)Peticions.QueEts);
			}
			while(missatgersClientsConectats.Count+missatgersServidorsConectats.Count<ips.Length)
				Thread.Sleep(100);
			switch(Missatger.ModeServidor)
			{
				case MissatgerObject.ModeServidorEnum.auto:

					if(missatgersServidorsConectats.Count==0)
					{
						EsServidor=true;
						ipServidor=ObetenirIpServidor();
					}
					else
						EsServidor=false;
					break;
				case MissatgerObject.ModeServidorEnum.client:
					EsServidor=false;
					break;
				case MissatgerObject.ModeServidorEnum.servidor:
					EsServidor=true;
					break;
			}
			
		}

		#region IReceptor implementation

		public  void Reb(Gabriel.Cat.Missatger missatger, JsonObject obj, int referencia)
		{
			Peticions peticio=(Peticions)referencia;
			switch(peticio)
			{
				case Peticions.QueEts:
					if(EsServidor)
						missatger.Envia("",(int)Peticions.EsServidor);
					else
						missatger.Envia("",(int)Peticions.EsClient);
					break;
				case Peticions.EsClient:
					AfegirId(missatger.AdreçaDelClient.ToString());
					missatgersClientsConectats.Afegir(missatger.AdreçaDelClient,missatger);
					missatger.ConexioPerduda+=TreuClientEvent;
					
					break;
				case Peticions.EsServidor:
					AfegirId(missatger.AdreçaDelClient.ToString());
					missatgersServidorsConectats.Afegir(missatger.AdreçaDelClient,missatger);
					missatger.ConexioPerduda+=TreuServidorEvent;
					
					break;
				case Peticions.FinalitzaSessio:
					TreuId(missatger.AdreçaDelClient.ToString());
					missatger.Finaliza();break;
				case Peticions.Objecte:
					EnviaAlUsuari(obj,missatger.AdreçaDelClient.ToString(),MissatgerArg.TipusServidor.XarxaLocal);
					break;
					
			}
		}

		void TreuClientEvent(Gabriel.Cat.Missatger missatger)
		{
			missatgersClientsConectats.Elimina(missatger.AdreçaDelClient);
		}

		void TreuServidorEvent(Gabriel.Cat.Missatger missatger)
		{
			missatgersServidorsConectats.Elimina(missatger.AdreçaDelClient);
		}

		public void Reb(Gabriel.Cat.Missatger missatger, byte[] dades)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
