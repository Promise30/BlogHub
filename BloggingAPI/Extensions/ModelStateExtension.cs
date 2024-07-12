using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BloggingAPI.Extensions
{
    public static class ModelStateExtention
    {
        public static List<string> GetErrorMessages(this ModelStateDictionary dictionary)
        {
            return dictionary.SelectMany(m => m.Value.Errors)
                                .Select(m => m.ErrorMessage)
                                .ToList();
        }
    }
}
