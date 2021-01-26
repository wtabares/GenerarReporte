using System;
using System.Collections.Generic;
using System.IO;

namespace GenerarReporteConsole
{
    class Program
    {

		public const string folder840Path = @"D:\EDI\EDI-840";
		public const string folder843Path = @"D:\EDI\EDI-843";
		public const string folder850Path = @"D:\EDI\EDI-850";

		public const string tag840 = @"BQT";
		public const string tag843 = @"BQR";
		public const string tag850 = @"BEGS";

		public const string eol840 = @"^";
		public const string eol843 = @"*";
		public const string eol850 = @"*";

		static int Main(string[] args)
        {
			// revisamos si los argumentos traen la fecha
			if (args.Length == 0)
            {
				System.Console.WriteLine("FORMA DE USO: GenerarReporteConsole <fechaConsulta>");
				return 1;
            }

			string dateInput = args[0];
			ProcesarDirectorio(folder840Path, DateTime.Parse(dateInput), tag840);
			DisplayBreak('>');
			DisplayBreak('>');
			ProcesarDirectorio(folder843Path, DateTime.Parse(dateInput), tag843);
			DisplayBreak('>');
			DisplayBreak('>');
			ProcesarDirectorio(folder850Path, DateTime.Parse(dateInput), tag850);
			return 0;
        }

		static void ProcesarDirectorio(string path, DateTime date, string tag)
        {
			try
            {
				string[] files = Directory.GetFiles(path);
				DateTime fechaArchivo;
				int processFile ;

				foreach (string file in files)
                {
					fechaArchivo = File.GetLastWriteTime(file).Date;
					processFile = date.Date.CompareTo(fechaArchivo);
					if (processFile == 0)
					{
						ReadFile(file, tag);
					}
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

			int position;
			int endPosition;

			try
			{
				int currentLine = 1;
				// Open the file text using a stream reader.
				foreach (var line in File.ReadAllLines(pathFile))
				{
					position = 0;
					endPosition = 0;

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
							position = line.IndexOf(tag);
							endPosition = line.IndexOf(eol840);
							current.Add(line.Substring(position + 73, endPosition - 73));
						}
						else if (tag843.Equals(tag))
						{
							// logica para leer prop 843
							position = line.IndexOf(tag);
							endPosition = line.IndexOf(eol843);
							current.Add(line.Substring(position + 73, endPosition - 73));
						}

						else if (tag850.Equals(tag))
						{
							// agregar logica para prop 850 - BEG)
							position = line.IndexOf(tag);
							endPosition = line.IndexOf(eol850) - 2;
							current.Add(line.Substring(position + 65, endPosition - 65));
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
