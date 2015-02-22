using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.ModelBinding;


namespace NEmberJS.Validations
{
    public class ValidateModelAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {

            if (actionContext.ModelState.IsValid == false)
            {
                actionContext.Response = actionContext.Request.CreateResponse((HttpStatusCode)422,
                    ParserModelState(actionContext.ModelState));
            }

        }

        public Errors ParserModelState(ModelStateDictionary modelState)
        {
            var error = new Errors();

            foreach (var item in modelState)
            {
                var keySplit = item.Key.Split('.');
                var keyError = keySplit[keySplit.Length - 1];
                foreach (var erro in item.Value.Errors)
                {
                    string message = erro.ErrorMessage;
                    //error case, throw overflow application.
                    if (string.IsNullOrEmpty(erro.ErrorMessage) && erro.Exception != null)
                        message = erro.Exception.Message;
                    error.CreateErrorItem(keyError, message);
                }
                item.Value.Errors.ToList().ForEach(x => error.CreateErrorItem(keyError, x.ErrorMessage));
            }
            return error;
        }
    }

}

