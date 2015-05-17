/*
 * Creado por SharpDevelop.
 * Usuario: Pingu
 * Fecha: 19/11/2014
 * Hora: 18:29
 * 
 * Para cambiar esta plantilla use Herramientas | Opciones | Codificación | Editar Encabezados Estándar
 */
using System;

namespace Gabriel.Cat
{
	public enum NivellXifrat
	{
		basic,mitg,alt,moltAlt
	}
	/// <summary>
	/// Description of XifratIntern.
	/// </summary>
	static class XifratIntern
	{

		#region IXifrat implementation

		public static string Xifra(string textSenseXifrar,NivellXifrat seguretat)
		{
			if(String.IsNullOrEmpty(textSenseXifrar))
				throw new Exception("Falta el text per xifrar");
			string textXifrat="";
			switch(seguretat)
			{
					case	NivellXifrat.basic:break;
					case	NivellXifrat.mitg:break;
					case	NivellXifrat.alt:break;
					case	NivellXifrat.moltAlt:break;
			}
			return textXifrat;
		}

		public static string Desxifra(string textXifrat,NivellXifrat seguretat)
		{
			if(String.IsNullOrEmpty(textXifrat))
				throw new Exception("Falta el text per desxifrar");
			string textDesxifrat="";
			switch(seguretat)
			{
					case	NivellXifrat.basic:break;
					case	NivellXifrat.mitg:break;
					case	NivellXifrat.alt:break;
					case	NivellXifrat.moltAlt:break;
			}
			return textDesxifrat;
		}

		#endregion

	}
}
