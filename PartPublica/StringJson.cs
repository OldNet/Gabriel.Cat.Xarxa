using Newtonsoft.Json;
using System;
using System.Linq;

namespace Gabriel.Cat
{/// <summary>
/// Serveix per enviar string en format json rapidament!
///Es pot utlitzar (StringJson)string i (string)StringJson :)
/// </summary>
	public class StringJson:IJson
	{
		string missatge;


		public StringJson() { }
		public StringJson(string missatge) { this.missatge = missatge; }
		public StringJson(JsonObject json)
		{
			if(json==null)
				throw new Exception("Json invalid");
			StringJson jsonString=JsonConvert.DeserializeObject<StringJson>(json);
			missatge=jsonString.Missatge;
		}
		public string Missatge
		{
			get { return missatge; }
			set { missatge = value; }
		}

		#region IJson implementation


		public JsonObject ToJson()
		{
			return (JsonObject)JsonConvert.SerializeObject(this);
		}
		#endregion
		public static explicit operator string(StringJson text)
		{
			return text.missatge;
		}
		public static explicit operator StringJson(string text)
		{
			return new StringJson(text);
		}




		public static JsonObject StringToJson(string text)
		{
			StringJson textJson=new StringJson(text);
			return textJson.ToJson();
		}
	}
}
