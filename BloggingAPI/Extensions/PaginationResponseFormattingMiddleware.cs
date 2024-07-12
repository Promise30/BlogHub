namespace BloggingAPI.Extensions
{
    public class PaginationResponseFormattingMiddleware
    {
        private readonly RequestDelegate _next;

        public PaginationResponseFormattingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var originalBodyStream = context.Response.Body;
            using (var responseBody = new MemoryStream())
            {
                context.Response.Body = responseBody;

                await _next(context);

                if (context.Request.Query.ContainsKey("pageNumber") &&
                    context.Request.Query.ContainsKey("pageSize"))
                {
                    // Add pagination headers here
                    var pageNumber = Convert.ToInt32(context.Request.Query["pageNumber"]);
                    var pageSize = Convert.ToInt32(context.Request.Query["pageSize"]);

                    context.Response.Headers.Add("X-Pagination", "True");
                    context.Response.Headers.Add("Access-Control-Expose-Headers", "X-Pagination");
                    context.Response.Headers.Add("X-Page-Number", pageNumber.ToString());
                    context.Response.Headers.Add("X-Page-Size", pageSize.ToString());
                    // Add any other pagination headers you need
                }

                await CopyResponseBodyAsync(originalBodyStream, responseBody);
            }
        }

        private async Task CopyResponseBodyAsync(Stream originalBodyStream, MemoryStream responseBody)
        {
            responseBody.Position = 0;
            await responseBody.CopyToAsync(originalBodyStream);
        }
    }
}
