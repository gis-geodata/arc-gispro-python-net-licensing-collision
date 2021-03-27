using System.Collections.Generic;

namespace Collision.Python
{
    public class Error
    {
        public string Description
        {
            get;
            internal set;
        }

        public Error(string description)
        {

            Description = description;
        }

        public override string ToString()
        {
            return Description;
        }
    }

    public class ResponseWrapper<Tr>
    {
        public Tr Response
        {
            get;
            internal set;
        }

        public IEnumerable<Error> Errors
        {
            get;

            internal set;
        }

        public ResponseWrapper(Tr response, IEnumerable<Error> errors)
        {
            Response = response;
            Errors = errors;
        }

        public ResponseWrapper(IEnumerable<Error> errors)
        {
            Errors = errors;
        }

        public ResponseWrapper(Tr response)
        {
            Response = response;
            Errors = new Error[0];
        }
    }
}
