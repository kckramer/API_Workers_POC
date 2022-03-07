using System;

namespace WebAPI.Models
{
    public class InputMessage
    {
        private Guid correlationId;
        private string data;

        public InputMessage(Guid correlationId, string data)
        {
            this.correlationId = correlationId;
            this.data = data;
        }
    }
}