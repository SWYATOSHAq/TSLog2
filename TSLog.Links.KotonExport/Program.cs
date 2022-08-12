using TSLog.Links.KotonExport;

Console.WriteLine("Start export");

Worker worker = new Worker();
worker.Export();

Console.WriteLine("Finish Export!");
Console.ReadKey();
