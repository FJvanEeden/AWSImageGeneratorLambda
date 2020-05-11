using System;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;

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
      context.Logger.LogLine($"Processed message {message.MessageId}");

      // TODO: Do interesting work based on the new message
      await Task.CompletedTask;
    }
  }
}
