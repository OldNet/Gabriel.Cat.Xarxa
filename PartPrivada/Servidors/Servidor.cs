/*
 * Creado por SharpDevelop.
 * Usuario: Pingu
 * Fecha: 21/11/2014
 * Hora: 15:43
 * 
 * Para cambiar esta plantilla use Herramientas | Opciones | Codificación | Editar Encabezados Estándar
 */
using System;
using System.Collections.Generic;
using System.Linq;
namespace Gabriel.Cat
{
	public delegate void IdEventHandler(string id);
	public delegate void ObjecteRebutEventHandler(string ipId,JsonObject objecte,MissatgerArg.TipusServidor tipus);
	/// <summary>
	/// Description of Servidor.
	/// </summary>
	internal abstract class Servidor
	{
		LlistaOrdenada<string,string> ids = new LlistaOrdenada<string,string>();
		MissatgerObject missatger;
		public event IdEventHandler IdNova;
		public event IdEventHandler IdEsborrada;
		public 	event ObjecteRebutEventHandler ObjRebut;
		public string[] Ids {
			get {
				List<string> ids = new List<string>();
				foreach (var id in this.ids)
					ids.Add(id.Value);
				return ids.ToArray();
			}
		}

		public MissatgerObject Missatger {
			get {
				return missatger;
			}
			set {
				missatger = value;
			}
		}
		public abstract void Envia(string ipId,JsonObject json);
		public void Envia(string ipId,JsonObject json,int referencia)
		{
			Envia(ipId,new JsonAmbReferencia(json,referencia).ToJson());
		}
		protected void EnviaAlUsuari(JsonObject json,string ipId,MissatgerArg.TipusServidor tipus)
		{
			if(ObjRebut!=null)
				ObjRebut(ipId,json,tipus);
		}
		protected  void AfegirId(string id)
		{
			if (!ids.Existeix(id)) {
				ids.Afegir(id, id);
				if (IdNova != null)
					IdNova(id);
			}
		}
		protected  void TreuId(string id)
		{
			if (ids.Existeix(id)) {
				ids.Elimina(id);
				if (IdEsborrada != null)
					IdEsborrada(id);
			}
		}
		
	}
}
