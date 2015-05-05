using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharkLibrary.Api
{
    public class Result<T>
    {
        public ResultHeader header { get; set; }
        public T result { get; set; }
        public List<Error> errors { get; set; }
    }
}
