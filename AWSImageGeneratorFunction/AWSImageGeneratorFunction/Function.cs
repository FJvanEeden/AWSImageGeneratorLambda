using System;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace AWSImageGeneratorFunction
{
  public class Function
  {
    public Function()
    {

    }

    public async Task<string> FunctionHandler(SQSEvent evnt, ILambdaContext context)
    {
      foreach (var message in evnt.Records)
      {
        await ProcessMessageAsync(message, context);
      }

      Console.WriteLine("Processing complete.");
      
      return $"Processed {evnt.Records.Count} records.";
    }

    private async Task ProcessMessageAsync(SQSEvent.SQSMessage message, ILambdaContext context)
    {
      context.Logger.LogLine($"Processed message {message.Body}");

      //
      await PublishToSNS();
      await Task.CompletedTask;
    }

    private async Task PublishToSNS()
    {
      string message = "Test at {DateTime.UtcNow.ToLongDateString()}";
      var client = new AmazonSimpleNotificationServiceClient(region: Amazon.RegionEndpoint.USWest2);

      var testRequest = new PublishRequest
      {
        Message = message,
        TopicArn = "arn:aws:sns:us-west-2:494051244674:example-sns-receiver",
      };

      var response = await client.PublishAsync(testRequest);
    }
  }
}
