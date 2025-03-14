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

//var timeseriesRequest = new TimeseriesRequest()
//{
//    Sequence = new Sequence()
//    {
//        From = 0,
//        To = 1000
//    },
//};

//timeseriesRequest.TimeseriesIds.Add(0);


int batch_size = 1_000_000;
//int batch_size = 1_000_000;

int from = 0;
//int from = 1715132800;
int to = from + batch_size;

try
{
    while (true)
    {
        var resultsCount = 0;
        var responsesCount = 0;

        var request = new GetTimeseriesRequest();

        var timeseriesRequest = new TimeseriesRequest()
        {
            Sequence = new Sequence()
            {
                From = from,
                To = to
            },
            //Timestamps = new Timestamps()
            //{
            //    From = new Google.Protobuf.WellKnownTypes.Timestamp()
            //    {
            //        Seconds = from,
            //        Nanos = 0
            //    },
            //    To = new Google.Protobuf.WellKnownTypes.Timestamp()
            //    {
            //        Seconds = to,
            //        Nanos = 0
            //    },
            //},
            //IncludeNewestDataBeforeFilter = true,
            //LastData = LastData.ProcessTime,
        };

        //for (int i = 1; i < 1000; i++)
        //    timeseriesRequest.TimeseriesIds.Add(i);

        //timeseriesRequest.TimeseriesIds.Add(1);

        request.TimeseriesDataRequests.Add(timeseriesRequest);

        var timer = System.Diagnostics.Stopwatch.StartNew();
        var res = client.GetTimeseries(request);

        await foreach (var response in res.ResponseStream.ReadAllAsync())
        {
            //Console.WriteLine($"Response: {response.TimeseriesData.Count}");
            resultsCount += response.TimeseriesData.Count;
            responsesCount++;
        }

        timer.Stop();
        Console.WriteLine($"{DateTime.UtcNow.ToString()} (from: {from}, to: {to}) Elapsed time: {timer.ElapsedMilliseconds} ms. Data {resultsCount}");
        // Console.WriteLine($"Data per second: {resultsCount / (timer.ElapsedMilliseconds / 1000)}");

        from = to;
        to = from + batch_size;
    }
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}


