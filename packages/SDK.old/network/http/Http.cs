using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Theta.SDK.Base.Services.Auth;
using Theta.SDK.Bazaar.Services.Bazaar;
using Theta.SDK.Utils.Logging;

namespace Theta.SDK.Utils
{
    /// <summary>
    /// Provides network, async, and serialization to call Theta services from Unity.
    /// </summary>
    public class HttpServiceCaller : ServiceCaller
    {
        /// <summary>
        /// The preferred serial format for this caller.
        /// </summary>
        public readonly Serial.Format SerialFormat = Serial.Format.JSON;

        /// <summary>
        /// TODO
        /// </summary>
        public readonly Encoding StringEncoding = Encoding.UTF8;

        /// <summary>
        /// TODO
        /// </summary>
        /// <note> Explicityly disallow setting the handler externally, EVEN ON CONSTRUCT.</note>
        public HttpClientHandler Handler { get; private set; } = new HttpClientHandler
        {
            AllowAutoRedirect = true,
            MaxAutomaticRedirections = 3,
        };

        /// <summary>
        /// The internal HttpClient instance to call the Theta platform.
        /// </summary>
        private readonly HttpClient m_httpClient;

        /// <summary>
        /// The internal Authenticator used to authenticate against the Theta platform.
        /// </summary>
        private readonly Authenticator m_authenticator;

        /// <summary>
        /// CTOR
        /// </summary>
        public HttpServiceCaller(Authenticator authenticator) : this(authenticator, null)
        { }

        /// <summary>
        /// CTOR
        /// </summary>
        public HttpServiceCaller(Authenticator authenticator, HttpClient httpClient)
        {
            m_authenticator = authenticator;
            m_httpClient = httpClient ?? new HttpClient(Handler);
        }

        /// <summary>
        /// Clean up after yourself, pls
        /// </summary>
        public override void Dispose()
        {
            m_httpClient.Dispose();
        }

        //---
        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="path">The path to the endpoint.</param>
        /// <returns></returns>
        public ServiceEndpoint GetEndpoint(string path)
        {
            return Resolver.GetEndpoint(path);
        }

        //---
        /// <summary>
        /// Craft an instance of HttpContent from an arbitrary request.
        /// </summary>
        public HttpContent CraftHttpContent<TRequest>(TRequest request, Encoding encoding, Serial.Format format)
        {
            return CraftHttpContent(Serial.Pack(request, format), encoding, format);
        }

        /// <summary>
        /// Craft an instance of HttpContent from a content body string.
        /// </summary>
        public HttpContent CraftHttpContent(string requestData, Encoding encoding, Serial.Format serialFormat)
        {
            return new StringContent(requestData, encoding, Serial.ContentType[serialFormat]);
        }

        /// <summary>
        /// Craft an instance of HttpRequest from an arbitrary request object.
        /// </summary>
        public HttpRequestMessage CraftHttpRequest(HttpMethod method, ServiceEndpoint HttpServiceEndpoint, bool authenticate, object requestBody)
        {
            HttpRequestMessage request = new HttpRequestMessage(method, HttpServiceEndpoint);

            request.Content = CraftHttpContent(requestBody, StringEncoding, SerialFormat);
            request.Headers.Add("Accept", request.Content.Headers.ContentType.ToString());
            if (authenticate)
            {
                request.Headers.Add(Authenticator.TP_TOKEN_HEADER, m_authenticator.Token.Body);
            }

            return request;
        }

        //---
        /// <summary>
        /// Unpack an HttpResponseMessage to a particular TResponse.
        /// </summary>
        public static (int, string, TResponse) UnpackResponse<TResponse>(HttpResponseMessage response, Serial.Format serialFormat)
        {

            int code = (int)response.StatusCode;
            string reason = response.ReasonPhrase;

            if (!response.IsSuccessStatusCode)
            {
                switch (Math.Floor((double)code / 100) * 100)
                {
                    case 500: // <- Server Errors; "Not (necessarily) the client's fault."
                        TPLogger.Warning("Bad news from server: {0}", response);
                        break;
                    case 400: // <- Client Errors; "Not (necessarily) the servver's fault."
                        TPLogger.Info("You done goofed; 4XX-range response: {0}", response);
                        break;
                    case 300: // <- Maybe useful messages from the server (redirects, mostly).
                        TPLogger.Debug("HTTP Status {0}; {1}", code, response);
                        break;
                }
            }

            return (code, reason, UnpackResponseContent<TResponse>(response.Content, serialFormat));
        }

        /// <summary>
        /// Unpack HttpContent for an Theta service call.
        /// </summary>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="content"></param>
        /// <returns></returns>
        public static TResponse UnpackResponseContent<TResponse>(HttpContent content, Serial.Format serialFormat)
        {
            TResponse response = default;
            try
            {
                response = Serial.Unpack<TResponse>(content.ReadAsStringAsync().Result, serialFormat);
            }
            catch (Exception exception)
            {
                //TODO: Send (useful) exceptions to callbacks.
                TPLogger.Warning("Couldn't read or unpack http content: {0}", exception);
            }

            return response;
        }

        //---
        /// <summary>
        /// Authenticate and update token, resolver, etc.
        /// </summary>
        public override async Task<AuthToken> Authenticate(LoginRequest request)
        {
            ServiceCallResult<LoginRequest, LoginResponse> result = new ServiceCallResult<LoginRequest, LoginResponse>
            {
                Request = request,
            };

            ServiceEndpoint endpoint = m_authenticator.Resolver.GetEndpoint("auth/login");
            using (HttpRequestMessage httpRequest = CraftHttpRequest(HttpMethod.Post, endpoint, false, request))
            {
                TPLogger.Debug("Attempting to login: {0}", request);

                HttpResponseMessage httpResponse = await m_httpClient.SendAsync(httpRequest);
                if (!httpResponse.IsSuccessStatusCode)
                {
                    throw new AuthException($"Login request failed: {httpResponse}");
                }

                (result.Code, result.Reason, result.Response) = UnpackResponse<LoginResponse>(httpResponse, SerialFormat);
            }

            m_authenticator.Token = new AuthToken(result.Response.authToken);
            m_authenticator.LoginRequest = request;

            Resolver = new ServiceResolver(
                result.Response.serviceHost,
                result.Response.servicePort,
                m_authenticator.Resolver.Secure
            );

            return m_authenticator.Token;
        }

        //---
        /// <summary>
        /// Make an Theta service call with both a request and a response.
        /// </summary>
        public override async Task<ServiceCallResult<TRequest, TResponse>> Call<TRequest, TResponse>(string service, string call, TRequest requestBody)
        {
            ServiceCallResult<TRequest, TResponse> result = new ServiceCallResult<TRequest, TResponse>
            {
                Request = requestBody,
            };

            ServiceEndpoint endpoint = GetEndpoint($"{service}/{call}");
            using (HttpRequestMessage httpRequest = CraftHttpRequest(HttpMethod.Post, endpoint, true, requestBody))
            {
                TPLogger.Debug("Sending service request: {0}", httpRequest);
                HttpResponseMessage httpResponse = await m_httpClient.SendAsync(httpRequest);

                (result.Code, result.Reason, result.Response) = UnpackResponse<TResponse>(httpResponse, SerialFormat);
            }

            return result;
        }
    }
}
