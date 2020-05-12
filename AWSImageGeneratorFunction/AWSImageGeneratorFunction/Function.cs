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

      Console.WriteLine("Processing complete #99.");
      
      return $"Processed {evnt.Records.Count} records.";
    }

    private async Task ProcessMessageAsync(SQSEvent.SQSMessage message, ILambdaContext context)
    {
      context.Logger.LogLine($"Processed message {message.Body}");

      message.Body += " YOUR MESSAGE WENT THROUGH A LAMBDA...";

      await PublishToSNS(message.Body);
      await Task.CompletedTask;
    }

    private async Task PublishToSNS(string bodyFromSQS)
    {
      string message = " Test at {DateTime.UtcNow.ToLongDateString()}: " + bodyFromSQS;
      var client = new AmazonSimpleNotificationServiceClient(region: Amazon.RegionEndpoint.USWest2);

      var snsRequest = new PublishRequest
      {
        Message = message,
        TopicArn = "arn:aws:sns:us-west-2:494051244674:example-sns-receiver",
      };

      var response = await client.PublishAsync(snsRequest);
    }
  }
}
