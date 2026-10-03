using Crm.Domain.Consts;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json.Serialization;
using System.Text;

namespace Crm.Api.Models
{
    /// <summary>
    /// Base class for any responce to be return to client.
    /// </summary>
    public class ApiResponse
    {
        public ApiResponse(EnumEntityType entityCode, EnumEntityEvents eventCode)
        {
            EntityCode = entityCode;
            EventCode = eventCode;            
            EventMessageId = eventCode.ToString();
        }
        public EnumEntityType EntityCode { get; }
        public EnumEntityEvents EventCode { get; }
        public String EventMessageId { get; protected set; }
    }

    /// <summary>
    /// Any ApiBadRequestResponse to be return to client
    /// </summary>
    public class ApiBadRequestResponse : ApiResponse
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? ErrorDetail { get; }

        public ApiBadRequestResponse(EnumEntityType entityCode, EnumEntityEvents eventCode, ModelStateDictionary modelState)
            : base(entityCode, eventCode)
        {
            StringBuilder errorDetail = new StringBuilder();
            foreach (var errorKeyValue in modelState)
            {
                errorDetail.Append(errorKeyValue.Key).Append(" : ");
                foreach (var error in errorKeyValue.Value.Errors)
                {
                    if (!String.IsNullOrEmpty(error.ErrorMessage))
                    {
                        errorDetail.Append(error.ErrorMessage);
                    }
                    else if (error.Exception != null)
                    {
                        errorDetail.Append(error.Exception.Message); 
                    }
                    errorDetail.Append("! ");
                }
            }

            ErrorDetail = errorDetail.ToString();
        }

        public ApiBadRequestResponse(EnumEntityType entityCode, EnumEntityEvents eventCode, string? errorDetail)
          : base(entityCode, eventCode)
        {
            ErrorDetail = errorDetail;
        }

        public ApiBadRequestResponse(EnumEntityType entityCode, EnumEntityEvents eventCode, object? errorDetail)
          : base(entityCode, eventCode)
        {
            this.ErrorDetail = errorDetail;
        }

        public ApiBadRequestResponse(EnumEntityType entityCode, EnumEntityEvents eventCode, Exception error)
           : base(entityCode, eventCode)
        {
            ErrorDetail = error.Message;
        }
    }

    public class ApiOkResponse : ApiResponse
    {
        public object? Data { get; }

        public ApiOkResponse(EnumEntityType entityCode, EnumEntityEvents eventCode, object? result)
            : base(entityCode, eventCode)
        {
            Data = result;
        }
    }
}
