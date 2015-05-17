/*
 * Creado por SharpDevelop.
 * Usuario: Pingu
 * Fecha: 19/11/2014
 * Hora: 18:00
 * 
 * Para cambiar esta plantilla use Herramientas | Opciones | Codificación | Editar Encabezados Estándar
 */
using System;
using System.Collections.Generic;

namespace Gabriel.Cat
{
	/// <summary>
	/// Description of Paquet.
	/// </summary>
	class Paquet:IEnumerable<PartPaquet>
	{
		PartPaquet[] parts;
		int xifrat;
		static int idPaquetGen=0;
		int idPaquet;
		int numParts;
		//-1 si no està
		/// <summary>
		/// 
		/// </summary>
		/// <param name="contingut">no pot ser null ni "" </param>
		/// <param name="maximCaractersPerPart">-1 si no hi ha limit</param>
		/// <param name="xifrat">xifrat usat per xifrar el contingut, -1 (o negatiu) si no esta xifrat</param>
		public Paquet(string contingut, int maximCaractersPerPart, int xifrat)
		{
			if (String.IsNullOrEmpty(contingut) || maximCaractersPerPart == 0)
				throw new Exception("Ha de tenir un contingut!");
			
			if (maximCaractersPerPart > 0)
				numParts = contingut.Length / maximCaractersPerPart;
			else
				numParts = 1;
			idPaquet=idPaquetGen++;
			parts = new PartPaquet[numParts];
			if (maximCaractersPerPart < 0)
				parts[0] = new PartPaquet(idPaquet,contingut, 0,numParts);
			else
				for (int j = 0, longitud = 0; j < numParts; j++,longitud += maximCaractersPerPart)
					parts[j]=new PartPaquet(idPaquet,contingut.Substring(longitud, maximCaractersPerPart), j,numParts);
			if(xifrat<-1)
				xifrat=-1;
			this.xifrat = xifrat;
			
		}
		public Paquet(int numParts,int idPaquet):this(numParts,-1,idPaquet){}
		public Paquet(int numParts,int xifrat,int idPaquet)
		{

			if(numParts<0)
				throw new Exception("Ha d'haver un numero més gran de 0 de parts");
			if(idPaquet<0)
				throw new Exception("Erro id incorrecte");
			this.numParts=numParts;
			parts = new PartPaquet[numParts];
			if(xifrat<-1)
				xifrat=-1;
			this.xifrat=xifrat;
			this.idPaquet=idPaquet;
		}
		public Paquet(int numParts,string idPaquet):this(numParts,Convert.ToInt32(idPaquet.Split(':')[1])){}

		public int NumParts {
			get {
				return numParts;
			}
			set {
				numParts = value;
			}
		}
		public int Xifrat {
			get {
				return xifrat;
			}
			set {
				if(value<-1)
					value=-1;
				xifrat = value;
			
			}

			
		}
		
		/// <summary>
		/// Retorna null si no estan totes les parts
		/// </summary>
		public string Contingut {
			get {
				string contingut = "";
				bool esPotFet = PaquetSenser();
				for (int i = 0; i < parts.Length && esPotFet; i++)
					contingut += parts[i].PartMissatge;
				if (esPotFet)
					return contingut;
				else
					return null;
			}
		}

		public int IdPaquet {
			get {
				return idPaquet;
			}
		}
		public PartPaquet[] Parts {
			get {
				return parts;
			}

		}
		public PartPaquet this[int index]
		{
			get{return parts[index];}
			set{parts[index]=value;}
		}
		public bool PaquetSenser()
		{
			bool senser=true;
			for (int i = 0; i < parts.Length && senser; i++)
				if (parts[i] == null)
					senser = false;
			return senser;
		}

		#region IEnumerable implementation

		public IEnumerator<PartPaquet> GetEnumerator()
		{
			foreach(PartPaquet part in parts)
				yield return part;
		}

		#endregion

		#region IEnumerable implementation

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion
	}
}
