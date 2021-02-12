using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GenerarReporteConsole
{
    class Program
    {

		public const string folder840Path = @"D:\EDI\EDI-840";
		public const string folder843Path = @"D:\EDI\EDI-843";
		public const string folder850Path = @"D:\EDI\EDI-850";

		public const string tag840 = @"BQT";
		public const string tag843 = @"BQR";
		public const string tag850 = @"BEG";

		static int Main(string[] args)
        {
			try
            {
				string dateInput;
				// revisamos si los argumentos traen la fecha
				if (args.Length == 0)
				{
					dateInput = DateTime.Today.ToString();
				}
				else
				{
					dateInput = args[0];
				}

				ProcesarDirectorio(folder840Path, DateTime.Parse(dateInput), tag840);
				DisplayBreak('>');
				DisplayBreak('>');
				ProcesarDirectorio(folder843Path, DateTime.Parse(dateInput), tag843);
				DisplayBreak('>');
				DisplayBreak('>');
				ProcesarDirectorio(folder850Path, DateTime.Parse(dateInput), tag850);
				return 0;
			}
			catch (Exception e)
			{
				Console.WriteLine("Error: {0}", e.Message);
				return -1;
			}
		}

		static void ProcesarDirectorio(string path, DateTime date, string tag)
        {
			try
            {
				// filtrar solo los archivos de la fecha solicitada y ordenar por fecha
				string[] files = Directory.GetFiles(path)
					.Where(file => new FileInfo(file).LastWriteTime.Date == date.Date)
					.OrderBy(file => File.GetLastWriteTime(file))
					.ToArray();

				foreach (string file in files)
                {
					ReadFile(file, tag);
				}
            }
			catch (Exception e)
            {
				Console.WriteLine("Error: {0}", e.Message);
            }
        }

		static void ReadFile(string pathFile, string tag)
		{
			String numeroControl = "";
			String fileName = Path.GetFileName(pathFile);
			DateTime lastMod = File.GetLastWriteTime(pathFile);
			List<string> current = new List<string>();

			try
			{
				int currentLine = 1;
				// Open the file text using a stream reader.
				foreach (var line in File.ReadAllLines(pathFile))
				{
					// obtener numero de control
					if (currentLine == 2)
                    {
						numeroControl = line.Substring(40,8);
					}
					if (line.Contains(tag))
					{
						if (tag840.Equals(tag))
                        {
							// logica para leer prop 840
							IEnumerable<string> tokens =
								!string.IsNullOrEmpty(line) ? line.Split(' ', StringSplitOptions.RemoveEmptyEntries) : Enumerable.Empty<string>();
							string token = tokens.Last();
							current.Add(token.Substring(0, token.Length-1));
						}
						else if (tag843.Equals(tag))
						{
							// logica para leer prop 843
							IEnumerable<string> tokens =
								!string.IsNullOrEmpty(line) ? line.Split(' ', StringSplitOptions.RemoveEmptyEntries) : Enumerable.Empty<string>();
							string token = tokens.Last();
							current.Add(token.Substring(0, token.Length - 1));
						}
						else if (tag850.Equals(tag))
						{
							// agregar logica para prop 850 - BEG)
							string token = !string.IsNullOrEmpty(line) ? line.Substring(65, 30) : "";
							current.Add(token.Trim());
						}
					}
					currentLine++;
				}
				DisplayData(fileName, lastMod.ToString(), numeroControl, current);
			}
			catch (IOException e)
			{
				Console.WriteLine("The file could not be read: {0}", e.Message);
			}
		}


		public static void DisplayData(string fileName, string lastMod, string noControl, List<string> props)
        {
			bool isEmpty = props.Count == 0;

			if (isEmpty)
            {
				Console.WriteLine("{0},{1},{2},{3}", fileName, lastMod, noControl, "NO DATA");
			}
			else
            {
				// Show content
				int lin = 1;
				foreach (var item in props)
				{
					if (lin == 1)
					{
						Console.WriteLine("{0},{1},{2},{3}", fileName, lastMod, noControl, item.Trim());
						lin++;
					}
					else
					{
						Console.WriteLine(",,,{0}", item.Trim());
					}
				}
			}
			DisplayBreak('*');
		}

		public static void DisplayBreak(Char caracter)
        {
			Console.WriteLine();
			Console.WriteLine(new string(caracter, 100));
			Console.WriteLine();
		}

	}
}
