using Grpc.Core;
using Grpc.Net.Client;

using Historian;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Net;

var opts = new GrpcChannelOptions
{
    MaxReceiveMessageSize = null,
};

var channel = GrpcChannel.ForAddress("http://localhost:2710", opts);
var client = new Historian.HistorianReader.HistorianReaderClient(channel);

int batch_size = 1_000_000;

long from = 0;
//int from = 1715132800;
long to = from + batch_size;

var sequenceRequest = new Sequence()
{
    From = from,
    To = to
};

try
{
    while (true)
    {
        var resultsCount = 0;
        var responsesCount = 0;

        var request = new GetTimeseriesRequest();

        var timeseriesRequest = new TimeseriesRequest()
        {
            Sequence = sequenceRequest,
            IncludeNewestDataBeforeFilter = false
        };

        //timeseriesRequest.TimeseriesIds.Add(1);

        request.TimeseriesDataRequests.Add(timeseriesRequest);

        var timer = System.Diagnostics.Stopwatch.StartNew();
        var res = client.GetTimeseries(request);

        long maxSeq = 0;

        await foreach (var response in res.ResponseStream.ReadAllAsync())
        {
            maxSeq = response.TimeseriesData.Max(x => x.Sequence);

            resultsCount += response.TimeseriesData.Count;
            responsesCount++;
        }

        timer.Stop();
        Console.WriteLine($"{DateTime.UtcNow} (from: {from}, to: {to}) Elapsed time: {timer.ElapsedMilliseconds} ms. Data {resultsCount} Max seq: {maxSeq}");

        from = to;
        to = from + batch_size;
    }
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}


