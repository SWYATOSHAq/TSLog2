// See https://aka.ms/new-console-template for more information

using TSLog.Links.Connector;

string dataSource = string.Empty;
var worker = new Worker(dataSource);
worker.Run();

