/*
 * Creado por SharpDevelop.
 * Usuario: tetradog
 * Fecha: 16/05/2017
 * Hora: 22:20
 * Licencia GNU GPL V3
 * Para cambiar esta plantilla use Herramientas | Opciones | Codificación | Editar Encabezados Estándar
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Gabriel.Cat.Extension;

namespace Gabriel.Cat
{
	/// <summary>
	/// Description of FileGitHub.
	/// </summary>
	public class FileGitHub:IComparable,IClauUnicaPerObjecte
	{
		public static LlistaOrdenada<FileGitHub> FilesToManage{ get; private set; }
		
		Uri pathFile;
		Uri linkFileGitHub;
		string lastCommitId;
		
		static FileGitHub()
		{
			FilesToManage = new LlistaOrdenada<FileGitHub>();
		}
		public FileGitHub(Uri linkFileGitHub,Uri pathFile,string lastCommitId="")
		{
			this.pathFile=pathFile;
			this.linkFileGitHub=linkFileGitHub;
			this.lastCommitId=lastCommitId;
		}
		private FileGitHub(string[] parts):this(new Uri(parts[0]),new Uri(parts[1]),parts[2])
		{}

		#region IClauUnicaPerObjecte implementation


		public IComparable Clau {
			get {
				return linkFileGitHub.ToString();
			}
		}


		#endregion

		public void UpdateFile()
		{
			WebClient wcFile;
			if (UpdateLastFileId()) {
				//si no esta actualizado
				wcFile = new WebClient();
				if (File.Exists(pathFile.AbsolutePath))
					File.Delete(pathFile.AbsolutePath);
				File.WriteAllBytes(pathFile.AbsolutePath, wcFile.DownloadData(linkFileGitHub.AbsolutePath));
			}
		}

		bool UpdateLastFileId()
		{
			System.Windows.Forms.HtmlDocument wbFile;
			string lastComitOnline=null;
			bool actualizado;
			System.Windows.Forms.HtmlElementCollection links;
			if(!linkFileGitHub.Exist())
				throw new FileNotFoundOnGithubException(linkFileGitHub);
			//obtengo el commit del archivo

			wbFile=linkFileGitHub.DownloadUrl();
			links=wbFile.GetElementsByTagName("a");
			for(int i=0;i<links.Count&&lastComitOnline==null;i++)
				if(links[i].GetAttribute("class")=="commit-tease-sha")
					lastComitOnline=links[i].InnerText;
			//si no existe el archivo lanzo una excepcion
			actualizado = !String.Equals(lastCommitId, lastComitOnline);
			if (actualizado)
				lastCommitId = lastComitOnline;
			
			return actualizado;
		}

		#region IComparable implementation

		public int CompareTo(object obj)
		{
			int compareTo;
			FileGitHub other = obj as FileGitHub;
			if (other != null)
				compareTo = linkFileGitHub.ToString().CompareTo(other.linkFileGitHub.ToString());
			else
				compareTo = -1;
			return compareTo;
		}

		#endregion
		public override string ToString()
		{
			return pathFile.ToString()+";"+linkFileGitHub.ToString()+";"+lastCommitId;
		}
		/// <summary>
		/// Actualiza los archivos
		/// </summary>
		/// <param name="eliminarDelDiccionariSiNoEstaOnline"></param>
		public static void UpdateFiles( bool eliminarDelDiccionariSiNoEstaOnline=true)
		{
			List<FileGitHub> filesRemoved = new List<FileGitHub>();
			
			for (int i = 0; i < FilesToManage.Count; i++)
				try {
				FilesToManage.GetValueAt(i).UpdateFile();
			} catch {
				if(eliminarDelDiccionariSiNoEstaOnline)
					filesRemoved.Add(FilesToManage.GetValueAt(i));
			}
			if(eliminarDelDiccionariSiNoEstaOnline)
				FilesToManage.RemoveRange(filesRemoved.Casting<IComparable>());
		}
		
		public static void UpdateDiccionary(Uri pathDiccionary)
		{
			List<string> filesString=new List<string>();
			if(File.Exists(pathDiccionary.ToString()))
				File.Delete(pathDiccionary.ToString());
			for(int i=0;i<FilesToManage.Count;i++)
				filesString.Add(FilesToManage.GetValueAt(i).ToString());
			File.WriteAllLines(pathDiccionary.ToString(),filesString.ToArray());
		}
		public static void LoadDiccionary(Uri pathDiccionary)
		{
			string[] filesDic;
			if(File.Exists(pathDiccionary.ToString()))
			{
				
				filesDic=File.ReadAllLines(pathDiccionary.ToString());
				for(int i=0;i<filesDic.Length;i++)
					FilesToManage.Add(new FileGitHub(filesDic[i].Split(';')));
			}
		}
	}
	public class FileNotFoundOnGithubException:Exception
	{
		public FileNotFoundOnGithubException(Uri pathFile)
			: base("File not found:" + pathFile)
		{
		}
	}
}
