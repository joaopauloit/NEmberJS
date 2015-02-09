using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.ModelBinding;


namespace NEmberJS.Validations
{
    public class ValidateModelActionFilerAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            
            if (actionContext.ModelState.IsValid == false)
            {
                actionContext.Response = actionContext.Request.CreateResponse((HttpStatusCode)422,
                    ParserModelState(actionContext.ModelState));
            }
        }

        public Errors ParserModelState(ModelStateDictionary modelState ){

            var error = new Dictionary<string,object>();
            modelState.Keys.ToList().ForEach(key=> error.Add(key.Split('.')[1], modelState[key].Errors.Select(modelError=>modelError.ErrorMessage).ToArray()));
            return new Errors(error);
        }
    }

}

