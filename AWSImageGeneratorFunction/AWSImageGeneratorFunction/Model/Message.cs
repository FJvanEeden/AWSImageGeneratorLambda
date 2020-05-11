using System;
using System.Collections.Generic;
using System.Text;

namespace AWSImageGeneratorFunction.Model
{
  public class Message<T>
  {
    public string EventId { get; set; }
    public string EventName { get; set; }
    public T MessagePayload { get; set; }
    public bool HasAttachment { get; set; }
    public bool HasMultipleAttachment { get; set; }
    public string EventSource { get; set; }
  }
}
