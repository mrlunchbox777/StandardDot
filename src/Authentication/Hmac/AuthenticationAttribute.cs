// using System;
// using System.Collections.Generic;
// using System.IO;
// using System.Linq;
// using System.Net;
// using System.Security.Cryptography;
// using System.Security.Principal;
// using System.Text;
// using System.Web;
// using System.Web.Mvc;
// using System.Web.Mvc.Filters;
// using PreS.Areas.Api.Models;
// using PreS.Enums;
// using PreS.Extensions;
// using PreS.Models;
// using StandardDot.CoreExtensions;

// namespace StandardDot.Hmac
// {
//     /// <summary>
//     /// Uses the HMAC Authentication Service to handle requests
//     /// </summary>
//     [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
//     public class HmacAuthenticationAttribute : AuthorizeAttribute
//     {
//         // 5 mins
//         private const ulong _requestMaxAgeInSeconds = 300;

//         private const string _authenticationScheme = "sds";

//         private const string _authorizationKey = "Authorization";

//         // should we check the time stamp to ensure it's close to what we have
//         // this can cause unnecessary failures if set to true, but can allow replay requests if set to false
//         // the tolerance is the requestMaxAgeInSeconds
//         private const bool _shouldEnsureTimeStampIsWithinTolerance = true;

//         private string[] GetAutherizationHeaderValues(string rawAuthzHeader)
//         {
//             string[] credArray = rawAuthzHeader.Split(':');

//             if (credArray.Length == 4)
//             {
//                 return credArray;
//             }
//             else
//             {
//                 return null;
//             }
//         }

//         private Tuple<bool, string> IsValidRequest(HttpContextBase httpContext, string appId, string incomingBase64Signature,
//             string nonce, string requestTimeStamp)
//         {
//             if (httpContext?.Request.Url == null)
//             {
//                 LoggingService.LogMessageStatic("Api Auth Request Not Valid",
//                     "No valid request Url", LogLevel.Debug);
//                 return new Tuple<bool, string>(false, "No Valid Request");
//             }

//             string requestContentBase64String = "";
//             string requestUri = HttpUtility.UrlEncode(httpContext.Request.Url.AbsoluteUri.ToLower());
//             string requestHttpMethod = httpContext.Request.HttpMethod.ToUpper();

//             string sharedKey = ApiKeyService.Apps[appId];
//             if (string.IsNullOrWhiteSpace(sharedKey))
//             {
//                 LoggingService.LogMessageStatic("Api Auth Request Not Valid",
//                     "Unable to find appId", LogLevel.Debug);
//                 return new Tuple<bool, string>(false, "Unable to find appId. Is your app active?");
//             }

//             if (IsReplayRequest(nonce, requestTimeStamp))
//             {
//                 LoggingService.LogMessageStatic("Api Auth Request Not Valid",
//                     "Looked like a replay request.\r\nNonce - " + nonce + "\r\nRequest Timestamp - " + requestTimeStamp
//                     + "\r\nOur Timestamp" + CurrentUnixTs.TotalSeconds, LogLevel.Debug);
//                 return new Tuple<bool, string>(false,
//                     "This looks like a replay request. Are you creating a new nonce every time?");
//             }

//             Tuple<byte[], string> hashAndContent = ComputeHash(httpContext);

//             if (hashAndContent.Item1 != null)
//             {
//                 requestContentBase64String = Convert.ToBase64String(hashAndContent.Item1);
//             }

//             string data =
//                 $"{appId}{requestHttpMethod}{requestUri}{requestTimeStamp}{nonce}{requestContentBase64String}";

//             byte[] secretKeyBytes = Convert.FromBase64String(sharedKey);

//             byte[] signature = data.GetBytes();

//             using (HMACSHA256 hmac = new HMACSHA256(secretKeyBytes))
//             {
//                 byte[] signatureBytes = hmac.ComputeHash(signature);

//                 string signatureString = Convert.ToBase64String(signatureBytes);
//                 bool matched = incomingBase64Signature.Equals(signatureString,
//                     StringComparison.Ordinal);
//                 if (!matched)
//                 {
//                     LoggingService.LogMessageStatic("Api Auth Request Not Valid",
//                         "Signatures didn't match.\r\nRequest Signature - " + incomingBase64Signature
//                         + ".\r\nInternal Signature - " + signatureString + ".\r\nInternal signature data - " + data
//                         + "\r\nRaw Content - " + hashAndContent.Item2,
//                         LogLevel.Debug);
//                 }
//                 return new Tuple<bool, string>(matched, matched ? "" : "Signatures didn't match.");
//             }

//         }

//         private bool IsReplayRequest(string nonce, string requestTimeStamp)
//         {
//             if (System.Runtime.Caching.MemoryCache.Default.Contains(nonce))
//             {
//                 return true;
//             }

//             if (!_shouldEnsureTimeStampIsWithinTolerance)
//             {
//                 return true;
//             }

//             TimeSpan currentTs = CurrentUnixTs;

//             ulong serverTotalSeconds = Convert.ToUInt64(currentTs.TotalSeconds);
//             ulong requestTotalSeconds = Convert.ToUInt64(requestTimeStamp);

//             if ((serverTotalSeconds - requestTotalSeconds) > _requestMaxAgeInSeconds)
//             {
//                 return true;
//             }

//             System.Runtime.Caching.MemoryCache.Default.Add(nonce, requestTimeStamp,
//                 DateTimeOffset.UtcNow.AddSeconds(_requestMaxAgeInSeconds));

//             return false;
//         }

//         private static TimeSpan CurrentUnixTs
//         {
//             get
//             {
//                 DateTime epochStart = new DateTime(1970, 01, 01, 0, 0, 0, 0, DateTimeKind.Utc);
//                 TimeSpan currentTs = DateTime.UtcNow - epochStart;
//                 return currentTs;
//             }
//         }

//         /// <summary>
//         /// Returns (hash, raw content)
//         /// </summary>
//         /// <param name="httpContext"></param>
//         /// <returns></returns>
//         private static Tuple<byte[], string> ComputeHash(HttpContextBase httpContext)
//         {
//             using (MD5 md5 = MD5.Create())
//             {
//                 Stream contentStream = httpContext.Request.GetBufferedInputStream();
//                 byte[] content = contentStream.GetByteArrayFromStream();
//                 // add this in so that the content can still be found
//                 httpContext.Items.Add(ApiHttpContext.ApiHttpContextContentDataTag, content);

//                 string rawContent = "";
//                 if (content.Length == 0)
//                 {
//                     return new Tuple<byte[], string>(null, rawContent);
//                 }

//                 byte[] hash = md5.ComputeHash(content);
//                 rawContent = content.GetString();
//                 return new Tuple<byte[], string>(hash, rawContent);
//             }
//         }

//         public Tuple<AuthenticationHttpCodeResult, GenericPrincipal> DoAuthorization(HttpContextBase context,
//             List<AllowAnonymousAttribute> actionAttributes, List<AllowAnonymousAttribute> controllerAttributes)
//         {
//             // If we are allowing anonymous we should just let them through
//             if (actionAttributes.Any()
//                 || controllerAttributes.Any())
//             {
//                 return null;
//             }

//             HttpRequestBase req = context.Request;
//             string auth = req.Headers?[_authorizationKey];
//             string[] authParts = auth?.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
//             if (auth != null && authParts.Length > 0
//                 && _authenticationScheme.Equals(authParts[0].Trim(), StringComparison.OrdinalIgnoreCase))
//             {
//                 string rawAuthzHeader = authParts[1];

//                 string[] autherizationHeaderArray = GetAutherizationHeaderValues(rawAuthzHeader);

//                 if (autherizationHeaderArray != null && autherizationHeaderArray.Length == 4)
//                 {
//                     string appId = autherizationHeaderArray[0];
//                     string incomingBase64Signature = autherizationHeaderArray[1];
//                     string nonce = autherizationHeaderArray[2];
//                     string requestTimeStamp = autherizationHeaderArray[3];

//                     Tuple<bool, string> isValid =
//                         IsValidRequest(context, appId, incomingBase64Signature, nonce, requestTimeStamp);

//                     if (isValid.Item1)
//                     {
//                         GenericPrincipal currentPrincipal = new GenericPrincipal(new GenericIdentity(appId), null);
//                         return new Tuple<AuthenticationHttpCodeResult, GenericPrincipal>(null, currentPrincipal);
//                     }
//                     else
//                     {
//                         LoggingService.LogMessageStatic("Bad Api Auth",
//                             "\r\nappId - " + appId + "\r\nincomingBase64Signature - " + incomingBase64Signature
//                             + "\r\nnonce - " + nonce + "\r\nrequestTimeStamp - " + requestTimeStamp + ". Message - "
//                             + isValid.Item2, LogLevel.Info);
//                         return new Tuple<AuthenticationHttpCodeResult, GenericPrincipal>(
//                             new AuthenticationHttpCodeResult(HttpStatusCode.Unauthorized, isValid.Item2), null);
//                     }
//                 }
//                 else
//                 {
//                     LoggingService.LogMessageStatic("Bad Api Auth",
//                         "Incorrect items found in the Authorization Header. Parts should be 4 found "
//                         + (autherizationHeaderArray?.Length.ToString() ?? "0"), LogLevel.Info);
//                     return new Tuple<AuthenticationHttpCodeResult, GenericPrincipal>(
//                         new AuthenticationHttpCodeResult(HttpStatusCode.Unauthorized,
//                             "Incorrect items found in the Authorization Header. Parts should be 4 found "
//                             + (autherizationHeaderArray?.Length.ToString() ?? "0")),
//                         null);
//                 }
//             }
//             else
//             {
//                 LoggingService.LogMessageStatic("Bad Api Auth",
//                     (string.IsNullOrWhiteSpace(auth) ? "No auth header." : "")
//                     + (authParts?.Length > 0
//                         ? "Not enough auth parts. Did you include the namespace and parameter string?"
//                         : "")
//                     + (_authenticationScheme.Equals(authParts?[0]?.Trim(), StringComparison.OrdinalIgnoreCase)
//                         ? "Improper namespace. Did you include it?"
//                         : ""), LogLevel.Info);
//                 return new Tuple<AuthenticationHttpCodeResult, GenericPrincipal>(new AuthenticationHttpCodeResult(
//                     HttpStatusCode.Unauthorized,
//                     (string.IsNullOrWhiteSpace(auth) ? "No auth header." : "")
//                     + (authParts?.Length > 0
//                         ? "Not enough auth parts. Did you include the namespace and parameter string?"
//                         : "")
//                     + (_authenticationScheme.Equals(authParts?[0]?.Trim(), StringComparison.OrdinalIgnoreCase)
//                         ? "Improper namespace. Did you include it?"
//                         : "")), null);
//             }
//         }

//         #region Implementation of IAuthenticationFilter
//         public void OnAuthentication(AuthenticationContext filterContext)
//         {
//             List<AllowAnonymousAttribute> actionAttributes = filterContext.ActionDescriptor
//                 .GetCustomAttributes(typeof(AllowAnonymousAttribute), true).Cast<AllowAnonymousAttribute>().ToList();
//             List<AllowAnonymousAttribute> controllerAttributes = filterContext.ActionDescriptor.ControllerDescriptor
//                 .GetCustomAttributes(typeof(AllowAnonymousAttribute), true).Cast<AllowAnonymousAttribute>().ToList();
//             Tuple<AuthenticationHttpCodeResult, GenericPrincipal> auth =
//                 DoAuthorization(filterContext.HttpContext, actionAttributes, controllerAttributes);
//             if (auth == null)
//             {
//                 return;
//             }

//             if (auth.Item2 != null)
//             {
//                 filterContext.Principal = auth.Item2;
//             }

//             if (auth.Item1 != null)
//             {
//                 filterContext.Result = auth.Item1;
//             }
//         }

//         public void OnAuthenticationChallenge(AuthenticationChallengeContext filterContext)
//         {
//             filterContext.Result = new ResultWithChallenge(filterContext.Result);
//         }
//         #endregion

//         #region Implementation of IAuthorizationFilter
//         public override void OnAuthorization(AuthorizationContext filterContext)
//         {
//             List<AllowAnonymousAttribute> actionAttributes = filterContext.ActionDescriptor
//                 .GetCustomAttributes(typeof(AllowAnonymousAttribute), true).Cast<AllowAnonymousAttribute>().ToList();
//             List<AllowAnonymousAttribute> controllerAttributes = filterContext.ActionDescriptor.ControllerDescriptor
//                 .GetCustomAttributes(typeof(AllowAnonymousAttribute), true).Cast<AllowAnonymousAttribute>().ToList();
//             Tuple<AuthenticationHttpCodeResult, GenericPrincipal> auth =
//                 DoAuthorization(filterContext.HttpContext, actionAttributes, controllerAttributes);
//             if (auth == null)
//             {
//                 return;
//             }

//             if (auth.Item2 != null)
//             {
//                 filterContext.HttpContext.User = auth.Item2;
//             }

//             if (auth.Item1 != null)
//             {
//                 filterContext.Result = auth.Item1;
//             }
//         }
//         #endregion
//     }
// }