# Hmac Authentication

## Navigation

* [Home](/README.md)
	* [Index](/docs/Index.md)
		* [Authentication](/src/Authentication/README.md)

### Children

* [Hmac Help](/src/Authentication/Hmac/API%20Integration%20Help/HMAC%20Authentication%20Help.md)
* [Hmac Integration Tests](/src/AuthenticationIntegrationsTests/Hmac/README.md)
* [Hmac Unit Tests](/src/AuthenticationUnitTests/Hmac/README.md)

## Info

Check [Hmac Authentication help](/src/Authentication/Hmac/API%20Integration%20Help/HMAC%20Authentication%20Help.md) for integration instructions

The endpoints document contains all the endpoints you'll need to integrate.
This is an example and a breakdown of what is stored there (note never include the curly braces around the param values when submitting)

Name, Method, `URI?paramName={ParamValue}`

Get Basic Content To Post, GET `https://{host}/api/{controller}/GetBasicContentToPost`

An Example DTO

```json
{
	"Name": "BasicContent",
	"Id": "2f097177-a9aa-4851-a4b3-403c7e05bff6"
}
```

A breakdown of the types in the DTO

```json
{
	"Name": "string",
	"Id": "string"
}
```

There is an included a C# class that will allow you to simply add the headers to a webclient via an extention method, and parse the errors that are returned.

If you aren't using a .NET Standard language, it can be used as a reference for reproduction.

If you are able to use it you can call it like this (this assumes you have a SerializeJson extention for objects, and a GetBytes extention for strings, as well as the BasicContent DTO)

```csharp
Uri newUri = new Uri(url);
HeaderGenerator.SetAppIdAndSecretKey(appId, secretKey);
BasicContent content = new BasicContent {Name = "test", Id = appId};
string payload = content.SerializeJson();

Tuple<HttpStatusCode, string> codeAndDescription;
try
{
	using (WebClient webClient = new WebClient())
	{
		webClient.AddHmacHeaders(newUri, HttpMethod.Post, payload, HeaderGenerator);
		await webClient.UploadDataTaskAsync(newUri, HttpMethod.Post.Method, payload.GetBytes());
		codeAndDescription = webClient.GetStatusCodeAndDescription();
	}
}
catch (WebException ex)
{
	HttpWebResponse response = (HttpWebResponse)ex.Response;
	codeAndDescription = new Tuple<HttpStatusCode, string>(response.StatusCode, response.StatusDescription);
}
```

The [Hmac Authentication help](/src/Authentication/Hmac/API%20Integration%20Help/HMAC%20Authentication%20Help.md) breaks down how HMAC authentication works and how you can implement it. It shows the different values that will be needed, and how to put them together.