/*
 * Creado por SharpDevelop.
 * Usuario: Pingu
 * Fecha: 19/11/2014
 * Hora: 17:57
 * 
 * Para cambiar esta plantilla use Herramientas | Opciones | Codificación | Editar Encabezados Estándar
 */
using System;
using Newtonsoft.Json;

namespace Gabriel.Cat
{
	/// <summary>
	/// Description of CartaPaquet.
	/// </summary>
	class CartaPaquet:Carta
	{
		int xifrat;
		string idPaquet;
		int numParts;
		int idRerenciaDesti;
		public CartaPaquet(int xifrat,int idReceptor, string idPaquet, int numParts)
		{
			Xifrat = xifrat;
			IdIntern = idPaquet;
			NumParts = numParts;
			IdRerenciaDesti=idReceptor;
			
		}
		public CartaPaquet(JsonObject jSon):base("0")
		{
			if(jSon==null)
				throw new Exception("Json CartaPaquet incorrecte");
			CartaPaquet cartaJson=JsonConvert.DeserializeObject<CartaPaquet>(jSon);
			if(!cartaJson.IdIntern.Contains(ToString()))
				throw new Exception("Json incorrecte");
			this.IdIntern=cartaJson.IdIntern;
			this.Xifrat=cartaJson.Xifrat;
			this.IdPaquet=cartaJson.IdPaquet;
			this.NumParts=cartaJson.NumParts;
			this.IdRerenciaDesti=cartaJson.IdRerenciaDesti;
		}
		public CartaPaquet():base("0"){}

		public int IdRerenciaDesti {
			get {
				return idRerenciaDesti;
			}
			set {
				idRerenciaDesti = value;
			}
		}

		public int Xifrat {
			get {
				return xifrat;
			}
			set {
				xifrat = value;
			}
		}

		public string IdPaquet {
			get {
				return idPaquet;
			}
			set {
				idPaquet = value;
			}
		}

		public int NumParts {
			get {
				return numParts;
			}
			set {
				numParts = value;
			}
		}
		public override string ToString()
		{
			return base.ToString()+"Paquet";
		}
	}
}
