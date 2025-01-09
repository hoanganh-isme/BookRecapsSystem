using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Responses
{
    public class RecapResponse<T, U>
    {
        public RecapResponse()
        {
        }

        public RecapResponse(T data, U data2)
        {
            Succeeded = true;
            Message = string.Empty;
            Errors = null;
            Data = data;
            Data2 = data2;
        }

        public T Data { get; set; }  // Recap data
        public U Data2 { get; set; } // PlayListItems and isLiked data
        public bool Succeeded { get; set; }
        public string[] Errors { get; set; }
        public string Message { get; set; }
    }

}
