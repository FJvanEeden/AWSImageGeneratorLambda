using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
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
      var x = 0;
      var y = 0;
      int i;
      byte[] pixel = new byte[4];
      byte[] newData = new byte[width * height * 4];

      foreach (var data in rawImageData)
      {
        i = x % 4;
        if (x > 0 && i == 0)
        {
          byte r = pixel[0];
          byte g = pixel[1];
          byte b = pixel[2];
          byte a = pixel[3];
          byte[] newPixel = new byte[] { b, g, r, a };
          Array.Copy(newPixel, 0, newData, y, 4);
          y += 4;
        }
        pixel[i] = data;
        x++;
      }

      i = x % 4;
      if (x > 0 && i == 0)
      {
        byte r = pixel[0];
        byte g = pixel[1];
        byte b = pixel[2];
        byte a = pixel[3];
        byte[] newPixel = new byte[] { b, g, r, a };
        Array.Copy(newPixel, 0, newData, y, 4);
      }

      using (Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb))
      {
        BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, bmp.PixelFormat);
        IntPtr pNative = bmpData.Scan0;
        Marshal.Copy(newData, 0, pNative, newData.Length - 1);

        bmp.UnlockBits(bmpData);
        return (Image)bmp.Clone();
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
