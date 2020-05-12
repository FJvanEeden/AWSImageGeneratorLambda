using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Amazon.Lambda;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using ThirdParty.BouncyCastle.Asn1;

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

      await PublishToSNS(message.Body);
      await Task.CompletedTask;
    }

    private async Task PublishToSNS(string bodyFromSQS)
    {
      var client = new AmazonSimpleNotificationServiceClient(region: Amazon.RegionEndpoint.USWest2);

      var snsRequest = new PublishRequest
      {
        Message = bodyFromSQS,
        TopicArn = "arn:aws:sns:us-west-2:494051244674:example-sns-receiver",
      };

      var response = await client.PublishAsync(snsRequest);
    }

    public static Image GenerateImageFromByteArr(IEnumerable<byte> rawImageData, int width, int height)
    {
      byte[] newData = rawImageData.ToArray();

      using (Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb))
      {
        BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, bmp.PixelFormat);
        IntPtr pNative = bmpData.Scan0;
        Marshal.Copy(newData, 0, pNative, newData.Length - 1);

        bmp.UnlockBits(bmpData);
        return (Image)bmp.Clone();
      }
    }

    // Test Image
    private static IEnumerable<byte> RedImageWithBlueCircle(int width, int height)
    {
      int index = 0;
      // { b, g, r, a };
      var red = new byte[] { 0, 0, 255, 255 };
      var blue = new byte[] { 255, 0, 0, 255 };
      var green = new byte[] { 0, 255, 0, 255 };

      int blueXOffset = 300;
      int blueYOffset = 200;
      int blueInnerRadius = 300;
      int blueOuterRadius = 500;

      int greenXOffset = 800;
      int greenYOffset = 700;
      int greenInnerRadius = 400;
      int greenOuterRadius = 500;

      for (int y = 0; y < height; y++)
      {
        for (int x = 0; x < width; x++)
        {
          if (((x - blueXOffset) * (x - blueXOffset) + (y - blueYOffset) * (y - blueYOffset) > blueInnerRadius * blueInnerRadius) && ((x - blueXOffset) * (x - blueXOffset) + (y - blueYOffset) * (y - blueYOffset) < blueOuterRadius * blueOuterRadius))
          {
            for (int p = 0; p < 4; p++)
            {
              yield return blue[index % 4];
              index++;
            }
          }
          else if (((x - greenXOffset) * (x - greenXOffset) + (y - greenYOffset) * (y - greenYOffset) > greenInnerRadius * greenInnerRadius) && ((x - greenXOffset) * (x - greenXOffset) + (y - greenYOffset) * (y - greenYOffset) < greenOuterRadius * greenOuterRadius))
          {
            for (int p = 0; p < 4; p++)
            {
              yield return green[index % 4];
              index++;
            }
          }
          else
          {
            for (int p = 0; p < 4; p++)
            {
              yield return red[index % 4];
              index++;
            }
          }
        }
      }
    }

    public async Task<byte[]> ReadStreamToByte(Stream inputStream)
    {
      using (MemoryStream ms = new MemoryStream())
      {
        await inputStream.CopyToAsync(ms);
        return ms.ToArray();
      }
    }
    public static Stream ImageToStream(Image image)
    {
      var stream = new MemoryStream();
      image.Save(stream, image.RawFormat);
      stream.Position = 0;
      return stream;
    }
  }
}
