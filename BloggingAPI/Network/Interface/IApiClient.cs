namespace BloggingAPI.Network.Interface
{
    public interface IApiClient
    {
        public Task<string> PostDataAsync<T>(string endPoint, Dictionary<string, string> headers, T dto);
        public Task<string> GetDataAsync<T>(string endPoint, Dictionary<string, string> headers);

    }
}
